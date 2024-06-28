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
    public class InGameMenu : MenuBase
    {
        public override string MenuName { get; } = "InGameMenu";
        [SerializeField, Required] private Button _goBackButton;
        [SerializeField, Required] private Button _buttonResume;
        [SerializeField, Required] private Button _buttonSettings;
        [SerializeField, Required] private Button _buttonQuitToMainMenu;
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsHostPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsClientPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitLocalPrompt;
        
        private CanvasGroup _canvasGroup;
        private float _secondBeforeCanOpenAgain = 0.5f; // to avoid input race between closing and opening
        private WaitForSeconds _waitForSeconds;
        public bool CanOpen = true;
        
        private void Awake()
        {
            CanOpen = true;
            
            _waitForSeconds = new WaitForSeconds(_secondBeforeCanOpenAgain);
            
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            BindNavigableHorizontal(_goBackButton, _buttonResume);
            BindNavigableHorizontal(_buttonResume, _goBackButton);
            
            BindNavigableVertical(_goBackButton, _buttonResume);
            BindNavigableVertical(_buttonResume, _buttonSettings);
            BindNavigableVertical(_buttonSettings, _buttonQuitToMainMenu);
            BindNavigableVertical(_buttonQuitToMainMenu, _goBackButton);
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _goBackButton.onClick.AddListener(GoBack);
            _buttonResume.onClick.AddListener(GoBack);
            _buttonSettings.onClick.AddListener(OnButtonSettingsClicked);
            _buttonQuitToMainMenu.onClick.AddListener(OnButtonQuitToMainMenuClicked);
            PlayerManager.Instance.DisablePlayerInputs((byte)InstanceFinder.ClientManager.Connection.ClientId);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _goBackButton.onClick.RemoveListener(GoBack);
            _buttonResume.onClick.RemoveListener(GoBack);
            _buttonSettings.onClick.RemoveListener(OnButtonSettingsClicked);
            _buttonQuitToMainMenu.onClick.RemoveListener(OnButtonQuitToMainMenuClicked);
            PlayerManager.Instance.EnablePlayerInputs((byte)InstanceFinder.ClientManager.Connection.ClientId);
            StartCoroutine(CooldownBeforeCanOpenAgain());
        }

        public override void GoBack()
        {
            base.GoBack();
            UI.GoToMenu<InGameNoUI>();
            StartCoroutine(CooldownBeforeCanOpenAgain());
        }
        
        private void OnButtonSettingsClicked()
        {
            //UILocalManager.Instance.GoToMenu<ParametersMenu>();
            UI.GoToMenu<ParametersMenu>();
        }
        
        private void OnButtonQuitToMainMenuClicked()
        {
            StartCoroutine(QuitToMainMenuCoroutine());
        }
        
        private IEnumerator QuitToMainMenuCoroutine()
        {
            if (InstanceFinder.IsServerStarted)
            {
                if (BootstrapManager.Instance.IsOnline) // Online Lobby Host
                {
                    _quitOnlineAsHostPrompt.Open();
                    yield return _quitOnlineAsHostPrompt.WaitForResponse();
                    if (_quitOnlineAsHostPrompt.IsSuccess)
                    {
                        BootstrapManager.Instance.TryLeaveOnline();
                        GameManager.Instance.MenuToGoOnResetAfterLoadingScene = nameof(MainMenu);
                        GameManager.Instance.LoadMenuScene();
                        GameManager.Instance.RestoreDefaultGameSettings();
                        GameManager.Instance.ResetGame();
                        PlayerManager.Instance.ResetRealPlayerInfos();
                        Close();
                    }
                }
                else // Local Lobby
                {
                    _quitLocalPrompt.Open();
                    yield return _quitLocalPrompt.WaitForResponse();
                    if (_quitLocalPrompt.IsSuccess)
                    {
                        Close();
                        yield return new WaitUntil((() => GameManager.HasInstance));
                        GameManager.Instance.MenuToGoOnResetAfterLoadingScene = nameof(MainMenu);
                        GameManager.Instance.LoadMenuScene();
                        GameManager.Instance.RestoreDefaultGameSettings();
                        GameManager.Instance.ResetGame();
                        yield return new WaitUntil((() => PlayerManager.HasInstance));
                        PlayerManager.Instance.ResetRealPlayerInfos();
                    }
                }
            }
            else // Online Lobby Client
            {
                _quitOnlineAsClientPrompt.Open();
                yield return _quitOnlineAsClientPrompt.WaitForResponse();
                if (_quitOnlineAsClientPrompt.IsSuccess)
                {
                    BootstrapManager.Instance.TryLeaveOnline();
                    GameManager.Instance.MenuToGoOnResetAfterLoadingScene = nameof(MainMenu);
                    GameManager.Instance.LoadMenuScene();
                    GameManager.Instance.RestoreDefaultGameSettings();
                    GameManager.Instance.ResetGame();
                    PlayerManager.Instance.ResetRealPlayerInfos();
                    Close();
                }
            }
        }

        private IEnumerator CooldownBeforeCanOpenAgain()
        {
            if (!CanOpen) yield break;
            CanOpen = false;
            yield return _waitForSeconds;
            CanOpen = true;
        }
    }
}