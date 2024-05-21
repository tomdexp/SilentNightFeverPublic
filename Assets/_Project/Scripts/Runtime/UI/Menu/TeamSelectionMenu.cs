using _Project.Scripts.Runtime.Networking;
using GameKit.Dependencies.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    public class TeamSelectionMenu : Menu
    {
        [Title("    J1")]
        [SerializeField] private GameObject _playerALabel;
        [SerializeField] private RectTransform _playerASlotA;
        [SerializeField] private RectTransform _playerASlotB;

        [Title("    J2")]
        [SerializeField] private GameObject _playerBLabel;
        [SerializeField] private RectTransform _playerBSlotA;
        [SerializeField] private RectTransform _playerBSlotB;

        [Title("    J3")]
        [SerializeField] private GameObject _playerCLabel;
        [SerializeField] private RectTransform _playerCSlotA;
        [SerializeField] private RectTransform _playerCSlotB;

        [Title("    J4")]
        [SerializeField] private GameObject _playerDLabel;
        [SerializeField] private RectTransform _playerDSlotA;
        [SerializeField] private RectTransform _playerDSlotB;

        private void Awake()
        {
            _needSetup = true;
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        public override void SetupMenu()
        {
            _needSetup = true;
            PlayerManager.TryGetInstance().TryStartTeamManagement();
            PlayerManager.Instance.OnPlayerTeamInfosChanged += OnPlayerTeamInfosChanged;
            PlayerManager.Instance.OnPlayersReadyChanged += OnPlayersReadyChanged;
            PlayerManager.Instance.OnAllPlayersReady += OnAllPlayersReady;
            _isSetup = true;
        }

        private void OnAllPlayersReady()
        {
            if (GameManager.HasInstance) GameManager.Instance.LoadGameScene();
        }

        private void UnbindEvents()
        {
            if (PlayerManager.HasInstance == false) return;
            PlayerManager.Instance.OnPlayerTeamInfosChanged -= OnPlayerTeamInfosChanged;
            PlayerManager.Instance.OnPlayersReadyChanged -= OnPlayersReadyChanged;
        }

        private void OnPlayersReadyChanged(List<Player.PlayerReadyInfo> readyInfos)
        {
            foreach (var readyInfo in readyInfos)
            {
                switch (readyInfo.PlayerIndexType)
                {
                    case Player.PlayerIndexType.A:
                        TextMeshProUGUI _playerALabelTMP = _playerALabel.GetComponent<TextMeshProUGUI>();
                        if (readyInfo.IsPlayerReady == true)
                        {
                    
                            _playerALabelTMP.color = Color.green;
                        } else
                        {
                            _playerALabelTMP.color = Color.black;
                        }
                   
                        break;
                    case Player.PlayerIndexType.B:

                        TextMeshProUGUI _playerBLabelTMP = _playerBLabel.GetComponent<TextMeshProUGUI>();
                        if (readyInfo.IsPlayerReady == true)
                        {
                            _playerBLabelTMP.color = Color.green;
                        }
                        else
                        {
                            _playerBLabelTMP.color = Color.black;
                        }
                        break;
                    case Player.PlayerIndexType.C:

                        TextMeshProUGUI _playerCLabelTMP = _playerCLabel.GetComponent<TextMeshProUGUI>();
                        if (readyInfo.IsPlayerReady == true)
                        {
                            _playerCLabelTMP.color = Color.green;
                        }
                        else
                        {
                            _playerCLabelTMP.color = Color.black;
                        }
                        break;
                    case Player.PlayerIndexType.D:

                            TextMeshProUGUI _playerDLabelTMP = _playerDLabel.GetComponent<TextMeshProUGUI>();
                        if (readyInfo.IsPlayerReady == true)
                        {
                            _playerDLabelTMP.color = Color.green;
                        }
                        else
                        {
                            _playerDLabelTMP.color = Color.black;
                        }
                        break;
                    case Player.PlayerIndexType.Z:
                        Logger.LogWarning("Player of index Z in team management screen, which shouldn't happen");
                        break;
                }
            }
        }


        private void OnPlayerTeamInfosChanged(List<Player.PlayerTeamInfo> teamInfos)
        {
            foreach (var teamInfo in teamInfos)
            {
                switch (teamInfo.PlayerIndexType)
                {
                    case Player.PlayerIndexType.A:

                        RectTransform _playerALabelRectTransform = _playerALabel.GetComponent<RectTransform>();
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            _playerALabelRectTransform.anchorMax = _playerASlotA.anchorMax;
                            _playerALabelRectTransform.anchorMin = _playerASlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            _playerALabelRectTransform.anchorMax = _playerASlotB.anchorMax;
                            _playerALabelRectTransform.anchorMin = _playerASlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.B:
                        RectTransform _playerBLabelRectTransform = _playerBLabel.GetComponent<RectTransform>();
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            _playerBLabelRectTransform.anchorMax = _playerBSlotA.anchorMax;
                            _playerBLabelRectTransform.anchorMin = _playerBSlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            _playerBLabelRectTransform.anchorMax = _playerBSlotB.anchorMax;
                            _playerBLabelRectTransform.anchorMin = _playerBSlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.C:
                        RectTransform _playerCLabelRectTransform = _playerCLabel.GetComponent<RectTransform>();
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            _playerCLabelRectTransform.anchorMax = _playerCSlotA.anchorMax;
                            _playerCLabelRectTransform.anchorMin = _playerCSlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            _playerCLabelRectTransform.anchorMax = _playerCSlotB.anchorMax;
                            _playerCLabelRectTransform.anchorMin = _playerCSlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.D:
                        RectTransform _playerDLabelRectTransform = _playerDLabel.GetComponent<RectTransform>();
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            _playerDLabelRectTransform.anchorMax = _playerDSlotA.anchorMax;
                            _playerDLabelRectTransform.anchorMin = _playerDSlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            _playerDLabelRectTransform.anchorMax = _playerDSlotB.anchorMax;
                            _playerDLabelRectTransform.anchorMin = _playerDSlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.Z:
                        Logger.LogWarning("Player of index Z in team management screen, which shouldn't happen");
                        continue;
                    default:
                        continue;
                }
            }
        }

    }

}