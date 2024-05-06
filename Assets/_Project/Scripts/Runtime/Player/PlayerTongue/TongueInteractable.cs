using System;
using System.Collections;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerTongue
{
    [RequireComponent(typeof(NetworkTransform))]
    public class TongueInteractable : NetworkBehaviour
    {
        public event Action<PlayerStickyTongue> OnInteract;
        public readonly SyncVar<bool> IsInteractable = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        [Title("Reference")] 
        public Transform Target;
        [ShowIf("@Behavior == InteractableBehavior.AttachToTongue")]
        public Collider[] CollidersToDisableOnAttach;
        [Title("Settings")] 
        public InteractableBehavior Behavior;
        

        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private bool _isAttached;
        [SerializeField, ReadOnly] private Transform _attachedTongueTip;
        
        private NetworkTransform _networkTransform;
        public enum InteractableBehavior
        {
            Nothing,
            AttachToTongue
        }
        
        private void Awake()
        {
            if (!Target)
            {
                Target = transform;
            }
            _networkTransform = GetComponent<NetworkTransform>();
        }

        public override void OnStartServer()
        {
            IsInteractable.Value = true;
            OnInteract += ReplicateOnInteract;
        }

        public override void OnStopServer()
        {
            OnInteract -= ReplicateOnInteract;
        }

        private void ReplicateOnInteract(PlayerStickyTongue playerStickyTongue)
        {
            OnInteractClientRpc(playerStickyTongue);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void OnInteractClientRpc(PlayerStickyTongue playerStickyTongue)
        {
            OnInteract?.Invoke(playerStickyTongue);
            Logger.LogTrace("OnInteractClientRpc", Logger.LogType.Client, this);
        }

        public void TryInteract(PlayerStickyTongue tongue)
        {
            Logger.LogTrace("Try Interact", Logger.LogType.Client, this);
            if (IsInteractable.Value)
            {
                if (Behavior == InteractableBehavior.AttachToTongue)
                {
                    LocallyAttachToTongueTip(tongue.TongueTip);
                }
                InteractServerRpc(tongue);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc(PlayerStickyTongue tongue)
        {
            Logger.LogTrace("InteractServerRpc", Logger.LogType.Server, this);
            OnInteract?.Invoke(tongue);
            if (Behavior == InteractableBehavior.AttachToTongue)
            {
                LocallyAttachToTongueTip(tongue.TongueTip);
            }
            InteractClientRpc(tongue);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void InteractClientRpc(PlayerStickyTongue tongue)
        {
            Logger.LogTrace("InteractClientRpc", Logger.LogType.Client, this);
            if (Behavior == InteractableBehavior.AttachToTongue)
            {
                LocallyAttachToTongueTip(tongue.TongueTip);
            }
        }

        private void LocallyAttachToTongueTip(Transform tongueTip)
        {
            StartCoroutine(LocallyAttachToTongueTipCoroutine(tongueTip));
        }

        private IEnumerator LocallyAttachToTongueTipCoroutine(Transform tongueTip)
        {
            if (_isAttached) yield return null;
            Logger.LogTrace("LocallyAttachToTongueTip", Logger.LogType.Local, this);
            yield return new WaitForSeconds(0.5f);
            foreach (var col in CollidersToDisableOnAttach)
            {
                col.enabled = false;
            }
            _networkTransform.enabled = false;
            _isAttached = true;
            _attachedTongueTip = tongueTip;
            transform.SetParent(tongueTip);
            transform.position = tongueTip.position;
            transform.rotation = tongueTip.rotation;
        }
    }
}