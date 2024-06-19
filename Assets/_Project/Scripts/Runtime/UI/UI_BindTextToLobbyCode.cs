using System;
using _Project.Scripts.Runtime.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_BindTextToLobbyCode : MonoBehaviour
    {
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (Time.frameCount % 30 == 0)
            {
                UpdateText();
            }
        }

        private void UpdateText()
        {
            if (!BootstrapManager.HasInstance) return;
            if (!BootstrapManager.Instance.HasJoinCode) return;
            _text.text = BootstrapManager.Instance.CurrentJoinCode;
        }
    }
}