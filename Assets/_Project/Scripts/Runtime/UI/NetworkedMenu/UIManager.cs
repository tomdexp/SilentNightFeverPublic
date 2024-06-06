using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class UIManager : NetworkSingleton<UIManager>
    {
        public bool IsNavigationWithMouse { get; private set; }
        [SerializeField] private InputAction _goBackAction;
        [SerializeField] private List<MenuBase> _menus = new List<MenuBase>();
        private readonly SyncVar<int> _currentMenuIndex = new SyncVar<int>(-1);
        private ConfirmationPrompt _currentConfirmationPrompt;
        private ConfirmationPrompt _kickedFromServerPrompt;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _currentMenuIndex.OnChange += OnCurrentMenuIndexChanged;
            _goBackAction.performed += OnGoBack;
            _goBackAction.Enable();
            _kickedFromServerPrompt = FindAnyObjectByType<KickedFromServerCanvas>().GetComponent<ConfirmationPrompt>();
            if (!_kickedFromServerPrompt)
            {
                Logger.LogError("KickedFromServerCanvas does not have a ConfirmationPrompt component", Logger.LogType.Client, this);
            }
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _currentMenuIndex.OnChange -= OnCurrentMenuIndexChanged;
            _goBackAction.Disable();
            _goBackAction.performed -= OnGoBack;
        }

        private void LateUpdate()
        {
            HandleMouseOrGamepadNavigation();
        }

        private void HandleMouseOrGamepadNavigation()
        {
            // If we detect a press from a gamepad, hide the mouse cursor and lock it
            if (Gamepad.current != null && IsNavigationWithMouse)
            {
                var gamepadButtonPressedThisFrame = false;
                foreach (var x in Gamepad.current.allControls)
                {
                    if (x is ButtonControl && x.IsPressed() && !x.synthetic)
                    {
                        gamepadButtonPressedThisFrame = true;
                        break;
                    }
                }
                if (gamepadButtonPressedThisFrame)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Confined;
                    // Put cursor at the bottom left corner of the screen to avoid triggering hover on UI elements
                    Mouse.current.WarpCursorPosition(new Vector2(0,0));
                    IsNavigationWithMouse = false;
                    if (_currentConfirmationPrompt)
                    {
                        _currentConfirmationPrompt.TrySelectDefault();
                    }
                    else if (_menus.Count > 0)
                    {
                        if (_currentMenuIndex.Value != -1)
                            _menus[_currentMenuIndex.Value].TrySelectDefault();
                    }
                }
            }
            
            // If we detect a press from the mouse, show the mouse cursor and unlock it
            if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                IsNavigationWithMouse = true;
            }
        }
        
        

        private void OnGoBack(InputAction.CallbackContext context)
        {
            if (_currentConfirmationPrompt)
            {
                _currentConfirmationPrompt.OnCancel();
                return;
            }
            if (_currentMenuIndex.Value == -1)
            {
                Logger.LogError("No menu to go back to", Logger.LogType.Client,this);
                return;
            }
            _menus[_currentMenuIndex.Value].GoBack();
        }

        private void OnCurrentMenuIndexChanged(int prev, int next, bool asServer)
        {
            if (asServer) return;
            if (_menus.Count == 0)
            {
                Logger.LogDebug("No menus registered yet", Logger.LogType.Client,this);
                StartCoroutine(RetryOnCurrentMenuIndexChanged(prev, next, false));
                return;
            }
            if (prev != -1)
            {
                _menus[prev].Close();
                Logger.LogTrace($"Menu changed from {GetMenuType(prev).Name} to {GetMenuType(next).Name}", Logger.LogType.Client,this);
            }
            else
            {
                Logger.LogTrace($"Opening first menu {GetMenuType(next).Name}", Logger.LogType.Client,this);
                // close all menu except the first one
                for (int i = 1; i < _menus.Count; i++)
                {
                    if (i != next)
                    {
                        _menus[i].Close();
                    }
                }
            }
            _menus[next].Open();
            FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(_menus[next].MenuName);
        }
        
        
        // Special case when the network is reset, the client receive the CurrentMenuIndex before the menus are registered, so we need to wait for the menus to be registered and retry
        private IEnumerator RetryOnCurrentMenuIndexChanged(int prev, int next, bool asServer)
        {
            while (!AreAllMenusRegistered())
            {
                yield return null;
            }
            OnCurrentMenuIndexChanged(prev, next, asServer);
        }
        
        public void GoToMenu<T>() where T : MenuBase
        {
            if (!IsServerStarted) return;
            Logger.LogTrace($"Going to menu {typeof(T).Name}", Logger.LogType.Server,this);
            MenuBase nextMenu = GetMenu<T>();
            if (!nextMenu)
            {
                Logger.LogError($"Menu {typeof(T).Name} not found", Logger.LogType.Server, this);
                return;
            }
            _currentMenuIndex.Value = _menus.IndexOf(nextMenu);
        }
        
        private MenuBase GetMenu<T>() where T : MenuBase
        {
            return _menus.FirstOrDefault(menu => menu.GetType() == typeof(T));
        }
        
        private Type GetMenuType(int menuTypeId)
        {
            // check if the index is valid
            if (menuTypeId < 0 || menuTypeId >= _menus.Count)
            {
                Logger.LogError($"Invalid menu index {menuTypeId}", Logger.LogType.Client,this);
                return null;
            }
            return _menus[menuTypeId].GetType();
        }

        public void RegisterMenu(MenuBase menu)
        {
            Logger.LogTrace($"Registering menu {menu.MenuName}", Logger.LogType.Client,this);
            _menus.Add(menu);
            SortMenuByNames();
            CheckAllMenuRegistered();
        }
        
        public string GetCurrentMenuName()
        {
            if (_currentMenuIndex.Value == -1)
            {
                return null;
            }
            return _menus[_currentMenuIndex.Value].MenuName;
        }

        private void CheckAllMenuRegistered()
        {
            if (AreAllMenusRegistered())
            {
                Logger.LogTrace("All menus registered", Logger.LogType.Client, this);
                
                if (!IsServerStarted) return; // because we are going to modify a SyncVar only the server can modify
                
                // if menuToGoOnReset is empty, set it to the first menu (yes this is quite ugly but it works)
                // We always find the object, because this script is a network behaviour and is destroyed and recreated on network reset
                if (string.IsNullOrEmpty(FindAnyObjectByType<MenuToGoOnReset>().MenuName))
                {
                    Logger.LogTrace("Setting MenuToGoOnReset to PressStartMenu", Logger.LogType.Client, this);
                    FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(nameof(PressStartMenu));
                }
                var newMenuName = FindAnyObjectByType<MenuToGoOnReset>().MenuName;
                var newMenu = _menus.FirstOrDefault(menu => menu.MenuName == newMenuName);
                // close all menus except the one we want to go to
                for (int i = 0; i < _menus.Count; i++)
                {
                    if (_menus[i] != newMenu)
                    {
                        _menus[i].Close();
                    }
                }
                _currentMenuIndex.Value = _menus.IndexOf(newMenu);
            }
        }
        
        private bool AreAllMenusRegistered()
        {
            var menus = FindObjectsByType<MenuBase>(FindObjectsSortMode.None);
            bool allMenusRegistered = true;
            foreach (var menu in menus)
            {
                if (!_menus.Contains(menu))
                {
                    allMenusRegistered = false;
                }
            }
            return allMenusRegistered;
        }
        
        private void SortMenuByNames()
        {
            _menus = _menus.OrderBy(menu => menu.MenuName).ToList();
        }

        public void SwitchToMetroCamera()
        {
            var metroCamera = FindAnyObjectByType<MetroCamera>();
            var canvasCamera = FindAnyObjectByType<MetroWorldSpaceCanvasCamera>();
            metroCamera.GetComponent<CinemachineCamera>().Priority.Value = 10;
            canvasCamera.GetComponent<CinemachineCamera>().Priority.Value = 0;
        }
        
        public void SwitchToCanvasCamera()
        {
            var metroCamera = FindAnyObjectByType<MetroCamera>();
            var canvasCamera = FindAnyObjectByType<MetroWorldSpaceCanvasCamera>();
            metroCamera.GetComponent<CinemachineCamera>().Priority.Value = 0;
            canvasCamera.GetComponent<CinemachineCamera>().Priority.Value = 10;
        }

        public void RegisterConfirmationPrompt(ConfirmationPrompt confirmationPrompt)
        {
            Logger.LogTrace("Registering confirmation prompt", Logger.LogType.Client, this);
            if (_currentConfirmationPrompt)
            {
                Logger.LogError("A confirmation prompt is already registered", Logger.LogType.Client, this);
                return;
            }
            _currentConfirmationPrompt = confirmationPrompt;
            _currentConfirmationPrompt.OnResponseReceived += () => _currentConfirmationPrompt = null;
        }
    }
}