using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DemHoiDenLong.Editor
{
    public static class FindMissingGlyphs
    {
        [MenuItem("DemHoiDenLong/Find Missing Glyphs")]
        public static void ScanMissingGlyphs()
        {
            var results = new List<string>();
            int missingCount = 0;

            // 1. Scan loaded scenes
            for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var tmps = root.GetComponentsInChildren<TMP_Text>(true);
                    foreach (var tmp in tmps)
                    {
                        if (CheckTmp(tmp, $"Scene '{scene.name}' / {GetHierarchyPath(tmp.transform)}", out string info))
                        {
                            results.Add(info);
                            missingCount++;
                        }
                    }
                }
            }

            // 2. Scan prefabs in Assets/_Project
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var tmps = prefab.GetComponentsInChildren<TMP_Text>(true);
                foreach (var tmp in tmps)
                {
                    if (CheckTmp(tmp, $"Prefab '{path}' / {GetHierarchyPath(tmp.transform)}", out string info))
                    {
                        results.Add(info);
                        missingCount++;
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== BÁO CÁO TÌM GLYPH THIẾU (FIND MISSING GLYPHS) ===");
            sb.AppendLine($"Tổng số đối tượng TMP thiếu glyph: {missingCount}");
            if (missingCount == 0)
            {
                sb.AppendLine("Không tìm thấy ký tự nào thiếu glyph trong LiberationSans SDF!");
            }
            else
            {
                foreach (var res in results)
                {
                    sb.AppendLine(res);
                }
            }

            Debug.Log(sb.ToString());
            try
            {
                System.IO.File.WriteAllText("missing_glyphs_report.txt", sb.ToString());
            }
            catch { }
        }

        private static bool CheckTmp(TMP_Text tmp, string location, out string info)
        {
            info = null;
            if (tmp == null || string.IsNullOrEmpty(tmp.text)) return false;

            TMP_FontAsset font = tmp.font != null ? tmp.font : TMP_Settings.defaultFontAsset;
            if (font == null) return false;

            var missingChars = new HashSet<char>();
            string text = tmp.text;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c) || c == '\n' || c == '\r' || c == '\t') continue;

                if (!font.HasCharacter(c, searchFallbacks: true, tryAddCharacter: true))
                {
                    missingChars.Add(c);
                }
            }

            if (missingChars.Count > 0)
            {
                var charList = new List<string>();
                foreach (char c in missingChars)
                {
                    charList.Add($"'{c}' (U+{(int)c:X4})");
                }
                info = $"[MISSING] {location}\n  Text: \"{text}\"\n  Missing: {string.Join(", ", charList)} (Font: {font.name})";
                return true;
            }

            return false;
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return string.Empty;
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
