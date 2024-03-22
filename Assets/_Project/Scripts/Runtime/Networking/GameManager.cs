using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    public class GameManager : NetworkPersistentSingleton<GameManager>
    {
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
            Debug.Log("Starting game...");
        }
    }
}