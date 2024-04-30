using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwPlayerPanning : MonoBehaviour
{

    private float[] vVolumes = new float[2];
    private AkChannelConfig channelConfig;

    public GameObject listenerPlayer1;
    public GameObject listenerPlayer2;


    [SerializeField] private bool isSpatialized = false;

    // Start is called before the first frame update
    void Start()
    {
        channelConfig = AkChannelConfig.Standard(2);
        AkSoundEngine.SetListenerSpatialization(listenerPlayer1, isSpatialized, channelConfig, vVolumes);
        vVolumes[0] = 0f;
        vVolumes[1] = -96f;
        AkSoundEngine.SetListenerSpatialization(listenerPlayer2, isSpatialized, channelConfig, vVolumes);
        vVolumes[0] = -96f;
        vVolumes[1] = 0f;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
