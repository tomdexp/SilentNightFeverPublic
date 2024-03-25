using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

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
                Debug.Log("Game already started !");
                return;
            }
            Debug.Log("Attempting to start game...");
            if (!PlayerManager.HasInstance)
            {
                Debug.LogError("No player manager instance found ! It should be spawned by the Default Spawn Objects script");
                return;
            }
            if (PlayerManager.Instance.NumberOfPlayers != 4)
            {
                Debug.Log("Not enough players to start the game ! (current : " + PlayerManager.Instance.NumberOfPlayers +"/4)");
                return;
            }
            PlayerManager.Instance.SpawnAllPlayers();
            Debug.Log("Game started !");
            IsGameStarted.Value = true;
        }
    }
}