using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting.UTP;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    public class NetworkStartObjectSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject[] _networkObjects;
        
        public override void OnStartServer()
        {
            if (!IsServerStarted) return;
            if (_networkObjects == null) return;
            foreach (var networkObject in _networkObjects)
            {
                var go = Instantiate(networkObject.gameObject);
                InstanceFinder.ServerManager.Spawn(go, null);
            }
        }
    }
}