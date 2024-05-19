using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;


namespace _Project.Scripts.Runtime.Audio
{
    /// <summary>
    /// This component enable the AkGameObject component to get the four players listeners
    /// </summary>
    [RequireComponent(typeof(AkGameObj))]
    public class AkGameObjSNFListenerAdapter : MonoBehaviour
    {
        private AkGameObj _akGameObj;

        private void Awake()
        {
            _akGameObj = GetComponent<AkGameObj>();
        }

        void Start()
        {
            StartCoroutine(TrySubscribingToGameStartedEvent());
        }

        private IEnumerator TrySubscribingToGameStartedEvent()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.IsGameStarted.OnChange += OnGameStarted;
        }

        private void OnGameStarted(bool prev, bool next, bool asserver)
        {
            if (next == true && prev == false)
            {
                FindPlayersAndSetupListeners();
            }
        }

        private void FindPlayersAndSetupListeners()
        {
            var Player1 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.A).gameObject;
            var Player2 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.B).gameObject;
            var Player3 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.C).gameObject;
            var Player4 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.D).gameObject;
            var Player1Listener = Player1.GetComponentInChildren<AkAudioListener>();
            var Player2Listener = Player2.GetComponentInChildren<AkAudioListener>();
            var Player3Listener = Player3.GetComponentInChildren<AkAudioListener>();
            var Player4Listener = Player4.GetComponentInChildren<AkAudioListener>();
            
            if (!Player1 || !Player2 || !Player3 || !Player4)
            {
                Logger.LogError("Could not find all players!", Logger.LogType.Local, this);
                return;
            }
            if (!Player1Listener || !Player2Listener || !Player3Listener || !Player4Listener)
            {
                Logger.LogError("Could not find all audio listeners!", Logger.LogType.Local, this);
                return;
            }
            
            _akGameObj.AddListener(Player1Listener);
            _akGameObj.AddListener(Player2Listener);
            _akGameObj.AddListener(Player3Listener);
            _akGameObj.AddListener(Player4Listener);
        }
    }
}