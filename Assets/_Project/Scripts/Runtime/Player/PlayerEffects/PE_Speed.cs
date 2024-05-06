using System.Collections;
using _Project.Scripts.Runtime.Networking;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerEffects
{
    [CreateAssetMenu(fileName = nameof(PE_Speed), menuName = "Scriptable Objects/" + nameof(PE_Speed))]
    public class PE_Speed : PlayerEffect
    {
        [Title("PE_Speed Settings")]
        [SerializeField] private float _speedMultiplier = 1.5f;
        [SerializeField] private float _duration = 5f;
        
        private Coroutine _effectCoroutine;
        
        public override void ApplyEffect(NetworkPlayer player)
        {
            if (_effectCoroutine == null)
            {
                Logger.LogDebug("PE_Speed : Speed effect started", Logger.LogType.Client, this);
                _effectCoroutine = player.StartCoroutine(SpeedEffectCoroutine(player));
            }
        }
        
        private IEnumerator SpeedEffectCoroutine(NetworkPlayer player)
        {
            var playerData = player.PlayerData;
            var defaultPlayerMovementSpeed = playerData.PlayerMovementSpeed;
            playerData.PlayerMovementSpeed *= _speedMultiplier;
            yield return new WaitForSeconds(_duration);
            playerData.PlayerMovementSpeed = defaultPlayerMovementSpeed;
            Logger.LogDebug("PE_Speed : Speed effect ended", Logger.LogType.Client, this);
            _effectCoroutine = null;
        }
    }
}