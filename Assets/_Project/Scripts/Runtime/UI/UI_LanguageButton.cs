using System;
using System.Collections;
using DG.Tweening;
using Mono.CSharp;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    public class UI_LanguageButton : MonoBehaviour
    {
        [SerializeField, Required] private UIData _uiData;
        [SerializeField, Required] private Image _flagImage;
        [SerializeField, Required] private TMP_Text _languageNameText;
        [SerializeField, Required] private TMP_Text _languageCodeText;
        
        private UIData.LanguageSelectionUI _languageSelectionUI;
        private Button _button;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _button.onClick.AddListener(OnClick);
            if (!_flagImage)
            {
                Logger.LogError("Flag Image not set", Logger.LogType.Local, this);
            }
            if (!_languageNameText)
            {
                Logger.LogError("Language Name Text not set", Logger.LogType.Local, this);
            }
            if (!_languageCodeText)
            {
                Logger.LogError("Language Code Text not set", Logger.LogType.Local, this);
            }
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            StartCoroutine(SelectNewLocaleCoroutine());
        }
        
        public void SetLanguageSelectionUI(UIData.LanguageSelectionUI languageSelectionUI)
        {
            _languageSelectionUI = languageSelectionUI;
            _flagImage.sprite = languageSelectionUI.Flag;
            string languageName = languageSelectionUI.Locale.LocaleName;
            // remove the Identifier from the language name
            if (languageName.Contains("("))
            {
                languageName = languageName.Substring(0, languageName.IndexOf("(", StringComparison.Ordinal));
            }
            _languageNameText.text = languageName;
            _languageCodeText.text = languageSelectionUI.Locale.Identifier.Code;
        }

        public void Show()
        {
            _canvasGroup.DOFade(1, _uiData.LanguageButtonFadeDuration).SetEase(_uiData.LanguageButtonFadeEase);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }
        
        public void Hide()
        {
            _canvasGroup.DOFade(0, _uiData.LanguageButtonFadeDuration).SetEase(_uiData.LanguageButtonFadeEase);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private IEnumerator SelectNewLocaleCoroutine()
        {
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = _languageSelectionUI.Locale;
            Logger.LogInfo("Selected Locale: " + _languageSelectionUI.Locale.LocaleName, Logger.LogType.Local, this);
        }
    }
}