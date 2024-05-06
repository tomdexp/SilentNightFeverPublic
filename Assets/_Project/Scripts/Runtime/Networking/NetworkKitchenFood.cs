using System.Collections;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Networking
{
    public class NetworkKitchenFood : NetworkConsumable
    {
        protected override void Consume(PlayerStickyTongue tongue)
        {
            Logger.LogDebug("Consuming food", context:this);
            StartCoroutine(ConsumeCoroutine(tongue));
        }
        
        private IEnumerator ConsumeCoroutine(PlayerStickyTongue tongue)
        {
            yield return new WaitForSeconds(2f);
            PlayerManager.Instance.TryGiveEffectToPlayer<PE_KitchenFood>(tongue.GetNetworkPlayer().GetPlayerIndexType());
        }
    }
}