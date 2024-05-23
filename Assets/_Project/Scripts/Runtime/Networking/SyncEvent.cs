using System;
using FishNet;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;

namespace _Project.Scripts.Runtime.Networking
{
    public class SyncEvent : SyncBase, ICustomSync
    {
        public event Action<bool> OnEvent;

        public void Invoke()
        {
            if (InstanceFinder.NetworkManager.IsServerStarted) 
            {
                // Invoke locally and mark as dirty to ensure it syncs to clients.
                OnEvent?.Invoke(true);
                Dirty();
            }
        }

        protected override void WriteDelta(PooledWriter writer, bool resetSyncTick = true)
        {
            base.WriteDelta(writer, resetSyncTick);
            writer.WriteBoolean(true);
        }

        protected override void Read(PooledReader reader, bool asServer)
        {
            // Read the byte, even though we don't need it, to advance the reader's position.
            reader.ReadBoolean();
        
            if (!asServer) // Only invoke on clients if coming from server.
            {
                OnEvent?.Invoke(false);
            }
        }

        public object GetSerializedType() => null; // No special serialization needed.
    }
}

