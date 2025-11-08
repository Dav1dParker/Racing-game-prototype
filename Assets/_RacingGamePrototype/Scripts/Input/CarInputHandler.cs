using UnityEngine;
using UnityEngine.InputSystem;

namespace _RacingGamePrototype.Scripts.Input
{
    [DisallowMultipleComponent]
    public sealed class CarInputHandler : MonoBehaviour
    {
        private InputSystem_Actions _controls;
        private double _lastGamepadTime;
        private double _lastKeyboardTime;

        public float Throttle { get; private set; }
        public float Steer { get; private set; }
        public bool BoostPressed { get; private set; }
        public bool UsingGamepad { get; private set; }

        private void Awake()
        {
            _controls = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _controls.Enable();

            _controls.Player.Throttle.performed += ctx =>
            {
                Throttle = ctx.ReadValue<float>();
                UpdateLastDevice(ctx);
            };
            _controls.Player.Throttle.canceled += _ => Throttle = 0f;

            _controls.Player.Steer.performed += ctx =>
            {
                Steer = ctx.ReadValue<float>();
                UpdateLastDevice(ctx);
            };
            _controls.Player.Steer.canceled += _ => Steer = 0f;

            _controls.Player.Boost.performed += ctx =>
            {
                BoostPressed = true;
                UpdateLastDevice(ctx);
            };
            _controls.Player.Boost.canceled += _ => BoostPressed = false;
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void UpdateLastDevice(InputAction.CallbackContext ctx)
        {
            if (ctx.control.device is Gamepad)
                _lastGamepadTime = ctx.time;
            else if (ctx.control.device is Keyboard || ctx.control.device is Mouse)
                _lastKeyboardTime = ctx.time;

            UsingGamepad = _lastGamepadTime > _lastKeyboardTime;
        }
    }
}