using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks.Kitchen
{
    [CreateAssetMenu(fileName = nameof(LandmarkData_Kitchen), menuName = "Scriptable Objects/Landmark Data/" + nameof(LandmarkData_Kitchen))]
    public class LandmarkData_Kitchen : LandmarkData
    {
        [Title("Landmark Kitchen Data")]
        public NetworkObject[] FoodsToSpawn;
    }
}