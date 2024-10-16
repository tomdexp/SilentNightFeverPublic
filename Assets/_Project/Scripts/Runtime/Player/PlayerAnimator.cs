﻿using System;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private PlayerData _playerData;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private bool _isMoving;
        [SerializeField, ReadOnly] private float _speed;
        [SerializeField, ReadOnly] private float _movementSpeed;
        [SerializeField, ReadOnly] private bool _isTongueOut;
        
        private Animator _animator;
        private Transform _transform;
        private PlayerAkAudioListener _playerAkAudioListener;
        private PlayerStickyTongue _playerStickyTongue;
        
        private Vector3 _lastPosition;
        private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
        private static readonly int MovementSpeedParam = Animator.StringToHash("MovementSpeed");
        private static readonly int OpenMouthParam = Animator.StringToHash("OpenMouth");
        private static readonly int CloseMouthParam = Animator.StringToHash("CloseMouth");

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _transform = GetComponentInParent<PlayerController>().transform;
            _playerStickyTongue = GetComponentInParent<PlayerController>().GetTongue();
            _playerStickyTongue.OnTongueOut += OnTongueOut;
            _playerStickyTongue.OnTongueIn += OnTongueIn;
            if (!_animator)
            {
                Logger.LogError("No Animator found on Player", Logger.LogType.Local, this);
            }
            if (!_transform)
            {
                Logger.LogError("No Transform found on PlayerController", Logger.LogType.Local, this);
            }
            if (!_playerStickyTongue)
            {
                Logger.LogError("No PlayerStickyTongue found in parent !", Logger.LogType.Local, this);
            }
            _playerAkAudioListener = GetComponentInParent<PlayerAkAudioListener>();
            if (!_playerAkAudioListener)
            {
                Logger.LogError("No PlayerAkAudioListener found in parent !", Logger.LogType.Local, this);
            }
        }

        private void OnDestroy()
        {
            _playerStickyTongue.OnTongueOut -= OnTongueOut;
            _playerStickyTongue.OnTongueIn -= OnTongueIn;
        }

        private void FixedUpdate()
        {
            _speed = (_transform.position - _lastPosition).magnitude / Time.deltaTime;
            _isMoving = _speed > 0.01f;
            _animator.SetBool(IsMovingParam, _isMoving);
            _lastPosition = _transform.position;
        }

        private void LateUpdate()
        {
            // threshold
            if (_speed < 0.01f)
            {
                _animator.SetFloat(MovementSpeedParam, 0);
                return;
            }
            _movementSpeed = _speed / _playerData.PlayerMaxSpeedForAnimation;
            _movementSpeed = Mathf.Clamp(_movementSpeed, 0, 1);
            _movementSpeed = Mathf.Lerp(_animator.GetFloat(MovementSpeedParam), _movementSpeed, Time.deltaTime * 10);
            _animator.SetFloat(MovementSpeedParam, _movementSpeed);
        }
        
        
        private void OnTongueOut()
        {
            if (_isTongueOut) return;
            _animator.SetTrigger(OpenMouthParam);
            _isTongueOut = true;
        }
        
        private void OnTongueIn()
        {
            if (!_isTongueOut) return;
            _animator.SetTrigger(CloseMouthParam);
            _isTongueOut = false;
        }

        public void PlayFootstepSound(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight < 0.5f) return;
            if (AudioManager.HasInstance)
            { 
                AudioManager.Instance.PlayAudioLocal(AudioManager.Instance.AudioManagerData.EventPlayerFootstep, _playerAkAudioListener.gameObject);
            }
        }
    }
}