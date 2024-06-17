using System;
using Mono.CSharp;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(UI_Toggle))]
    public class UI_ToggleVsync : MonoBehaviour
    {
        private UI_Toggle _toggle;
        
        private void Awake()
        {
            _toggle = GetComponent<UI_Toggle>();
            _toggle.SetValue(QualitySettings.vSyncCount == 1);
            _toggle.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            _toggle.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(bool newValue)
        {
            QualitySettings.vSyncCount = newValue ? 1 : 0;
        }
    }
}