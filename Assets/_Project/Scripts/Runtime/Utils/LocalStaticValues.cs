using UnityEditor;

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
        
        [InitializeOnEnterPlayMode]
        static void Reset()
        {
            Logger.LogDebug("Resetting LocalStaticValues");
            HasApplicationStartWwiseEventFired = false;
        }
    }
}