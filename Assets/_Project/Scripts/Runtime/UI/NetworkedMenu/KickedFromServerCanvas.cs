using System;
using System.Collections;
using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    [RequireComponent(typeof(ConfirmationPrompt))]
    public class KickedFromServerCanvas : MonoBehaviour
    {
        private ConfirmationPrompt _kickedFromServerPrompt;

        private void Awake()
        {
            _kickedFromServerPrompt = GetComponent<ConfirmationPrompt>();
        }

        private void Start()
        {
            BootstrapManager.Instance.OnKickedFromServer += OnKickedFromServer;
        }

        private void OnDestroy()
        {
            if (BootstrapManager.HasInstance)
            {
                BootstrapManager.Instance.OnKickedFromServer -= OnKickedFromServer;
            }
        }

        private void OnKickedFromServer()
        {
            Logger.LogTrace("Kicked from server, opening return to main menu prompt", Logger.LogType.Client,this);
            StartCoroutine(OnKickedFromServerCoroutine());
        }

        private IEnumerator OnKickedFromServerCoroutine()
        {
            yield return new WaitForSeconds(1f); // don't remove this, it's necessary to wait for old instance of UIManager to be destroyed
            yield return new WaitUntil(() => UIManager.HasInstance);
            _kickedFromServerPrompt.Open();
            yield return _kickedFromServerPrompt.WaitForResponse();
            UIManager.Instance.GoToMenu<MainMenu>();
        }
    }
}