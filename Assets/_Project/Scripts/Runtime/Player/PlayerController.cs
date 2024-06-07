using System;
using System.Collections;
using _Project.Scripts.Runtime.Inputs;
using _Project.Scripts.Runtime.Landmarks.Kitchen;
using _Project.Scripts.Runtime.Landmarks.Voodoo;
using _Project.Scripts.Runtime.Landmarks.Zoom;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using _Project.Scripts.Runtime.UI;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private TongueAnchor _characterTongueAnchor;
        [SerializeField, Required] private Collider _playerCollider;
        [SerializeField, Required] private Transform _cameraFollowTarget;
        [SerializeField, Required] private Transform _cameraLookAtTarget;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private bool _canRotate = true;
        [SerializeField, ReadOnly] private bool _canMove = true;
        [SerializeField, ReadOnly] private PlayerStickyTongue _otherPlayerAttachedFromTongue;
        [SerializeField, ReadOnly] private NetworkPlayer _otherAttachedNetworkPlayer;
        [SerializeField, ReadOnly] private float _distanceToAttachedPlayer;
        [SerializeField, ReadOnly] private bool _influencedByAttachedTongue;
        
        private IInputProvider _inputProvider;
        private NetworkPlayer _networkPlayer;
        private Rigidbody _rigidbody;
        private PlayerStickyTongue _playerStickyTongue;
        private Quaternion _targetRotation;
        private PlayerCamera _playerCamera;
        private NetworkTransform _networkTransform;
        
        public readonly SyncVar<Vector2> VoodooPuppetDirection = new SyncVar<Vector2>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

        public event Action OnPlayerSpawnedLocally;
        
        private void Awake()
        {
            var inputProvider = GetComponent<IInputProvider>();
            if (inputProvider != null)
            {
                BindInputProvider(inputProvider);
            }
            else
            {
                Logger.LogError("No input provider found on PlayerController", context:this);
            }
            _networkPlayer = GetComponent<NetworkPlayer>();
            if (!_networkPlayer)
            {
                Logger.LogError("No NetworkPlayer found on PlayerController", context:this);
            }
            _rigidbody = GetComponent<Rigidbody>();
            if (!_rigidbody)
            {
                Logger.LogError("No Rigidbody found on PlayerController", context:this);
            }
            _playerStickyTongue = GetComponentInChildren<PlayerStickyTongue>();
            if (!_playerStickyTongue)
            {
                Logger.LogError("No PlayerStickyTongue found on PlayerController or its children", context:this);
            }
            if (!_characterTongueAnchor)
            {
                Logger.LogError("No CharacterTongueAnchor set on PlayerController", context:this);
            }
            if (!_playerCollider)
            {
                Logger.LogError("No PlayerCollider set on PlayerController", context:this);
            }
            _networkTransform = GetComponent<NetworkTransform>();
            if (!_networkTransform)
            {
                Logger.LogError("No NetworkTransform found on PlayerController", context:this);
            }
            if (!_cameraFollowTarget)
            {
                Logger.LogError("No CameraFollowTarget set on PlayerController", context:this);
            }
            if (!_cameraLookAtTarget)
            {
                Logger.LogError("No CameraLookAtTarget set on PlayerController", context:this);
            }
        }
        
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            // if previous owner was server, we know the object just spawned
            if (prevOwner.ClientId == -1)
            {
                Logger.LogInfo("Previous owner was Server, PlayerController just spawned", Logger.LogType.Client, this);
                if (IsOwner)
                {
                    _playerStickyTongue.OnTongueRetractStart += DisablePlayerRotation;
                    _playerStickyTongue.OnTongueIn += EnablePlayerRotation;
                    _characterTongueAnchor.OnTongueBindChange += OnTongueBindChange;
                    _playerStickyTongue.ResetTongue();
                    TriggerOnPlayerReadyLocally();
                }
            }
        }

        [ServerRpc(RunLocally = true)]
        private void TriggerOnPlayerReadyLocally()
        {
            Logger.LogTrace("TriggerOnPlayerReadyLocally", Logger.LogType.Server,this);
            OnPlayerSpawnedLocally?.Invoke();
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
                _characterTongueAnchor.OnTongueBindChange -= OnTongueBindChange;
            }
        }

        private void OnTongueBindChange(PlayerStickyTongue tongue)
        {
            Logger.LogTrace("Tongue bind change", context:this);
            if (tongue)
            {
                // Tongue bind
                _otherPlayerAttachedFromTongue = tongue;
                _otherAttachedNetworkPlayer = tongue.GetComponentInParent<NetworkPlayer>();
            }
            else
            {
                // Tongue unbind
                _otherPlayerAttachedFromTongue = null;
                _otherAttachedNetworkPlayer = null;
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
                    if (_playerCamera)
                    {
                        direction.x = -(Mathf.Cos(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.x - Mathf.Sin(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.y);
                        direction.z = -(Mathf.Sin(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.x + Mathf.Cos(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.y);
                    }
                    _targetRotation = Quaternion.LookRotation(direction.normalized);
                }
            }

            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);
            if (_playerCamera)
            {
                movement.x = -(Mathf.Cos(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.x - Mathf.Sin(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.y);
                movement.z = -(Mathf.Sin(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.x + Mathf.Cos(_playerCamera.CameraAngle - (float)Math.PI / 2) * movementInput.y);
            }

            if (_otherPlayerAttachedFromTongue)
            {
                if (_otherPlayerAttachedFromTongue.GetNetworkPlayer().IsOnline)
                {
                    _distanceToAttachedPlayer =
                        Vector3.Distance(transform.position, _otherPlayerAttachedFromTongue.transform.position);
                    _influencedByAttachedTongue =
                        _distanceToAttachedPlayer > _networkPlayer.PlayerData.OtherTongueMinDistance;
                    if (_influencedByAttachedTongue)
                    {
                        var direction = _otherPlayerAttachedFromTongue.transform.position - transform.position;
                        direction.y = 0;
                        direction.Normalize();
                        // add the direction to the movement
                        movement += direction * _networkPlayer.PlayerData.OtherTongueAttachedForce;
                    }
                }
                else
                {
                    _influencedByAttachedTongue = false;
                    _distanceToAttachedPlayer = 0;
                }
            }
            else
            {
                _influencedByAttachedTongue = false;
                _distanceToAttachedPlayer = 0;
            }
            
            
            if(VoodooPuppetDirection.Value != Vector2.zero)
            {
                var direction = new Vector3(VoodooPuppetDirection.Value.x, 0, VoodooPuppetDirection.Value.y);
                movement += direction * _networkPlayer.PlayerData.LandmarkData_Voodoo.ForcedMovementInfluenceFactor;
            }
            
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
            ClearInputProvider();
            
            // Bind the new input provider
            _inputProvider = inputProvider;
            _inputProvider.OnActionInteractPerformed += OnInteractPerformed;
            _inputProvider.OnActionInteractCanceled += OnInteractCanceled;

            Logger.LogDebug("Bound input provider : " + _inputProvider.GetType().Name, context: this);
        }

        public void ClearInputProvider()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
                _inputProvider.OnActionInteractCanceled -= OnInteractCanceled;
                _inputProvider = null;
                Logger.LogDebug("Cleared input provider.", context: this);
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            Logger.LogTrace("Interact performed locally !", context: this);
            if (!PlayerManager.Instance.CanPlayerUseTongue.Value)
            {
                Logger.LogDebug($"Player {_networkPlayer.GetPlayerIndexType()} can't use tongue because PlayerManager forbids it", Logger.LogType.Client, this);
                return;
            }
            _playerStickyTongue.TryUseTongue();
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            Logger.LogTrace("Interact canceled locally !", context: this);
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
                    _playerCamera = playerCamera;
                    // get the associated CinemachineCamera
                    var cinemachineCamera = playerCamera.GetComponent<CinemachineCamera>();
                    if (cinemachineCamera)
                    {
                        // Bind the player controller to the Cinemachine Camera
                        cinemachineCamera.Follow = _cameraFollowTarget;
                        cinemachineCamera.LookAt = _cameraLookAtTarget;
                        Logger.LogDebug("Bound player " + realPlayerInfo.PlayerIndexType + " to camera " + cinemachineCamera.name, context: this);
                    }
                    return;
                }
            }
        }

        public void Teleport(Transform tr)
        {
            Teleport(tr.position);
        }
        
        public void Teleport(Vector3 position)
        {
            if (IsOwner) // Only the owner can teleport
            {
                StartCoroutine(TeleportCoroutine(position));
            }
            else if (IsServerStarted)
            {
                TeleportTargetRpc(Owner, position);
            }
            else
            {
                TeleportServerRpc(Owner, position);
            }
        }

        public void Teleport(string landmarkName)
        {
            switch (landmarkName)
            {
                case "kitchen":
                    Landmark_Kitchen kitchenLandmark = FindAnyObjectByType<Landmark_Kitchen>();
                    if (kitchenLandmark)
                    {
                        var offset = new Vector3(2, 1, 2);
                        Teleport(kitchenLandmark.transform.position + offset);
                    }
                    else
                    {
                        Logger.LogInfo("Landmark_Kitchen not found", context: this);
                    }
                    break;
                case "voodoo":
                    Landmark_Voodoo voodooLandmark = FindAnyObjectByType<Landmark_Voodoo>();
                    if (voodooLandmark)
                    {
                        var offset = new Vector3(2, 1, 2);
                        Teleport(voodooLandmark.transform.position + offset);
                    }
                    else
                    {
                        Logger.LogInfo("Landmark_Voodoo not found", context: this);
                    }
                    break;
                case "zoom":
                    Landmark_Zoom zoomLandmark = FindAnyObjectByType<Landmark_Zoom>();
                    if (zoomLandmark)
                    {
                        var offset = new Vector3(2, 1, 2);
                        Teleport(zoomLandmark.transform.position + offset);
                    }
                    else
                    {
                        Logger.LogInfo("Landmark_Zoom not found", context: this);
                    }
                    break;
            }
        }
        
        [TargetRpc]
        private void TeleportTargetRpc(NetworkConnection conn, Vector3 position)
        {
            StartCoroutine(TeleportCoroutine(position));
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TeleportServerRpc(NetworkConnection conn, Vector3 position)
        {
            Teleport(position);
        }

        private IEnumerator TeleportCoroutine(Vector3 position)
        {
            Logger.LogInfo("Teleporting player to " + position, context: this);
            bool rigidbodyStateBefore = _rigidbody.isKinematic;
            _rigidbody.isKinematic = true;
            _playerCollider.enabled = false;
            yield return new WaitForSeconds(0.5f);
            _rigidbody.MovePosition(position);
            _characterTongueAnchor.GetRigidbody().MovePosition(position);
            _networkPlayer.GetPlayerController().GetTongue().ResetTongue();
            yield return new WaitForSeconds(0.5f);
            _rigidbody.velocity = Vector3.zero;
            _characterTongueAnchor.GetRigidbody().velocity = Vector3.zero;
            yield return new WaitForSeconds(0.5f);
            _playerCollider.enabled = true;
            _rigidbody.isKinematic = rigidbodyStateBefore;
            Logger.LogDebug("Player teleported to " + transform.position, context: this);
        }
        
        public TongueAnchor GetCharacterTongueAnchor()
        {
            return _characterTongueAnchor;
        }
        
        public PlayerStickyTongue GetTongue()
        {
            return _playerStickyTongue;
        }
        
        private void DisablePlayerRotation()
        {
            Logger.LogTrace("DisablePlayerRotation", context: this);
            _canRotate = false;
        }

        private void EnablePlayerRotation()
        {
            Logger.LogTrace("EnablePlayerRotation", context: this);
            _canRotate = true;
        }
    }
}