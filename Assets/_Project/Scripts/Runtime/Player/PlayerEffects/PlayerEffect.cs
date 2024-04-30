using _Project.Scripts.Runtime.Networking;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Player.PlayerEffects
{ 
    public abstract class PlayerEffect : ScriptableObject
    {
        public abstract void ApplyEffect(NetworkPlayer player);
    }
}