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
                    PlayerABoffset();
                    break;
                case PlayerIndexType.B:
                    PlayerABoffset();
                    break;
                case PlayerIndexType.C:
                    PlayerCDoffset();
                    break;
                case PlayerIndexType.D:
                    PlayerCDoffset();
                    break;
                case PlayerIndexType.Z:
                    break;
            }
            
            AkSoundEngine.SetListenerSpatialization(gameObject, isSpatialized, channelConfig, _volumesOffset);
            Logger.LogTrace($"Player panning initialized at [{_volumesOffset[0]},{_volumesOffset[1]}] for Player " + NetworkPlayer.GetPlayerIndexType(), Logger.LogType.Local, this);
        }

        private void PlayerABoffset()
        {
            _volumesOffset[0] = 0;
            _volumesOffset[1] = -96;
        }

        private void PlayerCDoffset()
        {
            _volumesOffset[0] = -96;
            _volumesOffset[1] = 0;
        }

    }
}
