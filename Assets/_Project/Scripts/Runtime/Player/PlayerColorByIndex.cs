using System;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerColorByIndex : MonoBehaviour
    {
        [SerializeField] private Color _colorPlayerA;
        [SerializeField] private Color _colorPlayerB;
        [SerializeField] private Color _colorPlayerC;
        [SerializeField] private Color _colorPlayerD;
        private NetworkPlayer _networkPlayer;
        private MeshRenderer[] _meshRenderers;

        private void Awake()
        {
            _networkPlayer = GetComponent<NetworkPlayer>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material = new Material(meshRenderer.material);
            }
        }

        private void Update()
        {
            SetColorByPlayerIndex();
        }

        private void SetColorByPlayerIndex()
        {
            var playerIndexType = _networkPlayer.GetPlayerIndexType();
            switch (playerIndexType)
            {
                case PlayerIndexType.A:
                    SetColor(_colorPlayerA);
                    break;
                case PlayerIndexType.B:
                    SetColor(_colorPlayerB);
                    break;
                case PlayerIndexType.C:
                    SetColor(_colorPlayerC);
                    break;
                case PlayerIndexType.D:
                    SetColor(_colorPlayerD);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetColor(Color color)
        {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material.color = color;
            }
        }
    }
}