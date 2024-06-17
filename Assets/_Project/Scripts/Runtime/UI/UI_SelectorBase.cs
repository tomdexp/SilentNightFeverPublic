using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    public abstract class UI_SelectorBase : MonoBehaviour
    {
        [SerializeField, Required] private Button _previousButton;
        [SerializeField, Required] private Button _nextButton;
        public Button PreviousButton => _previousButton;
        public Button NextButton => _nextButton;

        public virtual void Awake()
        {
            _previousButton.onClick.AddListener(OnPreviousButtonClicked);
            _nextButton.onClick.AddListener(OnNextButtonClicked);
        }
        public virtual void OnDestroy()
        {
            _previousButton.onClick.RemoveListener(OnPreviousButtonClicked);
            _nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }
        
        protected abstract void OnPreviousButtonClicked();
        protected abstract void OnNextButtonClicked();
        
        
    }
}