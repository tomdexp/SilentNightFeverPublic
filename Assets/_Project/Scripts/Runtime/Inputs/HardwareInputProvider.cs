using System;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using QFSW.QC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Users;
using Logger = _Project.Scripts.Runtime.Utils.Logger;
using PlayerController = _Project.Scripts.Runtime.Player.PlayerController;

namespace _Project.Scripts.Runtime.Inputs
{
    /// <summary>
    /// This class implement the IInputProvider interface and is used to provide input to the game
    /// Its meant to handle real keyboard and gamepad
    /// </summary>
    
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public class HardwareInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<InputAction.CallbackContext> OnActionInteractStarted;
        public event Action<InputAction.CallbackContext> OnActionInteractPerformed;
        public event Action<InputAction.CallbackContext> OnActionInteractCanceled;
        public event Action OnActionPausePerformed;

        private PlayerInputActions _inputActions;
        private Vector2 _movementInput;
        private RealPlayerInfo _playerInfo;
        private RealPlayerInfo _currentPossessedPlayer;
        
        private void OnDestroy()
        {
            if (_inputActions == null) return;
            _inputActions.Disable();
            _inputActions.Player.Interact.started -= OnInteractInputActionStarted;
            _inputActions.Player.Interact.performed -= OnInteractInputActionPerformed;
            _inputActions.Player.Interact.canceled -= OnInteractInputActionCanceled;
            _inputActions.Player.Move.performed -= OnMoveInputAction;
            _inputActions.Player.Move.canceled -= OnMoveInputAction;
            _inputActions.Dispose();
        }

        private void OnInteractInputActionStarted(InputAction.CallbackContext context)
        {
            if (InstanceFinder.ClientManager.Connection.ClientId != _playerInfo.ClientId) return;
            if (context.control.device.path != _playerInfo.DevicePath) return;
            OnActionInteractStarted?.Invoke(context);
        }

        private void OnInteractInputActionPerformed(InputAction.CallbackContext context)
        {
            if (InstanceFinder.ClientManager.Connection.ClientId != _playerInfo.ClientId) return;
            if (context.control.device.path != _playerInfo.DevicePath) return;
            OnActionInteractPerformed?.Invoke(context);
        }

        private void OnInteractInputActionCanceled(InputAction.CallbackContext context)
        {
            if (InstanceFinder.ClientManager.Connection.ClientId != _playerInfo.ClientId) return;
            if (context.control.device.path != _playerInfo.DevicePath) return;
            OnActionInteractCanceled?.Invoke(context);
        }

        private void OnMoveInputAction(InputAction.CallbackContext context)
        {
            if (InstanceFinder.ClientManager.Connection.ClientId != _playerInfo.ClientId) return;
            if (context.control.device.path != _playerInfo.DevicePath) return;
            _movementInput = context.ReadValue<Vector2>();
        }
        
        public void SetRealPlayerInfo(RealPlayerInfo playerInfo)
        { 
            _playerInfo = playerInfo;
            BindWithRealPlayerInfo();
        }

        public void DisableInput()
        {
            if (_inputActions != null)
            {
                _inputActions.Player.Move.Disable();
                _inputActions.Player.Interact.Disable();
                Logger.LogTrace("Disabled input provider for clientID: " + _playerInfo.ClientId, context:this);
            }
        }

        public void EnableInput()
        {
            if (_inputActions != null)
            {
                _inputActions.Player.Move.Enable();
                _inputActions.Player.Interact.Enable();
                Logger.LogTrace("Enabled input provider for clientID: " + _playerInfo.ClientId, context:this);
            }
        }

        public Vector2 GetMovementInput()
        {
            return _movementInput;
        }
        
        private void BindWithRealPlayerInfo()
        {
            // We should only bind the input provider if the client id is the same as the player id
            if (InstanceFinder.ClientManager.Connection.ClientId != _playerInfo.ClientId) return; 
            
            // Remove the '/' at the beginning of the device path to get the device name
            var deviceName = _playerInfo.DevicePath.Substring(1);
            
            InputDevice inputDevice = InputSystem.GetDevice(deviceName);
            if (inputDevice == null)
            {
                Logger.LogError("No input device found with name: " + deviceName, context:this);
                return;
            }
            
            if (_inputActions != null)
            {
                _inputActions.Disable();
                _inputActions.Dispose();
            }
            _inputActions = new PlayerInputActions();
            InputUser newUser = InputUser.PerformPairingWithDevice(inputDevice);
            newUser.AssociateActionsWithUser(_inputActions);
            _inputActions.Player.Interact.started += OnInteractInputActionStarted;
            _inputActions.Player.Interact.performed += OnInteractInputActionPerformed;
            _inputActions.Player.Interact.canceled += OnInteractInputActionCanceled;
            _inputActions.Player.Move.performed += OnMoveInputAction;
            _inputActions.Player.Move.canceled += OnMoveInputAction;
            _inputActions.Player.Pause.performed += OnPauseInputAction;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _inputActions.Player.DebugPossess.performed += OnDebugPossess;
#endif
            _inputActions.Enable();
            Logger.LogTrace("Bound input provider with device: " + deviceName + " for clientID: " + _playerInfo.ClientId, context:this);
        }

        private void OnPauseInputAction(InputAction.CallbackContext context)
        {
            OnActionPausePerformed?.Invoke();
        }

        private void OnDebugPossess(InputAction.CallbackContext context)
        {
            if(!InstanceFinder.IsServerStarted) return; // only the host can possess players
            var console = FindAnyObjectByType<QuantumConsole>();
            if (console && console.IsActive) return;
            // check if hold or press interact
            if (context.interaction is HoldInteraction)
            {
                // UNPOSSSESS
                if (_currentPossessedPlayer.ClientId == 255)
                {
                    PlayerManager.Instance.TryUnpossessPlayer(_currentPossessedPlayer.PlayerIndexType);
                    GetComponent<PlayerController>().BindInputProvider(this);
                    _currentPossessedPlayer = new RealPlayerInfo();
                }
            }
            else if (context.interaction is PressInteraction)
            {
                // POSSESS
                List<RealPlayerInfo> realPlayerInfos = PlayerManager.Instance.GetRealPlayerInfos();
                RealPlayerInfo nextPlayer = new RealPlayerInfo();
                bool foundNextPlayer = false;
                foreach (var realPlayerInfo in realPlayerInfos)
                {
                    if (realPlayerInfo.ClientId == 255 &&
                        realPlayerInfo.PlayerIndexType != _currentPossessedPlayer.PlayerIndexType)
                    {
                        // Cycle through all fake players
                        if (realPlayerInfo.PlayerIndexType > _currentPossessedPlayer.PlayerIndexType)
                        {
                            nextPlayer = realPlayerInfo;
                            foundNextPlayer = true;
                            break;
                        }
                    }
                }

                if (!foundNextPlayer)
                {
                    if (_currentPossessedPlayer.ClientId == 255)
                    {
                        // If no other fake player is found and we are currently possessing a player, unpossess
                        PlayerManager.Instance.TryUnpossessPlayer(_currentPossessedPlayer.PlayerIndexType);
                        GetComponent<PlayerController>().BindInputProvider(this);
                        _currentPossessedPlayer = new RealPlayerInfo();
                    }
                    else
                    {
                        Logger.LogTrace("No fake player found to possess", context:this);
                    }

                    return;
                }
                // Unpossess the current player
                if (_currentPossessedPlayer.ClientId == 255)
                {
                    PlayerManager.Instance.TryUnpossessPlayer(_currentPossessedPlayer.PlayerIndexType);
                }
                PlayerManager.Instance.TryPossessPlayer(_playerInfo.PlayerIndexType, nextPlayer.PlayerIndexType);
                _currentPossessedPlayer = nextPlayer;
            }
        }
        public RealPlayerInfo GetRealPlayerInfo()
        {
            return _playerInfo;
        }
    }
    
}