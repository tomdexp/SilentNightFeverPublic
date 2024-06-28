using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class EpilespyDisabler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _inactiveWhenEpilespyFilterEnabled;
        [SerializeField, ReadOnly] private bool _isHidden = false;

        private void Start()
        {
            OnEpilepsyFilterEnableChanged(ApplicationSettings.ApplicationSettings.EpilepsyFilterEnable.Value);
            ApplicationSettings.ApplicationSettings.EpilepsyFilterEnable.OnValueChanged += OnEpilepsyFilterEnableChanged;
        }
        
        private void OnDestroy()
        {
            ApplicationSettings.ApplicationSettings.EpilepsyFilterEnable.OnValueChanged -= OnEpilepsyFilterEnableChanged;
        }

        private void OnEpilepsyFilterEnableChanged(bool isEnabled)
        {
            if (ApplicationSettings.ApplicationSettings.HighContrastFilterEnable.Value && !isEnabled) // to avoid activating the objects when high contrast filter is enabled
            {
                return;
            }
            foreach (var go in _inactiveWhenEpilespyFilterEnabled)
            {
                if (!go)
                {
                    continue;
                }
                go.SetActive(!isEnabled);
            }
            _isHidden = isEnabled;
        }
    }
}