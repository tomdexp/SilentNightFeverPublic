using System.Collections;
using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using _Project.Scripts.Runtime.Player.PlayerTongue;
using FishNet;
using FishNet.Object;
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
            yield return new WaitForSeconds(1f);
            PlayAudioServerRpc();
            yield return new WaitForSeconds(1f);
            PlayerManager.Instance.TryGiveEffectToPlayer<PE_KitchenFood>(tongue.GetNetworkPlayer().GetPlayerIndexType());
            DespawnServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRpc()
        {
            if (IsServerStarted)
            {
                // TODO: This might not play well because it destroys the object, I don't know how Wwise handles this
                Despawn();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayAudioServerRpc()
        {
            AudioManager.Instance.PlayAudioNetworked(AudioManager.Instance.AudioManagerData.EventLandmarkKitchenFoodEaten, gameObject);
        }
        
    }
}