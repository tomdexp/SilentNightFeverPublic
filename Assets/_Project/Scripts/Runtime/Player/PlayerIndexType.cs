using System;

namespace _Project.Scripts.Runtime.Player
{
    [Serializable]
    public enum PlayerIndexType
    {
        A, // upper left player
        B, // upper right player
        C, // lower left player
        D, // lower right player
        Z // Z == not set yet
    }
}