using System;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks.Voodoo
{
    [RequireComponent(typeof(TongueAnchor))]
    public class Landmark_Voodoo_Puppet : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private ParticleSystem _voodooParticlesLoop;
        [SerializeField, Required] private ParticleSystem _voodooParticlesRelease;
        private TongueAnchor _tongueAnchor;
        private bool _isVoodooActive;
        
        private void Awake()
        {
            _tongueAnchor = GetComponent<TongueAnchor>();
        }

        private void Start()
        {
            _tongueAnchor.OnTongueBindChange += OnTongueBindChange;
        }

        private void OnDestroy()
        {
            _tongueAnchor.OnTongueBindChange -= OnTongueBindChange;
        }

        private void OnTongueBindChange(PlayerStickyTongue tongue)
        {
            if (!_isVoodooActive && tongue)
            {
                _isVoodooActive = true;
                _voodooParticlesLoop.Play();
            }
            else if (_isVoodooActive && !tongue)
            {
                _isVoodooActive = false;
                _voodooParticlesLoop.Stop();
                _voodooParticlesRelease.Play();
            }
        }
    }
}