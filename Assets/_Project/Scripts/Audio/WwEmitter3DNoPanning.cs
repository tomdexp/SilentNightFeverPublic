using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Audio
{
    public class WwEmitter3DNoPanning : MonoBehaviour
    {
        [Title("References")]
        public string EventName = "Default";
        public string StopEvent = "Default";
        [FormerlySerializedAs("akGameObj")] [SerializeField] private AkGameObj _akGameObj;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private bool _isInCollider = false;
        [SerializeField, ReadOnly] private int _currentNumberPlayer = 0;
        [SerializeField, ReadOnly] private List<GameObject> _players = new List<GameObject>();
        [ReadOnly] public GameObject Player1;
        [ReadOnly] public GameObject Player2;
        [ReadOnly] public GameObject Player3;
        [ReadOnly] public GameObject Player4;
        private AkAudioListener Player1Listener;
        private AkAudioListener Player2Listener;
        private AkAudioListener Player3Listener;
        private AkAudioListener Player4Listener;
        private Dictionary<GameObject, AkAudioListener> _playerListeners = new Dictionary<GameObject, AkAudioListener>();
        

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
                FindPlayers();
            }
        }

        private void FindPlayers()
        {
            Logger.LogTrace("Finding players...", Logger.LogType.Local, this);
            
            _players.Clear();
            _playerListeners.Clear();
            
            Player1 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.A).gameObject;
            Player2 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.B).gameObject;
            Player3 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.C).gameObject;
            Player4 = PlayerManager.Instance.GetNetworkPlayer(PlayerIndexType.D).gameObject;
            Player1Listener = Player1.GetComponentInChildren<AkAudioListener>();
            Player2Listener = Player2.GetComponentInChildren<AkAudioListener>();
            Player3Listener = Player3.GetComponentInChildren<AkAudioListener>();
            Player4Listener = Player4.GetComponentInChildren<AkAudioListener>();
            
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
            
            _playerListeners.Add(Player1, Player1Listener);
            _playerListeners.Add(Player2, Player2Listener);
            _playerListeners.Add(Player3, Player3Listener);
            _playerListeners.Add(Player4, Player4Listener);
            
            _players.Add(Player1);
            _players.Add(Player2);
            _players.Add(Player3);
            _players.Add(Player4);
            
            Logger.LogTrace("Players and associated AkAudioListeners found!", Logger.LogType.Local, this);

            AkSoundEngine.RegisterGameObj(gameObject); //maintenant ce gameobject est reconnu dans Wwise
        }

        void Update()
        {
            if(_players.Count == 0) return;
            
            GameObject closestPlayer = null;
            float closestDistance = float.MaxValue;
    
            foreach (var player in _players)
            {
                float distance = Vector3.SqrMagnitude(transform.position - player.transform.position);
                if (distance < closestDistance)
                {
                    closestPlayer = player;
                    closestDistance = distance;
                }
            }
            //En gros on active le plus proche et on desactive tout les autres, mais faudrait voir a pas le faire toutes les frames (et a pas utiliser getcomponentinchildren)
            // Veuch : tom pardonne moi je t'en supplie (tom: je garde cette ligne pour la postérité, l'histoire se souviendra...)
            foreach (var player in _players)
            {
                if (player != closestPlayer)
                {
                    //player.GetComponentInChildren<AkAudioListener>().StopListeningToEmitter(_akGameObj);
                    _playerListeners[player].StopListeningToEmitter(_akGameObj);
                }
            }
    
            //closestPlayer.GetComponentInChildren<AkAudioListener>().StartListeningToEmitter(_akGameObj);
            _playerListeners[closestPlayer].StartListeningToEmitter(_akGameObj);
        }

        private void OnTriggerEnter(Collider other)
        {
            _currentNumberPlayer += 1;
            
            if (!other.CompareTag("PlayerCapsule") || _isInCollider)
            {
                return;
            }
            if (_currentNumberPlayer != 0) {
                _isInCollider = true;
                AkSoundEngine.PostEvent(EventName, gameObject);
                Debug.Log("sound is playing");
            }
        
        }
        private void OnTriggerExit(Collider other)
        {
            _currentNumberPlayer -= 1;
            Debug.Log(_currentNumberPlayer);

            if (!other.CompareTag("PlayerCapsule") || !_isInCollider || _currentNumberPlayer != 0)
            {
                return;
            }
            if(_currentNumberPlayer == 0)
            {
                _isInCollider = false;
                AkSoundEngine.PostEvent(StopEvent, gameObject);
            }
       
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.tag != "Player" || _isInCollider)
            {
                return;
            }

            _isInCollider = true;
            AkSoundEngine.PostEvent(EventName, gameObject);
        
        }
    }
}
