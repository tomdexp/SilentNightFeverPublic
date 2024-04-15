using FishNet.Object;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenInstanciator : MonoBehaviour
{

    [SerializeField] private ProcGenVisualizer _procGenVisualizer;
    [SerializeField] private GameObject _objectToSpawn;


    [Button]
    void GeneratePoints()
    {
        _procGenVisualizer.Generate();

        for (int i = 0; i < _procGenVisualizer.points.Count; i++)
        {
            Instantiate(_objectToSpawn, new Vector3(_procGenVisualizer.points[i].x,0, _procGenVisualizer.points[i].y), Quaternion.identity);
        }
    }
}
