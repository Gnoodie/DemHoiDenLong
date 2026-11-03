using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DemHoiDenLong.EditorTools
{
    public static class FindLivesContainer
    {
        [MenuItem("DemHoiDenLong/Find Lives Container")]
        public static void FindIt()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Debug.Log("--- Canvas Children Dump ---");
            DumpChildren(canvas.transform, "");
        }

        private static void DumpChildren(Transform t, string indent)
        {
            Debug.Log($"{indent}- {t.name} (active={t.gameObject.activeSelf})");
            for (int i = 0; i < t.childCount; i++)
            {
                DumpChildren(t.GetChild(i), indent + "  ");
            }
        }
    }
}
