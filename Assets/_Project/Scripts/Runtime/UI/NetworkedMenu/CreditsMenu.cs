using _Project.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CreditsMenu : MenuBase
    {
        public override string MenuName { get; } = "CreditsMenu";
        [SerializeField] private Button _backButton;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            if (!_backButton)
            {
                Logger.LogError("Back Button not set", Logger.LogType.Client, this);
            }
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _backButton.onClick.AddListener(GoBack);
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _backButton.onClick.RemoveListener(GoBack);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _backButton.onClick.RemoveListener(GoBack);
        }
        
        public override void GoBack()
        {
            base.GoBack();
            UIManager.Instance.GoToMenu<MainMenu>();
        }
    }
}