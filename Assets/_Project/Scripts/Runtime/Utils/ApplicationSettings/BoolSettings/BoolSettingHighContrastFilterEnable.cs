using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings.BoolSettings
{
    public class BoolSettingHighContrastFilterEnable : BoolSetting
    {
        public BoolSettingHighContrastFilterEnable(bool defaultValue) : base(defaultValue)
        {
        }
        
        [Command("/options.get." + nameof(BoolSettingHighContrastFilterEnable), MonoTargetType.Registry)]
        public override void CommandGet()
        {
            base.CommandGet();
        }

        [Command("/options.set." + nameof(BoolSettingHighContrastFilterEnable), MonoTargetType.Registry)]
        public override void Set(bool value)
        {
            base.Set(value);
        }
    }
}