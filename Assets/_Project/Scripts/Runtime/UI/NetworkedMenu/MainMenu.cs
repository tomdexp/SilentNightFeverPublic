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
            _canvasGroup.alpha = 0;
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
        }

        public override void Open()
        {
            base.Open();
            
            UIManager.Instance.SwitchToCanvasCamera();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _playButton.onClick.AddListener(PlayButtonClicked);
            _optionsButton.onClick.AddListener(OptionsButtonClicked);
        }

        

        public override void Close()
        {
            base.Close();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
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
            throw new NotImplementedException();
        }
    }
}