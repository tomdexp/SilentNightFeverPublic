using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings.BoolSettings
{
    public class BoolSettingHoldButtonToAnchorTongue : BoolSetting
    {
        public BoolSettingHoldButtonToAnchorTongue(bool defaultValue) : base(defaultValue)
        {
        }
        
        [Command("/options.get." + nameof(BoolSettingHoldButtonToAnchorTongue), MonoTargetType.Registry)]
        public override void CommandGet()
        {
            base.CommandGet();
        }

        [Command("/options.set." + nameof(BoolSettingHoldButtonToAnchorTongue), MonoTargetType.Registry)]
        public override void Set(bool value)
        {
            base.Set(value);
        }
    }
}