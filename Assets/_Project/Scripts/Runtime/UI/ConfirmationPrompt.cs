using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using _Project.Scripts.Runtime.UI.NetworkedMenu;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    
    [RequireComponent(typeof(CanvasGroup))]
    public class ConfirmationPrompt : MonoBehaviour
    {
        [SerializeField, Required] private Button _confirmButton;
        [SerializeField, Required] private Button _cancelButton;
        
        private bool _responseReceived = false;
        private bool _isSuccess = false;
        private CanvasGroup _canvasGroup;

        public bool IsSuccess => _isSuccess;
        public event Action OnResponseReceived;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            
            // Add listeners to the buttons
            _confirmButton.onClick.AddListener(OnConfirm);
            _cancelButton.onClick.AddListener(OnCancel);
            
            BindNavigableHorizontal(_confirmButton, _cancelButton);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(OnConfirm);
            _cancelButton.onClick.RemoveListener(OnCancel);
        }

        public void Open()
        {
            Logger.LogTrace($"Showing confirmation prompt : {name}", Logger.LogType.Client, this);
            _canvasGroup.Open();
            _responseReceived = false;
            _isSuccess = false;
            UIManager.Instance.RegisterConfirmationPrompt(this);
            TrySelectDefault();
        }
        
        public void TrySelectDefault()
        {
            if (!UIManager.Instance.IsNavigationWithMouse)
            {
                EventSystem.current.SetSelectedGameObject(_cancelButton.gameObject);
            }
        }

        private void OnConfirm()
        {
            Logger.LogTrace($"Confirmation prompt confirmed : {name}", Logger.LogType.Client, this);
            _isSuccess = true;
            _responseReceived = true;
            _canvasGroup.Close();
        }

        public void OnCancel()
        {
            Logger.LogTrace($"Confirmation prompt cancelled : {name}", Logger.LogType.Client, this);
            _isSuccess = false;
            _responseReceived = true;
            _canvasGroup.Close();
        }

        public IEnumerator WaitForResponse()
        {
            yield return new WaitUntil(() => _responseReceived);
            Logger.LogTrace($"Confirmation prompt response received : {name}", Logger.LogType.Client, this);
            OnResponseReceived?.Invoke();
        }
        
        private void BindNavigableHorizontal(Selectable leftSelectable, Selectable rightSelectable)
        {
            Navigation nav1 = leftSelectable.navigation;
            Navigation nav2 = rightSelectable.navigation;
            nav1.mode = Navigation.Mode.Explicit;
            nav2.mode = Navigation.Mode.Explicit;
            nav1.selectOnRight = rightSelectable;
            nav2.selectOnLeft = leftSelectable;
            leftSelectable.navigation = nav1;
            rightSelectable.navigation = nav2;
        }
    }

}