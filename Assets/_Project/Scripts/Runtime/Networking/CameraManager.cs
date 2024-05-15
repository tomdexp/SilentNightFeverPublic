using System;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class CameraManager : NetworkPersistentSingleton<CameraManager>
    {
        [SerializeField, Required] private GameObject _canvasSplitScreen;

        private void Start()
        {
            if (!_canvasSplitScreen)
            {
                Logger.LogError("Canvas Split Screen is not set in the inspector of the CameraManager", Logger.LogType.Local, this);
            }
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
    }
}