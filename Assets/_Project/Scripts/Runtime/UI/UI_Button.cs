﻿using System;
using _Project.Scripts.Runtime.Audio;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        
        private Button _button;
        private float _originalScale;
        
        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
            _originalScale = transform.localScale.x;
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (_buttonType == ButtonType.Enter)
            {
                if(AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIButtonClickEnter, AudioManager.Instance.gameObject);
                transform.DOScale(_uiData.ClickEnterScale, _uiData.ClickEnterDuration).SetEase(_uiData.ClickEnterEase).OnComplete(() =>
                {
                    transform.DOScale(_originalScale, _uiData.ClickEnterDuration).SetEase(_uiData.ClickEnterEase);
                });
            }
            else
            {
                if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIButtonClickBack, AudioManager.Instance.gameObject);
                transform.DOScale(_uiData.ClickBackScale, _uiData.ClickBackDuration).SetEase(_uiData.ClickBackEase).OnComplete(() =>
                {
                    transform.DOScale(_originalScale, _uiData.ClickBackDuration).SetEase(_uiData.ClickBackEase);
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
            if(AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventUIButtonHover, AudioManager.Instance.gameObject);
            if(_buttonType == ButtonType.Enter)
            {
                transform.DOScale(_uiData.HoverEnterScale, _uiData.HoverEnterDuration).SetEase(_uiData.HoverEnterEase);
            }
            else
            {
                transform.DOScale(_uiData.HoverBackScale, _uiData.HoverBackDuration).SetEase(_uiData.HoverBackEase);
            }
        }
        
        private void PlayUnHoverFeedbacks()
        {
            if(_buttonType == ButtonType.Enter)
            {
                transform.DOScale(_originalScale, _uiData.HoverEnterDuration).SetEase(_uiData.HoverEnterEase);
            }
            else
            {
                transform.DOScale(_originalScale, _uiData.HoverBackDuration).SetEase(_uiData.HoverBackEase);
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
    }
}