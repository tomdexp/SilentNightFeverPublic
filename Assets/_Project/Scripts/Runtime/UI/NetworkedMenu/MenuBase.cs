using System;
using System.Collections;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public abstract class MenuBase : MonoBehaviour
    {
        public abstract string MenuName { get; }

        private IEnumerator Start()
        {
            while(!UIManager.HasInstance) yield return null;
            UIManager.Instance.RegisterMenu(this);
        }

        public virtual void Open()
        {
            Logger.LogTrace($"Opening {MenuName}", Logger.LogType.Client,this);
        }

        public virtual void Close()
        {
            Logger.LogTrace($"Closing {MenuName}", Logger.LogType.Client,this);
        }

        public virtual void GoBack()
        {
            Logger.LogTrace($"Going back from {MenuName}", Logger.LogType.Client,this);
        }
    }
}