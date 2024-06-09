using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerCameraOverride : MonoBehaviour
    {
        [Title("Settings")]
        [field: SerializeField] public PlayerIndexType PlayerIndexType { get; private set; }
        [SerializeField, Required] private CinemachineCamera _cinemachineCamera;
        private PlayerCamera _playerCamera;

        private IEnumerator Start()
        {
            if (!_cinemachineCamera)
            {
                Logger.LogError("No CinemachineCamera found on PlayerCameraOverride", Logger.LogType.Local, this);
                yield break;
            }

            _cinemachineCamera.OutputChannel = PlayerIndexType switch
            {
                PlayerIndexType.A => OutputChannels.Channel01,
                PlayerIndexType.B => OutputChannels.Channel02,
                PlayerIndexType.C => OutputChannels.Channel03,
                PlayerIndexType.D => OutputChannels.Channel04,
                _ => throw new ArgumentOutOfRangeException()
            };

            _cinemachineCamera.Priority.Value = 999;
            
            yield return new WaitUntil(() => PlayerManager.HasInstance);
            yield return new WaitUntil(() => PlayerManager.Instance.AreAllPlayerSpawnedLocally);
            var playerCameras = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None);
            foreach (var playerCamera in playerCameras)
            {
                if (playerCamera.PlayerIndexType == PlayerIndexType)
                {
                    _playerCamera = playerCamera;
                    break;
                }
            }
        }

        private void LateUpdate()
        {
            if (_playerCamera)
            {
                _playerCamera.transform.position = _cinemachineCamera.transform.position;
                _playerCamera.transform.rotation = _cinemachineCamera.transform.rotation;
            }
        }
    }
}