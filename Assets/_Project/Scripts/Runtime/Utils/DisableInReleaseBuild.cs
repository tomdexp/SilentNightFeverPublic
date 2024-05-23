using UnityEngine;

namespace _Project.Scripts.Runtime.Utils
{
    public class DisableInReleaseBuild : MonoBehaviour
    {
        private void Awake()
        {
            if (Debug.isDebugBuild) return;
            Logger.LogTrace($"Disabling GameObject ({gameObject.name} in Release Build", Logger.LogType.Local, this);
            gameObject.SetActive(false);
        }
    }
}