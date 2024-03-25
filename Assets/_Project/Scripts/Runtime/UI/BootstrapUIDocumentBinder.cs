using System;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Runtime.UI
{
    public class BootstrapUIDocumentBinder : MonoBehaviour
    {
        private UIDocument _uiDocument;

        private void Start()
        {
            _uiDocument = GetComponent<UIDocument>();
            
            BootstrapManager.Instance.OnJoinCodeReceived += code =>
            {
                var joinCodeInput = _uiDocument.rootVisualElement.Q("join-code-input-field") as TextField;
                joinCodeInput.value = code;
                var joinCodeLabel = _uiDocument.rootVisualElement.Q("current-join-code") as Label;
                joinCodeLabel.text = "Join Code : " + code;
            };
            
            if (_uiDocument == null)
            {
                Debug.LogError("No UIDocument found on BootstrapUIDocumentBinder.");
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
    }
}