using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using DG.Tweening;
using FishNet;
using FishNet.Transporting;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UI_BindRealPlayerToImage : MonoBehaviour
    {
        [SerializeField, Required] private UIData _uiData;
        [SerializeField] private PlayerIndexType _playerIndexType;
        [SerializeField, Required] private Image _image;
        [SerializeField, Required] private TMP_Text _joinText;
        [SerializeField, Required] private RectTransform _rectTransform;
        [SerializeField, Required] private TMP_Text _playerText;
        [SerializeField] private Color _noPlayerColor;
        [SerializeField] private Color _playerPresentColor;
        [SerializeField] private float _width;
        [SerializeField] private Vector3 _openPosition;
        [SerializeField] private Vector3 _closePosition;
        [SerializeField, Required] private MMF_Player _feedbacksReady;
        [SerializeField, Required] private MMF_Player _feedbacksNotReady;
        
        private bool _isPlayingReadyFeedbacks;
        private bool _isPlayingNotReadyFeedbacks;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Close();
        }

        private void Start()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            StartCoroutine(TryRegisterPlayerManagerEvents());
            UpdateUI();
        }

        public void Open()
        {
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIPlayerButtonMove, AudioManager.Instance.gameObject);
            _rectTransform.DOAnchorPos(_openPosition, _uiData.ControllerCanvasLeftToRightAnimDuration).SetEase(_uiData.ControllerCanvasLeftToRightAnimEase);
        }

        public void Close()
        {
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIPlayerButtonMove, AudioManager.Instance.gameObject);
            _rectTransform.DOAnchorPos(_closePosition, _uiData.ControllerCanvasLeftToRightAnimDuration).SetEase(_uiData.ControllerCanvasLeftToRightAnimEase);
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
            Logger.LogDebug("PlayerManager events registered", Logger.LogType.Local, this);
        }

        private void OnRealPlayerInfosChanged(List<RealPlayerInfo> _)
        {
            UpdateUI();
        }
        
        public void UpdateUI()
        {
            _rectTransform.sizeDelta = new Vector2(_width, _rectTransform.sizeDelta.y);
            if (!PlayerManager.HasInstance)
            {
                _image.color = _noPlayerColor;
                _joinText.alpha = 1;
                _playerText.alpha = 0;
                if (_isPlayingReadyFeedbacks)
                {
                    _feedbacksReady?.StopFeedbacks();
                    _feedbacksReady?.RestoreInitialValues();
                    _isPlayingReadyFeedbacks = false;
                }
                if (!_isPlayingNotReadyFeedbacks)
                {
                    _feedbacksNotReady?.PlayFeedbacks();
                    _isPlayingNotReadyFeedbacks = true;
                }
                return;
            }
            var realPlayerInfos = PlayerManager.Instance.GetRealPlayerInfos();
            if (realPlayerInfos == null)
            {
                _image.color = _noPlayerColor;
                _joinText.alpha = 1;
                _playerText.alpha = 0;
                if (_isPlayingReadyFeedbacks)
                {
                    _feedbacksReady?.StopFeedbacks();
                    _feedbacksReady?.RestoreInitialValues();
                    _isPlayingReadyFeedbacks = false;
                }
                if (!_isPlayingNotReadyFeedbacks)
                {
                    _feedbacksNotReady?.PlayFeedbacks();
                    _isPlayingNotReadyFeedbacks = true;
                }
                return;
            }
            bool hasFoundPlayer = false;
            foreach (var realPlayerInfo in realPlayerInfos.Where(realPlayerInfo => realPlayerInfo.PlayerIndexType == _playerIndexType))
            {
                _image.color = _playerPresentColor;
                hasFoundPlayer = true;
                _joinText.alpha = 0;
                _playerText.alpha = 1;

                if (!_isPlayingReadyFeedbacks)
                {
                    _feedbacksReady?.PlayFeedbacks();
                    Logger.LogDebug("Playing ready feedbacks", Logger.LogType.Local, this);
                    _isPlayingReadyFeedbacks = true;
                }
                if (_isPlayingNotReadyFeedbacks)
                {
                    _feedbacksNotReady?.StopFeedbacks();
                    _feedbacksNotReady?.RestoreInitialValues();
                    _isPlayingNotReadyFeedbacks = false;
                }
            }
            if (!hasFoundPlayer)
            {
                _image.color = _noPlayerColor;
                _joinText.alpha = 1;
                _playerText.alpha = 0;
                if (_isPlayingReadyFeedbacks)
                {
                    _feedbacksReady?.StopFeedbacks();
                    _feedbacksReady?.RestoreInitialValues();
                    _isPlayingReadyFeedbacks = false;
                }
                if (!_isPlayingNotReadyFeedbacks)
                {
                    _feedbacksNotReady?.PlayFeedbacks();
                    _isPlayingNotReadyFeedbacks = true;
                }
            }
        }
        
        [Button]
        public void RegisterOpenPosition()
        {
            _openPosition = _rectTransform.anchoredPosition;
        }
        
        [Button]
        public void RegisterClosePosition()
        {
            _closePosition = _rectTransform.anchoredPosition;
        }
    }
}