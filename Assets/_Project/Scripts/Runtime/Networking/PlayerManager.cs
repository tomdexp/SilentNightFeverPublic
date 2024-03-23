using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Networking
{
    /// <summary>
    /// This class is responsible for managing the players in the game
    /// It should create the players and assign them to the correct team
    /// </summary>
    public class PlayerManager : NetworkPersistentSingleton<PlayerManager>
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private InputAction _joinInputAction;
        [SerializeField] private InputAction _leaveInputAction;
        private readonly SyncList<RealPlayerInfo> _realPlayerInfos = new SyncList<RealPlayerInfo>();
        public int NumberOfPlayers => _realPlayerInfos.Count;
        public event Action<List<RealPlayerInfo>> OnRealPlayerInfosChanged; 

        public override void OnStartClient()
        {
            base.OnStartClient();
            _realPlayerInfos.Clear();
            _realPlayerInfos.OnChange += OnChangedRealPlayerInfos;
            _joinInputAction.performed += JoinInputActionPerformed;
            _leaveInputAction.performed += LeaveInputActionPerformed;
            _joinInputAction.Enable();
            _leaveInputAction.Enable();
        }
        
        public override void OnStopClient()
        {
            base.OnStopClient();
            _realPlayerInfos.Clear();
            _realPlayerInfos.OnChange -= OnChangedRealPlayerInfos;
            _joinInputAction.Disable();
            _leaveInputAction.Disable();
            _joinInputAction.performed -= JoinInputActionPerformed;
            _leaveInputAction.performed -= LeaveInputActionPerformed;
        }
        
        private void OnChangedRealPlayerInfos(SyncListOperation op, int index, RealPlayerInfo oldItem, RealPlayerInfo newItem, bool asServer)
        {
            OnRealPlayerInfosChanged?.Invoke(_realPlayerInfos.Collection);
        }

        private void JoinInputActionPerformed(InputAction.CallbackContext context)
        {
            // This method is always call locally
            // Verify if the real player is already in the list
            var newRealPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)LocalConnection.ClientId,
                DevicePath = context.control.device.path
            };
            Debug.Log("JoinInputActionPerformed with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                      newRealPlayerInfo.DevicePath + " received.");
            if (_realPlayerInfos.Count == 0)
            {
                // We know this player is not in the list since its empty
                Debug.Log("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                          newRealPlayerInfo.DevicePath + " not in the list. Adding it...");
                if (!IsServerStarted)
                {
                    TryAddRealPlayerServerRpc(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
                    return;
                }
                else
                {
                    TryAddRealPlayer(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
                    return;
                }
            }

            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                if (_realPlayerInfos[i].ClientId == newRealPlayerInfo.ClientId &&
                    _realPlayerInfos[i].DevicePath == newRealPlayerInfo.DevicePath)
                {
                    // This Real Player is already in the list
                    Debug.Log("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                              newRealPlayerInfo.DevicePath + " already in the list.");
                    return;
                }
            }

            // This Real Player is not in the list
            Debug.Log("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                      newRealPlayerInfo.DevicePath + " not in the list. Adding it...");
            if (!IsServerStarted)
            {
                TryAddRealPlayerServerRpc(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
            }
            else
            {
                TryAddRealPlayer(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
            }
        }
        
        public void SetPlayerJoiningEnabled(bool value)
        {
            if (value)
            {
                _joinInputAction.Enable();
            }
            else
            {
                _joinInputAction.Disable();
            }
        }
        
        public void SetPlayerLeavingEnabled(bool value)
        {
            if (value)
            {
                _leaveInputAction.Enable();
            }
            else
            {
                _leaveInputAction.Disable();
            }
        }
        
        
        private void LeaveInputActionPerformed(InputAction.CallbackContext context)
        {
            // This method is always call locally
            var realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)LocalConnection.ClientId,
                DevicePath = context.control.device.path
            };
            Debug.Log("LeaveInputActionPerformed with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                      realPlayerInfo.DevicePath + " received.");
            if (_realPlayerInfos.Count == 0)
            {
                // We know this player is not in the list since its empty
                Debug.Log("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                          realPlayerInfo.DevicePath + " not in the list. Nothing to remove.");
                return;
            }

            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                if (_realPlayerInfos[i].ClientId == realPlayerInfo.ClientId &&
                    _realPlayerInfos[i].DevicePath == realPlayerInfo.DevicePath)
                {
                    // This Real Player is in the list
                    Debug.Log("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                              realPlayerInfo.DevicePath + " found in the list. Removing it...");
                    if (!IsServerStarted)
                    {
                        TryRemoveRealPlayerServerRpc(realPlayerInfo.ClientId, realPlayerInfo.DevicePath);
                        return;
                    }
                    else
                    {
                        TryRemoveRealPlayer(realPlayerInfo.ClientId, realPlayerInfo.DevicePath);
                        return;
                    }
                }
            }

            // This Real Player is not in the list
            Debug.Log("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                      realPlayerInfo.DevicePath + " not in the list. Nothing to remove.");
        }

        

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

        [ServerRpc(RequireOwnership = false)]
        private void TryAddRealPlayerServerRpc(byte clientId, string devicePath)
        {
            TryAddRealPlayer(clientId, devicePath);
        }
        
        private void TryAddRealPlayer(byte clientId, string devicePath)
        {
            if (_realPlayerInfos.Count >= 4)
            {
                Debug.Log("Cannot add more than 4 players.");
                return;
            }
            
            // We need to find a free PlayerIndexType
            PlayerIndexType freePlayerIndexType = PlayerIndexType.Z;
            for (int i = 0; i < 4; i++)
            {
                bool found = false;
                foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
                {
                    if (realPlayerInfo.PlayerIndexType == (PlayerIndexType)i)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    freePlayerIndexType = (PlayerIndexType)i;
                    break;
                }
            }
            
            _realPlayerInfos.Add(new RealPlayerInfo
            {
                ClientId = clientId,
                DevicePath = devicePath,
                PlayerIndexType = freePlayerIndexType
            });
            Debug.Log("+ RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " added. There are now " + _realPlayerInfos.Count + " players.");
            // make sure there is no duplicate PlayerIndexType
            foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
            {
                Debug.Log("PlayerIndexType: " + realPlayerInfo.PlayerIndexType + " for clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TryRemoveRealPlayerServerRpc(byte clientId, string devicePath)
        {
            TryRemoveRealPlayer(clientId, devicePath);
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
        
        public void TryAddFakePlayer()
        {
            if (!IsServerStarted)
            {
                AddFakePlayerServerRpc();
            }
            else
            {
                AddFakePlayer();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void AddFakePlayerServerRpc()
        {
            AddFakePlayer();
        }

        private void AddFakePlayer()
        {
            var randomString = Guid.NewGuid().ToString();
            randomString = randomString.Substring(0, 6);
            var fakePlayerInfo = new RealPlayerInfo
            {
                ClientId = 255,
                DevicePath = "/FakeDevice(" + randomString + ")"
            };
            Debug.Log("Adding fake player with clientId " + fakePlayerInfo.ClientId + " and devicePath " + fakePlayerInfo.DevicePath);
            TryAddRealPlayer(fakePlayerInfo.ClientId, fakePlayerInfo.DevicePath);
        }
        
        public void TryRemoveFakePlayer()
        {
            if (!IsServerStarted)
            {
                RemoveFakePlayerServerRpc();
            }
            else
            {
                RemoveFakePlayer();
            }
        }

        [ServerRpc]
        private void RemoveFakePlayerServerRpc()
        {
            RemoveFakePlayer();
        }
        
        private void RemoveFakePlayer()
        {
            var fakePlayer = _realPlayerInfos.Collection.Last(x => x.ClientId == 255);
            if (fakePlayer.ClientId == 0) return;
            TryRemoveRealPlayer(fakePlayer.ClientId, fakePlayer.DevicePath);
        }
        
        public void SpawnAllPlayers()
        {
            // ONLY CALLED BY THE SERVER
            if (!IsServerStarted)
            {
                Debug.LogError("This method should only be called on the server, if you see this message, it's not normal.");
                return;
            }
            if (_realPlayerInfos.Count != 4)
            {
                Debug.LogError("Not enough real players to spawn all players.");
                return;
            }
            foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
            {
                var nob = Instantiate(_playerPrefab);
                InstanceFinder.ServerManager.Spawn(nob);
                nob.GetComponent<NetworkPlayer>().SetRealPlayerInfo(realPlayerInfo);
                if (realPlayerInfo.ClientId == 255) continue; // No need to give ownership to fake players
                var conn = InstanceFinder.ServerManager.Clients[realPlayerInfo.ClientId];
                nob.GiveOwnership(conn);
            }
        }
    }
}