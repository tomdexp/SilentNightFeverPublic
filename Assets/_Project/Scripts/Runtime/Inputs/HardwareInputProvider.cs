using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Inputs
{
    /// <summary>
    /// This class implement the IInputProvider interface and is used to provide input to the game
    /// Its meant to handle real keyboard and gamepad
    /// </summary>
    
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public class HardwareInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<InputAction.CallbackContext> OnActionInteractStarted;
        public event Action<InputAction.CallbackContext> OnActionInteractPerformed;
        public event Action<InputAction.CallbackContext> OnActionInteractCanceled;
        
        private string _currentDevicePath;
        private PlayerInputActions _inputActions;
        private Vector2 _movementInput;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            _inputActions.Player.Interact.started += OnActionInteractStarted;
            _inputActions.Player.Interact.performed += OnActionInteractPerformed;
            _inputActions.Player.Interact.canceled += OnActionInteractCanceled;
            _inputActions.Player.Move.performed += OnMoveInputActionPerformed;
        }

        private void OnMoveInputActionPerformed(InputAction.CallbackContext context)
        {
            if (context.control.device.path != _currentDevicePath) return;
            _movementInput = context.ReadValue<Vector2>();
        }

        private void OnDestroy()
        {
            _inputActions.Disable();
            _inputActions.Player.Interact.started -= OnActionInteractStarted;
            _inputActions.Player.Interact.performed -= OnActionInteractPerformed;
            _inputActions.Player.Interact.canceled -= OnActionInteractCanceled;
            _inputActions.Player.Move.performed -= OnMoveInputActionPerformed;
        }

        public void SetDevicePath(string devicePath)
        { 
            _currentDevicePath = devicePath;
        }
        
        public Vector2 GetMovementInput()
        {
            return _movementInput;
        }
    }
}