using FishNet;
using FishNet.Object;
using Mono.CSharp;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using Unity.Mathematics;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

public class ProcGenInstanciator : MonoBehaviour
{
    public bool _patxiMode = false;

    [SerializeField] private Vector2 _regionSize;

    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _ground;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _invisibleWall;

    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _playerPrefab;

    [Title("    Team A")]
    [SerializeField] private ProcGenParameters _teamAParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _teamAPrefab;
    private List<Vector2> _teamAPoints;

    [Title("    Team B")]
    [SerializeField] private ProcGenParameters _teamBParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _teamBPrefab;
    private List<Vector2> _teamBPoints;

    [Title("    Landmarks")]
    [SerializeField] private ProcGenParameters _landmarksParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _landmarksPrefab;
    private List<Vector2> _landmarksPoints;

    [Title("    Crowd")]
    [SerializeField] private ProcGenParameters _CrowdParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _CrowdPrefab;
    private List<Vector2> _CrowdPoints;


    [Title("    Environment")]
    [HideIf("@_patxiMode == true"), SerializeField] private ProcGenParameters _FernParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _FernPrefab;
    private List<Vector2> _FernPoints;

    [HideIf("@_patxiMode == true"), SerializeField] private ProcGenParameters _TreeParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _TreePrefab;
    private List<Vector2> _TreePoints;

    [HideIf("@_patxiMode == true"), SerializeField] private ProcGenParameters _testLandmarkParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _testLandmarkPrefab;
    private List<Vector2> _testLandmarkPoints;

    [HideIf("@_patxiMode == true"), SerializeField] private ProcGenParameters _testCubeParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _testCubePrefab;
    private List<Vector2> _testCubePoints;

    [HideIf("@_patxiMode == true"), SerializeField] private ProcGenParameters _testDiscParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _testDiscPrefab;
    private List<Vector2> _testDiscPoints;

    private bool _readyToSpawnPrefabs = false;

    // Events
    public event Action OnMapGenerated;
    public event Action OnPrefabSpawned;

    private void Awake()
    {
        VerifyPrefabSetup();
    }


    [Button]
    public void GenerateMap()
    {
        GenerateTerrain();

        _teamAPoints = GeneratePoints(_teamAParameters, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true);
        _landmarksPoints = GeneratePoints(_landmarksParameters, true);

        // Decoration
        _FernPoints = GeneratePoints(_FernParameters, false);
        _TreePoints = GeneratePoints(_TreeParameters, false);
        _testLandmarkPoints = GeneratePoints(_testLandmarkParameters, false);

        _testCubePoints = GeneratePoints(_testCubeParameters, false);
        _testDiscPoints = GeneratePoints(_testDiscParameters, false);

        _CrowdPoints = GeneratePoints(_CrowdParameters, false);


        _readyToSpawnPrefabs = true;
        OnMapGenerated?.Invoke();
    }

    private void GenerateTerrain()
    {
        // Generate Map ground
        NetworkObject ground = Instantiate(_ground, new Vector3(_regionSize.x / 2, -1f, _regionSize.y / 2), Quaternion.identity);
        ground.transform.localScale = new Vector3(_regionSize.x / 10 + _regionSize.x / 100, 1, _regionSize.y / 10 + _regionSize.y / 100);
        InstanceFinder.ServerManager.Spawn(ground);

        // Generate Map invisible wall boundings
        // North
        NetworkObject wallNorth = Instantiate(_invisibleWall, new Vector3(_regionSize.x / 2, 1, _regionSize.y + 1), Quaternion.identity);
        wallNorth.transform.localScale = new Vector3(_regionSize.x + 1, 4, 1);
        InstanceFinder.ServerManager.Spawn(wallNorth);

        // South
        NetworkObject wallSouth = Instantiate(_invisibleWall, new Vector3(_regionSize.x / 2, 1, -1), Quaternion.identity);
        wallSouth.transform.localScale = new Vector3(_regionSize.x + 1, 4, 1);
        InstanceFinder.ServerManager.Spawn(wallSouth);

        // East
        NetworkObject wallEast = Instantiate(_invisibleWall, new Vector3(_regionSize.y + 1, 1, _regionSize.y / 2), Quaternion.identity);
        wallEast.transform.localScale = new Vector3(1, 4, _regionSize.y + 1);
        InstanceFinder.ServerManager.Spawn(wallEast);

        // West
        NetworkObject wallWest = Instantiate(_invisibleWall, new Vector3(-1, 1, _regionSize.y / 2), Quaternion.identity);
        wallWest.transform.localScale = new Vector3(1, 4, _regionSize.y + 1);
        InstanceFinder.ServerManager.Spawn(wallWest);
    }

    private List<Vector2> GeneratePoints(ProcGenParameters parameters, bool forceExactNumber)
    {
        Vector2 newRegionSize = _regionSize;
        newRegionSize.x *= (100 - parameters._edgeDistance) / 100;
        newRegionSize.y *= (100 - parameters._edgeDistance) / 100;

        List<Vector2> points = PoissonDiscSampling.GenerateExactNumberOfPoints(parameters._minDistance, parameters._maxDistance, newRegionSize, parameters._numOfPoints, 720, 10000);
        if (points.Count < parameters._numOfPoints && forceExactNumber)
        {
            Debug.Log("Not enougth points, something went wrong? \n Number of spawned objects : " + points.Count);
        }

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            point.x += parameters._edgeDistance;
            point.y += parameters._edgeDistance;
            points[i] = point;
        }

        return points;
    }

    private void SpawnPrefabs(List<Vector2> pointsLocation, NetworkObject prefab)
    {
        for (int i = 0; i < pointsLocation.Count; i++)
        {
            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[i].x, 0, pointsLocation[i].y), Quaternion.identity);
            pref.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0,360),0));
            InstanceFinder.ServerManager.Spawn(pref);
        }
    }

    [Button, HideIf("@_readyToSpawnPrefabs == false")]
    public void SpawnAllPrefabs()
    {
        if (_readyToSpawnPrefabs == false)
        {
            Debug.LogError("You must generate spawn points before trying to spawn them");
            return;
        }

        SpawnPrefabs(_teamAPoints, _teamAPrefab);
        SpawnPrefabs(_teamBPoints, _teamBPrefab);
        SpawnPrefabs(_landmarksPoints, _landmarksPrefab);
        SpawnPrefabs(_testLandmarkPoints, _testLandmarkPrefab);
        SpawnPrefabs(_FernPoints, _FernPrefab);
        SpawnPrefabs(_TreePoints, _TreePrefab);
        SpawnPrefabs(_testCubePoints, _testCubePrefab);
        SpawnPrefabs(_testDiscPoints, _testDiscPrefab);
        SpawnPrefabs(_CrowdPoints, _CrowdPrefab);
        OnPrefabSpawned?.Invoke();
    }

    private void VerifyPrefabSetup()
    {
        if (!_ground) Logger.LogError("Ground prefab is missing");
        if (!_invisibleWall) Logger.LogError("Invisible wall prefab is missing");
        if (!_playerPrefab) Logger.LogError("Player prefab is missing");
        if (!_teamAPrefab) Logger.LogError("Team A prefab is missing");
        if (!_teamBPrefab) Logger.LogError("Team B prefab is missing");
        if (!_landmarksPrefab) Logger.LogError("Landmarks prefab is missing");
        if (!_FernPrefab) Logger.LogError("Fern prefab is missing");
        if (!_TreePrefab) Logger.LogError("Tree prefab is missing");
        if (!_testLandmarkPrefab) Logger.LogError("Test Landmark prefab is missing");
        if (!_testCubePrefab) Logger.LogError("Test Cube prefab is missing");
        if (!_testDiscPrefab) Logger.LogError("Test Disc prefab is missing");
        if (!_CrowdPrefab) Logger.LogError("Crowd prefab is missing");
    }

    [Button]
    public void PlacePlayers()
    {
        GameObject[] playersTeamA = PlayerManager.Instance.GetNetworkPlayers(PlayerTeamType.A).Select(player => player.gameObject).ToArray();
        GameObject[] playersTeamB = PlayerManager.Instance.GetNetworkPlayers(PlayerTeamType.B).Select(player => player.gameObject).ToArray();

        for (int i = 0; i < playersTeamA.Length; i++)
        {
            playersTeamA[i].GetComponent<PlayerController>().Teleport(new Vector3(_teamAPoints[i].x, 0, _teamAPoints[i].y));
        }
        for (int i = 0; i < playersTeamB.Length; i++)
        {
            playersTeamB[i].GetComponent<PlayerController>().Teleport(new Vector3(_teamBPoints[i].x, 0, _teamBPoints[i].y));
        }
    }

    public void GenerateNewPlayerPoints()
    {
        _teamAPoints = GeneratePoints(_teamAParameters, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true);
    }
}
