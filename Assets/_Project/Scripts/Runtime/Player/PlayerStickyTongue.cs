using System;
using _Project.Scripts.Runtime.Networking;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerStickyTongue : NetworkBehaviour
    {
        [SerializeField,Required] private NetworkPlayer _networkPlayer;
        [SerializeField,ReadOnly] private bool _isTongueOut;
        private readonly SyncVar<Vector3> _currentTongueTipPosition = new SyncVar<Vector3>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        
        public event Action OnTongueOut;
        public event Action OnTongueIn;
        
        public void TryUseTongue()
        {
            if (_isTongueOut) return;
            _isTongueOut = true;
        }
    }
}