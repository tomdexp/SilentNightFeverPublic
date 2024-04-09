using Micosmo.SensorToolkit;
using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class TongueInteractable : MonoBehaviour
    {
        public void TryInteract(PlayerStickyTongue tongue, RayHit hitInfo)
        {
            Debug.Log("Push");
        }
    }
}