using _Project.Scripts.Runtime.Player;
using TMPro;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_BindTextToPlayerIndexScreen : MonoBehaviour
    {
        private PlayerIndexScreen _playerIndexScreen;
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _playerIndexScreen = GetComponentInParent<PlayerIndexScreen>();
            // if (_playerIndexScreen)
            // {
            //     Logger.LogError("PlayerIndexScreen not found in parent", Logger.LogType.Local, this);
            // }
        }

        private void Update()
        {
            if (!_playerIndexScreen) return;
            _text.text = _playerIndexScreen.PlayerIndexType == PlayerIndexType.Z
                ? "Player Z"
                : "Player " + _playerIndexScreen.PlayerIndexType;
        }
    }
}