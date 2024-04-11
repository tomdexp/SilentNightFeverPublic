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
    }
}