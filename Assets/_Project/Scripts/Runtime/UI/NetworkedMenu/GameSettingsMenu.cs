using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameSettingsMenu : MenuBase
    {
        public override string MenuName { get; } = "GameSettingsMenu";
        [SerializeField, Required] private UI_SelectorBase _roundNumberSelector;
        [SerializeField, Required] private UI_SelectorBase _cameraAngleSelector;
        [SerializeField, Required] private UI_Toggle _cameraEffectsToggle;
        [SerializeField, Required] private UI_Toggle _controlEffectsToggle;
        [SerializeField, Required] private Button _startGameButton;
        [SerializeField, Required] private Button _backButton;
        [SerializeField, Required] private ConfirmationPrompt _quitGameSettingsPrompt;
        [SerializeField, Required] private TMP_Text _waitingForHostText;
        private CanvasGroup _canvasGroup;
        
        private Toggle _cameraEffectsToggleComponent;
        private Toggle _controlEffectsToggleComponent;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            _cameraEffectsToggleComponent = _cameraEffectsToggle.GetComponentInChildren<Toggle>();
            _controlEffectsToggleComponent = _controlEffectsToggle.GetComponentInChildren<Toggle>();
            
            BindNavigableVertical(_backButton, _roundNumberSelector.PreviousButton);
            BindOneWayNavigableVerticalOnUp(_roundNumberSelector.NextButton, _backButton);
            
            BindNavigableHorizontal(_roundNumberSelector.PreviousButton, _roundNumberSelector.NextButton);
            BindNavigableHorizontal(_roundNumberSelector.NextButton, _roundNumberSelector.PreviousButton);
            BindNavigableVertical(_roundNumberSelector.PreviousButton, _cameraAngleSelector.PreviousButton);
            BindNavigableVertical(_roundNumberSelector.NextButton, _cameraAngleSelector.NextButton);
            BindNavigableHorizontal(_cameraAngleSelector.PreviousButton, _cameraAngleSelector.NextButton);
            BindNavigableHorizontal(_cameraAngleSelector.NextButton, _cameraAngleSelector.PreviousButton);
            
            BindNavigableVertical(_cameraAngleSelector.PreviousButton, _cameraEffectsToggleComponent);
            BindNavigableVertical(_cameraAngleSelector.NextButton, _controlEffectsToggleComponent);
            
            BindNavigableHorizontal(_cameraEffectsToggleComponent, _controlEffectsToggleComponent);
            
            BindOneWayNavigableVerticalOnDown(_cameraEffectsToggleComponent, _startGameButton);
            BindOneWayNavigableVerticalOnDown(_controlEffectsToggleComponent, _startGameButton);
            BindOneWayNavigableVerticalOnUp(_startGameButton, _cameraEffectsToggleComponent);
            BindOneWayNavigableVerticalOnDown(_startGameButton, _backButton);
        }
        
        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToCanvasCamera();
            _canvasGroup.Open();
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            _backButton.onClick.AddListener(GoBack);

            if (!InstanceFinder.IsServerStarted)
            {
                _waitingForHostText.alpha = 1;
                _roundNumberSelector.gameObject.SetActive(false);
                _cameraAngleSelector.gameObject.SetActive(false);
                _cameraEffectsToggle.gameObject.SetActive(false);
                _controlEffectsToggle.gameObject.SetActive(false);
                _startGameButton.GetComponent<UI_Button>().Lock();
                
            }
            else
            {
                _waitingForHostText.alpha = 0;
            }
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
            _backButton.onClick.RemoveListener(GoBack);
        }
        
        public override void GoBack()
        {
            base.GoBack();
            if (!InstanceFinder.IsServerStarted) return;
            StartCoroutine(GoBackCoroutine());
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
            _backButton.onClick.RemoveListener(GoBack);
        }

        private void OnStartGameButtonClicked()
        {
            if (!InstanceFinder.IsServerStarted) return;
            if (GameManager.HasInstance)
            {
                UI_SelectorRoundNumber roundNumberSelector = (UI_SelectorRoundNumber)_roundNumberSelector;
                GameManager.Instance.NumberOfRoundFromGameSettings = roundNumberSelector.SelectedRoundNumber;
                GameManager.Instance.LoadOnBoardingScene();
            }
        }

        private IEnumerator GoBackCoroutine()
        {
            _quitGameSettingsPrompt.Open();
            yield return _quitGameSettingsPrompt.WaitForResponse();
            if (_quitGameSettingsPrompt.IsSuccess)
            {
                UIManager.Instance.GoToMenu<CustomizationMenu>();
            }
        }
    }
}