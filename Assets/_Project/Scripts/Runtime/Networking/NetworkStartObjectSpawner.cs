using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using Sirenix.OdinInspector;
using UnityEngine;

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
                    Debug.Log("Spawning network object " + networkObject.name);
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