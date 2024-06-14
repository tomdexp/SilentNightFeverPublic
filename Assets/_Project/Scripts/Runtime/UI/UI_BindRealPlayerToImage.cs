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
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UI_BindRealPlayerToImage : MonoBehaviour
    {
        [SerializeField, Required] private UIData _uiData;
        [SerializeField] private PlayerIndexType _playerIndexType;
        [SerializeField, Required] private Image _image;
        [SerializeField, Required] private TMP_Text _joinText;
        [SerializeField, Required] private TMP_Text _playerText;
        [SerializeField] private Color _noPlayerColor;
        [SerializeField] private Color _playerPresentColor;
        [SerializeField] private float _width;
        [SerializeField, Required] private Transform _closeTransform;
        
        private RectTransform _rectTransform;
        private Vector3 _openPosition;
        private Vector3 _closePosition;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _openPosition = _rectTransform.localPosition;
            _closePosition = _closeTransform.localPosition;
        }

        private void Start()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            UpdateUI();
        }

        public void Open()
        {
            transform.DOLocalMove(_openPosition, _uiData.ControllerCanvasLeftToRightAnimDuration)
                .SetEase(_uiData.ControllerCanvasLeftToRightAnimEase);
        }

        public void Close()
        {
            transform.localPosition = _closePosition;
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
            _rectTransform.sizeDelta = new Vector2(_width, _rectTransform.sizeDelta.y);
            if (!PlayerManager.HasInstance)
            {
                _image.color = _noPlayerColor;
                _joinText.alpha = 1;
                _playerText.alpha = 0;
                return;
            }
            var realPlayerInfos = PlayerManager.Instance.GetRealPlayerInfos();
            if (realPlayerInfos == null)
            {
                _image.color = _noPlayerColor;
                _joinText.alpha = 1;
                _playerText.alpha = 0;
                return;
            }
            bool hasFoundPlayer = false;
            foreach (var realPlayerInfo in realPlayerInfos.Where(realPlayerInfo => realPlayerInfo.PlayerIndexType == _playerIndexType))
            {
                _image.color = _playerPresentColor;
                hasFoundPlayer = true;
                _joinText.alpha = 0;
                _playerText.alpha = 1;
            }
            if (!hasFoundPlayer)
            {
                _image.color = _noPlayerColor;
                _joinText.alpha = 1;
                _playerText.alpha = 0;
            }
        }
    }
}