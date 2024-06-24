using _Project.Scripts.Runtime.Utils;
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
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
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
            _goBackButton.onClick.AddListener(Close);
            _buttonResume.onClick.AddListener(Close);
            _buttonSettings.onClick.AddListener(OnButtonSettingsClicked);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _goBackButton.onClick.RemoveListener(Close);
            _buttonResume.onClick.RemoveListener(Close);
            _buttonSettings.onClick.RemoveListener(OnButtonSettingsClicked);
        }

        public override void GoBack()
        {
            base.GoBack();
            Close();
        }
        
        private void OnButtonSettingsClicked()
        {
            //UILocalManager.Instance.GoToMenu<ParametersMenu>();
            UI.GoToMenu<ParametersMenu>();
        }
    }
}