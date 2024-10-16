﻿using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils.Singletons {
    public class NetworkSingleton<T> : NetworkBehaviour where T : Component {
        protected static T instance;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        var go = new GameObject(typeof(T).Name + " Auto-Generated");
                        instance = go.AddComponent<T>();
                        Logger.LogError("NetworkSingleton: " + typeof(T).Name + " has been auto-generated, its not normal, in can happen sometimes in the Editor, quit and play again !", context:instance);
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake() {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton() {
            if (!Application.isPlaying) return;

            instance = this as T;
        }
    }
}