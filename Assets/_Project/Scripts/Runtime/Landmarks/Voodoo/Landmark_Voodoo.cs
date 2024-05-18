using System;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Landmarks.Voodoo
{
    public class Landmark_Voodoo : Landmark
    {
        public new LandmarkData_Voodoo Data
        {
            get => (LandmarkData_Voodoo)base.Data;
            set => base.Data = value;
        }
        
        [Title("Landmark Voodoo References")]
        [SerializeField] private Transform _playerAPuppet;
        [SerializeField] private Transform _playerBPuppet;
        [SerializeField] private Transform _playerCPuppet;
        [SerializeField] private Transform _playerDPuppet;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private Vector2 _playerADirection;
        [SerializeField, ReadOnly] private Vector2 _playerBDirection;
        [SerializeField, ReadOnly] private Vector2 _playerCDirection;
        [SerializeField, ReadOnly] private Vector2 _playerDDirection;
        
        private Vector3 _playerAPuppetInitialPosition;
        private Vector3 _playerBPuppetInitialPosition;
        private Vector3 _playerCPuppetInitialPosition;
        private Vector3 _playerDPuppetInitialPosition;

        public override void OnStartServer()
        {
            _playerAPuppetInitialPosition = _playerAPuppet.position;
            _playerBPuppetInitialPosition = _playerBPuppet.position;
            _playerCPuppetInitialPosition = _playerCPuppet.position;
            _playerDPuppetInitialPosition = _playerDPuppet.position;
        }

        private void Update()
        {
            if(!IsServerStarted) return;
            if(!PlayerManager.HasInstance) return;
            if(PlayerManager.Instance.NumberOfPlayers != 4) return;
            ApplyVoodooPuppetDirection(_playerAPuppet, _playerAPuppetInitialPosition, ref _playerADirection, PlayerIndexType.A);
            ApplyVoodooPuppetDirection(_playerBPuppet, _playerBPuppetInitialPosition, ref _playerBDirection, PlayerIndexType.B);
            ApplyVoodooPuppetDirection(_playerCPuppet, _playerCPuppetInitialPosition, ref _playerCDirection, PlayerIndexType.C);
            ApplyVoodooPuppetDirection(_playerDPuppet, _playerDPuppetInitialPosition, ref _playerDDirection, PlayerIndexType.D);
        }

        protected override void ResetLandmark(byte roundNumber)
        {
            Logger.LogTrace("Resetting Landmark " + nameof(Landmark_Voodoo), Logger.LogType.Server, this);
            _playerAPuppet.position = _playerAPuppetInitialPosition;
            _playerBPuppet.position = _playerBPuppetInitialPosition;
            _playerCPuppet.position = _playerCPuppetInitialPosition;
            _playerDPuppet.position = _playerDPuppetInitialPosition;
        }

        private void ApplyVoodooPuppetDirection(Transform puppet,Vector3 initialPosition, ref Vector2 currentDirection, PlayerIndexType playerIndexType)
        {
            var isIdle = Vector3.Distance(puppet.position, initialPosition) < Data.MinDistanceThreshold;
            if (isIdle)
            {
                currentDirection = Vector2.zero;
                PlayerManager.Instance.SetVoodooPuppetDirection(playerIndexType, Vector2.zero);
                return;
            }
            // if the puppet is not idle then gen it direction from its initial position
            var direction = (puppet.position - initialPosition).normalized;
            // convert the direction to a vector2 by ignoring the y axis
            var direction2D = new Vector2(direction.x, direction.z);
            currentDirection = direction2D;
            PlayerManager.Instance.SetVoodooPuppetDirection(playerIndexType, direction2D);
        }
    }
}