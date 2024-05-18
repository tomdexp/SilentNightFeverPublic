using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Runtime.Landmarks.Kitchen
{
    public class Landmark_Kitchen : Landmark
    {
        public new LandmarkData_Kitchen Data
        {
            get => (LandmarkData_Kitchen)base.Data;
            set => base.Data = value;
        }
        
        [Title("Landmark Kitchen Reference")]
        [SerializeField] private Transform[] _foodSpawnPoints;
        
        private List<NetworkObject> _spawnedFoods = new List<NetworkObject>();

        public override void OnStartServer()
        {
            base.OnStartServer();
            VerifySetup();
        }

        private void VerifySetup()
        {
            if (_foodSpawnPoints.Length == 0)
            {
                Logger.LogError("No food spawn points assigned to Landmark " + nameof(Landmark_Kitchen), Logger.LogType.Server, this);
            }
            if (Data.FoodsToSpawn.Length == 0)
            {
                Logger.LogError("No foods to spawn assigned to Landmark " + nameof(Landmark_Kitchen), Logger.LogType.Server, this);
            }
        }
        
        

        private void SpawnFoods()
        {
            Logger.LogDebug("Spawning foods for Landmark " + nameof(Landmark_Kitchen), Logger.LogType.Server, this);
            foreach (var spawnPoint in _foodSpawnPoints)
            {
                var foodToSpawn = Data.FoodsToSpawn[Random.Range(0, Data.FoodsToSpawn.Length)];
                var nob = Instantiate(foodToSpawn, spawnPoint.position, spawnPoint.rotation);
                _spawnedFoods.Add(nob);
                ServerManager.Spawn(nob);
                Logger.LogDebug($"Spawned {nob.name} at {spawnPoint.position}", Logger.LogType.Server, this);
            }
        }

        private void OnDrawGizmos()
        {
            if (_foodSpawnPoints.Length == 0) return;
            foreach (var spawnPoint in _foodSpawnPoints)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            }
        }

        protected override void ResetLandmark(byte _)
        {
            Logger.LogDebug("Resetting Landmark " + nameof(Landmark_Kitchen), Logger.LogType.Server, this);
            if (_spawnedFoods.Count != 0)
            {
                foreach (var spawnedFood in _spawnedFoods)
                {
                    ServerManager.Despawn(spawnedFood);
                }
            }
            
            SpawnFoods();
        }
    }
}