using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking.Rounds
{
    [CreateAssetMenu(fileName = nameof(RoundsConfig), menuName = "Scriptable Objects/" + nameof(RoundsConfig))]
    public class RoundsConfig : ScriptableObject
    {
        public byte RoundsCount = 3;
        public RoundsWinType RoundsWinType = RoundsWinType.BestOfX;

        [ShowIf("RoundsWinType", RoundsWinType.FirstToX), ReadOnly, SerializeField]
        [InfoBox("This value is calculated based on the RoundsCount and RoundsWinType")]
        private int _maxRounds;

        private void OnValidate()
        {
            switch (RoundsWinType)
            {
                case RoundsWinType.BestOfX:
                    _maxRounds = RoundsCount;
                    break;
                case RoundsWinType.FirstToX:
                    _maxRounds = RoundsCount*2 - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}