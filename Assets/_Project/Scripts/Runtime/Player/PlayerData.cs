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
        
        [Title("Tongue Settings")]
        [Tooltip("Minimum time between tongue uses in seconds")]
        public float TongueAbilityCooldownSeconds = 1f;
        public float TongueSpeed = 10f;
        public float MaxTongueDistance = 10f;
        public float TongueForce = 10f;
        public float TongueSphereCastRadius = 0.5f;
    }
}