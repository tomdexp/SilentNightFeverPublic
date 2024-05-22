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
using Sirenix.Utilities;
using Random = UnityEngine.Random;

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
    [SerializeField] public List<SpawnableNetworkObject> _landmarksPrefabList;
    [HideInInspector] public List<Vector2> _landmarksPoints;
    public List<NetworkObject> _spawnedLandmarks;

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

    private List<List<Vector2>> _alreadySpawnedPoints = new();
    private List<float> _alreadySpawnedPointsRadius = new();

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
        _teamAPoints = GeneratePoints(_teamAParameters, true, true, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true, true, true);
        _landmarksPoints = GeneratePoints(_landmarksParameters, true, true, true);

        // Decoration
        _FernPoints = GeneratePoints(_FernParameters, false, false, true);
        _TreePoints = GeneratePoints(_TreeParameters, false, true, true);
        _testLandmarkPoints = GeneratePoints(_testLandmarkParameters, false, true, true);

        _testCubePoints = GeneratePoints(_testCubeParameters, false, false, true);
        _testDiscPoints = GeneratePoints(_testDiscParameters, false, false, true);

        _CrowdPoints = GeneratePoints(_CrowdParameters, false, false, true);


        _readyToSpawnPrefabs = true;
        OnMapGenerated?.Invoke();
    }

    private void GenerateTerrain()
    {
        // Generate Map ground
        NetworkObject ground = Instantiate(_ground, new Vector3(_regionSize.x / 2, -1f, _regionSize.y / 2), Quaternion.identity);
        ground.transform.localScale = new Vector3(_regionSize.x / 10 + _regionSize.x / 25, 1, _regionSize.y / 10 + _regionSize.x / 25);
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

    /// <summary>
    /// Generate points for object to spawn
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="forceExactNumber"> If we want the exact same number of elements spawn than specified in parameters</param>
    /// <param name="addToAlreadySpawnedPoints"> If we want other objects to consider the presence of these object to be taken into consideration when spawning them</param>
    /// <param name="considerAlreadySpawnedObjects">If we want these object to not spawn on other objects (where addToAlreadySpawnedPoints was used for them) </param>
    /// <returns></returns>
    private List<Vector2> GeneratePoints(ProcGenParameters parameters, bool forceExactNumber, bool addToAlreadySpawnedPoints, bool considerAlreadySpawnedObjects)
    {
        Vector2 newRegionSize = _regionSize;
        newRegionSize.x *= (100 - parameters._edgeDistance) / 100;
        newRegionSize.y *= (100 - parameters._edgeDistance) / 100;

        List<Vector2> points;

        if (considerAlreadySpawnedObjects && !_alreadySpawnedPoints.IsNullOrEmpty())
        {
            // We create a copy of AlreadySpawnedPoints but the point are centered arround the region size we are trying to spawn our new points. (Hard to explain sorry)
            List<List<Vector2>> tmpAlreadySpawnedPoints = new();
            List<float> tmpAlreadySpawnedPointsRadius = new();


            for (int i = 0; i < _alreadySpawnedPoints.Count; i++)
            {
                tmpAlreadySpawnedPoints.Add(new List<Vector2>());
                tmpAlreadySpawnedPointsRadius.Add(_alreadySpawnedPointsRadius[i] + parameters._distanceWithOtherObjects);
                for (int j = 0; j < _alreadySpawnedPoints[i].Count; j++)
                {
                    Vector2 tmpPoint = _alreadySpawnedPoints[i][j];
                    tmpPoint.x -= parameters._edgeDistance;
                    tmpPoint.y -= parameters._edgeDistance;
                    tmpAlreadySpawnedPoints[i].Add(tmpPoint);
                }
            }

            points = PoissonDiscSampling.GenerateExactNumberOfPoints(parameters._minDistance, parameters._maxDistance, newRegionSize, parameters._numOfPoints, tmpAlreadySpawnedPoints, tmpAlreadySpawnedPointsRadius, 720, 3000);
        }
        else
        {
            points = PoissonDiscSampling.GenerateExactNumberOfPoints(parameters._minDistance, parameters._maxDistance, newRegionSize, parameters._numOfPoints, 720, 3000);
        }

        if (points.Count < parameters._numOfPoints && forceExactNumber)
        {
            Debug.Log("Not enougth points, something went wrong? \n Number of spawned objects : " + points.Count);
        }


        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            point.x += (parameters._edgeDistance / 100 * _regionSize.x) / 2;
            point.y += (parameters._edgeDistance / 100 * _regionSize.y) / 2;
            points[i] = point;
        }


        if (addToAlreadySpawnedPoints)
        {
            _alreadySpawnedPoints.Add(points);
            _alreadySpawnedPointsRadius.Add(parameters._distanceWithOtherObjects);
        }

        return points;
    }

    private void SpawnPrefabs(List<Vector2> pointsLocation, NetworkObject prefab)
    {
        for (int i = 0; i < pointsLocation.Count; i++)
        {
            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[i].x, 0, pointsLocation[i].y), Quaternion.identity);
            pref.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));
            InstanceFinder.ServerManager.Spawn(pref);
        }
    }

    // Tries to spawns each prefabs respecting the min and max amount specified
    private void SpawnPrefabs(List<Vector2> pointsLocation, List<SpawnableNetworkObject> SNOList)
    {
        List<SpawnableNetworkObject> minList = new();

        // We create a list with min*element there is in our SNOList
        foreach (var SNO in SNOList)
        {
            for (int i = 0; i < SNO.Min; i++)
            {
                minList.Add(SNO);
            }
        }

        int spawnIndex = 0;

        // For each element of our minList, we spawn its prefab the it and tell the SNOList we spawned it
        while (!minList.IsNullOrEmpty() && spawnIndex < pointsLocation.Count)
        {
            int random = Random.Range(0, minList.Count-1);
            NetworkObject prefab = minList[random].Object;
            minList.RemoveAt(random);

            foreach (var SNO in SNOList)
            {
                if (SNO.Object == prefab)
                {
                    SNO.Min -= 1;
                    SNO.Max -= 1;
                    if (SNO.Max == 0)
                    {
                        SNOList.Remove(SNO);
                    }
                    break;
                }
            }

            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[spawnIndex].x, 0, pointsLocation[spawnIndex].y), Quaternion.identity);
            _spawnedLandmarks.Add(pref);
            pref.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
            InstanceFinder.ServerManager.Spawn(pref);

            spawnIndex++;
        }

        // Then we spawn the remaining elements from the SNOList (considering the max attributes)
        while (!SNOList.IsNullOrEmpty() && spawnIndex < pointsLocation.Count)
        {
            int random = Random.Range(0, SNOList.Count-1);
            NetworkObject prefab = SNOList[random].Object;
            SNOList[random].Max -= 1;

            if (SNOList[random].Max == 0)
            {
                SNOList.RemoveAt(random);
            }

            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[spawnIndex].x, 0, pointsLocation[spawnIndex].y), Quaternion.identity);
            _spawnedLandmarks.Add(pref);
            pref.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
            InstanceFinder.ServerManager.Spawn(pref);

            spawnIndex++;
        }

        if (spawnIndex < pointsLocation.Count)
        {
            Debug.LogWarning("Max number of prefab too low, spawned less landmarks than anticipated, spawned : " + spawnIndex + " instead of : " + pointsLocation.Count);
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
        SpawnPrefabs(_landmarksPoints, _landmarksPrefabList);
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
        if (_landmarksPrefabList.IsNullOrEmpty()) Logger.LogError("Landmark prefabs are missing");
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
        Logger.LogDebug("Placing players...", Logger.LogType.Server, this);
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
        _teamAPoints = GeneratePoints(_teamAParameters, true, false, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true, false, true);
    }
}

[System.Serializable]
public class SpawnableNetworkObject
{
    [HideLabel] public NetworkObject Object;

    [HorizontalGroup("MinMax", Width = 0.4f), MinValue(0)]
    public int Min;

    [HorizontalGroup("MinMax", Width = 0.4f), MinValue("@Min"), PropertySpace(SpaceBefore = 0, SpaceAfter = 15)]
    public int Max;

}