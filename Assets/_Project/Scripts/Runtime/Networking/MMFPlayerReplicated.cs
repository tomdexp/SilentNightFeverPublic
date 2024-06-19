using System;
using FishNet;
using FishNet.Transporting;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    [RequireComponent(typeof(MMF_Player))]
    public class MMFPlayerReplicated : MonoBehaviour
    {
        [SerializeField] private string _id;
        private MMF_Player _mmfPlayer;
        
        private void Awake()
        {
            _mmfPlayer = GetComponent<MMF_Player>();
        }

        private void Start()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<MMFPlayerReplicatedBroadcast>(OnChatBroadcast);
        }

        private void OnDestroy()
        {
            if(InstanceFinder.ClientManager) InstanceFinder.ClientManager.UnregisterBroadcast<MMFPlayerReplicatedBroadcast>(OnChatBroadcast);
        }

        private void OnChatBroadcast(MMFPlayerReplicatedBroadcast broadcast, Channel channel)
        {
            if (broadcast.Id != _id) return;
            Logger.LogTrace("Received MMFPlayerReplicatedBroadcast with id: " + _id + " and command: " + broadcast.FeedbackCommand, Logger.LogType.Client, this);
            switch (broadcast.FeedbackCommand)
            {
                case MMFPlayerReplicatedBroadcast.Command.Play:
                    _mmfPlayer.PlayFeedbacks();
                    break;
                case MMFPlayerReplicatedBroadcast.Command.Stop:
                    _mmfPlayer.StopFeedbacks();
                    break;
                case MMFPlayerReplicatedBroadcast.Command.Restore:
                    _mmfPlayer.StopFeedbacks();
                    _mmfPlayer.RestoreInitialValues();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void PlayFeedbacksForAll()
        {
            if (!InstanceFinder.IsServerStarted) return;
            var broadcast = new MMFPlayerReplicatedBroadcast
            {
                Id = _id,
                FeedbackCommand = MMFPlayerReplicatedBroadcast.Command.Play
            };
            InstanceFinder.ServerManager.Broadcast(broadcast);
            Logger.LogTrace("Broadcasting Play MMFPlayerReplicatedBroadcast with id: " + _id, Logger.LogType.Client, this);
        }

        public void StopFeedbacksForAll()
        {
            if (!InstanceFinder.IsServerStarted) return;
            var broadcast = new MMFPlayerReplicatedBroadcast
            {
                Id = _id,
                FeedbackCommand = MMFPlayerReplicatedBroadcast.Command.Stop
            };
            InstanceFinder.ServerManager.Broadcast(broadcast);
            Logger.LogTrace("Broadcast Stop MMFPlayerReplicatedBroadcast with id: " + _id, Logger.LogType.Client, this);
        }

        /// <summary>
        /// Restore will automatically stop the feedbacks before restoring the initial values
        /// </summary>
        public void RestoreFeedbacksForAll()
        {
            if (!InstanceFinder.IsServerStarted) return;
            var broadcast = new MMFPlayerReplicatedBroadcast
            {
                Id = _id,
                FeedbackCommand = MMFPlayerReplicatedBroadcast.Command.Restore
            };
            InstanceFinder.ServerManager.Broadcast(broadcast);
            Logger.LogTrace("Broadcast Restore MMFPlayerReplicatedBroadcast with id: " + _id, Logger.LogType.Client,
                this);
        }
    }
}