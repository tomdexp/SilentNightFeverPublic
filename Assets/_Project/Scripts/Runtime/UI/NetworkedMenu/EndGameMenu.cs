using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class EndGameMenu : MenuBase
    {
        public override string MenuName { get; } = "EndGameMenu";

        [SerializeField, Required] private UI_Button _restartButton;
        [SerializeField, Required] private UI_Button _teamSelectionButton;
        [SerializeField, Required] private UI_Button _mainMenuButton;
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsHostPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsClientPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitLocalPrompt;

        private CanvasGroup _canvasGroup;
        private Button _buttonRestart;
        private Button _buttonTeamSelection;
        private Button _buttonMainMenu;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();

            _buttonRestart = _restartButton.GetComponentInChildren<Button>();
            _buttonTeamSelection = _teamSelectionButton.GetComponentInChildren<Button>();
            _buttonMainMenu = _mainMenuButton.GetComponentInChildren<Button>();

            BindNavigableVertical(_buttonRestart, _buttonTeamSelection);
            BindNavigableVertical(_buttonTeamSelection, _buttonMainMenu);
            BindNavigableVertical(_buttonMainMenu, _buttonRestart);

            StartCoroutine(TrySubscribeToGameEnd());

            _buttonRestart.onClick.AddListener(OnRestartButtonClicked);
            _buttonTeamSelection.onClick.AddListener(OnTeamSelectionButtonClicked);
            _buttonMainMenu.onClick.AddListener(OnMainMenuButtonClicked);
        }

        private IEnumerator TrySubscribeToGameEnd()
        {
            while (!GameManager.HasInstance) yield return null;
            GameManager.Instance.OnGameEndedSyncEvent.OnEvent += OnGameEnded;
        }

        private void OnGameEnded(bool asServer)
        {
            Open();
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            if (InstanceFinder.IsServerStarted)
            {
                if (!UILocalManager.Instance.IsNavigationWithMouse) _buttonRestart.Select();
                SetDefaultSelectedOnOpen(_buttonRestart);
            }
            else
            {
                _restartButton.Lock();
                _teamSelectionButton.Lock();
                if (!UILocalManager.Instance.IsNavigationWithMouse) _buttonMainMenu.Select();
                SetDefaultSelectedOnOpen(_buttonMainMenu);
            }
            StartCoroutine(TransitionManager.Instance.EndLoadingRoundTransition());
        }
        

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (GameManager.HasInstance) GameManager.Instance.OnGameEndedSyncEvent.OnEvent -= OnGameEnded;
        }

        private void OnRestartButtonClicked()
        {
            if (!InstanceFinder.IsServerStarted) return;
            GameManager.Instance.ResetAndRestartGame();
            Close();
        }

        private void OnTeamSelectionButtonClicked()
        {
            if (!InstanceFinder.IsServerStarted) return;
            Close();
            GameManager.Instance.MenuToGoOnResetAfterLoadingScene = nameof(PlayerIndexSelectionMenu);
            GameManager.Instance.LoadMenuScene();
            GameManager.Instance.RestoreDefaultGameSettings();
            GameManager.Instance.ResetGame();
        }

        private void OnMainMenuButtonClicked()
        {
            StartCoroutine(OnMainMenuButtonClickedCoroutine());
        }

        private IEnumerator OnMainMenuButtonClickedCoroutine()
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
    }
}