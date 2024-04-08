using UnityEngine;

namespace _Project.Scripts.Runtime.Player
{
    public class TongueInteractable : MonoBehaviour
    {
        public void TryInteract(PlayerStickyTongue tongue, RaycastHit hitInfo)
        {
            Debug.Log("Push");
        }
    }
}