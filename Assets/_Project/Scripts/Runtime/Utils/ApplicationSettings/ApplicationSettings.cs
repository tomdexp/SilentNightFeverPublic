using _Project.Scripts.Runtime.Utils.ApplicationSettings.BoolSettings;
using QFSW.QC;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings
{
    // All options specific to an instance of the game, those are not MatchOptions
    public static class ApplicationSettings
    {
        public static BoolSettingHoldButtonToAnchorTongue HoldButtonToAnchorTongue = new BoolSettingHoldButtonToAnchorTongue(false);
        public static BoolSettingUseRadialTongueSensor UseRadialTongueSensor = new BoolSettingUseRadialTongueSensor(false);
        public static BoolSettingEpilepsyFilterEnable EpilepsyFilterEnable = new BoolSettingEpilepsyFilterEnable(false);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            HoldButtonToAnchorTongue.Load();
            UseRadialTongueSensor.Load();
            EpilepsyFilterEnable.Load();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void RegistrySetup()
        {
            QuantumRegistry.RegisterObject(UseRadialTongueSensor);
            Logger.LogInfo("QuantumRegistry updated");
        }

        public static void Save()
        {
            HoldButtonToAnchorTongue.Save();
            UseRadialTongueSensor.Save();
            EpilepsyFilterEnable.Save();
            PlayerPrefs.Save();
        }
        
        public enum Language
        {
            English,
            French
        }
        
        [Command("/options.set.localization", "Set the localization")]
        public static void SetLocalizations(Language language)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int)language];
        }
        
        [Command("/options.list", "List all options and their values")]
        public static void ListOptions()
        {
            Logger.LogInfo("Listing all options");
            Logger.LogInfo("Localization : " + LocalizationSettings.SelectedLocale.LocaleName);
            HoldButtonToAnchorTongue.CommandGet();
            UseRadialTongueSensor.CommandGet();
            EpilepsyFilterEnable.CommandGet();
            Logger.LogInfo("Options listed");
            
        }
            
        [Command("/options.reset.all", "Reset all options to default")]
        public static void ResetToDefault()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int)Language.English];
            HoldButtonToAnchorTongue.ResetToDefault();
            UseRadialTongueSensor.ResetToDefault();
            EpilepsyFilterEnable.ResetToDefault();
            Save();
            Logger.LogInfo("Options reset to default");
        }
    }
}