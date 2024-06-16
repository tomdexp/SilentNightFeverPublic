using System;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
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
        [SerializeField, Required] private Button _languageSelectionButton;
        
        private CanvasGroup _canvasGroup;
        private UI_Button _uiButtonPlay;
        private UI_Button _uiButtonOptions;
        private UI_Button _uiButtonCredits;
        private UI_Button _uiButtonQuit;

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
            
            BindNavigableHorizontal(_languageSelectionButton, _playButton);
            BindNavigableHorizontal(_languageSelectionButton, _optionsButton);
            BindNavigableHorizontal(_languageSelectionButton, _creditsButton);
            BindNavigableHorizontal(_languageSelectionButton, _quitButton);
            
            _uiButtonPlay = _playButton.GetComponent<UI_Button>();
            _uiButtonOptions = _optionsButton.GetComponent<UI_Button>();
            _uiButtonCredits = _creditsButton.GetComponent<UI_Button>();
            _uiButtonQuit = _quitButton.GetComponent<UI_Button>();
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
            float interval = 0.2f;
            var sequence = DOTween.Sequence(gameObject);
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonPlay.Open());
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonOptions.Open());
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonCredits.Open());
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonQuit.Open());
            sequence.Play();
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventMainMenuStart, AudioManager.Instance.gameObject);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _playButton.onClick.RemoveListener(PlayButtonClicked);
            _optionsButton.onClick.RemoveListener(OptionsButtonClicked);
            _uiButtonPlay.Close();
            _uiButtonOptions.Close();
            _uiButtonCredits.Close();
            _uiButtonQuit.Close();
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
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<ParametersMenu>();
        }
        
        private void CreditsButtonClicked()
        {
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<CreditsMenu>();
        }

        private void QuitButtonClicked()
        {
            Application.Quit();
        }
    }
}