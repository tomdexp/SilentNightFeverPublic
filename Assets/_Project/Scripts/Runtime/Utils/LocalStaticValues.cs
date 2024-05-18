# if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Project.Scripts.Runtime.Utils
{
    public class LocalStaticValues
    {
        // Used to avoid firing the OnApplicationStart event multiple times for the AudioManager
        public static bool HasApplicationStartWwiseEventFired = false;
        
        static LocalStaticValues()
        {
            Reset();
        }
        
#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
#endif
        static void Reset()
        {
            Logger.LogDebug("Resetting LocalStaticValues");
            HasApplicationStartWwiseEventFired = false;
        }
    }
}