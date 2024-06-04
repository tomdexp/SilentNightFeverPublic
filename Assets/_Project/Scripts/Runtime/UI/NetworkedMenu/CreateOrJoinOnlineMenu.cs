using System;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
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
        [SerializeField, Required] private CanvasGroup _creatingLobbyCanvasGroup;
        [SerializeField, Required] private CanvasGroup _joiningLobbyCanvasGroup;
        [SerializeField, Required] private UI_InputFieldLobbyCode _lobbyCodeInputField;
        private CanvasGroup _canvasGroup;
        private string _lobbyCode;
        private bool _isCreatingLobby;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
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
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            UIManager.Instance.SwitchToCanvasCamera();
            _joinLobbyButton.interactable = false;
            _joinLobbyButton.onClick.AddListener(JoinLobbyButtonClicked);
            _createLobbyButton.onClick.AddListener(CreateLobbyButtonClicked);
            _lobbyCodeInputField.OnLobbyCodeChanged += LobbyCodeChanged;
            BootstrapManager.Instance.OnServerMigrationStarted += ServerMigrationStarted;
            BootstrapManager.Instance.OnServerMigrationFinished += ServerMigrationFinished;
        }

        private void ServerMigrationStarted()
        {
            if (_isCreatingLobby)
            {
                _creatingLobbyCanvasGroup.alpha = 1;
                _creatingLobbyCanvasGroup.interactable = true;
                _creatingLobbyCanvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                _joiningLobbyCanvasGroup.alpha = 1;
                _joiningLobbyCanvasGroup.interactable = true;
                _joiningLobbyCanvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void ServerMigrationFinished()
        {
            _creatingLobbyCanvasGroup.alpha = 0;
            _creatingLobbyCanvasGroup.interactable = false;
            _creatingLobbyCanvasGroup.blocksRaycasts = false;
            _joiningLobbyCanvasGroup.alpha = 0;
            _joiningLobbyCanvasGroup.interactable = false;
            _joiningLobbyCanvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            UIManager.Instance.GoToMenu<ControllerLobbyMenu>();
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _lobbyCodeInputField.OnLobbyCodeChanged -= LobbyCodeChanged;
            _joinLobbyButton.onClick.RemoveListener(JoinLobbyButtonClicked);
            _createLobbyButton.onClick.RemoveListener(CreateLobbyButtonClicked);
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnServerMigrationStarted -= ServerMigrationStarted;
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.OnServerMigrationFinished -= ServerMigrationFinished;
        }

        private void OnDestroy()
        {
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