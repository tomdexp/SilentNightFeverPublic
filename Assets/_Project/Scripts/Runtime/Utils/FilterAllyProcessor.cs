using System.Collections;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using Micosmo.SensorToolkit;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class FilterAllyProcessor : SignalProcessor
    {
        private NetworkPlayer _networkPlayer;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            _networkPlayer = GetComponentInParent<NetworkPlayer>();
            if (!_networkPlayer)
            {
                Logger.LogError("NetworkPlayer not found in parent", Logger.LogType.Local, this);
            }
        }
        
        public override bool Process(ref Signal signal, Sensor sensor)
        {
            // We try to search for a NetworkPlayer based on the architecture on the prefab of the player
            // If we find it we check if the player is an ally, if it is we multiply the strength of the signal by 10
            if(!_networkPlayer) return false;
            if (!signal.Object.TryGetComponent(out TongueCollider tongueCollider)) return false;
            var tongueAnchor = tongueCollider.GetComponentInParent<TongueAnchor>();
            if (!tongueAnchor) return false;
            var targetNetworkPlayer = tongueAnchor.LinkedNetworkObjectForOwnership.GetComponent<NetworkPlayer>();
            if (!targetNetworkPlayer) return false;
            if (targetNetworkPlayer.GetPlayerTeamType() != _networkPlayer.GetPlayerTeamType()) return false;
            signal.Strength *= 10f;
            return true;
        }
    }
}