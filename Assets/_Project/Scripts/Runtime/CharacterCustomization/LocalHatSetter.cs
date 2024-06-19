using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.CharacterCustomization
{
    public class LocalHatSetter : MonoBehaviour
    {
        [SerializeField] private PlayerIndexType _playerIndexType = PlayerIndexType.Z;
        [SerializeField, Required] private GameObject _hatContainer;
        
        private MeshRenderer[] _hats;
        private int _currentHatIndex;
        
        private void Awake()
        {
            if (!_hatContainer)
            {
                Logger.LogError("HatSetter is missing a reference to the HatContainer GameObject !", Logger.LogType.Local, this);
                return;
            }

            _hats = _hatContainer.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        }

        private void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                UpdateHat();
            }
        }

        private void UpdateHat()
        {
            if (!PlayerManager.HasInstance)
            {
                DisableHats();
                return;
            }
            
            var hatInfos = PlayerManager.Instance.GetPlayerHatInfos();
            
            if (hatInfos.Count == 0)
            {
                DisableHats();
                return; 
            }
            
            foreach (var hatInfo in hatInfos)
            {
                if (hatInfo.PlayerIndexType == _playerIndexType)
                {
                    SetHatByName(hatInfo.PlayerHatType.ToString());
                }
            }
        }
        
        private void SetHatByName(string hatName)
        {
            if (_hats.IsNullOrEmpty())
            {
                Logger.LogWarning("No hats found in the hat container", Logger.LogType.Local, this);
                return;
            } 

            DisableHats();

            foreach (var hat in _hats)
            {
                if (hat.name == hatName)
                {
                    hat.gameObject.SetActive(true);
                    //Logger.LogTrace("Setting hat: " + hatName, Logger.LogType.Local, this);
                    return;
                }
            }
        }

        private void DisableHats()
        {
            foreach (var hat in _hats)
            {
                hat.gameObject.SetActive(false);
                //Logger.LogTrace($"Disabling hat {hat.name}", Logger.LogType.Local, this);
            }
            //Logger.LogTrace("Disabling all hats", Logger.LogType.Client, this);
        }
    }
}