using System.Collections;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
using FishNet;
using GameKit.Dependencies.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ControllerLobbyMenu : MenuBase
    {
        public override string MenuName { get; } = "ControllerLobbyMenu";
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsHostPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitOnlineAsClientPrompt;
        [SerializeField, Required] private ConfirmationPrompt _quitLocalPrompt;
        [SerializeField, Required] private CanvasGroup _onlineCanvasGroup;
        [SerializeField, Required] private CanvasGroup _localCanvasGroup;
        [SerializeField, Required] private UI_BindRealPlayerToImage _playerACanvas;
        [SerializeField, Required] private UI_BindRealPlayerToImage _playerBCanvas;
        [SerializeField, Required] private UI_BindRealPlayerToImage _playerCCanvas;
        [SerializeField, Required] private UI_BindRealPlayerToImage _playerDCanvas;
        [SerializeField] private float _secondsBeforeStartWhenAllControllerConnected = 2.5f;
        [SerializeField] private float _delayBetweenPlayerAnimation = 0.5f;
        private CanvasGroup _canvasGroup;
        private bool _timerStarted;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            if (BootstrapManager.Instance.IsOnline)
            {
                Logger.LogDebug("Opening controller menu as online");
                _onlineCanvasGroup.gameObject.SetActive(true);
                _localCanvasGroup.gameObject.SetActive(false);
            }
            else
            {
                Logger.LogDebug("Opening controller menu as local");
                _onlineCanvasGroup.gameObject.SetActive(false);
                _localCanvasGroup.gameObject.SetActive(true);
            }
            _timerStarted = false;
            PlayerManager.Instance.SetPlayerJoiningEnabled(true);
            PlayerManager.Instance.SetPlayerLeavingEnabled(true);
            if(InstanceFinder.IsServerStarted) PlayerManager.Instance.ResetRealPlayerInfos();
            
            var sequence = DOTween.Sequence();
            sequence.AppendCallback((() => _playerACanvas.Open()));
            sequence.AppendInterval(_delayBetweenPlayerAnimation);
            sequence.AppendCallback((() => _playerBCanvas.Open()));
            sequence.AppendInterval(_delayBetweenPlayerAnimation);
            sequence.AppendCallback((() => _playerCCanvas.Open()));
            sequence.AppendInterval(_delayBetweenPlayerAnimation);
            sequence.AppendCallback((() => _playerDCanvas.Open()));
            sequence.Play();
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventGamepadMenuStart, AudioManager.Instance.gameObject);
        }

        private void Update()
        {
            if (!PlayerManager.HasInstance) return;
            if (!_timerStarted)
            {
                if (PlayerManager.Instance.NumberOfPlayers == 4)
                {
                    _timerStarted = true;
                    StartCoroutine(TimerBeforeNextScreen());
                }
            }
        }
        
        private IEnumerator TimerBeforeNextScreen()
        {
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerJoiningEnabledClientRpc(false);
            if (InstanceFinder.IsServerStarted) PlayerManager.Instance.SetPlayerLeavingEnabledClientRpc(false);
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(1f);
            sequence.AppendCallback((() => _playerDCanvas.Close()));
            sequence.AppendInterval(_delayBetweenPlayerAnimation);
            sequence.AppendCallback((() => _playerCCanvas.Close()));
            sequence.AppendInterval(_delayBetweenPlayerAnimation);
            sequence.AppendCallback((() => _playerBCanvas.Close()));
            sequence.AppendInterval(_delayBetweenPlayerAnimation);
            sequence.AppendCallback((() => _playerACanvas.Close()));
            sequence.Play();
            yield return new WaitForSeconds(_secondsBeforeStartWhenAllControllerConnected);
            if (InstanceFinder.IsServerStarted)
            {
                UIManager.Instance.GoToMenu<PlayerIndexSelectionMenu>();
            }
        }

        public override void Close()
        {
            base.Close();
            _onlineCanvasGroup.gameObject.SetActive(false);
            _localCanvasGroup.gameObject.SetActive(false);
            _canvasGroup.Close();
            PlayerManager.Instance.SetPlayerJoiningEnabled(false);
            PlayerManager.Instance.SetPlayerLeavingEnabled(false);
            _playerACanvas.Close();
            _playerBCanvas.Close();
            _playerCCanvas.Close();
            _playerDCanvas.Close();
        }

        public override void GoBack()
        {
            base.GoBack();
            StartCoroutine(GoBackCoroutine());
        }
        
        private IEnumerator GoBackCoroutine()
        {
            if (InstanceFinder.IsServerStarted)
            {
                if (BootstrapManager.Instance.IsOnline) // Online Lobby Host
                {
                    _quitOnlineAsHostPrompt.Open();
                    yield return _quitOnlineAsHostPrompt.WaitForResponse();
                    if (_quitOnlineAsHostPrompt.IsSuccess)
                    {
                        BootstrapManager.Instance.TryLeaveOnline();
                        FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(nameof(PlayOnlineOrLocalMenu));
                        //UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
                    }
                }
                else // Local Lobby
                {
                    _quitLocalPrompt.Open();
                    yield return _quitLocalPrompt.WaitForResponse();
                    if (_quitLocalPrompt.IsSuccess)
                    {
                        UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
                    }
                }
            }
            else // Online Lobby Client
            {
                _quitOnlineAsClientPrompt.Open();
                yield return _quitOnlineAsClientPrompt.WaitForResponse();
                if (_quitOnlineAsClientPrompt.IsSuccess)
                {
                    BootstrapManager.Instance.TryLeaveOnline();
                    FindAnyObjectByType<MenuToGoOnReset>().SetMenuName(nameof(PlayOnlineOrLocalMenu));
                    //UIManager.Instance.GoToMenu<PlayOnlineOrLocalMenu>();
                }
            }
        }
    }
}