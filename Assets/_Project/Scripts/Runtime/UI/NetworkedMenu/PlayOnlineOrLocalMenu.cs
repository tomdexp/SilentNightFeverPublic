using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayOnlineOrLocalMenu : MenuBase
    {
        public override string MenuName { get; } = "PlayOnlineOrLocalMenu";
        [SerializeField, Required] private Button _playOnlineButton;
        [SerializeField, Required] private Button _playLocalButton;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            if (!_playOnlineButton)
            {
                Debug.LogError("Play Online Button not set");
            }
            if (!_playLocalButton)
            {
                Debug.LogError("Play Local Button not set");
            }
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            UIManager.Instance.SwitchToCanvasCamera();
            _playOnlineButton.onClick.AddListener(PlayOnlineButtonClicked);
            _playLocalButton.onClick.AddListener(PlayLocalButtonClicked);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _playOnlineButton.onClick.RemoveListener(PlayOnlineButtonClicked);
            _playLocalButton.onClick.RemoveListener(PlayLocalButtonClicked);
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<MainMenu>();
        }
        
        private void PlayOnlineButtonClicked()
        {
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<CreateOrJoinOnlineMenu>();
        }
        
        private void PlayLocalButtonClicked()
        {
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<ControllerLobbyMenu>();
        }
    }
}