﻿using System;

namespace _Project.Scripts.Runtime.Player
{
    /// <summary>
    /// This struct represent a Real player since our game can have multiple players on the same client
    /// We should have four of these, one for each player
    /// The PlayerManager should create all four players from these infos
    /// </summary>
    [Serializable]
    public struct RealPlayerInfo
    {
        public byte ClientId;
        public PlayerIndexType PlayerIndexType;
        public string DevicePath;

        public override string ToString()
        {
            return $"RealPlayerInfo(ClientId: {ClientId}, PlayerIndexType: {PlayerIndexType}, DevicePath: {DevicePath})";
        }
    }
}