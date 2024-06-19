using System;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class HighContrastDisabler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _inactiveWhenHighContrast;

        private void Start()
        {
            OnHighContrastFilterEnableChanged(ApplicationSettings.ApplicationSettings.HighContrastFilterEnable.Value);
            ApplicationSettings.ApplicationSettings.HighContrastFilterEnable.OnValueChanged += OnHighContrastFilterEnableChanged;
        }
        
        private void OnDestroy()
        {
            ApplicationSettings.ApplicationSettings.HighContrastFilterEnable.OnValueChanged -= OnHighContrastFilterEnableChanged;
        }

        private void OnHighContrastFilterEnableChanged(bool isEnabled)
        {
            foreach (var go in _inactiveWhenHighContrast)
            {
                go.SetActive(!isEnabled);
            }
        }
    }
}