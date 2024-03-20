using System.Collections.Generic;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    public class PlayerManager : NetworkPersistentSingleton<PlayerManager>
    {
        [SerializeField] private NetworkObject _playerPrefab;
        private List<RealPlayerInfo> _realPlayerInfos = new List<RealPlayerInfo>();
        
        public void TrySpawnPlayer()
        {
            if (!IsServerStarted)
            {
                Debug.Log("TrySpawnPlayer request denied locally because not server, ignore this if you are a client-only player.");
                SpawnPlayerServerRpc();
            }
            else
            {
                SpawnPlayer();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc()
        {
            SpawnPlayer();
        }
        
        private void SpawnPlayer()
        {
            Debug.Log("Attempting to spawn player...");
            if (!IsServerStarted)
            {
                Debug.LogError("This method should only be called on the server, if you see this message, it's not normal.");
            }
            var go = Instantiate(_playerPrefab);
            InstanceFinder.ServerManager.Spawn(go);
        }

        private void TryAddRealPlayer(byte clientId, string devicePath)
        {
            if (_realPlayerInfos.Count >= 4)
            {
                Debug.LogError("Cannot add more than 4 players.");
                return;
            }
            _realPlayerInfos.Add(new RealPlayerInfo
            {
                ClientId = clientId,
                DevicePath = devicePath,
                PlayerIndexType = (PlayerIndexType)_realPlayerInfos.Count
            });
            Debug.Log("+ RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " added. There are now " + _realPlayerInfos.Count + " players.");
            // make sure there is no duplicate PlayerIndexType
            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                for (int j = i + 1; j < _realPlayerInfos.Count; j++)
                {
                    if (_realPlayerInfos[i].PlayerIndexType == _realPlayerInfos[j].PlayerIndexType)
                    {
                        Debug.LogError("Duplicate PlayerIndexType detected, this is not allowed. It means you try to add more than 4 players.");
                    }
                }
            }
        }
        
        private void TryRemoveRealPlayer(byte clientId, string devicePath)
        {
            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                if (_realPlayerInfos[i].ClientId == clientId && _realPlayerInfos[i].DevicePath == devicePath)
                {
                    _realPlayerInfos.RemoveAt(i);
                    Debug.Log("- RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " removed. There are now " + _realPlayerInfos.Count + " players.");
                    return;
                }
            }
        }
    }
}