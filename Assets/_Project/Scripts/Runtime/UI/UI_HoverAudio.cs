using _Project.Scripts.Runtime.Audio;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Runtime.UI
{
    public class UI_HoverAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Title("References")]
        [SerializeField] private AK.Wwise.Event _hoverEvent;
        [SerializeField] private AK.Wwise.Event _unHoverEvent;
        
        private void PlayHoverEvent()
        {
            if (!_hoverEvent.IsValid()) return;
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(_hoverEvent, AudioManager.Instance.gameObject);
        }
        
        private void PlayUnHoverEvent()
        {
            if (!_unHoverEvent.IsValid()) return;
            if (AudioManager.HasInstance) AudioManager.Instance.PlayAudioLocal(_unHoverEvent, AudioManager.Instance.gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            PlayHoverEvent();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            PlayUnHoverEvent();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayHoverEvent();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayUnHoverEvent();
        }
    }
}