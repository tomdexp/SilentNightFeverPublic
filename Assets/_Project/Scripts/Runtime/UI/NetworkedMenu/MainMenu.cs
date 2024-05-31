using System;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public class MainMenu : MenuBase
    {
        public override string MenuName { get; } = "MainMenu";
        [SerializeField, Required] private CanvasGroup _pubCanvasGroup;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        public override void Open()
        {
            base.Open();
            
            // Go from metro camera to canvas camera
            
            var metroCamera = FindAnyObjectByType<MetroCamera>();
            var canvasCamera = FindAnyObjectByType<MetroWorldSpaceCanvasCamera>();
            metroCamera.GetComponent<CinemachineCamera>().Priority.Value = 0;
            canvasCamera.GetComponent<CinemachineCamera>().Priority.Value = 10;
            _pubCanvasGroup.alpha = 0;
            _canvasGroup.alpha = 1;
        }
        
        public override void Close()
        {
            base.Close();
            _canvasGroup.alpha = 0;
            _pubCanvasGroup.alpha = 1;
        }

        public override void GoBack()
        {
            base.GoBack();
            if (UIManager.HasInstance) UIManager.Instance.GoToMenu<PressStartMenu>();
        }
    }
}