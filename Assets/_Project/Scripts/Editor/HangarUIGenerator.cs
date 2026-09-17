using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.Editor
{
    /// <summary>
    /// Tạo toàn bộ UI Hangar theo thiết kế "Lễ hội Trung Thu" (bầu trời đêm rằm, lồng đèn đỏ, trăng vàng).
    /// Layout dùng inset pixel cộng dồn TOP-DOWN nhất quán, tránh chồng lấn giữa các khối.
    /// Góc bo tròn được tạo bằng sprite sinh tự động (không cần asset ảnh ngoài).
    /// </summary>
    public class HangarUIGenerator : UnityEditor.EditorWindow
    {
        // ====== LAYOUT ======
        private const float REF_W = 1080f;
        private const float REF_H = 1920f;
        private const float OUTER_PAD = 24f;
        private const float CURRENCY_H = 130f;
        private const float GAP = 16f;

        private const float SCROLL_H = 520f;
        private const float NAME_H = 70f;
        private const float ICON_STATS_H = 340f;
        private const float SKILL_H = 120f;
        private const float UPGRADE_H = 140f;
        // StoryPanel không có chiều cao cố định - tự lấp đầy phần còn lại phía dưới.

        // ====== MÀU SẮC LỄ HỘI TRUNG THU ======
        // Bầu trời đêm rằm sâu thẳm, tương phản với sắc đỏ/vàng rực rỡ của lồng đèn và điểm xuyết ngọc bích.
        private static readonly Color BG_NIGHT_SKY   = new Color(0.04f, 0.06f, 0.16f); // #0a1029 Bầu trời đêm rằm
        private static readonly Color PANEL_INDIGO   = new Color(0.11f, 0.16f, 0.32f); // #1c2952 Nền panel (trời đêm sáng hơn)
        private static readonly Color LANTERN_RED    = new Color(0.89f, 0.26f, 0.20f); // #e34233 Đỏ lồng đèn truyền thống
        private static readonly Color MOON_GOLD      = new Color(0.98f, 0.79f, 0.22f); // #fac938 Vàng trăng rằm, viền, text nhấn
        private static readonly Color MOON_GOLD_DIM  = new Color(0.98f, 0.79f, 0.22f, 0.4f);
        private static readonly Color TEXT_MAIN      = new Color(1.00f, 0.96f, 0.89f); // #fff5e3 Trắng ngà lồng đèn giấy
        private static readonly Color TEXT_MUTED     = new Color(0.64f, 0.71f, 0.85f); // #a3b5d9 Xanh xám nhạt (nhường bước cho glow)
        private static readonly Color JADE_GREEN     = new Color(0.13f, 0.49f, 0.37f); // #217d5e Xanh ngọc bích (nút bấm nâng cấp)
        private static readonly Color JADE_GLOW      = new Color(0.44f, 0.87f, 0.69f); // #70deaf Icon ngọc bích sáng
        private static readonly Color STORY_BG       = new Color(0.08f, 0.12f, 0.25f); // #141e40 Nền câu chuyện
        private static readonly Color UNLOCK_OVERLAY = new Color(0.0f, 0.0f, 0.0f, 0.85f);

        private static Sprite _roundedSprite;

        [MenuItem("Tools/DemHoiDenLong/Generate Hangar UI")]
        public static void GenerateUI()
        {
            // Xóa Canvas cũ nếu đã tồn tại để tránh trùng lặp
            GameObject oldCanvas = GameObject.Find("Canvas_Hangar");
            if (oldCanvas != null)
            {
                DestroyImmediate(oldCanvas);
            }

            _roundedSprite = GetOrCreateRoundedSprite(28); // Tăng độ bo tròn mềm mại như chiếc lồng đèn

            // 1. Canvas
            GameObject canvasObj = new GameObject("Canvas_Hangar");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(REF_W, REF_H);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            canvasObj.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                System.Type newInputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newInputModule != null) es.AddComponent(newInputModule);
                else es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 1.1 Nền toàn màn hình (đỏ maroon)
            GameObject bg = CreateUIElement("Background", canvasObj.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.sprite = GetOrImportSprite("Assets/_Project/Sprites/UI/background.png");
            bgImg.color = Color.white;

            // 2. CurrencyPanel
            GameObject currencyPanel = CreateUIElement("CurrencyPanel", canvasObj.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(OUTER_PAD, -CURRENCY_H), new Vector2(-OUTER_PAD, -OUTER_PAD));
            CurrencyUI currencyScript = currencyPanel.AddComponent<CurrencyUI>();

            // Đường viền vàng mảnh dưới thanh tiền (giữ lại theo yêu cầu)
            GameObject currencyLine = CreateUIElement("BottomLine", currencyPanel.transform,
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 2));
            currencyLine.AddComponent<Image>().color = MOON_GOLD_DIM;

            GameObject starIcon = CreateUIElement("StarIcon", currencyPanel.transform,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, -20), new Vector2(50, 20));
            Image starImg = starIcon.AddComponent<Image>();
            starImg.sprite = GetOrImportSprite("Assets/_Project/Sprites/UI/sao.png");
            starImg.color = Color.white;

            GameObject starText = CreateText("StarLanternText", currencyPanel.transform, "1,250",
                new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(60, 0), new Vector2(0, 0), 24, 44);
            starText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            starText.GetComponent<TextMeshProUGUI>().color = MOON_GOLD;

            GameObject diaIcon = CreateUIElement("DiaIcon", currencyPanel.transform,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, -20), new Vector2(-10, 20));
            Image diaImg = diaIcon.AddComponent<Image>();
            diaImg.sprite = GetOrImportSprite("Assets/_Project/Sprites/UI/kimcuong.png");
            diaImg.color = Color.white;

            GameObject diaText = CreateText("DiamondText", currencyPanel.transform, "88",
                new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-60, 0), 24, 44);
            diaText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
            diaText.GetComponent<TextMeshProUGUI>().color = new Color(0.494f, 0.784f, 0.890f);

            SerializedObject soCurrency = new SerializedObject(currencyScript);
            soCurrency.FindProperty("starLanternText").objectReferenceValue = starText.GetComponent<TextMeshProUGUI>();
            soCurrency.FindProperty("diamondText").objectReferenceValue = diaText.GetComponent<TextMeshProUGUI>();
            soCurrency.ApplyModifiedProperties();

            // 2.1 Tiêu đề "TRẠI LÂN ÁNH TRĂNG"
            GameObject titleObj = CreateText("HangarTitle", canvasObj.transform, "TRẠI LÂN ÁNH TRĂNG",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(OUTER_PAD, -(OUTER_PAD + CURRENCY_H + 50f)), new Vector2(-OUTER_PAD, -(OUTER_PAD + CURRENCY_H)), 24, 36);
            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = MOON_GOLD;
            titleTmp.characterSpacing = 8;

            // 3. HangarPanel
            float hangarTopInset = OUTER_PAD + CURRENCY_H + 50f + GAP;
            GameObject hangarPanel = CreateUIElement("HangarPanel", canvasObj.transform,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, OUTER_PAD), new Vector2(0, -hangarTopInset));
            HangarUI hangarScript = hangarPanel.AddComponent<HangarUI>();

            // 3.1 Scroll View (danh sách Lân)
            GameObject scrollObj = CreateUIElement("Scroll View", hangarPanel.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(OUTER_PAD, -SCROLL_H), new Vector2(-OUTER_PAD, 0));

            GameObject viewport = CreateUIElement("Viewport", scrollObj.transform,
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateUIElement("Content", viewport.transform,
                new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            content.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            HorizontalLayoutGroup hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 24;
            hlg.padding = new RectOffset(10, 10, 10, 10);
            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // 3.2 DetailsPanel (thêm nền kính mờ để làm nổi bật text phía trên mặt trăng sáng)
            float detailsTopInset = SCROLL_H + GAP;
            GameObject detailsPanel = CreateUIElement("DetailsPanel", hangarPanel.transform,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(OUTER_PAD, 0), new Vector2(-OUTER_PAD, -detailsTopInset));
            Image detailsImg = detailsPanel.AddComponent<Image>();
            detailsImg.sprite = _roundedSprite;
            detailsImg.type = Image.Type.Sliced;
            detailsImg.color = new Color(0.04f, 0.06f, 0.15f, 0.88f); // Nền tối bán trong suốt

            // --- Tên Lân ---
            GameObject lanNameObj = CreateText("LanNameText", detailsPanel.transform, "* Lân Ánh Nguyệt *",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(10, -NAME_H), new Vector2(-10, 0), 28, 48);
            lanNameObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            lanNameObj.GetComponent<TextMeshProUGUI>().color = MOON_GOLD;

            // --- Icon lớn + Thông số ---
            float rowTopInset = NAME_H + GAP;
            float rowBottomInset = rowTopInset + ICON_STATS_H;
            
            GameObject bigIconFrameObj = CreateUIElement("BigIconFrame", detailsPanel.transform,
                new Vector2(0, 1), new Vector2(0.4f, 1),
                new Vector2(10, -rowBottomInset), new Vector2(0, -rowTopInset));
            Image bigIconFrameImg = bigIconFrameObj.AddComponent<Image>();
            bigIconFrameImg.sprite = _roundedSprite; bigIconFrameImg.type = Image.Type.Sliced;
            bigIconFrameImg.color = PANEL_INDIGO;

            GameObject bigIconObj = CreateUIElement("BigIcon", bigIconFrameObj.transform,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(8, 8), new Vector2(-8, -8));
            Image bigIconImg = bigIconObj.AddComponent<Image>();
            bigIconImg.color = Color.white; // Màu trắng để không làm sai màu sprite của Lân

            GameObject statsObj = CreateUIElement("StatsContainer", detailsPanel.transform,
                new Vector2(0.4f, 1), new Vector2(1, 1),
                new Vector2(20, -rowBottomInset), new Vector2(-10, -rowTopInset));
            VerticalLayoutGroup vlgStats = statsObj.AddComponent<VerticalLayoutGroup>();
            vlgStats.childControlHeight = true; vlgStats.childControlWidth = true;
            vlgStats.childForceExpandHeight = true; vlgStats.childForceExpandWidth = true;
            vlgStats.spacing = 10;
            vlgStats.childAlignment = TextAnchor.MiddleLeft;

            GameObject hpTxt = CreateStatRow("hp", statsObj.transform, "HP", "100");
            GameObject dmgTxt = CreateStatRow("dmg", statsObj.transform, "DMG", "10");
            GameObject frTxt = CreateStatRow("fr", statsObj.transform, "TỐC BẮN", "1.0");

            // --- Mô tả kỹ năng ---
            float skillTopInset = rowBottomInset + GAP;
            float skillBottomInset = skillTopInset + SKILL_H;
            GameObject skillObj = CreateText("SkillDescText", detailsPanel.transform, "Kỹ năng · ...",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(10, -skillBottomInset), new Vector2(-10, -skillTopInset), 16, 26);
            skillObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            skillObj.GetComponent<TextMeshProUGUI>().color = TEXT_MUTED;

            // --- Khối 3 nút nâng cấp (tiếp nối top-down ngay dưới Skill) ---
            float upgradeTopInset = skillBottomInset + GAP;
            float upgradeBottomInset = upgradeTopInset + UPGRADE_H;
            GameObject upgradesPanel = CreateUIElement("UpgradesPanel", detailsPanel.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(10, -upgradeBottomInset), new Vector2(-10, -upgradeTopInset));
            HorizontalLayoutGroup upHlg = upgradesPanel.AddComponent<HorizontalLayoutGroup>();
            upHlg.childControlWidth = true; upHlg.childControlHeight = true;
            upHlg.childForceExpandWidth = true; upHlg.childForceExpandHeight = true;
            upHlg.spacing = 12;

            GameObject upHp = CreatePillButton("UpgradeHPBtn", upgradesPanel.transform, "NÂNG HP", out TextMeshProUGUI hpCost);
            GameObject upDmg = CreatePillButton("UpgradeDMGBtn", upgradesPanel.transform, "NÂNG DMG", out TextMeshProUGUI dmgCost);
            GameObject upFr = CreatePillButton("UpgradeFRBtn", upgradesPanel.transform, "NÂNG TỐC", out TextMeshProUGUI frCost);

            // --- UnlockPanel: phủ đúng lên UpgradesPanel ---
            GameObject unlockPanel = CreateUIElement("UnlockPanel", detailsPanel.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(10, -upgradeBottomInset), new Vector2(-10, -upgradeTopInset));
            Image unlockBg = unlockPanel.AddComponent<Image>();
            unlockBg.sprite = _roundedSprite; unlockBg.type = Image.Type.Sliced;
            unlockBg.color = UNLOCK_OVERLAY;

            GameObject unlockBtn = CreatePillButton("UnlockBtn", unlockPanel.transform, "MỞ KHÓA", out TextMeshProUGUI unlockCost);
            RectTransform unlockBtnRt = unlockBtn.GetComponent<RectTransform>();
            unlockBtnRt.anchorMin = new Vector2(0.25f, 0.15f);
            unlockBtnRt.anchorMax = new Vector2(0.75f, 0.85f);
            unlockBtnRt.offsetMin = Vector2.zero;
            unlockBtnRt.offsetMax = Vector2.zero;

            // --- StoryPanel: lấp đầy phần còn lại phía dưới, luôn khớp dù đổi các hằng số phía trên ---
            float storyTopInset = upgradeBottomInset + GAP;
            GameObject storyPanel = CreateUIElement("StoryPanel", detailsPanel.transform,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(10, 10), new Vector2(-10, -storyTopInset));
            Image storyImg = storyPanel.AddComponent<Image>();
            storyImg.sprite = _roundedSprite; storyImg.type = Image.Type.Sliced;
            storyImg.color = STORY_BG;

            GameObject chapterChip = CreateUIElement("ChapterChip", storyPanel.transform,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -52), new Vector2(220, -16));
            Image chipImg = chapterChip.AddComponent<Image>();
            chipImg.sprite = _roundedSprite; chipImg.type = Image.Type.Sliced;
            chipImg.color = PANEL_INDIGO;
            GameObject storyChapterText = CreateText("ChapterText", chapterChip.transform, "HỒI 01 · ĐÊM RƯỚC ĐÈN",
                Vector2.zero, Vector2.one, new Vector2(10, 4), new Vector2(-10, -4), 12, 16);
            storyChapterText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            storyChapterText.GetComponent<TextMeshProUGUI>().color = MOON_GOLD;

            GameObject storyTitleText = CreateText("TitleText", storyPanel.transform, "Lời hứa dưới trăng",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -96), new Vector2(-16, -56), 22, 30);
            storyTitleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            storyTitleText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            storyTitleText.GetComponent<TextMeshProUGUI>().color = TEXT_MAIN;

            GameObject storyTokenText = CreateText("TokenText", storyPanel.transform, "Tín vật: Đèn Kéo Quân",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 12), new Vector2(-16, 42), 14, 18);
            storyTokenText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            storyTokenText.GetComponent<TextMeshProUGUI>().color = LANTERN_RED;

            GameObject storyDescText = CreateText("DescText", storyPanel.transform, "Từng là chú lân nhỏ canh giữ phố đèn...",
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(16, 50), new Vector2(-16, -100), 14, 20);
            storyDescText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            storyDescText.GetComponent<TextMeshProUGUI>().color = TEXT_MUTED;

            // 4. LanItemPrefab
            GameObject lanItemObj = CreateUIElement("LanItemPrefab", canvasObj.transform,
                new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, Vector2.zero);
            lanItemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(240, SCROLL_H - 20f);
            LanSelectionItemUI itemScript = lanItemObj.AddComponent<LanSelectionItemUI>();
            Image itemBg = lanItemObj.AddComponent<Image>();
            itemBg.sprite = _roundedSprite; itemBg.type = Image.Type.Sliced;
            itemBg.color = LANTERN_RED;
            Button itemBtn = lanItemObj.AddComponent<Button>();

            GameObject itemIconFrame = CreateUIElement("IconFrame", lanItemObj.transform,
                new Vector2(0, 0.22f), new Vector2(1, 1), new Vector2(10, 0), new Vector2(-10, -10));
            Image itemIconFrameImg = itemIconFrame.AddComponent<Image>();
            itemIconFrameImg.sprite = _roundedSprite; itemIconFrameImg.type = Image.Type.Sliced;
            itemIconFrameImg.color = STORY_BG;

            GameObject itemIcon = CreateUIElement("Icon", itemIconFrame.transform,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(6, 6), new Vector2(-6, -6));
            itemIcon.AddComponent<Image>().color = Color.white; // Để icon hiển thị đúng màu thay vì bị mờ

            GameObject itemName = CreateText("Name", lanItemObj.transform, "Lân A",
                new Vector2(0, 0), new Vector2(1, 0.22f), new Vector2(4, 6), new Vector2(-4, 0), 14, 22);
            itemName.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            itemName.GetComponent<TextMeshProUGUI>().color = TEXT_MAIN;

            GameObject itemLock = CreateUIElement("LockedOverlay", lanItemObj.transform,
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            Image lockImg = itemLock.AddComponent<Image>();
            lockImg.sprite = _roundedSprite; lockImg.type = Image.Type.Sliced;
            lockImg.color = new Color(0, 0, 0, 0.7f);
            
            // Icon khóa tạm thời dùng kimcuong
            GameObject lockIconObj = CreateUIElement("LockIcon", itemLock.transform,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(-24, -24), new Vector2(24, 24));
            Image lockIconImg = lockIconObj.AddComponent<Image>();
            lockIconImg.sprite = GetOrImportSprite("Assets/_Project/Sprites/UI/kimcuong.png");
            lockIconImg.color = Color.white;

            GameObject lockLabel = CreateText("LockLabel", itemLock.transform, "ĐÃ KHÓA",
                new Vector2(0, 0), new Vector2(1, 0.3f), Vector2.zero, Vector2.zero, 14, 22);
            lockLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            lockLabel.GetComponent<TextMeshProUGUI>().color = MOON_GOLD;

            SerializedObject soItem = new SerializedObject(itemScript);
            soItem.FindProperty("iconImage").objectReferenceValue = itemIcon.GetComponent<Image>();
            soItem.FindProperty("nameText").objectReferenceValue = itemName.GetComponent<TextMeshProUGUI>();
            soItem.FindProperty("lockedOverlay").objectReferenceValue = itemLock;
            soItem.FindProperty("selectButton").objectReferenceValue = itemBtn;
            soItem.ApplyModifiedProperties();

            lanItemObj.SetActive(false);

            // 5. Nối dây HangarUI (giữ nguyên tên field như file gốc bạn cung cấp)
            SerializedObject soHangar = new SerializedObject(hangarScript);
            soHangar.FindProperty("listContainer").objectReferenceValue = content.transform;
            soHangar.FindProperty("itemPrefab").objectReferenceValue = itemScript;
            soHangar.FindProperty("lanNameText").objectReferenceValue = lanNameObj.GetComponent<TextMeshProUGUI>();
            soHangar.FindProperty("lanIconLarge").objectReferenceValue = bigIconObj.GetComponent<Image>();
            soHangar.FindProperty("skillDescText").objectReferenceValue = skillObj.GetComponent<TextMeshProUGUI>();

            soHangar.FindProperty("storyChapterText").objectReferenceValue = storyChapterText.GetComponent<TextMeshProUGUI>();
            soHangar.FindProperty("storyTitleText").objectReferenceValue = storyTitleText.GetComponent<TextMeshProUGUI>();
            soHangar.FindProperty("storyDescriptionText").objectReferenceValue = storyDescText.GetComponent<TextMeshProUGUI>();
            soHangar.FindProperty("storyTokenText").objectReferenceValue = storyTokenText.GetComponent<TextMeshProUGUI>();

            soHangar.FindProperty("hpText").objectReferenceValue = hpTxt.GetComponent<TextMeshProUGUI>();
            soHangar.FindProperty("damageText").objectReferenceValue = dmgTxt.GetComponent<TextMeshProUGUI>();
            soHangar.FindProperty("fireRateText").objectReferenceValue = frTxt.GetComponent<TextMeshProUGUI>();

            soHangar.FindProperty("upgradeHpBtn").objectReferenceValue = upHp.GetComponent<Button>();
            soHangar.FindProperty("upgradeHpCostText").objectReferenceValue = hpCost;
            soHangar.FindProperty("upgradeDmgBtn").objectReferenceValue = upDmg.GetComponent<Button>();
            soHangar.FindProperty("upgradeDmgCostText").objectReferenceValue = dmgCost;
            soHangar.FindProperty("upgradeFrBtn").objectReferenceValue = upFr.GetComponent<Button>();
            soHangar.FindProperty("upgradeFrCostText").objectReferenceValue = frCost;

            soHangar.FindProperty("unlockPanel").objectReferenceValue = unlockPanel;
            soHangar.FindProperty("unlockBtn").objectReferenceValue = unlockBtn.GetComponent<Button>();
            soHangar.FindProperty("unlockCostText").objectReferenceValue = unlockCost;

            soHangar.ApplyModifiedProperties();

            Debug.Log("Hangar UI (Lễ hội Trung Thu) Generated Successfully!");
        }

        // ====== HELPERS ======

        private static GameObject CreateStatRow(string name, Transform parent, string label, string value)
        {
            GameObject row = CreateUIElement(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = true; h.childForceExpandHeight = true;

            GameObject labelObj = CreateText("Label", row.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16, 24);
            labelObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            labelObj.GetComponent<TextMeshProUGUI>().color = TEXT_MUTED;

            GameObject valueObj = CreateText("Value", row.transform, value, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16, 24);
            valueObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
            valueObj.GetComponent<TextMeshProUGUI>().color = TEXT_MAIN;

            return row; // Lưu ý: HangarUI cần set text trên "Value" con nếu muốn cập nhật số liệu runtime
        }

        private static GameObject CreatePillButton(string name, Transform parent, string label, out TextMeshProUGUI txt)
        {
            GameObject btnObj = CreateUIElement(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image img = btnObj.AddComponent<Image>();
            img.sprite = _roundedSprite; img.type = Image.Type.Sliced;
            img.color = name == "UnlockBtn" ? LANTERN_RED : JADE_GREEN;
            btnObj.AddComponent<Button>();

            GameObject iconObj = CreateUIElement("BtnIcon", btnObj.transform, 
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, -18), new Vector2(48, 18));
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;
            if (name == "UnlockBtn")
            {
                iconImg.sprite = GetOrImportSprite("Assets/_Project/Sprites/UI/kimcuong.png");
            }
            else
            {
                iconImg.sprite = GetOrImportSprite("Assets/_Project/Sprites/UI/upgrade_btn.png");
            }

            GameObject vBox = CreateUIElement("Content", btnObj.transform, Vector2.zero, Vector2.one, new Vector2(52, 4), new Vector2(-4, -4));
            VerticalLayoutGroup v = vBox.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.MiddleCenter;
            v.spacing = 2; v.childControlHeight = false; v.childControlWidth = true; v.childForceExpandWidth = true;
            
            GameObject labelObj = CreateText("Label", vBox.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 12, 18);
            labelObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            labelObj.GetComponent<TextMeshProUGUI>().color = TEXT_MAIN;
            labelObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.preferredHeight = 18;

            GameObject costObj = CreateText("Cost", vBox.transform, "0", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 12, 18);
            costObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            costObj.GetComponent<TextMeshProUGUI>().color = MOON_GOLD;
            LayoutElement costLe = costObj.AddComponent<LayoutElement>();
            costLe.preferredHeight = 18;

            txt = costObj.GetComponent<TextMeshProUGUI>();
            return btnObj;
        }

        private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax; // offsetMin/offsetMax đã tự tính đúng cả vị trí và kích thước - KHÔNG ghi đè anchoredPosition sau đó
            rt.localScale = Vector3.one;
            return go;
        }

        private static GameObject CreateText(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float fontMin = 20, float fontMax = 60)
        {
            GameObject go = CreateUIElement(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = fontMin;
            tmp.fontSizeMax = fontMax;
            tmp.overflowMode = TextOverflowModes.Truncate;
            return go;
        }

        /// <summary>
        /// Sinh (hoặc tái sử dụng) 1 sprite hình chữ nhật bo góc để dùng làm nền cho
        /// mọi panel/card/button cần "border-radius" - Unity không có sẵn tính năng này cho Image thường.
        /// Sprite được lưu tại Assets/Editor Default Resources/GeneratedUI để chỉ cần sinh 1 lần.
        /// </summary>
        private static Sprite GetOrCreateRoundedSprite(int radius)
        {
            string folder = "Assets/Editor Default Resources/GeneratedUI";
            string assetPath = folder + "/RoundedRect_r" + radius + ".png";

            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null) return existing;

            if (!AssetDatabase.IsValidFolder("Assets/Editor Default Resources"))
                AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Editor Default Resources", "GeneratedUI");

            int size = radius * 2 + 6;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float dx = 0, dy = 0;
                    if (px < radius) dx = radius - px; else if (px > size - radius) dx = px - (size - radius);
                    if (py < radius) dy = radius - py; else if (py > size - radius) dy = py - (size - radius);
                    bool opaque = !(dx > 0 && dy > 0) || (dx * dx + dy * dy <= radius * radius);
                    pixels[y * size + x] = opaque ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, png);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(radius, radius, radius, radius);
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Sprite GetOrImportSprite(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Không tìm thấy file ảnh tại: {path}");
                return null;
            }

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}