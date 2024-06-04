using System;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    // This component act as a validation layer for the lobby code input field
    [RequireComponent(typeof(TMP_InputField))]
    public class UI_InputFieldLobbyCode : MonoBehaviour
    {
        private TMP_InputField _inputField;
        public event Action<bool, string> OnLobbyCodeChanged; // bool is true if the code is valid, string is the code
        private StringBuilder _sb = new StringBuilder();
        
        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
            _inputField.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            _inputField.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(string newValue)
        {
            // A join code is composed of 6 alphanumeric characters (A-Z, 0-9)
            // 1st pass : make all letters uppercase
            // 2nd pass : check if the character is alphanumeric
            // 3rd pass : check if the length is 6
    
            _sb.Clear();
    
            // 1st pass
            newValue = newValue.ToUpper();

    
            // 2nd pass with char.IsLetterOrDigit
            for (int i = 0; i < newValue.Length; i++)
            {
                if (char.IsLetterOrDigit(newValue[i]))
                {
                    _sb.Append(newValue[i]);
                }
            }

            string filteredValue = _sb.ToString();

            // 3rd pass
            if (filteredValue.Length > 6)
            {
                filteredValue = filteredValue.Substring(0, 6);
            }

            _inputField.text = filteredValue;
            OnLobbyCodeChanged?.Invoke(filteredValue.Length == 6, filteredValue);
        }
    }
}