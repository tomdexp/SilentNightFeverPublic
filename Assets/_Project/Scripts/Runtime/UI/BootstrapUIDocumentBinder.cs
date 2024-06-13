using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class BootstrapUIDocumentBinder : MonoBehaviour
    {
        [SerializeField] private InputAction _toggleVisibilityInputAction;
        private UIDocument _uiDocument;
        private bool _isVisible = true;

        private void Start()
        {
            bool showOnStart = false;
            bool activate = false; // if false, do not even register the events
#if UNITY_EDITOR
            showOnStart = true;
            activate = true;
#else
            if (Debug.isDebugBuild) // Is Development Build ?
            {
                showOnStart = false;
                activate = true;
            }
            else // Is release, so we don't use it
            {
                showOnStart = false;
                activate = false;
            }
#endif

            
            
            
            Logger.LogDebug("BootstrapUIDocumentBinder Start", context:this);
            _uiDocument = GetComponent<UIDocument>();
            if (!_uiDocument)
            {
                Logger.LogError("No UIDocument found on BootstrapUIDocumentBinder.", context:this);
                return;
            }
            
            if (!activate)
            {
                _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
                return;
            }
            
            _toggleVisibilityInputAction.performed += OnToggleVisibilityInputAction;
            _toggleVisibilityInputAction.Enable();
            
            BootstrapManager.Instance.OnJoinCodeReceived += OnJoinCodeReceived;
            
            if (!_uiDocument)
            {
                Logger.LogError("No UIDocument found on BootstrapUIDocumentBinder.", context:this);
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
                startGameButton.clicked += () =>
                {
                    // check if we are in the onboarding scene
                    string sceneName = SceneManager.GetActiveScene().name;
                    if (sceneName == "OnboardingScene")
                    {
                        GameManager.Instance.TryStartOnBoarding();
                    }
                    else
                    {
                        GameManager.Instance.TryStartGame();
                    }
                };
            }

            if (showOnStart)
            {
                _isVisible = true;
                _uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                _isVisible = false;
                _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
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
            _isVisible = !_isVisible;
            _uiDocument.rootVisualElement.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
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