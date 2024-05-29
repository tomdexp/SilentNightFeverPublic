using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_TextBlinker : MonoBehaviour
    {
        [SerializeField] private Color _blinkColor = Color.white;
        [SerializeField] private float _blinkDuration = 0.5f;
        [SerializeField] private Ease _blinkEase = Ease.Linear;
        
        private TMP_Text _text;
        private Color _originalColor;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _originalColor = _text.color;
        }

        private void Start()
        {
            Blink();
        }

        private void Blink()
        {
            _text.DOColor(_blinkColor, _blinkDuration).SetEase(_blinkEase).OnComplete(() =>
            {
                _text.DOColor(_originalColor, _blinkDuration).SetEase(_blinkEase).OnComplete(Blink);
            });
        }
    }
}