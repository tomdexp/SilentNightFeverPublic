using System;
using FishNet.Observing;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class HighContrastDisabler : MonoBehaviour
    {
        [SerializeField] private GameObject[] _inactiveWhenHighContrast;
        [SerializeField, ReadOnly] private bool _isHidden = false;
        
        private Renderer _observerRenderer; // use to determine if this is culled of not

        public bool IsCulled
        {
            get
            {
                if (_observerRenderer) return !_observerRenderer.enabled;
                return false;
            }
        }

        private void Awake()
        {
            _observerRenderer = GetComponentInChildren<Renderer>();
            if (!_observerRenderer)
            {
                //Logger.LogWarning("No renderer found in children of " + gameObject.name);
            }
        }

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
            if (ApplicationSettings.ApplicationSettings.EpilepsyFilterEnable.Value && !isEnabled) // to avoid activating the objects when epilepsy filter is enabled
            {
                return;
            }
            foreach (var go in _inactiveWhenHighContrast)
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