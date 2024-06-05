using System;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MainMenu : MenuBase
    {
        public override string MenuName { get; } = "MainMenu";
        [SerializeField, Required] private Button _playButton;
        [SerializeField, Required] private Button _optionsButton;
        [SerializeField, Required] private Button _creditsButton;
        [SerializeField, Required] private Button _quitButton;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            if (!_playButton)
            {
                Logger.LogError("Play Button not set", Logger.LogType.Client, this);
            }
            if (!_optionsButton)
            {
                Logger.LogError("Options Button not set", Logger.LogType.Client, this);
            }
            if (!_creditsButton)
            {
                Logger.LogError("Credits Button not set", Logger.LogType.Client, this);
            }
            if (!_quitButton)
            {
                Logger.LogError("Quit Button not set", Logger.LogType.Client, this);
            }
            BindNavigableVertical(_playButton, _optionsButton);
            BindNavigableVertical(_optionsButton, _creditsButton);
            BindNavigableVertical(_creditsButton, _quitButton);
            BindNavigableVertical(_quitButton, _playButton);
        }

        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToCanvasCamera();
            _canvasGroup.Open();
            _playButton.onClick.AddListener(PlayButtonClicked);
            _optionsButton.onClick.AddListener(OptionsButtonClicked);
            _creditsButton.onClick.AddListener(CreditsButtonClicked);
            _quitButton.onClick.AddListener(QuitButtonClicked);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _playButton.onClick.RemoveListener(PlayButtonClicked);
            _optionsButton.onClick.RemoveListener(OptionsButtonClicked);
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<PressStartMenu>();
        }
        
        private void PlayButtonClicked()
        {
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
        }
        
        private void OptionsButtonClicked()
        {
            Logger.LogWarning("Options not implemented yet", Logger.LogType.Client, this);
        }
        
        private void CreditsButtonClicked()
        {
            Logger.LogWarning("Credits not implemented yet", Logger.LogType.Client, this);
        }

        private void QuitButtonClicked()
        {
            Logger.LogWarning("Quit not implemented yet", Logger.LogType.Client, this);
        }
    }
}