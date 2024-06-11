using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using JetBrains.Annotations;
using Lofelt.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerTongue
{
    [RequireComponent(typeof(PlayerStickyTongue))]
    public class PlayerStickyTongueFeedbacks : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private ParticleSystem _tongueThrowParticles;
        [SerializeField] private ParticleSystem _tongueTipParticles;
        
        [Title("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private float _tensionPercent;
        
        private PlayerStickyTongue _playerStickyTongue;
        private NetworkPlayer _networkPlayer;
        private PlayerData _playerData;
        

        private void Start()
        {
            _playerStickyTongue = GetComponent<PlayerStickyTongue>();
            _networkPlayer = _playerStickyTongue.GetNetworkPlayer();
            _playerData = _networkPlayer.PlayerData;
            _playerStickyTongue.OnTongueOut += OnTongueOut;
            _playerStickyTongue.OnTongueBind += OnTongueBindOrInteract;
            _playerStickyTongue.OnTongueInteract += OnTongueBindOrInteract;
            //Logger.LogTrace("Playing vibration clip on tongue tension", Logger.LogType.Local, this);
            //GamepadRumbler.Load(_playerData.TongueTensionVibrationClip.gamepadRumble);
            //GamepadRumbler.Play();
            //HapticController.Load(_playerData.TongueTensionVibrationClip);
        }

        private void OnDestroy()
        {
            _playerStickyTongue.OnTongueOut -= OnTongueOut;
            _playerStickyTongue.OnTongueBind -= OnTongueBindOrInteract;
            _playerStickyTongue.OnTongueInteract -= OnTongueBindOrInteract;
        }
        
        private void OnTongueOut()
        {
            _tongueThrowParticles.Play();
        }
        
        private void OnTongueBindOrInteract()
        {
            _tongueTipParticles.Play();
        }

        /*
        private void Update()
        {
            _tensionPercent = _playerData.TongueTensionVibrationCurve.Evaluate((_playerStickyTongue.NormalizedTension.Value / 100f));
            HapticController.clipLevel = _tensionPercent;
            GamepadRumbler.Play();
        }

        private void OnDestroy()
        {
            GamepadRumbler.Stop();
            HapticController.Stop();
        }*/
    }
}