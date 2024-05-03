using System.Collections;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    [RequireComponent(typeof(TongueInteractable))]
    public class TongueInteractableColorChange : MonoBehaviour
    {
        [SerializeField, Required] private MeshRenderer _meshRenderer;
        [SerializeField] private Color _onInteractColor;
        private Color _defaultColor;
        private TongueInteractable _tongueInteractable;
        
        private void Awake()
        {
            if (!_meshRenderer)
            {
                Logger.LogError("MeshRenderer is not set !", context:this);
            }
            _tongueInteractable = GetComponent<TongueInteractable>();
            _tongueInteractable.OnInteract += ChangeColor;
            // make a copy of the material to avoid changing the material of all instances of the object
            _meshRenderer.material = new Material(_meshRenderer.material);
            _defaultColor = _meshRenderer.material.color;
        }

        private void ChangeColor(PlayerStickyTongue _)
        {
            StartCoroutine(ChangeColorCoroutine());
        }
        
        private IEnumerator ChangeColorCoroutine()
        {
            Logger.LogTrace("Change color to interact color", Logger.LogType.Client, this);
            _meshRenderer.material.color = _onInteractColor;
            yield return new WaitForSeconds(2f);
            _meshRenderer.material.color = _defaultColor;
            Logger.LogTrace("Change color to default color", Logger.LogType.Client, this);
        }
    }
}