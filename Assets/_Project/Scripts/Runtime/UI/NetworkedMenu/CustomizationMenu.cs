namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class CustomizationMenu : MenuBase
    {
        public override string MenuName { get; } = "CustomizationMenu";

        public override void Open()
        {
            base.Open();
            UIManager.Instance.SwitchToMetroCamera();
        }

        public override void GoBack()
        {
            base.GoBack();
            UIManager.Instance.GoToMenu<PlayerIndexSelectionMenu>();
        }
    }
}