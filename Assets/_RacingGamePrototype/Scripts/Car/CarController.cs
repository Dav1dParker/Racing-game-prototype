using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem;

namespace _RacingGamePrototype.Scripts.Car
{
    public class CarController : MonoBehaviour
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
        private Vector2 _moveInput;
        private bool _breaking;
        private InputSystem_Actions _carControls;


        private void Awake()
        {
            _carControls = new InputSystem_Actions();
            _rb = GetComponent<Rigidbody>();
            if (centerOfMass != null)
            {
                _rb.centerOfMass = centerOfMass.localPosition;
            }
        }
        
        private void OnEnable()
        {
            _carControls.Enable();
            _carControls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _carControls.Player.Move.canceled  += ctx => _moveInput = Vector2.zero;
            //_carControls.Player.Brake.performed += ctx => brakePressed = true;
            //_carControls.Player.Brake.canceled  += ctx => brakePressed = false;
        }

        private void OnDisable()
        {
            _carControls.Disable();
        }

        
        public void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
            if (ctx.performed)
                Debug.Log($"Move input: {_moveInput}");
        }


        private void FixedUpdate()
        {
            HandleMotor();
            HandleSteering();
            ApplyTurnResistance();
        }
        
        
        private void HandleMotor()
        {
            float vertical = _moveInput.y;
            float currentSpeed = Vector3.Dot(_rb.linearVelocity,transform.forward);
            bool movingForward = currentSpeed > 1f;
            bool movingBackward = currentSpeed < -1f;
            float motorTorque = 0;
            float brakeTorque = 0;

            if (vertical > 0f)
            {
                if (movingBackward)
                    brakeTorque = brakeForce;
                else
                    motorTorque = vertical * motorForce;
            }
            else if (vertical < 0f)
            {
                if (movingForward)
                    brakeTorque = brakeForce;
                else
                    motorTorque = vertical * motorForce;
            }
            else
            {
                brakeTorque = 0.1f * brakeForce;
            }

            foreach (WheelCollider wheel in rearWheels)
            {
                wheel.motorTorque = motorTorque;
                wheel.brakeTorque = brakeTorque;
            }

            foreach (WheelCollider wheel in frontWheels)
            {
                wheel.brakeTorque = brakeTorque;
            }
        }

        private void HandleSteering()
        {
            float steer = _moveInput.x * maxSteerAngle;
            foreach (var wheel in frontWheels)
                wheel.steerAngle = steer;
        }

        private void ApplyTurnResistance()
        {
            if (_rb.linearVelocity.magnitude > slipThreshold && Math.Abs(_moveInput.x) > 0.1f)
            {
                _rb.linearVelocity *= turnDrag;
            }
        }
    }
}

