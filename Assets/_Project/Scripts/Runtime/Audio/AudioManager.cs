using System;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils.Singletons;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Audio
{
    /// <summary>
    /// This is the Networked AudioManager.
    /// It is responsible for initializing the Wwise audio system and playing event across the network.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AkInitializer))]
    [RequireComponent(typeof(AkGameObj))]
    public class AudioManager : NetworkPersistentSingleton<GameManager>
    {
        public AudioManagerData AudioManagerData;
        public event Action OnBanksLoadStart;
        public event Action OnBanksLoadComplete;

        protected override void Awake()
        {
            base.Awake();
            LoadAllBanks();
        }
        
        private void LoadAllBanks()
        {
            OnBanksLoadStart?.Invoke();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Logger.LogDebug("Loading all Wwise banks...", Logger.LogType.Local, this);
            foreach (var bank in AudioManagerData.BanksToLoadOnApplicationStart)
            {
                bank.Load();
            }
            stopwatch.Stop();
            OnBanksLoadComplete?.Invoke();
            Logger.LogDebug("All Wwise banks loaded ! Took " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Local, this);
        }
        
        /// <summary>
        /// This method plays an event without replication over the network
        /// </summary>
        public void PlayAudioLocal(string eventName, GameObject go)
        {
            InternalPlayAudioLocal(AkSoundEngine.GetIDFromString(eventName), go);
        }
        
        /// <summary>
        /// This method plays an event without replication over the network
        /// </summary>
        public void PlayAudioLocal(uint eventId, GameObject go)
        {
            InternalPlayAudioLocal(eventId, go);
        }
        
        public void PlayAudioNetworked(uint eventId, GameObject go)
        {
            ReplicateAudio(eventId, go);
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void ReplicateAudio(uint eventId, GameObject go, NetworkConnection conn = null)
        {
            Logger.LogTrace("Playing audio event over the network : " + eventId, Logger.LogType.Local, this);
            InternalPlayAudioLocal(eventId, go);
            PlayAudioForClients(eventId, go, conn);
        }
        
        [ObserversRpc(ExcludeServer = true)]
        private void PlayAudioForClients(uint eventId, GameObject go, NetworkConnection conn = null)
        {
            if (conn != null && conn == InstanceFinder.ClientManager.Connection)
            {
                return;
            }
            InternalPlayAudioLocal(eventId, go);
        }
        
        /// <summary>
        /// Wrapper for playing audio events locally
        /// </summary>
        private void InternalPlayAudioLocal(uint eventId, GameObject go)
        {
            Logger.LogTrace("Playing audio event locally : " + eventId, Logger.LogType.Local, this);
            AkSoundEngine.PostEvent(eventId, go);
        }
    }
}