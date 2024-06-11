using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings.BoolSettings
{
    public class BoolSettingEpilepsyFilterEnable : BoolSetting
    {
        public BoolSettingEpilepsyFilterEnable(bool defaultValue) : base(defaultValue)
        {
        }
        
        [Command("/options.get." + nameof(BoolSettingEpilepsyFilterEnable), MonoTargetType.Registry)]
        public override void CommandGet()
        {
            base.CommandGet();
        }

        [Command("/options.set." + nameof(BoolSettingEpilepsyFilterEnable), MonoTargetType.Registry)]
        public override void Set(bool value)
        {
            base.Set(value);
        }
    }
}