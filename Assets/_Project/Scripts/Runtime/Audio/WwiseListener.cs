using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseListener : MonoBehaviour
{
    public float[] VolumesOffset = new float[2]; //Panning offset
    AkChannelConfig cfg = new AkChannelConfig();

    void Start()
    {
        //Mofifying Listener spatialisation
        cfg.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_STEREO);
        AkSoundEngine.SetListenerSpatialization(this.gameObject, true, cfg, VolumesOffset);
    }
}