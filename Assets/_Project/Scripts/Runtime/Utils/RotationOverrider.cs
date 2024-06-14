using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class RotationOverrider : MonoBehaviour
    {
        [SerializeField] private Vector3 _rotation;
        
        private void Update()
        {
            transform.rotation = Quaternion.Euler(_rotation);
        }
    }
}