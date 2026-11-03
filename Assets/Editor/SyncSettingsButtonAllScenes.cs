using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using DemHoiDenLong.UI;
using System.IO;

namespace DemHoiDenLong.EditorTools
{
    [InitializeOnLoad]
    public static class SyncSettingsButtonAllScenes
    {
        static SyncSettingsButtonAllScenes()
        {
            EditorApplication.update += RunSyncOnce;
        }

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            EditorApplication.update += RunSyncOnce;
        }

        private static bool hasExecuted = false;

        private static void RunSyncOnce()
        {
            if (hasExecuted) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            hasExecuted = true;
            EditorApplication.update -= RunSyncOnce;

            try
            {
                ExecuteSync();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SyncSettings] Exception: {ex}");
            }
        }

        [MenuItem("Tools/DemHoiDenLong/Đồng Bộ Nút Cài Đặt Mọi Màn Hình")]
        public static void ExecuteSync()
        {
            Sprite settingBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/setting_button.png");
            string btnPrefabPath = "Assets/_Project/Prefabs/UI/SettingsButton.prefab";
            string uiPrefabPath = "Assets/_Project/Prefabs/UI/SettingsUI.prefab";

            GameObject btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(btnPrefabPath);
            GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(uiPrefabPath);

            if (settingBtnSprite == null || btnPrefab == null || uiPrefab == null)
            {
                Debug.LogError("[SyncSettings] Missing assets: setting_button.png, SettingsButton.prefab, or SettingsUI.prefab");
                return;
            }

            // 1. Update SettingsButton.prefab
            GameObject btnInst = PrefabUtility.InstantiatePrefab(btnPrefab) as GameObject;
            RectTransform bRt = btnInst.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 1f);
            bRt.anchorMax = new Vector2(1f, 1f);
            bRt.pivot = new Vector2(1f, 1f);
            bRt.anchoredPosition = new Vector2(-28f, -28f);
            bRt.sizeDelta = new Vector2(64f, 64f);
            bRt.localScale = Vector3.one;

            Image btnImg = btnInst.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.sprite = settingBtnSprite;
                btnImg.color = Color.white;
                btnImg.preserveAspect = true;
                btnImg.raycastTarget = true;
            }

            if (btnInst.GetComponent<OpenSettingsButton>() == null)
            {
                btnInst.AddComponent<OpenSettingsButton>();
            }

            Transform textTr = btnInst.transform.Find("Text");
            if (textTr != null)
            {
                Object.DestroyImmediate(textTr.gameObject);
            }

            PrefabUtility.SaveAsPrefabAsset(btnInst, btnPrefabPath);
            Object.DestroyImmediate(btnInst);

            // Copy to Resources for runtime auto-instantiate
            string resPath = "Assets/Resources/UI";
            Directory.CreateDirectory(resPath);
            AssetDatabase.CopyAsset(uiPrefabPath, resPath + "/SettingsUI.prefab");
            AssetDatabase.CopyAsset(btnPrefabPath, resPath + "/SettingsButton.prefab");

            // 2. Sync Scenes
            string[] scenes = new string[] {
                "Assets/Scenes/Main_Menu.unity",
                "Assets/Scenes/LevelSelect.unity",
                "Assets/Scenes/Gameplay.unity"
            };

            string currentScene = EditorSceneManager.GetActiveScene().path;

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Canvas canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning($"[SyncSettings] No Canvas in {scenePath}");
                    continue;
                }

                Transform canvasTr = canvas.transform;
                Transform safeArea = canvasTr.Find("SafeAreaRoot") ?? canvasTr;

                // Ensure SettingsButton
                Transform existingBtn = safeArea.Find("SettingsButton") 
                                     ?? safeArea.Find("Button_Settings")
                                     ?? canvasTr.Find("SettingsButton")
                                     ?? canvasTr.Find("Button_Settings");

                GameObject buttonObj;
                if (existingBtn != null)
                {
                    buttonObj = existingBtn.gameObject;
                    buttonObj.name = "SettingsButton";
                    buttonObj.transform.SetParent(safeArea, true);
                }
                else
                {
                    buttonObj = PrefabUtility.InstantiatePrefab(btnPrefab, safeArea) as GameObject;
                    buttonObj.name = "SettingsButton";
                }

                RectTransform sRt = buttonObj.GetComponent<RectTransform>();
                sRt.anchorMin = new Vector2(1f, 1f);
                sRt.anchorMax = new Vector2(1f, 1f);
                sRt.pivot = new Vector2(1f, 1f);
                sRt.anchoredPosition = new Vector2(-28f, -28f);
                sRt.sizeDelta = new Vector2(64f, 64f);
                sRt.localScale = Vector3.one;

                Image sImg = buttonObj.GetComponent<Image>();
                if (sImg != null)
                {
                    sImg.sprite = settingBtnSprite;
                    sImg.color = Color.white;
                    sImg.preserveAspect = true;
                    sImg.raycastTarget = true;
                }

                if (buttonObj.GetComponent<OpenSettingsButton>() == null)
                {
                    buttonObj.AddComponent<OpenSettingsButton>();
                }

                Transform bText = buttonObj.transform.Find("Text");
                if (bText != null) Object.DestroyImmediate(bText.gameObject);

                // Ensure StarsChip does not overlap SettingsButton
                Transform starChip = safeArea.Find("StarsChip") ?? canvasTr.Find("StarsChip");
                if (starChip != null)
                {
                    RectTransform scRt = starChip.GetComponent<RectTransform>();
                    scRt.anchorMin = new Vector2(1f, 1f);
                    scRt.anchorMax = new Vector2(1f, 1f);
                    scRt.pivot = new Vector2(1f, 1f);
                    scRt.anchoredPosition = new Vector2(-114f, -40f);
                }

                // Ensure SettingsUI in scene
                Transform existingUI = canvasTr.Find("SettingsUI") ?? safeArea.Find("SettingsUI");
                if (existingUI == null)
                {
                    GameObject uiObj = PrefabUtility.InstantiatePrefab(uiPrefab, canvasTr) as GameObject;
                    uiObj.name = "SettingsUI";
                    RectTransform uiRt = uiObj.GetComponent<RectTransform>();
                    uiRt.anchorMin = new Vector2(0.5f, 0.5f);
                    uiRt.anchorMax = new Vector2(0.5f, 0.5f);
                    uiRt.pivot = new Vector2(0.5f, 0.5f);
                    uiRt.anchoredPosition = Vector2.zero;
                    uiObj.SetActive(false);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[SyncSettings] Successfully synced SettingsButton and SettingsUI in {scenePath}");
            }

            // Reopen initial scene if needed
            if (!string.IsNullOrEmpty(currentScene) && File.Exists(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SyncSettings] All scenes and prefabs have been synchronized with the golden gear SettingsButton!");
        }
    }
}
