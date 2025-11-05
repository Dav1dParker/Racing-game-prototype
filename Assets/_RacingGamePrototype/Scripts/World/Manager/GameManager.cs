using UnityEngine;
using _RacingGamePrototype.Scripts.Car;
using _RacingGamePrototype.Scripts.LapSystem;

namespace _RacingGamePrototype.Scripts.World.Manager
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private CarController car;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private LapManager lapManager;

        private InputSystem_Actions _controls;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private bool _isReversed = false;

        private void Awake()
        {
            Instance = this;
            _controls = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            if (_controls == null)
                _controls = new InputSystem_Actions();

            _controls.Enable();
            _controls.Player.Respawn.performed += OnRespawnPerformed;
            _controls.Player.Reverse.performed += OnReversePerformed;
        }

        private void OnDisable()
        {
            if (_controls == null) return;

            _controls.Player.Respawn.performed -= OnRespawnPerformed;
            _controls.Player.Reverse.performed  -= OnReversePerformed;
            _controls.Disable();
        }
        
        private void OnRespawnPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            RespawnCar();
        }

        private void OnReversePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            ReverseTrack();
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (spawnPoint)
            {
                _spawnPosition = spawnPoint.position;
                _spawnRotation = spawnPoint.rotation;
            }
        }

        private void RespawnCar()
        {
            if (!car || !spawnPoint) return;

            var rb = car.Rigidbody;
            var targetPos = spawnPoint.position;
            var targetRot = _isReversed
                ? spawnPoint.rotation * Quaternion.Euler(0f, 180f, 0f)
                : spawnPoint.rotation;

            // Temporarily disable physics so transform change sticks
            rb.isKinematic = true;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPos;
            rb.rotation = targetRot;

            car.transform.SetPositionAndRotation(targetPos, targetRot);

            rb.isKinematic = false;

            LapManager.Instance?.ResetLapState();
        }



        private void ReverseTrack()
        {
            if (!car || !spawnPoint) return;

            _isReversed = !_isReversed;
            LapManager.Instance?.ReverseTrack();

            RespawnCar();
        }

    }
}
