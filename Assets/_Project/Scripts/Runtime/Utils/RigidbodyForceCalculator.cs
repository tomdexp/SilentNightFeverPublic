using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class RigidbodyForceCalculator : MonoBehaviour
    {
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private Vector3 _lastVelocity;
        [SerializeField, ReadOnly] private Vector3 _currentVelocity;
        [SerializeField, ReadOnly] private Vector3 _velocityChange;
        [SerializeField, ReadOnly] private Vector3 _acceleration;
        [SerializeField, ReadOnly] private Vector3 _currentForce;
        
        private Rigidbody _rigidbody;
    
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _lastVelocity = _rigidbody.velocity;
        }
    
        void FixedUpdate()
        {
            _currentVelocity = _rigidbody.velocity;
            _velocityChange = _currentVelocity - _lastVelocity;
            _acceleration = _velocityChange / Time.fixedDeltaTime;
            _currentForce = _rigidbody.mass * _acceleration;
        
            _lastVelocity = _currentVelocity;
        }
    }
}