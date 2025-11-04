using UnityEngine;

namespace _RacingGamePrototype.Scripts.LapSystem
{
    [RequireComponent(typeof(Collider))]
    public sealed class Checkpoint : MonoBehaviour
    {
        public int index;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LapManager.Instance?.OnCheckpointPassed(index);
            }
        }
    }
}