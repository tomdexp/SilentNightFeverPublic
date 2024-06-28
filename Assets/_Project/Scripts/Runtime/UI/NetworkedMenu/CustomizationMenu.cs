using System.Collections;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.CharacterCustomization;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using FishNet;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class CustomizationMenu : MenuBase
    {
        public override string MenuName { get; } = "CustomizationMenu";
        [SerializeField, Required] private ConfirmationPrompt _quitCustomizationPrompt;
        [SerializeField] private float _delayBetweenHatConfirmedAndNextMenu = 1f;
        [SerializeField, Required] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup.CloseInstant();
        }

        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToCustomizationCamera();
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventCharacterCustomizationStart);
            _canvasGroup.Open();
            var characterCustomizer = FindAnyObjectByType<CharacterCustomizer>();
            if (characterCustomizer)
            {
                characterCustomizer.StartCustomization();
            }
            else
            {
                Logger.LogError("No CharacterCustomizer found in the scene", Logger.LogType.Client, this);
            }
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
            _canvasGroup.Close();
            var characterCustomizer = FindAnyObjectByType<CharacterCustomizer>();
            if (characterCustomizer)
            {
                characterCustomizer.StopCustomization();
            }
            else
            {
                Logger.LogError("No CharacterCustomizer found in the scene", Logger.LogType.Client, this);
            }
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