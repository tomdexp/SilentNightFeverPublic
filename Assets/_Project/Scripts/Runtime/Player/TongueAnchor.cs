using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class TongueAnchor : NetworkBehaviour
    {
        [field: SerializeField] public byte MaxTonguesAtOnce { get; private set; } = 1;
        public bool HasFreeSpace => _currentNumberOfTongues.Value < MaxTonguesAtOnce;
        private readonly SyncVar<byte> _currentNumberOfTongues = new SyncVar<byte>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        
        public void TryBindTongue(PlayerStickyTongue tongue, RaycastHit hitInfo)
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
    }
}