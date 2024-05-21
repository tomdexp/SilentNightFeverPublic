using System;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using UnityEngine.UI;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public class UI_ButtonCopyJoinCodeToClipboard : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (!BootstrapManager.HasInstance) return;
            if (!BootstrapManager.Instance.HasJoinCode) return;
            GUIUtility.systemCopyBuffer = BootstrapManager.Instance.CurrentJoinCode;
            Logger.LogInfo($"Copied join code to clipboard : {BootstrapManager.Instance.CurrentJoinCode} !", Logger.LogType.Local, this);
        }
    }
}