using _Project.Scripts.Runtime.Utils;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Runtime.UI.NetworkedMenu
{
    public static class UI
    {
        public static void GoToMenu<T>() where T : MenuBase
        {
            if (IsMenuV2SceneLoaded())
            {
                UIManager.Instance.GoToMenu<T>();
            }
            else
            {
                UILocalManager.Instance.GoToMenu<T>();
            }
        }
        
        public static bool IsMenuV2SceneLoaded()
        {
            int sceneCount = SceneManager.loadedSceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == SceneType.MenuV2Scene.ToString())
                {
                    return true;
                }
            }

            return false;
        }
    }
}