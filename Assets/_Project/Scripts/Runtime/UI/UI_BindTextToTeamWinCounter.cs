using System;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using TMPro;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UI_BindTextToTeamWinCounter : MonoBehaviour
    {
        [SerializeField] private PlayerTeamType _teamType = PlayerTeamType.Z;
        private TMP_Text _text;
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (_teamType == PlayerTeamType.Z)
            {
                Logger.LogWarning("Team type is Z, this is not a valid team type. Please set a valid team type.", Logger.LogType.Local, this);
            }
        }

        private void Update()
        {
            if (_teamType == PlayerTeamType.Z) return;
            if (GameManager.HasInstance)
            {
                switch (_teamType)
                {
                    case PlayerTeamType.A:
                        _text.text = $"Team A : {GameManager.Instance.GetWinCount(_teamType)}";
                        break;
                    case PlayerTeamType.B:
                        _text.text = $"Team B : {GameManager.Instance.GetWinCount(_teamType)}";
                        break;
                    case PlayerTeamType.Z:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _text.text = "Team Z : 0";
            }
        }
    }
}