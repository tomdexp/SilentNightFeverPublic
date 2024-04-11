using QFSW.QC;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public static class GameOptions
    {
        public static bool HoldButtonToAnchorTongue = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            HoldButtonToAnchorTongue = PlayerPrefs.GetInt("HoldButtonToAnchorTongue", 0) == 1;
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(nameof(HoldButtonToAnchorTongue), HoldButtonToAnchorTongue ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        [Command("/options.set." + nameof(HoldButtonToAnchorTongue), "Set the hold button to anchor tongue")]
        public static void SetHoldButtonToAnchorTongue(bool value)
        {
            HoldButtonToAnchorTongue = value;
            Save();
            Logger.LogInfo($"Options {nameof(HoldButtonToAnchorTongue)} set to " + HoldButtonToAnchorTongue);
        }
        
        [Command("/options.get." + nameof(HoldButtonToAnchorTongue))]
        public static void GetHoldButtonToAnchorTongue()
        {
            Logger.LogInfo($"Options {nameof(HoldButtonToAnchorTongue)} is " + HoldButtonToAnchorTongue);
        }
        
        [Command("/options.list", "List all options and their values")]
        public static void ListOptions()
        {
            Logger.LogInfo("Listing all options");
            Logger.LogInfo($"{nameof(HoldButtonToAnchorTongue)} : " + HoldButtonToAnchorTongue);
            Logger.LogInfo("Options listed");
        }
        
        [Command("/options.reset.all", "Reset all options to default")]
        public static void ResetToDefault()
        {
            HoldButtonToAnchorTongue = false;
            Save();
            Logger.LogInfo("Options reset to default");
        }
    }
}