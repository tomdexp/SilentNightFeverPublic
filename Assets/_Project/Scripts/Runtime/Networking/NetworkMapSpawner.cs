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

    public override void OnStartServer()
    {
        base.OnStartServer();
        _procGenInstanciator = GetComponent<ProcGenInstanciator>();
        StartCoroutine(TrySubscribingToGameStartedEvent());
        StartCoroutine(TrySubscribingToAllPlayerSpawnedLocallyEvent());
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (GameManager.HasInstance) GameManager.Instance.OnAnyRoundStarted -= OnRoundStart;
        if (PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayerSpawnedLocally -= OnAllPlayerSpawnedLocally;
    }

    private IEnumerator TrySubscribingToGameStartedEvent()
    {
        while (!GameManager.HasInstance)
        {
            yield return null;
        }
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
    
    private void OnRoundStart(byte roundIndex)
    {
        if (roundIndex == 1) return; // The teleportation on the first round is handled by the OnPlayerReadyLocally event
        Logger.LogDebug("New round started, generating new player points", Logger.LogType.Server, this);
        _procGenInstanciator.GenerateNewPlayerPoints();
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

    private void PlacePlayers()
    {
        _procGenInstanciator.PlacePlayers();
    }
}

