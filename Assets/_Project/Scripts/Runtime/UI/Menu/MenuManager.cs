using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.UI
{
    public class MenuManager : MonoBehaviour
    {
        private NavigationHistory _navigationHistory;

        private PlayerInputActions _input = null;

        private void Awake()
        {
            _input = new PlayerInputActions();
            _input.Enable();
        }

        private void Start()
        {
            TryGetComponent(out _navigationHistory);
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.UI.Cancel.performed += RevertLastInstruction;
        }

        private void OnDisable()
        {
            _input.Disable();
            _input.UI.Cancel.performed -= RevertLastInstruction;
        }

        private void RevertLastInstruction(InputAction.CallbackContext val)
        {
            if (val.ReadValueAsButton() != true || !_navigationHistory) return;

            _navigationHistory.RevertLastInstruction();
        }

        public void QuitGame()
        {
            Application.Quit();
        }



    }
}
