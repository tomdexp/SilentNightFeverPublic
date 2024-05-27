using _Project.Scripts.Runtime.Audio;
using _Project.Scripts.Runtime.Landmarks.Kitchen;
using _Project.Scripts.Runtime.Player;
using UnityEditor;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Editor
{
    public static class EditorMenuUtility
    {
        [MenuItem("Silent Night Fever/Open PlayerData")]
        private static void GoToPlayerData()
        {
            GoToData("Assets/_Project/Settings/PlayerData.asset");
        }
        
        [MenuItem("Silent Night Fever/Open AudioManagerData")]
        private static void GoToAudioManagerData()
        {
            GoToData("Assets/_Project/Settings/AudioManagerData.asset");
        }
        
        [MenuItem("Silent Night Fever/Build")]
        private static void GoToBuildFolder()
        {
            GoToData("Assets/_Project/Settings/BuildConfigs/Full_Release.buildconfiguration");
        }

        [MenuItem("Silent Night Fever/Open LandmarksData")]
        private static void GoToLandmarksData()
        {
            GoToData("Assets/_Project/Settings/Resources/LandmarkDatas/LandmarkData_Kitchen.asset");
        }

        private static void GoToData(string path)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}