using _RacingGamePrototype.Scripts.Car;
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

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            CarController.OnBoostStart += HandleBoostStart;
            CarController.OnBoostEnd += HandleBoostEnd;
            CarController.OnPickup += HandlePickup;
            CarController.OnRechargeBoost += HandleRecharge;
        }

        private void OnDisable()
        {
            CarController.OnBoostStart -= HandleBoostStart;
            CarController.OnBoostEnd -= HandleBoostEnd;
            CarController.OnPickup -= HandlePickup;
            CarController.OnRechargeBoost -= HandleRecharge;
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

        private void HandlePickup()
        {
            //PlayOneShot(pickup); // Disabled because currently all pickups use their own sounds already
        }
        
        private void HandleRecharge()
        {
            PlayOneShot(recharge);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (!clip) return;
            _source.PlayOneShot(clip);
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