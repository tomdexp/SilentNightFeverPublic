using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersMenu";
        [SerializeField] private Button _controlsButton;
        [SerializeField] private Button _audioButton;
        [SerializeField] private Button _graphicsButton;
        [SerializeField] private Button _backButton;
        private CanvasGroup _canvasGroup;
        
        private UI_Button _uiButtonControls;
        private UI_Button _uiButtonAudio;
        private UI_Button _uiButtonGraphics;
        private UI_Button _uiButtonBack;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            BindNavigableVertical(_controlsButton, _audioButton);
            BindNavigableVertical(_audioButton, _graphicsButton);
            BindNavigableVertical(_graphicsButton, _backButton);
            BindNavigableVertical(_backButton, _controlsButton);
            
            _uiButtonControls = _controlsButton.GetComponent<UI_Button>();
            _uiButtonAudio = _audioButton.GetComponent<UI_Button>();
            _uiButtonGraphics = _graphicsButton.GetComponent<UI_Button>();
            _uiButtonBack = _backButton.GetComponent<UI_Button>();
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _controlsButton.onClick.AddListener(ControlsButtonClicked);
            _audioButton.onClick.AddListener(AudioButtonClicked);
            _graphicsButton.onClick.AddListener(GraphicsButtonClicked);
            _backButton.onClick.AddListener(GoBack);
            
            float interval = 0.1f;
            var sequence = DOTween.Sequence(gameObject);
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonControls.Open());
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonAudio.Open());
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonGraphics.Open());
            sequence.AppendInterval(interval);
            sequence.AppendCallback(() => _uiButtonBack.Open());
            sequence.Play();
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventSettingsMenuStart, AudioManager.Instance.gameObject);

        }

        private void ControlsButtonClicked()
        {
            //UIManager.Instance.GoToMenu<ParametersControlsMenu>();
            UI.GoToMenu<ParametersControlsMenu>();
        }

        private void AudioButtonClicked()
        {
            //UIManager.Instance.GoToMenu<ParametersAudioMenu>();
            UI.GoToMenu<ParametersAudioMenu>();
        }

        private void GraphicsButtonClicked()
        {
            //UIManager.Instance.GoToMenu<ParametersGraphicsMenu>();
            UI.GoToMenu<ParametersGraphicsMenu>();
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _controlsButton.onClick.RemoveListener(ControlsButtonClicked);
            _audioButton.onClick.RemoveListener(AudioButtonClicked);
            _graphicsButton.onClick.RemoveListener(GraphicsButtonClicked);
            _backButton.onClick.RemoveListener(GoBack);
            _uiButtonControls.Close();
            _uiButtonAudio.Close();
            _uiButtonGraphics.Close();
            _uiButtonBack.Close();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _controlsButton.onClick.RemoveListener(ControlsButtonClicked);
            _audioButton.onClick.RemoveListener(AudioButtonClicked);
            _graphicsButton.onClick.RemoveListener(GraphicsButtonClicked);
            _backButton.onClick.RemoveListener(GoBack);
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UI.IsMenuV2SceneLoaded())
            {
                UI.GoToMenu<MainMenu>();
            }
            else
            {
                UI.GoToMenu<InGameMenu>();
            }
        }
    }
}