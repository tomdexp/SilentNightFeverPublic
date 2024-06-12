using System;
using _Project.Scripts.Runtime.Utils;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    public class DespawnInOtherScene : NetworkBehaviour
    {
        [SerializeField] private SceneType SceneThatBelongsTo;
        private bool _isDespawning;
        private void Update()
        {
            if (!IsServerStarted) return;
            if (Time.frameCount % 30 != 0) return;
            // if the scene current name is not the same as the scene that the object belongs to
            if (SceneThatBelongsTo.ToString() != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                if (!_isDespawning)
                {
                    _isDespawning = true;
                    Despawn();
                }
            }
            else
            {
                _isDespawning = false;
            }
        }
    }
}