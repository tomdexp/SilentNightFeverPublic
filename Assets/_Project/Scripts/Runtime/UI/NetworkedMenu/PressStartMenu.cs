using System;
using _Project.Scripts.Runtime.Utils;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class PressStartMenu : MenuBase
    {
        public override string MenuName { get; } = "PressStartMenu";
        
        private IDisposable m_EventListener;

        public override void Open()
        {
            base.Open();
            
            // Go from canvas camera to metro camera
            var metroCamera = FindAnyObjectByType<MetroCamera>();
            var canvasCamera = FindAnyObjectByType<MetroWorldSpaceCanvasCamera>();
            metroCamera.GetComponent<CinemachineCamera>().Priority.Value = 10;
            canvasCamera.GetComponent<CinemachineCamera>().Priority.Value = 0;
            // Start listening.
            m_EventListener =
                InputSystem.onAnyButtonPress
                    .Call(OnButtonPressed);
        }

        public override void Close()
        {
            base.Close();
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