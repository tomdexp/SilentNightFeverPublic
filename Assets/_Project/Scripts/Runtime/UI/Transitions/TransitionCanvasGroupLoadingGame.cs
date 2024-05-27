using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    public class TransitionCanvasGroupLoadingGame : TransitionCanvasGroup
    {
        public override IEnumerator BeginTransition()
        {
            var tween = DOTween.
                To(() => _fadeValue.Value, x => _fadeValue.Value = x, 1, Data.TransitionLoadingGameFadeInDuration)
                .SetEase(Data.TransitionLoadingGameFadeInEase);
            yield return tween.WaitForCompletion();
        }

        public override IEnumerator EndTransition()
        {
            var tween = DOTween
                .To(() => _fadeValue.Value, x => _fadeValue.Value = x, 0, Data.TransitionLoadingGameFadeOutDuration)
                .SetEase(Data.TransitionLoadingGameFadeOutEase);
            yield return tween.WaitForCompletion();
        }
    }
}