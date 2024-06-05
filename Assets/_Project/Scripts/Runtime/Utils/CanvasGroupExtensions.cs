using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public static class CanvasGroupExtensions
    {
        private const float _fadeDuration = 0.25f;
        private const Ease _openEase = Ease.InQuint;
        private const Ease _closeEase = Ease.OutQuint;
        
        public static void Open(this CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1, _fadeDuration).SetEase(_openEase);
        }

        public static void Close(this CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0, _fadeDuration).SetEase(_closeEase);
        }

        public static void CloseInstant(this CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
        }
    }
}