using System;

namespace _Project.Scripts.Runtime.Player
{
    // This should only be used for the Team managing phase, because after, the player index type and their team are predefined
    // Be we still need to know which player wants to join which team in the beginning
    [Serializable]
    public struct PlayerTeamInfo 
    {
        public PlayerIndexType PlayerIndexType; // The RealPlayer index, which means J1, J2, J3 and J4
        public PlayerTeamType PlayerTeamType;
        public PlayerIndexType ScreenPlayerIndexType; // The ScreenPlayer index, which means A, B, C and D on the team selection menu
    }
}