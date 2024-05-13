using System;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils;
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
    public class AudioManager : NetworkPersistentSingleton<AudioManager>
    {
        public AudioManagerData AudioManagerData;
        public event Action OnBanksLoadStart;
        public event Action OnBanksLoadComplete;

        protected override void Awake()
        {
            base.Awake();
            LoadAllBanks();
        }

        private void Start()
        {
            // Avoid playing the application start event multiple times
            if (!LocalStaticValues.HasApplicationStartWwiseEventFired)
            {
                PlayAudioLocal(AudioManagerData.EventApplicationStart, gameObject);
                LocalStaticValues.HasApplicationStartWwiseEventFired = true;
            }
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
        public void PlayAudioLocal(AK.Wwise.Event eventRef, GameObject go)
        {
            InternalPlayAudioLocal(eventRef.Id, go);
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
        
        /// <summary>
        /// This method plays an event with replication over the network, beware of calling this method too often
        /// </summary>
        public void PlayAudioNetworked(AK.Wwise.Event eventRef, GameObject go)
        {
            ReplicateAudio(eventRef.Id, go);
        }
        
        /// <summary>
        /// This method plays an event with replication over the network, beware of calling this method too often
        /// </summary>
        public void PlayAudioNetworked(uint eventId, GameObject go)
        {
            ReplicateAudio(eventId, go);
        }
        
        /// <summary>
        /// This method plays an event with replication over the network, beware of calling this method too often
        /// </summary>
        public void PlayAudioNetworked(string eventName, GameObject go)
        {
            ReplicateAudio(AkSoundEngine.GetIDFromString(eventName), go);
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
            if (eventId == 0)
            {
                switch (AudioManagerData.EventNotFoundLogLevel)
                {
                    case Logger.LogLevel.Trace:
                        Logger.LogTrace("Tried to play an audio event with ID 0, it means that the event is probably not assigned properly in AudioManagerData", Logger.LogType.Local, this);
                        break;
                    case Logger.LogLevel.Debug:
                        Logger.LogDebug("Tried to play an audio event with ID 0, it means that the event is probably not assigned properly in AudioManagerData", Logger.LogType.Local, this);
                        break;
                    case Logger.LogLevel.Info:
                        Logger.LogInfo("Tried to play an audio event with ID 0, it means that the event is probably not assigned properly in AudioManagerData", Logger.LogType.Local, this);
                        break;
                    case Logger.LogLevel.Warning:
                        Logger.LogWarning("Tried to play an audio event with ID 0, it means that the event is probably not assigned properly in AudioManagerData", Logger.LogType.Local, this);
                        break;
                    case Logger.LogLevel.Error:
                        Logger.LogError("Tried to play an audio event with ID 0, it means that the event is probably not assigned properly in AudioManagerData", Logger.LogType.Local, this);
                        break;
                }
                return;
            }
            Logger.LogTrace("Playing audio event locally : " + eventId, Logger.LogType.Local, this);
            AkSoundEngine.PostEvent(eventId, go);
        }
    }
}