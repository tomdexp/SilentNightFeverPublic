using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_HoverFeedbacks : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField, Required] private MMF_Player _hoverFeedback;
        [SerializeField, Required] private MMF_Player _unHoverFeedback;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayHoverFeedbacks();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayUnHoverFeedbacks();
        }

        public void OnSelect(BaseEventData eventData)
        {
            PlayHoverFeedbacks();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            PlayUnHoverFeedbacks();
        }
        
        private void PlayHoverFeedbacks()
        {
            _unHoverFeedback.StopFeedbacks();
            _hoverFeedback.PlayFeedbacks();
        }
        
        private void PlayUnHoverFeedbacks()
        {
            _hoverFeedback.StopFeedbacks();
            _unHoverFeedback.PlayFeedbacks();
        }
    }
}