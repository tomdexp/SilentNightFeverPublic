﻿using System;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public class CreateLobbyButton : MonoBehaviour
    {
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (BootstrapManager.HasInstance) BootstrapManager.Instance.TryStartHostWithRelay();
        }
    }
}