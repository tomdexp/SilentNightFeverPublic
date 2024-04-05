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
        [SerializeField,Required] private Transform _tongueThrowDirection;
        [SerializeField,Required] private NetworkPlayer _networkPlayer;
        [SerializeField,ReadOnly] private bool _isTongueOut;
        [SerializeField, ReadOnly] private bool _canThrowTongue = true;
        [SerializeField, ReadOnly] private bool _canRetractTongue = true;
        private readonly SyncVar<Vector3> _currentTongueTipPosition = new SyncVar<Vector3>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
        public event Action OnTongueOut;
        public event Action OnTongueIn;
        
        public void TryUseTongue()
        {
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} is trying to use tongue.");
            if (_isTongueOut)
            {
                RetractTongue();
            }
            else
            {
                ThrowTongue();
            }
        }
        
        private void ThrowTongue()
        {
            if (_isTongueOut || !_canThrowTongue) return;
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Throwing tongue locally");
            float sphereCastRadius = _networkPlayer.PlayerData.TongueSphereCastRadius;
            float maxDistance = _networkPlayer.PlayerData.MaxTongueDistance;

            bool didHit = Physics.SphereCast(_tongueThrowDirection.position, sphereCastRadius, _tongueThrowDirection.forward, out var hitInfo, maxDistance);

            if (didHit)
            {
                var tongueAnchor = hitInfo.collider.GetComponent<TongueAnchor>();
                if (tongueAnchor != null)
                {
                    Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue anchor");
                    tongueAnchor.TryBindTongue(this, hitInfo);
                }
                else
                {
                    var tonguePushable = hitInfo.collider.GetComponent<TongueInteractable>();
                    if (tonguePushable != null)
                    {
                        Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue pushable");
                        tonguePushable.TryInteract(this, hitInfo);
                    }
                }
            }
            else
            {
                Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Did not hit anything with tongue");
                _currentTongueTipPosition.Value = _tongueThrowDirection.position + _tongueThrowDirection.forward * maxDistance;
            }
            _isTongueOut = true;
            OnTongueOut?.Invoke();
        }

        
        private void RetractTongue()
        {
            if (!_isTongueOut) return;
            if (!_canRetractTongue) return;
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Retracting tongue locally");
            _currentTongueTipPosition.Value = Vector3.zero;
            OnTongueIn?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (_networkPlayer != null)
            {
                // Draw tongue max distance
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(_tongueThrowDirection.position, _tongueThrowDirection.forward * _networkPlayer.PlayerData.MaxTongueDistance);
            }
        }
    }
}