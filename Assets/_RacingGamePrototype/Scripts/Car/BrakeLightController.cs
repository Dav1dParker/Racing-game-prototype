using UnityEngine;


namespace _RacingGamePrototype.Scripts.Car
{
    public sealed class BrakeLightController : MonoBehaviour
    {
        [SerializeField] private float onIntensity = 4f;
        [SerializeField] private float offIntensity = 0f;
        [SerializeField] private float smooth = 10f;

        private Light _light;
        private bool _isBraking;

        private void Awake() => _light = GetComponent<Light>();

        public void SetBraking(bool braking) => _isBraking = braking;

        private void Update()
        {
            float target = _isBraking ? onIntensity : offIntensity;
            _light.intensity = Mathf.Lerp(_light.intensity, target, Time.deltaTime * smooth);
        }
    }
}
