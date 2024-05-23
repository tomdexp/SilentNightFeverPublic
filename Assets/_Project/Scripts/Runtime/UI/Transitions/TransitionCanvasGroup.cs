using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    /// <summary>
    /// This component exist to facilitate the creation of Transition, since there is going to be multiples Transition (Menu to Game, Game Loding, Round Loading, etc)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TransitionCanvasGroup : NetworkBehaviour
    {
        [Title("References")] 
        public UIData Data;
        
        protected readonly SyncVar<float> _fadeValue = new SyncVar<float>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers, 30f, Channel.Unreliable));
        private CanvasGroup _canvasGroup;

        public abstract IEnumerator BeginTransition();
        public abstract IEnumerator EndTransition();
        
        [Button(ButtonSizes.Large)]
        public void DebugBeginTransition()
        {
            StartCoroutine(BeginTransition());
        }
        
        [Button(ButtonSizes.Large)]
        public void DebugEndTransition()
        {
            StartCoroutine(EndTransition());
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            _canvasGroup.alpha = _fadeValue.Value;
        }
    }
}