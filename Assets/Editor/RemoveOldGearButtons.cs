using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace DemHoiDenLong.EditorTools
{
    [InitializeOnLoad]
    public static class RemoveOldGearButtons
    {
        static RemoveOldGearButtons()
        {
            EditorApplication.update += RunCleanOnce;
        }

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            EditorApplication.update += RunCleanOnce;
        }

        private static bool hasExecuted = false;

        private static void RunCleanOnce()
        {
            if (hasExecuted) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            hasExecuted = true;
            EditorApplication.update -= RunCleanOnce;

            try
            {
                CleanAllOldGearButtons();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RemoveOldGearButtons] Exception: {ex}");
            }
        }

        [MenuItem("Tools/DemHoiDenLong/Xóa Các Nút Setting Cũ (Gear Icon)")]
        public static void CleanAllOldGearButtons()
        {
            Sprite oldGearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/gear.png");
            string[] scenes = new string[] {
                "Assets/Scenes/Main_Menu.unity",
                "Assets/Scenes/LevelSelect.unity",
                "Assets/Scenes/Gameplay.unity"
            };

            string currentScene = EditorSceneManager.GetActiveScene().path;
            int totalRemoved = 0;

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                List<GameObject> toDestroy = new List<GameObject>();

                foreach (var img in allImages)
                {
                    if (img == null) continue;

                    bool isOldGear = false;
                    if (img.sprite != null)
                    {
                        string path = AssetDatabase.GetAssetPath(img.sprite);
                        if (img.sprite == oldGearSprite || path.Contains("Art/Icons/gear.png") || img.sprite.name.ToLower() == "gear")
                        {
                            isOldGear = true;
                        }
                    }

                    if (isOldGear)
                    {
                        // Check if this object or its parent is the button
                        GameObject target = img.gameObject;
                        if (target.transform.parent != null && target.name == "Icon")
                        {
                            target = target.transform.parent.gameObject;
                        }
                        else if (target.GetComponent<Button>() == null && target.transform.parent != null && target.transform.parent.GetComponent<Button>() != null)
                        {
                            target = target.transform.parent.gameObject;
                        }

                        if (!toDestroy.Contains(target))
                        {
                            toDestroy.Add(target);
                        }
                    }
                }

                foreach (var go in toDestroy)
                {
                    Debug.Log($"[RemoveOldGearButtons] Destroying old gear setting button: '{go.name}' in scene: {scene.name}");
                    Object.DestroyImmediate(go);
                    totalRemoved++;
                }

                if (toDestroy.Count > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            // Also check all prefabs in Assets/_Project/Prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/_Project/Prefabs" });
            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                Image[] imgs = prefab.GetComponentsInChildren<Image>(true);
                bool modified = false;

                foreach (var img in imgs)
                {
                    if (img == null || img.sprite == null) continue;
                    string sPath = AssetDatabase.GetAssetPath(img.sprite);
                    if (img.sprite == oldGearSprite || sPath.Contains("Art/Icons/gear.png") || img.sprite.name.ToLower() == "gear")
                    {
                        GameObject rootInst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                        Image[] instImgs = rootInst.GetComponentsInChildren<Image>(true);
                        foreach (var i in instImgs)
                        {
                            if (i.sprite == oldGearSprite || (i.sprite != null && i.sprite.name.ToLower() == "gear"))
                            {
                                GameObject t = i.gameObject;
                                if (t.transform.parent != null && (t.name == "Icon" || t.transform.parent.GetComponent<Button>() != null))
                                {
                                    t = t.transform.parent.gameObject;
                                }
                                Debug.Log($"[RemoveOldGearButtons] Removing old gear button from prefab {path}: {t.name}");
                                Object.DestroyImmediate(t);
                                modified = true;
                                totalRemoved++;
                                break;
                            }
                        }

                        if (modified)
                        {
                            PrefabUtility.SaveAsPrefabAsset(rootInst, path);
                        }
                        Object.DestroyImmediate(rootInst);
                        break;
                    }
                }
            }

            // Reopen active scene
            if (!string.IsNullOrEmpty(currentScene) && System.IO.File.Exists(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[RemoveOldGearButtons] Done! Removed {totalRemoved} old gear setting button(s).");
        }
    }
}
