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
            _fadeValue.Value = 1;
        }

        public override IEnumerator EndTransition()
        {
            if (DoSyncFadeValue)
            {
                var tween = DOTween
                    .To(() => _fadeValue.Value, x => _fadeValue.Value = x, 0, Data.TransitionFadeOutDuration)
                    .SetEase(Data.TransitionFadeOutEase);
                yield return tween.WaitForCompletion();
                _fadeValue.Value = 0;
            }
            else
            {
                var tween = DOTween
                    .To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, Data.TransitionFadeOutDuration)
                    .SetEase(Data.TransitionFadeOutEase);
                yield return tween.WaitForCompletion();
                _canvasGroup.alpha = 0;
            }
            
        }
    }
}