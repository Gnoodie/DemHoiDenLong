using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DemHoiDenLong.EditorTools
{
    public static class InspectGameplayHierarchy
    {
        [MenuItem("DemHoiDenLong/Inspect Gameplay Hierarchy")]
        public static void Inspect()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== CANVAS HIERARCHY IN GAMEPLAY.UNITY ===");

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                sb.AppendLine("Canvas NOT FOUND!");
            }
            else
            {
                PrintTransform(canvas.transform, 0, sb);
            }

            Debug.Log(sb.ToString());
            System.IO.File.WriteAllText("D:/Dev_Game/DemHoiDenLong/Logs/gameplay_hierarchy.txt", sb.ToString(), Encoding.UTF8);
        }

        private static void PrintTransform(Transform t, int depth, StringBuilder sb)
        {
            string indent = new string(' ', depth * 2);
            RectTransform rt = t.GetComponent<RectTransform>();
            string rectInfo = "";
            if (rt != null)
            {
                rectInfo = $" [pos=({rt.anchoredPosition.x:F1}, {rt.anchoredPosition.y:F1}), size=({rt.sizeDelta.x:F1}, {rt.sizeDelta.y:F1}), ancMin=({rt.anchorMin.x:F2}, {rt.anchorMin.y:F2}), ancMax=({rt.anchorMax.x:F2}, {rt.anchorMax.y:F2}), piv=({rt.pivot.x:F2}, {rt.pivot.y:F2})]";
            }

            string compInfo = "";
            Image img = t.GetComponent<Image>();
            if (img != null)
            {
                string sprName = img.sprite != null ? img.sprite.name : "NULL";
                compInfo += $" [Image spr={sprName}, color=#{ColorUtility.ToHtmlStringRGBA(img.color)}, type={img.type}]";
            }
            TMPro.TMP_Text tmp = t.GetComponent<TMPro.TMP_Text>();
            if (tmp != null)
            {
                compInfo += $" [TMP text=\"{tmp.text}\", size={tmp.fontSize}]";
            }
            Slider slider = t.GetComponent<Slider>();
            if (slider != null)
            {
                compInfo += $" [Slider val={slider.value}/{slider.maxValue}]";
            }

            sb.AppendLine($"{indent}- {t.name} (active={t.gameObject.activeSelf}){rectInfo}{compInfo}");

            for (int i = 0; i < t.childCount; i++)
            {
                PrintTransform(t.GetChild(i), depth + 1, sb);
            }
        }
    }
}
