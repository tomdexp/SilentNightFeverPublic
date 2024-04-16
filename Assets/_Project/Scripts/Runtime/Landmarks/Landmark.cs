using _Project.Scripts.Runtime.Landmarks.Components;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Landmarks
{
    public abstract class Landmark : NetworkBehaviour
    {
        public virtual void SetValue<T>(float value, PlayerIndexType sourcePlayer) where T : ILandmarkComponent
        {
            
        }
    }
}