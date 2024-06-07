using System.Collections.Generic;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Procedural_Generation
{
    [CreateAssetMenu(fileName = nameof(ProcGenPreset), menuName = "Scriptable Objects/" + nameof(ProcGenPreset))]
    public class ProcGenPreset : ScriptableObject
    {
        [Title("Preset Settings")]
        public CrowdSizeType CrowdSizeType;
        public MapSizeType MapSizeType;
        [Title("Map Settings")]
        public Vector2 MapSize = new Vector2(75, 75);
        [Title("References")]
        public ProcGenParameters TeamsParameters;
        public ProcGenParameters LandmarksParameters;
        public List<SpawnableNetworkObject> LandmarksPrefabList;
        public ProcGenParameters CrowdParameters;
        public ProcGenParameters PlantsParameters;
        public ProcGenParameters TreesParameters;
    }
}