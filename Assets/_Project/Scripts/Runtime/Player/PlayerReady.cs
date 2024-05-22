using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace _Project.Scripts.Runtime.Player
{
    [Serializable]
    public struct PlayerReadyInfo
    {
        public PlayerIndexType PlayerIndexType;
        public bool IsPlayerReady;
    }
}