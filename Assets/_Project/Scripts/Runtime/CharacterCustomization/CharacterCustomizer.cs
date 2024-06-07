using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet;
using FishNet.Transporting;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

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
        StopCustomization();
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (PlayerManager.HasInstance)
        {
            PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;
            PlayerManager.Instance.OnCharacterCustomizationStarted -= StartCustomization;
            Logger.LogTrace("Unsubscribed from PlayerManager events", Logger.LogType.Client, this);
        }

        if (args.ConnectionState == LocalConnectionState.Started)
        {
            StartCoroutine(TrySubscribingToEvents());
        }
    }

    private void OnDestroy()
    {
        if (PlayerManager.HasInstance) PlayerManager.Instance.OnPlayerHatInfosChanged -= OnPlayerHatInfosChanged;
        if (PlayerManager.HasInstance) PlayerManager.Instance.OnCharacterCustomizationStarted -= StartCustomization;
    }

    private IEnumerator TrySubscribingToEvents()
    {
        while (!PlayerManager.HasInstance)
        {
            yield return null;
        }
        PlayerManager.Instance.OnCharacterCustomizationStarted += StartCustomization;
        Logger.LogTrace("Subscribed to PlayerManager events", Logger.LogType.Client, this);
        PlayerManager.Instance.OnPlayerHatInfosChanged += OnPlayerHatInfosChanged;
    }

    private void StartCustomization()
    {
        Logger.LogTrace("Starting customization by set active mannequin", Logger.LogType.Client, this);
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

    private void OnPlayerHatInfosChanged(List<PlayerHatInfo> hatInfos)
    {
        foreach (var hatInfo in hatInfos)
        {
            switch (hatInfo.PlayerIndexType)
            {
                case PlayerIndexType.A:
                    if (hatInfo.HasConfirmed)
                    {
                        _playerAMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerAMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                case PlayerIndexType.B:
                    if (hatInfo.HasConfirmed)
                    {
                        _playerBMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerBMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                case PlayerIndexType.C:
                    if (hatInfo.HasConfirmed)
                    {
                        _playerCMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerCMannequinArrows.gameObject.SetActive(true);
                    }
                    break;

                case PlayerIndexType.D:
                    if (hatInfo.HasConfirmed)
                    {
                        _playerDMannequinArrows.gameObject.SetActive(false);
                    }
                    else
                    {
                        _playerDMannequinArrows.gameObject.SetActive(true);
                    }
                    break;
            }
        }
    }
}
