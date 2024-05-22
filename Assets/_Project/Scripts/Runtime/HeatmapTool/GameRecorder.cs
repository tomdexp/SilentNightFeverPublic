using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

public class GameRecorder : NetworkBehaviour
{
    [ReadOnly] public ProcGenInstanciator _procGenInstanciator = null;
    
    public event Action OnCloudSaveBegin;
    public event Action OnCloudSaveEnd;

    private GameObject _playerA;
    private GameObject _playerB;
    private GameObject _playerC;
    private GameObject _playerD;

    private int _currentRoundIndex;

    private GameInfos _gameInfos;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _gameInfos = new GameInfos();
        _gameInfos.LandmarksLocation = new List<LandmarksInfos>();
        _gameInfos.RoundInfo = new List<RoundInfos>();
        StartCoroutine(TrySubscribingProcGenEvents());
        StartCoroutine(TrySubscribingToGameManager());
    }


    private void OnDestroy()
    {
        if (GameManager.HasInstance) GameManager.Instance.OnAnyRoundStarted -= StartRegisteringPlayerLocation;
        if (_procGenInstanciator != null)
        {
            _procGenInstanciator.OnMapGenerated -= RegisterLandmarksLocation;
        }
    }

    private IEnumerator TrySubscribingProcGenEvents()
    {
        while (!_procGenInstanciator)
        {
            _procGenInstanciator = FindAnyObjectByType<ProcGenInstanciator>();
            yield return null;
        }
        _procGenInstanciator.OnMapGenerated += RegisterLandmarksLocation;
    }
    
    private IEnumerator TrySubscribingToGameManager()
    {
        while (!GameManager.HasInstance)
        {
            yield return null;
        }
        GameManager.Instance.OnAnyRoundStarted += StartRegisteringPlayerLocation;
        GameManager.Instance.OnGameEnded += SaveGameInfosToJSON;
    }


    private void RegisterLandmarksLocation()
    {
        _procGenInstanciator.OnMapGenerated -= RegisterLandmarksLocation;

        for (int i = 0; i < _procGenInstanciator._spawnedLandmarks.Count; i++)
        {
            // TODO : For now, we only spawn one type of prefab, this will need to be changed when spawn different prefabs
            _gameInfos.LandmarksLocation.Add(new LandmarksInfos(_procGenInstanciator._spawnedLandmarks[i].name, _procGenInstanciator._spawnedLandmarks[i].transform.position));
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
                Logger.LogWarning("Couldn't find all players, can't register their location", Logger.LogType.Server, this);
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
        float registerInterval = 0.3f;
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
        string path = Application.dataPath + "/GameInfos" + dt.ToString("HHmmss") + ".json";
        string name = "GameInfos" + dt.ToString("HHmmss") + ".json";
        Logger.LogInfo($"GameInfos file {name} saved to {path}", Logger.LogType.Server, this);
        SaveJSONToCloud(json, name);
    }

    [Button(ButtonStyle.FoldoutButton)]
    public async void SaveJSONToCloud(string fileJson, string fileName)
    {
        OnCloudSaveBegin?.Invoke();
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        int bytes = Encoding.UTF8.GetByteCount(fileJson);
        Logger.LogInfo($"Begin to save file to cloud of size {bytes} byte", Logger.LogType.Server, this);
        Logger.LogTrace(fileJson);
        var fileBytes = Encoding.UTF8.GetBytes(fileJson);
        var arguments = new Dictionary<string, object>
        {
            { "FileName", fileName },
            { "FileJson", fileBytes}
            
        };
        var response = await CloudCodeService.Instance.CallEndpointAsync<SaveGameInfoJSONResponse>("SaveGameInfoJSON", arguments);
        Debug.Log(response.response);
        stopwatch.Stop();
        Logger.LogInfo($"File saved to cloud in {stopwatch.ElapsedMilliseconds} ms", Logger.LogType.Server, this);
        OnCloudSaveEnd?.Invoke();
    }
    
    [Button(ButtonStyle.FoldoutButton)]
    public GameInfos ReconstructJsonFromBytes(string fileBytes)
    {
        // remove the " at the start and the end
        fileBytes = fileBytes.Substring(1, fileBytes.Length - 2);
        string json = Encoding.UTF8.GetString(Convert.FromBase64String(fileBytes));
        Logger.LogTrace(json);
        GameInfos gameInfos = JsonUtility.FromJson<GameInfos>(json);
        return gameInfos;
    }

    public class SaveGameInfoJSONResponse
    {
        public string message;
        public string response;
        public bool success;
    }
}
