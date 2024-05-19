using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Project.Scripts.Runtime.Inputs;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

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
        [SerializeField] private InputAction _goToLeftTeamInputAction;
        [SerializeField] private InputAction _goToRightTeamInputAction;
        [SerializeField] private InputAction _joinAndFullFakePlayerInputAction;
        private readonly SyncList<RealPlayerInfo> _realPlayerInfos = new SyncList<RealPlayerInfo>();
        private readonly SyncList<PlayerTeamInfo> _playerTeamInfos = new SyncList<PlayerTeamInfo>();
        private readonly SyncVar<bool> _canChangeTeam = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        public int NumberOfPlayers => _realPlayerInfos.Count;
        public event Action<List<RealPlayerInfo>> OnRealPlayerInfosChanged; 
        public event Action<List<PlayerTeamInfo>> OnPlayerTeamInfosChanged;
        public event Action<RealPlayerInfo,RealPlayerInfo> OnRealPlayerPossessed; // source, target
        public event Action<RealPlayerInfo> OnRealPlayerUnpossessed;
        public event Action OnAllPlayerSpawnedLocally;
        
        private int _numberOfPlayerSpawnedLocally = 0;

        public override void OnStartServer()
        {
            _realPlayerInfos.Clear();
            _playerTeamInfos.Clear();
        }

        public override void OnStopServer()
        {
            _realPlayerInfos.Clear();
            _playerTeamInfos.Clear();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _realPlayerInfos.OnChange += OnChangedRealPlayerInfos;
            _playerTeamInfos.OnChange += OnChangedPlayerTeamInfos;
            _joinInputAction.performed += JoinInputActionPerformed;
            _goToRightTeamInputAction.performed += GoToRightTeamInputActionPerformed;
            _goToLeftTeamInputAction.performed += GoToLeftTeamInputActionPerformed;
            _leaveInputAction.performed += LeaveInputActionPerformed;
            _joinAndFullFakePlayerInputAction.performed += JoinAndFullFakePlayerInputActionOnPerformed;
            _joinInputAction.Enable();
            _leaveInputAction.Enable();
            _goToRightTeamInputAction.Enable();
            _goToLeftTeamInputAction.Enable();
            _joinAndFullFakePlayerInputAction.Enable();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _realPlayerInfos.OnChange -= OnChangedRealPlayerInfos;
            _playerTeamInfos.OnChange -= OnChangedPlayerTeamInfos;
            _joinInputAction.Disable();
            _leaveInputAction.Disable();
            _goToRightTeamInputAction.Disable();
            _goToLeftTeamInputAction.Disable();
            _joinAndFullFakePlayerInputAction.Disable();
            _joinInputAction.performed -= JoinInputActionPerformed;
            _leaveInputAction.performed -= LeaveInputActionPerformed;
            _goToRightTeamInputAction.performed -= GoToRightTeamInputActionPerformed;
            _goToLeftTeamInputAction.performed -= GoToLeftTeamInputActionPerformed;
            _joinAndFullFakePlayerInputAction.performed -= JoinAndFullFakePlayerInputActionOnPerformed;
        }
        
        private void OnChangedRealPlayerInfos(SyncListOperation op, int index, RealPlayerInfo oldItem, RealPlayerInfo newItem, bool asServer)
        {
            OnRealPlayerInfosChanged?.Invoke(_realPlayerInfos.Collection);
        }
        
        private void OnChangedPlayerTeamInfos(SyncListOperation op, int index, PlayerTeamInfo oldItem, PlayerTeamInfo newItem, bool asServer)
        { 
            OnPlayerTeamInfosChanged?.Invoke(_playerTeamInfos.Collection);
            if (op == SyncListOperation.Set)
            {
                // Debug the current teams in a single log
                string teams = "";
                foreach (PlayerTeamInfo playerTeamInfo in _playerTeamInfos.Collection)
                {
                    teams += "Player " +playerTeamInfo.PlayerIndexType + " : Team " + playerTeamInfo.PlayerTeamType + " | ";
                }
                Logger.LogTrace("Teams: " + teams, Logger.LogType.Server, this);
            }
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
            Logger.LogTrace("JoinInputActionPerformed with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                      newRealPlayerInfo.DevicePath + " received.", context:this);
            if (_realPlayerInfos.Count == 0)
            {
                // We know this player is not in the list since its empty
                Logger.LogTrace("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                                newRealPlayerInfo.DevicePath + " not in the list. Adding it...", context:this);
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
                    Logger.LogTrace("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                                    newRealPlayerInfo.DevicePath + " already in the list.", context:this);
                    return;
                }
            }

            // This Real Player is not in the list
            Logger.LogTrace("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                            newRealPlayerInfo.DevicePath + " not in the list. Adding it...", context:this);
            if (!IsServerStarted)
            {
                TryAddRealPlayerServerRpc(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
            }
            else
            {
                TryAddRealPlayer(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
            }
        }
        
        private void GoToLeftTeamInputActionPerformed(InputAction.CallbackContext context)
        {
            TryChangeTeam(context, true);
        }
        
        private void GoToRightTeamInputActionPerformed(InputAction.CallbackContext context)
        {
            TryChangeTeam(context, false);
        }

        private void TryChangeTeam(InputAction.CallbackContext context, bool goToLeft)
        {
            if (!_canChangeTeam.Value)
            {
                Logger.LogWarning("Can't change team yet, the variable _canChangeTeam is currently false, don't forget so start team management via TryStartTeamManagement()", context:this);
                return;
            }
            // Reconstruct the RealPlayerInfo
            var realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)LocalConnection.ClientId,
                DevicePath = context.control.device.path
            };
            var exist = DoesRealPlayerExist(realPlayerInfo);
            if (!exist)
            {
                Logger.LogWarning("Can't change team for RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath + " as it does not exist.", context:this);
                return;
            }
            var playerIndexType = GetPlayerIndexTypeFromRealPlayerInfo(realPlayerInfo);
            if (!IsServerStarted)
            {
                ChangeTeamServerRpc(playerIndexType, goToLeft);
            }
            else
            {
                ChangeTeam(playerIndexType, goToLeft);
            }
        }
        
        public void ForceChangeTeam(PlayerIndexType playerIndexType, bool goToLeft)
        {
            if (!_canChangeTeam.Value)
            {
                Logger.LogWarning("Can't change team yet, the variable _canChangeTeam is currently false, don't forget so start team management via TryStartTeamManagement()", context:this);
                return;
            }
            if (!IsServerStarted)
            {
                ChangeTeamServerRpc(playerIndexType, goToLeft);
            }
            else
            {
                ChangeTeam(playerIndexType, goToLeft);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ChangeTeamServerRpc(PlayerIndexType playerIndexType, bool goToLeft)
        {
            ChangeTeam(playerIndexType, goToLeft);
        }

        private void ChangeTeam(PlayerIndexType playerIndexType, bool goToLeft)
        {
            // Structures cannot have their values modified when they reside within a collection. You must instead create a local variable for the collection index you wish to modify, change values on the local copy, then set the local copy back into the collection
            PlayerTeamType newTeam = PlayerTeamType.Z;
            if (goToLeft)
            {
                newTeam = PlayerTeamType.A;
            }
            else
            {
                newTeam = PlayerTeamType.B;
            }
            // get the index of the player in the list
            var playerTeamInfo = _playerTeamInfos.Collection.First(x => x.PlayerIndexType == playerIndexType);
            var index = _playerTeamInfos.IndexOf(playerTeamInfo);
            PlayerTeamInfo copy = _playerTeamInfos[index];
            if (copy.PlayerTeamType == newTeam)
            {
                Logger.LogTrace("Player " + playerIndexType + " is already in team " + newTeam, Logger.LogType.Server, this);
                return;
            }
            copy.PlayerTeamType = newTeam;
            _playerTeamInfos[index] = copy;
            Logger.LogDebug("Player " + playerIndexType + " changed team to " + newTeam, Logger.LogType.Server, this);
        }

        private void JoinAndFullFakePlayerInputActionOnPerformed(InputAction.CallbackContext context)
        {
            JoinInputActionPerformed(context);
            AddFakePlayer();
            AddFakePlayer();
            AddFakePlayer();
            GameManager.Instance.TryStartGame();
        }
        
        public void SetPlayerJoiningEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerJoiningEnabled: " + value, context:this);
            if (value)
            {
                _joinInputAction.Enable();
            }
            else
            {
                _joinInputAction.Disable();
            }
        }
        
        [ObserversRpc]
        private void SetPlayerJoiningEnabledClientRpc(bool value)
        {
            SetPlayerJoiningEnabled(value);
        }
        
        public void SetPlayerLeavingEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerLeavingEnabled: " + value, context:this);
            if (value)
            {
                _leaveInputAction.Enable();
            }
            else
            {
                _leaveInputAction.Disable();
            }
        }
        
        [ObserversRpc]
        private void SetPlayerLeavingEnabledClientRpc(bool value)
        {
            SetPlayerLeavingEnabled(value);
        }
        
        [ObserversRpc]
        private void SetPlayerChangingTeamEnabledClientRpc(bool value)
        {
            SetPlayerChangingTeamEnabled(value);
        }
        
        public void SetPlayerChangingTeamEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerChangingTeamEnabled: " + value, context:this);
            if (value)
            {
                _goToLeftTeamInputAction.Enable();
                _goToRightTeamInputAction.Enable();
            }
            else
            {
                _goToLeftTeamInputAction.Disable();
                _goToRightTeamInputAction.Disable();
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
            Logger.LogTrace("LeaveInputActionPerformed with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                            realPlayerInfo.DevicePath + " received.", context:this);
            if (_realPlayerInfos.Count == 0)
            {
                // We know this player is not in the list since its empty
                Logger.LogTrace("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                                realPlayerInfo.DevicePath + " not in the list. Nothing to remove.", context:this);
                return;
            }

            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                if (_realPlayerInfos[i].ClientId == realPlayerInfo.ClientId &&
                    _realPlayerInfos[i].DevicePath == realPlayerInfo.DevicePath)
                {
                    // This Real Player is in the list
                    Logger.LogTrace("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                                    realPlayerInfo.DevicePath + " found in the list. Removing it...", context:this);
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
            Logger.LogTrace("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                            realPlayerInfo.DevicePath + " not in the list. Nothing to remove.", context:this);
        }
        
        public void TrySpawnPlayer()
        {
            if (!IsServerStarted)
            {
                Logger.LogTrace("TrySpawnPlayer request denied locally because not server, ignore this if you are a client-only player.", context:this);
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
            Logger.LogTrace("Attempting to spawn player...", Logger.LogType.Server, context:this);
            if (!IsServerStarted)
            {
                Logger.LogError("This method should only be called on the server, if you see this message, it's not normal.", context:this);
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
            if (GameManager.Instance.IsGameStarted.Value) return;
            
            if (_realPlayerInfos.Count >= 4)
            {
                Logger.LogTrace("Cannot add more than 4 players.", Logger.LogType.Server, context:this);
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
            Logger.LogTrace("+ RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " added. There are now " + _realPlayerInfos.Count + " players.", Logger.LogType.Server, context:this);
            // make sure there is no duplicate PlayerIndexType
            foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
            {
                Logger.LogTrace("PlayerIndexType: " + realPlayerInfo.PlayerIndexType + " for clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath, Logger.LogType.Server, context:this);
                // TODO Investigate : why is this loop still required
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
                    Logger.LogTrace("- RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " removed. There are now " + _realPlayerInfos.Count + " players.", Logger.LogType.Server, context:this);
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
            if (GameManager.Instance.IsGameStarted.Value) return;
            var randomString = Guid.NewGuid().ToString();
            randomString = randomString.Substring(0, 6);
            var fakePlayerInfo = new RealPlayerInfo
            {
                ClientId = 255,
                DevicePath = "/FakeDevice(" + randomString + ")"
            };
            Logger.LogTrace("Adding fake player with clientId " + fakePlayerInfo.ClientId + " and devicePath " + fakePlayerInfo.DevicePath, Logger.LogType.Server, context:this);
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

        [ServerRpc(RequireOwnership = false)]
        private void RemoveFakePlayerServerRpc()
        {
            RemoveFakePlayer();
        }
        
        private void RemoveFakePlayer()
        {
            if (GameManager.Instance.IsGameStarted.Value) return;
            var fakePlayer = _realPlayerInfos.Collection.Last(x => x.ClientId == 255);
            if (fakePlayer.ClientId == 0) return;
            TryRemoveRealPlayer(fakePlayer.ClientId, fakePlayer.DevicePath);
        }
        
        public void SpawnAllPlayers()
        {
            // ONLY CALLED BY THE SERVER
            if (!IsServerStarted)
            {
                Logger.LogError("This method should only be called on the server, if you see this message, it's not normal.", Logger.LogType.Server, context:this);
                return;
            }
            if (_realPlayerInfos.Count != 4)
            {
                Logger.LogWarning("Not enough real players to spawn all players.", Logger.LogType.Server, context:this);
                return;
            }
            _numberOfPlayerSpawnedLocally = 0;
            SetPlayerJoiningEnabledClientRpc(false);
            SetPlayerLeavingEnabledClientRpc(false);
            SetPlayerChangingTeamEnabledClientRpc(false);
            foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
            {
                Logger.LogTrace("Spawning player for real player " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath, Logger.LogType.Server, context:this);
                var nob = Instantiate(_playerPrefab);
                InstanceFinder.ServerManager.Spawn(nob);
                var networkPlayer = nob.GetComponentInChildren<NetworkPlayer>();
                networkPlayer.SetRealPlayerInfo(realPlayerInfo);
                networkPlayer.GetPlayerController().OnPlayerSpawnedLocally += OnPlayerSpawnedLocally;
                if (realPlayerInfo.ClientId == 255)
                {
                    // If the player is a fake player, give ownership to the first client
                    var conn = InstanceFinder.ServerManager.Clients[1];
                    nob.GiveOwnership(conn);
                    NetworkObject[] networkObjects = nob.GetComponentsInChildren<NetworkObject>();
                    foreach (NetworkObject networkObject in networkObjects)
                    {
                        networkObject.GiveOwnership(conn);
                    }
                }
                else
                {
                    var conn = InstanceFinder.ServerManager.Clients[realPlayerInfo.ClientId];
                    nob.GiveOwnership(conn);
                    NetworkObject[] networkObjects = nob.GetComponentsInChildren<NetworkObject>();
                    foreach (NetworkObject networkObject in networkObjects)
                    {
                        networkObject.GiveOwnership(conn);
                    }
                }
            }
        }

        private void OnPlayerSpawnedLocally()
        {
            _numberOfPlayerSpawnedLocally++;
            if (_numberOfPlayerSpawnedLocally == 4)
            {
                OnAllPlayerSpawnedLocally?.Invoke();
                Logger.LogDebug("All players spawned locally.", Logger.LogType.Server, context:this);
            }
        }

        public void TryPossessPlayer(PlayerIndexType sourcePlayerIndexType, PlayerIndexType targetPlayerIndexType)
        {
            if (!IsServerStarted)
            {
                PossessPlayerServerRpc(sourcePlayerIndexType, targetPlayerIndexType);
            }
            else
            {
                PossessPlayer(sourcePlayerIndexType, targetPlayerIndexType);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void PossessPlayerServerRpc(PlayerIndexType sourcePlayerIndexType, PlayerIndexType targetPlayerIndexType)
        {
            PossessPlayer(sourcePlayerIndexType, targetPlayerIndexType);
        }

        private void PossessPlayer(PlayerIndexType sourcePlayerIndexType, PlayerIndexType targetPlayerIndexType)
        {
            if (!IsServerStarted) return;
            // TODO NETWORKING : Only the host can possess a fake player
            var sourceNetworkPlayer = GetNetworkPlayer(sourcePlayerIndexType);
            var targetNetworkPlayer = GetNetworkPlayer(targetPlayerIndexType);
            if (targetNetworkPlayer.GetRealPlayerInfo().ClientId != 255)
            {
                Logger.LogError("Cannot possess a real player.", context:this);
                return;
            }
            var conn = InstanceFinder.ServerManager.Clients[sourceNetworkPlayer.OwnerId];
            //targetNetworkPlayer.GiveOwnership(conn);
            targetNetworkPlayer.GetComponent<PlayerController>().BindInputProvider(sourceNetworkPlayer.GetComponent<HardwareInputProvider>());
            sourceNetworkPlayer.GetComponent<PlayerController>().ClearInputProvider();
            Logger.LogDebug("Player " + targetPlayerIndexType + " possessed by player " + sourcePlayerIndexType, context:this);
            OnRealPlayerPossessed?.Invoke(sourceNetworkPlayer.GetRealPlayerInfo(), targetNetworkPlayer.GetRealPlayerInfo());
        }
        
        public void TryUnpossessPlayer(PlayerIndexType playerIndexType)
        {
            if (!IsServerStarted)
            {
                UnpossessPlayerServerRpc(playerIndexType);
            }
            else
            {
                UnpossessPlayer(playerIndexType);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnpossessPlayerServerRpc(PlayerIndexType playerIndexType)
        {
            UnpossessPlayer(playerIndexType);
        }

        private void UnpossessPlayer(PlayerIndexType playerIndexType)
        {
            if (!IsServerStarted) return;
            var networkPlayer = GetNetworkPlayer(playerIndexType);
            if (networkPlayer.GetRealPlayerInfo().ClientId != 255)
            {
                Logger.LogError("Cannot unpossess a real player.", context:this);
                return;
            }
            //networkPlayer.RemoveOwnership();
            networkPlayer.GetComponent<PlayerController>().ClearInputProvider();
            OnRealPlayerUnpossessed?.Invoke(networkPlayer.GetRealPlayerInfo());
            Logger.LogDebug("Player " + playerIndexType + " unpossessed.", context:this);
        }

        public NetworkPlayer GetNetworkPlayer(PlayerIndexType playerIndexType)
        {
            return FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).ToList().Find(x => x.GetPlayerIndexType() == playerIndexType);
        }
        
        public List<RealPlayerInfo> GetRealPlayerInfos()
        {
            return _realPlayerInfos.Collection;
        }
        
        [ServerRpc(RunLocally = true, RequireOwnership = false)]
        public void SetCanChangeTeam(bool value)
        {
            Logger.LogDebug("SetCanChangeTeam: " + value, Logger.LogType.Server, context:this);
            _canChangeTeam.Value = value;
        }
        
        [Button(ButtonSizes.Medium)]
        public void TryStartTeamManagement()
        {
            if (_realPlayerInfos.Collection.Count != 4)
            {
                Logger.LogWarning("Not enough real players to start team management.", context:this);
                return;
            }
            
            if (!IsServerStarted)
            {
                StartTeamManagementServerRpc();
            }
            else
            {
                StartTeamManagement();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartTeamManagementServerRpc()
        {
            StartTeamManagement();
        }
        
        private void StartTeamManagement()
        {
            SetPlayerLeavingEnabledClientRpc(false);
            SetPlayerJoiningEnabledClientRpc(false);
            Logger.LogInfo("Team management started", Logger.LogType.Server, context:this);
            List<PlayerTeamInfo> playerTeamInfos = new List<PlayerTeamInfo>();
            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                playerTeamInfos.Add(new PlayerTeamInfo
                {
                    PlayerIndexType = _realPlayerInfos[i].PlayerIndexType,
                    PlayerTeamType = PlayerTeamType.Z
                });
            }
            _playerTeamInfos.AddRange(playerTeamInfos);
            SetCanChangeTeam(true);
        }
        
        public bool DoesRealPlayerExist(RealPlayerInfo realPlayerInfo)
        {
            return _realPlayerInfos.Collection.Any(x => x.ClientId == realPlayerInfo.ClientId && x.DevicePath == realPlayerInfo.DevicePath);
        }

        public PlayerIndexType GetPlayerIndexTypeFromRealPlayerInfo(RealPlayerInfo realPlayerInfo)
        {
            // We can't just use realPlayerInfo.PlayerIndexType because it's not the same instance, we have to take the sync list of the server
            return _realPlayerInfos.Collection.First(x => x.ClientId == realPlayerInfo.ClientId && x.DevicePath == realPlayerInfo.DevicePath).PlayerIndexType;
        }
        
        public void TryGiveEffectToPlayer<T>(PlayerIndexType playerIndexType) where T : PlayerEffect
        {
            Logger.LogTrace("TryGiveEffectToPlayer " + playerIndexType, context:this);
            if (!IsServerStarted)
            {
                GiveEffectToPlayerServerRpc(playerIndexType, PlayerEffectHelper.EffectToByte<T>());
            }
            else
            {
                GiveEffectToPlayerClientRpc(playerIndexType, PlayerEffectHelper.EffectToByte<T>());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void GiveEffectToPlayerServerRpc(PlayerIndexType playerIndexType, byte effectIndex)
        {
            Logger.LogTrace("GiveEffectToPlayerServerRpc " + playerIndexType, Logger.LogType.Server, this);
            GiveEffectToPlayerClientRpc(playerIndexType,effectIndex);
        }
        
        [ObserversRpc]
        private void GiveEffectToPlayerClientRpc(PlayerIndexType playerIndexType, byte effectIndex)
        {
            Logger.LogTrace("GiveEffectToPlayerClientRpc " + playerIndexType, context: this);
            Type effectType = PlayerEffectHelper.ByteToEffect(effectIndex);

            // Use reflection to call the generic method GiveEffectToPlayer
            MethodInfo giveEffectMethod = typeof(PlayerManager).GetMethod("GiveEffectToPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericMethod = giveEffectMethod.MakeGenericMethod(effectType);

            // Invoke the generic method with the specific type
            genericMethod.Invoke(this, new object[] { playerIndexType });
        }

        
        private void GiveEffectToPlayer<T>(PlayerIndexType playerIndexType) where T : PlayerEffect
        {
            var networkPlayer = GetNetworkPlayer(playerIndexType);
            if (networkPlayer == null)
            {
                Logger.LogWarning("NetworkPlayer not found for playerIndexType " + playerIndexType, context:this);
                return;
            }
            networkPlayer.GiveEffect<T>();
        }

        public IEnumerable<NetworkPlayer> GetNetworkPlayers()
        {
            return FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        }

        public IEnumerable<NetworkPlayer> GetNetworkPlayers(PlayerTeamType teamType)
        {
            return FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Where(x => x.GetPlayerTeamType() == teamType);
        }
    }
}