using _Project.Scripts.Runtime.Utils;
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
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            BindNavigableVertical(_controlsButton, _audioButton);
            BindNavigableVertical(_audioButton, _graphicsButton);
            BindNavigableVertical(_graphicsButton, _backButton);
            BindNavigableVertical(_backButton, _controlsButton);
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _controlsButton.onClick.AddListener(ControlsButtonClicked);
            _audioButton.onClick.AddListener(AudioButtonClicked);
            _graphicsButton.onClick.AddListener(GraphicsButtonClicked);
            _backButton.onClick.AddListener(GoBack);
        }

        private void ControlsButtonClicked()
        {
            UIManager.Instance.GoToMenu<ParametersControlsMenu>();
        }

        private void AudioButtonClicked()
        {
            UIManager.Instance.GoToMenu<ParametersAudioMenu>();
        }

        private void GraphicsButtonClicked()
        {
            //throw new System.NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _controlsButton.onClick.RemoveListener(ControlsButtonClicked);
            _audioButton.onClick.RemoveListener(AudioButtonClicked);
            _graphicsButton.onClick.RemoveListener(GraphicsButtonClicked);
            _backButton.onClick.RemoveListener(GoBack);
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
            UIManager.Instance.GoToMenu<MainMenu>();
        }
    }
}