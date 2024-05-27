using System;
using System.Collections;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Landmarks.Zoom
{
    public class Landmark_Zoom : Landmark
    {
        public new LandmarkData_Zoom Data
        {
            get => (LandmarkData_Zoom)base.Data;
            set => base.Data = value;
        }
        
        [Title("Landmark Zoom Reference")]
        [SerializeField] private RotationTracker _sliderRotationTracker;
        [SerializeField] private NetworkObject[] _anchors;
        
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private float _teamAFov;
        [SerializeField, ReadOnly] private float _teamBFov;
        [SerializeField, ReadOnly] private float _absSignedAngle;
        [SerializeField, ReadOnly] private float _t;
        [SerializeField, ReadOnly] private float _newFov;
        [SerializeField, ReadOnly] private Rigidbody _sliderRigidbody;
        [SerializeField, ReadOnly] private bool _isActive;
        [SerializeField, ReadOnly] private bool _hasJustMoved;
        [SerializeField, ReadOnly] private bool _isMoving;
        [field: SerializeField, ReadOnly] public float Speed { get; private set; }
        
        public event Action OnStartTurning;
        public event Action OnStopTurning;
        public event Action OnStep;
        
        private WaitForSeconds _waitForSeconds;

        protected override void OnStart()
        {
            base.OnStart();
            _sliderRigidbody = _sliderRotationTracker.GetComponent<Rigidbody>();
            if (!_sliderRigidbody)
            {
                Logger.LogError("No Rigidbody component found on the Rotation Tracker", Logger.LogType.Server, this);
            }
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            foreach (var nob in _anchors)
            {
                nob.UnsetParent();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _sliderRotationTracker.OnSignedAngleChanged += OnSignedAngleChanged;
            _waitForSeconds = new WaitForSeconds(Data.MinSecondsBetweenStepForContinuation);
            StartCoroutine(CheckForStartAndEndEvents());
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            _sliderRotationTracker.OnSignedAngleChanged -= OnSignedAngleChanged;
            StopCoroutine(CheckForStartAndEndEvents());
        }

        private void OnSignedAngleChanged(float newSignedAngle)
        {
            if (!_isActive) return;
            // If its negative, then we need to calculate the new FOV for team A
            // If its positive, then we need to calculate the new FOV for team B
            OnStep?.Invoke();
            _hasJustMoved = true;
            _absSignedAngle = Mathf.Abs(newSignedAngle);
            _t = Mathf.InverseLerp(Data.MinSignedAngle, Data.MaxSignedAngle, _absSignedAngle);
            _newFov = Mathf.Lerp(CameraManager.Instance.DefaultPlayerFov,Data.MaxFov,  _t);
            if (newSignedAngle < 0)
            {
                _teamAFov = _newFov;
                _teamBFov = CameraManager.Instance.DefaultPlayerFov;
            }
            else
            {
                _teamBFov = _newFov;
                _teamAFov = CameraManager.Instance.DefaultPlayerFov;
            }
            CameraManager.Instance.SetFov(PlayerTeamType.A, _teamAFov);
            CameraManager.Instance.SetFov(PlayerTeamType.B, _teamBFov);
        }

        protected override void ResetLandmark(byte _)
        {
            Logger.LogDebug("Resetting Landmark " + nameof(Landmark_Zoom), Logger.LogType.Server, this);
            StartCoroutine(ResetLandmarkCoroutine());
        }
        
        private IEnumerator ResetLandmarkCoroutine()
        {
            _isActive = false;
            _sliderRigidbody.velocity = Vector3.zero;
            _sliderRigidbody.angularVelocity = Vector3.zero;
            _sliderRigidbody.isKinematic = true;
            yield return new WaitForSeconds(0.1f);
            _sliderRigidbody.transform.localEulerAngles = Vector3.zero;
            CameraManager.Instance.SetFov(PlayerTeamType.A, CameraManager.Instance.DefaultPlayerFov);
            CameraManager.Instance.SetFov(PlayerTeamType.B, CameraManager.Instance.DefaultPlayerFov);
            yield return new WaitForSeconds(0.1f);
            _sliderRigidbody.isKinematic = false;
            _isActive = true;
        }

        private void FixedUpdate()
        {
            Speed = _sliderRigidbody.angularVelocity.magnitude;
        }

        private IEnumerator CheckForStartAndEndEvents()
        {
            while (true)
            {
                yield return _waitForSeconds;
                if (_hasJustMoved && !_isMoving)
                {
                    OnStartTurning?.Invoke();
                    Logger.LogDebug("Landmark Zoom is moving", Logger.LogType.Server, this);
                    _isMoving = true;
                }
                else if (!_hasJustMoved && _isMoving)
                {
                    OnStopTurning?.Invoke();
                    Logger.LogDebug("Landmark Zoom has stopped moving", Logger.LogType.Server, this);
                    _isMoving = false;
                }
                _hasJustMoved = false;
            }
        }
    }
}