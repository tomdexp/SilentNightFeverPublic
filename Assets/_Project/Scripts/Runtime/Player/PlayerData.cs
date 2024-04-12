using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Runtime.Player
{
    [CreateAssetMenu(fileName = nameof(PlayerData), menuName = "Scriptable Objects/" + nameof(PlayerData))]
    public class PlayerData : ScriptableObject
    {
        [Title("Player Movement Settings")]
        [PropertyRange(0,100)] public float PlayerMovementSpeed = 5f;
        [PropertyRange(0,100)] public float PlayerRotationSpeed = 5f;
        
        [Title("Tongue Settings")]
        [Tooltip("Minimum time between tongue uses in seconds")]
        public float TongueAbilityCooldownSeconds = 1f;
        public float TongueThrowSpeed = 1f;
        public float TongueRetractSpeed = 2f;
        public float MaxTongueDistance = 10f;
        public float TongueForce = 10f;
        public float TongueSphereCastRadius = 0.5f;
        public Ease TongueThrowEase = Ease.Linear;
        public Ease TongueRetractEase = Ease.Linear;
        public float SmoothPlayerMassChangeOnTongueMoveDuration = 0.5f;
    }
}