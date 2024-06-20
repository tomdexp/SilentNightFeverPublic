using System;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
using FishNet;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CreateOrJoinOnlineMenu : MenuBase
    {
        public override string MenuName { get; } = "CreateOrJoinOnlineMenu";
        [SerializeField, Required] private Button _createLobbyButton;
        [SerializeField, Required] private Button _joinLobbyButton;
        [SerializeField, Required] private Button _goBackButton;
        [SerializeField, Required] private CanvasGroup _creatingLobbyCanvasGroup;
        [SerializeField, Required] private CanvasGroup _joiningLobbyCanvasGroup;
        [SerializeField, Required] private UI_InputFieldLobbyCode _lobbyCodeInputField;
        private CanvasGroup _canvasGroup;
        private string _lobbyCode;
        private bool _isCreatingLobby;
        
        private UI_Button _createLobbyButtonUI;
        private UI_Button _joinLobbyButtonUI;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            if (!_createLobbyButton)
            {
                Logger.LogError("Create Lobby Button not set");
            }
            if (!_joinLobbyButton)
            {
                Logger.LogError("Join Lobby Button not set");
            }
            if (!_lobbyCodeInputField)
            {
                Logger.LogError("Lobby Code Input Field not set");
            }
            if (!_creatingLobbyCanvasGroup)
            {
                Logger.LogError("Creating Lobby Canvas Group not set");
            }
            if (!_joiningLobbyCanvasGroup)
            {
                Logger.LogError("Joining Lobby Canvas Group not set");
            }
            
            BindNavigableVertical(_goBackButton, _createLobbyButton);
            BindNavigableVertical(_createLobbyButton, _goBackButton);
            BindNavigableHorizontal(_goBackButton, _createLobbyButton);
            BindNavigableHorizontal(_createLobbyButton, _goBackButton);
            
            _createLobbyButtonUI = _createLobbyButton.GetComponent<UI_Button>();
            _joinLobbyButtonUI = _joinLobbyButton.GetComponent<UI_Button>();
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            UIManager.Instance.SwitchToCanvasCamera();
            _joinLobbyButton.interactable = false;
            _joinLobbyButton.onClick.AddListener(JoinLobbyButtonClicked);
            _createLobbyButton.onClick.AddListener(CreateLobbyButtonClicked);
            _goBackButton.onClick.AddListener(GoBack);
            _lobbyCodeInputField.OnLobbyCodeChanged += LobbyCodeChanged;
            BootstrapManager.Instance.OnServerMigrationStarted += ServerMigrationStarted;
            BootstrapManager.Instance.OnServerMigrationFinished += ServerMigrationFinished;
            
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(0.33f);
            sequence.AppendCallback(() => _createLobbyButtonUI.Open());
            sequence.AppendInterval(0.33f);
            sequence.AppendCallback(() => _joinLobbyButtonUI.Open());
            sequence.AppendInterval(0.33f);
            sequence.Append(_lobbyCodeInputField.transform.DOScale(Vector3.one * 1.2f, 0.33f));
            sequence.Append(_lobbyCodeInputField.transform.DOScale(Vector3.one, 0.33f));
            sequence.Play();
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventOnlineCreateOrJoinMenuStart, AudioManager.Instance.gameObject);
        }

        private void ServerMigrationStarted()
        {
            var menuToGoOnReset = FindAnyObjectByType<MenuToGoOnReset>();
            if (menuToGoOnReset)
            {
                menuToGoOnReset.SetMenuName(nameof(ControllerLobbyMenu));
            }
            else
            {
                Logger.LogWarning("No MenuToGoOnReset found in scene, after server migration, going to correct menu is not assured", Logger.LogType.Client, this);
            }
            if (_isCreatingLobby)
            {
                _creatingLobbyCanvasGroup.Open();
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                _joiningLobbyCanvasGroup.Open();
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void ServerMigrationFinished()
        {
            Logger.LogDebug("Server Migration Finished", Logger.LogType.Client, this);
            _creatingLobbyCanvasGroup.Close();
            _joiningLobbyCanvasGroup.Close();
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            if(InstanceFinder.IsServerStarted) UIManager.Instance.GoToMenu<ControllerLobbyMenu>();
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _lobbyCodeInputField.OnLobbyCodeChanged -= LobbyCodeChanged;
            _joinLobbyButton.onClick.RemoveListener(JoinLobbyButtonClicked);
            _createLobbyButton.onClick.RemoveListener(CreateLobbyButtonClicked);
            _goBackButton.onClick.RemoveListener(GoBack);
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnServerMigrationStarted -= ServerMigrationStarted;
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnServerMigrationFinished -= ServerMigrationFinished;
            _createLobbyButtonUI.Close();
            _joinLobbyButtonUI.Close();
            _lobbyCodeInputField.transform.localScale = Vector3.zero;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _lobbyCodeInputField.OnLobbyCodeChanged -= LobbyCodeChanged;
            _joinLobbyButton.onClick.RemoveListener(JoinLobbyButtonClicked);
            _createLobbyButton.onClick.RemoveListener(CreateLobbyButtonClicked);
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnServerMigrationStarted -= ServerMigrationStarted;
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnServerMigrationFinished -= ServerMigrationFinished;
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
        }
        
        private void CreateLobbyButtonClicked()
        {
            _isCreatingLobby = true;
            BootstrapManager.Instance.TryStartHostWithRelay();
        }
        
        private void JoinLobbyButtonClicked()
        {
            _isCreatingLobby = false;
            BootstrapManager.Instance.TryJoinAsClientWithRelay(_lobbyCode);
        }
        
        private void LobbyCodeChanged(bool isValid, string code)
        {
            _joinLobbyButton.interactable = isValid;
            _lobbyCode = code;
        }
    }
}