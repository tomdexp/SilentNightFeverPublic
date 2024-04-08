using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class TongueAnchor : NetworkBehaviour
    {
        [field: SerializeField] public byte MaxTonguesAtOnce { get; private set; } = 1;
        public Transform Target;
        public bool HasFreeSpace => _currentNumberOfTongues.Value < MaxTonguesAtOnce;
        private readonly SyncVar<byte> _currentNumberOfTongues = new SyncVar<byte>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        private Rigidbody _rigidbody;

        private void Awake()
        {
            if (Target == null)
            {
                Target = transform;
            }
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void TryBindTongue(PlayerStickyTongue tongue, RayHit hitInfo)
        {
            // SOURCE CLIENT BIND TO TONGUE AND TELL SERVER
            Debug.Log("TryBindTongue");
            if (!HasFreeSpace)
            {
                Debug.Log("No free space on tongue anchor.",this);
                return;
            }
            BindTongueServerRpc(tongue);
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void BindTongueServerRpc(PlayerStickyTongue tongue, NetworkConnection connection = null)
        {
            Debug.Log("BindTongueServerRpc");
            if (!HasFreeSpace)
            {
                Debug.Log("No free space on tongue anchor.",this);
                ForceRetractTongueTargetRpc(connection, tongue);
                return;
            }
            _currentNumberOfTongues.Value++;
            NetworkObject.GiveOwnership(connection);
        }
        
        [TargetRpc]
        private void ForceRetractTongueTargetRpc(NetworkConnection connection, PlayerStickyTongue tongue)
        {
            Debug.Log("ForceRetractTongueTargetRpc on " + connection.ClientId);
            tongue.ForceRetractTongue();
        }
        
        public Rigidbody GetRigidbody()
        {
            return _rigidbody;
        }
    }
}