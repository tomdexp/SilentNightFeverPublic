using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public class Menu : MonoBehaviour
    {
        protected Menu _parentMenu;

        [Space(20)]
        [Title("    Selectables"), Tooltip("Default selected element when opening the menu")]
        [SerializeField] protected Selectable _defaultSelectable;
        protected Selectable _lastSelectable;
        public bool AlreadyOpened { get; protected set; }

        [Space(20)]
        [Title("    Events")]
        public UnityEvent OnMenuEnter;
        public UnityEvent OnMenuExit;
        public static Action OnMenuEnterAction;
        public static Action OnMenuExitAction;

        // Setup variables
        protected bool _needSetup = false;
        protected bool _isSetup = false;

        private void Awake()
        { }

        private void Reset()
        { }

        public virtual bool OpenMenu(bool selectLastSelectable = true)
        {
            if (_needSetup && _needSetup != _isSetup)
            {
                Debug.LogWarning("This menu needs setup to be opened");
                return false;
            }

            gameObject.SetActive(true);

            AlreadyOpened = true;
            OnMenuEnter?.Invoke();
            OnMenuEnterAction?.Invoke();

            if (_defaultSelectable)
            {
                EventSystem.current.SetSelectedGameObject(_defaultSelectable.gameObject);
            }
            if (selectLastSelectable && _lastSelectable)
            {
                EventSystem.current.SetSelectedGameObject(_lastSelectable.gameObject);
            }
            return true;
        }

        public void ExitMenu()
        {
            gameObject.SetActive(false);

            EventSystem.current.currentSelectedGameObject?
                .TryGetComponent(out _lastSelectable);
            _isSetup = false;

            AlreadyOpened = true;
            OnMenuExit?.Invoke();
            OnMenuExitAction?.Invoke();

        }

        public bool OpenSubMenu(Menu subMenu, bool closeCurrentMenu = true, bool selectLastSelectable = true)
        {
            if (!subMenu) return false;

            if (!subMenu.CanMenuBeOpened()) return false;

            if (closeCurrentMenu)
                ExitMenu();

            subMenu.SetParentMenu(this);

            return subMenu.OpenMenu(selectLastSelectable);

        }

        public bool OpenSubMenu(Menu subMenu)
        {
            return OpenSubMenu(subMenu, false, false);
        }

        public bool OpenSubMenuAndCloseCurrentMenu(Menu subMenu)
        {
            return OpenSubMenu(subMenu, true, false);
        }

        public bool OpenParentMenu(bool selectLastSelectable = true)
        {
            if (!_parentMenu)
            {
                Debug.LogWarning("This menu doesn't have any parent menu");
                return false;
            }
            if (!_parentMenu.CanMenuBeOpened()) return false;

            ExitMenu();
            return _parentMenu.OpenMenu(_parentMenu);
        }

        public void SetParentMenu(Menu parentMenu)
        {
            this._parentMenu = parentMenu;
        }

        public virtual void SetupMenu()
        {
            Debug.Log("this menu doesn't need to be setup");
        }

        protected bool CanMenuBeOpened()
        {
            if (_needSetup && _needSetup != _isSetup)
            {
                Debug.LogWarning("This menu needs setup to be opened");
                return false;
            }
            return true;
        }

        #region Utilities
        private bool TryFindParentMenu(out Menu parentMenu)
        {
            Menu[] AllMenus = FindObjectsByType<Menu>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var menu in AllMenus)
            {
                if (menu._parentMenu == this)
                {
                    parentMenu = menu;
                    return true;
                }
            }
            parentMenu = null;
            return false;
        }
        #endregion

        #region Delegates
        public void OpenMenuDelegate(bool selectLastSelectable = true)
        {
            OpenMenu(selectLastSelectable);
        }

        public void ExitMenuDelegate()
        {
            ExitMenu();
        }

        public void OpenSubMenuDelegate(Menu subMenu)
        {
            OpenSubMenu(subMenu);
        }

        public void OpenSubMenuAndCloseCurrentMenuDelegate(Menu subMenu)
        {
            OpenSubMenuAndCloseCurrentMenu(subMenu);
        }

        void OpenParentMenuDelegate(bool selectLastSelectable = true)
        {
            OpenParentMenu(selectLastSelectable);
        }

        public virtual void SetupMenuDelegate()
        {
            SetupMenu();
        }

        #endregion
    }
}