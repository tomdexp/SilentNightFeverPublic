﻿using System;
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
        private Rigidbody _rigidbody;
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
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("No Rigidbody found on PlayerController.");
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
            _rigidbody.isKinematic = false;
            // Very very simple movement, no character controller, no physics, just for testing
            var movementInput = _inputProvider.GetMovementInput();
            movementInput *= _networkPlayer.PlayerData.PlayerMovementSpeed;
            if (_playerStickyTongue.IsTongueBind())
            {
                // rotate the forward toward the tongue tip
                var direction = _playerStickyTongue.GetTongueTipPosition() - transform.position;
                transform.forward = direction.normalized;
            }
            else
            {
                if (movementInput.magnitude > 0.1f)
                {
                    transform.forward = new Vector3(movementInput.x, 0, movementInput.y);
                }
            }
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);
            _rigidbody.velocity = movement;
        }

        public void BindInputProvider(IInputProvider inputProvider)
        {
            // Clean the old input provider
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
                _inputProvider.OnActionInteractCanceled -= OnInteractCanceled;
            }
            
            // Bind the new input provider
            _inputProvider = inputProvider;
            _inputProvider.OnActionInteractPerformed += OnInteractPerformed;
            _inputProvider.OnActionInteractCanceled += OnInteractCanceled;
            
            Debug.Log("Bound input provider : " + _inputProvider.GetType().Name);
        }

        

        public void ClearInputProvider()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
                _inputProvider.OnActionInteractCanceled -= OnInteractCanceled;
                _inputProvider = null;
                Debug.Log("Cleared input provider.");
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Interact performed locally !");
            _playerStickyTongue.TryUseTongue();
        }
        
        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            Debug.Log("Interact performed locally !");
            _playerStickyTongue.TryRetractTongue();
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