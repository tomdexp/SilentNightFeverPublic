using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ControllerLobbyMenu : MenuBase
    {
        public override string MenuName { get; } = "ControllerLobbyMenu";
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsHostPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsClientPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitLocalPrompt;
        [SerializeField] private float _secondsBeforeStartWhenAllControllerConnected = 2.5f;
        private CanvasGroup _canvasGroup;
        private bool _timerStarted;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _timerStarted = false;
            PlayerManager.Instance.SetPlayerJoiningEnabled(true);
            PlayerManager.Instance.SetPlayerLeavingEnabled(true);
            if(InstanceFinder.IsServerStarted) PlayerManager.Instance.ResetRealPlayerInfos();
        }

        private void Update()
        {
            if (!PlayerManager.HasInstance) return;
            if (!_timerStarted)
            {
                if (PlayerManager.Instance.NumberOfPlayers == 4)
                {
                    _timerStarted = true;
                    StartCoroutine(TimerBeforeNextScreen());
                }
            }
        }
        
        private IEnumerator TimerBeforeNextScreen()
        {
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerJoiningEnabledClientRpc(false);
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerLeavingEnabledClientRpc(false);
            yield return new WaitForSeconds(_secondsBeforeStartWhenAllControllerConnected);
            if (InstanceFinder.IsServerStarted)
            {
                UIManager.Instance.GoToMenu<PlayerIndexSelectionMenu>();
            }
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            PlayerManager.Instance.SetPlayerJoiningEnabled(false);
            PlayerManager.Instance.SetPlayerLeavingEnabled(false);
        }

        public override void GoBack()
        {
            base.GoBack();
            StartCoroutine(GoBackCoroutine());
        }
        
        private IEnumerator GoBackCoroutine()
        {
            if (InstanceFinder.IsServerStarted)
            {
                if (BootstrapManager.Instance.IsOnline) // Online Lobby Host
                {
                    _quitOnlineAsHostPrompt.Open();
                    yield return _quitOnlineAsHostPrompt.WaitForResponse();
                    if (_quitOnlineAsHostPrompt.IsSuccess)
                    {
                        BootstrapManager.Instance.TryLeaveOnline();
                        FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(nameof(PlayOnlineOrLocalMenu));
                        //UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
                    }
                }
                else // Local Lobby
                {
                    _quitLocalPrompt.Open();
                    yield return _quitLocalPrompt.WaitForResponse();
                    if (_quitLocalPrompt.IsSuccess)
                    {
                        UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
                    }
                }
            }
            else // Online Lobby Client
            {
                _quitOnlineAsClientPrompt.Open();
                yield return _quitOnlineAsClientPrompt.WaitForResponse();
                if (_quitOnlineAsClientPrompt.IsSuccess)
                {
                    BootstrapManager.Instance.TryLeaveOnline();
                    FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(nameof(PlayOnlineOrLocalMenu));
                    //UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
                }
            }
        }
    }
}