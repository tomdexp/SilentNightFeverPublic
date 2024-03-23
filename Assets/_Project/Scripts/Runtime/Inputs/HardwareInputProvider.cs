using System;
using _Project.Scripts.Runtime.Player;
using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

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
        
        private PlayerInputActions _inputActions;
        private Vector2 _movementInput;
        private RealPlayerInfo _playerInfo;
        
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
                Debug.LogError("No input device found with name: " + deviceName);
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
            _inputActions.Enable();
            Debug.Log("Bound input provider with device: " + deviceName + " for clientID: " + _playerInfo.ClientId);
        }
    }
}