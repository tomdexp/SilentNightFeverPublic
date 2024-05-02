using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [field: SerializeField] public PlayerIndexType PlayerIndexType { get; private set; }
        [SerializeField] private float _cameraOffsetRadius;
        [SerializeField] private float _cameraHeight= 4;
        [ReadOnly] public float _cameraAngle;

        private void Awake()
        {
            RandomRotateCameraAroundPlayer();
        }

        private void RandomRotateCameraAroundPlayer()
        {
            if (!TryGetComponent(out CinemachineFollow cinemachineFollow)) return;

            _cameraAngle = Random.Range(0, 3.14f);
            float x = Mathf.Cos(_cameraAngle) * _cameraOffsetRadius;
            float y = Mathf.Sin(_cameraAngle) * _cameraOffsetRadius;

            cinemachineFollow.FollowOffset = new Vector3(x, _cameraHeight, y);
        }
    }
}