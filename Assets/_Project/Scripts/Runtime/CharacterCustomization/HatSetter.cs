using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatSetter : NetworkBehaviour
{
    [SerializeField] private PlayerIndexType _playerIndexType = 0;
    [Space]

    [SerializeField] private bool _randomHat = true;
    [SerializeField, Required] private GameObject _hatContainer = null;
    private MeshRenderer[] _hats;

    private readonly SyncVar<int> _randomHatIndex = new SyncVar<int>();

    private void Awake()
    {
        if (_hatContainer == null)
        {
            Debug.LogWarning("HatSetter is missing a reference to the HatContainer Gameobject !");
            return;
        }

        _hats = _hatContainer.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
    }

    private void OnEnable()
    {
        _randomHatIndex.OnChange += SetHat;

        if (_randomHat == false)
        {
            if (TryGetComponent(out NetworkPlayer NP))
            {
                _playerIndexType = NP.GetPlayerIndexType();
            };

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

        if (_randomHat == true)
        {
            _randomHatIndex.Value = Random.Range(0, _hats.Length);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        SetHatByIndex(_randomHatIndex.Value);
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


    private IEnumerator TrySubscribingToEvents()
    {
        while (!PlayerManager.HasInstance)
        {
            yield return null;
        }
        PlayerManager.Instance.OnPlayerHatInfosChanged += OnPlayerHatInfosChanged;
    }

    private void OnPlayerHatInfosChanged(List<_Project.Scripts.Runtime.Player.PlayerHatInfo> hatInfos)
    {
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
    }

    private void SetHatByName(string hatName)
    {
        if (_hats.IsNullOrEmpty()) { return; }

        DisableHats();

        foreach (var hat in _hats)
        {
            if (hat.name == hatName)
            {
                hat.gameObject.SetActive(true);
                return;
            }
        }
    }

    private void DisableHats()
    {
        foreach (var hat in _hats)
        {
            hat.gameObject.SetActive(false);
        }
    }

}
