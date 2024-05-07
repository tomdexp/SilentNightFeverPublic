using System;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(AkAudioListener))]
    public class AkAudioListenerNoPanning : MonoBehaviour
    {
        private AkAudioListener _akAudioListener;
        private float[] _volumes = new float[2] {0, -14f};
        private AkChannelConfig _channelConfig = new AkChannelConfig();
        
        private void Awake()
        {
            _akAudioListener = GetComponent<AkAudioListener>();
        }

        private void Start()
        {
            _channelConfig.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_STEREO);
            var result = AkSoundEngine.SetListenerSpatialization(gameObject, false, _channelConfig, _volumes);
            Logger.LogTrace(result + " for " + gameObject.name, Logger.LogType.Local, this);
        }
    }
}