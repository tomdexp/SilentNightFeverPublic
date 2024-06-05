using _Project.Scripts.Runtime.Networking;
using FishNet;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] GameObject _playerAMannequinArrows;
    [SerializeField] GameObject _playerBMannequinArrows;
    [SerializeField] GameObject _playerCMannequinArrows;
    [SerializeField] GameObject _playerDMannequinArrows;

    private void Awake()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (PlayerManager.HasInstance) PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;

        if (args.ConnectionState == LocalConnectionState.Started)
        {
            StartCoroutine(TrySubscribingToEvents());
        }
    }


    private void OnDestroy()
    {
        if (PlayerManager.HasInstance) PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;
    }

    private IEnumerator TrySubscribingToEvents()
    {
        while (!PlayerManager.HasInstance)
        {
            yield return null;
        }
        PlayerManager.Instance.OnPlayerHatInfosChanged += OnPlayerHatInfosChanged;
    }

    private void OnPlayerHatInfosChanged(List<_Project.Scripts.Runtime.Player.PlayerHatInfo> hatInfos)
    {
        foreach (var hatInfo in hatInfos)
        {
            switch (hatInfo.PlayerIndexType)
            {
                case _Project.Scripts.Runtime.Player.PlayerIndexType.A:
                    if (hatInfo.HasConfirmed == true)
                    {
                        _playerAMannequinArrows.gameObject.SetActive(false);
                    } else
                    {
                        _playerAMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                case _Project.Scripts.Runtime.Player.PlayerIndexType.B:
                    if (hatInfo.HasConfirmed == true)
                    {
                        _playerBMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerBMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                case _Project.Scripts.Runtime.Player.PlayerIndexType.C:
                    if (hatInfo.HasConfirmed == true)
                    {
                        _playerCMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerCMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                case _Project.Scripts.Runtime.Player.PlayerIndexType.D:
                    if (hatInfo.HasConfirmed == true)
                    {
                        _playerDMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerDMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
