using System;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.LapSystem
{
    public sealed class LapManager : MonoBehaviour
    {
        public static LapManager Instance { get; private set; }

        [Header("Lap Settings")]
        [SerializeField] private int totalCheckpoints = 3;

        private int _nextCheckpoint;
        private float _lapStartTime;
        private float _currentLapTime;
        private float _bestLapTimeForward = Mathf.Infinity;
        private float _bestLapTimeReverse = Mathf.Infinity;
        private bool _lapActive;
        private bool _hasStarted;
        private bool _allCheckpointsPassed;
        private bool _reversed;
        private LapTimeData _lapData;

        public float CurrentLapTime => _currentLapTime;
        public float BestLapTimeForward => _bestLapTimeForward;
        public float BestLapTimeReverse => _bestLapTimeReverse;
        
        public static event Action OnLapFinished;
        public bool IsReversed => _reversed;

        private void Awake()
        {
            Instance = this;
            _lapData = LapTimeStorage.Load();
            LapTimeStorage.Save(_lapData);
            _bestLapTimeForward = _lapData.bestForward;
            _bestLapTimeReverse = _lapData.bestReverse;
        }


        private void Update()
        {
            if (_lapActive)
                _currentLapTime = Time.time - _lapStartTime;
        }

        public void ReverseTrack()
        {
            _reversed = !_reversed;
            ResetLapState();
            Debug.Log($"Track direction reversed: {_reversed}");
        }

        public void ResetLapState()
        {
            _lapActive = false;
            _hasStarted = false;
            _allCheckpointsPassed = false;
            _currentLapTime = 0f;
            _nextCheckpoint = _reversed ? totalCheckpoints - 1 : 0;
            _lapStartTime = 0f;
            Debug.Log("Lap state reset");
        }

        public void OnCheckpointPassed(int index)
        {
            if (!_hasStarted)
            {
                if ((!_reversed && index == 0) || (_reversed && index == totalCheckpoints - 1))
                {
                    StartNewLap();
                    _hasStarted = true;
                }
                return;
            }

            if (!_lapActive)
                return;

            if (!_reversed)
            {
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
            else
            {
                if (index == _nextCheckpoint)
                {
                    _nextCheckpoint--;
                    if (_nextCheckpoint < 0)
                    {
                        _allCheckpointsPassed = true;
                        _nextCheckpoint = totalCheckpoints - 1;
                    }
                }

                if (index == totalCheckpoints - 1 && _allCheckpointsPassed)
                {
                    CompleteLap();
                    StartNewLap();
                }
            }
        }

        private void StartNewLap()
        {
            _lapStartTime = Time.time;
            _currentLapTime = 0f;
            _lapActive = true;
            _allCheckpointsPassed = false;
            _nextCheckpoint = _reversed ? totalCheckpoints - 2 : 1;
            Debug.Log($"Lap started! Reversed: {_reversed}");
        }

        private void CompleteLap()
        {
            _lapActive = false;
            float lapTime = Time.time - _lapStartTime;
            bool recordBroken = false;

            if (_reversed)
            {
                if (lapTime < _lapData.bestReverse)
                {
                    _lapData.bestReverse = lapTime;
                    _bestLapTimeReverse = lapTime;
                    recordBroken = true;
                }
            }
            else
            {
                if (lapTime < _lapData.bestForward)
                {
                    _lapData.bestForward = lapTime;
                    _bestLapTimeForward = lapTime;
                    recordBroken = true;
                }
            }

            if (recordBroken)
                LapTimeStorage.Save(_lapData);

            OnLapFinished?.Invoke();
        }

    }
}
