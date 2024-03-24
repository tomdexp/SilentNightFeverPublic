using System;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class CinemachineCameraChannelMaskOverride : MonoBehaviour
    {
        [SerializeField] private OutputChannels _outputChannels;
        private CinemachineBrain _cinemachineBrain;
        private CinemachineCamera _cinemachineCamera;

        private void Awake()
        {
            _cinemachineBrain = GetComponent<CinemachineBrain>();
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            if (_cinemachineBrain != null)
            {
                _cinemachineBrain.ChannelMask = _outputChannels;
            }
            if (_cinemachineCamera != null)
            {
                _cinemachineCamera.OutputChannel = _outputChannels;
            }
        }
    }
}