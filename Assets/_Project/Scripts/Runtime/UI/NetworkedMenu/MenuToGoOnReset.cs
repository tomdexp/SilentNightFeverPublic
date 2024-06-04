using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    /// <summary>
    /// This is a special class that is local but act as a binding between the networked menu and the local menu.
    /// When the network is reset, by hosting a new game or joining a new game, this class will be used to determine which menu to go to.
    /// </summary>
    public class MenuToGoOnReset : MonoBehaviour
    {
        public string MenuName { get; private set; }
        
        public void SetMenuName(string menuName)
        {
            MenuName = menuName;
        }
    }
}