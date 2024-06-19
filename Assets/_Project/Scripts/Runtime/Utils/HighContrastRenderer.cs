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
        

        private void Update()
        {
            // check every 60 frames
            if (Time.frameCount % 60 != 0) return;
            var isHighContrast = ApplicationSettings.ApplicationSettings.HighContrastFilterEnable.Value;
            foreach (var material in Materials)
            {
                if (!material) continue;
                if (!material.HasProperty(HighContrast)) continue;
                material.SetFloat(HighContrast, isHighContrast ? 1 : 0);
            }
        }
        
    }
}