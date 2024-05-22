using System.Collections;
using _Project.Scripts.Runtime.Landmarks.Components;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using FishNet.Object;
using Sirenix.OdinInspector;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Runtime.Landmarks
{
    public abstract class Landmark : NetworkBehaviour
    {
        [Title("Landmark Data Reference")]
        public LandmarkData Data;
        public virtual void SetValue<T>(float value, PlayerIndexType sourcePlayer) where T : ILandmarkComponent { }

        private void Awake()
        {
            if (!Data)
            {
                Logger.LogError($"Landmark {name} has no data assigned, please set the data in the prefab component inspector", context:this);
                return;
            }
            Logger.LogInfo($"Landmark {name} has been initialized", context:this);
        }

        private void Start()
        {
            if(IsServerStarted) StartCoroutine(TrySubscribeToGameManagerEvent());
        }

        private void OnDestroy()
        {
            if (IsServerStarted && GameManager.HasInstance)
            {
                GameManager.Instance.OnAnyRoundStarted -= ResetLandmark;
            }
        }

        private IEnumerator TrySubscribeToGameManagerEvent()
        {
            while (!GameManager.HasInstance)
            {
                yield return null;
            }
            GameManager.Instance.OnAnyRoundStarted += ResetLandmark;
            Logger.LogDebug($"Subscribed ResetLandmark to GameManager event for Landmark {name}", Logger.LogType.Server, context:this);
        }

        protected abstract void ResetLandmark(byte roundNumber);
    }
}