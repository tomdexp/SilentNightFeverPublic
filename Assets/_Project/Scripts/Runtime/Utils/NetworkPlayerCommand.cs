using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils
{
    public static class NetworkPlayerCommand
    {
        [Command("/player.set.size", "Set the size of the player.")]
        public static void SetPlayerSize(PlayerIndexType player, float size)
        {
            if(PlayerManager.HasInstance) PlayerManager.Instance.GetNetworkPlayer(player).TrySetSize(size);
        }
    }
}