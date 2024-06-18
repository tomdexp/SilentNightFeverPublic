using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.UI.NetworkedMenu;
using _Project.Scripts.Runtime.Utils.Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class UILocalManager : Singleton<UILocalManager>
    {
        public bool IsNavigationWithMouse { get; private set; }
        [SerializeField] private InputAction _goBackAction;
        [SerializeField] private List<MenuBase> _menus = new List<MenuBase>();
        private int _previousMenuIndex = -1;
        private int _currentMenuIndex = -1;
        private ConfirmationPrompt _currentConfirmationPrompt;
        private ConfirmationPrompt _kickedFromServerPrompt;
        
        public event Action<int, int> MenuIndexChanged;

        private IEnumerator Start()
        {
            MenuIndexChanged += OnCurrentMenuIndexChanged;
            _goBackAction.performed += OnGoBack;
            _goBackAction.Enable();
            _kickedFromServerPrompt = FindAnyObjectByType<KickedFromServerCanvas>().GetComponent<ConfirmationPrompt>();
            if (!_kickedFromServerPrompt)
            {
                Logger.LogError("KickedFromServerCanvas does not have a ConfirmationPrompt component", Logger.LogType.Client, this);
            }
            
            yield return new WaitUntil(() => BootstrapManager.HasInstance);
            BootstrapManager.Instance.OnKickedFromServer += OnKickedFromServer;
        }

        private void OnKickedFromServer()
        {
            Logger.LogTrace("Kicked from server, opening return to main menu prompt", Logger.LogType.Client,this);
            StartCoroutine(OnKickedFromServerCoroutine());
        }

        private IEnumerator OnKickedFromServerCoroutine()
        {
            yield return new WaitForSeconds(1f); // don't remove this, it's necessary to wait for old instance of UIManager to be destroyed
            yield return new WaitUntil(() => HasInstance);
            _kickedFromServerPrompt.Open();
            yield return _kickedFromServerPrompt.WaitForResponse();
            GoToMenu<MainMenu>();
        }

        private void OnDestroy()
        {
            MenuIndexChanged -= OnCurrentMenuIndexChanged;
            _goBackAction.Disable();
            _goBackAction.performed -= OnGoBack;
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnKickedFromServer -= OnKickedFromServer;
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
                        if (_currentMenuIndex != -1)
                            _menus[_currentMenuIndex].TrySelectDefault();
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
            if (_currentMenuIndex == -1)
            {
                Logger.LogError("No menu to go back to", Logger.LogType.Client,this);
                return;
            }
            _menus[_currentMenuIndex].GoBack();
        }

        private void OnCurrentMenuIndexChanged(int prev, int next)
        {
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
            OnCurrentMenuIndexChanged(prev, next);
        }
        
        public void GoToMenu<T>() where T : MenuBase
        {
            Logger.LogTrace($"Going to menu {typeof(T).Name}", Logger.LogType.Server,this);
            MenuBase nextMenu = GetMenu<T>();
            if (!nextMenu)
            {
                Logger.LogError($"Menu {typeof(T).Name} not found", Logger.LogType.Server, this);
                return;
            }
            _previousMenuIndex = _currentMenuIndex;
            _currentMenuIndex = _menus.IndexOf(nextMenu);
            MenuIndexChanged?.Invoke(_previousMenuIndex, _currentMenuIndex);
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
            if (_currentMenuIndex == -1)
            {
                return null;
            }
            return _menus[_currentMenuIndex].MenuName;
        }

        private void CheckAllMenuRegistered()
        {
            if (AreAllMenusRegistered())
            {
                Logger.LogTrace("All menus registered", Logger.LogType.Client, this);
                foreach (var menu in _menus)
                {
                    menu.Close();
                }
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

        public void RegisterConfirmationPrompt(ConfirmationPrompt confirmationPrompt)
        {
            Logger.LogTrace("Registering confirmation prompt", Logger.LogType.Client, this);
            if (_currentConfirmationPrompt)
            {
                Logger.LogError("A confirmation prompt is already registered", Logger.LogType.Client, this);
                return;
            }
            _currentConfirmationPrompt = confirmationPrompt;
            _currentConfirmationPrompt.OnResponseReceived += () =>
            {
                Logger.LogTrace("Confirmation prompt unregistered after received response", Logger.LogType.Client, this);
                _currentConfirmationPrompt = null;
                if (_currentMenuIndex != -1)
                {
                    _menus[_currentMenuIndex].TrySelectDefault();
                }
            };
        }
    }
}