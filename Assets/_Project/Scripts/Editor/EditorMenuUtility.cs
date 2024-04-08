using _Project.Scripts.Runtime.Player;
using UnityEditor;
using UnityEngine;
using System.Reflection;

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
                Debug.LogError("PlayerData could not be loaded. Please make sure the path is correct.");
            }
        }
    }
}