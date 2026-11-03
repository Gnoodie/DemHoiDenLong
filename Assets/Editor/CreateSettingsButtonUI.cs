using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.UI;
using System.IO;

public class CreateSettingsButtonUI
{
    [MenuItem("Tools/DemHoiDenLong/Setup Settings Button UI")]
    public static void CreateSettingsButtonPrefab()
    {
        string prefabPath = "Assets/_Project/Prefabs/UI/SettingsButton.prefab";

        // Create Button Object
        GameObject buttonObj = new GameObject("SettingsButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(OpenSettingsButton));
        
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 150); // Small square button
        
        // We don't have a gear icon, so let's use the Unity default UI sprite
        Image img = buttonObj.GetComponent<Image>();
        img.type = Image.Type.Sliced;
        img.color = new Color32(20, 30, 60, 255); // Dark blue-ish background

        // Add Text inside the button
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = "Cài Đặt";
        tmpText.fontSize = 30;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = new Color32(250, 201, 56, 255); // Moon Gold

        // Save Prefab
        Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));
        PrefabUtility.SaveAsPrefabAsset(buttonObj, prefabPath);
        Object.DestroyImmediate(buttonObj);
        
        Debug.Log("SettingsButton Prefab created successfully at: " + prefabPath);
    }
}
