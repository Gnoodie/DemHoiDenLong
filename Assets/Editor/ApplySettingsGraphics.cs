using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ApplySettingsGraphics
{
    [MenuItem("Tools/DemHoiDenLong/Cập Nhật Đồ Họa Cài Đặt Mới")]
    public static void ApplyGraphics()
    {
        // 1. Đảm bảo hình ảnh được set dạng Sprite
        string[] newImages = { "Assets/_Project/Art/UI/setting_button.png", "Assets/_Project/Art/UI/exit_button.png" };
        foreach (string path in newImages)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        Sprite settingBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newImages[0]);
        Sprite exitBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newImages[1]);

        if (settingBtnSprite == null || exitBtnSprite == null)
        {
            Debug.LogError("Không tìm thấy hình ảnh. Hãy chắc chắn tên file là setting_button.png và exit_button.png trong thư mục Assets/_Project/Art/UI");
            return;
        }

        // 2. Cập nhật SettingsButton.prefab
        string btnPrefabPath = "Assets/_Project/Prefabs/UI/SettingsButton.prefab";
        GameObject btnRoot = AssetDatabase.LoadAssetAtPath<GameObject>(btnPrefabPath);
        if (btnRoot != null)
        {
            GameObject btnInstance = PrefabUtility.InstantiatePrefab(btnRoot) as GameObject;
            
            // Set image and reset color
            Image img = btnInstance.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = settingBtnSprite;
                img.color = Color.white; // Bỏ màu xanh tạm thời đi
            }

            // Xóa chữ "Cài Đặt" cũ vì giờ đã có Icon
            Transform textTransform = btnInstance.transform.Find("Text");
            if (textTransform != null)
            {
                Object.DestroyImmediate(textTransform.gameObject);
            }

            PrefabUtility.SaveAsPrefabAsset(btnInstance, btnPrefabPath);
            Object.DestroyImmediate(btnInstance);
            Debug.Log("Cập nhật đồ họa cho SettingsButton thành công.");
        }
        else
        {
            Debug.LogError("Không tìm thấy SettingsButton.prefab!");
        }

        // 3. Cập nhật SettingsUI.prefab -> ExitButton
        string uiPrefabPath = "Assets/_Project/Prefabs/UI/SettingsUI.prefab";
        GameObject uiRoot = AssetDatabase.LoadAssetAtPath<GameObject>(uiPrefabPath);
        if (uiRoot != null)
        {
            GameObject uiInstance = PrefabUtility.InstantiatePrefab(uiRoot) as GameObject;
            Transform exitTransform = uiInstance.transform.Find("ExitButton");
            if (exitTransform != null)
            {
                Image img = exitTransform.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = exitBtnSprite;
                    img.color = Color.white; // Bỏ màu đỏ tạm thời
                    img.type = Image.Type.Simple;
                    
                    RectTransform rt = exitTransform.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(360, 120); // Kích thước chuẩn
                }

                // Xóa chữ "Thoát Game" cũ nếu có (vì ảnh exit_button.png thường đã có vẽ chữ)
                Transform textTransform = exitTransform.Find("Text");
                if (textTransform != null)
                {
                    Object.DestroyImmediate(textTransform.gameObject);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(uiInstance, uiPrefabPath);
            Object.DestroyImmediate(uiInstance);
            Debug.Log("Cập nhật đồ họa cho ExitButton thành công.");
        }
        else
        {
             Debug.LogError("Không tìm thấy SettingsUI.prefab!");
        }
    }
}
