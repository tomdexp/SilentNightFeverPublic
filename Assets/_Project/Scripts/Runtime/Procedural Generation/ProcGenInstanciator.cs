using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenInstanciator : MonoBehaviour
{
    [SerializeField] private ProcGenVisualizer _procGenVisualizer;
    [SerializeField] private Vector2 _regionSize;
    [SerializeField] private GameObject _ground;

    [Title("    Team A")]
    [SerializeField] private ProcGenParameters _teamAParameters;
    [SerializeField] private GameObject _teamAPrefab;
    private List<Vector2> _teamAPoints;

    [Title("    Team B")]
    [SerializeField] private ProcGenParameters _teamBParameters;
    [SerializeField] private GameObject _teamBPrefab;
    private List<Vector2> _teamBPoints;

    [Title("    Landmarks")]
    [SerializeField] private ProcGenParameters _landmarksParameters;
    [SerializeField] private GameObject _landmarksPrefab;
    private List<Vector2> _landmarksPoints;

    [Title("    Crowd")]
    [SerializeField] private ProcGenParameters _CrowdParameters;
    [SerializeField] private GameObject _CrowdPrefab;
    private List<Vector2> _CrowdPoints;

    private bool _readyToSpawnPrefabs = false;

    // Events
    public event Action OnMapGenerated;
    public event Action OnPrefabSpawned;

    [Button]
    private void GenerateMap()
    {
        // Generate Map ground
        GameObject ground = Instantiate(_ground, new Vector3(_regionSize.x / 2, -2, _regionSize.y / 2), Quaternion.identity);
        ground.transform.localScale = new Vector3(_regionSize.x / 10 + _regionSize.x / 100, 1, _regionSize.y / 10 + _regionSize.y / 100) ;

        _teamAPoints = GeneratePoints(_teamAParameters, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true);
        _landmarksPoints = GeneratePoints(_landmarksParameters, true);
        _CrowdPoints = GeneratePoints(_CrowdParameters, false);

        _readyToSpawnPrefabs = true;
        OnMapGenerated?.Invoke();
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


}
