using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks.Zoom
{
    [CreateAssetMenu(fileName = nameof(LandmarkData_Zoom), menuName = "Scriptable Objects/Landmark Data/" + nameof(LandmarkData_Zoom))]
    public class LandmarkData_Zoom : LandmarkData
    {
        [Title("Landmark Zoom Data")]
        public float MaxFov = 25;
        public float MinSignedAngle = 0;
        public float MaxSignedAngle = 180;
        [Tooltip("Threshold value to have a start and end event for the slider")]
        public float MinSecondsBetweenStepForContinuation = 0.5f;
    }
}