using System;
using _Project.Scripts.Runtime.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_BindTextToRoundCounter : MonoBehaviour
    {
        private const string _prefix = "Round n°";
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
                _text.text = _prefix + GameManager.Instance.CurrentRoundNumber.Value;
            }
            else
            {
                _text.text = _prefix + "0";
            }
        }
    }
}