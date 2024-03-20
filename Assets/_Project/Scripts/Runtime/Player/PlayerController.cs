using System;
using _Project.Scripts.Runtime.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Player
{
    // Player controller should only be local, it just moves the Client NetworkTransform, we trust the client here
    public class PlayerController : MonoBehaviour
    {
        private IInputProvider _inputProvider;
        
        // Maybe we can move this to the IInputProvider interface and use ref to bind the events ?
        public void BindInputProvider(IInputProvider inputProvider)
        {
            // Clean the old input provider
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
            }
            
            // Bind the new input provider
            _inputProvider = inputProvider;
            _inputProvider.OnActionInteractPerformed += OnInteractPerformed;
        }
        
        private void OnDestroy()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnActionInteractPerformed -= OnInteractPerformed;
            }
        }

        private void Update()
        {
            // Very very simple movement, no character controller, no physics, just for testing
            var movementInput = _inputProvider.GetMovementInput();
            transform.position += new Vector3(movementInput.x, 0, movementInput.y) * Time.deltaTime;
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Interact performed locally !");
        }
    }
}