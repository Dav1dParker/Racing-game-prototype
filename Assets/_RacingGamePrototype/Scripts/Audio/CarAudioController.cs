using _RacingGamePrototype.Scripts.Car;
using _RacingGamePrototype.Scripts.LapSystem;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class CarAudioController : MonoBehaviour
    {
        [SerializeField] private AudioClip boostStart;
        //[SerializeField] private AudioClip boostLoop;
        //[SerializeField] private AudioClip boostEnd;
        [SerializeField] private AudioClip pickup;
        [SerializeField] private AudioClip recharge;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float musicVolume = 0.4f;
        private AudioSource _musicSource;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();

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
            CarController.OnBoostStart += HandleBoostStart;
            CarController.OnBoostEnd += HandleBoostEnd;
            CarController.OnPickup += HandlePickup;
            CarController.OnRechargeBoost += HandleRecharge;
            LapManager.OnLapFinished += HandleLapFinished;
        }

        private void OnDisable()
        {
            CarController.OnBoostStart -= HandleBoostStart;
            CarController.OnBoostEnd -= HandleBoostEnd;
            CarController.OnPickup -= HandlePickup;
            CarController.OnRechargeBoost -= HandleRecharge;
            LapManager.OnLapFinished -= HandleLapFinished;
        }

        private void HandleBoostStart()
        {
            PlayOneShot(boostStart);
            //PlayLoop(boostLoop);
        }

        private void HandleBoostEnd()
        {
            StopLoop();
            //PlayOneShot(boostEnd);
        }
        
        private void HandleLapFinished()
        {
            PlayOneShot(recharge, 0.7f, 0.5f);
        }

        private void HandlePickup()
        {
            //PlayOneShot(pickup); // Disabled because currently all pickups use their own sounds already
        }
        
        private void HandleRecharge()
        {
            PlayOneShot(recharge, 0.5f, 1f);
        }
        
        private void PlayOneShot(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (!clip || !_source) return;
            _source.clip = clip;
            _source.pitch = pitch;
            _source.volume = volume;
            _source.loop = false;
            _source.Play();
        }


        private void PlayLoop(AudioClip clip)
        {
            if (!clip) return;
            _source.clip = clip;
            _source.loop = true;
            _source.Play();
        }

        private void StopLoop()
        {
            _source.loop = false;
            _source.Stop();
        }
    }
}