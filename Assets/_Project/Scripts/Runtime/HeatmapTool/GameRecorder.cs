using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.UI;
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
    [ReadOnly] public ProcGenInstanciator _procGenInstanciator = null;

    private GameObject _playerA;
    private GameObject _playerB;
    private GameObject _playerC;
    private GameObject _playerD;

    private int _currentRoundIndex;

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
        GameManager.Instance.OnAnyRoundStarted -= StartRegisteringPlayerLocation;
        if (_procGenInstanciator != null)
        {
            _procGenInstanciator.OnMapGenerated -= RegisterLandmarksLocation;
        }
    }

    private IEnumerator TrySubscribingEvents()
    {
        while (_procGenInstanciator == null)
        {
            _procGenInstanciator = FindAnyObjectByType<ProcGenInstanciator>();
            yield return null;
        }
        _procGenInstanciator.OnMapGenerated += RegisterLandmarksLocation;
        GameManager.Instance.OnAnyRoundStarted += StartRegisteringPlayerLocation;
        GameManager.Instance.OnGameEnded += SaveGameInfosToJSON;
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


    private void StartRegisteringPlayerLocation(byte roundIndex)
    {
        _currentRoundIndex = roundIndex;

        if (!(_playerA && _playerB && _playerC && _playerD))
        {
            GameObject[] players = PlayerManager.Instance.GetNetworkPlayers().Select(player => player.gameObject).ToArray();

            if (players.Count() != 4)
            {
                Debug.LogWarning("Couldn't find all players, can't register their location");
                return;
            }

            _playerA = players[0];
            _playerB = players[1];
            _playerC = players[2];
            _playerD = players[3];

        }

        StartCoroutine(RegisterPlayerLocation(roundIndex));
    }

    public IEnumerator RegisterPlayerLocation(byte roundIndex)
    {
        float registerInterval = 1.0f;
        float timeSinceRoundStarted = 0.0f;

        _gameInfos.RoundInfo.Add(new RoundInfos());

        _gameInfos.RoundInfo[roundIndex - 1].timeInterval = registerInterval;
        _gameInfos.RoundInfo[roundIndex - 1].PlayerInfos = new List<PlayerInfos>();

        while (roundIndex == _currentRoundIndex)
        {
            PlayerInfos PInfos = new PlayerInfos(_playerA.transform.localPosition, _playerB.transform.localPosition, _playerC.transform.localPosition, _playerD.transform.localPosition);
            _gameInfos.RoundInfo[roundIndex - 1].PlayerInfos.Add(PInfos);

            yield return new WaitForSeconds(registerInterval);
            timeSinceRoundStarted += registerInterval;
        }
    }

    public void SaveGameInfosToJSON(PlayerTeamType winningTeam)
    {
        string json = JsonUtility.ToJson(_gameInfos);
        DateTime dt = DateTime.Now;
        File.WriteAllText(Application.dataPath + "/GameInfos" + dt.ToString("HHmmss") + ".json", json);
    }
}
