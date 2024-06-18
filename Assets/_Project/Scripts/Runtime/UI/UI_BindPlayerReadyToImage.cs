using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using FishNet.Transporting;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_BindPlayerReadyToImage : MonoBehaviour
    {
        [SerializeField] private PlayerIndexType _playerIndexType;
        [SerializeField] private Image _image;
        [SerializeField] private Color _noPlayerColor;
        [SerializeField] private Color _playerReadyColor;
        
        private MMF_Player _feedbacksReady;
        private bool _isRegistered;
        
        
        private void Start()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            StartCoroutine(TryRegisterPlayerManagerEvents());
            UpdateUI();
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
            if (_isRegistered) yield break;
            PlayerManager.Instance.OnPlayersReadyChanged += OnPlayersReadyChanged;
            _isRegistered = true;
        }

        private void OnDestroy()
        {
            if(InstanceFinder.ClientManager) InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            if (PlayerManager.HasInstance) PlayerManager.Instance.OnPlayersReadyChanged -= OnPlayersReadyChanged;
        }

        private void OnPlayersReadyChanged(List<PlayerReadyInfo> _)
        {
            UpdateUI();
        }
        
        public void UpdateUI()
        {
            if (!PlayerManager.HasInstance)
            {
                _image.color = _noPlayerColor;
                _feedbacksReady?.StopFeedbacks();
                _feedbacksReady?.RestoreInitialValues();
                return;
            }

            var readyPlayerInfos = PlayerManager.Instance.GetPlayerReadyInfos();
            if (readyPlayerInfos == null)
            {
                _image.color = _noPlayerColor;
                _feedbacksReady?.StopFeedbacks();
                _feedbacksReady?.RestoreInitialValues();
                return;
            }
            
            var readyPlayerInfo = readyPlayerInfos.FirstOrDefault(x => x.PlayerIndexType == _playerIndexType);
            if (readyPlayerInfo.IsPlayerReady)
            {
                _image.color = _playerReadyColor;
                _feedbacksReady?.PlayFeedbacks();
            }
            else
            {
                _image.color = _noPlayerColor;
                _feedbacksReady?.StopFeedbacks();
                _feedbacksReady?.RestoreInitialValues();
            }
        }
        
        public void SetReadyColor(Color color)
        {
            _playerReadyColor = color;
        }
        
        public void SetReadyFeedbacks(MMF_Player feedbacks)
        {
            _feedbacksReady = feedbacks;
        }
    }
}