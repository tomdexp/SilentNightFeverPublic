using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwPlayerPanning : MonoBehaviour
{
    [Required] public NetworkPlayer NetworkPlayer;

    private float[] vVolumes = new float[2];
    private AkChannelConfig channelConfig = new AkChannelConfig();

    [SerializeField] private bool isSpatialized = true;

    // Start is called before the first frame update
    void Start()
    {
        channelConfig = AkChannelConfig.Standard(AkSoundEngine.AK_SPEAKER_SETUP_STEREO);

        vVolumes[0] = 0;
        vVolumes[1] = -14;
        AkSoundEngine.SetListenerSpatialization(this.gameObject, isSpatialized, channelConfig, vVolumes);



        switch (NetworkPlayer.GetPlayerIndexType())
        {
            case _Project.Scripts.Runtime.Player.PlayerIndexType.A:
                PlayerABoffset();
                break;
            case _Project.Scripts.Runtime.Player.PlayerIndexType.B:
                PlayerABoffset();
                break;
            case _Project.Scripts.Runtime.Player.PlayerIndexType.C:
                PlayerCDoffset();
                break;
            case _Project.Scripts.Runtime.Player.PlayerIndexType.D:
                PlayerCDoffset();
                break;
            case _Project.Scripts.Runtime.Player.PlayerIndexType.Z:

                break;
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayerABoffset()
    {
        vVolumes[0] = 0;
        vVolumes[1] = -96;
    }

    private void PlayerCDoffset()
    {
        vVolumes[0] = -96;
        vVolumes[1] = 0;
    }

}
