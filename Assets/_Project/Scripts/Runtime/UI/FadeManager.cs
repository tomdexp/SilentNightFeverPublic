using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Utils.Singletons;
using DG.Tweening;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeManager : NetworkPersistentSingleton<FadeManager>
    {
        [Title("Settings")]
        private float _fadeInDuration = 1.0f;
        [SerializeField] private Ease _easeFadeIn = Ease.Linear;
        [SerializeField] private float _fadeOutDuration = 1.0f;
        [SerializeField] private Ease _easeFadeOut = Ease.Linear;
        [SerializeField] private float _delayBeforeFadeOut = .5f;

        private CanvasGroup _canvasGroup;
        private Queue<Action> _fadeQueue = new Queue<Action>();
        private bool _isFading;
        private readonly SyncVar<float> _currentFadeValue = new SyncVar<float>();

        public event Action OnFadeInStart;
        public event Action OnFadeInComplete;
        public event Action OnFadeOutStart;
        public event Action OnFadeOutComplete;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(TrySubscribeToGameManagerEvents());
        }

        private IEnumerator TrySubscribeToGameManagerEvents()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.OnBeforeSceneChange += OnBeforeSceneChange;
            GameManager.Instance.OnAfterSceneChange += OnAfterSceneChange;
            Logger.LogTrace("UI_Fader: Subscribed to GameManager events", Logger.LogType.Server, context:this);
        }

        private void OnDestroy()
        {
            if (!GameManager.HasInstance) return;
            GameManager.Instance.OnBeforeSceneChange -= OnBeforeSceneChange;
            GameManager.Instance.OnAfterSceneChange -= OnAfterSceneChange;
            Logger.LogTrace("UI_Fader: Unsubscribed from GameManager events", Logger.LogType.Server, context:this);
        }

        private void Update()
        {
            if (_isFading) _currentFadeValue.Value = _canvasGroup.alpha;
            if(!IsServerStarted) _canvasGroup.alpha = _currentFadeValue.Value;
        }

        private void OnBeforeSceneChange(float seconds)
        {
            Logger.LogTrace("UI_Fader: OnLoadStart");
            _fadeInDuration = seconds;
            EnqueueFade(FadeIn);
        }
        
        private void OnAfterSceneChange()
        {
            Logger.LogTrace("UI_Fader: OnLoadEnd");
            EnqueueFade(FadeOut);
        }

        public void FadeIn()
        {
            OnFadeInStart?.Invoke();
            _canvasGroup.DOFade(1.0f, _fadeInDuration).SetEase(_easeFadeIn).OnComplete(() =>
            {
                OnFadeInComplete?.Invoke();
                ProcessNextFade();
            });
        }

        public void FadeOut()
        {
            DOVirtual.DelayedCall(_delayBeforeFadeOut, () =>
            {
                OnFadeOutStart?.Invoke();
                _canvasGroup.DOFade(0.0f, _fadeOutDuration).SetEase(_easeFadeOut).OnComplete(() =>
                {
                    OnFadeOutComplete?.Invoke();
                    ProcessNextFade();
                });
            });
        }

        private void EnqueueFade(Action fadeAction)
        {
            _fadeQueue.Enqueue(fadeAction);
            if (!_isFading)
            {
                ProcessNextFade();
            }
        }

        private void ProcessNextFade()
        {
            if (_fadeQueue.Count > 0)
            {
                _isFading = true;
                var nextFade = _fadeQueue.Dequeue();
                nextFade.Invoke();
            }
            else
            {
                _isFading = false;
            }
        }

        public void FadeInThenOut()
        {
            EnqueueFade(() =>
            {
                FadeIn();
                DOVirtual.DelayedCall(_fadeInDuration, () => EnqueueFade(FadeOut));
            });
        }

        public void FadeOutThenIn()
        {
            EnqueueFade(() =>
            {
                FadeOut();
                DOVirtual.DelayedCall(_fadeOutDuration, () => EnqueueFade(FadeIn));
            });
        }
    }
}
