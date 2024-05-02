using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerTongue
{
    public class TongueInteractable : NetworkBehaviour
    {
        public event Action OnInteract;
        public readonly SyncVar<bool> IsInteractable = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        public Transform Target;

        private void Awake()
        {
            if (!Target)
            {
                Target = transform;
            }
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

        private void ReplicateOnInteract()
        {
            OnInteractClientRpc();
        }

        [ObserversRpc(ExcludeServer = true)]
        private void OnInteractClientRpc()
        {
            OnInteract?.Invoke();
            Logger.LogTrace("OnInteractClientRpc", Logger.LogType.Client, this);
        }

        public void TryInteract(PlayerStickyTongue tongue, RayHit hitInfo)
        {
            Logger.LogTrace("Try Interact", Logger.LogType.Client, this);
            if (IsInteractable.Value)
            {
                InteractServerRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc()
        {
            Logger.LogTrace("InteractServerRpc", Logger.LogType.Server, this);
            OnInteract?.Invoke();
        }
    }
}