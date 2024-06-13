using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    /// <summary>
    /// This scriptable holds all data relevant to UI, colors, fonts, fade easing, etc.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(UIData), menuName = "Scriptable Objects/" + nameof(UIData))]
    public class UIData : ScriptableObject
    {
        [Title("Team A Settings")]
        public Color TeamAColor = Color.red;
        public Sprite TeamASprite;
        
        [Title("Team B Settings")]
        public Color TeamBColor = Color.blue;
        public Sprite TeamBSprite;
        
        [Title("Transition Settings", "Fade")]
        public float TransitionFadeInDuration = 1.0f;
        public Ease TransitionFadeInEase = Ease.Linear;
        public float TransitionFadeOutDuration = 1.0f;
        public Ease TransitionFadeOutEase = Ease.Linear;
        
        [Title("Transition Settings", "Loading Game")]
        public float TransitionLoadingGameFadeInDuration = 1.0f;
        public Ease TransitionLoadingGameFadeInEase = Ease.Linear;
        public float TransitionLoadingGameFadeOutDuration = 1.0f;
        public Ease TransitionLoadingGameFadeOutEase = Ease.Linear;
        
        [Title("Transition Settings", "Loading Round")]
        public float TransitionLoadingRoundFadeInDuration = 1.0f;
        public Ease TransitionLoadingRoundFadeInEase = Ease.Linear;
        public float TransitionLoadingRoundFadeOutDuration = 1.0f;
        public Ease TransitionLoadingRoundFadeOutEase = Ease.Linear;
        
        [Title("Button Settings", "Enter Type")]
        public float HoverEnterScale = 1.1f;
        public float HoverEnterDuration = 0.1f;
        public Ease HoverEnterEase = Ease.Linear;
        public float ClickEnterScale = 0.9f;
        public float ClickEnterDuration = 0.1f;
        public Ease ClickEnterEase = Ease.Linear;
        
        [Title("Button Settings", "Back Type")]
        public float HoverBackScale = 1.1f;
        public float HoverBackDuration = 0.1f;
        public Ease HoverBackEase = Ease.Linear;
        public float ClickBackScale = 0.9f;
        public float ClickBackDuration = 0.1f;
        public Ease ClickBackEase = Ease.Linear;
        
        [Title("Button Settings", "Open Animation")]
        public float OpenAnimDurationScaleUp = 0.5f;
        public Ease OpenAnimEaseScaleUp = Ease.OutBack;
        public float OpenAnimDurationScaleUpFactor = 1.2f;
        public float OpenAnimDurationScaleDown = 0.5f;
        public Ease OpenAnimEaseScaleDown = Ease.OutBack;

        [Title("Team Selection Canvas Settings")]
        public float SecondsAfterAllPlayersReadyToStartCustomization = 3;
        public float PlayerTeamLabelMovementDuration = 1;
        public Ease PlayerTeamLabelMovementEase = Ease.Linear;
        
        [Title("Language Selection")]
        public LanguageSelectionUI[] SupportedLanguages;
        public float LanguageButtonFadeDuration = 0.2f;
        public Ease LanguageButtonFadeEase = Ease.InExpo;
        public float SecondsBetweenLanguageButtonAppearAnimation = 0.2f;

        [Serializable]
        public class LanguageSelectionUI
        {
            public Sprite Flag;
            public Locale Locale;
            
        }

        [Button, DisableInEditorMode]
        private void PrintAllLocalesToConsole()
        {
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                Debug.Log($"Locale {i}: {locale.LocaleName} ({locale.Identifier.Code})");
            }
        }
    }
}