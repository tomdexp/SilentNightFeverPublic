using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatSetter : NetworkBehaviour
{
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
        _randomHatIndex.OnChange += SetHat; ;
    }

    private void OnDisable()
    {
        _randomHatIndex.OnChange -= SetHat;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _randomHat = true;
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

    private void SetHat(int prev, int next, bool asServer)
    {
        SetHatByIndex(next);
    }

    private void SetHatByIndex(int index)
    {
        if (_hats.IsNullOrEmpty()) { return; }

        _hats[index].gameObject.SetActive(true);
    }

}
