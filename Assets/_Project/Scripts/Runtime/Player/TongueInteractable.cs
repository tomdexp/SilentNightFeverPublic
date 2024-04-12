using FishNet.Object;
using Micosmo.SensorToolkit;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    public class TongueInteractable : NetworkBehaviour
    {
        public void TryInteract(PlayerStickyTongue tongue, RayHit hitInfo)
        {
            Logger.LogTrace("TongueInteractable", Logger.LogType.Client, this);
        }
    }
}