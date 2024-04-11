using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    [DefaultExecutionOrder(-1000)]
    public class NetworkStartObjectSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject[] _networkObjects;

        private void Awake()
        {
            //InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                if (InstanceFinder.IsServerStarted == false) return; // Only the server should spawn network objects
                // Spawn network objects
                foreach (var networkObject in _networkObjects)
                {
                    Logger.LogTrace("Spawning network object " + networkObject.name, Logger.LogType.Server);
                    var go = Instantiate(networkObject);
                    InstanceFinder.ServerManager.Spawn(go);
                }
            }
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                if (InstanceFinder.IsServerStarted == false) return; // Only the server should spawn network objects
                // Spawn network objects
                foreach (var networkObject in _networkObjects)
                {
                    var go = Instantiate(networkObject);
                    InstanceFinder.ServerManager.Spawn(go);
                }
            }
        }
    }
}