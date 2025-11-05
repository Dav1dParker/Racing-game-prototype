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
            _controls.Enable();
            _controls.Player.Respawn.performed += _ => RespawnCar();
            _controls.Player.Reverse.performed += _ => ReverseTrack();
        }

        private void OnDisable()
        {
            if (_controls != null)
            {
                _controls.Player.Respawn.performed -= _ => RespawnCar();
                _controls.Player.Reverse.performed -= _ => ReverseTrack();
                _controls.Disable();
            }
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
            if (!car) return;

            car.Rigidbody.linearVelocity = Vector3.zero;
            car.Rigidbody.angularVelocity = Vector3.zero;
            car.transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            LapManager.Instance?.ResetLapState();
            //Debug.Log("Car respawned");
        }

        private void ReverseTrack()
        {
            if (!car) return;

            _isReversed = !_isReversed;
            LapManager.Instance?.ReverseTrack();

            car.Rigidbody.linearVelocity = Vector3.zero;
            car.Rigidbody.angularVelocity = Vector3.zero;

            car.transform.position = _spawnPosition;
            if (!_isReversed)
                car.transform.rotation = _spawnRotation;
            else
                car.transform.rotation = _spawnRotation * Quaternion.Euler(0f, 180f, 0f);

            Debug.Log("Track reversed and car flipped");
        }
    }
}
