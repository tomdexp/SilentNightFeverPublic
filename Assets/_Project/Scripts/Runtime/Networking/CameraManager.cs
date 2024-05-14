using System;
using _Project.Scripts.Runtime.Utils.Singletons;
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

        [Button(ButtonSizes.Large)]
        public void EnableSplitScreenCameras()
        {
            _canvasSplitScreen.SetActive(true);
            Logger.LogDebug("SplitScreen Cameras enabled", Logger.LogType.Local, this);
        }
        
        [Button(ButtonSizes.Large)]
        public void DisableSplitScreenCameras()
        {
            _canvasSplitScreen.SetActive(false);
            Logger.LogDebug("SplitScreen Cameras disabled", Logger.LogType.Local, this);
        }
    }
}