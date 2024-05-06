using System;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ColorRandomize : MonoBehaviour
    {
        private MeshRenderer[] _meshRenderers;
        private Color _randomColor;

        private void Awake()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material = new Material(meshRenderer.material);
            }

            // Générer des valeurs RGB aléatoires
            float r = UnityEngine.Random.Range(0, 1f);
            float g = UnityEngine.Random.Range(0, 1f);
            float b = UnityEngine.Random.Range(0, 1f);
            // Créer une nouvelle couleur avec les valeurs aléatoires
            _randomColor = new Color(r, g, b, 1);
        }

        private void Start()
        {
            SetRandomColor();
        }

        [Button]
        private void SetRandomColor()
        {

            foreach (var meshRenderer in _meshRenderers)
            {
                //meshRenderer.material.SetColor("_Color", _randomColor);
                meshRenderer.material.color = _randomColor;
            }
        }
    }
}