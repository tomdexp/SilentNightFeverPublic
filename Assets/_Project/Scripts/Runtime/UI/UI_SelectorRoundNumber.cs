using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_SelectorRoundNumber : UI_SelectorBase
    {
        [SerializeField] private int[] _availableNumbers = { 1, 3, 5 };
        [SerializeField, Required] private TMP_Text _roundNumberText;
        
        private int _currentSelectedRoundNumberIndex = 1;
        public int SelectedRoundNumber => _availableNumbers[_currentSelectedRoundNumberIndex];

        private void Start()
        {
            UpdateText();
        }

        protected override void OnPreviousButtonClicked()
        {
            // cycle
            _currentSelectedRoundNumberIndex = _currentSelectedRoundNumberIndex == 0 ? _availableNumbers.Length - 1 : _currentSelectedRoundNumberIndex - 1;
            UpdateText();
        }

        protected override void OnNextButtonClicked()
        {
            // cycle
            _currentSelectedRoundNumberIndex = _currentSelectedRoundNumberIndex == _availableNumbers.Length - 1 ? 0 : _currentSelectedRoundNumberIndex + 1;
            UpdateText();
        }
        
        private void UpdateText()
        {
            _roundNumberText.text = _availableNumbers[_currentSelectedRoundNumberIndex].ToString();
        }
    }
}