using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class GameManager : NetworkPersistentSingleton<GameManager>
    {
        public readonly SyncVar<bool> IsGameStarted = new SyncVar<bool>();
        
        // Entry point
        public void TryStartGame()
        {
            if (!IsServerStarted)
            {
                StartGameServerRpc();
            }
            else
            {
                StartGame();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc()
        {
            StartGame();
        }
        
        private void StartGame()
        {
            if (IsGameStarted.Value)
            {
                Logger.LogDebug("Game already started !", Logger.LogType.Server, NetworkObject);
                return;
            }
            Logger.LogTrace("Attempting to start game...", Logger.LogType.Server, NetworkObject);
            if (!PlayerManager.HasInstance)
            {
                Logger.LogError("No player manager instance found ! It should be spawned by the Default Spawn Objects script", Logger.LogType.Server, NetworkObject);
                return;
            }
            if (PlayerManager.Instance.NumberOfPlayers != 4)
            {
                Logger.LogWarning("Not enough players to start the game ! (current : " + PlayerManager.Instance.NumberOfPlayers +"/4)", Logger.LogType.Server, NetworkObject);
                return;
            }
            PlayerManager.Instance.SpawnAllPlayers();
            Logger.LogInfo("Game started !", Logger.LogType.Server, NetworkObject);
            IsGameStarted.Value = true;
        }
    }
}