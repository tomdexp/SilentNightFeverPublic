using _Project.Scripts.Runtime.Utils;
using FishNet.Object;

namespace _Project.Scripts.Runtime.Networking
{
    public class NetworkKitchenFood : NetworkConsumable
    {
        protected override void Consume()
        {
            Logger.LogDebug("Consuming food", context:this);
            TryApplyFoodEffect();
        }

        private void TryApplyFoodEffect()
        {
            if (IsServerStarted)
            {
                ApplyEffect();
            }
            ApplyEffectServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ApplyEffectServerRpc()
        {
            ApplyEffect();
        }

        private void ApplyEffect()
        {
            Logger.LogDebug("Applying kitchen food effect",Logger.LogType.Server, context:this);
            
        }
    }
}