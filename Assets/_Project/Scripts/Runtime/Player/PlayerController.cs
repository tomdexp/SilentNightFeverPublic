using System;
using _Project.Scripts.Runtime.Inputs;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Player
{
    // Player controller should only be local, it just moves the Client NetworkTransform, we trust the client here
    public class PlayerController : MonoBehaviour
    {
        private IInputProvider _inputProvider;
        private NetworkPlayer _networkPlayer;

        private void Awake()
        {
            var inputProvider = GetComponent<IInputProvider>();
            if (inputProvider != null)
            {
                BindInputProvider(inputProvider);
            }
            else
            {
                Debug.LogWarning("No input provider found on PlayerController.");
            }
            _networkPlayer = GetComponent<NetworkPlayer>();
            if (_networkPlayer == null)
            {
                Debug.LogError("No NetworkPlayer found on PlayerController.");
            }
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
            movementInput *= _networkPlayer.PlayerData.PlayerMovementSpeed;
            transform.position += new Vector3(movementInput.x, 0, movementInput.y) * Time.deltaTime;
        }

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
            
            Debug.Log("Bound input provider : " + _inputProvider.GetType().Name);
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("Interact performed locally !");
        }

        public void SetRealPlayerInfo(RealPlayerInfo realPlayerInfo)
        {
            if (_inputProvider != null)
            {
                _inputProvider.SetRealPlayerInfo(realPlayerInfo);
            }
        }
    }
}