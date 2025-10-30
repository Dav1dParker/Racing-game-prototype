using System;
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
        
        [Header("Physics settings")]
        [SerializeField] private float turnDrag = 0.98f;
        [SerializeField] private float gripRestoreSpeed = 20f;
        [SerializeField] private float wheelGrip = 2.0f;
        //[SerializeField] private float slipThreshold = 5f;
        [SerializeField] private float driftGThreshold = 0.8f;
        [SerializeField] private float driftRecoveryG = 0.4f;


        private Rigidbody _rb;
        private float _throttleInput;
        private float _steerInput;
        private InputSystem_Actions _carControls;
        private float _lateralG; 
        private bool _isDrifting;

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
        }

        private void OnDisable() => _carControls.Disable();

        private void FixedUpdate()
        {
            float lateralAccel = Mathf.Abs((_rb.angularVelocity.y * _rb.linearVelocity.magnitude)) / 9.81f; // G force
            _lateralG = Mathf.Lerp(_lateralG, lateralAccel, Time.fixedDeltaTime * 10f);
            
            
            if (!_isDrifting && _lateralG > driftGThreshold)
                _isDrifting = true;
            else if (_isDrifting && _lateralG < driftRecoveryG)
                _isDrifting = false;
            
            HandleMotor();
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
            float steer = _steerInput * maxSteerAngle;
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
                _rb.AddTorque(transform.up * (_steerInput * 2f), ForceMode.Acceleration);
                frontSide = 1.4f;
                rearSide  = 0.9f;
                forward   = 0.9f;
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

    }
}
