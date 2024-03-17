using System.Collections;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Managing.Server;
using FishNet.Transporting.UTP;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    /// <summary>
    /// This class is responsible for linking the lobby scene and the game scene
    /// </summary>
    public class BootstrapManager : PersistentSingleton<BootstrapManager>
    {
        [Button]
        public void StopAndRestart()
        {
            StartCoroutine(StopAndRestartCoroutine());
        }

        private IEnumerator StopAndRestartCoroutine()
        {
            InstanceFinder.ServerManager.StopConnection(false);
            yield return new WaitForSecondsRealtime(2f);
            var fishyUnityTransport = FindAnyObjectByType<FishyUnityTransport>();
            fishyUnityTransport.SetProtocol(FishyUnityTransport.ProtocolType.RelayUnityTransport);
            InstanceFinder.ServerManager.StartConnection();
        }
    }
}