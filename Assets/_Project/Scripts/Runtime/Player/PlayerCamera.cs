using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [field: SerializeField] public PlayerIndexType PlayerIndexType { get; private set; }
    }
}