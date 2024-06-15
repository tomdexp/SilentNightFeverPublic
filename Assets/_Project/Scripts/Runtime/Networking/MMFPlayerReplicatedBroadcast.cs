using FishNet.Broadcast;
using Unity.Collections;

namespace _Project.Scripts.Runtime.Networking
{
    public struct MMFPlayerReplicatedBroadcast : IBroadcast
    {
        public string Id;
        public Command FeedbackCommand;

        public enum Command
        {
            Play,
            Stop,
            Restore
        }
    }
}