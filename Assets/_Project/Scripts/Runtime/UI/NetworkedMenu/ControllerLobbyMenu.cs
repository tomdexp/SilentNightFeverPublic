using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
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
        [SerializeField] private float _secondsBeforeStartWhenAllControllerConnected = 2.5f;
        private CanvasGroup _canvasGroup;
        private bool _timerStarted;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerJoiningEnabledClientRpc(true);
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerLeavingEnabledClientRpc(true);
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
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerJoiningEnabledClientRpc(false);
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerLeavingEnabledClientRpc(false);
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
            Logger.LogWarning("This should have a prompt confirmation because it will quit the lobby if online !");
        }
    }
}