using System;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    [RequireComponent(typeof(TongueInteractable))]
    public abstract class NetworkConsumable : NetworkBehaviour
    {
        private TongueInteractable _tongueInteractable;

        private void Awake()
        {
            _tongueInteractable = GetComponent<TongueInteractable>();
            if (!_tongueInteractable)
            {
                Logger.LogError("TongueInteractable component not found", context: this);
            }
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _tongueInteractable.OnInteract += Consume;
        }

        protected abstract void Consume();
    }
}