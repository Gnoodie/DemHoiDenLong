using UnityEngine;
using UnityEditor;

public class SetupSettingsInScene
{
    [MenuItem("Tools/DemHoiDenLong/Thêm Nút Cài Đặt Vào Cảnh Hiện Tại (Scene)")]
    public static void AddSettingsToCurrentScene()
    {
        // 1. Tìm Canvas trong Scene
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas nào trong Scene! Vui lòng mở màn hình có Canvas (như Gameplay hoặc MainMenu) trước khi chạy công cụ này.");
            return;
        }

        // 2. Load các Prefab
        GameObject settingsUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/SettingsUI.prefab");
        GameObject settingsButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/SettingsButton.prefab");

        if (settingsUIPrefab == null || settingsButtonPrefab == null)
        {
            Debug.LogError("Không tìm thấy SettingsUI.prefab hoặc SettingsButton.prefab! Hãy đảm bảo bạn đã chạy 2 công cụ tạo Prefab trước đó.");
            return;
        }

        // 3. Khởi tạo Bảng Cài Đặt (SettingsUI)
        GameObject settingsUIObj = PrefabUtility.InstantiatePrefab(settingsUIPrefab, canvas.transform) as GameObject;
        settingsUIObj.name = "SettingsUI";
        RectTransform settingsUIRect = settingsUIObj.GetComponent<RectTransform>();
        settingsUIRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsUIRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsUIRect.pivot = new Vector2(0.5f, 0.5f);
        settingsUIRect.anchoredPosition = Vector2.zero;
        settingsUIObj.SetActive(false); // Mặc định ẩn

        // 4. Khởi tạo Nút Cài Đặt (SettingsButton)
        GameObject buttonObj = PrefabUtility.InstantiatePrefab(settingsButtonPrefab, canvas.transform) as GameObject;
        buttonObj.name = "SettingsButton";
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        
        // Đặt ở góc trên bên Phải (Top-Right) để đối xứng với Thanh máu (Top-Left) và Skill (Bottom)
        btnRect.anchorMin = new Vector2(1f, 1f);
        btnRect.anchorMax = new Vector2(1f, 1f);
        btnRect.pivot = new Vector2(1f, 1f);
        
        // Cách mép phải 50px, cách mép trên 50px
        btnRect.anchoredPosition = new Vector2(-50, -50);
        
        // Đăng ký thay đổi để Unity cho phép Save Scene
        Undo.RegisterCreatedObjectUndo(settingsUIObj, "Add SettingsUI");
        Undo.RegisterCreatedObjectUndo(buttonObj, "Add SettingsButton");

        Debug.Log("Đã thêm nút Cài Đặt (Góc trên bên phải) và Bảng Settings vào Canvas thành công!");
    }
}
