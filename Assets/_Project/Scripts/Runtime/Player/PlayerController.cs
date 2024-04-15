using System;
using _Project.Scripts.Runtime.Inputs;
using _Project.Scripts.Runtime.Networking;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerController : NetworkBehaviour
    {
        private IInputProvider _inputProvider;
        private NetworkPlayer _networkPlayer;
        private Rigidbody _rigidbody;
        private PlayerStickyTongue _playerStickyTongue;
        private Quaternion _targetRotation;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private bool _canRotate = true;
        [SerializeField, ReadOnly] private bool _canMove = true;

        private void Awake()
        {
            var inputProvider = GetComponent<IInputProvider>();
            if (inputProvider != null)
            {
                BindInputProvider(inputProvider);
            }
            else
            {
                Logger.LogError("No input provider found on PlayerController.", context:this);
            }
            _networkPlayer = GetComponent<NetworkPlayer>();
            if (_networkPlayer == null)
            {
                Logger.LogError("No NetworkPlayer found on PlayerController.", context:this);
            }
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Logger.LogError("No Rigidbody found on PlayerController.", context:this);
            }
            _playerStickyTongue = GetComponentInChildren<PlayerStickyTongue>();
            if (_playerStickyTongue == null)
            {
                Logger.LogError("No PlayerStickyTongue found on PlayerController or its children.", context:this);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                _playerStickyTongue.OnTongueRetractStart += DisablePlayerRotation;
                _playerStickyTongue.OnTongueIn += EnablePlayerRotation;
            }
        }

        private void OnDestroy()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
            }

            if (IsOwner)
            {
                _playerStickyTongue.OnTongueRetractStart -= DisablePlayerRotation;
                _playerStickyTongue.OnTongueIn -= EnablePlayerRotation;
            }
            
        }

        private void FixedUpdate()
        {
            if (_inputProvider == null) return;
            if (!Owner.IsLocalClient) return;
            _rigidbody.isKinematic = false;
            
            var movementInput = _inputProvider.GetMovementInput();
            movementInput *= _networkPlayer.PlayerData.PlayerMovementSpeed;
            
            if (_playerStickyTongue.IsTongueBind())
            {
                // rotate the forward toward the tongue tip
                var direction = _playerStickyTongue.GetTongueTipPosition() - transform.position;
                direction.y = 0;
                _targetRotation = Quaternion.LookRotation(direction);
            }
            else
            {
                if (movementInput.magnitude > 0.1f)
                {
                    var direction = new Vector3(movementInput.x, 0, movementInput.y);
                    _targetRotation = Quaternion.LookRotation(direction);
                }
            }
            
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);
            
            if (_canMove)
            {
                _rigidbody.velocity = movement;
            }
            
            if (_canRotate)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * _networkPlayer.PlayerData.PlayerRotationSpeed);
            }
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
            
            Logger.LogDebug("Bound input provider : " + _inputProvider.GetType().Name, context:this);
        }

        public void ClearInputProvider()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
                _inputProvider.OnActionInteractCanceled -= OnInteractCanceled;
                _inputProvider = null;
                Logger.LogDebug("Cleared input provider.", context:this);
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            Logger.LogTrace("Interact performed locally !", context:this);
            _playerStickyTongue.TryUseTongue();
        }
        
        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            Logger.LogTrace("Interact canceled locally !", context:this);
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
                    if (cinemachineCamera)
                    {
                        // Bind the player controller to the Cinemachine Camera
                        cinemachineCamera.Follow = transform;
                        cinemachineCamera.LookAt = transform;
                        Logger.LogDebug("Bound player " + realPlayerInfo.PlayerIndexType + " to camera " + cinemachineCamera.name, context:this);
                    }
                    return;
                }
            }
        }
        
        private void DisablePlayerRotation()
        {
            Logger.LogTrace("DisablePlayerRotation", context:this);
            _canRotate = false;
        }
        
        private void EnablePlayerRotation()
        {
            Logger.LogTrace("EnablePlayerRotation", context:this);
            _canRotate = true;
        }
    }
}