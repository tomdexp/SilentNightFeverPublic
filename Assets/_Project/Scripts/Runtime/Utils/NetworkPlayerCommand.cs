using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using QFSW.QC;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public static class NetworkPlayerCommand
    {
        [Command("/player.set.size", "Set the size of the player.")]
        public static void SetPlayerSize(PlayerIndexType player, float size)
        {
            if(PlayerManager.HasInstance) PlayerManager.Instance.GetNetworkPlayer(player).TrySetSize(size);
        }

        [Command("/player.teleport", "Teleport the player to the specified position.")]
        public static void TeleportPlayer(PlayerIndexType player, Vector3 position)
        {
            if (PlayerManager.HasInstance) PlayerManager.Instance.GetNetworkPlayer(player).GetPlayerController().Teleport(position);
        }
        
        [Command("/player.teleport.toPlayer", "Teleport the player to the specified player.")]
        public static void TeleportPlayer(PlayerIndexType sourcePlayer, PlayerIndexType targetPlayer)
        {
            var targetPlayerPosition = PlayerManager.Instance.GetNetworkPlayer(targetPlayer).gameObject.transform.position;
            if (PlayerManager.HasInstance) PlayerManager.Instance.GetNetworkPlayer(sourcePlayer).GetPlayerController().Teleport(targetPlayerPosition);
        }
    }
}