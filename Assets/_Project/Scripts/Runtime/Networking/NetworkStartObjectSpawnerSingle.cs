using System;
using System.Collections;
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using Unity.Mathematics;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    [DefaultExecutionOrder(-500)]
    public class NetworkStartObjectSpawnerSingle : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        
        private void Awake()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                if (InstanceFinder.IsServerStarted == false) return; // Only the server should spawn network objects
                Logger.LogTrace("Spawning network object " + networkObject.name, Logger.LogType.Server, context: this);
                var go = Instantiate(networkObject, transform.position, quaternion.identity);
                InstanceFinder.ServerManager.Spawn(go);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, .5f);
        }
    }
}