using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    // Used only for the Onboarding scene, this is not needed on the real map
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [Title("Settings")]
        [field: SerializeField] public PlayerIndexType PlayerIndexType { get; private set; }
        [field: SerializeField] public Transform SpawnPoint { get; private set; }

        private void OnDrawGizmos()
        {
            if (!SpawnPoint) return;
            Gizmos.color = PlayerIndexType switch
            {
                PlayerIndexType.A => Color.red,
                PlayerIndexType.B => Color.blue,
                PlayerIndexType.C => Color.green,
                PlayerIndexType.D => Color.yellow,
                PlayerIndexType.Z => Color.magenta,
                _ => throw new ArgumentOutOfRangeException()
            };
            Gizmos.DrawSphere(SpawnPoint.position, 0.5f);
        }
    }
}