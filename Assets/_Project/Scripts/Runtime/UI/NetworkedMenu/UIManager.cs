using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Utils;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Cinemachine;
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
            FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(_menus[next].MenuName);
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
            CheckAllMenuRegistered();
        }

        private void CheckAllMenuRegistered()
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
            
            if (allMenusRegistered)
            {
                Logger.LogTrace("All menus registered", Logger.LogType.Client, this);
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
    }
}