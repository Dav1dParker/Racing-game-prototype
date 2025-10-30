using UnityEngine;
using TMPro;



namespace _RacingGamePrototype.Scripts.UI
{
    public sealed class Speedometer : MonoBehaviour
    {
        [SerializeField] private Rigidbody targetRigidbody;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private string format = "{0:0} km/h";

        private void Update()
        {
            if (!targetRigidbody || !speedText) return;

            float speed = targetRigidbody.linearVelocity.magnitude * 3.6f; // m/s → km/h
            speedText.text = string.Format(format, speed);
        }
    }
}