using System;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class LocalPlayerColorByIndex : MonoBehaviour
    {
        [field:SerializeField] public PlayerIndexType PlayerIndexType { get; private set; }
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private MeshRenderer[] _meshRenderers;
        [SerializeField] private SkinnedMeshRenderer[] _skinnedMeshRenderers;

        private static readonly int BodyColorParam = Shader.PropertyToID("_Body_Color");

        private void Update()
        {
            SetColorByPlayerIndex();
        }

        private void SetColorByPlayerIndex()
        {
            switch (PlayerIndexType)
            {
                case PlayerIndexType.A:
                    SetColor(_playerData.PlayerAColor);
                    break;
                case PlayerIndexType.B:
                    SetColor(_playerData.PlayerBColor);
                    break;
                case PlayerIndexType.C:
                    SetColor(_playerData.PlayerCColor);
                    break;
                case PlayerIndexType.D:
                    SetColor(_playerData.PlayerDColor);
                    break;
            }
        }

        private void SetColor(Color color)
        {
            foreach (var meshRenderer in _meshRenderers) meshRenderer.material.color = color;
            foreach (var skinnedMeshRenderer in _skinnedMeshRenderers)
            {
                // set the "_Body_Color" property of the shader
                skinnedMeshRenderer.material.SetColor(BodyColorParam, color);
            }
        }
    }
}