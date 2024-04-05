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
            BindTongueServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void BindTongueServerRpc()
        {
            Debug.Log("BindTongueServerRpc");
            _currentNumberOfTongues.Value++;
        }
    }
}