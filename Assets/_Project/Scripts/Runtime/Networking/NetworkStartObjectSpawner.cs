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

        private void Start()
        {
            //InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            
            // If the server is already started, spawn the network objects
            if (InstanceFinder.IsServerStarted)
            {
                SpawnAllNetworkObjects();
            }
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                SpawnAllNetworkObjects();
            }
            Logger.LogTrace("Client connection state changed to " + args.ConnectionState, context:this);
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
        
        private void SpawnAllNetworkObjects()
        {
            if (InstanceFinder.IsServerStarted == false) return; // Only the server should spawn network objects
            // Spawn network objects
            foreach (var networkObject in _networkObjects)
            {
                Logger.LogTrace("Spawning network object " + networkObject.name, context:this);
                var go = Instantiate(networkObject);
                InstanceFinder.ServerManager.Spawn(go);
            }
        }
    }
}