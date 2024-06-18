using System.Collections;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public abstract class MenuBase : MonoBehaviour
    {
        public abstract string MenuName { get; }
        [SerializeField] private Selectable _defaultSelectedOnOpen;
        private bool _isRegistered;
        
        public virtual void Start()
        {
            int sceneCount = SceneManager.sceneCount;
            bool isMenuV2SceneLoaded = false;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == SceneType.MenuV2Scene.ToString())
                {
                    isMenuV2SceneLoaded = true;
                    break;
                }
            }
            if (isMenuV2SceneLoaded)
            {
                Logger.LogTrace($"Loaded menu scene so registering {MenuName} online", Logger.LogType.Client,this);
                InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState; // for network reset
                StartCoroutine(TryRegisterMenuOnline()); // for scene transitions that don't causes network reset
            }
            else
            {
                Logger.LogTrace($"Menu scene is not loaded so registering {MenuName} local", Logger.LogType.Client,this);
                StartCoroutine(TryRegisterMenuLocal());
            }
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
                var currentScene = SceneManager.GetActiveScene().name;
                if (currentScene == SceneType.MenuV2Scene.ToString())
                {
                    _isRegistered = false;
                    StartCoroutine(TryRegisterMenuOnline());
                }
            }
        }

        private IEnumerator TryRegisterMenuOnline()
        {
            while(!UIManager.HasInstance) yield return null;
            if (_isRegistered) yield break;
            UIManager.Instance.RegisterMenu(this);
            _isRegistered = true;
        }
        
        private IEnumerator TryRegisterMenuLocal()
        {
            while(!UILocalManager.HasInstance) yield return null;
            if (_isRegistered) yield break;
            UILocalManager.Instance.RegisterMenu(this);
            _isRegistered = true;
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
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == SceneType.MenuV2Scene.ToString())
            {
                if (_defaultSelectedOnOpen && !UIManager.Instance.IsNavigationWithMouse)
                {
                    EventSystem.current.SetSelectedGameObject(_defaultSelectedOnOpen.gameObject);
                }
                else if (!_defaultSelectedOnOpen || UIManager.Instance.IsNavigationWithMouse)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            else
            {
                if (_defaultSelectedOnOpen && !UILocalManager.Instance.IsNavigationWithMouse)
                {
                    EventSystem.current.SetSelectedGameObject(_defaultSelectedOnOpen.gameObject);
                }
                else if (!_defaultSelectedOnOpen || UILocalManager.Instance.IsNavigationWithMouse)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
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
        
        protected void BindOneWayNavigableHorizontalOnRight(Selectable from, Selectable to)
        {
            Navigation nav1 = from.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav1.selectOnRight = to;
            from.navigation = nav1;
        }
        
        protected void BindOneWayNavigableHorizontalOnLeft(Selectable from, Selectable to)
        {
            Navigation nav1 = from.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav1.selectOnLeft = to;
            from.navigation = nav1;
        }
        
        protected void BindOneWayNavigableVerticalOnUp(Selectable from, Selectable to)
        {
            Navigation nav1 = from.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav1.selectOnUp = to;
            from.navigation = nav1;
        }
        
        protected void BindOneWayNavigableVerticalOnDown(Selectable from, Selectable to)
        {
            Navigation nav1 = from.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav1.selectOnDown = to;
            from.navigation = nav1;
        }
        
        public void SetDefaultSelectedOnOpen(Selectable selectable)
        {
            _defaultSelectedOnOpen = selectable;
        }
    }
}