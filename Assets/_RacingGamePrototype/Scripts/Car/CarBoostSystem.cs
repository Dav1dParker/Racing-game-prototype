using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _RacingGamePrototype.Scripts.Car
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarBoostSystem : MonoBehaviour
    {
        [SerializeField] private float boostForce = 5000f;
        [SerializeField] private float boostDuration = 2f;
        [SerializeField] private float boostCooldown = 5f;

        private Rigidbody _rb;
        private Coroutine _boostCoroutine;
        private Coroutine _pickupCoroutine;
        private bool _isBoosting;
        private bool _canBoost = true;
        private float _boostRemaining;
        private float _cooldownRemaining;
        private float _boostCooldownProgress = 1f;
        private bool _usingGamepad;

        public float BoostProgress => _boostCooldownProgress;
        public bool IsBoosting => _isBoosting;

        public static event System.Action OnBoostStart;
        public static event System.Action OnBoostEnd;
        public static event System.Action OnRecharge;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void SetInputDevice(bool usingGamepad)
        {
            _usingGamepad = usingGamepad;
        }

        public void TryBoost()
        {
            if (!_canBoost) return;
            if (_boostCoroutine != null)
                StopCoroutine(_boostCoroutine);
            _boostCoroutine = StartCoroutine(BoostRoutine());
        }

        public void ApplyPickupBoost(float force, float duration)
        {
            if (_pickupCoroutine != null)
                StopCoroutine(_pickupCoroutine);
            _pickupCoroutine = StartCoroutine(PickupBoostRoutine(force, duration));
        }
        
        public void RechargeBoost()
        {
            _canBoost = true;
            _cooldownRemaining = 0f;
            _boostCooldownProgress = 1f;

            OnRecharge?.Invoke();
        }

        public void ResetBoost()
        {
            if (_boostCoroutine != null)
                StopCoroutine(_boostCoroutine);
            if (_pickupCoroutine != null)
                StopCoroutine(_pickupCoroutine);

            _boostCoroutine = null;
            _pickupCoroutine = null;

            _isBoosting = false;
            _canBoost = true;
            _boostRemaining = 0f;
            _cooldownRemaining = 0f;
            _boostCooldownProgress = 1f;

            SetVibration(0f, 0f);
            OnBoostEnd?.Invoke();
        }

        private IEnumerator BoostRoutine()
        {
            _canBoost = false;
            _isBoosting = true;
            _cooldownRemaining = boostCooldown;
            _boostRemaining = boostDuration;

            SetVibration(0.8f, 1.0f);
            OnBoostStart?.Invoke();

            while (_boostRemaining > 0f)
            {
                _rb.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
                _boostRemaining -= Time.fixedDeltaTime;
                _boostCooldownProgress = Mathf.Clamp01(_boostRemaining / boostDuration);
                yield return new WaitForFixedUpdate();
            }

            EndBoost();
        }

        private IEnumerator PickupBoostRoutine(float force, float duration)
        {
            OnBoostStart?.Invoke();
            float t = duration;
            while (t > 0f)
            {
                _rb.AddForce(transform.forward * force, ForceMode.Acceleration);
                t -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            OnBoostEnd?.Invoke();
        }

        private void EndBoost()
        {
            SetVibration(0f, 0f);
            _isBoosting = false;
            OnBoostEnd?.Invoke();
            StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            while (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= Time.deltaTime;
                _boostCooldownProgress = 1f - Mathf.Clamp01(_cooldownRemaining / boostCooldown);
                yield return null;
            }

            _canBoost = true;
            _boostCooldownProgress = 1f;
            OnRecharge?.Invoke();
        }

        private void SetVibration(float low, float high)
        {
            if (!_usingGamepad || Gamepad.current == null) return;
            Gamepad.current.SetMotorSpeeds(low, high);
        }
    }
}
