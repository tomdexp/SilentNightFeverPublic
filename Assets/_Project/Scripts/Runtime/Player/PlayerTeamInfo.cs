using System;

namespace _Project.Scripts.Runtime.Player
{
    // This should only be used for the Team managing phase, because after, the player index type and their team are predefined
    // Be we still need to know which player wants to join which team in the beginning
    [Serializable]
    public struct PlayerTeamInfo 
    {
        public PlayerIndexType PlayerIndexType;
        public PlayerTeamType PlayerTeamType;
    }
}