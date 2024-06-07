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
        [SerializeField] private float _delayBetweenHatConfirmedAndNextMenu = 1f;

        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToMetroCamera();
            if (InstanceFinder.IsServerStarted)
            {
                if(PlayerManager.HasInstance) PlayerManager.Instance.TryStartCharacterCustomization();
                if(PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayersConfirmedHat += OnAllPlayersConfirmedHat;
            }
        }

        private void OnAllPlayersConfirmedHat()
        {
            PlayerManager.Instance.TryStopCharacterCustomization();
            StartCoroutine(OnAllPlayersConfirmedHatCoroutine());
        }

        private IEnumerator OnAllPlayersConfirmedHatCoroutine()
        {
            yield return new WaitForSeconds(_delayBetweenHatConfirmedAndNextMenu);
            UIManager.Instance.GoToMenu<GameSettingsMenu>();
        }

        public override void Close()
        {
            base.Close();
            if (InstanceFinder.IsServerStarted)
            {
                if(PlayerManager.HasInstance) PlayerManager.Instance.TryStopCharacterCustomization();
                if(PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayersConfirmedHat -= OnAllPlayersConfirmedHat;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (InstanceFinder.IsServerStarted)
            {
                if(PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayersConfirmedHat -= OnAllPlayersConfirmedHat;
            }
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