﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Project.Scripts.Runtime.Inputs;
using _Project.Scripts.Runtime.Networking.Broadcast;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using _Project.Scripts.Runtime.UI.NetworkedMenu;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using QFSW.QC;
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
        [SerializeField] private InputAction _nextHatInputAction;
        [SerializeField] private InputAction _previousHatInputAction;
        [SerializeField] private InputAction _confirmHatInputAction;
        [SerializeField] private InputAction _cancelHatInputAction;
        [SerializeField] private InputAction _joinAndFullFakePlayerInputAction;
        [SerializeField] private float _changeTeamCooldownSeconds = 1f;
        private readonly SyncList<RealPlayerInfo> _realPlayerInfos = new SyncList<RealPlayerInfo>();
        private readonly SyncList<PlayerTeamInfo> _playerTeamInfos = new SyncList<PlayerTeamInfo>();
        private readonly SyncList<PlayerReadyInfo> _playerReadyInfos = new SyncList<PlayerReadyInfo>();
        private readonly SyncList<PlayerHatInfo> _playerHatInfos = new SyncList<PlayerHatInfo>();
        private bool _canChangeTeam = false;

        public readonly SyncVar<bool> CanPlayerUseTongue = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        public int NumberOfPlayers => _realPlayerInfos.Count;
        public bool AreAllPlayerSpawnedLocally => _numberOfPlayerSpawnedLocally == 4;
        public event Action<List<RealPlayerInfo>> OnRealPlayerInfosChanged;
        public event Action<List<PlayerReadyInfo>> OnPlayersReadyChanged;
        public event Action<List<PlayerTeamInfo>> OnPlayerTeamInfosChanged;
        public event Action<List<PlayerHatInfo>> OnPlayerHatInfosChanged;
        public event Action<RealPlayerInfo, RealPlayerInfo> OnRealPlayerPossessed; // source, target
        public event Action<RealPlayerInfo> OnRealPlayerUnpossessed;
        public event Action OnAllPlayerSpawnedLocally;
        public event Action OnRemoteClientDisconnected;
        public event Action OnTeamManagementStarted;
        public event Action OnCharacterCustomizationStarted;
        public event Action OnAllPlayersReady;
        public event Action OnAllPlayersConfirmedHat;

        private int _numberOfPlayerSpawnedLocally = 0;
        
        private PlayerController _playerControllerA;
        private PlayerController _playerControllerB;
        private PlayerController _playerControllerC;
        private PlayerController _playerControllerD;
        
        private bool _inputCooldownPlayerA;
        private bool _inputCooldownPlayerB;
        private bool _inputCooldownPlayerC;
        private bool _inputCooldownPlayerD;
        

        public override void OnStartServer()
        {
            _realPlayerInfos.Clear();
            _playerTeamInfos.Clear();
            _playerReadyInfos.Clear();
            _playerHatInfos.Clear();
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            StartCoroutine(TrySubscribeToGameManagerEvents());
            
            _realPlayerInfos.OnChange += BroadcastRealPlayerInfosChanged;
            _playerTeamInfos.OnChange += BroadcastPlayerTeamInfosChanged;
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
                _playerHatInfos.Clear();
                OnRemoteClientDisconnected?.Invoke();
            }
        }

        public override void OnStopServer()
        {
            _realPlayerInfos.OnChange -= BroadcastRealPlayerInfosChanged;
            _playerTeamInfos.OnChange -= BroadcastPlayerTeamInfosChanged;
            
            _realPlayerInfos.Clear();
            _playerTeamInfos.Clear();
            _playerReadyInfos.Clear();
            _playerHatInfos.Clear();
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
            _playerHatInfos.OnChange += OnChangedPlayerHatInfos;
            _joinInputAction.performed += JoinInputActionPerformed;
            _goToRightTeamInputAction.performed += GoToRightTeamInputActionPerformed;
            _goToLeftTeamInputAction.performed += GoToLeftTeamInputActionPerformed;
            _leaveInputAction.performed += LeaveInputActionPerformed;
            _joinAndFullFakePlayerInputAction.performed += JoinAndFullFakePlayerInputActionOnPerformed;
            _nextHatInputAction.performed += NextHatInputActionOnPerformed;
            _previousHatInputAction.performed += PreviousHatInputActionOnPerformed;
            _confirmHatInputAction.performed += ConfirmHatInputActionOnPerformed;
            _cancelHatInputAction.performed += CancelHatInputActionOnPerformed;

            //_joinInputAction.Enable();
            //_leaveInputAction.Enable();
            //_goToRightTeamInputAction.Enable();
            //_goToLeftTeamInputAction.Enable();
            //_readyInputAction.Enable();
            //_cancelReadyInputAction.Enable();
            //_confirmHatInputAction.Enable();
            //_cancelHatInputAction.Enable();
            //_nextHatInputAction.Enable();
            //_previousHatInputAction.Enable();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _joinAndFullFakePlayerInputAction.Enable();
#endif
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _realPlayerInfos.OnChange -= OnChangedRealPlayerInfos;
            _playerTeamInfos.OnChange -= OnChangedPlayerTeamInfos;
            _playerReadyInfos.OnChange -= OnChangedPlayersReadyInfos;

            _playerHatInfos.OnChange -= OnChangedPlayerHatInfos;

            //_joinInputAction.Disable();
            //_leaveInputAction.Disable();
            //_goToRightTeamInputAction.Disable();
            //_goToLeftTeamInputAction.Disable();
            // _readyInputAction.Disable();
            // _cancelReadyInputAction.Enable();
            // _confirmHatInputAction.Disable();
            // _cancelHatInputAction.Disable();
            //_nextHatInputAction.Disable();
            //_previousHatInputAction.Disable();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _joinAndFullFakePlayerInputAction.Disable();
#endif
            _joinInputAction.performed -= JoinInputActionPerformed;
            _leaveInputAction.performed -= LeaveInputActionPerformed;
            _readyInputAction.performed -= ConfirmTeamInputActionPerformed;
            _confirmHatInputAction.performed -= ConfirmHatInputActionOnPerformed;
            _cancelHatInputAction.performed -= CancelHatInputActionOnPerformed;
            _cancelReadyInputAction.performed -= CancelConfirmTeamInputActionPerformed;
            _goToRightTeamInputAction.performed -= GoToRightTeamInputActionPerformed;
            _goToLeftTeamInputAction.performed -= GoToLeftTeamInputActionPerformed;
            _joinAndFullFakePlayerInputAction.performed -= JoinAndFullFakePlayerInputActionOnPerformed;

            _nextHatInputAction.performed -= NextHatInputActionOnPerformed;
            _previousHatInputAction.performed -= PreviousHatInputActionOnPerformed;
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
            if (!IsServerStarted) return;
            if (!UIManager.HasInstance) // means we are directly in game scene or in a scene without UI
            {
                SetPlayerJoiningEnabled(true);
                JoinInputActionPerformed(context);
                AddFakePlayer();
                AddFakePlayer();
                AddFakePlayer();
                // check if we are in the onboarding scene or in the game scene
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentSceneName == "OnBoardingScene")
                {
                    GameManager.Instance.TryStartOnBoarding();
                }
                else
                {
                    GameManager.Instance.TryStartGame();
                }
            }
            else
            {
                // check if there are already 4 players
                if (_realPlayerInfos.Count != 4)
                {
                    SetPlayerJoiningEnabled(true);
                    JoinInputActionPerformed(context);
                    AddFakePlayer();
                    AddFakePlayer();
                    AddFakePlayer();
                    return;
                }
                // check to see if the teams are already confirmed (if not, at least 1 player is in team Z)
                if(_playerTeamInfos.Any(x => x.PlayerTeamType == PlayerTeamType.Z))
                {
                    ChangeTeamServerRpc(PlayerIndexType.A, true);
                    ConfirmTeamServerRpc(PlayerIndexType.A);
                    ReadyAllFakePlayers();
                }
                else // it means we are at the hat confirmation stage
                {
                    // find the non fake player (which client id is not 255)
                    RealPlayerInfo realPlayerInfo = _realPlayerInfos.Collection.First(x => x.ClientId != 255);
                    ConfirmHatServerRpc(realPlayerInfo.PlayerIndexType, true);
                    AllFakePlayersConfirmHat();
                }
            }
            
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
        public void SetPlayerLeavingEnabledClientRpc(bool value)
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
            SetPlayerJoiningEnabledClientRpc(false);
            SetPlayerChangingTeamEnabledClientRpc(true);
            SetPlayerConfirmTeamEnabledClientRpc(true);
            

            List<PlayerTeamInfo> playerTeamInfos = new List<PlayerTeamInfo>();
            List<PlayerReadyInfo> playerReadyInfos = new List<PlayerReadyInfo>();

            if (_playerTeamInfos.Count == 4)
            {
                Logger.LogInfo("Team management already started, resetting the players", Logger.LogType.Server, context: this);
                _playerTeamInfos.Clear();
                _playerReadyInfos.Clear();
            }
            
            if (_playerTeamInfos.Count < _realPlayerInfos.Count)
            {
                for (int i = 0; i < _realPlayerInfos.Count; i++)
                {
                    playerTeamInfos.Add(new PlayerTeamInfo
                    {
                        PlayerIndexType = _realPlayerInfos[i].PlayerIndexType,
                        PlayerTeamType = PlayerTeamType.Z,
                        ScreenPlayerIndexType = PlayerIndexType.Z
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
                Logger.LogInfo("Team management already started, resettings the players", Logger.LogType.Client, context: this);
            }
        }
        
        public void TryEndTeamManagement()
        {
            if (!IsServerStarted)
            {
                EndTeamManagementServerRpc();
            }
            else
            {
                EndTeamManagement();
            }
        }

        [ServerRpc(RequireOwnership = false)]    
        private void EndTeamManagementServerRpc()
        {
            EndTeamManagement();
        }

        private void EndTeamManagement()
        {
            SetPlayerLeavingEnabledClientRpc(false);
            SetPlayerJoiningEnabledClientRpc(false);
            SetPlayerChangingTeamEnabledClientRpc(false);
            SetPlayerConfirmTeamEnabledClientRpc(false);
            
        }
        
        public void ResumeTeamManagement()
        {
            SetPlayerLeavingEnabledClientRpc(false);
            SetPlayerJoiningEnabledClientRpc(false);
            SetPlayerChangingTeamEnabledClientRpc(true);
            SetPlayerConfirmTeamEnabledClientRpc(true);
        }

        [ObserversRpc]
        private void OnTeamManagementStartedTriggerClientRPC()
        {
            OnTeamManagementStarted?.Invoke();
        }
        
        public void MapPlayerTeams()
        {
            RealPlayerInfo[] tempRealPlayerInfos = new RealPlayerInfo[_realPlayerInfos.Count];
            PlayerTeamInfo[] tempPlayerTeamInfos = new PlayerTeamInfo[_playerReadyInfos.Count];
            List<PlayerTeamInfo> tempListPlayerTeamInfos = new List<PlayerTeamInfo>();
            _realPlayerInfos.Collection.CopyTo(tempRealPlayerInfos);
            _playerTeamInfos.Collection.CopyTo(tempPlayerTeamInfos);
            for (int i = 0; i < tempRealPlayerInfos.Length; i++)
            {
                tempListPlayerTeamInfos.Add(tempPlayerTeamInfos[i]);
            }
            PlayerIndexType[] teamA = new PlayerIndexType[2];
            PlayerIndexType[] teamB = new PlayerIndexType[2];
            teamA = tempListPlayerTeamInfos.Where(x => x.PlayerTeamType == PlayerTeamType.A).Select(x => x.PlayerIndexType).ToArray();
            teamB = tempListPlayerTeamInfos.Where(x => x.PlayerTeamType == PlayerTeamType.B).Select(x => x.PlayerIndexType).ToArray();
            RealPlayerInfo newPlayerA = GetRealPlayerInfoFromPlayerIndexType(teamA[0]);
            RealPlayerInfo newPlayerB = GetRealPlayerInfoFromPlayerIndexType(teamB[0]);
            RealPlayerInfo newPlayerC = GetRealPlayerInfoFromPlayerIndexType(teamA[1]);
            RealPlayerInfo newPlayerD = GetRealPlayerInfoFromPlayerIndexType(teamB[1]);
            newPlayerA.PlayerIndexType = PlayerIndexType.A;
            newPlayerB.PlayerIndexType = PlayerIndexType.B;
            newPlayerC.PlayerIndexType = PlayerIndexType.C;
            newPlayerD.PlayerIndexType = PlayerIndexType.D;
            List<RealPlayerInfo> newRealPlayerInfos = new List<RealPlayerInfo>
            {
                newPlayerA,
                newPlayerB,
                newPlayerC,
                newPlayerD
            };
            _realPlayerInfos.Clear();
            _realPlayerInfos.AddRange(newRealPlayerInfos);
        }
        
        private RealPlayerInfo GetRealPlayerInfoFromPlayerIndexType(PlayerIndexType playerIndexType)
        {
            return _realPlayerInfos.Collection.First(x => x.PlayerIndexType == playerIndexType);
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
            // Structures cannot have their values modified when they reside within a collection.
            // You must instead create a local variable for the collection index you wish to modify, change values on the local copy, then set the local copy back into the collection

            // Check if the input is in cooldown
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    if (_inputCooldownPlayerA) return;
                    break;
                case PlayerIndexType.B:
                    if (_inputCooldownPlayerB) return;
                    break;
                case PlayerIndexType.C:
                    if (_inputCooldownPlayerC) return;
                    break;
                case PlayerIndexType.D:
                    if (_inputCooldownPlayerD) return;
                    break;
                case PlayerIndexType.Z:
                    break;
            }

            StartCoroutine(InputCooldownCoroutine(playerIndexType));
            
            // Player can go in the team Z (middle), A(left) or B(right)
            PlayerTeamType previousTeam = _playerTeamInfos.Collection.First(x => x.PlayerIndexType == playerIndexType).PlayerTeamType;
            PlayerTeamType newTeam = PlayerTeamType.Z;
            if (goToLeft)
            {
                switch (previousTeam)
                {
                    case PlayerTeamType.A:
                        return;
                    case PlayerTeamType.B:
                        newTeam = PlayerTeamType.Z;
                        break;
                    case PlayerTeamType.Z:
                        newTeam = PlayerTeamType.A;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (previousTeam)
                {
                    case PlayerTeamType.A:
                        newTeam = PlayerTeamType.Z;
                        break;
                    case PlayerTeamType.B:
                        return;
                    case PlayerTeamType.Z:
                        newTeam = PlayerTeamType.B;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // Check if team is full (2 players max) and find first free screen slot, ignore if the Team is Z
            // Find the first free screen slot
            PlayerIndexType newScreenPlayerIndexType = PlayerIndexType.Z;
            if (newTeam != PlayerTeamType.Z)
            {
                int numOfPlayersInThisTeam = 0;
                foreach (PlayerTeamInfo teamInfo in _playerTeamInfos.Collection)
                {
                    if (teamInfo.PlayerTeamType == newTeam)
                    {
                        numOfPlayersInThisTeam++;
                    }
                }
                if (numOfPlayersInThisTeam >= 2)
                {
                    Logger.LogDebug("Player " + playerIndexType + " can't change team to " + newTeam + " because team is already full " + numOfPlayersInThisTeam, Logger.LogType.Server, this);
                    return;
                }
            
                // If there is no player in the team, assign the first screen slot
                if (numOfPlayersInThisTeam == 0 && newTeam == PlayerTeamType.A)
                {
                    newScreenPlayerIndexType = PlayerIndexType.A;
                }
                if (numOfPlayersInThisTeam == 0 && newTeam == PlayerTeamType.B)
                {
                    newScreenPlayerIndexType = PlayerIndexType.B;
                }
            
                // If there is already exactly 1 player, check its screen index and assign the other one
                if (numOfPlayersInThisTeam == 1)
                {
                    foreach (PlayerTeamInfo teamInfo in _playerTeamInfos.Collection)
                    {
                        if (teamInfo.PlayerTeamType == newTeam)
                        {
                            PlayerIndexType occupiedScreenPlayerIndexType = teamInfo.ScreenPlayerIndexType;
                            switch (occupiedScreenPlayerIndexType)
                            {
                                case PlayerIndexType.A:
                                    newScreenPlayerIndexType = PlayerIndexType.C;
                                    break;
                                case PlayerIndexType.B:
                                    newScreenPlayerIndexType = PlayerIndexType.D;
                                    break;
                                case PlayerIndexType.C:
                                    newScreenPlayerIndexType = PlayerIndexType.A;
                                    break;
                                case PlayerIndexType.D:
                                    newScreenPlayerIndexType = PlayerIndexType.B;
                                    break;
                                case PlayerIndexType.Z:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
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
            copy.ScreenPlayerIndexType = newScreenPlayerIndexType;
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
        public void SetPlayerConfirmTeamEnabledClientRpc(bool value)
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

                _cancelReadyInputAction.Enable();
                _cancelReadyInputAction.performed += CancelConfirmTeamInputActionPerformed;
            }
            else
            {
                _readyInputAction.performed -= ConfirmTeamInputActionPerformed;
                _readyInputAction.Disable();
                
                _cancelReadyInputAction.Disable();
                _cancelReadyInputAction.performed -= CancelConfirmTeamInputActionPerformed;
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
            if (!_canChangeTeam)
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
                
                // Verify is this is a "set" operation to avoid firing multiple times
                if (AllPlayerAreReady() && IsServerStarted && asServer)
                {
                    MapPlayerTeams();
                    OnAllPlayersReady?.Invoke();
                    var broadcast = new AllPlayerReadyBroadcast();
                    ServerManager.Broadcast(broadcast);
                }
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

        #region =========== Start Character Customization Functions ===========

        [Button(ButtonSizes.Medium)]
        public void TryStartCharacterCustomization()
        {
            if (_realPlayerInfos.Collection.Count != 4)
            {
                Logger.LogWarning("Not enough real players to start character customization.", context: this);
                return;
            }

            if (!IsServerStarted)
            {
                StartCharacterCustomizationServerRpc();
            }
            else
            {
                StartCharacterCustomization();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartCharacterCustomizationServerRpc()
        {
            StartCharacterCustomization();
        }

        private void StartCharacterCustomization()
        {
            SetPlayerChangingHatEnabledClientRpc(true);
            SetPlayerConfirmHatEnabledClientRpc(true);

            List<PlayerHatInfo> playerHatInfos = new List<PlayerHatInfo>();
            
            if (_playerHatInfos.Count == 4)
            {
                Logger.LogInfo("Character customization already started, reseting it", Logger.LogType.Client, context: this);
                _playerHatInfos.Clear();
            }

            if (playerHatInfos.Count < _realPlayerInfos.Count)
            {
                for (int i = 0; i < _realPlayerInfos.Count; i++)
                {
                    playerHatInfos.Add(new PlayerHatInfo
                    {
                        PlayerIndexType = _realPlayerInfos[i].PlayerIndexType,
                        PlayerHatType = HatType.None,
                        HasConfirmed = false
                    });
                }
                _playerHatInfos.AddRange(playerHatInfos);
                Logger.LogInfo("Character customization started", Logger.LogType.Client, context: this);
                OnCharacterCustomizationStartedTriggerClientRPC();
            }
        }
        
        public void TryStopCharacterCustomization()
        {
            if (!IsServerStarted)
            {
                StopCharacterCustomizationServerRpc();
            }
            else
            {
                StopCharacterCustomization();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopCharacterCustomizationServerRpc()
        {
            StopCharacterCustomization();
        }

        private void StopCharacterCustomization()
        {
            SetPlayerChangingHatEnabledClientRpc(false);
            SetPlayerConfirmHatEnabledClientRpc(false);
        }


        [ObserversRpc]
        private void OnCharacterCustomizationStartedTriggerClientRPC()
        {
            OnCharacterCustomizationStarted?.Invoke();
            Logger.LogInfo("Character customization event invoked", Logger.LogType.Client, context: this);
        }


        [ObserversRpc]
        public void SetPlayerChangingHatEnabledClientRpc(bool value)
        {
            SetPlayerChangingHatEnabled(value);
        }

        private void SetPlayerChangingHatEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerChangingHatEnabled: " + value, context: this);
            if (value)
            {
                _nextHatInputAction.Enable();
                _previousHatInputAction.Enable();
            }
            else
            {
                _nextHatInputAction.Disable();
                _previousHatInputAction.Disable();
            }
        }


        [ObserversRpc]
        public void SetPlayerConfirmHatEnabledClientRpc(bool value)
        {
            SetPlayerConfirmHatEnabled(value);
        }

        private void SetPlayerConfirmHatEnabled(bool value)
        {
            Logger.LogTrace("SetPlayerConfirmHatEnabled: " + value, context: this);
            if (value)
            {
                _confirmHatInputAction.Enable();
                _cancelHatInputAction.Enable();
            }
            else
            {
                _confirmHatInputAction.Disable();
                _cancelHatInputAction.Disable();
            }
        }


        #endregion

        #region =========== Change Character Customization Functions ===========
        private void TryChangeHat(InputAction.CallbackContext context, bool next)
        {
            // Reconstruct the RealPlayerInfo
            var realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)LocalConnection.ClientId,
                DevicePath = context.control.device.path
            };
            var exist = DoesRealPlayerExist(realPlayerInfo);
            if (!exist)
            {
                Logger.LogWarning("Can't change hat for RealPlayer with clientId " + realPlayerInfo.ClientId + " and devicePath " + realPlayerInfo.DevicePath + " as it does not exist.", context: this);
                return;
            }
            var playerIndexType = GetPlayerIndexTypeFromRealPlayerInfo(realPlayerInfo);

            if (HasPlayerConfirmedHat(playerIndexType))
            {
                Logger.LogWarning($"Can't change hat because player {playerIndexType} with id {realPlayerInfo.ClientId} is already ready", context: this);
                return;
            }

            ChangeHatServerRpc(playerIndexType, next);
        }


        [ServerRpc(RequireOwnership = false)]
        private void ChangeHatServerRpc(PlayerIndexType playerIndexType, bool next)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    if (_inputCooldownPlayerA) return;
                    break;
                case PlayerIndexType.B:
                    if (_inputCooldownPlayerB) return;
                    break;
                case PlayerIndexType.C:
                    if (_inputCooldownPlayerC) return;
                    break;
                case PlayerIndexType.D:
                    if (_inputCooldownPlayerD) return;
                    break;
                case PlayerIndexType.Z:
                    break;
            }
            
            // Get the current hat of the player
            var playerHatInfo = _playerHatInfos.Collection.First(x => x.PlayerIndexType == playerIndexType);
            var index = _playerHatInfos.IndexOf(playerHatInfo);
            HatType newHat = _playerHatInfos[index].PlayerHatType;
            int hatCount = Enum.GetValues(typeof(HatType)).Length;

            if (next)
            {
                newHat = (HatType) ((int)(newHat + 1) % hatCount);
            } else
            {
                newHat = (HatType) ((int)(newHat - 1 + hatCount) % hatCount);
            }

            PlayerHatInfo copy = _playerHatInfos[index];
            copy.PlayerHatType = newHat;

            _playerHatInfos[index] = copy;
            Logger.LogDebug("Player " + playerIndexType + " changed hat to " + newHat.ToString(), Logger.LogType.Server, this);
            
            StartCoroutine(InputCooldownCoroutine(playerIndexType));
        }

        private void OnChangedPlayerHatInfos(SyncListOperation op, int index, PlayerHatInfo oldItem, PlayerHatInfo newItem, bool asServer)
        {
            OnPlayerHatInfosChanged?.Invoke(_playerHatInfos.Collection);

            if (op == SyncListOperation.Set)
            {
                // Debug the current hats in a single log
                string hats = "";
                foreach (PlayerHatInfo playerHatInfo in _playerHatInfos.Collection)
                {
                    hats += "Player " + playerHatInfo.PlayerIndexType + " : Hat " + playerHatInfo.PlayerHatType.ToString() + " | ";
                }
                Logger.LogTrace("Hats: " + hats, Logger.LogType.Server, this);

                // Verify is this is a "set" operation to avoid firing multiple times
                if (AllPlayerConfirmedHat() && IsServerStarted && asServer)
                {
                    OnAllPlayersConfirmedHat?.Invoke();
                }
            }
        }

        private void NextHatInputActionOnPerformed(InputAction.CallbackContext obj)
        {
            TryChangeHat(obj, true);
        }

        private void PreviousHatInputActionOnPerformed(InputAction.CallbackContext obj)
        {
            TryChangeHat(obj, false);
        }


        #endregion

        #region =========== Confirm Customization Function ===========

        private void TryConfirmHat(InputAction.CallbackContext context, bool confirm)
        {
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

            if (HasPlayerConfirmedHat(playerIndexType) && confirm)
            {
                Logger.LogWarning($"Can't confirm hat because player {playerIndexType} with id {realPlayerInfo.ClientId} has already chosen a hat.", context: this);
                return;
            }
            if (!HasPlayerConfirmedHat(playerIndexType) && !confirm)
            {
                Logger.LogWarning($"Can't un-confirm hat because player {playerIndexType} with id {realPlayerInfo.ClientId} hasn't chose a hat.", context: this);
                return;
            }

            ConfirmHatServerRpc(playerIndexType, confirm);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConfirmHatServerRpc(PlayerIndexType playerIndexType, bool confirm)
        {
            // Get the player
            var playerHatInfo = _playerHatInfos.Collection.First(x => x.PlayerIndexType == playerIndexType);
            var index = _playerHatInfos.IndexOf(playerHatInfo);
            PlayerHatInfo copy = _playerHatInfos[index];
            copy.HasConfirmed = confirm;

            _playerHatInfos[index] = copy;
            Logger.LogDebug("Player " + playerIndexType + " confirmed hat state is : " + confirm.ToString(), Logger.LogType.Server, this);
        }


        private bool HasPlayerConfirmedHat(PlayerIndexType playerIndexType)
        {
            return _playerHatInfos.Collection.Any(playerHatInfo => playerHatInfo.PlayerIndexType == playerIndexType && playerHatInfo.HasConfirmed);
        }

        private bool AllPlayerConfirmedHat()
        {
            bool res = true;
            foreach (var playerHatInfo in _playerHatInfos.Collection)
            {
                if (playerHatInfo.HasConfirmed != true)
                {
                    return false;
                }
            }

            return res;
        }

        private void ConfirmHatInputActionOnPerformed(InputAction.CallbackContext obj)
        {
            TryConfirmHat(obj, true);
        }

        private void CancelHatInputActionOnPerformed(InputAction.CallbackContext obj)
        {
            TryConfirmHat(obj, false);
        }

        public void AllFakePlayersConfirmHat()
        {
            if (!IsServerStarted) return;
            foreach (var realPlayerInfo in _realPlayerInfos.Collection)
            {
                if (realPlayerInfo.ClientId != 255) continue;
                ConfirmHatServerRpc(realPlayerInfo.PlayerIndexType, true);
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

        public void ResetRealPlayerInfos()
        {
            _realPlayerInfos.Clear();
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
            if (_numberOfPlayerSpawnedLocally == 4)
            {
                Logger.LogDebug("Players already spawned. Which probably means the game manager is resetting", Logger.LogType.Server, context: this);
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
        
        public void ResetPlayerSpawnedLocally()
        {
            _numberOfPlayerSpawnedLocally = 0;
        }

        public void TeleportAllPlayerToOnBoardingSpawnPoints()
        {
            var spawnPoints = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None).ToList();
            if (spawnPoints.Count < 4)
            {
                Logger.LogError("Not enough spawn points to teleport all players.", Logger.LogType.Server, context: this);
                return;
            }
            
            // order the spawn points by Index
            spawnPoints = spawnPoints.OrderBy(x => x.PlayerIndexType).ToList();
            
            GetNetworkPlayer(PlayerIndexType.A).Teleport(spawnPoints[0].SpawnPoint.transform.position);
            GetNetworkPlayer(PlayerIndexType.B).Teleport(spawnPoints[1].SpawnPoint.transform.position);
            GetNetworkPlayer(PlayerIndexType.C).Teleport(spawnPoints[2].SpawnPoint.transform.position);
            GetNetworkPlayer(PlayerIndexType.D).Teleport(spawnPoints[3].SpawnPoint.transform.position);
        }

        [Button]
        public void ForceAllPlayersCameraAngle(float angle)
        {
            var playerCameras = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None).ToList();
            foreach (var camera in playerCameras)
            {
                camera.ForceCameraAngle(angle);
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
            // WARNING NETWORKING : Only the host can possess a fake player
            var console = FindAnyObjectByType<QuantumConsole>();
            if (console && console.IsActive) return;
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
            var console = FindAnyObjectByType<QuantumConsole>();
            if (console && console.IsActive) return;
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
        
        public List<PlayerReadyInfo> GetPlayerReadyInfos()
        {
            return _playerReadyInfos.Collection;
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
        
        private void BroadcastRealPlayerInfosChanged(SyncListOperation op, int index, RealPlayerInfo oldItem, RealPlayerInfo newItem, bool asServer)
        {
            var broadcast = new RealPlayersInfoChangedBroadcast();
            ServerManager.Broadcast(broadcast);
        }
        
        private void BroadcastPlayerTeamInfosChanged(SyncListOperation op, int index, PlayerTeamInfo oldItem, PlayerTeamInfo newItem, bool asServer)
        {
            var broadcast = new PlayerTeamInfosChangedBroadcast();
            ServerManager.Broadcast(broadcast);
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

        public List<PlayerTeamInfo> GetPlayerTeamInfos()
        {
            return _playerTeamInfos.Collection;
        }
        
        public List<PlayerHatInfo> GetPlayerHatInfos()
        {
            return _playerHatInfos.Collection;
        }
        
        private IEnumerator InputCooldownCoroutine(PlayerIndexType playerIndexType)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    if (_inputCooldownPlayerA) yield break;
                    break;
                case PlayerIndexType.B:
                    if (_inputCooldownPlayerB) yield break;
                    break;
                case PlayerIndexType.C:
                    if (_inputCooldownPlayerC) yield break;
                    break;
                case PlayerIndexType.D:
                    if (_inputCooldownPlayerD) yield break;
                    break;
                case PlayerIndexType.Z:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerIndexType), playerIndexType, null);
            }
            
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    _inputCooldownPlayerA = true;
                    break;
                case PlayerIndexType.B:
                    _inputCooldownPlayerB = true;
                    break;
                case PlayerIndexType.C:
                    _inputCooldownPlayerC = true;
                    break;
                case PlayerIndexType.D:
                    _inputCooldownPlayerD = true;
                    break;
                case PlayerIndexType.Z:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerIndexType), playerIndexType, null);
            }
            yield return new WaitForSeconds(_changeTeamCooldownSeconds);
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    _inputCooldownPlayerA = false;
                    break;
                case PlayerIndexType.B:
                    _inputCooldownPlayerB = false;
                    break;
                case PlayerIndexType.C:
                    _inputCooldownPlayerC = false;
                    break;
                case PlayerIndexType.D:
                    _inputCooldownPlayerD = false;
                    break;
                case PlayerIndexType.Z:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerIndexType), playerIndexType, null);
            }
        }

        public void DisablePlayerInputs(byte clientId)
        {
            // disable all player inputs of a client
            foreach (var networkPlayer in GetNetworkPlayers())
            {
                if (networkPlayer.GetRealPlayerInfo().ClientId == clientId)
                {
                    networkPlayer.DisableInputs();
                }
            }
        }
        
        public void EnablePlayerInputs(byte clientId)
        {
            // enable all player inputs of a client
            foreach (var networkPlayer in GetNetworkPlayers())
            {
                if (networkPlayer.GetRealPlayerInfo().ClientId == clientId)
                {
                    networkPlayer.EnableInputs();
                }
            }
        }
        
        public void LogPlayerPositionsAndDistance()
        {
            var teamADistance = Vector3.Distance(_playerControllerA.transform.position, _playerControllerC.transform.position);
            var teamBDistance = Vector3.Distance(_playerControllerB.transform.position, _playerControllerD.transform.position);
            var procGen = FindAnyObjectByType<ProcGenInstanciator>();
            if (!procGen)
            {
                Logger.LogDebug("ProcGenInstanciator not found will not log player positions compared to their teleportation points", Logger.LogType.Server, this);
                return;
            }
            Vector3 playerATeleportPointLocation = new Vector3(procGen._teamAPoints[0].x, 0, procGen._teamAPoints[0].y);
            Vector3 playerBTeleportPointLocation = new Vector3(procGen._teamBPoints[0].x, 0, procGen._teamBPoints[0].y);
            Vector3 playerCTeleportPointLocation = new Vector3(procGen._teamAPoints[1].x, 0, procGen._teamAPoints[1].y);
            Vector3 playerDTeleportPointLocation = new Vector3(procGen._teamBPoints[1].x, 0, procGen._teamBPoints[1].y);
            var playerADistanceToTeleportPoint = Vector3.Distance(_playerControllerA.transform.position, playerATeleportPointLocation);
            var playerBDistanceToTeleportPoint = Vector3.Distance(_playerControllerB.transform.position, playerBTeleportPointLocation);
            var playerCDistanceToTeleportPoint = Vector3.Distance(_playerControllerC.transform.position, playerCTeleportPointLocation);
            var playerDDistanceToTeleportPoint = Vector3.Distance(_playerControllerD.transform.position, playerDTeleportPointLocation);
            Logger.LogDebug("Team A distance: " + teamADistance + " | Team B distance: " + teamBDistance, Logger.LogType.Server, this);
            Logger.LogDebug("Player A teleport point: " + playerATeleportPointLocation, Logger.LogType.Server, this);
            Logger.LogDebug("Player B teleport point: " + playerBTeleportPointLocation, Logger.LogType.Server, this);
            Logger.LogDebug("Player C teleport point: " + playerCTeleportPointLocation, Logger.LogType.Server, this);
            Logger.LogDebug("Player D teleport point: " + playerDTeleportPointLocation, Logger.LogType.Server, this);
            Logger.LogDebug("Player A position: " + _playerControllerA.transform.position, Logger.LogType.Server, this);
            Logger.LogDebug("Player B position: " + _playerControllerB.transform.position, Logger.LogType.Server, this);
            Logger.LogDebug("Player C position: " + _playerControllerC.transform.position, Logger.LogType.Server, this);
            Logger.LogDebug("Player D position: " + _playerControllerD.transform.position, Logger.LogType.Server, this);
            Logger.LogDebug("Player A distance to teleport point: " + playerADistanceToTeleportPoint, Logger.LogType.Server, this);
            Logger.LogDebug("Player B distance to teleport point: " + playerBDistanceToTeleportPoint, Logger.LogType.Server, this);
            Logger.LogDebug("Player C distance to teleport point: " + playerCDistanceToTeleportPoint, Logger.LogType.Server, this);
            Logger.LogDebug("Player D distance to teleport point: " + playerDDistanceToTeleportPoint, Logger.LogType.Server, this);
        }
    }
}