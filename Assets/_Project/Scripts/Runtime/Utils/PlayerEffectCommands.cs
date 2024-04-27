using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Player.PlayerEffects;
using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils
{
    public static class PlayerEffectCommands
    {
        [Command("/effect.give.speed", "Give speed effect to the player.")]
        public static void GiveEffect_PE_Speed(PlayerIndexType player)
        {
            PlayerManager.Instance.TryGiveEffectToPlayer<PE_Speed>(player);
        }
    }
}