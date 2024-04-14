
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public class LobbyMenu : Menu
    {

        [SerializeField] private Image _playerSlotA;
        [SerializeField] private Image _playerSlotB;
        [SerializeField] private Image _playerSlotC;
        [SerializeField] private Image _playerSlotD;
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _joinCodeLabel;

        private Menu _potentialParentMenu;

        private void Awake()
        {
            _needSetup = true;
        }

        public override bool OpenMenu(bool selectLastSelectable = true)
        {
            if (!base.OpenMenu(selectLastSelectable)) return false;
            EventSystem.current.SetSelectedGameObject(null);
            return true;
        }

        public void TrySetupLobby(Menu callingMenu)
        {
            _potentialParentMenu = callingMenu;
            BootstrapManager.Instance.OnServerMigrationFinished += SetupMenu;
            BootstrapManager.Instance.OnServerMigrationFailed += CancelMenu;
        }

        private void CancelMenu()
        {
            // TODO : print something to indicate lobby failed opening

            BootstrapManager.Instance.OnServerMigrationFinished -= SetupMenu;
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            BootstrapManager.Instance.OnServerMigrationFailed -= CancelMenu;
        }

        public override void SetupMenu()
        {
            _needSetup = true;
            _joinCodeLabel.text = "Join code : " +BootstrapManager.Instance.CurrentJoinCode;
            _isSetup = true;
            _parentMenu = _potentialParentMenu;
            _parentMenu.OpenSubMenuAndCloseCurrentMenu(this);
           //InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            StartCoroutine(BindEvents());
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                StartCoroutine(BindEvents());
            }
            else if (args.ConnectionState == LocalConnectionState.Stopping)
            {
                UnbindEvents();
            }
        }

        private IEnumerator BindEvents()
        {
            while (PlayerManager.HasInstance == false)
            {
                // We have to wait for the Server to spawn the PlayerManager
                yield return new WaitForSecondsRealtime(0.1f);
            }
            PlayerManager.Instance.OnRealPlayerInfosChanged += OnRealPlayerInfosChanged;
        }

        private void UnbindEvents()
        {
            if (PlayerManager.HasInstance == false) return;
            PlayerManager.Instance.OnRealPlayerInfosChanged -= OnRealPlayerInfosChanged;
        }

        private void OnRealPlayerInfosChanged(List<RealPlayerInfo> realPlayerInfos)
        {
            _playerSlotA.color = Color.red;
            _playerSlotB.color = Color.red;
            _playerSlotC.color = Color.red;
            _playerSlotD.color = Color.red;

            foreach (var realPlayerInfo in realPlayerInfos)
            {
                switch (realPlayerInfo.PlayerIndexType)
                {
                    case PlayerIndexType.A:
                        _playerSlotA.color = Color.green;
                        break;
                    case PlayerIndexType.B:
                        _playerSlotB.color = Color.green;
                        break;
                    case PlayerIndexType.C:
                        _playerSlotC.color = Color.green;
                        break;
                    case PlayerIndexType.D:
                        _playerSlotD.color = Color.green;
                        break;
                    case PlayerIndexType.Z:
                    default:
                        break;
                }
            }

            if (realPlayerInfos.Count == 4)
            {
                _startButton.interactable = true;
                EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
            } else
            {
                _startButton.interactable = false;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}