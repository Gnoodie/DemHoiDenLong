using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.UI;

public class AddReviveButtonToHUD
{
    [MenuItem("Tools/DemHoiDenLong/Thêm Nút Hồi Sinh Vào Gameplay HUD")]
    public static void AddReviveButton()
    {
        GameplayHUD script = Object.FindAnyObjectByType<GameplayHUD>(FindObjectsInactive.Include);

        if (script == null)
        {
            Debug.LogError("Không tìm thấy component GameplayHUD trong Scene! Hãy chắc chắn bạn đang mở Gameplay Scene.");
            return;
        }

        GameObject instance = script.gameObject;

        // 1. Thử tìm qua biến đã gán trong script
        System.Reflection.FieldInfo goPanelField = script.GetType().GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        GameObject goPanelObj = goPanelField != null ? (GameObject)goPanelField.GetValue(script) : null;
        Transform gameOverPanel = goPanelObj != null ? goPanelObj.transform : null;

        if (gameOverPanel != null)
        {
            Debug.Log("[Thành công] Tìm thấy GameOverPanel từ biến được kéo thả trong Inspector.");
        }
        else
        {
            Debug.LogWarning("[Lưu ý] Biến 'gameOverPanel' trong GameplayHUD chưa được gán (bị null) trong Inspector. Sẽ thử tìm kiếm toàn cục theo tên...");
            
            // 2. Tìm kiếm TOÀN BỘ CẢNH (toàn bộ scene)
            Transform[] allTransformsInScene = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            foreach (Transform t in allTransformsInScene)
            {
                if (t.gameObject.scene != instance.scene) continue; // Chỉ tìm trong cảnh hiện tại

                string nameLower = t.name.ToLower().Replace(" ", "").Replace("_", "");
                if (nameLower == "gameoverpanel" || nameLower == "gameover")
                {
                    gameOverPanel = t;
                    Debug.Log($"[Thành công] Tìm thấy bảng Game Over có tên '{t.name}' (Dù không gán biến) nằm tại: {GetHierarchyPath(t)}");
                    break;
                }
            }
        }

        if (gameOverPanel == null)
        {
            Debug.Log("[Lưu ý] Không tìm thấy GameOverPanel nào trong Scene. Sẽ tự động tạo mới một bảng Game Over...");
            
            // TẠO MỚI GameOverPanel
            GameObject newGoPanel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
            newGoPanel.transform.SetParent(instance.transform, false);
            
            RectTransform panelRect = newGoPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImg = newGoPanel.GetComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.8f); // Nền đen mờ mờ

            // Tạo chữ GAME OVER
            GameObject titleTextObj = new GameObject("GameOverText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleTextObj.transform.SetParent(newGoPanel.transform, false);
            RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
            titleTextRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleTextRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleTextRect.pivot = new Vector2(0.5f, 0.5f);
            titleTextRect.sizeDelta = new Vector2(600, 200);
            titleTextRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleTmpText = titleTextObj.GetComponent<TextMeshProUGUI>();
            titleTmpText.text = "THUA CUỘC";
            titleTmpText.fontSize = 80;
            titleTmpText.fontStyle = FontStyles.Bold;
            titleTmpText.alignment = TextAlignmentOptions.Center;
            titleTmpText.color = Color.red;

            gameOverPanel = newGoPanel.transform;
        }

        // Bất kể là tự tạo hay tìm thấy, ta đều phải GÁN vào biến bằng SerializedObject để lưu lại!
        SerializedObject serializedScript = new SerializedObject(script);
        SerializedProperty goPanelProp = serializedScript.FindProperty("gameOverPanel");
        if (goPanelProp != null)
        {
            goPanelProp.objectReferenceValue = gameOverPanel.gameObject;
        }
        
        // Lưu tạm trạng thái gắn GameOverPanel trước
        serializedScript.ApplyModifiedProperties();

        // Kiểm tra xem nút Revive đã tồn tại chưa
        Transform existingRevive = gameOverPanel.Find("ReviveButton");
        if (existingRevive != null)
        {
            Debug.Log("Nút Hồi Sinh (ReviveButton) đã tồn tại!");
            
            // Nếu đã tồn tại, nhớ gán biến reviveButtonObj nếu nó chưa được gán
            SerializedProperty existingReviveProp = serializedScript.FindProperty("reviveButtonObj");
            if (existingReviveProp != null)
            {
                existingReviveProp.objectReferenceValue = existingRevive.gameObject;
                serializedScript.ApplyModifiedProperties();
            }
        }
        else
        {

        // Tạo nút Revive
        GameObject reviveObj = new GameObject("ReviveButton", typeof(RectTransform), typeof(Image), typeof(Button));
        reviveObj.transform.SetParent(gameOverPanel, false);
        
        RectTransform reviveRect = reviveObj.GetComponent<RectTransform>();
        reviveRect.anchorMin = new Vector2(0.5f, 0.5f);
        reviveRect.anchorMax = new Vector2(0.5f, 0.5f);
        reviveRect.pivot = new Vector2(0.5f, 0.5f);
        reviveRect.sizeDelta = new Vector2(400, 120);
        reviveRect.anchoredPosition = new Vector2(0, 50); // Nằm chính giữa, hơi cao lên một chút

        Image img = reviveObj.GetComponent<Image>();
        img.type = Image.Type.Simple;
        img.color = new Color32(46, 204, 113, 255); // Màu xanh lá cây mộc mạc

        // Tạo chữ "Xem QC Hồi Sinh"
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(reviveObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero; textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = "Xem QC Hồi Sinh";
        tmpText.fontSize = 45;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        // Gắn sự kiện OnClick
        Button btn = reviveObj.GetComponent<Button>();
        
        // Cập nhật biến reviveButtonObj trong GameplayHUD
        SerializedProperty reviveProp = serializedScript.FindProperty("reviveButtonObj");
        if (reviveProp != null)
        {
            reviveProp.objectReferenceValue = reviveObj;
        }

        serializedScript.ApplyModifiedProperties();

        // Gắn sự kiện 
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, script.OnReviveButtonClicked);
        }

        // --- TẠO NÚT BỎ CUỘC ---
        Transform existingGiveUp = gameOverPanel.Find("GiveUpButton");
        if (existingGiveUp == null)
        {
            GameObject giveUpObj = new GameObject("GiveUpButton", typeof(RectTransform), typeof(Image), typeof(Button));
            giveUpObj.transform.SetParent(gameOverPanel, false);
            
            RectTransform giveUpRect = giveUpObj.GetComponent<RectTransform>();
            giveUpRect.anchorMin = new Vector2(0.5f, 0.5f);
            giveUpRect.anchorMax = new Vector2(0.5f, 0.5f);
            giveUpRect.pivot = new Vector2(0.5f, 0.5f);
            giveUpRect.sizeDelta = new Vector2(400, 100);
            giveUpRect.anchoredPosition = new Vector2(0, -90); // Nằm dưới nút Revive

            Image img = giveUpObj.GetComponent<Image>();
            img.type = Image.Type.Simple;
            img.color = new Color32(200, 50, 50, 255); // Màu đỏ nhạt

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(giveUpObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero; textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
            tmpText.text = "Bỏ Cuộc";
            tmpText.fontSize = 40;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            Button btnGiveUp = giveUpObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnGiveUp.onClick, script.OnMainMenuButtonClicked);
            Debug.Log("Đã thêm nút Bỏ Cuộc vào GameOverPanel!");
        }

        // Đánh dấu scene đã thay đổi để lưu lại
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(instance.scene);

        Debug.Log("Cập nhật GameOverPanel hoàn tất!");
    }

    private static string GetHierarchyPath(Transform obj)
    {
        string path = obj.name;
        while (obj.parent != null)
        {
            obj = obj.parent;
            path = obj.name + "/" + path;
        }
        return path;
    }
}
