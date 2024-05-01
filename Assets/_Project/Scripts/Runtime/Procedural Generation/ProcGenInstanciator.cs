using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProcGenInstanciator : MonoBehaviour
{
    public bool _patxiMode = false;

    [SerializeField] private Vector2 _regionSize;

    [HideIf("@_patxiMode == true"), SerializeField] private GameObject _ground;
    [HideIf("@_patxiMode == true"), SerializeField] private GameObject _invisibleWall;

    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _playerPrefab;

    [Title("    Team A")]
    [SerializeField] private ProcGenParameters _teamAParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private GameObject _teamAPrefab;
    private List<Vector2> _teamAPoints;

    [Title("    Team B")]
    [SerializeField] private ProcGenParameters _teamBParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private GameObject _teamBPrefab;
    private List<Vector2> _teamBPoints;

    [Title("    Landmarks")]
    [SerializeField] private ProcGenParameters _landmarksParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private GameObject _landmarksPrefab;
    private List<Vector2> _landmarksPoints;

    [Title("    Crowd")]
    [SerializeField] private ProcGenParameters _CrowdParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private GameObject _CrowdPrefab;
    private List<Vector2> _CrowdPoints;

    private bool _readyToSpawnPrefabs = false;

    // Events
    public event Action OnMapGenerated;
    public event Action OnPrefabSpawned;


    [Button]
    private void GenerateMap()
    {
        GenerateTerrain();

        _teamAPoints = GeneratePoints(_teamAParameters, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true);
        _landmarksPoints = GeneratePoints(_landmarksParameters, true);
        _CrowdPoints = GeneratePoints(_CrowdParameters, false);

        _readyToSpawnPrefabs = true;
        OnMapGenerated?.Invoke();
    }

    private void GenerateTerrain()
    {
        // Generate Map ground
        GameObject ground = Instantiate(_ground, new Vector3(_regionSize.x / 2, -2, _regionSize.y / 2), Quaternion.identity);
        ground.transform.localScale = new Vector3(_regionSize.x / 10 + _regionSize.x / 100, 1, _regionSize.y / 10 + _regionSize.y / 100);

        // Generate Map invisible wall boundings
        // North
        GameObject wallNorth = Instantiate(_invisibleWall, new Vector3(_regionSize.x / 2, 1, _regionSize.y + 1), Quaternion.identity);
        wallNorth.transform.localScale = new Vector3(_regionSize.x + 1, 4, 1);
        // South
        GameObject wallSouth = Instantiate(_invisibleWall, new Vector3(_regionSize.x / 2, 1, -1), Quaternion.identity);
        wallSouth.transform.localScale = new Vector3(_regionSize.x + 1, 4, 1);
        // East
        GameObject wallEast = Instantiate(_invisibleWall, new Vector3(_regionSize.y + 1, 1, _regionSize.y / 2), Quaternion.identity);
        wallEast.transform.localScale = new Vector3(1, 4, _regionSize.y + 1);
        // West
        GameObject wallWest = Instantiate(_invisibleWall, new Vector3(-1, 1, _regionSize.y / 2), Quaternion.identity);
        wallWest.transform.localScale = new Vector3(1, 4, _regionSize.y + 1);
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

    private void SpawnPrefabs(List<Vector2> pointsLocation, GameObject prefab)
    {
        for (int i = 0; i < pointsLocation.Count; i++)
        {
            Instantiate(prefab, new Vector3(pointsLocation[i].x, 0, pointsLocation[i].y), Quaternion.identity);
        }
    }

    [Button, HideIf("@_readyToSpawnPrefabs == false")]
    private void SpawnAllPrefabs()
    {
        if (_readyToSpawnPrefabs == false)
        {
            Debug.LogError("You must generate spawn points before trying to spawn them");
            return;
        }

        SpawnPrefabs(_teamAPoints, _teamAPrefab);
        SpawnPrefabs(_teamBPoints, _teamBPrefab);
        SpawnPrefabs(_landmarksPoints, _landmarksPrefab);
        SpawnPrefabs(_CrowdPoints, _CrowdPrefab);
        OnPrefabSpawned?.Invoke();
    }

    [Button]
    private void PlacePlayers()
    {
        GameObject[] players = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name == "PlayerPrefab(Clone)").ToArray();

        for (int i = 0; i < players.Count(); i++)
        {
            List<Vector2> teampoint;
            if (i < 2)
            {
                teampoint = _teamAPoints;
            } else
            {
                teampoint = _teamBPoints;
            }
            players[i].transform.position = new Vector3(teampoint[i%2].x, 0, teampoint[i%2].y);
        }
    }

}
