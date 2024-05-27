using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    public class TransitionCanvasGroupBlackFade : TransitionCanvasGroup
    {
        public override IEnumerator BeginTransition()
        {
            var tween = DOTween
                .To(() => _fadeValue.Value, x => _fadeValue.Value = x, 1, Data.TransitionFadeInDuration)
                .SetEase(Data.TransitionFadeInEase);
            yield return tween.WaitForCompletion();
        }

        public override IEnumerator EndTransition()
        {
            var tween = DOTween
                .To(() => _fadeValue.Value, x => _fadeValue.Value = x, 0, Data.TransitionFadeOutDuration)
                .SetEase(Data.TransitionFadeOutEase);
            yield return tween.WaitForCompletion();
        }
    }
}