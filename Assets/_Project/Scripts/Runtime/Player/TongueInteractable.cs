using Micosmo.SensorToolkit;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player
{
    public class TongueInteractable : MonoBehaviour
    {
        public void TryInteract(PlayerStickyTongue tongue, RayHit hitInfo)
        {
            Logger.LogTrace("Push");
        }
    }
}