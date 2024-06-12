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
    Cap01 = 1,
    Cap02 = 2,
    Glasses = 3,
    Plant2 = 4,
    Catgirl = 5,
}