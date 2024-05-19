using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ProcGen Parameters", fileName = "ProcGen Parameters")]
public class ProcGenParameters : ScriptableObject
{
    [SerializeField, MinValue(1)] public float _minDistance = 1;
    [SerializeField, MinValue("@_minDistance"), MaxValue("@_minDistance*3")] public float _maxDistance = 1;
    [SerializeField, MinValue(0)] public float _distanceWithOtherObjects = 1;
    [SerializeField, Range(0, 100), SuffixLabel("%")] public float _edgeDistance = 0;
    [SerializeField, MinValue(1)] public int _numOfPoints = 1;
}
