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

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _root = _uiDocument.rootVisualElement;
            if (_uiDocument == null)
            {
                Debug.LogError("No UIDocument found on PlayerManagerUIDocumentBinder.");
                return;
            }

            _playerA = _root.Query<VisualElement>("playerInfosA").Children<Label>("playerValue");
            _playerB = _root.Query<VisualElement>("playerInfosB").Children<Label>("playerValue");
            _playerC = _root.Query<VisualElement>("playerInfosC").Children<Label>("playerValue");
            _playerD = _root.Query<VisualElement>("playerInfosD").Children<Label>("playerValue");
            
            if (_playerA == null || _playerB == null || _playerC == null || _playerD == null)
            {
                Debug.LogError("PlayerManagerUIDocumentBinder: One of the player labels is null.");
                return;
            }
            
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
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
            Debug.Log("PlayerManagerUIDocumentBinder: PlayerManager found. Binding events.");
        }
        
        private void UnbindEvents()
        {
            if (PlayerManager.HasInstance) PlayerManager.Instance.OnRealPlayerInfosChanged -= OnRealPlayerInfosChanged;
            Debug.Log("PlayerManagerUIDocumentBinder: Unbinding events.");
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
    }
}