using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Networking
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("Player spawned on client");
        }
    }
}