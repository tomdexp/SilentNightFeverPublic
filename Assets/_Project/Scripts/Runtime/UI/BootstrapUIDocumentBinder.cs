using System;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class BootstrapUIDocumentBinder : MonoBehaviour
    {
        [SerializeField] private InputAction _toggleVisibilityInputAction;
        private UIDocument _uiDocument;

        private void Start()
        {
            Logger.LogDebug("BootstrapUIDocumentBinder Start", context:this);
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Utils.Logger.LogError("No UIDocument found on BootstrapUIDocumentBinder.", context:this);
                return;
            }
            
            _toggleVisibilityInputAction.performed += OnToggleVisibilityInputAction;
            _toggleVisibilityInputAction.Enable();
            
            BootstrapManager.Instance.OnJoinCodeReceived += OnJoinCodeReceived;
            
            if (_uiDocument == null)
            {
                Utils.Logger.LogError("No UIDocument found on BootstrapUIDocumentBinder.", context:this);
                return;
            }

            Button hostButton = (Button)_uiDocument.rootVisualElement.Q("host-relay");
            if (hostButton != null)
            {
                hostButton.clicked += () => BootstrapManager.Instance.TryStartHostWithRelay();
            }
            
            TextField joinCodeInput = (TextField)_uiDocument.rootVisualElement.Q("join-code-input-field");
            
            Button joinButton = (Button)_uiDocument.rootVisualElement.Q("join-relay");
            if (joinButton != null)
            {
                joinButton.clicked += () => BootstrapManager.Instance.TryJoinAsClientWithRelay(joinCodeInput.value);
            }
            
            Button addFakePlayerButton = (Button)_uiDocument.rootVisualElement.Q("add-fake-player");
            if (addFakePlayerButton != null)
            {
                addFakePlayerButton.clicked += () => PlayerManager.Instance.TryAddFakePlayer();
            }
            
            Button removeFakePlayerButton = (Button)_uiDocument.rootVisualElement.Q("remove-fake-player");
            if (removeFakePlayerButton != null)
            {
                removeFakePlayerButton.clicked += () => PlayerManager.Instance.TryRemoveFakePlayer();
            }
            
            Button startGameButton = (Button)_uiDocument.rootVisualElement.Q("start-game");
            if (startGameButton != null)
            {
                startGameButton.clicked += () => GameManager.Instance.TryStartGame();
            }
        }

        private void OnDestroy()
        {
            _toggleVisibilityInputAction.Disable();
            _toggleVisibilityInputAction.performed -= OnToggleVisibilityInputAction;
            if(BootstrapManager.HasInstance) BootstrapManager.Instance.OnJoinCodeReceived -= OnJoinCodeReceived;
        }

        private void OnToggleVisibilityInputAction(InputAction.CallbackContext context)
        {
            _uiDocument.enabled = !_uiDocument.enabled;
        }
        
        private void OnJoinCodeReceived(string code)
        {
            var joinCodeInput = _uiDocument.rootVisualElement.Q("join-code-input-field") as TextField;
            joinCodeInput.value = code;
            var joinCodeLabel = _uiDocument.rootVisualElement.Q("current-join-code") as Label;
            joinCodeLabel.text = "Join Code : " + code;
        }
    }
}