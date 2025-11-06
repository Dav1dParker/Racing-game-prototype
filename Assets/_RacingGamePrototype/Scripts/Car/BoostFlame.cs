using UnityEngine;

namespace _RacingGamePrototype.Scripts.Car
{
    public sealed class BoostFlame : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] flame;
        [SerializeField] private CarController car;

        private void Update()
        {
            if (!car || !flame[0]) return;

            foreach (var f in flame)
            {
                var emission = f.emission;
                emission.enabled = car.IsBoosting();
            }
            
        }
    }
}