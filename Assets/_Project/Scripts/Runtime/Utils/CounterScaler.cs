using System;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class CounterScaler : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        private Vector3 _initialScale;
        private Vector3 _targetInitialScale;

        private void Start()
        {
            _initialScale = transform.localScale;
            _targetInitialScale = _target.localScale;
        }
        
        // Keep the ratio opposite to the target's scale
        private void Update()
        {
            var ratio = _target.localScale.x / _targetInitialScale.x;
            transform.localScale = _initialScale / ratio;
        }
    }
}