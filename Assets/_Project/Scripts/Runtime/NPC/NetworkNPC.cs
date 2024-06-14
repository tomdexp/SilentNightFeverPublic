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
        private Animator _animator;
        
        private static readonly int BodyColorParam = Shader.PropertyToID("_Body_Color");
        private static readonly int OffsetParam = Animator.StringToHash("Offset");

        private void Awake()
        {
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _animator = GetComponentInChildren<Animator>();
            _animator.SetFloat(OffsetParam, UnityEngine.Random.Range(0f, 1f));
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
            _skinnedMeshRenderer.material.SetColor(BodyColorParam, _currentColor);
        }

        private void Update()
        {
            _animator.cullingMode = _skinnedMeshRenderer.enabled ? AnimatorCullingMode.AlwaysAnimate : AnimatorCullingMode.CullCompletely;
        }
    }
}