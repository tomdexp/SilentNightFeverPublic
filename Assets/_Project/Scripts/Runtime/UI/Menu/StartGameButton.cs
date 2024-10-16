﻿using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public class StartGameButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
    
            _button.onClick.AddListener(OnButtonClicked);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (GameManager.HasInstance) GameManager.Instance.LoadGameScene();
        }
    }
}