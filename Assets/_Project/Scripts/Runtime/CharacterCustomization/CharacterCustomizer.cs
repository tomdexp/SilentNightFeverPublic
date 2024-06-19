using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.CharacterCustomization
{
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
            StopCustomization();
        }

        public void StartCustomization()
        {
            Logger.LogTrace("Starting customization by activating mannequin", Logger.LogType.Client, this);
            _playerAMannequin.gameObject.SetActive(true);
            _playerBMannequin.gameObject.SetActive(true);
            _playerCMannequin.gameObject.SetActive(true);
            _playerDMannequin.gameObject.SetActive(true);
        }

        public void StopCustomization()
        {
            Logger.LogTrace("Stopping customization by disabling mannequin", Logger.LogType.Client, this);
            _playerAMannequin.gameObject.SetActive(false);
            _playerBMannequin.gameObject.SetActive(false);
            _playerCMannequin.gameObject.SetActive(false);
            _playerDMannequin.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                UpdateMannequins();
            }
        }

        private void UpdateMannequins()
        {
            if (!PlayerManager.HasInstance)
            {
                StopCustomization();
                return;
            }
        
            foreach (var hatInfo in PlayerManager.Instance.GetPlayerHatInfos())
            {
                switch (hatInfo.PlayerIndexType)
                {
                    case PlayerIndexType.A:
                        _playerAMannequinArrows.gameObject.SetActive(!hatInfo.HasConfirmed);
                        break;

                    case PlayerIndexType.B:
                        _playerBMannequinArrows.gameObject.SetActive(!hatInfo.HasConfirmed);
                        break;

                    case PlayerIndexType.C:
                        _playerCMannequinArrows.gameObject.SetActive(!hatInfo.HasConfirmed);
                        break;

                    case PlayerIndexType.D:
                        _playerDMannequinArrows.gameObject.SetActive(!hatInfo.HasConfirmed);
                        break;
                    case PlayerIndexType.Z:
                        break;
                }
            }
        }
    }
}
