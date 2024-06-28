using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineImpulseLoop : MonoBehaviour
    {
        [SerializeField] private bool _useStartDelay = false;
        [SerializeField] private Vector3 _velocity = Vector3.zero;
        private CinemachineImpulseSource _cinemachineImpulseSource;
        private WaitForSeconds _wait;

        private void Awake()
        {
            _cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
            _wait = new WaitForSeconds(_cinemachineImpulseSource.ImpulseDefinition.ImpulseDuration);
        }

        private IEnumerator Start()
        {
            if (_useStartDelay) yield return _wait; // Wait a bit to avoid directly playing the first impulse
            while (true)
            {
                _cinemachineImpulseSource.GenerateImpulseWithVelocity(_velocity);
                yield return _wait;
            }
        }
    }
}