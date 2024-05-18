using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using Sirenix.OdinInspector;
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

        [SerializeField] private Menu _mainMenu;

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
            if (PlayerManager.HasInstance)
            {
                PlayerManager.Instance.OnRealPlayerInfosChanged -= OnRealPlayerInfosChanged;
            }
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

        public void EnablePlayerDisconnectedEvent(bool enable)
        {
            if (enable)
                PlayerManager.Instance.OnRealPlayerInfosChanged += OnRealPlayerInfosChanged;
            else
                PlayerManager.Instance.OnRealPlayerInfosChanged -= OnRealPlayerInfosChanged;
        }

        private void OnRealPlayerInfosChanged(List<RealPlayerInfo> realPlayerInfos)
        {
            if (realPlayerInfos.Count < 4)
            {
                ReturnToMainMenu();
            }
        }

        public void ReturnToMainMenu()
        {
            // TODO : disconect from server
            _mainMenu.OpenMenu();
            _navigationHistory.CloseLastMenu();
            _navigationHistory.ClearHistory();
        }
    }
}
