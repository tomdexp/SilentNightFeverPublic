using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_BindRealPlayerToImage : MonoBehaviour
    {
        [SerializeField] private PlayerIndexType _playerIndexType;
        [SerializeField] private Image _image;
        [SerializeField] private Color _noPlayerColor;
        [SerializeField] private Color _playerPresentColor;
        
        private void Start()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            UpdateUI();
        }

        private void OnDestroy()
        {
            if(InstanceFinder.ClientManager) InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            if (PlayerManager.HasInstance) PlayerManager.Instance.OnRealPlayerInfosChanged -= OnRealPlayerInfosChanged;
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
            PlayerManager.Instance.OnRealPlayerInfosChanged += OnRealPlayerInfosChanged;
        }

        private void OnRealPlayerInfosChanged(List<RealPlayerInfo> _)
        {
            UpdateUI();
        }
        
        public void UpdateUI()
        {
            if (!PlayerManager.HasInstance)
            {
                _image.color = _noPlayerColor;
                return;
            }
            var realPlayerInfos = PlayerManager.Instance.GetRealPlayerInfos();
            if (realPlayerInfos == null)
            {
                _image.color = _noPlayerColor;
                return;
            }
            bool hasFoundPlayer = false;
            foreach (var realPlayerInfo in realPlayerInfos.Where(realPlayerInfo => realPlayerInfo.PlayerIndexType == _playerIndexType))
            {
                _image.color = _playerPresentColor;
                hasFoundPlayer = true;
            }
            if (!hasFoundPlayer)
            {
                _image.color = _noPlayerColor;
            }
        }
    }
}