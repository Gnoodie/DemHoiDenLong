using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public static class FindNullSprites
    {
        private const string SceneGameplayPath = "Assets/Scenes/Gameplay.unity";
        private const string SprStarPath = "Assets/_Project/Art/Icons/star.png";
        private const string SprPillPath = "Assets/_Project/Sprites/UI/ui_pill.png";

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("DemHoiDenLong/Find Null Sprites")]
        public static void FindAndFixNullSprites()
        {
            Sprite starSpr = LoadSprite(SprStarPath);
            Sprite pillSpr = LoadSprite(SprPillPath);
            LanData defaultLan = LanCatalog.GetPlayableLan();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== KẾT QUẢ QUÉT FIND NULL SPRITES (DEMHOIDENLONG) ===");

            int totalChecked = 0;
            int solidColorCount = 0;
            int fixedCount = 0;
            List<string> fixedList = new List<string>();
            List<string> unhandledList = new List<string>();

            // 1. Quét Scene Gameplay
            var scene = EditorSceneManager.OpenScene(SceneGameplayPath, OpenSceneMode.Single);
            var sceneImages = Object.FindObjectsByType<Image>(FindObjectsInactive.Include);

            foreach (var img in sceneImages)
            {
                totalChecked++;
                if (img.sprite != null) continue;

                string fullPath = GetHierarchyPath(img.transform);
                if (IsIntentionalSolidColor(img))
                {
                    solidColorCount++;
                    continue;
                }

                // Kiểm tra xem có thuộc HUD, VictoryPanel, GameOverPanel không
                if (IsUnderHUDOrPanels(img.transform))
                {
                    bool fixedThis = false;
                    string objNameLower = img.gameObject.name.ToLower();

                    if (objNameLower.Contains("star"))
                    {
                        if (starSpr != null)
                        {
                            Undo.RecordObject(img, "Fix Star Sprite");
                            img.sprite = starSpr;
                            EditorUtility.SetDirty(img);
                            fixedThis = true;
                            fixedList.Add($"[SCENE FIXED] {fullPath} -> Gán star.png");
                        }
                    }
                    else if (objNameLower.Contains("button") || objNameLower.Contains("btn") || objNameLower.Contains("chip"))
                    {
                        if (pillSpr != null)
                        {
                            Undo.RecordObject(img, "Fix Button/Chip Sprite");
                            img.sprite = pillSpr;
                            img.type = Image.Type.Sliced;
                            EditorUtility.SetDirty(img);
                            fixedThis = true;
                            fixedList.Add($"[SCENE FIXED] {fullPath} -> Gán ui_pill.png (Sliced)");
                        }
                    }
                    else if (objNameLower.Contains("portrait") || objNameLower.Contains("avatar") || objNameLower.Contains("lan"))
                    {
                        if (defaultLan != null && defaultLan.sprite != null)
                        {
                            Undo.RecordObject(img, "Fix Lan Portrait Sprite");
                            img.sprite = defaultLan.sprite;
                            img.color = Color.white;
                            img.preserveAspect = true;
                            EditorUtility.SetDirty(img);
                            fixedThis = true;
                            fixedList.Add($"[SCENE FIXED] {fullPath} -> Gán LanData.sprite ({defaultLan.lanName})");
                        }
                    }

                    if (fixedThis) fixedCount++;
                    else unhandledList.Add($"[SCENE UNHANDLED] {fullPath} (Color=#{ColorUtility.ToHtmlStringRGBA(img.color)})");
                }
                else
                {
                    unhandledList.Add($"[SCENE OTHER] {fullPath} (Color=#{ColorUtility.ToHtmlStringRGBA(img.color)})");
                }
            }

            if (fixedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            // 2. Quét các Prefab UI trong Assets/_Project/Prefabs/UI/
            string[] uiPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/UI" });
            foreach (var guid in uiPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var prefabImages = prefab.GetComponentsInChildren<Image>(true);
                foreach (var img in prefabImages)
                {
                    totalChecked++;
                    if (img.sprite != null) continue;

                    string fullPath = $"{path}/{GetHierarchyPath(img.transform)}";
                    if (IsIntentionalSolidColor(img))
                    {
                        solidColorCount++;
                        continue;
                    }

                    unhandledList.Add($"[PREFAB] {fullPath} (Color=#{ColorUtility.ToHtmlStringRGBA(img.color)})");
                }
            }

            sb.AppendLine($"1. Tổng số Image đã kiểm tra: {totalChecked}");
            sb.AppendLine($"2. Khối màu đặc hợp lệ (Backdrop, Dim, Slider Fill, Overlay): {solidColorCount}");
            sb.AppendLine($"3. Đã tự động sửa (HUD, VictoryPanel, GameOverPanel): {fixedCount}");
            foreach (var f in fixedList) sb.AppendLine($"   - {f}");

            sb.AppendLine($"4. Vị trí khác có sprite null (chỉ liệt kê): {unhandledList.Count}");
            foreach (var u in unhandledList) sb.AppendLine($"   - {u}");

            Debug.Log(sb.ToString());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoadedRuntimeCheck()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene.name != "Gameplay") return;

            var hud = Object.FindAnyObjectByType<DemHoiDenLong.UI.GameplayHUD>();
            if (hud == null) return;

            var images = hud.GetComponentsInChildren<Image>(true);
            int nullCount = 0;
            foreach (var img in images)
            {
                if (img.sprite == null)
                {
                    if (IsIntentionalSolidColor(img)) continue;
                    nullCount++;
                    Debug.LogWarning($"[FindNullSprites-Runtime] Image '{img.gameObject.name}' trong {hud.gameObject.name} có sprite null lúc Play!");
                }
            }

            if (nullCount == 0)
            {
                Debug.Log("[FindNullSprites-Runtime] Toàn bộ Image Gameplay UI hợp lệ, không có sprite null bất thường lúc Play.");
            }
        }

        private static bool IsUnderHUDOrPanels(Transform t)
        {
            Transform cur = t;
            while (cur != null)
            {
                string n = cur.name;
                if (n == "GameplayHUD" || n == "TopHUD_Bar" || n == "PlayerStatsPanel" ||
                    n == "LanPortrait" || n == "HPSlider" || n == "LivesContainer" ||
                    n == "ObjectiveChip" || n == "StarsChip" ||
                    n == "VictoryPanel" || n == "GameOverPanel")
                {
                    return true;
                }
                cur = cur.parent;
            }
            return false;
        }

        private static bool IsIntentionalSolidColor(Image img)
        {
            string name = img.gameObject.name.ToLower();
            Transform parent = img.transform.parent;
            string parentName = parent != null ? parent.name.ToLower() : "";

            // Nền tối phủ màn hình, lớp mờ, dim, overlay
            if (name.Contains("overlay") || name.Contains("dim") || name.Contains("backdrop")) return true;
            if (name == "victorypanel" || name == "gameoverpanel" || name == "bosshudpanel") return true;

            // Thanh Fill / Background của Slider
            if (name == "fill" || name == "background")
            {
                if (parentName.Contains("slider") || parentName.Contains("area") || parentName.Contains("bar"))
                {
                    return true;
                }
            }

            // Cooldown overlay
            if (name.Contains("cooldown")) return true;

            return false;
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t.parent == null) return t.name;
            return GetHierarchyPath(t.parent) + "/" + t.name;
        }
    }
}
