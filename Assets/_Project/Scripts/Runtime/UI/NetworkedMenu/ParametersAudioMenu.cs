using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersAudioMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersAudioMenu";
        [SerializeField, Required] private Button _backButton;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderMusic;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderAmbiance;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderSFX;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderLandmarks;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderHighPass;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderLowPass;
        [SerializeField, Required] private UI_BindSliderToRTPC _audioSliderNotch;
        
        private CanvasGroup _canvasGroup;
        private Slider _sliderMusic;
        private Slider _sliderAmbiance;
        private Slider _sliderSFX;
        private Slider _sliderLandmarks;
        private Slider _sliderHighPass;
        private Slider _sliderLowPass;
        private Slider _sliderNotch;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            _sliderMusic = _audioSliderMusic.GetComponentInChildren<Slider>();
            _sliderAmbiance = _audioSliderAmbiance.GetComponentInChildren<Slider>();
            _sliderSFX = _audioSliderSFX.GetComponentInChildren<Slider>();
            _sliderLandmarks = _audioSliderLandmarks.GetComponentInChildren<Slider>();
            _sliderHighPass = _audioSliderHighPass.GetComponentInChildren<Slider>();
            _sliderLowPass = _audioSliderLowPass.GetComponentInChildren<Slider>();
            _sliderNotch = _audioSliderNotch.GetComponentInChildren<Slider>();
            
            BindNavigableVertical(_sliderMusic, _sliderAmbiance);
            BindNavigableVertical(_sliderAmbiance, _sliderSFX);
            BindNavigableVertical(_sliderSFX, _sliderLandmarks);
            BindNavigableVertical(_sliderLandmarks, _sliderHighPass);
            BindNavigableVertical(_sliderHighPass, _sliderLowPass);
            BindNavigableVertical(_sliderLowPass, _sliderNotch);
            BindNavigableVertical(_sliderNotch, _backButton);
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _backButton.onClick.AddListener(GoBack);
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