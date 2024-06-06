using System;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    [Serializable]
    public struct PlayerHatInfo 
    {
        public PlayerIndexType PlayerIndexType;
        public HatType PlayerHatType;
        public bool HasConfirmed;
    }
}

public enum HatType
{
    None = 0,
    Plant2 = 1,
    Plant4 = 2,
    Pan = 3,
    Satellite = 4,
    Puppet = 5
}