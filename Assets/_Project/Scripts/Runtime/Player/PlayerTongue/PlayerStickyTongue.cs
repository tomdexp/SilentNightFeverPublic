﻿using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using Micosmo.SensorToolkit;
using Obi;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerTongue
{
    public class PlayerStickyTongue : NetworkBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private Transform _tongueThrowDirection;
        [SerializeField, Required] private Transform _tongueOrigin;
        [SerializeField, Required] private NetworkPlayer _networkPlayer;
        [SerializeField, Required] private Transform _tongueTip;
        [SerializeField, Required] private TriggerSensor _fovSensor;
        [SerializeField, Required] private FOVCollider _fovCollider;
        [SerializeField, Required] private ObiSolver _obiSolver;
        [SerializeField, Required] private ObiRope _obiRope;
        [SerializeField, Required] private Rigidbody _tongueTipRigidbody;
        [SerializeField, Required] private Rigidbody _playerRigidbody;

        [Title("Debug (Read-Only)")] 
        [SerializeField, ReadOnly] private bool _isTongueOut;
        [SerializeField, ReadOnly] private bool _canThrowTongue = true;
        [SerializeField, ReadOnly] private bool _canRetractTongue = true;
        [SerializeField, ReadOnly] private bool _isTongueBind = false;
        [SerializeField, ReadOnly] private float _defaultPlayerMass;
        [SerializeField, ReadOnly] private TongueAnchor _currentBindTongueAnchor;
        [SerializeField, ReadOnly] private MeshRenderer _tongueRenderer;
        [SerializeField, ReadOnly] private bool _isTongueActionPressed;
        public event Action OnTongueOut;
        public event Action OnTongueIn;
        public event Action OnTongueRetractStart;
        public Transform TongueTip => _tongueTip;
        
        public override void OnStartServer()
        {
            Logger.LogTrace("PlayerStickyTongue.OnStartServer", Logger.LogType.Local, this);
            StartCoroutine(TrySubscribingToRoundEndEvent());
        }
        
        public override void OnStartClient()
        {
            _tongueRenderer = _obiRope.GetComponent<MeshRenderer>();
            if (_tongueRenderer == null)
            {
                Logger.LogError("PlayerStickyTongue : Tongue renderer is null", Logger.LogType.Local, this);
            }
            Logger.LogTrace("PlayerStickyTongue.OnStartClient", Logger.LogType.Local, this);
            _tongueRenderer.enabled = false;
            _defaultPlayerMass = _playerRigidbody.mass;
            RetractTongue();
        }
        
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            // if previous owner was server, we know the object just spawned
            if (prevOwner.ClientId == -1)
            {
                if (IsOwner)
                { 
                    OnTongueOut += ReplicateOnTongueOut;
                    Logger.LogTrace("PlayerStickyTongue.OnTongueOut is registered for Replication", Logger.LogType.Client, this);
                    OnTongueIn += ReplicateOnTongueIn; 
                    Logger.LogTrace("PlayerStickyTongue.OnTongueIn is registered for Replication", Logger.LogType.Client, this);
                    OnTongueRetractStart += ReplicateOnTongueRetractStart;
                    Logger.LogTrace("PlayerStickyTongue.OnTongueRetractStart is registered for Replication", Logger.LogType.Client, this);
                }
            }
        }

        public override void OnStopClient()
        {
            if (IsOwner)
            {
                OnTongueOut -= ReplicateOnTongueOut;
                Logger.LogTrace("PlayerStickyTongue.OnTongueOut is unregistered for Replication", Logger.LogType.Client, this);
                OnTongueIn -= ReplicateOnTongueIn;
                Logger.LogTrace("PlayerStickyTongue.OnTongueIn is unregistered for Replication", Logger.LogType.Client, this);
                OnTongueRetractStart -= ReplicateOnTongueRetractStart;
                Logger.LogTrace("PlayerStickyTongue.OnTongueRetractStart is unregistered for Replication", Logger.LogType.Client, this);
            }
        }

        private void Update()
        {
            if(!IsOwner) return;
            if (!_isTongueOut)
            {
                _tongueTip.position = _tongueOrigin.position;
            }

            if (GameOptions.HoldButtonToAnchorTongue && _isTongueBind && !_isTongueActionPressed)
            {
                RetractTongue();
            }
        }
        
        private void OnDestroy()
        {
            if (GameManager.HasInstance) GameManager.Instance.OnAnyRoundEnded -= ResetTongueClientRpc;
        }
        
        private IEnumerator TrySubscribingToRoundEndEvent()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.OnAnyRoundEnded += ResetTongueClientRpc;
        }

        public void TryUseTongue()
        {
            _isTongueActionPressed = true;
            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} is trying to use tongue.", Logger.LogType.Client, this);
            if (!_isTongueOut)
            {
                ThrowTongue();
            }
            else if (_isTongueOut && !GameOptions.HoldButtonToAnchorTongue)
            {
                RetractTongue();
            }
        }

        public void TryRetractTongue()
        {
            _isTongueActionPressed = false;
            if (!GameOptions.HoldButtonToAnchorTongue) return;
            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} is trying to retract tongue.", Logger.LogType.Client, this);
            if (_isTongueOut)
            {
                RetractTongue();
            }
        }

        private void ThrowTongue()
        {
            StartCoroutine(ThrowTongueCoroutine());
        }

        private IEnumerator ThrowTongueCoroutine()
        {
            if (_isTongueOut || !_canThrowTongue)
            {
                Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Cannot throw tongue", Logger.LogType.Client, this);
                yield break;
            }

            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Throwing tongue locally", Logger.LogType.Client, this);

            bool didHit = _fovSensor.Detections.Count > 0;

            if (didHit)
            {
                Signal signal = _fovSensor.GetStrongestSignal();
                Logger.LogTrace(
                    $"Player {_networkPlayer.GetPlayerIndexType()} : Hit something with tongue: " +
                    signal.Object.name, Logger.LogType.Client, context:this);
                var tongueCollider = signal.Object.GetComponent<TongueCollider>();
                if (tongueCollider)
                {
                    Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue collider", Logger.LogType.Client, this);
                    var tongueAnchor = tongueCollider.GetComponentInParent<TongueAnchor>();
                    if (tongueAnchor)
                    {
                        Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue anchor", Logger.LogType.Client, this);
                        if (!tongueAnchor.HasFreeSpace)
                        {
                            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Tongue anchor has no free space", Logger.LogType.Client, this);
                            yield break;
                        }
                        tongueAnchor.TryBindTongue(this);
                        yield return BindTongueToAnchorCoroutine(tongueAnchor);
                    }
                    else
                    {
                        var tongueInteractable = tongueCollider.GetComponentInParent<TongueInteractable>();
                        if (tongueInteractable)
                        {
                            if (!tongueInteractable.IsInteractable.Value)
                            {
                                Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Tongue interactable is not interactable", Logger.LogType.Client, this);
                                yield break;
                            }
                            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue interactable", Logger.LogType.Client, this);
                            tongueInteractable.TryInteract(this);
                            yield return ThrowToInteractable(tongueInteractable);
                        }
                    }
                }
                else
                {
                    Logger.LogTrace(
                        $"Player {_networkPlayer.GetPlayerIndexType()} : hit something else but it has no tongue collider", Logger.LogType.Client, this);
                }
            }
            else
            {
                Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Did not hit anything with tongue", Logger.LogType.Client, this);
                yield return ThrowInAir();
            }
        }
        
        [ObserversRpc]
        private void ResetTongueClientRpc(byte _)
        {
            if (!Owner.IsLocalClient) return;
            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Retracting tongue requested by server", Logger.LogType.Client, this);
            ResetTongue();
        }

        private void RetractTongue(bool force = false)
        {
            StartCoroutine(RetractTongueCoroutine(force));
        }

        private IEnumerator RetractTongueCoroutine(bool force = false)
        {
            if (!force)
            {
                if (!_canRetractTongue) yield break;
                if (!_isTongueOut) yield break;
            }
            if (_isTongueBind)
            {
                yield return UnbindTongueFromAnchorCoroutine();
            }

            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Retracting tongue locally", Logger.LogType.Client, this);
            OnTongueIn?.Invoke();
        }

        public void ForceRetractTongue()
        {
            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Retracting tongue locally", Logger.LogType.Client, this);
            OnTongueIn?.Invoke();
        }

        [Button]
        private void ResetTongue()
        {
            StartCoroutine(ResetTongueCoroutine());
        }
        
        private IEnumerator ResetTongueCoroutine()
        {
            yield return new WaitForSeconds(1f);
            Logger.LogTrace($"Reset Tongue for Player {_networkPlayer.GetPlayerIndexType()}", Logger.LogType.Client, this);
            if (_isTongueBind)
            {
                _isTongueBind = false;
                if (!_currentBindTongueAnchor)
                {
                    Logger.LogError("Current bind tongue anchor is null", Logger.LogType.Client, this);
                }
                _currentBindTongueAnchor.TryUnbindTongue(this);
                _currentBindTongueAnchor = null;
                _tongueTipRigidbody.isKinematic = true;
                var fixedJoint = _tongueTip.gameObject.GetComponent<FixedJoint>();
                if (fixedJoint != null)
                {
                    Destroy(fixedJoint);
                }
            }
            _tongueTip.position = _tongueOrigin.position;
            SetTongueVisibilityServerRpc(false);
            _isTongueOut = false;
            yield return null;
        }

        private IEnumerator BindTongueToAnchorCoroutine(TongueAnchor tongueAnchor)
        {
            _isTongueOut = true;
            _tongueTipRigidbody.isKinematic = true;
            _tongueTip.position = _tongueOrigin.position;
            SetTongueVisibilityServerRpc(true);
            yield return ThrowTo(tongueAnchor.Target.position);
            _isTongueBind = true;
            _currentBindTongueAnchor = tongueAnchor;

            var fixedJoint = _tongueTip.gameObject.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = tongueAnchor.GetRigidbody();
            fixedJoint.autoConfigureConnectedAnchor = false;
            fixedJoint.connectedAnchor = Vector3.zero;
            fixedJoint.anchor = Vector3.zero;
            _tongueTipRigidbody.isKinematic = false;
        }

        private IEnumerator UnbindTongueFromAnchorCoroutine()
        {
            _isTongueBind = false;
            _currentBindTongueAnchor.TryUnbindTongue(this);
            _currentBindTongueAnchor = null;
            _tongueTipRigidbody.isKinematic = true;
            var fixedJoint = _tongueTip.gameObject.GetComponent<FixedJoint>();
            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
            }

            yield return Retract();
            SetTongueVisibilityServerRpc(false);
            _isTongueOut = false;
        }
        
        private IEnumerator ThrowToInteractable(TongueInteractable tongueInteractable)
        {
            _isTongueOut = true;
            _tongueTipRigidbody.isKinematic = true;
            _tongueTip.position = _tongueOrigin.position;
            SetTongueVisibilityServerRpc(true);
            yield return ThrowTo(tongueInteractable.Target.position);
            yield return new WaitForSeconds(_networkPlayer.PlayerData.TongueInteractDuration);
            yield return Retract();
            SetTongueVisibilityServerRpc(false);
            _isTongueOut = false;
        }

        private IEnumerator ThrowInAir()
        {
            _isTongueOut = true;
            _tongueTipRigidbody.isKinematic = true;
            _tongueTip.position = _tongueOrigin.position;
            SetTongueVisibilityServerRpc(true);
            yield return ThrowTo(_tongueThrowDirection.position + _tongueThrowDirection.forward * (_fovCollider.Length * _networkPlayer.PlayerData.TongueMissPercentOfMaxDistance));
            yield return new WaitForSeconds(_networkPlayer.PlayerData.TongueMissDuration);
            yield return Retract();
            SetTongueVisibilityServerRpc(false);
            _isTongueOut = false;
        }

        private IEnumerator ThrowTo(Vector3 targetPosition)
        {
            OnTongueOut?.Invoke();
            var duration = Vector3.Distance(_tongueTip.position, targetPosition) /
                           _networkPlayer.PlayerData.TongueThrowSpeed;
            _tongueTip.position = _tongueOrigin.position;
            var tween = _tongueTip.DOMove(targetPosition, duration).SetEase(_networkPlayer.PlayerData.TongueThrowEase);
            StartCoroutine(SmoothMassChangeUp(50));
            yield return tween.WaitForCompletion();
            StartCoroutine(SmoothMassChangeDown());
            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Thrown tongue to target position", Logger.LogType.Client, this);
        }

        private IEnumerator Retract()
        {
            OnTongueRetractStart?.Invoke();
            const float threshold = 0.1f;
            StartCoroutine(SmoothMassChangeUp(50));
            while (Vector3.Distance(_tongueTip.position, _tongueOrigin.position) > threshold)
            {
                var duration = Vector3.Distance(_tongueTip.position, _tongueOrigin.position) /
                               _networkPlayer.PlayerData.TongueRetractSpeed;
                var tween = _tongueTip.DOMove(_tongueOrigin.position, duration)
                    .SetEase(_networkPlayer.PlayerData.TongueRetractEase);
                yield return tween.WaitForCompletion();
            }

            StartCoroutine(SmoothMassChangeDown());
            OnTongueIn?.Invoke();
            Logger.LogTrace($"Player {_networkPlayer.GetPlayerIndexType()} : Retracted tongue to origin position", Logger.LogType.Client, this);
        }

        public bool IsTongueBind()
        {
            return _isTongueBind;
        }

        public Vector3 GetTongueTipPosition()
        {
            return _tongueTip.position;
        }

        private IEnumerator SmoothMassChangeUp(float multiplier)
        {
            var targetMass = _defaultPlayerMass * multiplier;
            var duration = _networkPlayer.PlayerData.SmoothPlayerMassChangeOnTongueMoveDuration;
            var tween = DOTween.To(() => _playerRigidbody.mass, x => _playerRigidbody.mass = x, targetMass, duration);
            yield return tween.WaitForCompletion();
        }

        private IEnumerator SmoothMassChangeDown()
        {
            var targetMass = _defaultPlayerMass;
            var duration = _networkPlayer.PlayerData.SmoothPlayerMassChangeOnTongueMoveDuration;
            var tween = DOTween.To(() => _playerRigidbody.mass, x => _playerRigidbody.mass = x, targetMass, duration);
            yield return tween.WaitForCompletion();
        }

        [ServerRpc(RequireOwnership = true, RunLocally = true)]
        private void SetTongueVisibilityServerRpc(bool value)
        {
            _obiSolver.enabled = value;
            _tongueRenderer.enabled = value;
            if (IsServerStarted)
            {
                SetTongueVisibilityClientRpc(value);
                Logger.LogTrace($"PlayerStickyTongue.SetTongueVisibilityServerRpc : {value}", Logger.LogType.Server, this);
            }
        }
        
        [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        private void SetTongueVisibilityClientRpc(bool value)
        {
            _obiSolver.enabled = value;
            _tongueRenderer.enabled = value;
            Logger.LogTrace($"PlayerStickyTongue.SetTongueVisibilityClientRpc : {value}", Logger.LogType.Client, this);
        }
        
        private void ReplicateOnTongueOut()
        {
            OnTongueOutServerRpc();
        }

        [ServerRpc]
        private void OnTongueOutServerRpc()
        {
            if(!Owner.IsLocalClient) OnTongueOut?.Invoke();
            OnTongueOutClientRpc();
        }

        [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        private void OnTongueOutClientRpc()
        {
            if(!Owner.IsLocalClient) OnTongueOut?.Invoke();
        }

        private void ReplicateOnTongueIn()
        {
            OnTongueInServerRpc();
        }

        [ServerRpc]
        private void OnTongueInServerRpc()
        {
            if(!Owner.IsLocalClient) OnTongueIn?.Invoke();
            OnTongueInClientRpc();
        }

        [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        private void OnTongueInClientRpc()
        {
            if(!Owner.IsLocalClient) OnTongueIn?.Invoke();
        }
        
        private void ReplicateOnTongueRetractStart()
        {
            OnTongueRetractStartServerRpc();
        }
        
        [ServerRpc]
        private void OnTongueRetractStartServerRpc()
        {
            if(!Owner.IsLocalClient) OnTongueRetractStart?.Invoke();
            OnTongueRetractStartClientRpc();
        }
        
        [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        private void OnTongueRetractStartClientRpc()
        {
            if(!Owner.IsLocalClient) OnTongueRetractStart?.Invoke();
        }
        
        public TongueAnchor GetCurrentBindTongueAnchor()
        {
            return _currentBindTongueAnchor;
        }
        
        public NetworkPlayer GetNetworkPlayer()
        {
            return _networkPlayer;
        }
    }
}