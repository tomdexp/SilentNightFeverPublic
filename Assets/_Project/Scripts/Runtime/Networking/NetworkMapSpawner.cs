using _Project.Scripts.Runtime.Networking;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;


[RequireComponent(typeof(ProcGenInstanciator))]
public class NetworkMapSpawner : NetworkBehaviour
{
    private ProcGenInstanciator _procGenInstanciator;
    [SerializeField] private bool _gameIsPlaying;

    void Start()
    {
        _procGenInstanciator = GetComponent<ProcGenInstanciator>();
        if (IsServerStarted)
        {
            StartCoroutine(TrySubscribingToGameStartedEvent());
            StartCoroutine(TrySubscribingToAllPlayerSpawnedLocallyEvent());
        }
    }

    private void OnDestroy()
    {
        if (GameManager.HasInstance) GameManager.Instance.IsGameStarted.OnChange -= OnGameStarted;
        if (GameManager.HasInstance) GameManager.Instance.OnAnyRoundStarted -= OnRoundStart;
        if (PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayerSpawnedLocally -= OnAllPlayerSpawnedLocally;
        _procGenInstanciator.OnMapGenerated -= OnMapGenerated;
        _procGenInstanciator.OnPrefabSpawned -= OnPrefabSpawned;
    }

    private IEnumerator TrySubscribingToGameStartedEvent()
    {
        while (!GameManager.HasInstance)
        {
            yield return null;
        }
        GameManager.Instance.IsGameStarted.OnChange += OnGameStarted;
        GameManager.Instance.OnAnyRoundStarted += OnRoundStart;
    }
    

    private IEnumerator TrySubscribingToAllPlayerSpawnedLocallyEvent()
    {
        while (!PlayerManager.HasInstance)
        {
            yield return null;
        }
        PlayerManager.Instance.OnAllPlayerSpawnedLocally += OnAllPlayerSpawnedLocally;
    }

    private void OnAllPlayerSpawnedLocally()
    {
        TryPlacePlayers();
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
    
    private void OnRoundStart(byte roundIndex)
    {
        if (roundIndex == 1) return; // The teleportation on the first round is handled by the OnPlayerReadyLocally event
        Logger.LogDebug("New round started, generating new player points", Logger.LogType.Server, this);
        _procGenInstanciator.GenerateNewPlayerPoints();
        TryPlacePlayers();
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

    private void PlacePlayers()
    {
        _procGenInstanciator.PlacePlayers();
    }
}

