using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    public class TransitionCanvasGroupLoadingGame : TransitionCanvasGroup
    {
        [Title("Reference")]
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private Image _progressBar;
        
        private bool _isLoading = false;
        
        public override IEnumerator BeginTransition()
        {
            BindToLoadingTextAndProgressbar();
            var tween = DOTween.
                To(() => _fadeValue.Value, x => _fadeValue.Value = x, 1, Data.TransitionLoadingGameFadeInDuration)
                .SetEase(Data.TransitionLoadingGameFadeInEase);
            yield return tween.WaitForCompletion();
            _fadeValue.Value = 1;
        }

        public override IEnumerator EndTransition()
        {
            var tween = DOTween
                .To(() => _fadeValue.Value, x => _fadeValue.Value = x, 0, Data.TransitionLoadingGameFadeOutDuration)
                .SetEase(Data.TransitionLoadingGameFadeOutEase);
            yield return tween.WaitForCompletion();
            UnbindFromLoadingTextAndProgressbar();
            _fadeValue.Value = 0;
        }

        private void BindToLoadingTextAndProgressbar()
        {
            var procGen = FindAnyObjectByType<ProcGenInstanciator>();
            _progressBar.fillAmount = 0;
            if (!procGen) return;
            _isLoading = true;
            procGen.OnLoadingProgressChanged += OnLoadingProgressChanged;
        }
        
        private void UnbindFromLoadingTextAndProgressbar()
        {
            var procGen = FindAnyObjectByType<ProcGenInstanciator>();
            if (!procGen) return;
            _isLoading = false;
            procGen.OnLoadingProgressChanged -= OnLoadingProgressChanged;
        }

        private void OnLoadingProgressChanged(float percent, string message)
        {
            _loadingText.text = $"{message}";
            _progressBar.fillAmount = percent;
        }

        protected override void Update()
        {
            base.Update();
            // tick less often
            if (Time.frameCount % 10 != 0) return;
            if (_isLoading)
            {
                var currentDots = _loadingText.text.Split('.').Length - 1;
                var dots = (currentDots + 1) % 4;
                // remove the existing dots
                _loadingText.text = _loadingText.text.Replace(".", "");
                // add the new dots
                for (var i = 0; i < dots; i++)
                {
                    _loadingText.text += ".";
                }
            }
        }
    }
}