﻿using System;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.NPC
{
    public class NetworkNPC : NetworkBehaviour
    {
        [Title("Reference")]
        [SerializeField, Required] private PlayerData _playerData;
        private readonly SyncVar<Color> _color = new SyncVar<Color>();
        private SkinnedMeshRenderer _skinnedMeshRenderer;

        private void Awake()
        {
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _skinnedMeshRenderer.material = new Material(_skinnedMeshRenderer.material);
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
            ApplyColor();
        }
        
        private void ApplyColor()
        {
            _skinnedMeshRenderer.material.color = _color.Value;
        }
    }
}