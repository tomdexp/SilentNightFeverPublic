using _Project.Scripts.Runtime.Landmarks.Zoom;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using QFSW.QC;
using QFSW.QC.Suggestors.Tags;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public static class NetworkPlayerCommand
    {
        [Command("/player.set.size", "Set the size of the player.")]
        public static void SetPlayerSize(PlayerIndexType player, float size)
        {
            if(PlayerManager.HasInstance) PlayerManager.Instance.GetNetworkPlayer(player).TrySetSize(size, true);
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

        [Command("/player.teleport.toLandmark", "Teleport the player to the specified landmark.")]
        public static void TeleportPlayerToZoomLandmark(PlayerIndexType player,
            [Suggestions("zoom", "kitchen", "voodoo")] string landmarkName)
        {
            if (PlayerManager.HasInstance)
                PlayerManager.Instance.GetNetworkPlayer(player).GetPlayerController().Teleport(landmarkName);
        }
    }
}