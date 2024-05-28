using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings.BoolSettings
{
    public class BoolSettingUseRadialTongueSensor : BoolSetting
    {
        public BoolSettingUseRadialTongueSensor(bool defaultValue) : base(defaultValue)
        {
        }

        [Command("/options.get." + nameof(BoolSettingUseRadialTongueSensor), MonoTargetType.Registry)]
        public override void CommandGet()
        {
            base.CommandGet();
        }

        [Command("/options.set." + nameof(BoolSettingUseRadialTongueSensor), MonoTargetType.Registry)]
        public override void Set(bool value)
        {
            base.Set(value);
        }
    }
}