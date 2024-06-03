using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class PlayerIndexSelectionMenu : MenuBase
    {
        public override string MenuName { get; } = "PlayerIndexSelectionMenu";
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}