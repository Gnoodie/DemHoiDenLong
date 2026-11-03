using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.UI;
using System.IO;

public class CreateSettingsUI
{
    [MenuItem("Tools/DemHoiDenLong/Setup Settings UI")]
    public static void CreateSettingsPrefab()
    {
        string[] imagePaths = new string[] {
            "Assets/_Project/Art/UI/settingpanel.png",
            "Assets/_Project/Art/UI/Slider_back.png",
            "Assets/_Project/Art/UI/Slider_fill.png",
            "Assets/_Project/Art/UI/Slider_handle.png",
            "Assets/_Project/Art/UI/icon_nhac.png",
            "Assets/_Project/Art/UI/icon_loa.png",
            "Assets/_Project/Art/UI/icon_close.png"
        };

        // Ensure all textures are sprites
        foreach (string path in imagePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }
        }

        string prefabPath = "Assets/_Project/Prefabs/UI/SettingsUI.prefab";

        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[0]);
        Sprite sliderBackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[1]);
        Sprite sliderFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[2]);
        Sprite sliderHandleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[3]);
        Sprite iconNhac = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[4]);
        Sprite iconLoa = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[5]);
        Sprite iconClose = AssetDatabase.LoadAssetAtPath<Sprite>(imagePaths[6]);

        if (panelSprite == null) { Debug.LogError("Cannot find settingpanel.png"); return; }

        GameObject root = new GameObject("SettingsUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SettingsUI));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(800, 1000);
        Image rootImage = root.GetComponent<Image>();
        rootImage.sprite = panelSprite;
        rootImage.type = Image.Type.Sliced;
        
        SettingsUI settingsScript = root.GetComponent<SettingsUI>();

        // Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(root.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -80);
        titleRect.sizeDelta = new Vector2(400, 120);
        
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "Cài Đặt";
        titleText.fontSize = 70;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color32(250, 201, 56, 255); // #fac938

        // BGM Row
        GameObject bgmRow = new GameObject("BGM_Row", typeof(RectTransform));
        bgmRow.transform.SetParent(root.transform, false);
        RectTransform bgmRect = bgmRow.GetComponent<RectTransform>();
        bgmRect.anchoredPosition = new Vector2(0, 120);
        
        GameObject bgmIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        bgmIcon.transform.SetParent(bgmRow.transform, false);
        bgmIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-280, 0);
        bgmIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        bgmIcon.GetComponent<Image>().sprite = iconNhac;

        Slider bgmSlider = CreateSlider("BGM_Slider", bgmRow.transform, new Vector2(60, 0), sliderBackSprite, sliderFillSprite, sliderHandleSprite);
        settingsScript.GetType().GetField("bgmSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(settingsScript, bgmSlider);

        // SFX Row
        GameObject sfxRow = new GameObject("SFX_Row", typeof(RectTransform));
        sfxRow.transform.SetParent(root.transform, false);
        RectTransform sfxRect = sfxRow.GetComponent<RectTransform>();
        sfxRect.anchoredPosition = new Vector2(0, -120);
        
        GameObject sfxIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        sfxIcon.transform.SetParent(sfxRow.transform, false);
        sfxIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-280, 0);
        sfxIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        sfxIcon.GetComponent<Image>().sprite = iconLoa;

        Slider sfxSlider = CreateSlider("SFX_Slider", sfxRow.transform, new Vector2(60, 0), sliderBackSprite, sliderFillSprite, sliderHandleSprite);
        settingsScript.GetType().GetField("sfxSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(settingsScript, sfxSlider);

        // Close Button
        GameObject closeObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(root.transform, false);
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-40, -40);
        closeRect.sizeDelta = new Vector2(100, 100);
        closeObj.GetComponent<Image>().sprite = iconClose;
        Button closeBtn = closeObj.GetComponent<Button>();
        settingsScript.GetType().GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(settingsScript, closeBtn);

        // Save Prefab
        Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        
        Debug.Log("SettingsUI Prefab created successfully at: " + prefabPath);
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 pos, Sprite bg, Sprite fill, Sprite handle)
    {
        DefaultControls.Resources uiResources = new DefaultControls.Resources();
        GameObject sliderObj = DefaultControls.CreateSlider(uiResources);
        sliderObj.name = name;
        sliderObj.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = pos;
        sliderRect.sizeDelta = new Vector2(440, 40);

        Slider slider = sliderObj.GetComponent<Slider>();
        
        // Background
        Image bgImg = sliderObj.transform.Find("Background").GetComponent<Image>();
        bgImg.sprite = bg;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = Color.white;

        // Fill Area adjustments
        RectTransform fillAreaRect = sliderObj.transform.Find("Fill Area").GetComponent<RectTransform>();
        fillAreaRect.sizeDelta = Vector2.zero; // Make sure it stretches properly
        
        Image fillImg = fillAreaRect.Find("Fill").GetComponent<Image>();
        fillImg.sprite = fill;
        fillImg.type = Image.Type.Sliced;
        fillImg.color = Color.white;

        // Handle
        Image handleImg = sliderObj.transform.Find("Handle Slide Area/Handle").GetComponent<Image>();
        handleImg.sprite = handle;
        handleImg.color = Color.white;
        handleImg.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

        return slider;
    }
}
