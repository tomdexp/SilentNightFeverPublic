using System;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio.Bindings
{
    [RequireComponent(typeof(PlayerStickyTongue))]
    public class PlayerStickyTongueAudio : MonoBehaviour
    {
        private PlayerStickyTongue _playerStickyTongue;
        private PlayerAkAudioListener _playerAkAudioListener;

        private void Start()
        {
            _playerStickyTongue = GetComponent<PlayerStickyTongue>();
            _playerAkAudioListener = GetComponentInParent<PlayerAkAudioListener>();
            if (!_playerAkAudioListener)
            {
                Logger.LogError("PlayerStickyTongueAudio: No PlayerAkAudioListener found in parent !", Logger.LogType.Local, this);
            }
            _playerStickyTongue.OnTongueOut += OnTongueOut;
            _playerStickyTongue.OnTongueRetractStart += OnTongueRetractStart;
            _playerStickyTongue.OnTongueBind += OnTongueInteractOrBind;
            _playerStickyTongue.OnTongueInteract += OnTongueInteractOrBind;
        }

        private void OnDestroy()
        {
            _playerStickyTongue.OnTongueOut -= OnTongueOut;
            _playerStickyTongue.OnTongueRetractStart -= OnTongueRetractStart;
            _playerStickyTongue.OnTongueBind -= OnTongueInteractOrBind;
            _playerStickyTongue.OnTongueInteract -= OnTongueInteractOrBind;
        }

        // Since OnTongueOut is already replicated, we just play the audio locally
        private void OnTongueOut()
        {
            AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerTongueThrow, _playerAkAudioListener.gameObject);
        } 
        
        // Since OnTongueRetractStart is already replicated, we just play the audio locally
        private void OnTongueRetractStart()
        {
            AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerTongueRetract, _playerAkAudioListener.gameObject);
        }
        
        // Since OnTongueBind and OnTongueInteract are already replicated, we just play the audio locally
        private void OnTongueInteractOrBind()
        {
            AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerTongueInteractOrBind, _playerAkAudioListener.gameObject);
        }
    }
}