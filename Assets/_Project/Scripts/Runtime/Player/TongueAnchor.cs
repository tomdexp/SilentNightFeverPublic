using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class TongueAnchor : NetworkBehaviour
    {
        [Title("Settings")]
        [field: SerializeField] public byte MaxTonguesAtOnce { get; private set; } = 1;
        public Transform Target;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private float _rigidbodySpeed;
        
        public bool HasFreeSpace => _currentNumberOfTongues.Value < MaxTonguesAtOnce;
        private readonly SyncVar<byte> _currentNumberOfTongues = new SyncVar<byte>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        private Rigidbody _rigidbody;

        private void Awake()
        {
            if (!Target)
            {
                Target = transform;
            }
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _rigidbodySpeed = _rigidbody.velocity.magnitude;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(WaitAndActivateRigidbody());
        }
        
        private IEnumerator WaitAndActivateRigidbody()
        {
            yield return new WaitForSeconds(1f);
            Logger.LogTrace("TongueAnchor : Server started and rigidbody is set to kinematic==false", Logger.LogType.Server, this);
            _rigidbody.isKinematic = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!InstanceFinder.IsServerStarted)
            {
                Logger.LogTrace("TongueAnchor : Client started and rigidbody is set to kinematic==true", Logger.LogType.Client, this);
                _rigidbody.isKinematic = true;
            }
        }

        public void TryBindTongue(PlayerStickyTongue tongue, RayHit hitInfo)
        {
            // SOURCE CLIENT BIND TO TONGUE AND TELL SERVER
            Logger.LogTrace("TryBindTongue to TongueAnchor", Logger.LogType.Client, this);
            if (!HasFreeSpace)
            {
                Logger.LogTrace("No free space on tongue anchor.",Logger.LogType.Client, this);
                return;
            }
            BindTongueServerRpc(tongue);
            SyncRigidbodyAuthorityServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void BindTongueServerRpc(PlayerStickyTongue tongue, NetworkConnection connection = null)
        {
            Logger.LogTrace("BindTongueServerRpc", Logger.LogType.Server, this);
            if (!HasFreeSpace)
            {
                Logger.LogTrace("No free space on tongue anchor.", Logger.LogType.Server, this);
                ForceRetractTongueTargetRpc(connection, tongue);
                return;
            }
            _currentNumberOfTongues.Value++;
            NetworkObject.GiveOwnership(connection);
        }
        
        public void TryUnbindTongue(PlayerStickyTongue tongue)
        {
            // SOURCE CLIENT UNBIND TO TONGUE AND TELL SERVER
            Logger.LogTrace("TryUnbindTongue", Logger.LogType.Client, this);
            UnbindTongueServerRpc(tongue);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnbindTongueServerRpc(PlayerStickyTongue tongue)
        {
            Logger.LogTrace("UnbindTongueServerRpc", Logger.LogType.Server, this);
            _currentNumberOfTongues.Value--;
            StartCoroutine(WaitForRigidbodyStabilization());
        }
        
        private IEnumerator WaitForRigidbodyStabilization()
        {
            while (_rigidbodySpeed > 0.01f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(3f);
            if (_currentNumberOfTongues.Value > 0)
            {
                Logger.LogDebug("Rigidbody stabilization aborted because someone has bind their tongue before the end", Logger.LogType.Server, this);
                yield break;
            }
            NetworkObject.RemoveOwnership();
            SyncRigidbodyAuthorityServerRpc();
        }

        [TargetRpc]
        private void ForceRetractTongueTargetRpc(NetworkConnection connection, PlayerStickyTongue tongue)
        {
            Logger.LogTrace("ForceRetractTongueTargetRpc on " + connection.ClientId, Logger.LogType.Client, this);
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
                Logger.LogTrace("SyncRigidbodyAuthorityServerRpc : client id " + connection.ClientId + " asking for authority", Logger.LogType.Server, this);
            }
            else
            {
                Logger.LogWarning("SyncRigidbodyAuthorityServerRpc : connection is null", Logger.LogType.Server, this);
            }
            if (connection == InstanceFinder.ClientManager.Connection)
            {
                _rigidbody.isKinematic = false;
            }
            else
            {
                _rigidbody.isKinematic = true;
            }
            Logger.LogTrace("SyncRigidbodyAuthorityServerRpc : rigidbody isKinematic = " + _rigidbody.isKinematic, Logger.LogType.Server, this);
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
            Logger.LogTrace("SyncRigidbodyAuthorityClientRpc : rigidbody isKinematic = " + _rigidbody.isKinematic, Logger.LogType.Client, this);
        }
    }
}