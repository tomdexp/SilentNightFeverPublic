using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Networking.Broadcast;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    /// <summary>
    /// This component exist to facilitate the creation of Transition, since there is going to be multiples Transition (Menu to Game, Game Loding, Round Loading, etc)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TransitionCanvasGroup : NetworkBehaviour
    {
        [Title("References")] 
        public UIData Data;
        protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<TransitionBroadcast>(OnTransitionBroadcast);
            Logger.LogTrace($"Registered broadcast for transitions of {GetType().Name}", Logger.LogType.Client, this);
        }
        
        protected void OnDestroy()
        {
            if (InstanceFinder.ClientManager)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<TransitionBroadcast>(OnTransitionBroadcast);
            }
        }

        private void OnTransitionBroadcast(TransitionBroadcast broadcast, Channel channel)
        {
            if (IsServerStarted) return; // Only for clients
            if (broadcast.Id != GetType().Name) return;
            Logger.LogTrace($"Received TransitionBroadcast (Id : {broadcast.Id}, open : {broadcast.Open})", Logger.LogType.Client, this);
            StartCoroutine(broadcast.Open ? BeginTransition() : EndTransition());
        }

        public virtual IEnumerator BeginTransition()
        {
            if (IsServerStarted)
            {
                var broadcast = new TransitionBroadcast
                {
                    Id = GetType().Name,
                    Open = true
                };
                InstanceFinder.ServerManager.Broadcast(broadcast);
                Logger.LogTrace($"Server invoked TransitionBroadcast (Id : {broadcast.Id}, open : {broadcast.Open})", Logger.LogType.Server, this);
            }
            else
            {
                Logger.LogTrace("Client invoked BeginTransition", Logger.LogType.Client, this);
            }
            yield return null;
        }

        public virtual IEnumerator EndTransition()
        {
            if (IsServerStarted)
            {
                var broadcast = new TransitionBroadcast
                {
                    Id = GetType().Name,
                    Open = false
                };
                InstanceFinder.ServerManager.Broadcast(broadcast);
                Logger.LogTrace($"Server invoked TransitionBroadcast (Id : {broadcast.Id}, open : {broadcast.Open})", Logger.LogType.Server, this);
            }
            else
            {
                Logger.LogTrace("Client invoked EndTransition", Logger.LogType.Client, this);
            }
            yield return null;
        }
        
        [Button(ButtonSizes.Large)]
        public void DebugBeginTransition()
        {
            StartCoroutine(BeginTransition());
        }
        
        [Button(ButtonSizes.Large)]
        public void DebugEndTransition()
        {
            StartCoroutine(EndTransition());
        }
    }
}