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
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            Debug.Log($"Server connection state changed to {args.ConnectionState}");
            if (args.ConnectionState == LocalConnectionState.Started)
            {
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