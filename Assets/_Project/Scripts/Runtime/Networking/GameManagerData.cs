using _Project.Scripts.Runtime.Networking.Rounds;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    [CreateAssetMenu(fileName = nameof(GameManagerData), menuName = "Scriptable Objects/" + nameof(GameManagerData))]
    public class GameManagerData : ScriptableObject
    {
        [Title("Game Manager Settings")]
        public float SecondsBetweenStartOfTheGameAndFirstRound = 1;
        public float SecondsBetweenRounds = 5;
        public float SecondsBetweenLastRoundCompletionAndEndOfTheGame = 5;
        
        [Title("Reference")]
        [Required] public RoundsConfig RoundsConfig;
        [Required] public RoundsConfig RoundsConfigFT1;
        [Required] public RoundsConfig RoundsConfigFT3;
        [Required] public RoundsConfig RoundsConfigFT5;
        
        public MatchConfig[] MatchConfigs;
    }
}