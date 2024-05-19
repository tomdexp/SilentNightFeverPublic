using System;
using _Project.Scripts.Runtime.Networking;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ColorRandomize : NetworkBehaviour
    {
        private MeshRenderer[] _meshRenderers;
        [AllowMutableSyncType]  private SyncVar<Color> _randomColor;

        private void OnEnable()
        {
            _randomColor.OnChange += SetRandomColor;
        }
        private void OnDisable()
        {
            _randomColor.OnChange -= SetRandomColor;
        }

        private void SetRandomColor(Color prev, Color next, bool asServer)
        {
            SetRandomColor();
        }

        private void SetRandomColor()
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material.color = _randomColor.Value;
            }
        }

        private void Start()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material = new Material(meshRenderer.material);
            }

            SetRandomColor();

            if (IsServerStarted)
            {
                float r = UnityEngine.Random.Range(0, 1f);
                float g = UnityEngine.Random.Range(0, 1f);
                float b = UnityEngine.Random.Range(0, 1f);
                _randomColor.Value = new Color(r, g, b, 1);
            }

        }
    }
}