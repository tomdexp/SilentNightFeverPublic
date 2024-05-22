using System;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio.Bindings
{
    [RequireComponent(typeof(GameManager))]
    public class GameManagerAudio : NetworkBehaviour
    {
        private GameManager _gameManager;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _gameManager = GetComponent<GameManager>();
            _gameManager.OnGameStarted += OnGameStarted;
            _gameManager.OnAnyRoundStarted += OnAnyRoundStarted;
            _gameManager.OnAnyRoundEnded += OnAnyRoundEnded;
            _gameManager.OnGameEnded += OnGameEnded;
            Logger.LogTrace("GameManagerAudio registered events !", Logger.LogType.Server, this);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            if(!GameManager.HasInstance) return;
            _gameManager.OnGameStarted -= OnGameStarted;
            _gameManager.OnAnyRoundStarted -= OnAnyRoundStarted;
            _gameManager.OnAnyRoundEnded -= OnAnyRoundEnded;
            _gameManager.OnGameEnded -= OnGameEnded;
            Logger.LogTrace("GameManagerAudio unregistered events !", Logger.LogType.Server, this);
        }

        [Server]
        private void OnGameStarted()
        {
            Logger.LogTrace("GameManagerAudio: OnGameStarted", Logger.LogType.Server, this);
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventGameStart, transform.gameObject);
        }

        [Server]
        private void OnAnyRoundStarted(byte _)
        {
            Logger.LogTrace("GameManagerAudio: OnAnyRoundStarted", Logger.LogType.Server, this);
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventRoundStart, transform.gameObject);
            // pick a new track number
            int randomTrackNumber = UnityEngine.Random.Range(
                AudioManager.Instance.AudioManagerData.RTPC_GP_MUSC_SWITCH_MinValue, 
                AudioManager.Instance.AudioManagerData.RTPC_GP_MUSC_SWITCH_MaxValue+1); // we do +1 because Random.Range is exclusive on the max value
            AudioManager.Instance.SetNetworkedRTPC(AudioManager.Instance.AudioManagerData.RTPC_GP_MUSC_SWITCH.Id, randomTrackNumber, transform.gameObject);
        }

        [Server]
        private void OnAnyRoundEnded(byte _)
        {
            Logger.LogTrace("GameManagerAudio: OnAnyRoundEnded", Logger.LogType.Server, this);
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventRoundEnd, transform.gameObject);
        }

        [Server]
        private void OnGameEnded(PlayerTeamType _)
        {
            Logger.LogTrace("GameManagerAudio: OnGameEnded", Logger.LogType.Server, this);
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventGameEnd, transform.gameObject);
        }
    }
}