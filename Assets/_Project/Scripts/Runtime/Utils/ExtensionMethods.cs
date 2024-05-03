using System;
using _Project.Scripts.Runtime.Player;

namespace _Project.Scripts.Runtime.Utils
{
    public static class ExtensionMethods
    {
        // extend the enum PlayerIndexType with a method to get the player index type as a string
        public static PlayerTeamType AsTeam(this PlayerIndexType playerIndexType)
        {
            return playerIndexType switch
            {
                PlayerIndexType.A => PlayerTeamType.A,
                PlayerIndexType.B => PlayerTeamType.B,
                PlayerIndexType.C => PlayerTeamType.A,
                PlayerIndexType.D => PlayerTeamType.B,
                PlayerIndexType.Z => PlayerTeamType.Z,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}