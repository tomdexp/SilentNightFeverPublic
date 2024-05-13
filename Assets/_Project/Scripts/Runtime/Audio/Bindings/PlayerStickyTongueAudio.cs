using _Project.Scripts.Runtime.Player.PlayerTongue;
using UnityEngine;

namespace _Project.Scripts.Runtime.Audio.Bindings
{
    [RequireComponent(typeof(PlayerStickyTongue))]
    public class PlayerStickyTongueAudio : MonoBehaviour
    {
        private PlayerStickyTongue _playerStickyTongue;

        private void Awake()
        {
            _playerStickyTongue = GetComponent<PlayerStickyTongue>();
            _playerStickyTongue.OnTongueOut += OnTongueOut;
            _playerStickyTongue.OnTongueRetractStart += OnTongueRetractStart;
        }

        // Since OnTongueOut is already replicated, we just play the audio locally
        private void OnTongueOut()
        {
            AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerTongueThrow, transform.gameObject);
        } 
        
        // Since OnTongueRetractStart is already replicated, we just play the audio locally
        private void OnTongueRetractStart()
        {
            AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerTongueRetract, transform.gameObject);
        }
    }
}