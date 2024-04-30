using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerMover : MonoBehaviour
    {
        public Vector3 Direction;
        public float Speed;
        
        private CharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            _characterController.Move(Direction * Speed * Time.deltaTime);
        }
    }
}