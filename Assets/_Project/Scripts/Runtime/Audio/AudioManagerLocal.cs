using System;
using _Project.Scripts.Runtime.Utils.Singletons;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    public class AudioManagerLocal : PersistentSingleton<AudioManagerLocal>
    {
        // marker class for the AudioManager to play global events on the same gameobject even between network resets
    }
}