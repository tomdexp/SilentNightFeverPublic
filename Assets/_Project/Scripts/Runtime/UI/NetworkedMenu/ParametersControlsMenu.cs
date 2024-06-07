using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersControlsMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersControlsMenu";
        
        [SerializeField, Required] private Button _remapMoveUpButton;
        [SerializeField, Required] private Button _remapMoveDownButton;
        [SerializeField, Required] private Button _remapMoveLeftButton;
        [SerializeField, Required] private Button _remapMoveRightButton;
        [SerializeField, Required] private Button _remapInteractButton;
        [SerializeField, Required] private Button _backButton;
        
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            BindNavigableVertical(_remapMoveUpButton, _remapMoveDownButton);
            BindNavigableVertical(_remapMoveDownButton, _remapMoveLeftButton);
            BindNavigableVertical(_remapMoveLeftButton, _remapMoveRightButton);
            BindNavigableVertical(_remapMoveRightButton, _remapInteractButton);
            BindNavigableVertical(_remapInteractButton, _backButton);
            BindNavigableVertical(_backButton, _remapMoveUpButton);
            BindNavigableHorizontal(_backButton, _remapInteractButton);
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

        public override void GoBack()
        {
            base.GoBack();
            UIManager.Instance.GoToMenu<ParametersMenu>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _remapMoveUpButton.onClick.RemoveListener(GoBack);
        }
    }
}