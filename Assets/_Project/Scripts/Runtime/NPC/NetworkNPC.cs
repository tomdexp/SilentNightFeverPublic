using System;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.NPC
{
    public class NetworkNPC : NetworkBehaviour
    {
        [Title("Reference")]
        [SerializeField, Required] private PlayerData _playerData;
        [SerializeField, ReadOnly] private Color _currentColor;
        private readonly SyncVar<Color> _color = new SyncVar<Color>();
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private bool _isInitialized;

        private void Awake()
        {
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            //_skinnedMeshRenderer.material = new Material(_skinnedMeshRenderer.material);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _color.Value = _playerData.NPCColors[UnityEngine.Random.Range(0, _playerData.NPCColors.Length)];
            ApplyColor();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!_isInitialized)
            {
                _currentColor = _color.Value;
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
            _skinnedMeshRenderer.material.color = _currentColor;
        }
    }
}