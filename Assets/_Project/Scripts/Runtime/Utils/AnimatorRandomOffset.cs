using System;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorRandomOffset : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int OffsetParam = Animator.StringToHash("Offset");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.SetFloat(OffsetParam, UnityEngine.Random.Range(0f, 1f));
        }
    }
}