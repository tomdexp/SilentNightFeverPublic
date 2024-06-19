using System;
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

        private void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            if (!PlayerManager.HasInstance)
            {
                NotReadyPlayer();
                return;
            }

            var readyPlayerInfos = PlayerManager.Instance.GetPlayerReadyInfos();
            if (readyPlayerInfos == null)
            {
                NotReadyPlayer();
                return;
            }
            
            var readyPlayerInfo = readyPlayerInfos.FirstOrDefault(x => x.PlayerIndexType == _playerIndexType);
            if (readyPlayerInfo.IsPlayerReady)
            {
                ReadyPlayer();
            }
            else
            {
                NotReadyPlayer();
            }
        }

        private void ReadyPlayer()
        {
            _image.color = _playerReadyColor;
            _feedbacksReady?.PlayFeedbacks();
        }

        private void NotReadyPlayer()
        {
            _image.color = _noPlayerColor;
            _feedbacksReady?.StopFeedbacks();
            _feedbacksReady?.RestoreInitialValues();
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