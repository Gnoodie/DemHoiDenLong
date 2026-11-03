using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.UI;

public class UpdateSettingsUIAddExit
{
    [MenuItem("Tools/DemHoiDenLong/Thêm Nút Thoát Vào Bảng Cài Đặt")]
    public static void AddExitButton()
    {
        string prefabPath = "Assets/_Project/Prefabs/UI/SettingsUI.prefab";
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("Không tìm thấy SettingsUI.prefab tại " + prefabPath);
            return;
        }

        // Bắt đầu chỉnh sửa Prefab
        GameObject instance = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;

        // Check if ExitButton already exists
        Transform existingExit = instance.transform.Find("ExitButton");
        if (existingExit != null)
        {
            Debug.Log("Nút thoát đã tồn tại! Đang cập nhật lại Image Type...");
            Image existingImg = existingExit.GetComponent<Image>();
            if (existingImg != null) existingImg.type = Image.Type.Simple;
            
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            return;
        }

        // Tạo nút Thoát
        GameObject exitObj = new GameObject("ExitButton", typeof(RectTransform), typeof(Image), typeof(Button));
        exitObj.transform.SetParent(instance.transform, false);
        
        RectTransform exitRect = exitObj.GetComponent<RectTransform>();
        exitRect.anchorMin = new Vector2(0.5f, 0f);
        exitRect.anchorMax = new Vector2(0.5f, 0f);
        exitRect.pivot = new Vector2(0.5f, 0f);
        exitRect.anchoredPosition = new Vector2(0, 100); // Cách đáy 100px
        exitRect.sizeDelta = new Vector2(300, 100);

        Image img = exitObj.GetComponent<Image>();
        img.type = Image.Type.Simple;
        img.color = new Color32(227, 66, 51, 255); // Lantern Red

        // Chữ "Thoát Game"
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(exitObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero; textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = "Thoát Game";
        tmpText.fontSize = 40;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        // Link vào SettingsUI script
        SettingsUI script = instance.GetComponent<SettingsUI>();
        Button btn = exitObj.GetComponent<Button>();
        script.GetType().GetField("exitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(script, btn);

        // Đẩy nút Close (X) lên cao hơn một chút nếu đang ở dưới
        Transform closeBtn = instance.transform.Find("CloseButton");
        if (closeBtn != null)
        {
            RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
            if (closeRect.anchorMin.y == 0) // Nút đóng ở dưới đáy
            {
                closeRect.anchoredPosition = new Vector2(0, 250); // Đẩy lên trên nút Thoát
            }
        }

        // Lưu lại
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);

        Debug.Log("Đã thêm nút Thoát Game vào SettingsUI.prefab thành công!");
    }
}
