using System;
using System.Collections;
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
using FishNet.Transporting;
using Sirenix.OdinInspector;
using Unity.Services.CloudSave.Models.Data.Player;
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
        [SerializeField] private InputAction _readyInputAction;
        [SerializeField] private InputAction _cancelReadyInputAction;
        [SerializeField] private InputAction _goToLeftTeamInputAction;
        [SerializeField] private InputAction _goToRightTeamInputAction;
        [SerializeField] private InputAction _joinAndFullFakePlayerInputAction;
        private readonly SyncList<RealPlayerInfo> _realPlayerInfos = new SyncList<RealPlayerInfo>();
        private readonly SyncList<PlayerTeamInfo> _playerTeamInfos = new SyncList<PlayerTeamInfo>();
        private readonly SyncList<PlayerReadyInfo> _playerReadyInfos = new SyncList<PlayerReadyInfo>();
        private bool _canChangeTeam = false;

        public readonly SyncVar<bool> CanPlayerUseTongue = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        public int NumberOfPlayers => _realPlayerInfos.Count;
        public event Action<List<RealPlayerInfo>> OnRealPlayerInfosChanged;
        public event Action<List<PlayerReadyInfo>> OnPlayersReadyChanged;
        public event Action<List<PlayerTeamInfo>> OnPlayerTeamInfosChanged;
        public event Action<RealPlayerInfo, RealPlayerInfo> OnRealPlayerPossessed; // source, target
        public event Action<RealPlayerInfo> OnRealPlayerUnpossessed;
        public event Action OnAllPlayerSpawnedLocally;
        public event Action OnRemoteClientDisconnected;
        public event Action OnTeamManagementStarted;
        public event Action OnAllPlayersReady;

        private int _numberOfPlayerSpawnedLocally = 0;
        
        private PlayerController _playerControllerA;
        private PlayerController _playerControllerB;
        private PlayerController _playerControllerC;
        private PlayerController _playerControllerD;
        

        public override void OnStartServer()
        {
            _realPlayerInfos.Clear();
            _playerTeamInfos.Clear();
            _playerReadyInfos.Clear();
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            StartCoroutine(TrySubscribeToGameManagerEvents());
        }

        private IEnumerator TrySubscribeToGameManagerEvents()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.OnAnyRoundStarted += OnAnyRoundStarted;
            GameManager.Instance.OnAnyRoundEnded += OnAnyRoundEnded;
        }

        private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                Logger.LogInfo("Remote client disconnected with id " + conn.ClientId, Logger.LogType.Server, this);
                _realPlayerInfos.Clear();
                _playerTeamInfos.Clear();
                _playerReadyInfos.Clear();
                OnRemoteClientDisconnected?.Invoke();
            }
        }

        public override void OnStopServer()
        {
            _realPlayerInfos.Clear();
            _playerTeamInfos.Clear();
            _playerReadyInfos.Clear();
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnAnyRoundStarted -= OnAnyRoundStarted;
                GameManager.Instance.OnAnyRoundEnded -= OnAnyRoundEnded;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _realPlayerInfos.OnChange += OnChangedRealPlayerInfos;
            _playerTeamInfos.OnChange += OnChangedPlayerTeamInfos;
            _playerReadyInfos.OnChange += OnChangedPlayersReadyInfos;
            _joinInputAction.performed += JoinInputActionPerformed;
            _goToRightTeamInputAction.performed += GoToRightTeamInputActionPerformed;
            _goToLeftTeamInputAction.performed += GoToLeftTeamInputActionPerformed;
            _leaveInputAction.performed += LeaveInputActionPerformed;
            _joinAndFullFakePlayerInputAction.performed += JoinAndFullFakePlayerInputActionOnPerformed;
            //_joinInputAction.Enable();
            //_leaveInputAction.Enable();
            //_goToRightTeamInputAction.Enable();
            //_goToLeftTeamInputAction.Enable();
            //_readyInputAction.Enable();
            //_cancelReadyInputAction.Enable();
            _joinAndFullFakePlayerInputAction.Enable();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _realPlayerInfos.OnChange -= OnChangedRealPlayerInfos;
            _playerTeamInfos.OnChange -= OnChangedPlayerTeamInfos;
            _playerReadyInfos.OnChange -= OnChangedPlayersReadyInfos;
            //_joinInputAction.Disable();
            //_leaveInputAction.Disable();
            //_goToRightTeamInputAction.Disable();
            //_goToLeftTeamInputAction.Disable();
            // _readyInputAction.Disable();
            // _cancelReadyInputAction.Enable();
            _joinAndFullFakePlayerInputAction.Disable();
            _joinInputAction.performed -= JoinInputActionPerformed;
            _leaveInputAction.performed -= LeaveInputActionPerformed;
            _readyInputAction.performed -= ConfirmTeamInputActionPerformed;
            _cancelReadyInputAction.performed -= CancelConfirmTeamInputActionPerformed;
            _goToRightTeamInputAction.performed -= GoToRightTeamInputActionPerformed;
            _goToLeftTeamInputAction.performed -= GoToLeftTeamInputActionPerformed;
            _joinAndFullFakePlayerInputAction.performed -= JoinAndFullFakePlayerInputActionOnPerformed;
        }


        private void OnAnyRoundStarted(byte _)
        {
            Logger.LogTrace("RoundStarted ! Activating Tongue usage for players", Logger.LogType.Server, this);
            CanPlayerUseTongue.Value = true;
        }
        
        private void OnAnyRoundEnded(byte _)
        {
           Logger.LogTrace("RoundEnded ! Deactivating Tongue usage for players", Logger.LogType.Server, this);
            CanPlayerUseTongue.Value = false;
        }
        
        private void OnChangedRealPlayerInfos(SyncListOperation op, int index, RealPlayerInfo oldItem, RealPlayerInfo newItem, bool asServer)
        {
            OnRealPlayerInfosChanged?.Invoke(_realPlayerInfos.Collection);
        }
        
        #region =========== Join and Leave Functions =========== 

        private void JoinAndFullFakePlayerInputActionOnPerformed(InputAction.CallbackContext context)
        {
            SetPlayerJoiningEnabled(true);
            JoinInputActionPerformed(context);
            AddFakePlayer();
            AddFakePlayer();
            AddFakePlayer();
            GameManager.Instance.TryStartGame();
        }


        public void SetPlayerJoiningEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerJoiningEnabled: " + value, context: this);
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
        public void SetPlayerJoiningEnabledClientRpc(bool value)
        {
            SetPlayerJoiningEnabled(value);
        }

        public void SetPlayerLeavingEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerLeavingEnabled: " + value, context: this);
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

        private void LeaveInputActionPerformed(InputAction.CallbackContext context)
        {
            // This method is always call locally
            var realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)LocalConnection.ClientId,
                DevicePath = context.control.device.path
            };
            Logger.LogTrace("LeaveInputActionPerformed with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                            realPlayerInfo.DevicePath + " received.", context: this);
            if (_realPlayerInfos.Count == 0)
            {
                // We know this player is not in the list since its empty
                Logger.LogTrace("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                                realPlayerInfo.DevicePath + " not in the list. Nothing to remove.", context: this);
                return;
            }

            for (int i = 0; i < _realPlayerInfos.Count; i++)
            {
                if (_realPlayerInfos[i].ClientId == realPlayerInfo.ClientId &&
                    _realPlayerInfos[i].DevicePath == realPlayerInfo.DevicePath)
                {
                    // This Real Player is in the list
                    Logger.LogTrace("RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " +
                                    realPlayerInfo.DevicePath + " found in the list. Removing it...", context: this);
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
                            realPlayerInfo.DevicePath + " not in the list. Nothing to remove.", context: this);
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
                      newRealPlayerInfo.DevicePath + " received.", context: this);
            if (_realPlayerInfos.Count == 0)
            {
                // We know this player is not in the list since its empty
                Logger.LogTrace("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                                newRealPlayerInfo.DevicePath + " not in the list. Adding it...", context: this);
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
                                    newRealPlayerInfo.DevicePath + " already in the list.", context: this);
                    return;
                }
            }

            // This Real Player is not in the list
            Logger.LogTrace("RealPlayer with clientId " + newRealPlayerInfo.ClientId + " and devicePath " +
                            newRealPlayerInfo.DevicePath + " not in the list. Adding it...", context: this);
            if (!IsServerStarted)
            {
                TryAddRealPlayerServerRpc(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
            }
            else
            {
                TryAddRealPlayer(newRealPlayerInfo.ClientId, newRealPlayerInfo.DevicePath);
            }
        }

        #endregion

        #region =========== StartTeamManagement Functions =========== 

        [Button(ButtonSizes.Medium)]
        public void TryStartTeamManagement()
        {
            if (_realPlayerInfos.Collection.Count != 4)
            {
                Logger.LogWarning("Not enough real players to start team management.", context: this);
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
            SetPlayerChangingTeamEnabledClientRpc(true);
            SetPlayerConfirmTeamEnabledClientRpc(true);
            SetPlayerJoiningEnabledClientRpc(false);

            List<PlayerTeamInfo> playerTeamInfos = new List<PlayerTeamInfo>();
            List<PlayerReadyInfo> playerReadyInfos = new List<PlayerReadyInfo>();

            if (_playerTeamInfos.Count < _realPlayerInfos.Count)
            {
                for (int i = 0; i < _realPlayerInfos.Count; i++)
                {
                    playerTeamInfos.Add(new PlayerTeamInfo
                    {
                        PlayerIndexType = _realPlayerInfos[i].PlayerIndexType,
                        PlayerTeamType = PlayerTeamType.Z
                    });
                    playerReadyInfos.Add(new PlayerReadyInfo
                    {
                        PlayerIndexType = _realPlayerInfos[i].PlayerIndexType,
                        IsPlayerReady = false
                    });

                }
                _playerTeamInfos.AddRange(playerTeamInfos);
                _playerReadyInfos.AddRange(playerReadyInfos);
                Logger.LogInfo("Team management started", Logger.LogType.Client, context: this);
                OnTeamManagementStartedTriggerClientRPC();
            }
            else
            {
                Logger.LogInfo("Team management already started", Logger.LogType.Client, context: this);
            }
        }

        [ObserversRpc]
        private void OnTeamManagementStartedTriggerClientRPC()
        {
            OnTeamManagementStarted?.Invoke();
        }
        
        public void MapPlayerTeams()
        {
            RealPlayerInfo[] tempRealPlayerInfos = new RealPlayerInfo[_realPlayerInfos.Count];
            PlayerReadyInfo[] tempPlayerReadyInfos = new PlayerReadyInfo[_playerReadyInfos.Count];
            List<RealPlayerInfo> tempListRealPlayerInfos = new List<RealPlayerInfo>();
            List<PlayerReadyInfo> tempListPlayerReadyInfos = new List<PlayerReadyInfo>();
            _realPlayerInfos.Collection.CopyTo(tempRealPlayerInfos);
            _playerReadyInfos.Collection.CopyTo(tempPlayerReadyInfos);
            for (int i = 0; i < tempRealPlayerInfos.Length; i++)
            {
                tempListRealPlayerInfos.Add(tempRealPlayerInfos[i]);
                tempListPlayerReadyInfos.Add(tempPlayerReadyInfos[i]);
            }
            // TODO: Map players to teams by using SwapRealPlayers method
        }

        private void SwapRealPlayers(PlayerIndexType player1, PlayerIndexType player2)
        {
            RealPlayerInfo realPlayer1 = _realPlayerInfos.Collection.First(x => x.PlayerIndexType == player1);
            RealPlayerInfo realPlayer2 = _realPlayerInfos.Collection.First(x => x.PlayerIndexType == player2);
            int index1 = _realPlayerInfos.IndexOf(realPlayer1);
            int index2 = _realPlayerInfos.IndexOf(realPlayer2);
            RealPlayerInfo copy1 = _realPlayerInfos[index1];
            RealPlayerInfo copy2 = _realPlayerInfos[index2];
            copy1.PlayerIndexType = player2;
            copy2.PlayerIndexType = player1;
            _realPlayerInfos[index1] = copy1;
            _realPlayerInfos[index2] = copy2;
        }
        
        #endregion

        #region ===========  Change Team Functions =========== 

        public void TrySetPlayerChangingTeamEnabled(bool value)
        {
            SetPlayerChangingTeamEnabledServerRpc(value);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerChangingTeamEnabledServerRpc(bool value)
        {
            SetPlayerChangingTeamEnabledClientRpc(value);
        }

        [ObserversRpc]
        private void SetPlayerChangingTeamEnabledClientRpc(bool value)
        {
            SetPlayerChangingTeamEnabled(value);
        }

        private void SetPlayerChangingTeamEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerChangingTeamEnabled: " + value, context: this);
            _canChangeTeam = value;
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


        [TargetRpc(ExcludeServer = false)]
        private void SetPlayerChangingTeamTargetRPC(NetworkConnection conn, bool value)
        {
            SetPlayerChangingTeamEnabled(value);
        }

        private void TryChangeTeam(InputAction.CallbackContext context, bool goToLeft)
        {
            //if (!_canChangeTeam)
            //{
            //    Logger.LogWarning("Can't change team yet, the variable _canChangeTeam is currently false, don't forget so start team management via TryStartTeamManagement()", context: this);
            //    return;
            //}
            // Reconstruct the RealPlayerInfo
            var realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)LocalConnection.ClientId,
                DevicePath = context.control.device.path
            };
            var exist = DoesRealPlayerExist(realPlayerInfo);
            if (!exist)
            {
                Logger.LogWarning("Can't change team for RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath + " as it does not exist.", context: this);
                return;
            }
            var playerIndexType = GetPlayerIndexTypeFromRealPlayerInfo(realPlayerInfo);

            if (IsPlayerReady(playerIndexType))
            {
                Logger.LogWarning($"Can't change team because player {playerIndexType} with id {realPlayerInfo.ClientId} is already ready", context: this);
                return;
            }

            ChangeTeamServerRpc(playerIndexType, goToLeft);
        }

        public void ForceChangeTeam(PlayerIndexType playerIndexType, bool goToLeft)
        {
            if (!_canChangeTeam)
            {
                Logger.LogWarning("Can't change team yet, the variable _canChangeTeam is currently false, don't forget so start team management via TryStartTeamManagement()", context: this);
                return;
            }

            ChangeTeamServerRpc(playerIndexType, goToLeft);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeTeamServerRpc(PlayerIndexType playerIndexType, bool goToLeft)
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


        private void OnChangedPlayerTeamInfos(SyncListOperation op, int index, PlayerTeamInfo oldItem, PlayerTeamInfo newItem, bool asServer)
        {
            OnPlayerTeamInfosChanged?.Invoke(_playerTeamInfos.Collection);
            if (op == SyncListOperation.Set)
            {
                // Debug the current teams in a single log
                string teams = "";
                foreach (PlayerTeamInfo playerTeamInfo in _playerTeamInfos.Collection)
                {
                    teams += "Player " + playerTeamInfo.PlayerIndexType + " : Team " + playerTeamInfo.PlayerTeamType + " | ";
                }
                Logger.LogTrace("Teams: " + teams, Logger.LogType.Server, this);
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

        #endregion

        #region =========== Confirm Team Functions =========== 

        [ObserversRpc]
        private void SetPlayerConfirmTeamEnabledClientRpc(bool value)
        {
            SetPlayerConfirmTeamEnabled(value);
        }

        [TargetRpc(ExcludeServer = false)]
        private void SetPlayerConfirmTeamEnabledTargetRpc(NetworkConnection conn, bool value)
        {
            SetPlayerConfirmTeamEnabled(value);
        }

        public void SetPlayerConfirmTeamEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerConfirmTeamEnabled: " + value, context: this);

            if (value)
            {
                _readyInputAction.Enable();
                _readyInputAction.performed += ConfirmTeamInputActionPerformed;

                _cancelReadyInputAction.Disable();
                _cancelReadyInputAction.performed -= CancelConfirmTeamInputActionPerformed;
            }
            else
            {
                _readyInputAction.performed -= ConfirmTeamInputActionPerformed;
                _readyInputAction.Disable();

                _cancelReadyInputAction.Enable();
                _cancelReadyInputAction.performed += CancelConfirmTeamInputActionPerformed;
            }
        }

        private void TryConfirmTeam(InputAction.CallbackContext context)
        {
            if (!_canChangeTeam)
            {
                Logger.LogWarning("Can't confirm team yet, the variable _canChangeTeam is currently false, don't forget so start team management via TryStartTeamManagement()", context: this);
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
                Logger.LogWarning("Can't change team for RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath + " as it does not exist.", context: this);
                return;
            }
            var playerIndexType = GetPlayerIndexTypeFromRealPlayerInfo(realPlayerInfo);
            ConfirmTeamServerRpc(playerIndexType);
        }


        [ServerRpc(RequireOwnership = false)]
        private void ConfirmTeamServerRpc(PlayerIndexType playerIndexType, NetworkConnection conn = null)
        {
            // Check if player is not already ready
            foreach (PlayerReadyInfo playerReadyInfo in _playerReadyInfos.Collection)
            {
                if (playerReadyInfo.PlayerIndexType == playerIndexType && playerReadyInfo.IsPlayerReady == true)
                {
                    Logger.LogWarning("Player " + playerIndexType + " is already ready " + playerReadyInfo.IsPlayerReady, Logger.LogType.Server, this);
                    return;
                }
            }

            // Find in which team player want to confirm
            PlayerTeamInfo confirmingTeam = new PlayerTeamInfo
            {
                PlayerTeamType = PlayerTeamType.Z,
                PlayerIndexType = playerIndexType
            };

            foreach (PlayerTeamInfo playerTeamInfo in _playerTeamInfos.Collection)
            {
                if (playerTeamInfo.PlayerIndexType == playerIndexType)
                {
                    confirmingTeam.PlayerTeamType = playerTeamInfo.PlayerTeamType;
                    break;
                }
            }

            // If player tried to confirm while not in a team, return
            if (confirmingTeam.PlayerTeamType == PlayerTeamType.Z)
            {
                Logger.LogWarning("Player " + playerIndexType + " didn't select a team " + confirmingTeam.PlayerTeamType, Logger.LogType.Server, this);
                return;
            }

            // Check if Team is made of 2 or less members 
            int numOfPlayersInThisTeam = 0;
            foreach (PlayerTeamInfo playerTeamInfo in _playerTeamInfos.Collection)
            {
                if (playerTeamInfo.PlayerTeamType == confirmingTeam.PlayerTeamType)
                {
                    numOfPlayersInThisTeam++;
                }
            }

            if (numOfPlayersInThisTeam > 2)
            {
                Logger.LogDebug("Player " + playerIndexType + " can't confirm joint team " + confirmingTeam + " because team is already full " + numOfPlayersInThisTeam, Logger.LogType.Server, this);
                return;
            }

            for (int i = _playerReadyInfos.Collection.Count - 1; i >= 0; i--)
            {
                if (_playerReadyInfos.Collection[i].PlayerIndexType == playerIndexType && !_playerReadyInfos.Collection[i].IsPlayerReady)
                {
                    PlayerReadyInfo copy = _playerReadyInfos.Collection[i];
                    copy.IsPlayerReady = true;
                    _playerReadyInfos[i] = copy;

                    break;
                }
            }

            Logger.LogDebug("Player " + playerIndexType + " confirmed being in team " + confirmingTeam, Logger.LogType.Server, this);

            //SetPlayerChangingTeamTargetRPC(conn, false);
            //SetPlayerConfirmTeamEnabledTargetRpc(conn, false);
        }

        public void TryCancelConfirmTeam(InputAction.CallbackContext context)
        {
            if (_canChangeTeam)
            {
                Logger.LogWarning("Can't quit team yet, the variable _canChangeTeam is currently true", context: this);
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
                Logger.LogWarning("Can't change team for RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath + " as it does not exist.", context: this);
                return;
            }
            var playerIndexType = GetPlayerIndexTypeFromRealPlayerInfo(realPlayerInfo);

            CancelConfirmTeamServerRpc(playerIndexType);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CancelConfirmTeamServerRpc(PlayerIndexType playerIndexType, NetworkConnection conn = null)
        {
            for (int i = _playerReadyInfos.Collection.Count - 1; i >= 0; i--)
            {
                if (_playerReadyInfos.Collection[i].PlayerIndexType == playerIndexType && _playerReadyInfos.Collection[i].IsPlayerReady)
                {
                    PlayerReadyInfo copy = _playerReadyInfos.Collection[i];
                    copy.IsPlayerReady = false;
                    _playerReadyInfos[i] = copy;

                    Logger.LogDebug("Player " + playerIndexType + " is no longer ready.", Logger.LogType.Server, this);

                    //SetPlayerChangingTeamTargetRPC(conn, true);
                    SetPlayerConfirmTeamEnabledTargetRpc(conn, true);
                    return;
                }
            }
            Logger.LogWarning("Player " + playerIndexType + " was not found in a team to remove him from.", Logger.LogType.Server, this);
        }

        private void OnChangedPlayersReadyInfos(SyncListOperation op, int index, PlayerReadyInfo oldItem, PlayerReadyInfo newItem, bool asServer)
        {
            OnPlayersReadyChanged?.Invoke(_playerReadyInfos.Collection);
            if (op == SyncListOperation.Set)
            {
                string readys = "";
                // Debug the current ready Players in a single log
                foreach (PlayerReadyInfo playerReadyInfo in _playerReadyInfos.Collection)
                {
                    readys += "Player " + playerReadyInfo.PlayerIndexType + " : Ready " + playerReadyInfo.IsPlayerReady + " | ";
                }
                Logger.LogTrace("Readys: " + readys, Logger.LogType.Server, this);
            }

            if (AllPlayerAreReady() && IsServerStarted)
            {
                OnAllPlayersReady?.Invoke();
            }
        }
        private bool AllPlayerAreReady()
        {
            bool res = true;
            foreach (var playerReadyInfo in _playerReadyInfos.Collection)
            {
                if (playerReadyInfo.IsPlayerReady != true)
                {
                    res = false;
                }
            }

            return res;
        }

        private bool IsPlayerReady(PlayerIndexType playerIndexType)
        {
            return _playerReadyInfos.Collection.Any(playerReadyInfo => playerReadyInfo.PlayerIndexType == playerIndexType && playerReadyInfo.IsPlayerReady);
        }

        private void ConfirmTeamInputActionPerformed(InputAction.CallbackContext context)
        {
            TryConfirmTeam(context);
        }

        private void CancelConfirmTeamInputActionPerformed(InputAction.CallbackContext context)
        {
            TryCancelConfirmTeam(context);
        }
        
        public void ReadyAllFakePlayers()
        {
            if (!IsServerStarted) return;
            StartCoroutine(ReadyAllFakePlayersCoroutine());
        }

        private IEnumerator ReadyAllFakePlayersCoroutine()
        {
            foreach (var realPlayerInfo in _realPlayerInfos.Collection)
            {
                if (realPlayerInfo.ClientId != 255) continue;
                int numberOfPlayersInTeamA = 0;
                int numberOfPlayersInTeamB = 0;
                foreach (var playerTeamInfo in _playerTeamInfos.Collection)
                {
                    if (playerTeamInfo.PlayerTeamType == PlayerTeamType.A)
                    {
                        numberOfPlayersInTeamA++;
                    }
                    else if (playerTeamInfo.PlayerTeamType == PlayerTeamType.B)
                    {
                        numberOfPlayersInTeamB++;
                    }
                }

                if (numberOfPlayersInTeamA != 2)
                {
                    ChangeTeamServerRpc(realPlayerInfo.PlayerIndexType, true);
                }
                else if (numberOfPlayersInTeamB != 2)
                {
                    ChangeTeamServerRpc(realPlayerInfo.PlayerIndexType, false);
                }
                else
                {
                    Logger.LogWarning("Cannot ready fake player " + realPlayerInfo.PlayerIndexType + " because both teams are full.", context: this);
                }
                ConfirmTeamServerRpc(realPlayerInfo.PlayerIndexType);
                yield return new WaitForSeconds(1f);
            }
        }

        #endregion


        public void TrySpawnPlayer()
        {
            if (!IsServerStarted)
            {
                Logger.LogTrace("TrySpawnPlayer request denied locally because not server, ignore this if you are a client-only player.", context: this);
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
            Logger.LogTrace("Attempting to spawn player...", Logger.LogType.Server, context: this);
            if (!IsServerStarted)
            {
                Logger.LogError("This method should only be called on the server, if you see this message, it's not normal.", context: this);
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
                Logger.LogTrace("Cannot add more than 4 players.", Logger.LogType.Server, context: this);
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
            Logger.LogTrace("+ RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " added. There are now " + _realPlayerInfos.Count + " players.", Logger.LogType.Server, context: this);
            // make sure there is no duplicate PlayerIndexType
            foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
            {
                Logger.LogTrace("PlayerIndexType: " + realPlayerInfo.PlayerIndexType + " for clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath, Logger.LogType.Server, context: this);
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
                    Logger.LogTrace("- RealPlayer with clientId " + clientId + " and devicePath " + devicePath + " removed. There are now " + _realPlayerInfos.Count + " players.", Logger.LogType.Server, context: this);
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
            Logger.LogTrace("Adding fake player with clientId " + fakePlayerInfo.ClientId + " and devicePath " + fakePlayerInfo.DevicePath, Logger.LogType.Server, context: this);
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
                Logger.LogError("This method should only be called on the server, if you see this message, it's not normal.", Logger.LogType.Server, context: this);
                return;
            }
            if (_realPlayerInfos.Count != 4)
            {
                Logger.LogWarning("Not enough real players to spawn all players.", Logger.LogType.Server, context: this);
                return;
            }
            _numberOfPlayerSpawnedLocally = 0;
            
            SetPlayerJoiningEnabledClientRpc(false);
            SetPlayerLeavingEnabledClientRpc(false);
            SetPlayerChangingTeamEnabledClientRpc(false);
            
            // We must change the player index type of real player info with the indexes of playerTeamInfos
            // we sort the _realPlayerInfos based
            
            foreach (RealPlayerInfo realPlayerInfo in _realPlayerInfos)
            {
                Logger.LogTrace("Spawning player for real player " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath, Logger.LogType.Server, context: this);
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
            
            _playerControllerA = GetNetworkPlayer(PlayerIndexType.A).GetPlayerController();
            _playerControllerB = GetNetworkPlayer(PlayerIndexType.B).GetPlayerController();
            _playerControllerC = GetNetworkPlayer(PlayerIndexType.C).GetPlayerController();
            _playerControllerD = GetNetworkPlayer(PlayerIndexType.D).GetPlayerController();
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
                Logger.LogError("Cannot unpossess a real player.", context: this);
                return;
            }
            //networkPlayer.RemoveOwnership();
            networkPlayer.GetComponent<PlayerController>().ClearInputProvider();
            OnRealPlayerUnpossessed?.Invoke(networkPlayer.GetRealPlayerInfo());
            Logger.LogDebug("Player " + playerIndexType + " unpossessed.", context: this);
        }

        public NetworkPlayer GetNetworkPlayer(PlayerIndexType playerIndexType)
        {
            return FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).ToList().Find(x => x.GetPlayerIndexType() == playerIndexType);
        }

        public List<RealPlayerInfo> GetRealPlayerInfos()
        {
            return _realPlayerInfos.Collection;
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

        private bool IsPlayerTeamsInfoValid()
        {
            // check that there is 4 players
            if (_playerTeamInfos.Count != 4)
            {
                Logger.LogWarning("PlayerTeamInfos count is not 4.", context: this);
                return false;
            }
            // check that there is no duplicate PlayerIndexType
            foreach (PlayerTeamInfo playerTeamInfo in _playerTeamInfos.Collection)
            {
                if (_playerTeamInfos.Collection.Count(x => x.PlayerIndexType == playerTeamInfo.PlayerIndexType) > 1)
                {
                    Logger.LogWarning("Duplicate PlayerIndexType " + playerTeamInfo.PlayerIndexType, context: this);
                    return false;
                }
            }
            // check that there is 2 players in each team
            if (_playerTeamInfos.Collection.Count(x => x.PlayerTeamType == PlayerTeamType.A) != 2)
            {
                Logger.LogWarning("Team A count is not 2.", context: this);
                return false;
            }
            if (_playerTeamInfos.Collection.Count(x => x.PlayerTeamType == PlayerTeamType.B) != 2)
            {
                Logger.LogWarning("Team B count is not 2.", context: this);
                return false;
            }
            return true;
        }

        [Server]
        public void SetVoodooPuppetDirection(PlayerIndexType playerIndexType, Vector2 direction)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    _playerControllerA.VoodooPuppetDirection.Value = direction;
                    break;
                case PlayerIndexType.B:
                    _playerControllerB.VoodooPuppetDirection.Value = direction;
                    break;
                case PlayerIndexType.C:
                    _playerControllerC.VoodooPuppetDirection.Value = direction;
                    break;
                case PlayerIndexType.D:
                    _playerControllerD.VoodooPuppetDirection.Value = direction;
                    break;
                case PlayerIndexType.Z:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerIndexType), playerIndexType, null);
            }
        }
    }
}