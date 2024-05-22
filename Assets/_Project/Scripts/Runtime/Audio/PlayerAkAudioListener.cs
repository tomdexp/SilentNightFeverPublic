using System;
using UnityEngine;

namespace _Project.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(AkAudioListener), typeof(AkGameObj))]
    public class PlayerAkAudioListener : MonoBehaviour
    {
        private AkAudioListener _akAudioListener;
        private AkGameObj _akGameObj;

        private void Awake()
        {
            _akAudioListener = GetComponent<AkAudioListener>();
            _akGameObj = GetComponent<AkGameObj>();
        }

        private void Start()
        {
            AudioManager.Instance.RegisterListener(_akAudioListener);
            _akAudioListener.StartListeningToEmitter(_akGameObj);
        }
    }
}