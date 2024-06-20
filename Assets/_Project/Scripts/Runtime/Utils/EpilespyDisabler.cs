using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class EpilespyDisabler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _inactiveWhenEpilespyFilterEnabled;

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
            foreach (var go in _inactiveWhenEpilespyFilterEnabled)
            {
                go.SetActive(!isEnabled);
            }
        }
    }
}