using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    [CreateAssetMenu(fileName = nameof(PlayerData), menuName = "Scriptable Objects/" + nameof(PlayerData))]
    public class PlayerData : ScriptableObject
    {
        [Title("Player Movement Settings")]
        [PropertyRange(0,100)]
        public float PlayerMovementSpeed = 5f;
    }
}