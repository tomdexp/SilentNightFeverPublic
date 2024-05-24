using System;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(NetworkPlayer))]
    public class PlayerColorByIndex : MonoBehaviour
    {
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
                    SetColor(_networkPlayer.PlayerData.PlayerAColor);
                    break;
                case PlayerIndexType.B:
                    SetColor(_networkPlayer.PlayerData.PlayerBColor);
                    break;
                case PlayerIndexType.C:
                    SetColor(_networkPlayer.PlayerData.PlayerCColor);
                    break;
                case PlayerIndexType.D:
                    SetColor(_networkPlayer.PlayerData.PlayerDColor);
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