using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    private AkChannelConfig channelConfig = new AkChannelConfig();
    private float[] vVolumes = new float[2];

    // Start is called before the first frame update
    void Awake()
    {
        channelConfig = AkChannelConfig.Standard(AkSoundEngine.AK_SPEAKER_SETUP_STEREO);
        AkSoundEngine.SetListenerSpatialization(this.gameObject, true, channelConfig, vVolumes);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
