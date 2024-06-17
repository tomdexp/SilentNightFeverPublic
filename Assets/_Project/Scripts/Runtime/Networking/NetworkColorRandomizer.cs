using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class NetworkColorRandomizer : NetworkBehaviour
    {
        [Title("Settings")]
        [SerializeField] private string _shaderPropertyName = "_Color";
        [SerializeField] private Color[] _availableColors;
        
        [Title("References")]
        [SerializeField] private MeshRenderer[] _meshRenderers;
        [SerializeField] private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        
        private readonly SyncVar<Color> _randomColor = new SyncVar<Color>();
        private Color _currentColor;
        private bool _isInitialized;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _randomColor.Value = _availableColors[Random.Range(0, _availableColors.Length)];
            ApplyColor();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!_isInitialized)
            {
                _currentColor = _randomColor.Value;
                _isInitialized = true;
                ApplyColor();
            }
            else
            {
                ApplyColor();
            }
        }

        private void ApplyColor()
        {
            if (!_isInitialized) return;
            foreach (var meshRenderer in _meshRenderers)
            {
                if (!meshRenderer) continue;
                if (meshRenderer.material.HasProperty(_shaderPropertyName))
                {
                    meshRenderer.material.SetColor(_shaderPropertyName, _currentColor);
                }
                else
                {
                    Logger.LogError($"Material of {meshRenderer.name} does not have a property named {_shaderPropertyName}", Logger.LogType.Client, this);
                }
                    
            }
            foreach (var skinnedMeshRenderer in _skinnedMeshRenderers)
            {
                if (!skinnedMeshRenderer) continue;
                if (skinnedMeshRenderer.material.HasProperty(_shaderPropertyName))
                {
                    skinnedMeshRenderer.material.SetColor(_shaderPropertyName, _currentColor);
                }
                else
                {
                    Logger.LogError($"Material of {skinnedMeshRenderer.name} does not have a property named {_shaderPropertyName}", Logger.LogType.Client, this);
                }
            }
        }
    }
}