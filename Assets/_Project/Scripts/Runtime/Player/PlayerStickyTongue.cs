using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using DG.Tweening;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using Obi;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerStickyTongue : NetworkBehaviour
    {
        [Title("References")]
        [SerializeField,Required] private Transform _tongueThrowDirection;
        [SerializeField,Required] private Transform _tongueOrigin;
        [SerializeField,Required] private NetworkPlayer _networkPlayer;
        [SerializeField,Required] private Transform _tongueTip;
        [SerializeField,Required] private RaySensor _raySensor;
        [SerializeField,Required] private ObiSolver _obiSolver;
        [SerializeField,Required] private ObiRope _obiRope;
        [SerializeField,Required] private Rigidbody _tongueTipRigidbody;
        
        [Title("Debug (Read-Only)")]
        [SerializeField,ReadOnly] private bool _isTongueOut;
        [SerializeField,ReadOnly] private bool _canThrowTongue = true;
        [SerializeField,ReadOnly] private bool _canRetractTongue = true;
        public event Action OnTongueOut;
        public event Action OnTongueIn;

        public override void OnStartClient()
        {
            base.OnStartClient();
            ApplyPlayerDataToRaySensor();
            RetractTongue();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ApplyPlayerDataToRaySensor();
        }

        [Button(ButtonSizes.Small)]
        private void ApplyPlayerDataToRaySensor()
        {
            if (_raySensor == null)
            {
                Debug.LogWarning("Ray sensor is null on player sticky tongue.", this);
                return;
            }
            _raySensor.Length = _networkPlayer.PlayerData.MaxTongueDistance;
            _raySensor.Sphere.Radius = _networkPlayer.PlayerData.TongueSphereCastRadius;
        }

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
            StartCoroutine(ThrowTongueCoroutine());
        }
        
        private IEnumerator ThrowTongueCoroutine()
        {
            if (_isTongueOut || !_canThrowTongue) yield break;
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Throwing tongue locally");

            var didHit = _raySensor.IsObstructed;

            if (didHit)
            {
                RayHit hitInfo = _raySensor.GetObstructionRayHit();
                Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit something with tongue: " + hitInfo.GameObject.name, hitInfo.GameObject);
                var tongueCollider = hitInfo.Collider.GetComponent<TongueCollider>();
                if (tongueCollider != null)
                {
                    Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue collider");
                    var tongueAnchor = tongueCollider.GetComponentInParent<TongueAnchor>();
                    if (tongueAnchor != null)
                    {
                        Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue anchor");
                        tongueAnchor.TryBindTongue(this, hitInfo);
                        yield return BindTongueToAnchorCoroutine(tongueAnchor);
                    }
                    else
                    {
                        var tonguePushable = tongueCollider.GetComponentInParent<TongueInteractable>();
                        if (tonguePushable != null)
                        {
                            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue pushable");
                            tonguePushable.TryInteract(this, hitInfo);
                        }
                    }
                }
                else
                {
                    Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : hit something else but it has no tongue collider");
                }
            }
            else
            {
                Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Did not hit anything with tongue");
            }
            _isTongueOut = true;
            OnTongueOut?.Invoke();
        }

        
        private void RetractTongue()
        {
            StartCoroutine(RetractTongueCoroutine());
        }
        
        private IEnumerator RetractTongueCoroutine()
        {
            if (!_isTongueOut) yield break;
            if (!_canRetractTongue) yield break;
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Retracting tongue locally");
            OnTongueIn?.Invoke();
        }
        
        public void ForceRetractTongue()
        {
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Retracting tongue locally");
            OnTongueIn?.Invoke();
        }
        
        private IEnumerator BindTongueToAnchorCoroutine(TongueAnchor tongueAnchor)
        {
            _tongueTipRigidbody.isKinematic = true;
            _tongueTipRigidbody.velocity = Vector3.zero;
            _tongueTipRigidbody.angularVelocity = Vector3.zero;
            _tongueTip.position = _tongueOrigin.position;
            _obiSolver.enabled = true;
            yield return ThrowTo(tongueAnchor.Target.position);
            // create a fixed joint between the tongue tip rigidbody and the tongue anchor rigidbody
            // var fixedJoint = _tongueTip.gameObject.AddComponent<FixedJoint>();
            // fixedJoint.connectedBody = tongueAnchor.GetRigidbody();
            // fixedJoint.autoConfigureConnectedAnchor = false;
            // fixedJoint.connectedAnchor = Vector3.zero;
            // fixedJoint.anchor = Vector3.zero;
            // _tongueTipRigidbody.isKinematic = false;
        }
        
        private IEnumerator ThrowTo(Vector3 targetPosition)
        {
            var duration = Vector3.Distance(_tongueTip.position, targetPosition) / _networkPlayer.PlayerData.TongueThrowSpeed;
            _tongueTip.position = _tongueOrigin.position;
            yield return _tongueTip.DOMove(targetPosition, duration).SetEase(_networkPlayer.PlayerData.TongueThrowEase);
        }

        private IEnumerator Retract()
        {
            var duration = Vector3.Distance(_tongueTip.position, _tongueOrigin.position) / _networkPlayer.PlayerData.TongueRetractSpeed;
            yield return _tongueTip.DOMove(_tongueOrigin.position, duration).SetEase(_networkPlayer.PlayerData.TongueRetractEase);
        }
    }
}