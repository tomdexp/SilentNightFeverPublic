using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerTongue
{
    [RequireComponent(typeof(Rigidbody))]
    public class TongueAnchor : NetworkBehaviour
    {
        [Title("Settings")]
        [field: SerializeField] public byte MaxTonguesAtOnce { get; private set; } = 1;
        public Transform Target;
        [Tooltip("If not, its the server that will own the Rigidbody")]
        public bool DefaultOwnershipIsLinkedNetworkObject = false;
        [Tooltip("If set, when unbind the rigidbody will wait for itself to be stabilized before changing ownership")]
        public bool UseRigidbodyStabilization = true;
        [Tooltip("If set, when unbind the ownership of the anchor will be the same as this object")]
        public NetworkObject LinkedNetworkObjectForOwnership;
        public RigidbodyAnchorBehavior RigidbodyBehavior = RigidbodyAnchorBehavior.KinematicWhenNotOwner;
        [Tooltip("If set, when a client is remote, it will simulate the force of the rigidbody")]
        public bool RemoteClientSimulateForce = false;
        [ShowIf("@RemoteClientSimulateForce")]
        public float RemoteClientSimulateForceMultiplier = 10f;
        [ShowIf("@RemoteClientSimulateForce")]
        public float RemoteClientSimulateForceMinDistance = 3f;
        public bool IsCharacterAnchor => DefaultOwnershipIsLinkedNetworkObject; // For now only character anchor has this set to true

        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private float _rigidbodySpeed;
        [SerializeField, ReadOnly] private PlayerStickyTongue _currentStickTongue;
        [SerializeField, ReadOnly] private int _ownerId;

        public enum RigidbodyAnchorBehavior
        {
            AlwaysKinematic,
            AlwaysNonKinematic,
            KinematicWhenNotOwner,
        }
        
        public bool HasFreeSpace => _currentNumberOfTongues.Value < MaxTonguesAtOnce;
        private readonly SyncVar<byte> _currentNumberOfTongues = new SyncVar<byte>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        private Rigidbody _rigidbody;
        
        public event Action<PlayerStickyTongue> OnTongueBindChange;
        private void Awake()
        {
            if (!Target)
            {
                Target = transform;
            }
            _rigidbody = GetComponent<Rigidbody>();
            if (DefaultOwnershipIsLinkedNetworkObject && LinkedNetworkObjectForOwnership == null)
            {
                Logger.LogError("DefaultOwnershipIsLinkedNetworkObject is set to true but LinkedNetworkObjectForOwnership is null.", context: this);
            }

            switch (RigidbodyBehavior)
            {
                case RigidbodyAnchorBehavior.AlwaysKinematic:
                    SetRigidbodyKinematic(true);
                    break;
                case RigidbodyAnchorBehavior.AlwaysNonKinematic:
                    SetRigidbodyKinematic(false);
                    break;
                case RigidbodyAnchorBehavior.KinematicWhenNotOwner:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            _rigidbodySpeed = _rigidbody.velocity.magnitude;
            _ownerId = NetworkObject.Owner.ClientId;
            if(RemoteClientSimulateForce) SimulateForceForRemoteClient();
        }
        
        // The server takes care to simulate the force of the rigidbody for the remote client
        private void SimulateForceForRemoteClient()
        {
            if (!IsServerStarted) return;
            if (!_currentStickTongue) return;
            if (!_currentStickTongue.GetNetworkPlayer().IsOnline) return;
            var playerPosition = _currentStickTongue.GetNetworkPlayer().transform.position;
            // get the distance between the player and the anchor and return a value from 0 to 1 based on MaxDistance
            var distance = Vector3.Distance(playerPosition, transform.position);
            if (distance < RemoteClientSimulateForceMinDistance) return;
            var direction = (playerPosition - transform.position).normalized;
            var force = direction * RemoteClientSimulateForceMultiplier;
            _rigidbody.AddForce(force);
            Logger.LogTrace("SimulateForceForRemoteClient : " + force, Logger.LogType.Server, this);
        }

        public override void OnStartServer()
        {
            StartCoroutine(WaitAndActivateRigidbody());
        }
        
        private IEnumerator WaitAndActivateRigidbody()
        {
            yield return new WaitForSeconds(1f);

            if (DefaultOwnershipIsLinkedNetworkObject && LinkedNetworkObjectForOwnership.Owner != null)
            {
                NetworkObject.GiveOwnership(LinkedNetworkObjectForOwnership.Owner);
                Logger.LogTrace("Giving ownership to " + LinkedNetworkObjectForOwnership.Owner.ClientId, Logger.LogType.Server, this);
                SyncRigidbodyAuthorityClientRpc(LinkedNetworkObjectForOwnership.Owner);
            }
            else
            {
                switch (RigidbodyBehavior)
                {
                    case RigidbodyAnchorBehavior.AlwaysKinematic:
                        SetRigidbodyKinematic(true);
                        break;
                    case RigidbodyAnchorBehavior.AlwaysNonKinematic:
                        SetRigidbodyKinematic(false);
                        break;
                    case RigidbodyAnchorBehavior.KinematicWhenNotOwner:
                        SetRigidbodyKinematic(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void OnStartClient()
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Logger.LogTrace("Client started and rigidbody is set to kinematic==true", Logger.LogType.Client, this);
                switch (RigidbodyBehavior)
                {
                    case RigidbodyAnchorBehavior.AlwaysKinematic:
                        SetRigidbodyKinematic(true);
                        break;
                    case RigidbodyAnchorBehavior.AlwaysNonKinematic:
                        SetRigidbodyKinematic(false);
                        break;
                    case RigidbodyAnchorBehavior.KinematicWhenNotOwner:
                        SetRigidbodyKinematic(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (DefaultOwnershipIsLinkedNetworkObject)
            {
                if (IsOwner) // TODO : It doesn't work for now
                {
                    Logger.LogTrace("Client started and DefaultOwnershipIsParentNetworkObject so rigidbody is set to kinematic==false", Logger.LogType.Client, this);
                    SetRigidbodyKinematic(false);
                }
                else
                {
                    Logger.LogTrace("Client started and DefaultOwnershipIsParentNetworkObject but not Owner so rigidbody is set to kinematic==true", Logger.LogType.Client, this);
                    SetRigidbodyKinematic(true);
                }
            }
        }

        public void TryBindTongue(PlayerStickyTongue tongue)
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
            SetStickTongueServerRpc(tongue);
            _currentNumberOfTongues.Value++;
            NetworkObject.GiveOwnership(connection);
            Logger.LogTrace("Giving ownership to " + connection.ClientId, Logger.LogType.Server, this);
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
            SetStickTongueServerRpc(null);
            if (UseRigidbodyStabilization)
            {
                StartCoroutine(WaitForRigidbodyStabilizationAndChangeOwnerShip());
            }
            else
            {
                if (DefaultOwnershipIsLinkedNetworkObject)
                {
                    Logger.LogTrace("Giving ownership to " + LinkedNetworkObjectForOwnership.Owner.ClientId, Logger.LogType.Server, this);
                    NetworkObject.GiveOwnership(LinkedNetworkObjectForOwnership.Owner);
                }
                else
                {
                    NetworkObject.RemoveOwnership();
                }
                SyncRigidbodyAuthorityServerRpc();
            }
        }
        
        private IEnumerator WaitForRigidbodyStabilizationAndChangeOwnerShip()
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

            if (DefaultOwnershipIsLinkedNetworkObject)
            {
                NetworkObject.GiveOwnership(NetworkObject.Owner);
            }
            else
            {
                NetworkObject.RemoveOwnership();
            }
            
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
            switch (RigidbodyBehavior)
            {
                case RigidbodyAnchorBehavior.AlwaysKinematic:
                    SetRigidbodyKinematic(true);
                    break;
                case RigidbodyAnchorBehavior.AlwaysNonKinematic:
                    SetRigidbodyKinematic(false);
                    break;
                case RigidbodyAnchorBehavior.KinematicWhenNotOwner:
                    if (connection == InstanceFinder.ClientManager.Connection)
                    {
                        SetRigidbodyKinematic(false);
                    }
                    else
                    {
                        SetRigidbodyKinematic(true);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Logger.LogTrace("SyncRigidbodyAuthorityServerRpc : rigidbody isKinematic = " + _rigidbody.isKinematic, Logger.LogType.Server, this);
            SyncRigidbodyAuthorityClientRpc(connection);
        }
        
        [ObserversRpc(ExcludeServer = true)]
        private void SyncRigidbodyAuthorityClientRpc(NetworkConnection connection)
        {
            switch (RigidbodyBehavior)
            {
                case RigidbodyAnchorBehavior.AlwaysKinematic:
                    SetRigidbodyKinematic(true);
                    break;
                case RigidbodyAnchorBehavior.AlwaysNonKinematic:
                    SetRigidbodyKinematic(false);
                    break;
                case RigidbodyAnchorBehavior.KinematicWhenNotOwner:
                    if (connection == InstanceFinder.ClientManager.Connection)
                    {
                        SetRigidbodyKinematic(false);
                    }
                    else
                    {
                        SetRigidbodyKinematic(true);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Logger.LogTrace("SyncRigidbodyAuthorityClientRpc : rigidbody isKinematic = " + _rigidbody.isKinematic, Logger.LogType.Client, this);
        }
        
        private void SetRigidbodyKinematic(bool isKinematic)
        {
            Logger.LogTrace("SetRigidbodyKinematic : rigidbody isKinematic = " + isKinematic, Logger.LogType.Client, this);
            _rigidbody.isKinematic = isKinematic;
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void SetStickTongueServerRpc(PlayerStickyTongue tongue)
        {
            _currentStickTongue = tongue;
            OnTongueBindChange?.Invoke(tongue);
            Logger.LogTrace("SetStickTongueServerRpc and event Invoke", Logger.LogType.Server, this);
            SetStickTongueClientRpc(tongue);
        }
        
        [ObserversRpc(ExcludeServer = true)]
        private void SetStickTongueClientRpc(PlayerStickyTongue tongue)
        {
            _currentStickTongue = tongue;
            OnTongueBindChange?.Invoke(tongue);
            Logger.LogTrace("SetStickTongueClientRpc and event Invoke", Logger.LogType.Client, this);
        }
        
        public PlayerStickyTongue GetCurrentStickTongue()
        {
            return _currentStickTongue;
        }
    }
}