using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(SphereCollider))]
    [DisallowMultipleComponent]
    public class Avoider : MonoBehaviour
    {
        [SerializeField] private float _avoidanceRadius = 1f;
        [SerializeField] private float _avoidanceSpeed = 1f;
        [SerializeField] private float _comeBackToOriginalPositionSpeed = 1f;
        [SerializeField] private float _returnCooldown = 2f; // Time to wait before returning to original position
        
        private Vector3 _originalPosition;
        private SphereCollider _sphereCollider;
        private List<Avoidable> _avoidables = new List<Avoidable>();
        private float _cooldownTimer;

        
        private void Awake()
        {
            var sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
        }

        private void Start()
        {
            _originalPosition = transform.position;
        }

        private void Update()
        {
            Avoid();
        }

        private void Avoid()
        {
            if (_avoidables.Count > 0)
            {
                _cooldownTimer = _returnCooldown; // Reset cooldown timer
                Vector3 avoidanceDirection = Vector3.zero;
                foreach (var avoidable in _avoidables)
                {
                    avoidanceDirection += transform.position - avoidable.transform.position;
                }
                avoidanceDirection = avoidanceDirection.normalized;
                avoidanceDirection.y = 0;
                transform.position += avoidanceDirection * (_avoidanceSpeed * Time.deltaTime);
            }
            else
            {
                // Countdown the cooldown timer
                if (_cooldownTimer > 0)
                {
                    _cooldownTimer -= Time.deltaTime;
                }
                else
                {
                    // Move back to the original position
                    transform.position = Vector3.MoveTowards(transform.position, _originalPosition, _comeBackToOriginalPositionSpeed * Time.deltaTime);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Avoidable avoidable))
            {
                _avoidables.Add(avoidable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Avoidable avoidable))
            {
                _avoidables.Remove(avoidable);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _avoidanceRadius);
        }
    }
}