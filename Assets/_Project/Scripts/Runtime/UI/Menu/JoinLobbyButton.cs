using _Project.Scripts.Runtime.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputField;

    public void TryJoinAsClient()
    {
        if (_inputField.text.Length == 7)
        {
            var joinCode = _inputField.text.Substring(0, 6);
            BootstrapManager.Instance.TryJoinAsClientWithRelay(joinCode);
        }
        else
        {
            Debug.LogWarning("Not a valid Lobby Code");
        }

    }
}
