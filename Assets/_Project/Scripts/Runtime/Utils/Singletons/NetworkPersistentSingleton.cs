using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts.Runtime.Utils.Singletons
{
    public class NetworkPersistentSingleton<T> : NetworkBehaviour where T : Component {
        public bool AutoUnparentOnAwake = true;

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
                        Logger.LogError("NetworkPersistentSingleton: " + typeof(T).Name + " has been auto-generated, its not normal, in can happen sometimes in the Editor, quit and play again !", context:instance);
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

            if (AutoUnparentOnAwake) {
                transform.SetParent(null);
            }

            if (instance == null) {
                instance = this as T;
                var networkObject = GetComponent<NetworkObject>();
                if (networkObject != null) {
                    if (networkObject.IsGlobal) return;
                    networkObject.SetIsGlobal(true);
                }
                else
                {
                    Logger.LogError("NetworkPersistentSingleton: " + typeof(T).Name + " does not have a NetworkObject component", context:this);
                }
            } else {
                if (instance != this) {
                    Destroy(gameObject);
                }
            }
        }
    }
}