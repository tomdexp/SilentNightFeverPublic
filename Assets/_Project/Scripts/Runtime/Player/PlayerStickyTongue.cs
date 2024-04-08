using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using DG.Tweening;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Micosmo.SensorToolkit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class PlayerStickyTongue : NetworkBehaviour
    {
        [SerializeField,Required] private Transform _tongueThrowDirection;
        [SerializeField,Required] private NetworkPlayer _networkPlayer;
        [SerializeField,Required] private Transform _tongueTip;
        [SerializeField,Required] private RaySensor _raySensor;
        
        [SerializeField,ReadOnly] private bool _isTongueOut;
        [SerializeField,ReadOnly] private bool _canThrowTongue = true;
        [SerializeField,ReadOnly] private bool _canRetractTongue = true;
        [SerializeField,ReadOnly] private Vector3 _tongueOriginalPosition;
        public event Action OnTongueOut;
        public event Action OnTongueIn;

        public override void OnStartClient()
        {
            base.OnStartClient();
            _tongueOriginalPosition = _tongueTip.position;
            ApplyPlayerDataToRaySensor();
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
        
        private IEnumerator ThrowTo(Vector3 targetPosition)
        {
            var duration = Vector3.Distance(_tongueTip.position, targetPosition) / _networkPlayer.PlayerData.TongueThrowSpeed;
            yield return _tongueTip.DOMove(targetPosition, duration).SetEase(_networkPlayer.PlayerData.TongueThrowEase);
        }

        private IEnumerator Retract()
        {
            var duration = Vector3.Distance(_tongueTip.position, _tongueOriginalPosition) / _networkPlayer.PlayerData.TongueRetractSpeed;
            yield return _tongueTip.DOMove(_tongueOriginalPosition, duration).SetEase(_networkPlayer.PlayerData.TongueRetractEase);
        }
    }
}