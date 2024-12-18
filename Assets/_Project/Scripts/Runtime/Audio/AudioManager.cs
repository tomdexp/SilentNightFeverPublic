﻿using System;
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
        private bool _banksLoaded;
        private GameObject _localPlayer;

        private void Start()
        {
            // Avoid playing the application start event multiple times or loading the banks multiple times
            CleanLocalPlayer();
            LoadAllBanks();
            if (!LocalStaticValues.HasApplicationStartWwiseEventFired)
            {
                PlayAudioLocal(AudioManagerData.EventApplicationStart);
                LocalStaticValues.HasApplicationStartWwiseEventFired = true;
            }
            _akGameObj = GetComponent<AkGameObj>();
        }

        private void OnApplicationQuit()
        {
            if (Instance == this)
            {
                Logger.LogInfo("Destroying AudioManager and terminating AkSoundEngine...", Logger.LogType.Local, this);
                AkSoundEngine.Term();
            }
        }

        private void LoadAllBanks()
        {
            if (_banksLoaded)
            {
                Logger.LogDebug("Audio Banks already loaded, skipping...", Logger.LogType.Local, this);
                return;
            }
            OnBanksLoadStart?.Invoke();
            _banksLoaded = false;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Logger.LogDebug("Loading all Wwise banks...", Logger.LogType.Local, this);
            foreach (var bank in AudioManagerData.BanksToLoadOnApplicationStart)
            {
                bank.Load();
            }
            stopwatch.Stop();
            OnBanksLoadComplete?.Invoke();
            _banksLoaded = true;
            Logger.LogDebug("All Wwise banks loaded ! Took " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Local, this);
        }
        
        /// <summary>
        /// This method plays an event without replication over the network
        /// </summary>
        public void PlayAudioLocal(AK.Wwise.Event eventRef, GameObject go)
        {
            InternalPlayAudioLocal(eventRef.Id, go);
        }
        
        public void PlayAudioLocal(IEnumerable<AK.Wwise.Event> eventsRef, GameObject go)
        {
            foreach (var audioEvent in eventsRef)
            {
                InternalPlayAudioLocal(audioEvent.Id, go);
            }
        }
        
        public void PlayAudioLocal(AK.Wwise.Event eventRef)
        {
            InternalPlayAudioLocal(eventRef.Id, GetLocalPlayer());
        }
        
        public void PlayAudioLocal(IEnumerable<AK.Wwise.Event> eventsRef)
        {
            foreach (var audioEvent in eventsRef)
            {
                InternalPlayAudioLocal(audioEvent.Id, GetLocalPlayer());
            }
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
        
        public void PlayAudioNetworked(AK.Wwise.Event eventRef)
        {
            ReplicateAudio(eventRef.Id);
        }

        public void PlayAudioNetworked(IEnumerable<AK.Wwise.Event> eventsRef)
        {
            foreach (var audioEvent in eventsRef)
            {
                ReplicateAudio(audioEvent.Id);
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
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void ReplicateAudio(uint eventId, NetworkConnection conn = null)
        {
            Logger.LogTrace("Playing audio event over the network : " + eventId, Logger.LogType.Local, this);
            InternalPlayAudioLocal(eventId, GetLocalPlayer());
            PlayAudioForClients(eventId, conn);
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
        
        [ObserversRpc(ExcludeServer = true)]
        private void PlayAudioForClients(uint eventId, NetworkConnection conn = null)
        {
            if (conn != null && conn == InstanceFinder.ClientManager.Connection)
            {
                return;
            }
            InternalPlayAudioLocal(eventId, GetLocalPlayer());
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
            
            if (!_banksLoaded)
            {
                Logger.LogWarning($"Tried to play and audio event with id {eventId}, but the banks are not loaded yet", Logger.LogType.Local, this);
                return;
            }
            
            var result = AkSoundEngine.PostEvent(eventId, go);
            Logger.LogTrace("Played audio event locally : " + eventId, Logger.LogType.Local, this);
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
        
        public void SetNetworkedRTPC(uint rtpcId, float value)
        {
            ReplicateRTPC(rtpcId, value);
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void ReplicateRTPC(uint rtpcId, float value, GameObject go, NetworkConnection conn = null)
        {
            Logger.LogTrace("Setting RTPC over the network with ID: " + rtpcId + " to " + value, Logger.LogType.Local, this);
            InternalSetRTPC(rtpcId, value, go);
            SetRTPCForClients(rtpcId, value, go, conn);
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        private void ReplicateRTPC(uint rtpcId, float value, NetworkConnection conn = null)
        {
            Logger.LogTrace("Setting RTPC over the network with ID: " + rtpcId + " to " + value, Logger.LogType.Local, this);
            InternalSetRTPC(rtpcId, value, GetLocalPlayer());
            SetRTPCForClients(rtpcId, value, conn);
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
        
        [ObserversRpc(ExcludeServer = true)]
        private void SetRTPCForClients(uint rtpcId, float value, NetworkConnection conn = null)
        {
            if (conn != null && conn == InstanceFinder.ClientManager.Connection)
            {
                return;
            }
            InternalSetRTPC(rtpcId, value, GetLocalPlayer());
        }

        private void InternalSetRTPC(uint rtpcId, float value, GameObject go)
        {
            if (rtpcId == 0)
            {
                if(AudioManagerData.RPTCLog) Logger.Log(AudioManagerData.RTPCNotFoundLogLevel, Logger.LogType.Local,
                    $"Tried to set an RTPC with ID {rtpcId} but was not found, it means that the RTPC is probably not assigned properly in AudioManagerData", this);
            }
            if(AudioManagerData.RPTCLog) Logger.LogTrace("Setting RTPC locally (ID: " + rtpcId + ") to " + value, Logger.LogType.Local, this);
            var result = AkSoundEngine.SetRTPCValue(rtpcId, value, go);
            var resultString = Enum.GetName(typeof(AKRESULT), result);
            if (result != AKRESULT.AK_Success)
            {
                Logger.LogError("Failed to set RTPC locally (ID: " + rtpcId + ") to " + value + " with result " + resultString, Logger.LogType.Local, this);
            }
        }
        
        private void InternalSetRPCGlobal(uint rtpcId, float value)
        {
            if (rtpcId == 0)
            {
                if(AudioManagerData.RPTCLog) Logger.Log(AudioManagerData.RTPCNotFoundLogLevel, Logger.LogType.Local,
                    $"Tried to set an RTPC with ID {rtpcId} but was not found, it means that the RTPC is probably not assigned properly in AudioManagerData", this);
            }
            if(AudioManagerData.RPTCLog) Logger.LogTrace("Setting Global RTPC locally (ID: " + rtpcId + ") to " + value, Logger.LogType.Local, this);
            var result = AkSoundEngine.SetRTPCValue(rtpcId, value);
            var resultString = Enum.GetName(typeof(AKRESULT), result);
            if (result != AKRESULT.AK_Success)
            {
                Logger.LogError("Failed to set Global RTPC locally (ID: " + rtpcId + ") to " + value + " with result " + resultString, Logger.LogType.Local, this);
            }
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
        
        public GameObject GetLocalPlayer()
        {
            if (!_localPlayer)
            {
                _localPlayer = FindAnyObjectByType<AudioManagerLocal>().gameObject;
                if (!AkSoundEngine.IsGameObjectRegistered(_localPlayer))
                {
                    var result = AkSoundEngine.RegisterGameObj(_localPlayer);
                    if (result != AKRESULT.AK_Success)
                    {
                        Logger.LogError("Failed to register local player to Wwise", Logger.LogType.Local, this);
                        return null;
                    }
                    Logger.LogTrace("Local Audio Player registered to Wwise !", Logger.LogType.Local, this);
                }
                if (!_localPlayer)
                {
                    Logger.LogError("No AudioManagerLocal found in scene", Logger.LogType.Local, this);
                    return null;
                }
                
                Logger.LogDebug("LocalPlayer wwise id is " + AkSoundEngine.GetAkGameObjectID(_localPlayer), Logger.LogType.Local, this);
                return _localPlayer;
            }
            
            Logger.LogDebug("LocalPlayer wwise id is " + AkSoundEngine.GetAkGameObjectID(_localPlayer), Logger.LogType.Local, this);
            return _localPlayer;
        }
        
        private void CleanLocalPlayer()
        {
            _localPlayer = null;
            Logger.LogTrace("Local Audio Player cleaned !", Logger.LogType.Local, this);
        }
    }
}