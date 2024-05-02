using System;
using _Project.Scripts.Runtime.Player;

namespace _Project.Scripts.Runtime.Networking.Rounds
{
    [Serializable]
    public struct RoundResult
    {
        public PlayerTeamType WinningTeam;
        public PlayerTeamType LosingTeam => WinningTeam == PlayerTeamType.A ? PlayerTeamType.B : PlayerTeamType.A;
        public uint SecondsElapsed;
        public byte RoundNumber;

        public override string ToString()
        {
            return $"Round {RoundNumber} - Team {WinningTeam} won the round in {SecondsElapsed} seconds";
        }
    }
}