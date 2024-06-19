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
        private UI_BindRandomText _loadingText;

        protected override void Start()
        {
            base.Start();
            _loadingText = GetComponentInChildren<UI_BindRandomText>();
        }

        public override IEnumerator BeginTransition()
        {
            yield return base.BeginTransition();
            _loadingText.PickRandomText();
            var tween = DOTween.
                To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1, Data.TransitionLoadingGameFadeInDuration)
                .SetEase(Data.TransitionLoadingGameFadeInEase);
            yield return tween.WaitForCompletion();
            _canvasGroup.alpha = 1;
        }

        public override IEnumerator EndTransition()
        {
            yield return base.EndTransition();
            yield return new WaitForSeconds(1f);
            var tween = DOTween
                .To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, Data.TransitionLoadingGameFadeOutDuration)
                .SetEase(Data.TransitionLoadingGameFadeOutEase);
            yield return tween.WaitForCompletion();
            _canvasGroup.alpha = 0;
        }
        
    }
}