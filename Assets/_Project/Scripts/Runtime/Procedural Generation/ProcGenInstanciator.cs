using FishNet.Object;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenInstanciator : MonoBehaviour
{

    [SerializeField] private ProcGenVisualizer _procGenVisualizer;
    [SerializeField] private Vector2 _regionSize; 

    [Title("    Team A")]
    [SerializeField] private ProcGenParameters _teamAParameters;
    [SerializeField] private GameObject _teamAPrefab;

    [Title("    Team B")]
    [SerializeField] private ProcGenParameters _teamBParameters;
    [SerializeField] private GameObject _teamBPrefab;

    [Title("    Landmarks")]
    [SerializeField] private ProcGenParameters _landmarksParameters;
    [SerializeField] private GameObject _landmarksPrefab;

    [Title("    Crowd")]
    [SerializeField] private ProcGenParameters _CrowdParameters;
    [SerializeField] private GameObject _CrowdPrefab;

    [Button]
    private void GenerateMap()
    {
        // Generate Map ground

        List<Vector2> tmpPoints;
        tmpPoints = GeneratePoints(_teamAParameters, true);
        SpawnPrefabs(tmpPoints, _teamAPrefab);

        tmpPoints = GeneratePoints(_teamBParameters, true);
        SpawnPrefabs(tmpPoints, _teamBPrefab);

        tmpPoints = GeneratePoints(_landmarksParameters, true);
        SpawnPrefabs(tmpPoints, _landmarksPrefab);

        tmpPoints = GeneratePoints(_CrowdParameters, false);
        SpawnPrefabs(tmpPoints, _CrowdPrefab);

    }

    private List<Vector2> GeneratePoints(ProcGenParameters parameters, bool forceExactNumber)
    {
        Vector2 newRegionSize = _regionSize;
        newRegionSize.x -= parameters.edgeDistance * 2;
        newRegionSize.y -= parameters.edgeDistance * 2;

        List<Vector2> points = PoissonDiscSampling.GenerateExactNumberOfPoints(parameters._minDistance, newRegionSize, parameters._numOfPoints, 360, 10000);
        if (points.Count < parameters._numOfPoints && forceExactNumber)
        {
            Debug.Log("Not enougth points, something went wrong? \n Number of spawned objects : " + points.Count);
        }

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            point.x += parameters.edgeDistance;
            point.y += parameters.edgeDistance;
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


}
