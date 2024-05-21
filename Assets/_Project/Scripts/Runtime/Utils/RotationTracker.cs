using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [DisallowMultipleComponent]
    public class RotationTracker : MonoBehaviour
    {
        [Title("Rotation Tracker Settings")] 
        [SerializeField] private float _angleStep = 2;
        
        [Title("Debug (Read-Only)")]
        [field: SerializeField, ReadOnly] public float SignedAngle { get; private set; }
        [field: SerializeField, ReadOnly] public float UnsignedAngle { get; private set; }
        [SerializeField, ReadOnly] private float _lastAngle;
        
        public event Action<float> OnSignedAngleChanged;
        public event Action<float> OnUnsignedAngleChanged;

        private void Update()
        {
            var angle = transform.localEulerAngles.y;
            var signedAngle = angle > 180 ? angle - 360 : angle;
            var unsignedAngle = signedAngle < 0 ? -signedAngle : signedAngle;
            
            if (Math.Abs(_lastAngle - angle) > _angleStep)
            {
                _lastAngle = angle;
                SignedAngle = signedAngle;
                UnsignedAngle = unsignedAngle;
                OnSignedAngleChanged?.Invoke(signedAngle);
                OnUnsignedAngleChanged?.Invoke(unsignedAngle);
            }
        }
    }
}