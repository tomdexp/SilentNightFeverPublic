using System;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ParametersGraphicsMenu : MenuBase
    {
        public override string MenuName { get; } = "ParametersGraphicsMenu";
        [SerializeField, Required] private Button _backButton;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _backButton.onClick.AddListener(GoBack);
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _backButton.onClick.RemoveListener(GoBack);
        }

        public override void GoBack()
        {
            base.GoBack();
            UIManager.Instance.GoToMenu<ParametersMenu>();
        }
    }
}