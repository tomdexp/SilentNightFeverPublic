using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_BindRandomText : MonoBehaviour
    {
        [SerializeField] private LocalizedString[] _availableStrings;
        
        private TMP_Text _text;
        private int _currentIndex;

        private void Start()
        {
            _text = GetComponent<TMP_Text>();
            PickRandomText();
            LocalizationSettings.Instance.OnSelectedLocaleChanged += OnLocaleChanged;
        }

        private void OnDestroy()
        {
            LocalizationSettings.Instance.OnSelectedLocaleChanged -= OnLocaleChanged;
        }
        
        private void OnLocaleChanged(Locale _)
        {
            UpdateText();
        }

        public void PickRandomText()
        {
            if (_availableStrings.Length == 0) return;
            _currentIndex = Random.Range(0, _availableStrings.Length);
            UpdateText();
        }

        private void UpdateText()
        {
            _text.text = _availableStrings[_currentIndex].GetLocalizedString();
        }
    }
}