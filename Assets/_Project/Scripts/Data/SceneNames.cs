using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Single source of truth cho ten cac Scene trong project.
    /// Dong thoi ghi log [NAV] moi lan chuyen Scene.
    /// </summary>
    public static class SceneNames
    {
        public const string MainMenu = "Main_Menu";
        public const string LevelSelect = "LevelSelect";
        public const string Gameplay = "Gameplay";

        /// <summary>
        /// Chuyen scene co log [NAV] tap trung.
        /// Ho tro ca Play Mode va Editor Mode cho test runner.
        /// </summary>
        public static void LoadScene(string sceneName, string reason = "")
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(currentScene))
            {
                currentScene = "None";
            }

            Debug.Log($"[NAV] LoadScene from={currentScene} to={sceneName} reason={reason}");

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                string scenePath = $"Assets/Scenes/{sceneName}.unity";
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
                return;
            }
#endif
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Kiem tra scene co the load duoc theo Build Settings hay khong.
        /// </summary>
        public static bool IsValidScene(string sceneName)
        {
            return Application.CanStreamedLevelBeLoaded(sceneName);
        }
    }
}
