using System;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Inputs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(InputRecorder))]
    public class InputRecorderExtension : MonoBehaviour
    {
        [SerializeField] private InputAction _playInLoopAction;
        [SerializeField] private bool _doLoop = false;
        
        private InputRecorder _inputRecorder;

        private void Awake()
        {
            _inputRecorder = GetComponent<InputRecorder>();
            _playInLoopAction.performed += OnPlayInLoop;
            _playInLoopAction.Enable();
            _inputRecorder.changeEvent.AddListener(OnChangeEvent);
        }

        private void OnChangeEvent(InputRecorder.Change change)
        {
            if (change == InputRecorder.Change.ReplayStopped && _doLoop)
            {
                _inputRecorder.StartReplay();
            }
        }

        private void OnPlayInLoop(InputAction.CallbackContext context)
        {
            _doLoop = !_doLoop;
            if (_doLoop)
            {
                _inputRecorder.StartReplay();
            }
        }
    }
}