using System;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;
using Random = UnityEngine.Random;

public class HatSetter : NetworkBehaviour
{
    [SerializeField] private PlayerIndexType _playerIndexType = 0;
    [Space]

    [SerializeField] private bool _randomHat = true;
    [SerializeField, Required] private GameObject _hatContainer = null;
    private MeshRenderer[] _hats;

    private readonly SyncVar<int> _randomHatIndex = new SyncVar<int>(0);
    private NetworkPlayer _networkPlayer;

    private void Awake()
    {
        if (!_hatContainer)
        {
            Debug.LogWarning("HatSetter is missing a reference to the HatContainer GameObject !");
            return;
        }

        _hats = _hatContainer.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
    }

    private void OnEnable()
    {
        _randomHatIndex.OnChange += SetHat;

        if (_randomHat == false)
        {
            // Is that ugly ? Tell me
            // Since we enable our mannequin after the start of the server
            // the OnStartNetwork is not called, so I do it here instead
            StartCoroutine(TrySubscribingToEvents());
        }
    }

    private void OnDisable()
    {
        _randomHatIndex.OnChange -= SetHat;

        if(PlayerManager.HasInstance) PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (_randomHat) _randomHatIndex.Value = Random.Range(0, _hats.Length);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(_randomHat) SetHatByIndex(_randomHatIndex.Value); // We don't want to trigger SetHatByIndex here if "_randomHat" is false
    }
    

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        // PlayerManager.HasInstance avant de desouscrire
        if (PlayerManager.HasInstance)
        {
            PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;
        }
    }

    private void Update()
    {
        if (_randomHat) return;
        if (Time.frameCount % 30 == 0) // only check every 30 frames
        {
            if (!_networkPlayer)
            {
                _networkPlayer = GetComponent<NetworkPlayer>();
            }

            if (_networkPlayer)
            {
                _playerIndexType = _networkPlayer.GetPlayerIndexType();
                if (PlayerManager.HasInstance) OnPlayerHatInfosChanged(PlayerManager.Instance.GetPlayerHatInfos());
            }
        }
    }

    private IEnumerator TrySubscribingToEvents()
    {
        while (!PlayerManager.HasInstance)
        {
            yield return null;
        }
        PlayerManager.Instance.OnPlayerHatInfosChanged += OnPlayerHatInfosChanged;
    }

    private void OnPlayerHatInfosChanged(List<PlayerHatInfo> hatInfos)
    {
        if (hatInfos.Count == 0)
        {
            DisableHats();
            return; 
        }
        foreach (var hatInfo in hatInfos)
        {
            if (hatInfo.PlayerIndexType == _playerIndexType)
            {
                SetHatByName(hatInfo.PlayerHatType.ToString());
            }
        }
    }


    private void SetHat(int prev, int next, bool asServer)
    {
        SetHatByIndex(next);
    }

    private void SetHatByIndex(int index)
    {
        if (_hats.IsNullOrEmpty()) { return; }

        DisableHats();

        _hats[index].gameObject.SetActive(true);
        //Logger.LogTrace("Setting hat by index, activating hat : " + _hats[index].name, Logger.LogType.Client, this);
    }

    private void SetHatByName(string hatName)
    {
        if (_hats.IsNullOrEmpty())
        {
            //Logger.LogWarning("No hats found in the hat container", Logger.LogType.Client, this);
            return;
        } 

        DisableHats();

        foreach (var hat in _hats)
        {
            if (hat.name == hatName)
            {
                hat.gameObject.SetActive(true);
                //Logger.LogTrace("Setting hat: " + hatName, Logger.LogType.Client, this);
                return;
            }
        }
    }

    private void DisableHats()
    {
        foreach (var hat in _hats)
        {
            hat.gameObject.SetActive(false);
            //Logger.LogTrace($"Disabling hat {hat.name}", Logger.LogType.Client, this);
        }
        //Logger.LogTrace("Disabling all hats", Logger.LogType.Client, this);
    }

}
