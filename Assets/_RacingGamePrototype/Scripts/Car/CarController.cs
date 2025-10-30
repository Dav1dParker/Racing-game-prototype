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
        [SerializeField] private float slipThreshold = 5f;

        private Rigidbody _rb;
        private float _throttleInput;
        private float _steerInput;
        private InputSystem_Actions _carControls;

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
            HandleMotor();
            HandleSteering();
            ApplyTurnResistance();
            UpdateGrip();
        }

        private void HandleMotor()
        {
            float vertical = _throttleInput;
            float motorTorque = 0f;
            float brakeTorque = 0f;
            IsBraking = false;

            if (vertical < 0f)
            {
                // Brake immediately on negative input (S)
                brakeTorque = brakeForce;
                IsBraking = true;
            }
            else if (vertical > 0f)
            {
                // Accelerate forward
                motorTorque = vertical * motorForce;
            }
            else
            {
                // Light rolling resistance
                brakeTorque = 0.05f * brakeForce;
            }

            foreach (var wheel in rearWheels)
            {
                wheel.motorTorque = motorTorque;
                wheel.brakeTorque = brakeTorque;
            }

            foreach (var wheel in frontWheels)
                wheel.brakeTorque = brakeTorque;
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
            localVel.x *= turnDrag; // damp sideways drift only
            _rb.linearVelocity = transform.TransformDirection(localVel);
        }

        private void UpdateGrip()
        {
            float throttle = Mathf.Abs(_throttleInput);
            float steer = Mathf.Abs(_steerInput);
            float speed = _rb.linearVelocity.magnitude;
            bool coasting = throttle < 0.1f;

            // Base stiffness
            float frontSide = 1.2f;
            float rearSide  = 1.1f;
            float forward   = 1.0f;

            // Lift-off balance
            if (coasting)
            {
                frontSide = 1.4f;
                rearSide  = 0.9f;
                forward   = 0.9f;
            }

            // Drift scaling
            if (speed > slipThreshold && steer > 0.1f)
            {
                frontSide *= Mathf.Lerp(1f, 0.8f, steer);
                rearSide  *= Mathf.Lerp(1f, 0.6f, steer);
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
        }
    }
}
