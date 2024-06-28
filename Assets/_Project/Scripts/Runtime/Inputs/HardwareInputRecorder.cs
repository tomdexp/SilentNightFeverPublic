using System;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Player;
using FishNet;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Inputs
{
    public class HardwareInputRecorder : MonoBehaviour, IInputProvider
    {
        [SerializeField] private InputAction _recordInputAction;
        [SerializeField] private InputAction _playInputAction;
        [SerializeField] private bool _playInLoop;
        [SerializeField, Required] private HardwareInputProvider _hardwareInputProviderTarget; // it implements IInputProvider
        [SerializeField, Required] private PlayerController _playerControllerTarget;
        
        private Queue<RecordedInput> _recordedInputs = new Queue<RecordedInput>();
        private Queue<RecordedInput> _backupRecordedInputs = new Queue<RecordedInput>();
        private bool _isRecording;
        private bool _isReplaying;
        
        private Vector2 _movementInputThisFrame;
        private bool _isInteractStartedThisFrame;
        private bool _isInteractPerformedThisFrame;
        private bool _isInteractCanceledThisFrame;
        
        private Vector2 _lastMovementInput;
        
        struct RecordedInput
        {
            public Vector2 MovementInput;
            public bool IsInteractStarted;
            public bool IsInteractPerformed;
            public bool IsInteractCanceled;
        }

        private void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _recordInputAction.performed += HandleRecording;
            _playInputAction.performed += HandleReplaying;
            _recordInputAction.Enable();
            _playInputAction.Enable();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _recordInputAction.Disable();
            _playInputAction.Disable();
            _recordInputAction.performed -= HandleRecording;
            _playInputAction.performed -= HandleReplaying;
#endif
        }

        private void HandleRecording(InputAction.CallbackContext context)
        {
            RealPlayerInfo realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)InstanceFinder.ClientManager.Connection.ClientId,
                DevicePath = context.control.device.path
            };
            
            var realPlayerInfoTarget = _hardwareInputProviderTarget.GetRealPlayerInfo();
            
            if (realPlayerInfo.ClientId != realPlayerInfoTarget.ClientId || realPlayerInfo.DevicePath != realPlayerInfoTarget.DevicePath)
            {
                return;
            }
            
            if (_isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void HandleReplaying(InputAction.CallbackContext context)
        {
            RealPlayerInfo realPlayerInfo = new RealPlayerInfo
            {
                ClientId = (byte)InstanceFinder.ClientManager.Connection.ClientId,
                DevicePath = context.control.device.path
            };
            
            var realPlayerInfoTarget = _hardwareInputProviderTarget.GetRealPlayerInfo();
            
            if (realPlayerInfo.ClientId != realPlayerInfoTarget.ClientId || realPlayerInfo.DevicePath != realPlayerInfoTarget.DevicePath)
            {
                return;
            }
            
            if (_isReplaying)
            {
                StopReplaying();
            }
            else
            {
                StartReplaying();
            }
        }

        private void Update()
        {
            if (_isRecording)
            {
                RecordInput();
            }
            if (_isReplaying)
            {
                ReplayInput();
            }
        }

        private void ReplayInput()
        {
            if (_recordedInputs.Count == 0)
            {
                if (_playInLoop)
                {
                    _recordedInputs = new Queue<RecordedInput>(_backupRecordedInputs);
                }
                else
                {
                    StopReplaying();
                    return;
                }
            };
            var recordedInput = _recordedInputs.Dequeue();
            _lastMovementInput = recordedInput.MovementInput;
            if (recordedInput.IsInteractStarted)
            {
                OnActionInteractStarted?.Invoke(new InputAction.CallbackContext());
            }
            if (recordedInput.IsInteractPerformed)
            {
                OnActionInteractPerformed?.Invoke(new InputAction.CallbackContext());
            }
            if (recordedInput.IsInteractCanceled)
            {
                OnActionInteractCanceled?.Invoke(new InputAction.CallbackContext());
            }
        }

        private void RecordInput()
        {
            var recordedInput = new RecordedInput
            {
                MovementInput = _hardwareInputProviderTarget.GetMovementInput(),
                IsInteractStarted = _isInteractStartedThisFrame,
                IsInteractPerformed = _isInteractPerformedThisFrame,
                IsInteractCanceled = _isInteractCanceledThisFrame
            };
            _recordedInputs.Enqueue(recordedInput);
            _isInteractStartedThisFrame = false;
            _isInteractPerformedThisFrame = false;
            _isInteractCanceledThisFrame = false;
        }

        public void StartRecording()
        {
            if (_isRecording) return;
            _isRecording = true;
            _recordedInputs.Clear();
            _hardwareInputProviderTarget.OnActionInteractStarted += RecordOnActionInteractStarted;
            _hardwareInputProviderTarget.OnActionInteractPerformed += RecordOnActionInteractPerformed;
            _hardwareInputProviderTarget.OnActionInteractCanceled += RecordOnActionInteractCanceled;
            Logger.LogDebug("Started recording inputs", context: this);
        }
        
        public void StopRecording()
        {
            if (!_isRecording) return;
            _isRecording = false;
            _hardwareInputProviderTarget.OnActionInteractStarted -= RecordOnActionInteractStarted;
            _hardwareInputProviderTarget.OnActionInteractPerformed -= RecordOnActionInteractPerformed;
            _hardwareInputProviderTarget.OnActionInteractCanceled -= RecordOnActionInteractCanceled;
            _backupRecordedInputs = new Queue<RecordedInput>(_recordedInputs);
            Logger.LogDebug("Stopped recording inputs", context: this);
        }
        
        public void StartReplaying()
        {
            if (_isReplaying) return;
            _isReplaying = true;
            _playerControllerTarget.BindInputProvider(this);
            Logger.LogDebug("Started replaying inputs", context: this);
        }
        
        public void StopReplaying()
        {
            if (!_isReplaying) return;
            _isReplaying = false;
            _playerControllerTarget.BindInputProvider(_hardwareInputProviderTarget);
            Logger.LogDebug("Stopped replaying inputs", context: this);
        }
        
        private void RecordOnActionInteractStarted(InputAction.CallbackContext context)
        {
            _isInteractStartedThisFrame = true;
        }
        private void RecordOnActionInteractPerformed(InputAction.CallbackContext context)
        {
            _isInteractPerformedThisFrame = true;
        }
        private void RecordOnActionInteractCanceled(InputAction.CallbackContext context)
        {
            _isInteractCanceledThisFrame = true;
        }

        public event Action<InputAction.CallbackContext> OnActionInteractStarted;
        public event Action<InputAction.CallbackContext> OnActionInteractPerformed;
        public event Action<InputAction.CallbackContext> OnActionInteractCanceled;
        public event Action OnActionPausePerformed;

        public Vector2 GetMovementInput()
        {
            return _lastMovementInput;
        }

        public void SetRealPlayerInfo(RealPlayerInfo realPlayerInfo)
        {
            
        }

        public void DisableInput()
        {
            
        }

        public void EnableInput()
        {
            
        }
    }
}