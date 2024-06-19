using _Project.Scripts.Runtime.Utils.ApplicationSettings;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(UI_Toggle))]
    public class UI_ToggleHighContrast : MonoBehaviour
    {
        private UI_Toggle _toggle;
        
        private void Start()
        {
            _toggle = GetComponent<UI_Toggle>();
            _toggle.SetValue(ApplicationSettings.HighContrastFilterEnable.Value);
            _toggle.OnValueChanged += OnValueChanged;
        }
        
        private void OnDestroy()
        {
            _toggle.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(bool newValue)
        {
            ApplicationSettings.HighContrastFilterEnable.Set(newValue);
        }
    }
}