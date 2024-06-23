using FishNet.Broadcast;

namespace _Project.Scripts.Runtime.Networking.Broadcast
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