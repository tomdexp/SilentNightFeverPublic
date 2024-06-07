using System.Collections;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameSettingsMenu : MenuBase
    {
        public override string MenuName { get; } = "GameSettingsMenu";
        [SerializeField, Required] private ConfirmationPrompt _quitGameSettingsPrompt;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
        }
        
        public override void Open()
        {
            base.Open();
            if(InstanceFinder.IsServerStarted) UIManager.Instance.SwitchToCanvasCamera();
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
            if (!InstanceFinder.IsServerStarted) return;
            StartCoroutine(GoBackCoroutine());
        }
        
        private IEnumerator GoBackCoroutine()
        {
            _quitGameSettingsPrompt.Open();
            yield return _quitGameSettingsPrompt.WaitForResponse();
            if (_quitGameSettingsPrompt.IsSuccess)
            {
                UIManager.Instance.GoToMenu<CustomizationMenu>();
            }
        }
    }
}