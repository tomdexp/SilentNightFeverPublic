using System;
using System.Collections;
using System.Linq;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils.Singletons;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class CameraManager : NetworkPersistentSingleton<CameraManager>
    {
        [Title("References")]
        [SerializeField, Required] private PlayerData _playerData;
        [SerializeField, Required] private GameObject _canvasSplitScreen;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private PlayerCamera _playerCameraA; // team A
        [SerializeField, ReadOnly] private PlayerCamera _playerCameraB; // team B
        [SerializeField, ReadOnly] private PlayerCamera _playerCameraC; // team A
        [SerializeField, ReadOnly] private PlayerCamera _playerCameraD; // team B
        
        public float DefaultPlayerFov => _playerData.CameraFov;

        private void Start()
        {
            if (!_canvasSplitScreen)
            {
                Logger.LogError("Canvas Split Screen is not set in the inspector of the CameraManager", Logger.LogType.Local, this);
            }
            if (!_playerData)
            {
                Logger.LogError("Player Data is not set in the inspector of the CameraManager", Logger.LogType.Local, this);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(TryGetPlayerCameras());
        }

        private IEnumerator TryGetPlayerCameras()
        {
            var cameras = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None);
            while (cameras.Length != 4)
            {
                cameras = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None);
                yield return null;
            }
            _playerCameraA = cameras.First(x => x.PlayerIndexType == PlayerIndexType.A);
            _playerCameraB = cameras.First(x => x.PlayerIndexType == PlayerIndexType.B);
            _playerCameraC = cameras.First(x => x.PlayerIndexType == PlayerIndexType.C);
            _playerCameraD = cameras.First(x => x.PlayerIndexType == PlayerIndexType.D);
        }

        public void TryEnableSplitScreenCameras()
        {
            if (!IsServerStarted)
            {
                EnableSplitScreenCamerasServerRpc();
            }
            else
            {
                EnableSplitScreenCamerasClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void EnableSplitScreenCamerasServerRpc()
        {
            EnableSplitScreenCamerasClientRpc();
        }

        [ObserversRpc]
        private void EnableSplitScreenCamerasClientRpc()
        {
            EnableSplitScreenCameras();
        }

        private void EnableSplitScreenCameras()
        {
            _canvasSplitScreen.SetActive(true);
            Logger.LogDebug("SplitScreen Cameras enabled", Logger.LogType.Local, this);
        }

        public void TryDisableSplitScreenCameras()
        {
            if (!IsServerStarted)
            {
                DisableSplitScreenCamerasServerRpc();
            }
            else
            {
                DisableSplitScreenCamerasClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void DisableSplitScreenCamerasServerRpc()
        {
            DisableSplitScreenCamerasClientRpc();
        }

        [ObserversRpc]
        private void DisableSplitScreenCamerasClientRpc()
        {
            DisableSplitScreenCameras();
        }

        private void DisableSplitScreenCameras()
        {
            _canvasSplitScreen.SetActive(false);
            Logger.LogDebug("SplitScreen Cameras disabled", Logger.LogType.Local, this);
        }

        public float GetFov(PlayerTeamType teamType)
        {
            return teamType switch
            {
                PlayerTeamType.A => _playerCameraA.CameraFov.Value,
                PlayerTeamType.B => _playerCameraB.CameraFov.Value,
                PlayerTeamType.Z => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(teamType), teamType, null)
            };
        }
        
        public void SetFov(PlayerTeamType teamType, float newFov)
        {
            switch (teamType)
            {
                case PlayerTeamType.A:
                    SetFov(PlayerIndexType.A, newFov);
                    SetFov(PlayerIndexType.C, newFov);
                    break;
                case PlayerTeamType.B:
                    SetFov(PlayerIndexType.B, newFov);
                    SetFov(PlayerIndexType.D, newFov);
                    break;
                case PlayerTeamType.Z:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(teamType), teamType, null);
            }
        }
        
        private void SetFov(PlayerIndexType playerIndexType, float newFov)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    SetCameraFov(_playerCameraA.CameraFov, newFov);
                    break;
                case PlayerIndexType.B:
                    SetCameraFov(_playerCameraB.CameraFov, newFov);
                    break;
                case PlayerIndexType.C:
                    SetCameraFov(_playerCameraC.CameraFov, newFov);
                    break;
                case PlayerIndexType.D:
                    SetCameraFov(_playerCameraD.CameraFov, newFov);
                    break;
                case PlayerIndexType.Z:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerIndexType), playerIndexType, null);
            }
        }
        
        private void SetCameraFov(SyncVar<float> cameraFov, float newFov)
        {
            DOTween.To(() => cameraFov.Value, x => cameraFov.Value = x, newFov, _playerData.CameraFovChangeDuration)
                .SetEase(_playerData.CameraFovChangeEase);
        }
    }
}