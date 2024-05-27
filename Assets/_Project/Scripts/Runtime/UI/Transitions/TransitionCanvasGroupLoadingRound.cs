using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    public class TransitionCanvasGroupLoadingRound : TransitionCanvasGroup
    {
        public override IEnumerator BeginTransition()
        {
            var tween = DOTween.
                To(() => _fadeValue.Value, x => _fadeValue.Value = x, 1, Data.TransitionLoadingRoundFadeInDuration)
                .SetEase(Data.TransitionLoadingRoundFadeInEase);
            yield return tween.WaitForCompletion();
        }

        public override IEnumerator EndTransition()
        {
            var tween = DOTween
                .To(() => _fadeValue.Value, x => _fadeValue.Value = x, 0, Data.TransitionLoadingRoundFadeOutDuration)
                .SetEase(Data.TransitionLoadingRoundFadeOutEase);
            yield return tween.WaitForCompletion();
        }
    }
}