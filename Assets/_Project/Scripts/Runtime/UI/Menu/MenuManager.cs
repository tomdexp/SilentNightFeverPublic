using _Project.Scripts.Runtime.Networking;
using FishNet;
using System;
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
            EnableBackButton();
        }

        private void OnDisable()
        {
            DisableBackButton();
        }

        #region NavigationHistory
        public void DisableBackButton()
        {
            _input.Disable();
            _input.UI.Cancel.performed -= RevertLastInstruction;
        }

        public void EnableBackButton()
        {
            _input.Enable();
            _input.UI.Cancel.performed += RevertLastInstruction;
        }

        private void RevertLastInstruction(InputAction.CallbackContext val)
        {
            if (val.ReadValueAsButton() != true || !_navigationHistory) return;

            _navigationHistory.RevertLastInstruction();
        }
        #endregion


        public void QuitGame()
        {
            Application.Quit();
        }

    }
}
