using System;
using _Project.Scripts.Runtime.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.Feedbacks
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerVoodooFeedbacks : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private ParticleSystem _voodooParticlesLoop;
        [SerializeField, Required] private ParticleSystem _voodooParticlesRelease;
        private bool _isVoodooActive;
        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }
        
        private void Update()
        {
            if (!_isVoodooActive && _playerController.VoodooPuppetDirection.Value != Vector2.zero)
            {
                _isVoodooActive = true;
                _voodooParticlesLoop.Play();
                AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.OnStartControlledByVoodoo, _playerController.gameObject);
                Logger.LogDebug("Start playing voodoo feedbacks", Logger.LogType.Local, this);
            }
            else if (_isVoodooActive && _playerController.VoodooPuppetDirection.Value == Vector2.zero)
            {
                _isVoodooActive = false;
                _voodooParticlesLoop.Stop();
                _voodooParticlesRelease.Play();
                AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.OnEndControlledByVoodoo, _playerController.gameObject);
                Logger.LogDebug("Stop playing voodoo feedbacks", Logger.LogType.Local, this);
            }
        }
    }
}