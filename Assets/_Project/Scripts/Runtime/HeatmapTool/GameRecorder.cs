using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameRecorder : NetworkBehaviour
{
    private ProcGenInstanciator _procGenInstanciator = null;

    private GameObject _playerA;
    private GameObject _playerB;
    private GameObject _playerC;
    private GameObject _playerD;

    private GameInfos _gameInfos;

    private void Start()
    {
        _gameInfos = new GameInfos();
        _gameInfos.LandmarksLocation = new List<LandmarksInfos>();
        _gameInfos.RoundInfo = new List<RoundInfos>();
        if (IsServerStarted)
        {
            StartCoroutine(TrySubscribingEvents());
        }
    }


    private void OnDestroy()
    {
        if (PlayerManager.HasInstance)
        {
            PlayerManager.Instance.OnAllPlayerSpawnedLocally -= StartRegisteringPlayerLocation;
        }
        if (_procGenInstanciator != null)
        {
            _procGenInstanciator.OnMapGenerated -= RegisterLandmarksLocation;
        }
    }

    private IEnumerator TrySubscribingEvents()
    {
        while (!PlayerManager.HasInstance)
        {
            yield return null;
        }
        PlayerManager.Instance.OnAllPlayerSpawnedLocally += StartRegisteringPlayerLocation;

        while (_procGenInstanciator == null)
        {
            _procGenInstanciator = FindAnyObjectByType<ProcGenInstanciator>();
            yield return new WaitForSeconds(1f);
        }
        _procGenInstanciator.OnMapGenerated += RegisterLandmarksLocation;
    }


    private void RegisterLandmarksLocation()
    {
        _procGenInstanciator.OnMapGenerated -= RegisterLandmarksLocation;

        for (int i = 0; i < _procGenInstanciator._landmarksPoints.Count; i++)
        {
            // TODO : For now, we only spawn one type of prefab, this will need to be changed when spawn different prefabs
            _gameInfos.LandmarksLocation.Add(new LandmarksInfos(_procGenInstanciator._landmarksPrefab.name, _procGenInstanciator._landmarksPoints[i]));
        }
        
    }

    private void StartRegisteringPlayerLocation()
    {
        PlayerManager.Instance.OnAllPlayerSpawnedLocally -= StartRegisteringPlayerLocation;

        GameObject[] players = PlayerManager.Instance.GetNetworkPlayers().Select(player => player.gameObject).ToArray();
     
        if (players.Count() != 4)
        {
            Debug.LogWarning("Couldn't find all players, can't register their location");
            return;
        }

        InvokeRepeating(nameof(RegisterPlayerLocation), 0, 1.0f);
    }

    private void RegisterPlayerLocation()
    {
        
    }

    [Button]
    public void SaveGameInfosToJSON()
    {
        string json = JsonUtility.ToJson(_gameInfos);
        File.WriteAllText(Application.dataPath + "/GameInfos.json", json);
    }
}
