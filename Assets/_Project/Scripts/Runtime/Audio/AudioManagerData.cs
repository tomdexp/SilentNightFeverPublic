using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = nameof(AudioManagerData), menuName = "Scriptable Objects/" + nameof(AudioManagerData))]
    public class AudioManagerData : ScriptableObject
    {
        [Title("Banks Settings")]
        public BankLoadConfig[] BanksToLoadOnApplicationStart;
        
        [Serializable]
        public class BankLoadConfig
        {
            public AK.Wwise.Bank Bank;
            [Tooltip("If true, decode this SoundBank upon load")]
            public bool DecodeBankOnLoad = true;
            [Tooltip("If true, save the decoded SoundBank to disk for faster loads in the future")]
            public bool SaveDecodedBank = true;

            public void Load()
            {
                Logger.LogDebug("Loading Wwise bank: " + Bank.Name + "...", Logger.LogType.Local, this);
                Bank.Load(DecodeBankOnLoad, SaveDecodedBank);
                Logger.LogDebug("Wwise bank loaded: " + Bank.Name, Logger.LogType.Local, this);
            }
        }
    }
}