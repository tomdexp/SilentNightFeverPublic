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
            PlayerPrefs.SetInt("HoldButtonToAnchorTongue", HoldButtonToAnchorTongue ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        [Command("/options.set.HoldButtonToAnchorTongue", "Set the hold button to anchor tongue")]
        public static void SetHoldButtonToAnchorTongue(bool value)
        {
            HoldButtonToAnchorTongue = value;
            Save();
            Logger.LogInfo("Options HoldButtonToAnchorTongue set to " + HoldButtonToAnchorTongue);
        }
        
        [Command("/options.list", "List all options and their values")]
        public static void ListOptions()
        {
            Logger.LogInfo("Listing all options");
            Logger.LogInfo("HoldButtonToAnchorTongue : " + HoldButtonToAnchorTongue);
            Logger.LogInfo("Options listed");
        }
    }
}