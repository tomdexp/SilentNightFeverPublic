using System;
using FishNet;
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

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("TongueAnchor : Server started and rigidbody is set to kinematic==false");
            _rigidbody.isKinematic = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!InstanceFinder.IsServerStarted)
            {
                Debug.Log("TongueAnchor : Client started and rigidbody is set to kinematic==true");
                _rigidbody.isKinematic = true;
            }
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
            SyncRigidbodyAuthorityServerRpc();
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
        
        public void TryUnbindTongue(PlayerStickyTongue tongue)
        {
            // SOURCE CLIENT UNBIND TO TONGUE AND TELL SERVER
            Debug.Log("TryUnbindTongue");
            UnbindTongueServerRpc(tongue);
        }

        private void UnbindTongueServerRpc(PlayerStickyTongue tongue)
        {
            Debug.Log("UnbindTongueServerRpc");
            _currentNumberOfTongues.Value--;
            NetworkObject.RemoveOwnership();
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
        
        [ServerRpc(RequireOwnership = false)]
        private void SyncRigidbodyAuthorityServerRpc(NetworkConnection connection = null)
        {
            if (connection != null)
            {
                Debug.Log("SyncRigidbodyAuthorityServerRpc : client id " + connection.ClientId + " asking for authority");
            }
            else
            {
                Debug.Log("SyncRigidbodyAuthorityServerRpc : connection is null");
            }
            if (connection == InstanceFinder.ClientManager.Connection)
            {
                _rigidbody.isKinematic = false;
            }
            else
            {
                _rigidbody.isKinematic = true;
            }
            Debug.Log("SyncRigidbodyAuthorityServerRpc : rigidbody isKinematic = " + _rigidbody.isKinematic);
            SyncRigidbodyAuthorityClientRpc(connection);
        }
        
        [ObserversRpc(ExcludeServer = true)]
        private void SyncRigidbodyAuthorityClientRpc(NetworkConnection connection)
        {
            if (connection == InstanceFinder.ClientManager.Connection)
            {
                _rigidbody.isKinematic = false;
            }
            else
            {
                _rigidbody.isKinematic = true;
            }
            Debug.Log("SyncRigidbodyAuthorityClientRpc : rigidbody isKinematic = " + _rigidbody.isKinematic);
        }
    }
}