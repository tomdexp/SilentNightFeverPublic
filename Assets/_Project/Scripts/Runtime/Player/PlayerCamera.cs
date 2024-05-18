using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = _Project.Scripts.Runtime.Utils.Logger;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerCamera : NetworkBehaviour
    {
        [Title("References")]
        [field: SerializeField, Required] public PlayerData PlayerData { get; private set; }
        [Title("Settings")]
        [field: SerializeField] public PlayerIndexType PlayerIndexType { get; private set; }

        public float CameraAngle => _cameraAngle.Value;
        private readonly SyncVar<Vector3> _cameraFollowOffset = new SyncVar<Vector3>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        private readonly SyncVar<float> _cameraAngle = new SyncVar<float>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

        private void Awake()
        {
            _cameraFollowOffset.OnChange += OnCameraFollowOffsetChanged;
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            RandomRotateCameraAroundPlayer();
            StartCoroutine(TrySubscribingToOnAnyRoundStartedEvent());
        }
        private IEnumerator TrySubscribingToOnAnyRoundStartedEvent()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.OnAnyRoundStarted += OnRoundStart;
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance) GameManager.Instance.OnAnyRoundStarted -= OnRoundStart;
        }

        private void OnRoundStart(byte roundIndex)
        {
            if (roundIndex == 1) return; // Camera is already set up for the first round
            RandomRotateCameraAroundPlayer();
        }

        private void OnCameraFollowOffsetChanged(Vector3 prev, Vector3 next, bool asServer)
        {
            if (asServer) return;
            ApplyCameraFollowOffset(next);
        }

        [Server]
        private void RandomRotateCameraAroundPlayer()
        {
            Logger.LogTrace("Randomly rotating camera around player " + PlayerIndexType, Logger.LogType.Server, this);
            _cameraAngle.Value = Random.Range(0, 3.14f);
            float x = Mathf.Cos(_cameraAngle.Value) * PlayerData.CameraOffsetRadius;
            float y = Mathf.Sin(_cameraAngle.Value) * PlayerData.CameraOffsetRadius;
            _cameraFollowOffset.Value = new Vector3(x, PlayerData.CameraHeight, y);
            ApplyCameraFollowOffset(_cameraFollowOffset.Value);
        }
        
        private void ApplyCameraFollowOffset(Vector3 newFollowOffset)
        {
            if (!TryGetComponent(out CinemachineFollow cinemachineFollow))
            {
                Logger.LogError("CinemachineFollow component not found on player camera, can't apply camera follow offset !", Logger.LogType.Local, this);
                return;
            }
            Logger.LogTrace("Applying camera follow offset of " + newFollowOffset + " to player camera " + PlayerIndexType, Logger.LogType.Client, this);
            cinemachineFollow.FollowOffset = newFollowOffset;
        }
    }
}