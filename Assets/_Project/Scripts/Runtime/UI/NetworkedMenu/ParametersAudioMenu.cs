using _Project.Scripts.Runtime.Utils;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersAudioMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersAudioMenu";
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
        }

        public override void GoBack()
        {
            base.GoBack();
            UIManager.Instance.GoToMenu<ParametersMenu>();
        }
    }
}