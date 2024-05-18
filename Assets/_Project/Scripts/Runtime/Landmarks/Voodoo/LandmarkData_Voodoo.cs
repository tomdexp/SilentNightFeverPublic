using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks.Voodoo
{
    [CreateAssetMenu(fileName = nameof(LandmarkData_Voodoo), menuName = "Scriptable Objects/Landmark Data/" + nameof(LandmarkData_Voodoo))]
    public class LandmarkData_Voodoo : LandmarkData
    {
        [Title("Landmark Voodoo Data")]
        [Tooltip("Inputs of the player will be mixed with puppet direction with this factor, if the player inputs nothing, the player will fully moves with the puppet direction")]
        public float ForcedMovementInfluenceFactor = 0.5f;
        [Tooltip("Puppet that moved from their origin position less than this value will be considered as idle")]
        public float MinDistanceThreshold = 0.1f;
    }
}