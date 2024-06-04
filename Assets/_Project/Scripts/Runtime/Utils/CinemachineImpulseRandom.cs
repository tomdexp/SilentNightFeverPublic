using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineImpulseRandom : MonoBehaviour
    {
        [SerializeField] private float _minTimeBetweenImpulse = 1f;
        [SerializeField] private float _maxTimeBetweenImpulse = 5f;
        [SerializeField] private float _waitBeforeFirstImpulse = 1f;
        
        private CinemachineImpulseSource _cinemachineImpulseSource;
        
        private void Awake()
        {
            _cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
        }
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_waitBeforeFirstImpulse);
            while (true)
            {
                _cinemachineImpulseSource.GenerateImpulse();
                yield return new WaitForSeconds(Random.Range(_minTimeBetweenImpulse, _maxTimeBetweenImpulse));
            }
        }
    }
}