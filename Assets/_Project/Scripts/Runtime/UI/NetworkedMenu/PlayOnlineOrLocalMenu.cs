using System;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayOnlineOrLocalMenu : MenuBase
    {
        public override string MenuName { get; } = "PlayOnlineOrLocalMenu";
        [SerializeField, Required] private Button _playOnlineButton;
        [SerializeField, Required] private Button _playLocalButton;
        [SerializeField, Required] private Button _goBackButton;
        [SerializeField, Required] private CanvasGroup _onlineFadeCanvasGroup;
        [SerializeField, Required] private CanvasGroup _localFadeCanvasGroup;
        private CanvasGroup _canvasGroup;
        
        private UI_Button _uiButtonPlayOnline;
        private UI_Button _uiButtonPlayLocal;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            if (!_playOnlineButton)
            {
                Logger.LogError("Play Online Button not set", context:this);
            }
            if (!_playLocalButton)
            {
                Logger.LogError("Play Local Button not set", context:this);
            }
            
            BindNavigableVertical(_goBackButton, _playOnlineButton);
            BindNavigableHorizontal(_goBackButton, _playOnlineButton);
            BindNavigableHorizontal(_playOnlineButton, _playLocalButton);
            BindNavigableHorizontal(_playLocalButton, _playOnlineButton); // Loop back
            
            _uiButtonPlayOnline = _playOnlineButton.GetComponent<UI_Button>();
            _uiButtonPlayLocal = _playLocalButton.GetComponent<UI_Button>();
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            UIManager.Instance.SwitchToCanvasCamera();
            _playOnlineButton.onClick.AddListener(PlayOnlineButtonClicked);
            _playLocalButton.onClick.AddListener(PlayLocalButtonClicked);
            _goBackButton.onClick.AddListener(GoBack);
            _uiButtonPlayOnline.OnHover += OnButtonPlayOnlineHover;
            _uiButtonPlayOnline.OnUnHover += OnButtonPlayOnlineUnHover;
            _uiButtonPlayLocal.OnHover += OnButtonPlayLocalHover;
            _uiButtonPlayLocal.OnUnHover += OnButtonPlayLocalUnHover;
            _uiButtonPlayLocal.Open();
            _uiButtonPlayOnline.Open();
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventLocalOrOnlineMenuStart, AudioManager.Instance.gameObject);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _playOnlineButton.onClick.RemoveListener(PlayOnlineButtonClicked);
            _playLocalButton.onClick.RemoveListener(PlayLocalButtonClicked);
            _goBackButton.onClick.RemoveListener(GoBack);
            _uiButtonPlayOnline.OnHover -= OnButtonPlayOnlineHover;
            _uiButtonPlayOnline.OnUnHover -= OnButtonPlayOnlineUnHover;
            _uiButtonPlayLocal.OnHover -= OnButtonPlayLocalHover;
            _uiButtonPlayLocal.OnUnHover -= OnButtonPlayLocalUnHover;
            _uiButtonPlayLocal.Close();
            _uiButtonPlayOnline.Close();
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<MainMenu>();
        }
        
        private void PlayOnlineButtonClicked()
        {
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventOnClickedOnlineGame, AudioManager.Instance.gameObject);
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<CreateOrJoinOnlineMenu>();
        }
        
        private void PlayLocalButtonClicked()
        {
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventOnClickedLocalGame, AudioManager.Instance.gameObject);
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<ControllerLobbyMenu>();
        }
        
        private void OnButtonPlayOnlineHover()
        {
            _onlineFadeCanvasGroup.Close();
            _onlineFadeCanvasGroup.interactable = false;
            _onlineFadeCanvasGroup.blocksRaycasts = false;
        }

        private void OnButtonPlayOnlineUnHover()
        {
            _onlineFadeCanvasGroup.Open();
            _onlineFadeCanvasGroup.interactable = false;
            _onlineFadeCanvasGroup.blocksRaycasts = false;
        }

        private void OnButtonPlayLocalHover()
        {
            _localFadeCanvasGroup.Close();
            _localFadeCanvasGroup.interactable = false;
            _localFadeCanvasGroup.blocksRaycasts = false;
        }

        private void OnButtonPlayLocalUnHover()
        {
            _localFadeCanvasGroup.Open();
            _localFadeCanvasGroup.interactable = false;
            _localFadeCanvasGroup.blocksRaycasts = false;
        }
    }
}