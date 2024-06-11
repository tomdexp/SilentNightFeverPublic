using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameSettingsMenu : MenuBase
    {
        public override string MenuName { get; } = "GameSettingsMenu";
        [SerializeField, Required] private Button _startGameButton;
        [SerializeField, Required] private ConfirmationPrompt _quitGameSettingsPrompt;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
        }
        
        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToCanvasCamera();
            _canvasGroup.Open();
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
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
        }

        private void OnStartGameButtonClicked()
        {
            if (!InstanceFinder.IsServerStarted) return;
            if (GameManager.HasInstance) GameManager.Instance.LoadOnBoardingScene();
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