using FishNet.Broadcast;

namespace _Project.Scripts.Runtime.Networking
{
    public struct TransitionBroadcast : IBroadcast
    {
        public string Id;
        public bool Open;
    }
}