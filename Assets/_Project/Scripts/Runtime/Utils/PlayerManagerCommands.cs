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
    }
}