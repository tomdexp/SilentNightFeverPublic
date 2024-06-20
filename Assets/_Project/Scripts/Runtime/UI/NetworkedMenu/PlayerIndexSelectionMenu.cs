using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils;
using DG.Tweening;
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    // Also know as Team Selection Menu
    public class PlayerIndexSelectionMenu : MenuBase
    {
        public override string MenuName { get; } = "PlayerIndexSelectionMenu";
        
        [Title("References")]
        [SerializeField, Required] private UIData _uiData;
        [SerializeField, Required] private Button _goBackButton;
        [SerializeField, Required] private MMFPlayerReplicated _feedbacksAllPlayersReady;
        [SerializeField, Required] private Transform _playerLabelA;
        [SerializeField, Required] private Transform _playerLabelB;
        [SerializeField, Required] private Transform _playerLabelC;
        [SerializeField, Required] private Transform _playerLabelD;
        [SerializeField, Required] private Transform _playerEndA;
        [SerializeField, Required] private Transform _playerEndB;
        [SerializeField, Required] private Transform _playerEndC;
        [SerializeField, Required] private Transform _playerEndD;
        [SerializeField] private Color _playerColorA;
        [SerializeField] private Color _playerColorB;
        [SerializeField] private Color _playerColorC;
        [SerializeField] private Color _playerColorD;
        [SerializeField, Required] private ConfirmationPrompt _quitPlayerIndexSelectionPrompt;
        [SerializeField, Required] private MMF_Player _feedbacksPlayerReadyA;
        [SerializeField, Required] private MMF_Player _feedbacksPlayerReadyB;
        [SerializeField, Required] private MMF_Player _feedbacksPlayerReadyC;
        [SerializeField, Required] private MMF_Player _feedbacksPlayerReadyD;
        
        private Vector3 _playerStartA;
        private Vector3 _playerStartB;
        private Vector3 _playerStartC;
        private Vector3 _playerStartD;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            _playerStartA = _playerLabelA.position;
            _playerStartB = _playerLabelB.position;
            _playerStartC = _playerLabelC.position;
            _playerStartD = _playerLabelD.position;
        }
        
        public override void Start()
        {
            base.Start();
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            StartCoroutine(TryRegisterPlayerManagerEvents());
            UpdateUI();
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            UIManager.Instance.SwitchToCanvasCamera();
            RestoreAllPlayerReadyFeedbacks();
            if (InstanceFinder.IsServerStarted && PlayerManager.HasInstance)
            {
                _feedbacksAllPlayersReady.RestoreFeedbacksForAll();
                PlayerManager.Instance.TryStartTeamManagement();
                PlayerManager.Instance.OnAllPlayersReady += OnAllPlayersReady;
                _goBackButton.onClick.AddListener(GoBack);
            }
            
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventTeamSelectionMenuStart, AudioManager.Instance.gameObject);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            if (InstanceFinder.IsServerStarted && PlayerManager.HasInstance)
            {
                PlayerManager.Instance.OnAllPlayersReady -= OnAllPlayersReady;
                _goBackButton.onClick.RemoveListener(GoBack);
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
            _quitPlayerIndexSelectionPrompt.Open();
            PlayerManager.Instance.TryEndTeamManagement();
            yield return _quitPlayerIndexSelectionPrompt.WaitForResponse();
            if (_quitPlayerIndexSelectionPrompt.IsSuccess)
            {
                UIManager.Instance.GoToMenu<ControllerLobbyMenu>();
            }
            else
            {
                PlayerManager.Instance.ResumeTeamManagement();
            }
        }
        
        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                StartCoroutine(TryRegisterPlayerManagerEvents());
            }
        }

        private IEnumerator TryRegisterPlayerManagerEvents()
        {
            while(!PlayerManager.HasInstance) yield return null;
            PlayerManager.Instance.OnPlayerTeamInfosChanged += OnPlayerTeamInfosChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(InstanceFinder.ClientManager) InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            if (PlayerManager.HasInstance) PlayerManager.Instance.OnPlayerTeamInfosChanged -= OnPlayerTeamInfosChanged;
            if (InstanceFinder.IsServerStarted && PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayersReady -= OnAllPlayersReady;
        }

        private void OnPlayerTeamInfosChanged(List<PlayerTeamInfo> _)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!PlayerManager.HasInstance)
            {
                GoToPlayerStart(_playerLabelA, _playerStartA);
                GoToPlayerStart(_playerLabelB, _playerStartB);
                GoToPlayerStart(_playerLabelC, _playerStartC);
                GoToPlayerStart(_playerLabelD, _playerStartD);
                return;
            }
            List<PlayerTeamInfo> teamInfos = PlayerManager.Instance.GetPlayerTeamInfos();
            
            // We know the destination of each teamInfos with ScreenPlayerIndexType
            foreach (var playerTeamInfo in teamInfos)
            {
                var playerLabel = GetPlayerLabel(playerTeamInfo.PlayerIndexType);
                if (playerTeamInfo.ScreenPlayerIndexType == PlayerIndexType.Z)
                {
                    GoToPlayerStart(playerLabel, GetPlayerStart(playerTeamInfo.PlayerIndexType));
                }
                else
                {
                    GoToPlayerEnd(playerLabel, GetPlayerEnd(playerTeamInfo.ScreenPlayerIndexType),
                        GetPlayerColor(playerTeamInfo.ScreenPlayerIndexType),
                        GetPlayerReadyFeedback(playerTeamInfo.ScreenPlayerIndexType));
                }
            }
        }
        
        private void OnAllPlayersReady()
        {
            StartCoroutine(OnAllPlayersReadyCoroutine());
        }

        private IEnumerator OnAllPlayersReadyCoroutine()
        {
            PlayerManager.Instance.TryEndTeamManagement();
            if (InstanceFinder.IsServerStarted) _feedbacksAllPlayersReady.PlayFeedbacksForAll();
            yield return new WaitForSeconds(_uiData.SecondsAfterAllPlayersReadyToStartCustomization);
            if (InstanceFinder.IsServerStarted)
            {
                UIManager.Instance.GoToMenu<CustomizationMenu>();
            }
        }

        private void GoToPlayerEnd(Transform playerLabel, Transform playerEnd, Color playerColor, MMF_Player playerReadyFeedback)
        {
            //if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIPlayerButtonMove, AudioManager.Instance.gameObject);
            playerLabel.GetComponent<UI_BindPlayerReadyToImage>().SetReadyColor(playerColor);
            playerLabel.GetComponent<UI_BindPlayerReadyToImage>().SetReadyFeedbacks(playerReadyFeedback);
            playerLabel.DOMove(playerEnd.position, _uiData.PlayerTeamLabelMovementDuration)
                .SetEase(_uiData.PlayerTeamLabelMovementEase);
        }
        
        private void GoToPlayerStart(Transform playerLabel, Vector3 playerStart)
        {
            //if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIPlayerButtonMove, AudioManager.Instance.gameObject);
            playerLabel.DOMove(playerStart, _uiData.PlayerTeamLabelMovementDuration)
                .SetEase(_uiData.PlayerTeamLabelMovementEase);
        }

        private Transform GetPlayerLabel(PlayerIndexType playerIndexType)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    return _playerLabelA;
                case PlayerIndexType.B:
                    return _playerLabelB;
                case PlayerIndexType.C:
                    return _playerLabelC;
                case PlayerIndexType.D:
                    return _playerLabelD;
                default:
                    return null;
            }
        }
        
        private Color GetPlayerColor(PlayerIndexType playerIndexType)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    return _playerColorA;
                case PlayerIndexType.B:
                    return _playerColorB;
                case PlayerIndexType.C:
                    return _playerColorC;
                case PlayerIndexType.D:
                    return _playerColorD;
                default:
                    return Color.white;
            }
        }

        private Transform GetPlayerEnd(PlayerIndexType playerIndexType)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    return _playerEndA;
                case PlayerIndexType.B:
                    return _playerEndB;
                case PlayerIndexType.C:
                    return _playerEndC;
                case PlayerIndexType.D:
                    return _playerEndD;
                default:
                    return null;
            }
        }

        private Vector3 GetPlayerStart(PlayerIndexType playerIndexType)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    return _playerStartA;
                case PlayerIndexType.B:
                    return _playerStartB;
                case PlayerIndexType.C:
                    return _playerStartC;
                case PlayerIndexType.D:
                    return _playerStartD;
                default:
                    return Vector3.zero;
            }
        }
        
        private MMF_Player GetPlayerReadyFeedback(PlayerIndexType playerIndexType)
        {
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    return _feedbacksPlayerReadyA;
                case PlayerIndexType.B:
                    return _feedbacksPlayerReadyB;
                case PlayerIndexType.C:
                    return _feedbacksPlayerReadyC;
                case PlayerIndexType.D:
                    return _feedbacksPlayerReadyD;
                default:
                    return null;
            }
        }
        
        private void RestoreAllPlayerReadyFeedbacks()
        {
            _feedbacksPlayerReadyA.StopFeedbacks();
            _feedbacksPlayerReadyB.StopFeedbacks();
            _feedbacksPlayerReadyC.StopFeedbacks();
            _feedbacksPlayerReadyD.StopFeedbacks();
            _feedbacksPlayerReadyA.RestoreInitialValues();
            _feedbacksPlayerReadyB.RestoreInitialValues();
            _feedbacksPlayerReadyC.RestoreInitialValues();
            _feedbacksPlayerReadyD.RestoreInitialValues();
        }
    }
}