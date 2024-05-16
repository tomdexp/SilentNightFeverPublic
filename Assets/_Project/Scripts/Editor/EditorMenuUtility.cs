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
        private static void OpenPlayerData()
        {
            // Load the PlayerData asset
            PlayerData playerData = AssetDatabase.LoadAssetAtPath<PlayerData>("Assets/_Project/Settings/PlayerData.asset");
            if (playerData != null)
            {
                PopUpAssetInspector.Create(playerData);
            }
            else
            {
                Logger.LogError("PlayerData could not be loaded. Please make sure the path is correct.");
            }
        }

        [MenuItem("Silent Night Fever/Open LandmarkData_Kitchen")]
        private static void OpenLandmarkKitchenData()
        {
            // Load the PlayerData asset
            LandmarkData_Kitchen data = AssetDatabase.LoadAssetAtPath<LandmarkData_Kitchen>("Assets/_Project/Settings/Resources/LandmarkDatas/LandmarkData_Kitchen.asset");
            if (data != null)
            {
                PopUpAssetInspector.Create(data);
            }
            else
            {
                Logger.LogError("LandmarkData_Kitchen could not be loaded. Please make sure the path is correct.");
            }
        }
        
        [MenuItem("Silent Night Fever/Open AudioManagerData")]
        private static void OpenAudioManagerData()
        {
            // Load the PlayerData asset
            AudioManagerData data = AssetDatabase.LoadAssetAtPath<AudioManagerData>("Assets/_Project/Settings/AudioManagerData.asset");
            if (data != null)
            {
                PopUpAssetInspector.Create(data);
            }
            else
            {
                Logger.LogError("AudioManagerData could not be loaded. Please make sure the path is correct.");
            }
        }
    }
}