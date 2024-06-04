using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using DG.Tweening;
using FishNet;
using FishNet.Transporting;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    // Also know as Team Selection Menu
    public class PlayerIndexSelectionMenu : MenuBase
    {
        public override string MenuName { get; } = "PlayerIndexSelectionMenu";
        
        [Title("References")]
        [SerializeField, Required] private UIData _uiData;
        [SerializeField, Required] private Transform _playerLabelA;
        [SerializeField, Required] private Transform _playerLabelB;
        [SerializeField, Required] private Transform _playerLabelC;
        [SerializeField, Required] private Transform _playerLabelD;
        [SerializeField, Required] private Transform _playerEndA;
        [SerializeField, Required] private Transform _playerEndB;
        [SerializeField, Required] private Transform _playerEndC;
        [SerializeField, Required] private Transform _playerEndD;
        
        private Vector3 _playerStartA;
        private Vector3 _playerStartB;
        private Vector3 _playerStartC;
        private Vector3 _playerStartD;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _playerStartA = _playerLabelA.position;
            _playerStartB = _playerLabelB.position;
            _playerStartC = _playerLabelC.position;
            _playerStartD = _playerLabelD.position;
        }
        
        public override void Open()
        {
            base.Open();
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            if (InstanceFinder.IsServerStarted && PlayerManager.HasInstance)
            {
                PlayerManager.Instance.TryStartTeamManagement();
                PlayerManager.Instance.OnAllPlayersReady += OnAllPlayersReady;
            }
            if (InstanceFinder.IsServerStarted) UIManager.Instance.SwitchToCanvasCamera();
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            if (InstanceFinder.IsServerStarted && PlayerManager.HasInstance) PlayerManager.Instance.OnAllPlayersReady -= OnAllPlayersReady;
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

        public override void Start()
        {
            base.Start();
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            UpdateUI();
        }

        private void OnDestroy()
        {
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
                    GoToPlayerEnd(playerLabel, GetPlayerEnd(playerTeamInfo.ScreenPlayerIndexType));
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
            yield return new WaitForSeconds(_uiData.SecondsAfterAllPlayersReadyToStartCustomization);
            if (InstanceFinder.IsServerStarted)
            {
                UIManager.Instance.GoToMenu<CustomizationMenu>();
            }
        }
        
        private void GoToPlayerEnd(Transform playerLabel, Transform playerEnd)
        {
            playerLabel.DOMove(playerEnd.position, _uiData.PlayerTeamLabelMovementDuration)
                .SetEase(_uiData.PlayerTeamLabelMovementEase);
        }
        
        private void GoToPlayerStart(Transform playerLabel, Vector3 playerStart)
        {
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
    }
}