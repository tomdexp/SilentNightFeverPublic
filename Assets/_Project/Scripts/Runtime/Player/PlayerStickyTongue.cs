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
        [Title("References")] [SerializeField, Required]
        private Transform _tongueThrowDirection;

        [SerializeField, Required] private Transform _tongueOrigin;
        [SerializeField, Required] private NetworkPlayer _networkPlayer;
        [SerializeField, Required] private Transform _tongueTip;
        [SerializeField, Required] private RaySensor _raySensor;
        [SerializeField, Required] private ObiSolver _obiSolver;
        [SerializeField, Required] private ObiRope _obiRope;
        [SerializeField, Required] private Rigidbody _tongueTipRigidbody;
        [SerializeField, Required] private Rigidbody _playerRigidbody;

        [Title("Debug (Read-Only)")] [SerializeField, ReadOnly]
        private bool _isTongueOut;

        [SerializeField, ReadOnly] private bool _canThrowTongue = true;
        [SerializeField, ReadOnly] private bool _canRetractTongue = true;
        [SerializeField, ReadOnly] private bool _isTongueBind = false;
        [SerializeField, ReadOnly] private float _defaultPlayerMass;
        [SerializeField, ReadOnly] private TongueAnchor _currentBindTongueAnchor;
        [SerializeField, ReadOnly] private MeshRenderer _tongueRenderer;
        public event Action OnTongueOut;
        public event Action OnTongueIn;

        public override void OnStartClient()
        {
            base.OnStartClient();
            _tongueRenderer = _obiRope.GetComponent<MeshRenderer>();
            if (_tongueRenderer == null)
            {
                Debug.LogError("No mesh renderer found on tongue renderer.", this);
            }

            _tongueRenderer.enabled = false;
            _defaultPlayerMass = _playerRigidbody.mass;
            ApplyPlayerDataToRaySensor();
            RetractTongue();
        }

        private void Update()
        {
            if (!_isTongueOut && IsOwner)
            {
                _tongueTip.position = _tongueOrigin.position;
            }
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
            if (!_isTongueOut)
            {
                ThrowTongue();
            }
        }

        public void TryRetractTongue()
        {
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} is trying to retract tongue.");
            if (_isTongueOut)
            {
                RetractTongue();
            }
        }

        private void ThrowTongue()
        {
            StartCoroutine(ThrowTongueCoroutine());
        }

        private IEnumerator ThrowTongueCoroutine()
        {
            if (_isTongueOut || !_canThrowTongue)
            {
                Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Cannot throw tongue");
                yield break;
            }

            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Throwing tongue locally");

            var didHit = _raySensor.IsObstructed;

            if (didHit)
            {
                RayHit hitInfo = _raySensor.GetObstructionRayHit();
                Debug.Log(
                    $"Player {_networkPlayer.GetPlayerIndexType()} : Hit something with tongue: " +
                    hitInfo.GameObject.name, hitInfo.GameObject);
                var tongueCollider = hitInfo.Collider.GetComponent<TongueCollider>();
                if (tongueCollider != null)
                {
                    Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue collider");
                    var tongueAnchor = tongueCollider.GetComponentInParent<TongueAnchor>();
                    if (tongueAnchor != null)
                    {
                        Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hit tongue anchor");
                        if (!tongueAnchor.HasFreeSpace)
                        {
                            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Tongue anchor has no free space");
                            yield break;
                        }
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
                    Debug.Log(
                        $"Player {_networkPlayer.GetPlayerIndexType()} : hit something else but it has no tongue collider");
                }
            }
            else
            {
                Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Did not hit anything with tongue");
            }

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
            if (_isTongueBind)
            {
                yield return UnbindTongueFromAnchorCoroutine();
            }

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
            _isTongueOut = true;
            _tongueTipRigidbody.isKinematic = true;
            _tongueTip.position = _tongueOrigin.position;
            SetTongueVisibilityServerRpc(true);
            yield return ThrowTo(tongueAnchor.Target.position);
            _isTongueBind = true;
            _currentBindTongueAnchor = tongueAnchor;

            var fixedJoint = _tongueTip.gameObject.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = tongueAnchor.GetRigidbody();
            fixedJoint.autoConfigureConnectedAnchor = false;
            fixedJoint.connectedAnchor = Vector3.zero;
            fixedJoint.anchor = Vector3.zero;
            _tongueTipRigidbody.isKinematic = false;
        }

        private IEnumerator UnbindTongueFromAnchorCoroutine()
        {
            _isTongueBind = false;
            _currentBindTongueAnchor.TryUnbindTongue(this);
            _currentBindTongueAnchor = null;
            _tongueTipRigidbody.isKinematic = true;
            var fixedJoint = _tongueTip.gameObject.GetComponent<FixedJoint>();
            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
            }

            yield return Retract();
            SetTongueVisibilityServerRpc(false);
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Hiding tongue and disabling solver");
            _isTongueOut = false;
        }

        private IEnumerator ThrowTo(Vector3 targetPosition)
        {
            var duration = Vector3.Distance(_tongueTip.position, targetPosition) /
                           _networkPlayer.PlayerData.TongueThrowSpeed;
            _tongueTip.position = _tongueOrigin.position;
            var tween = _tongueTip.DOMove(targetPosition, duration).SetEase(_networkPlayer.PlayerData.TongueThrowEase);
            StartCoroutine(SmoothMassChangeUp(50));
            yield return tween.WaitForCompletion();
            StartCoroutine(SmoothMassChangeDown());
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Thrown tongue to target position");
        }

        private IEnumerator Retract()
        {
            const float threshold = 0.1f;
            StartCoroutine(SmoothMassChangeUp(50));
            while (Vector3.Distance(_tongueTip.position, _tongueOrigin.position) > threshold)
            {
                var duration = Vector3.Distance(_tongueTip.position, _tongueOrigin.position) /
                               _networkPlayer.PlayerData.TongueRetractSpeed;
                var tween = _tongueTip.DOMove(_tongueOrigin.position, duration)
                    .SetEase(_networkPlayer.PlayerData.TongueRetractEase);
                yield return tween.WaitForCompletion();
            }

            StartCoroutine(SmoothMassChangeDown());
            Debug.Log($"Player {_networkPlayer.GetPlayerIndexType()} : Retracted tongue to origin position");
        }

        public bool IsTongueBind()
        {
            return _isTongueBind;
        }

        public Vector3 GetTongueTipPosition()
        {
            return _tongueTip.position;
        }

        private IEnumerator SmoothMassChangeUp(float multiplier)
        {
            var targetMass = _defaultPlayerMass * multiplier;
            var duration = _networkPlayer.PlayerData.SmoothPlayerMassChangeOnTongueMoveDuration;
            var tween = DOTween.To(() => _playerRigidbody.mass, x => _playerRigidbody.mass = x, targetMass, duration);
            yield return tween.WaitForCompletion();
        }

        private IEnumerator SmoothMassChangeDown()
        {
            var targetMass = _defaultPlayerMass;
            var duration = _networkPlayer.PlayerData.SmoothPlayerMassChangeOnTongueMoveDuration;
            var tween = DOTween.To(() => _playerRigidbody.mass, x => _playerRigidbody.mass = x, targetMass, duration);
            yield return tween.WaitForCompletion();
        }

        [ServerRpc(RequireOwnership = true, RunLocally = true)]
        private void SetTongueVisibilityServerRpc(bool value)
        {
            _obiSolver.enabled = value;
            _tongueRenderer.enabled = value;
            if (IsServerStarted) SetTongueVisibilityClientRpc(value);
        }
        
        [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        private void SetTongueVisibilityClientRpc(bool value)
        {
            _obiSolver.enabled = value;
            _tongueRenderer.enabled = value;
        }
    }
}