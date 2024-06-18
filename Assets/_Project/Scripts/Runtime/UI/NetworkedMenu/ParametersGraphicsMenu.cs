using System;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersGraphicsMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersGraphicsMenu";
        [SerializeField, Required] private UI_Toggle _highContrastToggle;
        [SerializeField, Required] private UI_Toggle _epilepsyModeToggle;
        [SerializeField, Required] private UI_Toggle _fullScreenToggle;
        [SerializeField, Required] private UI_Toggle _vsyncToggle;
        [SerializeField, Required] private UI_SelectorBase _resolutionSlider;
        [SerializeField, Required] private Button _backButton;
        private CanvasGroup _canvasGroup;
        
        private Toggle _highContrastToggleComponent;
        private Toggle _epilepsyModeToggleComponent;
        private Toggle _fullScreenToggleComponent;
        private Toggle _vsyncToggleComponent;
        private Button _resolutionSliderPreviousButton;
        private Button _resolutionSliderNextButton;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();

            _highContrastToggleComponent = _highContrastToggle.GetComponentInChildren<Toggle>();
            _epilepsyModeToggleComponent = _epilepsyModeToggle.GetComponentInChildren<Toggle>();
            _fullScreenToggleComponent = _fullScreenToggle.GetComponentInChildren<Toggle>();
            _vsyncToggleComponent = _vsyncToggle.GetComponentInChildren<Toggle>();
            _resolutionSliderPreviousButton = _resolutionSlider.PreviousButton;
            _resolutionSliderNextButton = _resolutionSlider.NextButton;
            
            BindNavigableVertical(_backButton, _highContrastToggleComponent);
            BindNavigableVertical(_highContrastToggleComponent, _epilepsyModeToggleComponent);
            BindNavigableVertical(_epilepsyModeToggleComponent, _fullScreenToggleComponent);
            BindNavigableVertical(_fullScreenToggleComponent, _vsyncToggleComponent);
            BindNavigableVertical(_vsyncToggleComponent, _resolutionSliderPreviousButton);
            
            BindNavigableHorizontal(_resolutionSliderPreviousButton, _resolutionSliderNextButton);
            
            BindOneWayNavigableVerticalOnUp(_resolutionSliderNextButton, _vsyncToggleComponent);
            BindOneWayNavigableVerticalOnDown(_resolutionSliderNextButton, _backButton);
            
            BindNavigableVertical(_resolutionSliderPreviousButton, _backButton);
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _backButton.onClick.AddListener(GoBack);
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventSettingsGraphicStart, AudioManager.Instance.gameObject);
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _backButton.onClick.RemoveListener(GoBack);
        }

        public override void GoBack()
        {
            base.GoBack();
            UIManager.Instance.GoToMenu<ParametersMenu>();
        }
    }
}