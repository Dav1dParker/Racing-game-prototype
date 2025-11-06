using _RacingGamePrototype.Scripts.Car;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.car
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class BoostFlame : MonoBehaviour
    {
        private ParticleSystem _flame;

        private void Awake()
        {
            _flame = GetComponent<ParticleSystem>();
            _flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnEnable()
        {
            CarController.OnBoostStart += HandleBoostStart;
            CarController.OnBoostEnd += HandleBoostEnd;
        }

        private void OnDisable()
        {
            CarController.OnBoostStart -= HandleBoostStart;
            CarController.OnBoostEnd -= HandleBoostEnd;
        }

        private void HandleBoostStart()
        {
            if (!_flame.isPlaying)
                _flame.Play();
        }

        private void HandleBoostEnd()
        {
            if (_flame.isPlaying)
                _flame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}