using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    /// <summary>
    /// This scriptable holds all data relevant to UI, colors, fonts, fade easing, etc.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(UIData), menuName = "Scriptable Objects/" + nameof(UIData))]
    public class UIData : ScriptableObject
    {
        [Title("Transition Settings")]
        public float TransitionFadeInDuration = 1.0f;
        public Ease TransitionFadeInEase = Ease.Linear;
        public float TransitionFadeOutDuration = 1.0f;
        public Ease TransitionFadeOutEase = Ease.Linear;
    }
}