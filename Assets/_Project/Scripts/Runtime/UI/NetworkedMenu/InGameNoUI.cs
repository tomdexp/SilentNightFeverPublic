using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class InGameNoUI : MenuBase
    {
        public override string MenuName { get; } = "InGameNoUI";
        
        public override void Open()
        {
            base.Open();
        }

        public override void Close()
        {
            base.Close();
        }

        public override void GoBack()
        {
            base.GoBack();
        }
    }
}