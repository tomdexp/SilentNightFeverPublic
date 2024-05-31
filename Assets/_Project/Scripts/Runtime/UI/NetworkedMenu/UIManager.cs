using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class UIManager : NetworkSingleton<UIManager>
    {
        [SerializeField] private InputAction _goBackAction;
        [SerializeField] private List<MenuBase> _menus = new List<MenuBase>();
        private readonly SyncVar<int> _currentMenuIndex = new SyncVar<int>(-1);

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _currentMenuIndex.OnChange += OnCurrentMenuIndexChanged;
            _goBackAction.performed += OnGoBack;
            _goBackAction.Enable();
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _currentMenuIndex.OnChange -= OnCurrentMenuIndexChanged;
            _goBackAction.Disable();
            _goBackAction.performed -= OnGoBack;
        }
        
        private void OnGoBack(InputAction.CallbackContext context)
        {
            if (_currentMenuIndex.Value == -1)
            {
                Logger.LogError("No menu to go back to", Logger.LogType.Client,this);
                return;
            }
            _menus[_currentMenuIndex.Value].GoBack();
        }

        private void OnCurrentMenuIndexChanged(int prev, int next, bool asServer)
        {
            if (prev != -1)
            {
                _menus[prev].Close();
                Logger.LogTrace($"Menu changed from {GetMenuType(prev).Name} to {GetMenuType(next).Name}", Logger.LogType.Client,this);
            }
            else
            {
                Logger.LogTrace($"Opening first menu {GetMenuType(next).Name}", Logger.LogType.Client,this);
            }
            _menus[next].Open();
        }

        [Server]
        public void GoToMenu<T>() where T : MenuBase
        {
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
            if (menu is PressStartMenu)
            {
                GoToMenu<PressStartMenu>();
            }
        }
        
        private void SortMenuByNames()
        {
            _menus = _menus.OrderBy(menu => menu.MenuName).ToList();
        }
    }
}