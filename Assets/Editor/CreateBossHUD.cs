using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using DemHoiDenLong.UI;

public class CreateBossHUD
{
    [MenuItem("Tools/DemHoiDenLong/Setup Boss HUD")]
    public static void SetupBossHUD()
    {
        // 1. Find Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Check if BossHUD already exists
        BossHUD existingHUD = Object.FindAnyObjectByType<BossHUD>(FindObjectsInactive.Include);
        if (existingHUD != null)
        {
            Debug.Log("BossHUD already exists in the scene!");
            Selection.activeGameObject = existingHUD.gameObject;
            return;
        }

        // 3. Create BossHUD GameObject
        GameObject bossHudObj = new GameObject("BossHUD");
        bossHudObj.transform.SetParent(canvas.transform, false);
        
        RectTransform bossRect = bossHudObj.GetComponent<RectTransform>();
        if (bossRect == null) bossRect = bossHudObj.AddComponent<RectTransform>();
        bossRect.anchorMin = Vector2.zero;
        bossRect.anchorMax = Vector2.one;
        bossRect.offsetMin = Vector2.zero;
        bossRect.offsetMax = Vector2.zero;
        
        BossHUD bossHud = bossHudObj.AddComponent<BossHUD>();

        // 4. Create Panel
        GameObject panelObj = new GameObject("BossHUDPanel");
        panelObj.transform.SetParent(bossHudObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.82f); // Lowered from 0.95f
        panelRect.anchorMax = new Vector2(0.8f, 0.85f); // Lowered from 0.98f
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 5. Create Slider
        GameObject sliderObj = new GameObject("BossHPSlider");
        sliderObj.transform.SetParent(panelObj.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0.2f);
        sliderRect.anchorMax = new Vector2(0.95f, 0.8f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // Slider Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.darkGray;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Slider Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        // Slider Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.red;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;

        // 6. Assign References using SerializedObject
        SerializedObject so = new SerializedObject(bossHud);
        so.FindProperty("hpSlider").objectReferenceValue = slider;
        so.FindProperty("hudPanel").objectReferenceValue = panelObj;
        so.ApplyModifiedProperties();

        // 7. Ensure EventSystem exists
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("Boss HUD successfully created and configured!");
        Selection.activeGameObject = bossHudObj;
    }
}
