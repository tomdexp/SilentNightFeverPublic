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
        private readonly SyncVar<Color> _randomColor = new SyncVar<Color>();

        private void Awake()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material = new Material(meshRenderer.material);
            }
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            float r = UnityEngine.Random.Range(0, 1f);
            float g = UnityEngine.Random.Range(0, 1f);
            float b = UnityEngine.Random.Range(0, 1f);
            _randomColor.Value = new Color(r, g, b, 1);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetRandomColor(_randomColor.Value);
        }

        private void Start()
        {
            _randomColor.OnChange += SetRandomColor;
        }


        private void OnDestroy()
        {
            _randomColor.OnChange -= SetRandomColor;
        }

        private void SetRandomColor(Color prev, Color next, bool asServer)
        {
            SetRandomColor(next);
        }

        private void SetRandomColor(Color newColor)
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material.color = newColor;
            }
        }
    }
}