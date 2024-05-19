using System.Collections;
using Mono.CSharp;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;

namespace _Project.Scripts.Editor
{
    public static class OnlineSetupVerifier
    {
        [MenuItem("GameObject/Silent Night Fever/Verify Online Setup", false, 0)]
        public static void Verify()
        {
            EditorCoroutineUtility.StartCoroutine(VerifyCoroutine(), typeof(OnlineSetupVerifier));
        }
        
        private static IEnumerator VerifyCoroutine()
        {
            Logger.LogInfo("Verifying Online Setup...");
            ShowSceneNotification("Verifying Online Setup...");
            bool hasNetworkManagersSpawnerPrefab = GameObject.Find("NetworkedManagersSpawner (MUST HAVE)");
            bool hasNetworkManagerSNFVariantPrefab = GameObject.Find("NetworkManager (SNF Variant)");
            bool hasBootstrapPrefab = GameObject.Find("BootstrapManager [Local]");
            yield return new EditorWaitForSeconds(2f);
            if (hasNetworkManagersSpawnerPrefab && hasNetworkManagerSNFVariantPrefab && hasBootstrapPrefab)
            {
                Logger.LogInfo("Online Setup is correct!");
                ShowSceneNotification("Online Setup is correct!");
            }
            else
            {
                Logger.LogError("Online Setup is incorrect! Please make sure you have the following prefabs in your scene: NetworkedManagersSpawner (MUST HAVE), NetworkManager (SNF Variant), BootstrapManager [Local]");
                ShowSceneNotification("Online Setup is incorrect! Please make sure you have the following prefabs in your scene: NetworkedManagersSpawner (MUST HAVE), NetworkManager (SNF Variant), BootstrapManager [Local]");
            }
            yield return null;
        }
        
        public static void ShowSceneNotification(string message)
        {
            foreach( SceneView scene in SceneView.sceneViews ) {
                scene.ShowNotification( new GUIContent( message ) );
            }
        }
    }
}