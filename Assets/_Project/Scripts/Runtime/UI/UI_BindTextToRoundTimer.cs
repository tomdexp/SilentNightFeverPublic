using System;
using _Project.Scripts.Runtime.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_BindTextToRoundTimer : MonoBehaviour
    {
        private TMP_Text _text;
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            // We want to text to have this exact format "00:00" where the first two digits are the minutes and the last two are the seconds
            
            if (GameManager.HasInstance)
            {
                TimeSpan t = TimeSpan.FromSeconds(GameManager.Instance.CurrentRoundTimer.Value);
                _text.text = string.Format("{0:D2}:{1:D2}", 
                    t.Minutes, 
                    t.Seconds);
            }
            else
            {
                _text.text = "00:00"; 
            }
        }
    }
}