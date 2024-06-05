using System;
using _Project.Scripts.Runtime.Utils;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PressStartMenu : MenuBase
    {
        public override string MenuName { get; } = "PressStartMenu";
        
        private CanvasGroup _canvasGroup;
        private IDisposable m_EventListener;
        private CanvasGroup _pubCanvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.CloseInstant();
            _pubCanvasGroup = FindAnyObjectByType<PubCanvas>().GetComponent<CanvasGroup>();
        }

        public override void Open()
        {
            base.Open();
            _canvasGroup.Open();
            _pubCanvasGroup.alpha = 1;
            UIManager.Instance.SwitchToMetroCamera();
            // Start listening.
            m_EventListener = InputSystem.onAnyButtonPress.Call(OnButtonPressed);
        }

        public override void Close()
        {
            base.Close();
            _canvasGroup.Close();
            _pubCanvasGroup.alpha = 0;
            m_EventListener.Dispose();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_EventListener?.Dispose();
            Logger.LogTrace("PressStartMenu destroyed", Logger.LogType.Client,this);
        }

        private void OnButtonPressed(InputControl obj)
        {
            if (!UIManager.HasInstance) return;
            if (UIManager.Instance.GetCurrentMenuName() == MenuName) // additional check due to caveats with the disposing of the event listener
            {
                UIManager.Instance.GoToMenu<MainMenu>();
            }
        }
    }
}