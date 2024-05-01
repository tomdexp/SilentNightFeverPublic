using System;
using _Project.Scripts.Runtime.Utils;

namespace _Project.Scripts.Runtime.Networking.Rounds
{
    [Serializable]
    public class Round
    {
        public bool IsRoundActive { get; private set; }
        public byte RoundNumber { get; private set; }
        
        public void StartRound()
        {
            Logger.LogInfo($"Round {RoundNumber} started !");
            IsRoundActive = true;
        }
        
        public void EndRound()
        {
            IsRoundActive = false;
        }
        
        public void SetRoundNumber(byte roundNumber)
        {
            RoundNumber = roundNumber;
        }
    }
}