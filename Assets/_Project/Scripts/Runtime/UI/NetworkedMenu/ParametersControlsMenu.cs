using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersControlsMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersControlsMenu";
        
        [SerializeField, Required] private UI_Toggle _holdButtonToAnchorTongueToggle;
        [SerializeField, Required] private UI_Toggle _aimAssistToggle;
        [SerializeField, Required] private Button _remapMoveUpButton;
        [SerializeField, Required] private Button _remapMoveDownButton;
        [SerializeField, Required] private Button _remapMoveLeftButton;
        [SerializeField, Required] private Button _remapMoveRightButton;
        [SerializeField, Required] private Button _remapInteractButton;
        [SerializeField, Required] private Button _backButton;
        
        private CanvasGroup _canvasGroup;
        private Toggle _holdButtonToAnchorTongueToggleComponent;
        private Toggle _aimAssistToggleComponent;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            _holdButtonToAnchorTongueToggleComponent = _holdButtonToAnchorTongueToggle.GetComponentInChildren<Toggle>();
            _aimAssistToggleComponent = _aimAssistToggle.GetComponentInChildren<Toggle>();
            
            BindNavigableVertical(_backButton, _holdButtonToAnchorTongueToggleComponent);
            BindNavigableVertical(_holdButtonToAnchorTongueToggleComponent, _aimAssistToggleComponent);
            BindNavigableVertical(_aimAssistToggleComponent, _backButton);
            
            BindNavigableVertical(_remapMoveUpButton, _remapMoveDownButton);
            BindNavigableVertical(_remapMoveDownButton, _remapMoveLeftButton);
            BindNavigableVertical(_remapMoveLeftButton, _remapMoveRightButton);
            BindNavigableVertical(_remapMoveRightButton, _remapInteractButton);
            BindNavigableVertical(_remapInteractButton, _remapMoveUpButton);
            
            BindOneWayNavigableHorizontalOnRight(_holdButtonToAnchorTongueToggleComponent, _remapMoveUpButton);
            BindOneWayNavigableHorizontalOnRight(_aimAssistToggleComponent, _remapMoveUpButton);
            
            BindOneWayNavigableHorizontalOnLeft(_remapMoveUpButton, _holdButtonToAnchorTongueToggleComponent);
            BindOneWayNavigableHorizontalOnLeft(_remapMoveDownButton, _holdButtonToAnchorTongueToggleComponent);
            BindOneWayNavigableHorizontalOnLeft(_remapMoveLeftButton, _holdButtonToAnchorTongueToggleComponent);
            BindOneWayNavigableHorizontalOnLeft(_remapMoveRightButton, _holdButtonToAnchorTongueToggleComponent);
            BindOneWayNavigableHorizontalOnLeft(_remapInteractButton, _holdButtonToAnchorTongueToggleComponent);
            
            BindOneWayNavigableHorizontalOnRight(_remapMoveUpButton, _backButton);
            BindOneWayNavigableHorizontalOnRight(_remapMoveDownButton, _backButton);
            BindOneWayNavigableHorizontalOnRight(_remapMoveLeftButton, _backButton);
            BindOneWayNavigableHorizontalOnRight(_remapMoveRightButton, _backButton);
            BindOneWayNavigableHorizontalOnRight(_remapInteractButton, _backButton);
            
            BindOneWayNavigableHorizontalOnLeft(_backButton, _remapMoveUpButton);
            
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _backButton.onClick.AddListener(GoBack);
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventSettingsControlsStart, AudioManager.Instance.gameObject);

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

        public override void OnDestroy()
        {
            base.OnDestroy();
            _remapMoveUpButton.onClick.RemoveListener(GoBack);
        }
    }
}