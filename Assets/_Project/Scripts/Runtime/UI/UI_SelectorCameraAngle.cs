using System;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_SelectorCameraAngle : UI_SelectorBase
    {
        [SerializeField, Required] private TMP_Text _cameraAngleText;
        [SerializeField] private LocalizedString _changeEveryRoundLocalizedString;
        [SerializeField] private LocalizedString _doNotChangeLocalizedString;

        public bool DoChangeCameraAngleEveryRound { get; private set; } = true;

        private void Start()
        {
            if (GameManager.HasInstance) GameManager.Instance.CanCameraHaveRandomAngleFromGameSettings = DoChangeCameraAngleEveryRound;
            UpdateText();
            LocalizationSettings.Instance.OnSelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            LocalizationSettings.Instance.OnSelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        private void OnSelectedLocaleChanged(Locale obj)
        {
            UpdateText();
        }

        protected override void OnPreviousButtonClicked()
        {
            DoChangeCameraAngleEveryRound = !DoChangeCameraAngleEveryRound;
            if (GameManager.HasInstance) GameManager.Instance.CanCameraHaveRandomAngleFromGameSettings = DoChangeCameraAngleEveryRound;
            UpdateText();
        }

        protected override void OnNextButtonClicked()
        {
            DoChangeCameraAngleEveryRound = !DoChangeCameraAngleEveryRound;
            if (GameManager.HasInstance) GameManager.Instance.CanCameraHaveRandomAngleFromGameSettings = DoChangeCameraAngleEveryRound;
            UpdateText();
        }
        
        private void UpdateText()
        {
            _cameraAngleText.text = DoChangeCameraAngleEveryRound ? _changeEveryRoundLocalizedString.GetLocalizedString() : _doNotChangeLocalizedString.GetLocalizedString();
        }
    }
}