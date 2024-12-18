﻿using System;
using System.Collections;
using _Project.Scripts.Runtime.Audio;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public class UI_Button : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerExitHandler, IDeselectHandler
    {
        private enum ButtonType
        {
            Enter,
            Back,
        }
        
        [SerializeField] private ButtonType _buttonType;
        [SerializeField, Required] private UIData _uiData;
        [SerializeField] private bool _keepFocusOnClicked = false;
        [SerializeField] private bool _playScaleFeedbackOnClicked = true;
        [SerializeField, Required] private MMF_Player _hoverFeedback;
        [SerializeField, Required] private MMF_Player _clickFeedback;
        [SerializeField, Required] private MMF_Player _unHoverFeedback;
        [SerializeField, Required] private MMF_Player _lockFeedback;
        [SerializeField, Required] private MMF_Player _unlockFeedback;
        
        public event Action OnHover;
        public event Action OnUnHover;
        
        private Button _button;
        private float _secondsBetweenClick = 0.1f;
        private bool _isOpen = true;

        private void Awake()
        {
            // check if current scale is not 1,1,1
            if (!Mathf.Approximately(transform.localScale.x, 1) 
                || !Mathf.Approximately(transform.localScale.y, 1) 
                || !Mathf.Approximately(transform.localScale.z, 1))
            {
                Logger.LogWarning($"The scale of the button is not 1,1,1 (current is {transform.localScale}) on {gameObject.name}. This can cause issues with the animations. Please set the scale to 1,1,1.", Logger.LogType.Local, this);
            }
        }

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
            DOTween.Kill(gameObject);
        }

        private void OnClick()
        {
            _clickFeedback.PlayFeedbacks();
            if (_buttonType == ButtonType.Enter)
            {
                if(AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIButtonClickEnter);
                if (!_playScaleFeedbackOnClicked) return;
                transform.DOKill();
                transform.DOScale(_uiData.ClickEnterScale, _uiData.ClickEnterDuration).SetEase(_uiData.ClickEnterEase).OnComplete(() =>
                {
                    transform.DOScale(1f, _uiData.ClickEnterDuration).SetEase(_uiData.ClickEnterEase);
                });
            }
            else
            {
                if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIButtonClickBack);
                if (!_playScaleFeedbackOnClicked) return;
                transform.DOKill();
                transform.DOScale(_uiData.ClickBackScale, _uiData.ClickBackDuration).SetEase(_uiData.ClickBackEase).OnComplete(() =>
                {
                    transform.DOScale(1f, _uiData.ClickBackDuration).SetEase(_uiData.ClickBackEase);
                });
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayHoverFeedbacks();
        }

        public void OnSelect(BaseEventData eventData)
        {
            PlayHoverFeedbacks();
        }

        private void PlayHoverFeedbacks()
        {
            if(!_button.interactable) return;
            OnHover?.Invoke();
            _hoverFeedback.PlayFeedbacks();
            if(AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIButtonHover);
            if(_buttonType == ButtonType.Enter)
            {
                transform.DOKill();
                transform.DOScale(_uiData.HoverEnterScale, _uiData.HoverEnterDuration).SetEase(_uiData.HoverEnterEase);
            }
            else
            {
                transform.DOKill();
                transform.DOScale(_uiData.HoverBackScale, _uiData.HoverBackDuration).SetEase(_uiData.HoverBackEase);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            PlayUnHoverFeedbacks();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            PlayUnHoverFeedbacks();
        }
        
        private void PlayUnHoverFeedbacks()
        {
            if(!_button.interactable) return;
            OnUnHover?.Invoke();
            _unHoverFeedback.PlayFeedbacks();
            if(transform.localScale.x == 0) return; // to avoid the button to scale to 1 when the Close() method is called
            if(_buttonType == ButtonType.Enter)
            {
                transform.DOKill();
                transform.DOScale(1f, _uiData.HoverEnterDuration).SetEase(_uiData.HoverEnterEase);
            }
            else
            {
                transform.DOKill();
                transform.DOScale(1f, _uiData.HoverBackDuration).SetEase(_uiData.HoverBackEase);
            }
        }

        [Button]
        public void Open()
        {
            Vector3 scaleUpTarget = Vector3.one * _uiData.OpenAnimDurationScaleUpFactor;
            transform.DOKill();
            var sequence = DOTween.Sequence(gameObject);
            sequence.Append(transform.DOScale(scaleUpTarget, _uiData.OpenAnimDurationScaleUp).SetEase(_uiData.OpenAnimEaseScaleUp));
            sequence.Append(transform.DOScale(1f, _uiData.OpenAnimDurationScaleDown).SetEase(_uiData.OpenAnimEaseScaleDown));
            sequence.Play();
            _isOpen = true;
        }

        [Button]
        public void Close()
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            _isOpen = false;
        }

        [Button]
        public void Lock()
        {
            _lockFeedback.PlayFeedbacks();
            _button.interactable = false;
        }
        
        [Button]
        public void Unlock()
        {
            _unlockFeedback.PlayFeedbacks();
            _button.interactable = true;
        }

        private void Update()
        {
            if (!_isOpen) transform.localScale = Vector3.zero;
        }
    }
}