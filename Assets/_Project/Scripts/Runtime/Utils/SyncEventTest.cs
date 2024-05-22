using System;
using _Project.Scripts.Runtime.Networking;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class SyncEventTest : NetworkBehaviour
    {
        public readonly SyncEvent OnTest = new SyncEvent();

        private void Start()
        {
            OnTest.OnEvent += OnTestEventFired;
        }

        private void OnTestEventFired(bool asServer)
        {
            Debug.Log($"OnTestEventFired: {asServer} with client id: {NetworkManager.ClientManager.Connection.ClientId}");
        }

        [Button(ButtonSizes.Large)]
        private void InvokeTestEvent()
        {
            InvokeServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void InvokeServerRpc()
        {
            OnTest.Invoke();
        }
    }
}