using System;
using System.Collections.Generic;
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
    [RequireComponent(typeof(AkGameObj))]
    public class AudioManager : NetworkPersistentSingleton<AudioManager>
    {
        public AudioManagerData AudioManagerData;
        public event Action OnBanksLoadStart;
        public event Action OnBanksLoadComplete;
        
        private AkGameObj _akGameObj;
        
        private List<AkAudioListener> _listeners = new List<AkAudioListener>();
        private List<AkGameObj> _emitters = new List<AkGameObj>();

        private void Start()
        {
            // Avoid playing the application start event multiple times or loading the banks multiple times
            if (!LocalStaticValues.HasApplicationStartWwiseEventFired)
            {
                LoadAllBanks();
                PlayAudioLocal(AudioManagerData.EventApplicationStart, gameObject);
                LocalStaticValues.HasApplicationStartWwiseEventFired = true;
            }
            _akGameObj = GetComponent<AkGameObj>();
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

        public void PlayAudioNetworked(IEnumerable<AK.Wwise.Event> eventsRef, GameObject go)
        {
            foreach (var audioEvent in eventsRef)
            {
                ReplicateAudio(audioEvent.Id, go);
            }
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
        
        public void SetLocalRTPC(string rtpcName, float value, GameObject go)
        {
            InternalSetRTPC(AkSoundEngine.GetIDFromString(rtpcName), value, go);
        }
        
        public void SetLocalRTPC(uint rtpcId, float value, GameObject go)
        {
            InternalSetRTPC(rtpcId, value, go);
        }
        
        public void SetLocalRTPC(AK.Wwise.RTPC rtpc, float value, GameObject go)
        {
            InternalSetRTPC(rtpc.Id, value, go);
        }

        public void SetLocalRTPC(AK.Wwise.RTPC rtpc, float value)
        {
            InternalSetRPCGlobal(rtpc.Id, value);
        }
        
        /// <summary>
        /// This method sets an RTPC value without replication over the network, beware of calling this method too often
        /// </summary>
        public void SetNetworkedRTPC(uint rtpcId, float value, GameObject go)
        {
            ReplicateRTPC(rtpcId, value, go);
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void ReplicateRTPC(uint rtpcId, float value, GameObject go, NetworkConnection conn = null)
        {
            Logger.LogTrace("Setting RTPC over the network with ID: " + rtpcId + " to " + value, Logger.LogType.Local, this);
            InternalSetRTPC(rtpcId, value, go);
            SetRTPCForClients(rtpcId, value, go, conn);
        }
        
        [ObserversRpc(ExcludeServer = true)]
        private void SetRTPCForClients(uint rtpcId, float value, GameObject go, NetworkConnection conn = null)
        {
            if (conn != null && conn == InstanceFinder.ClientManager.Connection)
            {
                return;
            }
            InternalSetRTPC(rtpcId, value, go);
        }

        private void InternalSetRTPC(uint rtpcId, float value, GameObject go)
        {
            if (rtpcId == 0)
            {
                if(AudioManagerData.RPTCLog) Logger.Log(AudioManagerData.RTPCNotFoundLogLevel, Logger.LogType.Local,
                    $"Tried to set an RTPC with ID {rtpcId} but was not found, it means that the RTPC is probably not assigned properly in AudioManagerData", this);
            }
            if(AudioManagerData.RPTCLog) Logger.LogTrace("Setting RTPC locally (ID: " + rtpcId + ") to " + value, Logger.LogType.Local, this);
            AkSoundEngine.SetRTPCValue(rtpcId, value, go);
        }
        
        private void InternalSetRPCGlobal(uint rtpcId, float value)
        {
            if (rtpcId == 0)
            {
                if(AudioManagerData.RPTCLog) Logger.Log(AudioManagerData.RTPCNotFoundLogLevel, Logger.LogType.Local,
                    $"Tried to set an RTPC with ID {rtpcId} but was not found, it means that the RTPC is probably not assigned properly in AudioManagerData", this);
            }
            if(AudioManagerData.RPTCLog) Logger.LogTrace("Setting Global RTPC locally (ID: " + rtpcId + ") to " + value, Logger.LogType.Local, this);
            AkSoundEngine.SetRTPCValue(rtpcId, value);
        }

        public void RegisterListener(AkAudioListener listener)
        {
            Logger.LogTrace($"Registering listener {listener.name} to AudioManager...", Logger.LogType.Local, this);
            _listeners.Add(listener);
            BindListenersAndEmitters();
        }
        
        public void RegisterEmitter(AkGameObj emitter)
        {
            Logger.LogTrace($"Registering emitter ({emitter.name}) to AudioManager...", Logger.LogType.Local, this);
            _emitters.Add(emitter);
            BindListenersAndEmitters();
        }
        
        private void BindListenersAndEmitters()
        {
            Logger.LogTrace("Binding listeners and emitters...", Logger.LogType.Local, this);

            // Clean all null references (when changing scene from onboarding -> game, players are destroyed then re-created)
            // but the listeners and emitters are not cleaned manually
            _listeners.RemoveAll(item => !item);
            _emitters.RemoveAll(item => !item);
            
            foreach (var akGameObj in _emitters)
            {
                foreach (var akAudioListener in _listeners)
                {
                    akAudioListener.StartListeningToEmitter(akGameObj);
                    Logger.LogTrace($"Binding listener {akAudioListener.name} to emitter {akGameObj.name}", Logger.LogType.Local, this);
                }
            }
        }
    }
}