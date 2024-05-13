using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = nameof(AudioManagerData), menuName = "Scriptable Objects/" + nameof(AudioManagerData))]
    public class AudioManagerData : ScriptableObject
    {
        [Title("Global Settings")]
        public Logger.LogLevel EventNotFoundLogLevel = Logger.LogLevel.Warning;
        
        [Title("Banks Settings")]
        public BankLoadConfig[] BanksToLoadOnApplicationStart;
        
        [Title("Players Panning Settings")]
        public float[] LeftSpeakerVolumeOffset = new float[2] {0, -96}; 
        public float[] RightSpeakerVolumeOffset = new float[2] {-96, 0};
        
        [Title("AkEvent References", "Global Events")]
        [Tooltip("Called at the start of the application, will only be called ONCE, when the application start, not when the game start, or a round start")]
        public AK.Wwise.Event EventApplicationStart;
        [Tooltip("Called when the Main Menu scene starts, it can be called MULTIPLE times, when the scene is loaded again, or when the game is restarted")]
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventMainMenuStart;
        [Tooltip("Called when the Character Selection menu pops up")]
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventCharacterSelectionStart;
        [Tooltip("Called when the whole game session start, so that the map is generated and its the first round")]
        public AK.Wwise.Event EventGameStart;
        [Tooltip("Called when the game end, so when the last round end, and the game is over")]
        public AK.Wwise.Event EventGameEnd;
        [Tooltip("Called when a round start, there is a delay between when a round end, and a round start to show UI")]
        public AK.Wwise.Event EventRoundStart;
        [Tooltip("Called when a round end, so when a Team wins, will be called for the first and last round too")]
        public AK.Wwise.Event EventRoundEnd;

        [Title("AkEvent References", "Player Events")]
        public AK.Wwise.Event EventPlayerTongueThrow;
        public AK.Wwise.Event EventPlayerTongueRetract;
        
        [Title("AkEvent References", "UI Events")]
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventUIButtonHover;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventUIButtonClick;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventUIOnlineLobbyCreated;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventUIOnlineLobbyPlayerJoin;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventUIPlayerJoinWithController;
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventUIPlayerLeftWithController;
        
        [Title("AkEvent References", "Landmark Events")]
        [InfoBox("Not Implemented Yet", InfoMessageType.Error)]
        public AK.Wwise.Event EventLandmarkKitchenFoodEaten;
        
        
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