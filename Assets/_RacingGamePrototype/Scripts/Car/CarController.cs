using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _RacingGamePrototype.Scripts.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarController : MonoBehaviour
    {
        [Header("Car setup")]
        [SerializeField] private Transform centerOfMass;
        [SerializeField] private WheelCollider[] frontWheels;
        [SerializeField] private WheelCollider[] rearWheels;

        [Header("Car settings")]
        [SerializeField] private float motorForce = 1500f;
        [SerializeField] private float brakeForce = 3000f;
        [SerializeField] private float maxSteerAngle = 30f;
        [SerializeField] private float minSteerAngle = 10f;
        [SerializeField] private float steerFadeSpeed = 60f;
        
        [Header("Physics settings")]
        [SerializeField] private float turnDrag = 0.98f;
        [SerializeField] private float gripRestoreSpeed = 20f;
        [SerializeField] private float wheelGrip = 2.0f;
        //[SerializeField] private float slipThreshold = 5f;
        //[SerializeField] private float driftGThreshold = 0.8f;
        //[SerializeField] private float driftHoldTime = 2f;
        [SerializeField] private float driftGChangeThreshold = 0.4f;
        [SerializeField] private float driftRecoveryG = 0.4f;
        [SerializeField] private float aeroDrag = 0.3f;
        [SerializeField] private float rollingResistance = 50f;

        
        [Header("Boost settings")]
        [SerializeField] private float boostForce = 5000f;
        [SerializeField] private float boostDuration = 2f;
        [SerializeField] private float boostCooldown = 5f;


        private Rigidbody _rb;
        private float _throttleInput;
        private float _steerInput;
        private InputSystem_Actions _carControls;
        //private float _lateralG; 
        private bool _isDrifting;
        private bool _isBoosting;
        //private float _boostTimer;
        private float _cooldownRemaining = 0f;
        //private float _boostCooldownTimer;
        private bool _canBoost = true;
        private Coroutine _boostCoroutine;
        //private float _driftTimer = 0f;
        //private float _lastLateralG;
        //private float _gChange;
        private Vector3 _lastLocalVelocity;
        private float _sidewaysG;
        private float _deltaG;
        private float _lastSidewaysG;


        private float _boostCooldownProgress = 1f;

        public bool IsBraking { get; private set; }

        private void Awake()
        {
            _carControls = new InputSystem_Actions();
            _rb = GetComponent<Rigidbody>();
            if (centerOfMass != null)
                _rb.centerOfMass = centerOfMass.localPosition;
        }

        private void OnEnable()
        {
            _carControls.Enable();

            _carControls.Player.Throttle.performed += ctx => _throttleInput = ctx.ReadValue<float>();
            _carControls.Player.Throttle.canceled  += ctx => _throttleInput = 0f;

            _carControls.Player.Steer.performed += ctx => _steerInput = ctx.ReadValue<float>();
            _carControls.Player.Steer.canceled  += ctx => _steerInput = 0f;
            
            _carControls.Player.Boost.performed += ctx => TryBoost();
        }

        private void OnDisable()
        {
            SetVibration(0f, 0f);
            _carControls.Disable();
        }

        private void FixedUpdate()
        {
            UpdateDriftState();
            HandleMotor();
            ApplyAerodynamicDrag();
            HandleSteering();
            ApplyTurnResistance();
            UpdateGrip();
        }

        private void HandleMotor()
        {
            float vertical = _throttleInput;
            float currentSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float motorTorque = 0f;
            float brakeTorque = 0f;
            IsBraking = false;

            const float stopThreshold = 1f;

            if (vertical > 0f)
            {
                if (currentSpeed < -stopThreshold)
                {
                    brakeTorque = brakeForce;
                    IsBraking = true;
                }
                else
                {
                    motorTorque = vertical * motorForce;
                }
            }
            else if (vertical < 0f)
            {
                if (currentSpeed > stopThreshold)
                {
                    brakeTorque = brakeForce;
                    IsBraking = true;
                }
                else
                {
                    motorTorque = vertical * motorForce * 0.6f;
                }
            }
            else
            {
                brakeTorque = 0.05f * brakeForce;
            }

            foreach (var wheel in rearWheels)
            {
                wheel.motorTorque = motorTorque;
                wheel.brakeTorque = brakeTorque;
            }

            foreach (var wheel in frontWheels)
                wheel.brakeTorque = brakeTorque;
            
            //Debug.Log($"BrakeTorque {brakeTorque}, MotorTorque {motorTorque}, Vel {_rb.linearVelocity.magnitude}");

            
        }


        private void HandleSteering()
        {
            float speed = _rb.linearVelocity.magnitude;
            float speedFactor = Mathf.Clamp01(speed / (steerFadeSpeed / 3.6f)); 
            
            float currentMaxSteer = Mathf.Lerp(maxSteerAngle, minSteerAngle, speedFactor);
            
            //Debug.Log(currentMaxSteer);
            float steer = _steerInput * currentMaxSteer;

            foreach (var wheel in frontWheels)
                wheel.steerAngle = steer;
        }


        private void ApplyTurnResistance()
        {
            if (_rb.linearVelocity.sqrMagnitude < 0.1f)
                return;

            Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
            localVel.x *= turnDrag;
            _rb.linearVelocity = transform.TransformDirection(localVel);
        }

        private void UpdateGrip()
        {
            float throttle = Mathf.Abs(_throttleInput);
            float steer = Mathf.Abs(_steerInput);
            float speed = _rb.linearVelocity.magnitude;
            bool coasting = throttle < 0.1f;
            
            float restoreSpeedMs = gripRestoreSpeed / 3.6f;

            // Base stiffness
            float frontSide = wheelGrip + 0.2f;
            float rearSide  = wheelGrip + 0.1f;
            float forward   = wheelGrip;

            // Lift-off balance
            if (coasting)
            {
                _rb.AddTorque(transform.up * (_steerInput * 0.5f), ForceMode.Acceleration);
                frontSide *= 1.3f;
                rearSide  *= 0.9f;
                forward   *= 0.9f;
            }
            
            // drift and slip
            bool burnout = speed <= restoreSpeedMs;

            if (_isDrifting)
            {
                frontSide *= 0.9f;
                rearSide  *= 0.5f;
                forward   *= 0.8f;
            }
            else
            {
                frontSide = Mathf.Max(frontSide, 1.2f);
                rearSide  = Mathf.Max(rearSide, 1.1f);
                forward   = Mathf.Max(forward, 1.0f);
            }


            if (burnout)
            {
                rearSide *= Mathf.Lerp(1f, 0.1f, steer);
                forward *= 0.7f;
            }
            else
            {
                rearSide  = Mathf.Max(rearSide, 1.1f);
                forward   = Mathf.Max(forward, 1.0f);
            }

            foreach (var f in frontWheels)
            {
                var fr = f.sidewaysFriction; fr.stiffness = frontSide; f.sidewaysFriction = fr;
                var ff = f.forwardFriction;  ff.stiffness = forward;   f.forwardFriction  = ff;
            }

            foreach (var r in rearWheels)
            {
                var fr = r.sidewaysFriction; fr.stiffness = rearSide;  r.sidewaysFriction = fr;
                var ff = r.forwardFriction;  ff.stiffness = forward;   r.forwardFriction  = ff;
            }
            
            //Debug.Log($"FrontSide: {frontSide}, RearSide: {rearSide}, Forward: {forward}");
        }
        
        
        
        private void TryBoost()
        {
            if (!_canBoost) return;
            _boostCoroutine = StartCoroutine(BoostRoutine());
        }

        private IEnumerator BoostRoutine()
        {
            _canBoost = false;
            _cooldownRemaining = boostCooldown;
            
            SetVibration(0.8f, 1.0f);
            float timer = boostDuration;
            while (timer > 0f)
            {
                _rb.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
                timer -= Time.fixedDeltaTime;
                _boostCooldownProgress = timer / boostDuration;
                yield return new WaitForFixedUpdate();
            }
            
            SetVibration(0f, 0f);
            
            while (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= Time.deltaTime;
                _boostCooldownProgress = 1f - (_cooldownRemaining / boostCooldown);
                yield return null;
            }

            _canBoost = true;
            _cooldownRemaining = 0f;
        }
        
        private void SetVibration(float low, float high)
        {
            if (Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(low, high);
        }

        
        public float GetBoostCooldownProgress()
        {
            return _boostCooldownProgress;
        }

        
        private void UpdateDriftState()
        {
            Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
            float lateralAccel = (localVel.x - _lastLocalVelocity.x) / Time.fixedDeltaTime;
            _lastLocalVelocity = localVel;

            _sidewaysG = Mathf.Abs(lateralAccel) / 9.81f;
            
            _deltaG = Mathf.Abs((_sidewaysG - _lastSidewaysG) / Time.fixedDeltaTime);
            _lastSidewaysG = _sidewaysG;
            
            if (!_isDrifting && _deltaG > driftGChangeThreshold)
                _isDrifting = true;
            else if (_isDrifting && _deltaG < driftRecoveryG)
                _isDrifting = false;
            //Debug.Log($"Sideways G: {_sidewaysG:F2}, LateralAccel: {lateralAccel:F2}, Drifting: {_isDrifting}");
        }
        
        
        private void ApplyAerodynamicDrag()
        {
            float speed = _rb.linearVelocity.magnitude;
            Vector3 dragForce = -_rb.linearVelocity.normalized * (speed * speed * aeroDrag + rollingResistance);
            _rb.AddForce(dragForce, ForceMode.Force);
        }
        
    }
}
