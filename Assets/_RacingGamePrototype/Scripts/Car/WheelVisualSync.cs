using UnityEngine;


namespace _RacingGamePrototype.Scripts.Car
{
    [System.Serializable]
    public struct WheelPair
    {
        public WheelCollider collider;
        public Transform mesh;
    }
    public sealed class WheelVisualSync : MonoBehaviour
    {
        [SerializeField] private WheelPair[] wheels;

        private void FixedUpdate()
        {
            foreach (var wheel in wheels)
            {
                UpdateSingleWheel(wheel);
            }
        }

        private static void UpdateSingleWheel(WheelPair wheel)
        {
            if (!wheel.collider || !wheel.mesh) return;
            
            wheel.collider.GetWorldPose(out var pos, out var rot);
            wheel.mesh.position = pos;
            wheel.mesh.rotation = rot;
        }
    }
}

