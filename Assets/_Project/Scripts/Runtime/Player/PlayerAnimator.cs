using System;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private bool _isMoving;
        
        private Animator _animator;
        private Rigidbody _rigidbody;
        private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponentInParent<PlayerController>().GetComponent<Rigidbody>();
            if (!_animator)
            {
                Logger.LogError("No Animator found on Player", Logger.LogType.Local, this);
            }
            if (!_rigidbody)
            {
                Logger.LogError("No Rigidbody found on Player", Logger.LogType.Local, this);
            }
        }

        private void Update()
        {
            _isMoving = _rigidbody.velocity.magnitude > 0.1f;
            _animator.SetBool(IsMovingParam, _isMoving);
        }
    }
}