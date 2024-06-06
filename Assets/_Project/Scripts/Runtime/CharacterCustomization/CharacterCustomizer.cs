using _Project.Scripts.Runtime.Networking;
using FishNet;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] GameObject _playerAMannequin;
    [SerializeField] GameObject _playerAMannequinArrows;
    [Space]
    [SerializeField] GameObject _playerBMannequin;
    [SerializeField] GameObject _playerBMannequinArrows;
    [Space]
    [SerializeField] GameObject _playerCMannequin;
    [SerializeField] GameObject _playerCMannequinArrows;
    [Space]
    [SerializeField] GameObject _playerDMannequin;
    [SerializeField] GameObject _playerDMannequinArrows;

    private void Awake()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (PlayerManager.HasInstance)
        {
            PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;
            PlayerManager.Instance.OnCharacterCustomizationStarted -= StartCustomization;
        }

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
        PlayerManager.Instance.OnCharacterCustomizationStarted += StartCustomization;
        PlayerManager.Instance.OnPlayerHatInfosChanged += OnPlayerHatInfosChanged;
    }

    private void StartCustomization()
    {
        _playerAMannequin.gameObject.SetActive(true);
        _playerBMannequin.gameObject.SetActive(true);
        _playerCMannequin.gameObject.SetActive(true);
        _playerDMannequin.gameObject.SetActive(true);
    }

    private void StopCustomization()
    {
        _playerAMannequin.gameObject.SetActive(false);
        _playerBMannequin.gameObject.SetActive(false);
        _playerCMannequin.gameObject.SetActive(false);
        _playerDMannequin.gameObject.SetActive(false);
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
                    }
                    else
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
