using _RacingGamePrototype.Scripts.Car;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.World.PickableItems
{
    public class RechargePickup : PickableItem
    {
        protected override void OnPickedUp(Collider collector)
        {
            var car = collector.GetComponent<CarBoostSystem>();
            if (car)
            {
                car.RechargeBoost();
            }
        }
    }
}

