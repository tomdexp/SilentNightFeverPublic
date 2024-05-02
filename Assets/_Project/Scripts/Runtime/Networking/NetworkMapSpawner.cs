using _Project.Scripts.Runtime.Networking;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ProcGenInstanciator))]
public class NetworkMapSpawner : NetworkBehaviour
{
    [SerializeField] private ProcGenInstanciator _procGenInstanciator;
    [SerializeField] private bool _gameIsPlaying;

    private void OnDisable()
    {
        // Unbind to all events
        GameManager.Instance.IsGameStarted.OnChange -= OnGameStarted;
        _procGenInstanciator.OnMapGenerated -= OnMapGenerated;
        _procGenInstanciator.OnPrefabSpawned -= OnPrefabSpawned;
    }

    void Start()
    {
        _procGenInstanciator = GetComponent<ProcGenInstanciator>();
        if (IsServerStarted)
        {
            StartCoroutine(trySubscribingToGameStartedEvent());
        }
    }

    public IEnumerator trySubscribingToGameStartedEvent()
    {
        while (!GameManager.HasInstance)
        {
            yield return null;
        }
        GameManager.Instance.IsGameStarted.OnChange += OnGameStarted;
    }

    // When game start, generate map
    private void OnGameStarted(bool prev, bool next, bool asServer)
    {
        if (next == true && prev == false && _gameIsPlaying == false)
        {
            _gameIsPlaying = true;
            _procGenInstanciator.OnMapGenerated += OnMapGenerated;
            _procGenInstanciator.GenerateMap();
        }
    }

    // When game map is generated, spawn elements on it
    private void OnMapGenerated()
    {
        _procGenInstanciator.OnMapGenerated -= OnMapGenerated;
        _procGenInstanciator.OnPrefabSpawned += OnPrefabSpawned;

        _procGenInstanciator.SpawnAllPrefabs();

    }

    // When elements are spawned on the map, we place the players
    private void OnPrefabSpawned()
    {
        _procGenInstanciator.OnPrefabSpawned -= OnPrefabSpawned;
        TryPlacePlayers();
    }

    public void TryPlacePlayers()
    {
        if (!IsServerStarted)
        {
            PlacePlayersServerRpc();
        }
        else
        {
            PlacePlayers();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlacePlayersServerRpc()
    {
        PlacePlayers();
    }

    [ObserversRpc]
    private void PlacePlayers()
    {
        _procGenInstanciator.PlacePlayers();
    }
}

