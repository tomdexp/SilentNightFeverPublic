using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ProcGen Parameters", fileName = "ProcGen Parameters")]
public class ProcGenParameters : ScriptableObject
{
    [SerializeField, MinValue(1)] public float _minDistance = 1;
    [SerializeField, MinValue(1)] public Vector2 _regionSize = Vector2.one;
    [SerializeField, MinValue(1)] public int _numOfPoints = 1;
}
