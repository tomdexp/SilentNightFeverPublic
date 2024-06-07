using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    [CreateAssetMenu(fileName = nameof(MatchConfig), menuName = "Scriptable Objects/" + nameof(MatchConfig))]
    public class MatchConfig : ScriptableObject
    {
        public CrowdSizeType CrowdSizeType;
        public MapSizeType MapSizeType;
    }
}