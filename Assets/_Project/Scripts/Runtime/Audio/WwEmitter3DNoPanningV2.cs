using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Landmarks;
using _Project.Scripts.Runtime.Landmarks.Kitchen;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using QFSW.QC;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = _Project.Scripts.Runtime.Utils.Logger;


namespace _Project.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(AkGameObj))]
    public class WwEmitter3DNoPanningV2 : MonoBehaviour
    {
        [Title("References")]
        public AkEvent EventEnterZoneAtLeastOnePlayer;
        public AkEvent EventExitZoneNoPlayer;
        public SphereCollider Collider;
        public LayerMask PlayerLayer;
        
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
        private AkGameObj _akGameObj;
        private bool _isSetup = false;
        private Landmark _landmark;

        private void Awake()
        {
            _akGameObj = GetComponent<AkGameObj>();
            _landmark = GetComponentInParent<Landmark>();
            if (!_landmark)
            {
                Logger.LogError("No Landmark found in parent!", Logger.LogType.Local, this);
            }
        }

        void Start()
        {
            StartCoroutine(TrySubscribingToGameStartedEvent());
            StartCoroutine(TryFindPlayers());
        }

        private IEnumerator TrySubscribingToGameStartedEvent()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.IsGameStarted.OnChange += OnGameStarted;
            GameManager.Instance.OnAnyRoundStarted += OnAnyRoundStarted;
            GameManager.Instance.OnAnyRoundEnded += OnAnyRoundEnded;
        }
        
        private void OnGameStarted(bool prev, bool next, bool asserver)
        {
            if (next == true && prev == false)
            {
                StartCoroutine(TryFindPlayers());
            }
        }
        
        private void OnAnyRoundStarted(byte _)
        {
            var radius = Collider.radius;
            // do an overlap sphere to check if there is a player in the collider
            Collider[] results = new Collider[4];
            var size = Physics.OverlapSphereNonAlloc(transform.position, radius, results, PlayerLayer);
            Logger.LogDebug($"Landmark {_landmark.gameObject.name} ambiance emitter detected " + size + " player(s) in the zone on round start", Logger.LogType.Local, this);
            if (size > 0)
            {
                _currentNumberPlayer = size;
                if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(EventEnterZoneAtLeastOnePlayer.data, gameObject);
                Logger.LogDebug($"Started playing the Landmark {_landmark.gameObject.name} ambiance", Logger.LogType.Local, this);
            }
        }
        
        private void OnAnyRoundEnded(byte _)
        {
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(EventExitZoneNoPlayer.data, gameObject);
            Logger.LogDebug($"Stopped playing the Landmark {_landmark.gameObject.name} ambiance", Logger.LogType.Local, this);
            _currentNumberPlayer = 0;
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.IsGameStarted.OnChange -= OnGameStarted;
                GameManager.Instance.OnAnyRoundEnded -= OnAnyRoundEnded;
            }
        }

        private IEnumerator TryFindPlayers()
        {
            if (_isSetup)
            {
                yield break;
            }
            Logger.LogTrace("Finding players...", Logger.LogType.Local, this);
            
            while (!PlayerManager.HasInstance)
            {
                //Logger.LogTrace("PlayerManager not found, waiting...", Logger.LogType.Local, this);
                yield return null;
            }

            while (!PlayerManager.Instance.AreAllPlayerSpawnedLocally)
            {
                //Logger.LogTrace("Not all players spawned locally, waiting...", Logger.LogType.Local, this);
                yield return null;
            }
            
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
                yield break;
            }
            if (!Player1Listener || !Player2Listener || !Player3Listener || !Player4Listener)
            {
                Logger.LogError("Could not find all audio listeners!", Logger.LogType.Local, this);
                yield break;
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
            _isSetup = true;

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
            if (other.TryGetComponent(out PlayerController _))
            {
                _currentNumberPlayer += 1;
                Logger.LogTrace($"Player entered the zone of landmark {_landmark.gameObject.name}, there is now " + _currentNumberPlayer + " player(s) in the zone", Logger.LogType.Local, this);
                if (_currentNumberPlayer == 1)
                {
                    //EventEnterZoneAtLeastOnePlayer.data.Post(gameObject);
                    if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(EventEnterZoneAtLeastOnePlayer.data, gameObject);
                    Logger.LogDebug($"Started playing the Landmark {_landmark.gameObject.name} ambiance", Logger.LogType.Local, this);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerController _))
            {
                _currentNumberPlayer -= 1;
                Debug.Log(_currentNumberPlayer);
                Logger.LogTrace($"Player exited the zone of landmark {_landmark.gameObject.name}, there is now " + _currentNumberPlayer + " player(s) in the zone", Logger.LogType.Local, this);
                if (_currentNumberPlayer == 0)
                {
                    //EventExitZoneNoPlayer.data.Post(gameObject);
                    if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(EventExitZoneNoPlayer.data, gameObject);
                    Logger.LogDebug($"Stopped playing the Landmark {_landmark.gameObject.name} ambiance", Logger.LogType.Local, this);
                }
            }
        }
    }
}