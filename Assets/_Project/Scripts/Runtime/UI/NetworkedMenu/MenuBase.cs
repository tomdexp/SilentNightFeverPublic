using System;
using System.Collections;
using FishNet;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public abstract class MenuBase : MonoBehaviour
    {
        public abstract string MenuName { get; }
        [SerializeField] private Selectable _defaultSelectedOnOpen;

        public virtual void Start()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        public virtual void OnDestroy()
        {
            if (InstanceFinder.ClientManager)
            {
                InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            }
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                StartCoroutine(TryRegisterMenu());
            }
        }

        private IEnumerator TryRegisterMenu()
        {
            while(!UIManager.HasInstance) yield return null;
            UIManager.Instance.RegisterMenu(this);
        }

        public virtual void Open()
        {
            Logger.LogTrace($"Opening {MenuName}", Logger.LogType.Client,this);
            TrySelectDefault();
        }

        public virtual void Close()
        {
            Logger.LogTrace($"Closing {MenuName}", Logger.LogType.Client,this);
        }

        public virtual void GoBack()
        {
            Logger.LogTrace($"Going back from {MenuName}", Logger.LogType.Client,this);
        }
        
        public void TrySelectDefault()
        {
            if (_defaultSelectedOnOpen && !UIManager.Instance.IsNavigationWithMouse)
            {
                EventSystem.current.SetSelectedGameObject(_defaultSelectedOnOpen.gameObject);
            }
            else if (!_defaultSelectedOnOpen)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        
        protected void BindNavigableVertical(Selectable selectable1, Selectable selectable2)
        {
            Navigation nav1 = selectable1.navigation;
            Navigation nav2 = selectable2.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav2.mode = Navigation.Mode.Explicit;
            nav1.selectOnDown = selectable2;
            nav2.selectOnUp = selectable1;
            selectable1.navigation = nav1;
            selectable2.navigation = nav2;
        }

        protected void BindNavigableHorizontal(Selectable selectable1, Selectable selectable2)
        {
            Navigation nav1 = selectable1.navigation;
            Navigation nav2 = selectable2.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav2.mode = Navigation.Mode.Explicit;
            nav1.selectOnRight = selectable2;
            nav2.selectOnLeft = selectable1;
            selectable1.navigation = nav1;
            selectable2.navigation = nav2;
        }
    }
}