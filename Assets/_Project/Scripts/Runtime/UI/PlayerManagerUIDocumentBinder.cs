using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Runtime.UI
{
    public class PlayerManagerUIDocumentBinder : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        
        private Label _playerA;
        private Label _playerB;
        private Label _playerC;
        private Label _playerD;
        private Label _possessedPlayersLabel;
        private string _possessedPlayersLabelDefaultText;

        private void Start()
        {
            _uiDocument = GetComponent<UIDocument>();
            _root = _uiDocument.rootVisualElement;
            if (_uiDocument == null)
            {
                Utils.Logger.LogError("No UIDocument found on PlayerManagerUIDocumentBinder.", context:this);
                return;
            }

            _playerA = _root.Query<VisualElement>("playerInfosA").Children<Label>("playerValue");
            _playerB = _root.Query<VisualElement>("playerInfosB").Children<Label>("playerValue");
            _playerC = _root.Query<VisualElement>("playerInfosC").Children<Label>("playerValue");
            _playerD = _root.Query<VisualElement>("playerInfosD").Children<Label>("playerValue");
            _possessedPlayersLabel = _root.Query<Label>("player-possessions-infos-value");
            
            if (_playerA == null || _playerB == null || _playerC == null || _playerD == null)
            {
                Utils.Logger.LogError("PlayerManagerUIDocumentBinder: One of the player labels is null.", context:this);
                return;
            }
            if (_possessedPlayersLabel == null)
            {
                Utils.Logger.LogError("PlayerManagerUIDocumentBinder: Possessed players label is null.", context:this);
                return;
            }
            
            _possessedPlayersLabelDefaultText = _possessedPlayersLabel.text;
            
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.ClientManager) InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
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
            PlayerManager.Instance.OnRealPlayerPossessed += OnRealPlayerPossessed;
            PlayerManager.Instance.OnRealPlayerUnpossessed += OnRealPlayerUnpossessed;
            Utils.Logger.LogTrace("PlayerManagerUIDocumentBinder: PlayerManager found. Binding events.", context:this);
        }

        private void UnbindEvents()
        {
            if (PlayerManager.HasInstance == false) return;
            PlayerManager.Instance.OnRealPlayerInfosChanged -= OnRealPlayerInfosChanged;
            PlayerManager.Instance.OnRealPlayerPossessed -= OnRealPlayerPossessed;
            PlayerManager.Instance.OnRealPlayerUnpossessed -= OnRealPlayerUnpossessed;
            Utils.Logger.LogTrace("PlayerManagerUIDocumentBinder: Unbinding events.", context:this);
        }

        private void OnRealPlayerInfosChanged(List<RealPlayerInfo> realPlayerInfos)
        {
            _playerA.text = "EMPTY";
            _playerB.text = "EMPTY";
            _playerC.text = "EMPTY";
            _playerD.text = "EMPTY";
            
            foreach (var realPlayerInfo in realPlayerInfos)
            {
                switch (realPlayerInfo.PlayerIndexType)
                {
                    case PlayerIndexType.A:
                        _playerA.text = realPlayerInfo.DevicePath + " - " + realPlayerInfo.ClientId;
                        break;
                    case PlayerIndexType.B:
                        _playerB.text = realPlayerInfo.DevicePath + " - " + realPlayerInfo.ClientId;
                        break;
                    case PlayerIndexType.C:
                        _playerC.text = realPlayerInfo.DevicePath + " - " + realPlayerInfo.ClientId;
                        break;
                    case PlayerIndexType.D:
                        _playerD.text = realPlayerInfo.DevicePath + " - " + realPlayerInfo.ClientId;
                        break;
                    case PlayerIndexType.Z:
                    default:
                        break;
                }
            }
        }
        
        private void OnRealPlayerPossessed(RealPlayerInfo source, RealPlayerInfo target)
        {
            _possessedPlayersLabel.text = source.PlayerIndexType + " ---is possessing--> " + target.PlayerIndexType;
        }
        
        private void OnRealPlayerUnpossessed(RealPlayerInfo realPlayerInfo)
        {
            _possessedPlayersLabel.text = _possessedPlayersLabelDefaultText;
        }
    }
}