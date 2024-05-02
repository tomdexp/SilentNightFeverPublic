using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks
{
    public abstract class LandmarkData : ScriptableObject
    {
        [Title("Landmark Data")]
        public LandmarkTag Tags;
    }
}