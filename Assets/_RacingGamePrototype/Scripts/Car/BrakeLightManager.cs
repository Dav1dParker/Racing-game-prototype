using UnityEngine;

namespace _RacingGamePrototype.Scripts.Car
{
    public sealed class BrakeLightManager : MonoBehaviour
    {
        [SerializeField] private BrakeLightController[] lights;
        [SerializeField] private CarController car;

        private void Update()
        {
            if (!car) return;
            foreach (var l in lights)
                l.SetBraking(car.IsBraking);
        }
    }

}
