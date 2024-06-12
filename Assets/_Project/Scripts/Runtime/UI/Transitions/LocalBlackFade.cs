using System;
using _Project.Scripts.Runtime.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    /// <summary>
    /// This class is used to do a black fade for when the menu scene is loaded for the first time
    /// Because the Transition Manager is not spawned yet
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LocalBlackFade : MonoBehaviour
    {
        [Required] public CanvasGroup CanvasGroup;

        private void Awake()
        {
            CanvasGroup.OpenInstant();
        }
    }   
}