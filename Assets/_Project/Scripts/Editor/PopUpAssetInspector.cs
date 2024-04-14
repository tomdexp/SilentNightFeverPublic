using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Editor
{
    public class PopUpAssetInspector : EditorWindow
    {
        private Object _asset;
        private UnityEditor.Editor _assetEditor;
   
        public static PopUpAssetInspector Create(Object asset)
        {
            var window = CreateWindow<PopUpAssetInspector>($"{asset.name} (type:{asset.GetType().Name})");
            window._asset = asset;
            window._assetEditor = UnityEditor.Editor.CreateEditor(asset);
            return window;
        }
 
        private void OnGUI()
        {
            GUI.enabled = false;
            _asset = EditorGUILayout.ObjectField("Asset", _asset, _asset.GetType(), false);
            GUI.enabled = true;
 
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _assetEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
    }
}