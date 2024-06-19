using System;
using GameKit.Dependencies.Utilities;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(Renderer))]
    public class HighContrastRenderer : MonoBehaviour
    {
        private Renderer _renderer;
        private static readonly int HighContrast = Shader.PropertyToID("_High_Contrast");
        private Material[] Materials => _renderer.materials;
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
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
            foreach (var material in Materials)
            {
                if (!material) continue;
                if (!material.HasProperty(HighContrast)) continue;
                material.SetFloat(HighContrast, isEnabled ? 1 : 0);
            }
        }
    }
}