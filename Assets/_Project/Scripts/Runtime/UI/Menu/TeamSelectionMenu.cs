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
        [SerializeField] private RectTransform _playerALabel;
        [SerializeField] private RectTransform _playerASlotA;
        [SerializeField] private RectTransform _playerASlotB;

        [Title("    J2")]
        [SerializeField] private RectTransform _playerBLabel;
        [SerializeField] private RectTransform _playerBSlotA;
        [SerializeField] private RectTransform _playerBSlotB;

        [Title("    J3")]
        [SerializeField] private RectTransform _playerCLabel;
        [SerializeField] private RectTransform _playerCSlotA;
        [SerializeField] private RectTransform _playerCSlotB;

        [Title("    J4")]
        [SerializeField] private RectTransform _playerDLabel;
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
            _isSetup = true;
        }

        private IEnumerator BindEvents()
        {
            while (PlayerManager.HasInstance == false)
            {
                // We have to wait for the Server to spawn the PlayerManager
                yield return new WaitForSecondsRealtime(0.1f);
            }
            PlayerManager.Instance.OnPlayerTeamInfosChanged += OnPlayerTeamInfosChanged;
        }

        private void UnbindEvents()
        {
            if (PlayerManager.HasInstance == false) return;
            PlayerManager.Instance.OnPlayerTeamInfosChanged -= OnPlayerTeamInfosChanged;
        }


        private void OnPlayerTeamInfosChanged(List<Player.PlayerTeamInfo> teamInfos)
        {
            foreach (var teamInfo in teamInfos)
            {
                switch (teamInfo.PlayerIndexType)
                {
                    case Player.PlayerIndexType.A:
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            _playerALabel.anchorMax = _playerASlotA.anchorMax;
                            _playerALabel.anchorMin = _playerASlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            _playerALabel.anchorMax = _playerASlotB.anchorMax;
                            _playerALabel.anchorMin = _playerASlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.B:
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            //_playerBLabel = _playerBSlotA;
                            _playerBLabel.anchorMax = _playerBSlotA.anchorMax;
                            _playerBLabel.anchorMin = _playerBSlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            //_playerBLabel = _playerBSlotB;
                            _playerBLabel.anchorMax = _playerBSlotB.anchorMax;
                            _playerBLabel.anchorMin = _playerBSlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.C:
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            //_playerCLabel = _playerCSlotA;
                            _playerCLabel.anchorMax = _playerCSlotA.anchorMax;
                            _playerCLabel.anchorMin = _playerCSlotA.anchorMin;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            //_playerCLabel = _playerCSlotB;
                            _playerCLabel.anchorMax = _playerCSlotB.anchorMax;
                            _playerCLabel.anchorMin = _playerCSlotB.anchorMin;
                        }
                        break;


                    case Player.PlayerIndexType.D:
                        if (teamInfo.PlayerTeamType == Player.PlayerTeamType.A)
                        {
                            _playerDLabel = _playerDSlotA;
                        }
                        else if (teamInfo.PlayerTeamType == Player.PlayerTeamType.B)
                        {
                            _playerDLabel = _playerDSlotB;
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