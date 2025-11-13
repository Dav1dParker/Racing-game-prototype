using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


namespace _RacingGamePrototype.Scripts.World.PickableItems
{
    [RequireComponent(typeof(Collider))]
    public abstract class PickableItem : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float floatAmplitude = 0.5f;
        [SerializeField] private float floatFrequency = 1f;

        [Header("Spawn")]
        [SerializeField] private bool shouldRespawn = true;
        [SerializeField] private float respawnTime = 10f;
        
        private Vector3 _startPosition;
        private float _offset;
        private Collider _collider;
        private Renderer _renderer;
        
        protected virtual void Awake()
        {
            _startPosition = transform.position;
            _offset = Random.Range(0f, Mathf.PI * 2f);
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
        }

        protected void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            
            Vector3 pos = _startPosition;
            pos.y += Mathf.Sin((Time.time * floatFrequency) + _offset) * floatAmplitude;
            transform.position = pos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPickupTarget(other)) return;
            OnPickedUp(other);

            if (shouldRespawn)
                StartCoroutine(RespawnRoutine());
            else
                Destroy(gameObject);
        }
        
        protected virtual bool IsPickupTarget(Collider other)
        {
            return other.attachedRigidbody && other.attachedRigidbody.CompareTag("Player");
        }

        protected abstract void OnPickedUp(Collider other);
        
        private IEnumerator RespawnRoutine()
        {
            SetVisible(false);
            yield return new WaitForSeconds(respawnTime);
            SetVisible(true);
        }
        
        private void SetVisible(bool state)
        {
            if (_collider) _collider.enabled = state;
            if (_renderer) _renderer.enabled = state;
        }

    }
}

