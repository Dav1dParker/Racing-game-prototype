using _RacingGamePrototype.Scripts.Input;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.Car
{
    [DisallowMultipleComponent]
    public sealed class CarController : MonoBehaviour
    {
        [SerializeField] private CarInputHandler input;
        [SerializeField] private CarPhysicsController physics;
        [SerializeField] private CarBoostSystem boost;

        private void FixedUpdate()
        {
            if (!input || !physics || !boost) return;

            boost.SetInputDevice(input.UsingGamepad);
            physics.FixedUpdatePhysics(input.Throttle, input.Steer);

            if (input.BoostPressed && !boost.IsBoosting)
                boost.TryBoost();
            
        }
        
        public bool IsBoosting() => boost && boost.IsBoosting;

        public void ResetCar(Vector3 position, Quaternion rotation)
        {
            if (boost != null)
                boost.ResetBoost();
            if (physics != null)
                physics.ResetPhysics(position, rotation);
        }
    }
}