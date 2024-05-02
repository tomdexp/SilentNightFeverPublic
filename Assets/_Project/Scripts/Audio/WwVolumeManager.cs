using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwVolumeManager : MonoBehaviour
{

    [SerializeField] [Range(0, 100)] private int VolumeMusic;

    // Start is called before the first frame update
    void Start()
    {
        AkSoundEngine.RegisterGameObj(gameObject);
        VolumeMusic = 80;
    }

    // Update is called once per frame
    void Update()
    {
        AkSoundEngine.SetRTPCValue("GP_PARAM_MUSC_Volume", VolumeMusic);
    }
}

