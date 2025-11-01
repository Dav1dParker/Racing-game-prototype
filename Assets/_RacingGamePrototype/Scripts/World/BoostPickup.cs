using UnityEngine;

namespace _RacingGamePrototype.Scripts.World
{
    public sealed class BoostPickup : PickableItem
    {
        [SerializeField] private float boostForce = 7000f;
        [SerializeField] private float boostDuration = 1.5f;

        protected override void OnPickedUp(Collider collector)
        {
            var car = collector.GetComponent<_RacingGamePrototype.Scripts.Car.CarController>();
            if (car)
            {
                car.ApplyInstantBoost(boostForce, boostDuration);
            }
        }
    }
}