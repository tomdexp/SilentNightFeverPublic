using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public class UI_LanguageSelectionButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required] private UIData _uiData;
        [SerializeField, Required] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField, Required] private UI_LanguageButton _languageButtonPrefab;
        
        private List<UI_LanguageButton> _languageButtons = new List<UI_LanguageButton>();
        private Coroutine _hoverCoroutine;
        private Coroutine _unHoverCoroutine;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            foreach (var language in _uiData.SupportedLanguages)
            {
                var languageButton = Instantiate(_languageButtonPrefab, _gridLayoutGroup.transform);
                languageButton.SetLanguageSelectionUI(language);
                _languageButtons.Add(languageButton);
            }

            BindNavigations();
            
            OnUnHover();
        }

        private void BindNavigations()
        {
            // the list a dropdown of languages
            
            // Bind this button with the first language button
            BindNavigableVertical(_button, _languageButtons[0].GetComponent<Button>());
            
            // then bind each buttons to the next one
            for (var index = 0; index < _languageButtons.Count - 1; index++)
            {
                var languageButton = _languageButtons[index];
                var nextLanguageButton = _languageButtons[index + 1];
                BindNavigableVertical(languageButton.GetComponent<Button>(), nextLanguageButton.GetComponent<Button>());
            }
            
            // then bind the last button with this button
            BindNavigableVertical(_languageButtons[^1].GetComponent<Button>(), _button);
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnHover();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            StartCoroutine(OnDeselectCoroutine(eventData));
        }

        private IEnumerator OnDeselectCoroutine(BaseEventData eventData)
        {
            yield return new WaitForEndOfFrame();
            // only play the unhover if the current selectable is not one of the language buttons
            foreach (var languageButton in _languageButtons)
            {
                if (EventSystem.current.currentSelectedGameObject == languageButton.gameObject)
                {
                    Logger.LogTrace("OnDeselect: currentSelectedGameObject is a language button", Logger.LogType.Local, this);
                    yield break;
                }
            }
            Logger.LogTrace($"OnDeselect: currentSelectedGameObject is not a language button : {EventSystem.current.currentSelectedGameObject}", Logger.LogType.Local, this);
            OnUnHover();
        }
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnUnHover();
        }
        
        private void OnHover()
        {
            if (_unHoverCoroutine != null)
            {
                StopCoroutine(_unHoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(OnHoverCoroutine());
        }
        
        private IEnumerator OnHoverCoroutine()
        {
            foreach (var button in _languageButtons)
            {
                button.Show();
                yield return new WaitForSeconds(_uiData.SecondsBetweenLanguageButtonAppearAnimation);
            }
        }
        
        private void OnUnHover()
        {
            if (_hoverCoroutine != null)
            {
                StopCoroutine(_hoverCoroutine);
            }
            _unHoverCoroutine = StartCoroutine(OnUnHoverCoroutine());
        }
        
        private IEnumerator OnUnHoverCoroutine()
        {
            for (var index = _languageButtons.Count - 1; index >= 0; index--)
            {
                var button = _languageButtons[index];
                button.Hide();
                yield return new WaitForSeconds(_uiData.SecondsBetweenLanguageButtonAppearAnimation);
            }
        }
        
        private void BindNavigableVertical(Selectable selectable1, Selectable selectable2)
        {
            Navigation nav1 = selectable1.navigation;
            Navigation nav2 = selectable2.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav2.mode = Navigation.Mode.Explicit;
            nav1.selectOnUp = selectable2; // reversed
            nav2.selectOnDown = selectable1; // reversed
            selectable1.navigation = nav1;
            selectable2.navigation = nav2;
        }
     }
}