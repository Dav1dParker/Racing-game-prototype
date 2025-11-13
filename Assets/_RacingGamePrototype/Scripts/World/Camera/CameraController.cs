using _RacingGamePrototype.Scripts.Car;
using Unity.Cinemachine;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.World.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController car;
        
        [Header("Settings")]
        [SerializeField] private float baseFOV = 60f;
        [SerializeField] private float fovAtMaxSpeed = 20f;
        [SerializeField] private float maxFOV = 85f;
        [SerializeField] private float boostFOV = 100f;
        [SerializeField] private float smooth = 5f;
        
        private CinemachineCamera _virtualCamera;
        private Rigidbody _rb;


        private void Awake()
        {
            if (car)
                _rb = car.GetComponent<Rigidbody>();
            _virtualCamera = GetComponent<CinemachineCamera>();
        }

        private void LateUpdate()
        {
            if (!_rb || !_virtualCamera) return;

            float targetFOV;
            if (car.IsBoosting())
            {
                targetFOV = boostFOV;
            }
            else
            {
                float speed = _rb.linearVelocity.magnitude;
                float t = Mathf.Clamp01(speed / (fovAtMaxSpeed / 3.6f));
                targetFOV = Mathf.Lerp(baseFOV, maxFOV, t);
            }
            var lens = _virtualCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * smooth);
            _virtualCamera.Lens = lens;
        }
    }
}