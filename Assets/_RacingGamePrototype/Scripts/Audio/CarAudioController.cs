using _RacingGamePrototype.Scripts.Car;
using _RacingGamePrototype.Scripts.LapSystem;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class CarAudioController : MonoBehaviour
    {
        [SerializeField] private AudioClip boostStart;
        [SerializeField] private float boostStartVolume = 0.7f;
        [SerializeField] private AudioClip pickup;
        [SerializeField] private float pickupVolume = 0.7f;
        [SerializeField] private AudioClip recharge;
        [SerializeField] private float rechargeVolume = 0.7f;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float musicVolume = 0.4f;

        private AudioSource _sfxSource;
        private AudioSource _boostSource;
        private AudioSource _musicSource;

        private void Awake()
        {
            _sfxSource = GetComponent<AudioSource>();

            _boostSource = gameObject.AddComponent<AudioSource>();
            _boostSource.loop = false;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.clip = backgroundMusic;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;
            _musicSource.playOnAwake = false;
        }

        private void Start()
        {
            if (backgroundMusic && !_musicSource.isPlaying)
                _musicSource.Play();
        }

        private void OnEnable()
        {
            CarBoostSystem.OnBoostStart += HandleBoostStart;
            CarBoostSystem.OnBoostEnd += HandleBoostEnd;
            //CarController.OnPickup += HandlePickup;
            CarBoostSystem.OnRecharge += HandleRecharge;
            LapManager.OnLapFinished += HandleLapFinished;
        }

        private void OnDisable()
        {
            CarBoostSystem.OnBoostStart -= HandleBoostStart;
            CarBoostSystem.OnBoostEnd -= HandleBoostEnd;
            //CarController.OnPickup -= HandlePickup;
            CarBoostSystem.OnRecharge -= HandleRecharge;
            LapManager.OnLapFinished -= HandleLapFinished;
        }

        private void HandleBoostStart()
        {
            if (!_boostSource.isPlaying)
                PlayBoost(boostStart, boostStartVolume);
        }
        private void HandleBoostEnd() => _boostSource.Stop();
        private void HandleLapFinished() => PlaySfx(recharge, 0.7f, 0.5f);
        private void HandlePickup() => PlaySfx(pickup, pickupVolume);
        private void HandleRecharge() => PlaySfx(recharge, rechargeVolume);

        private void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (!clip || !_sfxSource) return;
            _sfxSource.pitch = pitch;
            _sfxSource.volume = volume;
            _sfxSource.PlayOneShot(clip);
        }

        private void PlayBoost(AudioClip clip, float volume = 1f)
        {
            if (!clip || !_boostSource) return;
            if (_boostSource.isPlaying && _boostSource.clip == clip)
                return;

            _boostSource.clip = clip;
            _boostSource.volume = volume;
            _boostSource.loop = false;
            _boostSource.Play();
        }
    }
}