using System.Collections;
using _Project.Scripts.Runtime.Networking;
using FishNet;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class CustomizationMenu : MenuBase
    {
        public override string MenuName { get; } = "CustomizationMenu";
        [SerializeField] private ConfirmationPrompt _quitCustomizationPrompt;

        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToMetroCamera();
            if(InstanceFinder.IsServerStarted) PlayerManager.Instance.TryStartCharacterCustomization();
        }
        
        public override void Close()
        {
            base.Close();
            if(InstanceFinder.IsServerStarted) PlayerManager.Instance.TryStopCharacterCustomization();
        }

        public override void GoBack()
        {
            base.GoBack();
            if (!InstanceFinder.IsServerStarted) return;
            StartCoroutine(GoBackCoroutine());
        }
        
        private IEnumerator GoBackCoroutine()
        {
            _quitCustomizationPrompt.Open();
            if(InstanceFinder.IsServerStarted) PlayerManager.Instance.TryStopCharacterCustomization();
            yield return _quitCustomizationPrompt.WaitForResponse();
            if (_quitCustomizationPrompt.IsSuccess)
            {
                UIManager.Instance.GoToMenu<PlayerIndexSelectionMenu>();
            }
            else
            {
                PlayerManager.Instance.SetPlayerChangingHatEnabledClientRpc(true);
                PlayerManager.Instance.SetPlayerConfirmHatEnabledClientRpc(true);
            }
        }
     }
}