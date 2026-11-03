using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

public class SetupGameplayHUD
{
    [MenuItem("DemHoiDenLong/Setup Gameplay HUD")]
    public static void SetupHUD()
    {
        // Find or create Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Find GameplayHUD
        GameplayHUD hud = Object.FindAnyObjectByType<GameplayHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("GameplayHUD");
            hudObj.transform.SetParent(canvas.transform, false);
            hud = hudObj.AddComponent<GameplayHUD>();
        }
        
        RectTransform hudRt = hud.GetComponent<RectTransform>();
        if (hudRt == null) hudRt = hud.gameObject.AddComponent<RectTransform>();
        hudRt.anchorMin = Vector2.zero;
        hudRt.anchorMax = Vector2.one;
        hudRt.sizeDelta = Vector2.zero;
        hudRt.anchoredPosition = Vector2.zero;

        // Find Player
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        
        // Setup SerializedObject
        SerializedObject hudSO = new SerializedObject(hud);
        if (player != null)
        {
            hudSO.FindProperty("player").objectReferenceValue = player;
        }
        else
        {
            Debug.LogWarning("PlayerController not found in scene. Cannot link to HUD.");
        }

        // --- Player Stats Panel ---
        RectTransform statsPanel = CreateUIElement("PlayerStatsPanel", hud.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(600, 200));
        statsPanel.anchoredPosition = new Vector2(10, -10); // Top-Left with padding

        // HP Slider
        Slider hpSlider = CreateSlider(statsPanel, "HPSlider", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 50));
        hpSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(180, -30); // Offset to right for Skill Icon
        hpSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(-200, 50); // Stretch horizontally
        
        // HP Text
        TextMeshProUGUI hpText = CreateText(hpSlider.transform, "HPText", "100 / 100", 36, TextAlignmentOptions.Center);
        hpText.color = Color.white;
        
        // Lives Container
        RectTransform livesContainer = CreateUIElement("LivesContainer", statsPanel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(200, 50));
        livesContainer.anchoredPosition = new Vector2(180, -90); // Lowered Y position for hearts
        HorizontalLayoutGroup layout = livesContainer.gameObject.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = livesContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 10;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Image skillIcon = CreateImage(statsPanel, "SkillIcon", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, 150));
        skillIcon.rectTransform.anchoredPosition = new Vector2(10, -25);
        skillIcon.color = Color.white; 
        
        Sprite defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Icons/kim_lan_icon.png");
        if (defaultIcon != null)
        {
            skillIcon.sprite = defaultIcon;
        }

        Image skillCooldownOverlay = CreateImage(skillIcon.transform, "CooldownOverlay", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero);
        skillCooldownOverlay.color = new Color(0, 0, 0, 0.7f);
        skillCooldownOverlay.type = Image.Type.Filled;
        skillCooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        skillCooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
        skillCooldownOverlay.fillClockwise = false;
        skillCooldownOverlay.fillAmount = 0f;

        // Apply references
        hudSO.FindProperty("hpSlider").objectReferenceValue = hpSlider;
        hudSO.FindProperty("hpText").objectReferenceValue = hpText;
        hudSO.FindProperty("livesContainer").objectReferenceValue = livesContainer;
        hudSO.FindProperty("skillIcon").objectReferenceValue = skillIcon;
        hudSO.FindProperty("skillCooldownOverlay").objectReferenceValue = skillCooldownOverlay;

        hudSO.ApplyModifiedProperties();
        
        Debug.Log("Gameplay HUD setup complete. Player stats and Skill UI linked.");
    }

    private static RectTransform CreateUIElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<RectTransform>();

        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<TextMeshProUGUI>();

        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        return tmp;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<Slider>();

        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Slider));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;

        // Background
        Image bg = CreateImage(obj.transform, "Background", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        bg.color = Color.darkGray;

        // Fill Area
        RectTransform fillArea = CreateUIElement("Fill Area", obj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-10, 0));
        Image fill = CreateImage(fillArea, "Fill", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
        fill.color = Color.green;

        Slider slider = obj.GetComponent<Slider>();
        slider.targetGraphic = bg;
        slider.fillRect = fill.rectTransform;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        return slider;
    }

    private static Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<Image>();

        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        return obj.GetComponent<Image>();
    }
}
