using System;
using _Project.Scripts.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Runtime.Inputs
{
    /// <summary>
    /// This interface is used to provide input to the game, it is used to decouple the input from the game
    /// Its an Interface because it can be attached to a MonoBehaviour or a NetworkBehaviour
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// When the action starts, it almost the same as performed, but unless you have good reason to use this, use the performed event
        /// </summary>
        public event Action<InputAction.CallbackContext> OnActionInteractStarted;
        
        /// <summary>
        /// When the action is performed, most of the time you want to use this instead of started
        /// </summary>
        public event Action<InputAction.CallbackContext> OnActionInteractPerformed;
        
        /// <summary>
        /// When the action is canceled, this is useful for when you want to cancel an action, like when you want to cancel a charge
        /// </summary>
        public event Action<InputAction.CallbackContext> OnActionInteractCanceled;
        public event Action OnActionPausePerformed;
        
        /// <summary>
        /// To Get the movement input, scripts should provide their own implementation
        /// </summary>
        public Vector2 GetMovementInput();
        
        public void SetRealPlayerInfo(RealPlayerInfo realPlayerInfo);
    }
}