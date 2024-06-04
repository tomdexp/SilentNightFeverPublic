using UnityEngine;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings.BoolSettings
{
    public class BoolSetting : ApplicationSetting<bool>
    {
        public BoolSetting(bool defaultValue) : base(defaultValue) { }
        
        public override void Set(bool value)
        {
            base.Set(value);
            Value = value;
            Save();
        }
        
        public override void Load()
        {
            Value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
            Logger.LogInfo($"Loaded GameOption<bool> : {key} as {Value}", Logger.LogType.Local, this);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt(key, Value ? 1 : 0);
            PlayerPrefs.Save();
            Logger.LogInfo($"Saved GameOption<bool> : {key} as {Value}", Logger.LogType.Local, this);
        }
        
    }
}