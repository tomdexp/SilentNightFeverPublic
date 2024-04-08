using System;
using _Project.Scripts.Runtime.Inputs;
using _Project.Scripts.Runtime.Networking;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerController : NetworkBehaviour
    {
        private IInputProvider _inputProvider;
        private NetworkPlayer _networkPlayer;
        private CharacterController _characterController;
        private PlayerStickyTongue _playerStickyTongue;

        private void Awake()
        {
            var inputProvider = GetComponent<IInputProvider>();
            if (inputProvider != null)
            {
                BindInputProvider(inputProvider);
            }
            else
            {
                Debug.LogWarning("No input provider found on PlayerController.");
            }
            _networkPlayer = GetComponent<NetworkPlayer>();
            if (_networkPlayer == null)
            {
                Debug.LogError("No NetworkPlayer found on PlayerController.");
            }
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                Debug.LogError("No CharacterController found on PlayerController.");
            }
            _playerStickyTongue = GetComponentInChildren<PlayerStickyTongue>();
            if (_playerStickyTongue == null)
            {
                Debug.LogError("No PlayerStickyTongue found on PlayerController or its children.");
            }
        }
        
        private void OnDestroy()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
            }
        }

        private void Update()
        {
            if (_inputProvider == null) return;
            if (!Owner.IsLocalClient) return;
            _characterController.enabled = true;
            // Very very simple movement, no character controller, no physics, just for testing
            var movementInput = _inputProvider.GetMovementInput();
            movementInput *= _networkPlayer.PlayerData.PlayerMovementSpeed;
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);
            _characterController.Move(movement * Time.deltaTime);
            // rotate the foward to the movement direction based on the velocity
            if (movementInput.magnitude > 0.1f)
            {
                transform.forward = new Vector3(movementInput.x, 0, movementInput.y);
            }
        }

        public void BindInputProvider(IInputProvider inputProvider)
        {
            // Clean the old input provider
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
            }
            
            // Bind the new input provider
            _inputProvider = inputProvider;
            _inputProvider.OnActionInteractPerformed += OnInteractPerformed;
            
            Debug.Log("Bound input provider : " + _inputProvider.GetType().Name);
        }
        
        public void ClearInputProvider()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
                _inputProvider = null;
                Debug.Log("Cleared input provider.");
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Interact performed locally !");
            _playerStickyTongue.TryUseTongue();
        }

        public void SetRealPlayerInfo(RealPlayerInfo realPlayerInfo)
        {
            if (_inputProvider != null)
            {
                _inputProvider.SetRealPlayerInfo(realPlayerInfo);
            }
            BindToPlayerCamera(realPlayerInfo);
        }

        private void BindToPlayerCamera(RealPlayerInfo realPlayerInfo)
        {
            var playerCameras = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None);
            foreach (var playerCamera in playerCameras)
            {
                if (playerCamera.PlayerIndexType == realPlayerInfo.PlayerIndexType)
                {
                    // get the associated CinemachineCamera
                    var cinemachineCamera = playerCamera.GetComponent<CinemachineCamera>();
                    if (cinemachineCamera != null)
                    {
                        // Bind the player controller to the Cinemachine Camera
                        cinemachineCamera.Follow = transform;
                        cinemachineCamera.LookAt = transform;
                        Debug.Log("Bound player " + realPlayerInfo.PlayerIndexType + " to camera " + cinemachineCamera.name);
                    }
                    return;
                }
            }
        }
    }
}