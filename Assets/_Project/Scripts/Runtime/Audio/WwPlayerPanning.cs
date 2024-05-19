using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    public class WwPlayerPanning : MonoBehaviour
    {
        [Required] public NetworkPlayer NetworkPlayer;

        private float[] _volumesOffset = new float[2];
        private AkChannelConfig channelConfig = new AkChannelConfig();

        [SerializeField] private bool isSpatialized = true;

        void Start()
        {
            channelConfig = AkChannelConfig.Standard(AkSoundEngine.AK_SPEAKER_SETUP_STEREO);

            _volumesOffset[0] = 0;
            _volumesOffset[1] = -14;
        
            switch (NetworkPlayer.GetPlayerIndexType())
            {
                case PlayerIndexType.A:
                    LeftSpeakerOffset();
                    break;
                case PlayerIndexType.B:
                    RightSpeakerOffset();
                    break;
                case PlayerIndexType.C:
                    LeftSpeakerOffset();
                    break;
                case PlayerIndexType.D:
                    RightSpeakerOffset();
                    break;
                case PlayerIndexType.Z:
                    break;
            }
            
            AkSoundEngine.SetListenerSpatialization(gameObject, isSpatialized, channelConfig, _volumesOffset);
            Logger.LogTrace($"Player panning initialized at [{_volumesOffset[0]},{_volumesOffset[1]}] for Player " + NetworkPlayer.GetPlayerIndexType(), Logger.LogType.Local, this);
        }

        private void LeftSpeakerOffset()
        {
            _volumesOffset[0] = AudioManager.Instance.AudioManagerData.LeftSpeakerVolumeOffset[0];
            _volumesOffset[1] = AudioManager.Instance.AudioManagerData.LeftSpeakerVolumeOffset[1];
        }

        private void RightSpeakerOffset()
        {
            _volumesOffset[0] = AudioManager.Instance.AudioManagerData.RightSpeakerVolumeOffset[0];
            _volumesOffset[1] = AudioManager.Instance.AudioManagerData.RightSpeakerVolumeOffset[1];
        }
    }
}
