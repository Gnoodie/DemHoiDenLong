using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    [InitializeOnLoad]
    public static class FixSettingsUI
    {
        static FixSettingsUI()
        {
            EditorApplication.update += RunFixOnce;
        }

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            EditorApplication.update += RunFixOnce;
        }

        private static bool hasExecuted = false;

        private static void RunFixOnce()
        {
            if (hasExecuted) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            hasExecuted = true;
            EditorApplication.update -= RunFixOnce;

            try
            {
                Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FixSettingsUI] Exception: {ex}");
            }
        }

        [MenuItem("Tools/DemHoiDenLong/Fix Settings Sliders and Prefab")]
        public static void Execute()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/SettingsUI.prefab";
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("Cannot find SettingsUI.prefab at: " + prefabPath);
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;

            // 1. Remove Shake_Row if it exists in prefab
            Transform shakeRow = instance.transform.Find("Shake_Row");
            if (shakeRow != null)
            {
                Object.DestroyImmediate(shakeRow.gameObject);
                Debug.Log("[FixSettingsUI] Removed Shake_Row from prefab.");
            }

            // 2. Load sprites for sliders
            Sprite sliderBack = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Slider_back.png");
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Slider_fill.png");
            Sprite sliderHandle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Slider_handle.png");

            // 3. Inspect and normalize BGM_Row and SFX_Row
            Transform bgmRow = instance.transform.Find("BGM_Row");
            Transform sfxRow = instance.transform.Find("SFX_Row");

            if (bgmRow != null)
            {
                RectTransform rt = bgmRow.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 85f);
                rt.sizeDelta = new Vector2(700f, 120f);
                rt.localScale = Vector3.one;

                NormalizeRow(bgmRow, sliderBack, sliderFill, sliderHandle, "BGM_Slider");
            }

            if (sfxRow != null)
            {
                RectTransform rt = sfxRow.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -85f);
                rt.sizeDelta = new Vector2(700f, 120f);
                rt.localScale = Vector3.one;

                NormalizeRow(sfxRow, sliderBack, sliderFill, sliderHandle, "SFX_Slider");
            }

            // 4. Reposition ExitButton and VersionText
            Transform exitTr = instance.transform.Find("ExitButton");
            if (exitTr != null)
            {
                RectTransform rt = exitTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 115f);
                rt.sizeDelta = new Vector2(360f, 76f);

                var txt = exitTr.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                if (txt != null)
                {
                    txt.enableWordWrapping = false;
                    txt.fontSize = 28f;
                }
            }

            Transform verTr = instance.transform.Find("VersionText");
            if (verTr != null)
            {
                RectTransform rt = verTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 36f);
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            Debug.Log("[FixSettingsUI] Successfully updated SettingsUI.prefab with identical slider dimensions, preserved aspect ratio for handle, and single-line exit button!");
        }

        private static void NormalizeRow(Transform row, Sprite bgSpr, Sprite fillSpr, Sprite handleSpr, string expectedSliderName)
        {
            // 1. Icon
            Transform iconTr = row.Find("Icon");
            if (iconTr != null)
            {
                RectTransform iRt = iconTr.GetComponent<RectTransform>();
                iRt.anchorMin = new Vector2(0.5f, 0.5f);
                iRt.anchorMax = new Vector2(0.5f, 0.5f);
                iRt.pivot = new Vector2(0.5f, 0.5f);
                iRt.anchoredPosition = new Vector2(-260f, 0f);
                iRt.sizeDelta = new Vector2(100f, 100f);
                iRt.localScale = Vector3.one;

                Image img = iconTr.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    img.preserveAspect = true;
                }

                Button btn = iconTr.GetComponent<Button>();
                if (btn != null)
                {
                    btn.transition = Selectable.Transition.None;
                    var nav = btn.navigation;
                    nav.mode = Navigation.Mode.None;
                    btn.navigation = nav;
                }
            }

            // 2. Slider
            Slider slider = row.GetComponentInChildren<Slider>(true);
            if (slider != null)
            {
                slider.gameObject.name = expectedSliderName;
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.wholeNumbers = false;

                RectTransform sRt = slider.GetComponent<RectTransform>();
                sRt.anchorMin = new Vector2(0.5f, 0.5f);
                sRt.anchorMax = new Vector2(0.5f, 0.5f);
                sRt.pivot = new Vector2(0.5f, 0.5f);
                sRt.anchoredPosition = new Vector2(65f, 0f);
                sRt.sizeDelta = new Vector2(440f, 40f);
                sRt.localScale = Vector3.one;

                // Background
                Transform bgTr = slider.transform.Find("Background");
                if (bgTr != null)
                {
                    RectTransform bgRt = bgTr.GetComponent<RectTransform>();
                    bgRt.anchorMin = Vector2.zero;
                    bgRt.anchorMax = Vector2.one;
                    bgRt.offsetMin = Vector2.zero;
                    bgRt.offsetMax = Vector2.zero;
                    bgRt.sizeDelta = Vector2.zero;
                    bgRt.localScale = Vector3.one;

                    Image bgImg = bgTr.GetComponent<Image>();
                    if (bgImg != null)
                    {
                        if (bgSpr != null) bgImg.sprite = bgSpr;
                        bgImg.type = Image.Type.Sliced;
                        bgImg.color = Color.white;
                    }
                }

                // Fill Area
                Transform faTr = slider.transform.Find("Fill Area");
                if (faTr != null)
                {
                    RectTransform faRt = faTr.GetComponent<RectTransform>();
                    faRt.anchorMin = Vector2.zero;
                    faRt.anchorMax = Vector2.one;
                    faRt.offsetMin = Vector2.zero;
                    faRt.offsetMax = Vector2.zero;
                    faRt.sizeDelta = Vector2.zero;
                    faRt.localScale = Vector3.one;

                    Transform fillTr = faTr.Find("Fill");
                    if (fillTr != null)
                    {
                        Image fImg = fillTr.GetComponent<Image>();
                        if (fImg != null)
                        {
                            if (fillSpr != null) fImg.sprite = fillSpr;
                            fImg.type = Image.Type.Sliced;
                            fImg.color = Color.white;
                        }
                    }
                }

                // Handle Slide Area
                Transform hsaTr = slider.transform.Find("Handle Slide Area");
                if (hsaTr != null)
                {
                    RectTransform hsaRt = hsaTr.GetComponent<RectTransform>();
                    hsaRt.anchorMin = Vector2.zero;
                    hsaRt.anchorMax = Vector2.one;
                    hsaRt.offsetMin = new Vector2(20f, 0f);
                    hsaRt.offsetMax = new Vector2(-20f, 0f);
                    hsaRt.sizeDelta = new Vector2(-40f, 0f);
                    hsaRt.localScale = Vector3.one;

                    Transform handleTr = hsaTr.Find("Handle");
                    if (handleTr != null)
                    {
                        RectTransform hRt = handleTr.GetComponent<RectTransform>();
                        hRt.pivot = new Vector2(0.5f, 0.5f);
                        hRt.sizeDelta = new Vector2(80f, 40f);
                        hRt.localScale = Vector3.one;

                        Image hImg = handleTr.GetComponent<Image>();
                        if (hImg != null)
                        {
                            if (handleSpr != null) hImg.sprite = handleSpr;
                            hImg.color = Color.white;
                            hImg.preserveAspect = true; // Key protection against squishing!
                        }
                    }
                }
            }
        }
    }
}
