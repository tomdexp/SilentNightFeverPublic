using _Project.Scripts.Runtime.Utils.Singletons;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class HighContrastManager : PersistentSingleton<HighContrastManager>
    {
        public Material[] Materials;
        private static readonly int HighContrast = Shader.PropertyToID("_High_Contrast");

        private void Update()
        {
            // check every 60 frames
            if (Time.frameCount % 60 == 0)
            {
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
}