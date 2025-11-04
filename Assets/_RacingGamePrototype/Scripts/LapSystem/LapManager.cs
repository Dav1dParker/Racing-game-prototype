using UnityEngine;

namespace _RacingGamePrototype.Scripts.LapSystem
{
    public sealed class LapManager : MonoBehaviour
    {
        public static LapManager Instance { get; private set; }

        [Header("Lap Settings")]
        [SerializeField] private int totalCheckpoints = 3;

        private int _nextCheckpoint = 0;
        private float _lapStartTime;
        private float _currentLapTime;
        private float _bestLapTime = Mathf.Infinity;
        private bool _lapActive = false;
        private bool _hasStarted = false;
        private bool _allCheckpointsPassed = false;

        public float CurrentLapTime => _currentLapTime;
        public float BestLapTime => _bestLapTime;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (_lapActive)
                _currentLapTime = Time.time - _lapStartTime;
        }

        public void OnCheckpointPassed(int index)
        {
            if (!_hasStarted && index == 0)
            {
                StartNewLap();
                _hasStarted = true;
                return;
            }

            if (!_lapActive)
                return;
            
            if (index == _nextCheckpoint)
            {
                _nextCheckpoint++;
                
                if (_nextCheckpoint >= totalCheckpoints)
                {
                    _allCheckpointsPassed = true;
                    _nextCheckpoint = 0;
                }
                
            }
            
            if (index == 0 && _allCheckpointsPassed)
            {
                CompleteLap();
                StartNewLap();
            }
        }

        private void StartNewLap()
        {
            _lapStartTime = Time.time;
            _currentLapTime = 0f;
            _lapActive = true;
            _allCheckpointsPassed = false;
            _nextCheckpoint = 1;
            Debug.Log("Lap started!");
        }

        private void CompleteLap()
        {
            _lapActive = false;
            float lapTime = Time.time - _lapStartTime;

            if (lapTime < _bestLapTime)
                _bestLapTime = lapTime;

            Debug.Log($"Lap finished! Time: {lapTime:F2}s, Best: {_bestLapTime:F2}s");
        }
    }
}
