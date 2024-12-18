﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = nameof(AudioManagerData), menuName = "Scriptable Objects/" + nameof(AudioManagerData))]
    public class AudioManagerData : ScriptableObject
    {
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        public Logger.LogLevel EventNotFoundLogLevel = Logger.LogLevel.Warning;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        public Logger.LogLevel RTPCNotFoundLogLevel = Logger.LogLevel.Warning;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Tooltip("If False, the AudioManager will not log any RTPC, errors or not")]
        public bool RPTCLog = false;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsMasterSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsMusicSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsAmbianceSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsSFXSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsLandmarksSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsUISliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsHighPassSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsLowPassSliderDefaultValue = 50;
        [TabGroup("Settings","Global", SdfIconType.GearFill, TextColor = "green")]
        [Range(0,100)] public int SettingsNotchSliderDefaultValue = 50;
        
        [TabGroup("Settings","Banks", SdfIconType.SafeFill, TextColor = "orange")]
        public BankLoadConfig[] BanksToLoadOnApplicationStart;
        
        [TabGroup("Settings","Volumes Offset", SdfIconType.SpeakerFill, TextColor = "yellow")]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        public float[] LeftSpeakerVolumeOffset = new float[2] {0, -96}; 
        [TabGroup("Settings","Volumes Offset", SdfIconType.SpeakerFill, TextColor = "yellow")]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        public float[] RightSpeakerVolumeOffset = new float[2] {-96, 0};
        
        [TabGroup("Settings","RTPC", SdfIconType.Sliders, TextColor = "lightcyan")]
        [TabGroup("Settings/RTPC/Subgroup", "GP_MUSC_SWITCH", SdfIconType.MusicNoteBeamed, TextColor = "darkcyan")]
        [Tooltip("This RTPC value will change on any new round, so that the music switch to a new track")]
        public AK.Wwise.RTPC RTPC_GP_MUSC_SWITCH;
        [TabGroup("Settings/RTPC/Subgroup", "GP_MUSC_SWITCH", SdfIconType.MusicNoteBeamed, TextColor = "darkcyan")]
        [Tooltip("Inclusive Min Value (which means a number will be generated between Min and Max, including Min and Max)")]
        public int RTPC_GP_MUSC_SWITCH_MinValue = 1;
        [TabGroup("Settings/RTPC/Subgroup", "GP_MUSC_SWITCH", SdfIconType.MusicNoteBeamed, TextColor = "darkcyan")]
        [Tooltip("Inclusive Max Value (which means a number will be generated between Min and Max, including Min and Max)")]
        public int RTPC_GP_MUSC_SWITCH_MaxValue = 4;
        
        [TabGroup("Settings","RTPC", SdfIconType.Sliders, TextColor = "lightcyan")]
        [TabGroup("Settings/RTPC/Subgroup", "GP_LM_SatelliteSpeed", SdfIconType.Speedometer, TextColor = "darkgreen")]
        public AK.Wwise.RTPC RTPC_GP_LM_SatelliteSpeed;
        [TabGroup("Settings/RTPC/Subgroup", "GP_LM_SatelliteSpeed", SdfIconType.Speedometer, TextColor = "darkgreen")]
        public float RTPC_GP_LM_SatelliteSpeed_MinValue = 0f;
        [TabGroup("Settings/RTPC/Subgroup", "GP_LM_SatelliteSpeed", SdfIconType.Speedometer, TextColor = "darkgreen")]
        public float RTPC_GP_LM_SatelliteSpeed_MaxValue = 10f;
        
        [TabGroup("References", "Events", SdfIconType.MusicNote, TextColor = "lightmagenta")]
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called at the start of the application, will only be called ONCE, when the application start, not when the game start, or a round start")]
        public AK.Wwise.Event[] EventApplicationStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        public AK.Wwise.Event[] EventMenuSceneStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventMainMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventLocalOrOnlineMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventOnlineCreateOrJoinMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        public AK.Wwise.Event[] EventOnClickedOnlineGame;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        public AK.Wwise.Event[] EventOnClickedLocalGame;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventGamepadMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventTeamSelectionMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventCharacterCustomizationStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventSettingsMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventSettingsAudioMenuStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventSettingsControlsStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("It can be called MULTIPLE times, everytime this menu opens")]
        public AK.Wwise.Event[] EventSettingsGraphicStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        public AK.Wwise.Event[] EventOnBoardingStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when the whole game session start, so that the map is generated and its the first round")]
        public AK.Wwise.Event[] EventGameStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when the game end, so when the last round end, and the game is over")]
        public AK.Wwise.Event[] EventGameEnd;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when a round start, there is a delay between when a round end, and a round start to show UI")]
        public AK.Wwise.Event[] EventRoundStart;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when a round end, so when a Team wins, will be called for the first and last round too")]
        public AK.Wwise.Event[] EventRoundEnd;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when at the start of the second fade that hides the score, before the next round start")]
        public AK.Wwise.Event[] EventRoundHideScoreFade;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when the opening animations for when the Shining star fruit team won a round")]
        public AK.Wwise.Event[] EventWinningTeamRoundTeamShiningStarFruit;
        [TabGroup("References/Events/Subgroup", "Application", SdfIconType.Bullseye, TextColor = "magenta")]
        [Tooltip("Called when the opening animations for when the Plums of passion team won a round")]
        public AK.Wwise.Event[] EventWinningTeamRoundTeamPlumsOfPassion;
        


        [TabGroup("References", "Events", SdfIconType.MusicNote, TextColor = "lightmagenta")]
        [TabGroup("References/Events/Subgroup", "Player", SdfIconType.PeopleFill, TextColor = "lightgreen")]
        public AK.Wwise.Event EventPlayerTongueThrow;
        [TabGroup("References/Events/Subgroup", "Player", SdfIconType.PeopleFill, TextColor = "lightgreen")]
        public AK.Wwise.Event EventPlayerTongueRetract;
        [TabGroup("References/Events/Subgroup", "Player", SdfIconType.PeopleFill, TextColor = "lightgreen")]
        public AK.Wwise.Event EventPlayerTongueInteractOrBind;
        [TabGroup("References/Events/Subgroup", "Player", SdfIconType.PeopleFill, TextColor = "lightgreen")]
        public AK.Wwise.Event EventPlayerSizeChange;
        [TabGroup("References/Events/Subgroup", "Player", SdfIconType.PeopleFill, TextColor = "lightgreen")]
        public AK.Wwise.Event EventPlayerFootstep;
        
        
        [TabGroup("References", "Events", SdfIconType.MusicNote, TextColor = "lightmagenta")]
        [TabGroup("References/Events/Subgroup", "UI", SdfIconType.CursorFill, TextColor = "lightblue")]
        public AK.Wwise.Event EventUIButtonHover;
        [TabGroup("References/Events/Subgroup", "UI", SdfIconType.CursorFill, TextColor = "lightblue")]
        public AK.Wwise.Event EventUIButtonClickEnter;
        [TabGroup("References/Events/Subgroup", "UI", SdfIconType.CursorFill, TextColor = "lightblue")]
        public AK.Wwise.Event EventUIButtonClickBack;
        [TabGroup("References/Events/Subgroup", "UI", SdfIconType.CursorFill, TextColor = "lightblue")]
        public AK.Wwise.Event EventUIPlayerButtonMove;
        
        [TabGroup("References", "Events", SdfIconType.MusicNote, TextColor = "lightmagenta")]
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event EventLandmarkKitchenFoodEaten;
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event EventLandmarkZoomStartTurning;
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event EventLandmarkZoomStopTurning;
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event EventOnStartZoomElectricity;
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event EventOnStopZoomElectricity;
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event OnStartControlledByVoodoo ;
        [TabGroup("References/Events/Subgroup", "Landmarks", SdfIconType.PinMapFill, TextColor = "lightorange")]
        public AK.Wwise.Event OnEndControlledByVoodoo;
        
        
        [Serializable]
        public class BankLoadConfig
        {
            [TableColumnWidth(60)]
            public AK.Wwise.Bank Bank;
            [TableColumnWidth(20)]
            [Tooltip("If true, decode this SoundBank upon load")]
            public bool DecodeBankOnLoad = false;
            [TableColumnWidth(20)]
            [Tooltip("If true, save the decoded SoundBank to disk for faster loads in the future")]
            public bool SaveDecodedBank = false;

            public void Load()
            {
                Logger.LogInfo("Loading Wwise bank: " + Bank.Name + "...", Logger.LogType.Local, this);
                //Bank.Load(DecodeBankOnLoad, SaveDecodedBank);
                //Logger.LogDebug("Wwise bank loaded: " + Bank.Name, Logger.LogType.Local, this);
                var result = AkSoundEngine.LoadAndDecodeBank(Bank.Name, false, out _);
                var resultString = Enum.GetName(typeof(AKRESULT), result);
                Logger.LogInfo("Wwise bank loaded: " + Bank.Name + " with result: " + resultString, Logger.LogType.Local, this);
            }
        }
    }
}