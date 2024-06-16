using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_Toggle : MonoBehaviour
    {
        [SerializeField, Required] private Toggle _toggle;
        
        public event Action<bool> OnValueChanged;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool newValue)
        {
            OnValueChanged?.Invoke(newValue);
        }

        public void SetValue(bool newValue)
        {
            _toggle.isOn = newValue;
        }
    }
}