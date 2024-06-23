using _Project.Scripts.Runtime.Networking;
using FishNet;
using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils
{
    public static class PlayerManagerCommands
    {
        [Command("/player.set.joining", "Enable or disable the player joining")]
        public static void SetPlayerJoining(bool value)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Logger.LogError("Only the server can set this value !");
                return;
            }
            if(PlayerManager.HasInstance) PlayerManager.Instance.SetPlayerJoiningEnabledClientRpc(value);
        }
        
        [Command("/player.set.leaving", "Enable or disable the player leaving")]
        public static void SetPlayerLeaving(bool value)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Logger.LogError("Only the server can set this value !");
                return;
            }
            if(PlayerManager.HasInstance) PlayerManager.Instance.SetPlayerLeavingEnabledClientRpc(value);
        }
        
        [Command("/player.fake.all.ready", "Make all fake players ready")]
        public static void SetFakePlayersReady()
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Logger.LogError("Only the server can set this value !");
                return;
            }
            if(PlayerManager.HasInstance) PlayerManager.Instance.ReadyAllFakePlayers();
        }

        [Command("/player.infos", "Get the player infos")]
        public static void GetPlayerPositions()
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Logger.LogError("Only the server can know these informations !");
                return;
            }
            if(PlayerManager.HasInstance) PlayerManager.Instance.LogPlayerPositionsAndDistance();
        }
    }
}