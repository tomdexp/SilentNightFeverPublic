using System;
using _Project.Scripts.Runtime.Utils;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class PressStartMenu : MenuBase
    {
        public override string MenuName { get; } = "PressStartMenu";
        
        private IDisposable m_EventListener;
        private CanvasGroup _pubCanvasGroup;

        private void Awake()
        {
            _pubCanvasGroup = FindAnyObjectByType<PubCanvas>().GetComponent<CanvasGroup>();
        }

        public override void Open()
        {
            base.Open();
            _pubCanvasGroup.alpha = 1;
            UIManager.Instance.SwitchToMetroCamera();
            // Start listening.
            m_EventListener =
                InputSystem.onAnyButtonPress
                    .Call(OnButtonPressed);
        }

        public override void Close()
        {
            base.Close();
            _pubCanvasGroup.alpha = 0;
            m_EventListener.Dispose();
        }

        private void OnDestroy()
        {
            m_EventListener?.Dispose();
        }

        private void OnButtonPressed(InputControl obj)
        {
            if(UIManager.HasInstance) UIManager.Instance.GoToMenu<MainMenu>();
        }
    }
}