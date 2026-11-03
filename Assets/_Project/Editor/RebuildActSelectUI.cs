using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RebuildActSelectUI
{
    private const string PREFAB_PATH = "Assets/_Project/Prefabs/ActCard_Template.prefab";
    private const string SCENE_PATH = "Assets/Scenes/LevelSelect.unity";
    private const string FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    // Colors
    private static readonly Color ColorBgTop = new Color(0.055f, 0.102f, 0.227f, 1f);     // #0E1A3A
    private static readonly Color ColorBgBottom = new Color(0.106f, 0.09f, 0.278f, 1f);    // #1B1747
    private static readonly Color ColorCardBg = new Color(0.086f, 0.133f, 0.29f, 1f);      // #16224A
    private static readonly Color ColorInfoBar = new Color(0.063f, 0.102f, 0.235f, 1f);    // #101A3C
    private static readonly Color ColorChip = new Color(0.11f, 0.165f, 0.361f, 1f);       // #1C2A5C
    private static readonly Color ColorCream = new Color(0.957f, 0.945f, 0.902f, 1f);     // #F4F1E6
    private static readonly Color ColorSubText = new Color(0.663f, 0.706f, 0.839f, 1f);   // #A9B4D6
    private static readonly Color ColorGold = new Color(1f, 0.824f, 0.478f, 1f);          // #FFD27A
    private static readonly Color ColorDarkBrown = new Color(0.227f, 0.133f, 0f, 1f);      // #3A2200

    private static readonly Color ColorMint = new Color(0.498f, 0.89f, 0.753f, 1f);       // #7FE3C0
    private static readonly Color ColorApricot = new Color(1f, 0.706f, 0.42f, 1f);        // #FFB46B
    private static readonly Color ColorLavender = new Color(0.663f, 0.608f, 1f, 1f);       // #A99BFF

    private static readonly Color ColorBadgeCompleted = new Color(0.18f, 0.62f, 0.478f, 1f);  // #2E9E7A
    private static readonly Color ColorBadgeProgress = new Color(0.961f, 0.62f, 0.259f, 1f);  // #F59E42
    private static readonly Color ColorStarAchieved = new Color(1f, 0.824f, 0.478f, 1f);      // #FFD27A
    private static readonly Color ColorStarUnachieved = new Color(0.29f, 0.333f, 0.502f, 1f); // #4A5580

    [MenuItem("DemHoiDenLong/Rebuild ActSelect UI")]
    public static void ExecuteRebuild()
    {
        Debug.Log("[RebuildActSelectUI] Bat dau dung lai giao dien ActSelectView...");

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);

        // 1. Kiem tra va nạp cac Sprite
        Sprite sprRound36 = LoadSprite("Assets/_Project/Sprites/UI/ui_round_36.png");
        Sprite sprRoundTop36 = LoadSprite("Assets/_Project/Sprites/UI/ui_round_top_36.png");
        Sprite sprRoundBottom36 = LoadSprite("Assets/_Project/Sprites/UI/ui_round_bottom_36.png");
        Sprite sprPill = LoadSprite("Assets/_Project/Sprites/UI/ui_pill.png");
        Sprite sprCircle = LoadSprite("Assets/_Project/Sprites/UI/ui_circle.png");
        Sprite sprGradFade = LoadSprite("Assets/_Project/Sprites/UI/ui_gradient_fade.png");
        Sprite sprGlowRing = LoadSprite("Assets/_Project/Sprites/UI/ui_glow_ring.png");
        Sprite sprBgGrad = LoadSprite("Assets/_Project/Sprites/UI/ui_bg_gradient.png");

        Sprite sprSao = LoadSprite("Assets/_Project/Sprites/UI/sao.png");
        Sprite sprKimcuong = LoadSprite("Assets/_Project/Sprites/UI/kimcuong.png");
        Sprite sprArrowLeft = LoadSprite("Assets/_Project/Art/Icons/arrowLeft.png");
        Sprite sprArrowRight = LoadSprite("Assets/_Project/Art/Icons/arrowRight.png");
        Sprite sprGear = LoadSprite("Assets/_Project/Art/Icons/gear.png");
        Sprite sprLocked = LoadSprite("Assets/_Project/Art/Icons/locked 1.png") ?? LoadSprite("Assets/_Project/Art/Icons/locked.png");

        Sprite sprBanner1 = LoadSprite("Assets/_Project/Art/Backgrounds/back_act1.png");
        Sprite sprBanner2 = LoadSprite("Assets/_Project/Art/Backgrounds/back_act2.png");
        Sprite sprBanner3 = LoadSprite("Assets/_Project/Art/Backgrounds/back_act3.png");

        // 2. Rebuild ActCard_Template.prefab
        RebuildCardPrefab(sprRound36, sprRoundBottom36, sprPill, sprCircle, sprGradFade, sprGlowRing, sprSao, sprArrowRight, sprLocked, sprBanner1, fontAsset);

        // 3. Rebuild Scene LevelSelect.unity
        RebuildScene(sprBgGrad, sprCircle, sprPill, sprRound36, sprRoundTop36, sprSao, sprKimcuong, sprGear, sprArrowLeft, sprBanner1, sprBanner2, sprBanner3, fontAsset);

        Debug.Log("[RebuildActSelectUI] HOAN TAT dung lai giao dien ActSelectView!");
    }

    private static Sprite LoadSprite(string path)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (sprites.Length > 0) return sprites[0];
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void RebuildCardPrefab(
        Sprite sprRound36, Sprite sprRoundBottom36, Sprite sprPill, Sprite sprCircle,
        Sprite sprGradFade, Sprite sprGlowRing, Sprite sprSao, Sprite sprArrowRight,
        Sprite sprLocked, Sprite sprBannerDefault, TMP_FontAsset font)
    {
        Debug.Log("[RebuildActSelectUI] Rebuilding Prefab: " + PREFAB_PATH);

        // Load existing prefab or create instance
        GameObject root = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
        if (root == null)
        {
            Debug.LogError("Khong tim thay prefab: " + PREFAB_PATH);
            return;
        }

        // Setup Root ActCard
        RectTransform rtRoot = root.GetComponent<RectTransform>();
        rtRoot.sizeDelta = new Vector2(1000f, 440f);

        Image imgRoot = root.GetComponent<Image>();
        if (imgRoot == null) imgRoot = root.AddComponent<Image>();
        imgRoot.sprite = sprRound36;
        imgRoot.type = Image.Type.Sliced;
        imgRoot.color = ColorCardBg;
        imgRoot.raycastTarget = true;

        Shadow shadowRoot = root.GetComponent<Shadow>();
        if (shadowRoot == null) shadowRoot = root.AddComponent<Shadow>();
        shadowRoot.effectColor = new Color(0f, 0f, 0f, 120f / 255f);
        shadowRoot.effectDistance = new Vector2(0f, -6f);

        Mask maskRoot = root.GetComponent<Mask>();
        if (maskRoot == null) maskRoot = root.AddComponent<Mask>();
        maskRoot.showMaskGraphic = true;

        Button btnRoot = root.GetComponent<Button>();
        if (btnRoot == null) btnRoot = root.AddComponent<Button>();
        btnRoot.targetGraphic = imgRoot;

        LayoutElement leRoot = root.GetComponent<LayoutElement>();
        if (leRoot == null) leRoot = root.AddComponent<LayoutElement>();
        leRoot.preferredHeight = 440f;
        leRoot.minHeight = 440f;
        leRoot.flexibleHeight = 0f;

        ActCardUI cardUI = root.GetComponent<ActCardUI>();
        if (cardUI == null) cardUI = root.AddComponent<ActCardUI>();
        cardUI.button = btnRoot;

        // [Child 0]: GlowBorder (nam dau tien trong danh sach con)
        Transform tGlow = root.transform.Find("GlowBorder");
        GameObject objGlow = (tGlow != null) ? tGlow.gameObject : new GameObject("GlowBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objGlow.transform.SetParent(root.transform, false);
        objGlow.transform.SetSiblingIndex(0);

        RectTransform rtGlow = objGlow.GetComponent<RectTransform>();
        rtGlow.anchorMin = Vector2.zero;
        rtGlow.anchorMax = Vector2.one;
        rtGlow.offsetMin = new Vector2(-10f, -10f);
        rtGlow.offsetMax = new Vector2(10f, 10f);

        Image imgGlow = objGlow.GetComponent<Image>();
        imgGlow.sprite = sprGlowRing;
        imgGlow.type = Image.Type.Sliced;
        imgGlow.color = new Color(ColorMint.r, ColorMint.g, ColorMint.b, 230f / 255f);
        imgGlow.raycastTarget = false;
        cardUI.glowBorder = imgGlow;

        // [Child 1]: BannerImage (cao 270, anchor top-stretch)
        Transform tBanner = root.transform.Find("BannerImage");
        GameObject objBanner = (tBanner != null) ? tBanner.gameObject : new GameObject("BannerImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objBanner.transform.SetParent(root.transform, false);
        objBanner.transform.SetSiblingIndex(1);

        RectTransform rtBanner = objBanner.GetComponent<RectTransform>();
        rtBanner.anchorMin = new Vector2(0f, 1f);
        rtBanner.anchorMax = new Vector2(1f, 1f);
        rtBanner.pivot = new Vector2(0.5f, 1f);
        rtBanner.anchoredPosition = Vector2.zero;
        rtBanner.sizeDelta = new Vector2(0f, 270f);

        Image imgBanner = objBanner.GetComponent<Image>();
        if (imgBanner.sprite == null) imgBanner.sprite = sprBannerDefault;
        imgBanner.preserveAspect = false;
        imgBanner.type = Image.Type.Simple;
        imgBanner.raycastTarget = false;
        cardUI.bannerImage = imgBanner;

        // Children of BannerImage:
        // 1. TitleGradient (dung truoc cac chu)
        Transform tGrad = objBanner.transform.Find("TitleGradient");
        GameObject objGrad = (tGrad != null) ? tGrad.gameObject : new GameObject("TitleGradient", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objGrad.transform.SetParent(objBanner.transform, false);
        objGrad.transform.SetSiblingIndex(0);

        RectTransform rtGrad = objGrad.GetComponent<RectTransform>();
        rtGrad.anchorMin = new Vector2(0f, 0f);
        rtGrad.anchorMax = new Vector2(1f, 0f);
        rtGrad.pivot = new Vector2(0.5f, 0f);
        rtGrad.anchoredPosition = Vector2.zero;
        rtGrad.sizeDelta = new Vector2(0f, 170f);

        Image imgGrad = objGrad.GetComponent<Image>();
        imgGrad.sprite = sprGradFade;
        imgGrad.type = Image.Type.Simple;
        imgGrad.color = Color.white;
        imgGrad.raycastTarget = false;

        // 2. SubtitleText (dong phu nho phia tren tieu de)
        Transform tSub = objBanner.transform.Find("SubtitleText");
        GameObject objSub = (tSub != null) ? tSub.gameObject : new GameObject("SubtitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objSub.transform.SetParent(objBanner.transform, false);

        RectTransform rtSub = objSub.GetComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0f, 0f);
        rtSub.anchorMax = new Vector2(0f, 0f);
        rtSub.pivot = new Vector2(0f, 0f);
        rtSub.anchoredPosition = new Vector2(28f, 74f);
        rtSub.sizeDelta = new Vector2(600f, 32f);

        TextMeshProUGUI tmpSub = objSub.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpSub.font = font;
        tmpSub.fontSize = 26f;
        tmpSub.fontStyle = FontStyles.Bold;
        tmpSub.color = ColorApricot;
        tmpSub.alignment = TextAlignmentOptions.Left;
        tmpSub.text = "KHỞI ĐẦU ĐÊM RẰM";
        tmpSub.raycastTarget = false;
        cardUI.subtitleText = tmpSub;

        // 3. TitleText (chinh)
        Transform tTitle = objBanner.transform.Find("TitleText");
        if (tTitle == null) tTitle = objBanner.transform.Find("TitleText ");
        GameObject objTitle = (tTitle != null) ? tTitle.gameObject : new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objTitle.name = "TitleText";
        objTitle.transform.SetParent(objBanner.transform, false);

        RectTransform rtTitle = objTitle.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0f, 0f);
        rtTitle.anchorMax = new Vector2(0f, 0f);
        rtTitle.pivot = new Vector2(0f, 0f);
        rtTitle.anchoredPosition = new Vector2(28f, 20f);
        rtTitle.sizeDelta = new Vector2(700f, 54f);

        TextMeshProUGUI tmpTitle = objTitle.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpTitle.font = font;
        tmpTitle.fontSize = 44f;
        tmpTitle.fontStyle = FontStyles.Bold;
        tmpTitle.color = ColorCream;
        tmpTitle.alignment = TextAlignmentOptions.Left;
        tmpTitle.overflowMode = TextOverflowModes.Ellipsis;
        tmpTitle.text = "HỒI 1 - LÀNG QUÊ THANH BÌNH";
        tmpTitle.raycastTarget = false;
        cardUI.titleText = tmpTitle;

        // 4. StarsRow (3 sao goc duoi-phai banner)
        Transform tStarsRow = objBanner.transform.Find("StarsRow");
        GameObject objStarsRow = (tStarsRow != null) ? tStarsRow.gameObject : new GameObject("StarsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        objStarsRow.transform.SetParent(objBanner.transform, false);

        RectTransform rtStarsRow = objStarsRow.GetComponent<RectTransform>();
        rtStarsRow.anchorMin = new Vector2(1f, 0f);
        rtStarsRow.anchorMax = new Vector2(1f, 0f);
        rtStarsRow.pivot = new Vector2(1f, 0f);
        rtStarsRow.anchoredPosition = new Vector2(-28f, 22f);
        rtStarsRow.sizeDelta = new Vector2(150f, 44f);

        HorizontalLayoutGroup hlgStars = objStarsRow.GetComponent<HorizontalLayoutGroup>();
        hlgStars.spacing = 6f;
        hlgStars.childAlignment = TextAnchor.MiddleRight;
        hlgStars.childControlWidth = false;
        hlgStars.childControlHeight = false;
        hlgStars.childForceExpandWidth = false;
        hlgStars.childForceExpandHeight = false;

        cardUI.starsRow = objStarsRow;
        List<Image> starImgList = new List<Image>();
        for (int i = 0; i < 3; i++)
        {
            string starName = "Star_" + (i + 1);
            Transform tStar = objStarsRow.transform.Find(starName);
            GameObject objStar = (tStar != null) ? tStar.gameObject : new GameObject(starName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objStar.transform.SetParent(objStarsRow.transform, false);

            RectTransform rtStar = objStar.GetComponent<RectTransform>();
            rtStar.sizeDelta = new Vector2(44f, 44f);

            Image imgStar = objStar.GetComponent<Image>();
            imgStar.sprite = sprSao;
            imgStar.color = (i == 0) ? ColorStarAchieved : ColorStarUnachieved;
            imgStar.raycastTarget = false;
            starImgList.Add(imgStar);
        }
        cardUI.starIcons = starImgList.ToArray();

        // 5. StatusBadge (pill anchor top-left Pos (28, -28), cao 52)
        Transform tBadge = objBanner.transform.Find("StatusBadge");
        GameObject objBadge = (tBadge != null) ? tBadge.gameObject : new GameObject("StatusBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        objBadge.transform.SetParent(objBanner.transform, false);

        RectTransform rtBadge = objBadge.GetComponent<RectTransform>();
        rtBadge.anchorMin = new Vector2(0f, 1f);
        rtBadge.anchorMax = new Vector2(0f, 1f);
        rtBadge.pivot = new Vector2(0f, 1f);
        rtBadge.anchoredPosition = new Vector2(28f, -28f);
        rtBadge.sizeDelta = new Vector2(200f, 52f);

        Image imgBadge = objBadge.GetComponent<Image>();
        imgBadge.sprite = sprPill;
        imgBadge.type = Image.Type.Sliced;
        imgBadge.color = ColorBadgeCompleted;
        imgBadge.raycastTarget = false;
        cardUI.statusBadge = objBadge;
        cardUI.statusBadgeBg = imgBadge;

        HorizontalLayoutGroup hlgBadge = objBadge.GetComponent<HorizontalLayoutGroup>();
        if (hlgBadge == null) hlgBadge = objBadge.AddComponent<HorizontalLayoutGroup>();
        hlgBadge.padding = new RectOffset(22, 22, 0, 0);
        hlgBadge.spacing = 10f;
        hlgBadge.childAlignment = TextAnchor.MiddleCenter;
        hlgBadge.childControlWidth = false;
        hlgBadge.childControlHeight = false;
        hlgBadge.childForceExpandWidth = false;
        hlgBadge.childForceExpandHeight = false;

        ContentSizeFitter csfBadge = objBadge.GetComponent<ContentSizeFitter>();
        if (csfBadge == null) csfBadge = objBadge.AddComponent<ContentSizeFitter>();
        csfBadge.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csfBadge.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Xoa cac component/child cu cua StatusBadge gay ra o vuong trang
        while (objBadge.transform.childCount > 0)
        {
            Object.DestroyImmediate(objBadge.transform.GetChild(0).gameObject);
        }

        // Them Dot ui_circle 14px
        GameObject objDot = new GameObject("BadgeDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objDot.transform.SetParent(objBadge.transform, false);
        RectTransform rtDot = objDot.GetComponent<RectTransform>();
        rtDot.sizeDelta = new Vector2(14f, 14f);
        Image imgDot = objDot.GetComponent<Image>();
        imgDot.sprite = sprCircle;
        imgDot.color = ColorCream;
        imgDot.raycastTarget = false;

        // Them StatusTxt
        GameObject objTxt = new GameObject("StatusTxt", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objTxt.transform.SetParent(objBadge.transform, false);
        TextMeshProUGUI tmpBadge = objTxt.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpBadge.font = font;
        tmpBadge.fontSize = 24f;
        tmpBadge.fontStyle = FontStyles.Bold;
        tmpBadge.color = ColorCream;
        tmpBadge.alignment = TextAlignmentOptions.Center;
        tmpBadge.text = "HOÀN THÀNH";
        tmpBadge.raycastTarget = false;
        cardUI.statusBadgeText = tmpBadge;

        // 6. ActionChevron (cho the InProgress: nut tron 64x64 ui_circle #FFD27A + chevron phai nau dam)
        Transform tChev = objBanner.transform.Find("ActionChevron");
        GameObject objChev = (tChev != null) ? tChev.gameObject : new GameObject("ActionChevron", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objChev.transform.SetParent(objBanner.transform, false);

        RectTransform rtChev = objChev.GetComponent<RectTransform>();
        rtChev.anchorMin = new Vector2(1f, 0f);
        rtChev.anchorMax = new Vector2(1f, 0f);
        rtChev.pivot = new Vector2(1f, 0f);
        rtChev.anchoredPosition = new Vector2(-48f, 20f);
        rtChev.sizeDelta = new Vector2(56f, 56f);

        Image imgChevBg = objChev.GetComponent<Image>();
        imgChevBg.sprite = sprCircle;
        imgChevBg.color = ColorGold;
        imgChevBg.raycastTarget = false;

        Transform tChevIcon = objChev.transform.Find("Icon");
        GameObject objChevIcon = (tChevIcon != null) ? tChevIcon.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objChevIcon.transform.SetParent(objChev.transform, false);
        RectTransform rtChevIcon = objChevIcon.GetComponent<RectTransform>();
        rtChevIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtChevIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtChevIcon.pivot = new Vector2(0.5f, 0.5f);
        rtChevIcon.anchoredPosition = Vector2.zero;
        rtChevIcon.sizeDelta = new Vector2(34f, 34f);
        Image imgChevIcon = objChevIcon.GetComponent<Image>();
        imgChevIcon.sprite = sprArrowRight;
        imgChevIcon.color = ColorDarkBrown;
        imgChevIcon.raycastTarget = false;

        cardUI.actionChevron = objChev;
        objChev.SetActive(false);

        // 7. ActionPill ("Bấm Chọn" pill #FFD27A, text nau dam cỡ 26)
        Transform tPill = objBanner.transform.Find("ActionPill");
        GameObject objPill = (tPill != null) ? tPill.gameObject : new GameObject("ActionPill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        objPill.transform.SetParent(objBanner.transform, false);

        RectTransform rtPill = objPill.GetComponent<RectTransform>();
        rtPill.anchorMin = new Vector2(1f, 0f);
        rtPill.anchorMax = new Vector2(1f, 0f);
        rtPill.pivot = new Vector2(1f, 0f);
        rtPill.anchoredPosition = new Vector2(-28f, 22f);
        rtPill.sizeDelta = new Vector2(160f, 52f);

        Image imgPill = objPill.GetComponent<Image>();
        imgPill.sprite = sprPill;
        imgPill.type = Image.Type.Sliced;
        imgPill.color = ColorGold;
        imgPill.raycastTarget = false;

        HorizontalLayoutGroup hlgPill = objPill.GetComponent<HorizontalLayoutGroup>();
        if (hlgPill == null) hlgPill = objPill.AddComponent<HorizontalLayoutGroup>();
        hlgPill.padding = new RectOffset(20, 20, 0, 0);
        hlgPill.childAlignment = TextAnchor.MiddleCenter;
        hlgPill.childControlWidth = false;
        hlgPill.childControlHeight = false;

        ContentSizeFitter csfPill = objPill.GetComponent<ContentSizeFitter>();
        if (csfPill == null) csfPill = objPill.AddComponent<ContentSizeFitter>();
        csfPill.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csfPill.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        Transform tPillTxt = objPill.transform.Find("Text");
        GameObject objPillTxt = (tPillTxt != null) ? tPillTxt.gameObject : new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objPillTxt.transform.SetParent(objPill.transform, false);
        TextMeshProUGUI tmpPill = objPillTxt.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpPill.font = font;
        tmpPill.fontSize = 26f;
        tmpPill.fontStyle = FontStyles.Bold;
        tmpPill.color = ColorDarkBrown;
        tmpPill.alignment = TextAlignmentOptions.Center;
        tmpPill.text = "Bấm Chọn";
        tmpPill.raycastTarget = false;

        cardUI.actionPill = objPill;
        objPill.SetActive(false);

        // [Child 2]: InfoBar (ui_round_bottom_36, mau #101A3C, cao 170, chia 2 cot chip)
        Transform tInfo = root.transform.Find("InfoBar");
        GameObject objInfo = (tInfo != null) ? tInfo.gameObject : new GameObject("InfoBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup));
        objInfo.transform.SetParent(root.transform, false);
        objInfo.transform.SetSiblingIndex(2);

        RectTransform rtInfo = objInfo.GetComponent<RectTransform>();
        rtInfo.anchorMin = new Vector2(0f, 0f);
        rtInfo.anchorMax = new Vector2(1f, 0f);
        rtInfo.pivot = new Vector2(0.5f, 0f);
        rtInfo.anchoredPosition = Vector2.zero;
        rtInfo.sizeDelta = new Vector2(0f, 170f);

        Image imgInfo = objInfo.GetComponent<Image>();
        imgInfo.sprite = sprRoundBottom36;
        imgInfo.type = Image.Type.Sliced;
        imgInfo.color = ColorInfoBar;
        imgInfo.raycastTarget = false;

        HorizontalLayoutGroup hlgInfo = objInfo.GetComponent<HorizontalLayoutGroup>();
        if (hlgInfo == null) hlgInfo = objInfo.AddComponent<HorizontalLayoutGroup>();
        hlgInfo.padding = new RectOffset(20, 20, 20, 20);
        hlgInfo.spacing = 18f;
        hlgInfo.childAlignment = TextAnchor.MiddleCenter;
        hlgInfo.childControlWidth = true;
        hlgInfo.childControlHeight = true;
        hlgInfo.childForceExpandWidth = true;
        hlgInfo.childForceExpandHeight = true;

        // Xoa children cu cua InfoBar de rebuild chuan
        while (objInfo.transform.childCount > 0)
        {
            Object.DestroyImmediate(objInfo.transform.GetChild(0).gameObject);
        }

        // Left Chip (Tien Do Ai)
        GameObject objLeftChip = CreateInfoChip("LeftInfo", sprPill, sprCircle, sprSao, ColorMint, "TIẾN ĐỘ ẢI", "5/5 Màn", font, out TMP_Text tmpLeftLabel, out TMP_Text tmpLeftVal);
        objLeftChip.transform.SetParent(objInfo.transform, false);
        cardUI.progressLabelText = tmpLeftLabel;
        cardUI.progressText = tmpLeftVal;

        // Right Chip (Thu Thap Sao)
        GameObject objRightChip = CreateInfoChip("RightInfo", sprPill, sprCircle, sprSao, ColorMint, "THU THẬP SAO", "15/15 Sao", font, out TMP_Text tmpRightLabel, out TMP_Text tmpRightVal);
        objRightChip.transform.SetParent(objInfo.transform, false);
        cardUI.starsLabelText = tmpRightLabel;
        cardUI.starsText = tmpRightVal;

        // [Child 3]: LockOverlay (the khoa: phu kin the, ui_round_36 den alpha 0.72)
        Transform tLock = root.transform.Find("LockOverlay");
        GameObject objLock = (tLock != null) ? tLock.gameObject : new GameObject("LockOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objLock.transform.SetParent(root.transform, false);
        objLock.transform.SetSiblingIndex(3);

        RectTransform rtLock = objLock.GetComponent<RectTransform>();
        rtLock.anchorMin = Vector2.zero;
        rtLock.anchorMax = Vector2.one;
        rtLock.offsetMin = Vector2.zero;
        rtLock.offsetMax = Vector2.zero;

        Image imgLock = objLock.GetComponent<Image>();
        imgLock.sprite = sprRound36;
        imgLock.type = Image.Type.Sliced;
        imgLock.color = new Color(0f, 0f, 0f, 0.72f);
        imgLock.raycastTarget = true;
        cardUI.lockOverlay = objLock;

        // Xoa children cu cua LockOverlay
        while (objLock.transform.childCount > 0)
        {
            Object.DestroyImmediate(objLock.transform.GetChild(0).gameObject);
        }

        // Vong vien ngoai LockRing
        GameObject objLockRing = new GameObject("LockRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objLockRing.transform.SetParent(objLock.transform, false);
        RectTransform rtLockRing = objLockRing.GetComponent<RectTransform>();
        rtLockRing.anchorMin = new Vector2(0.5f, 0.5f);
        rtLockRing.anchorMax = new Vector2(0.5f, 0.5f);
        rtLockRing.pivot = new Vector2(0.5f, 0.5f);
        rtLockRing.anchoredPosition = new Vector2(0f, 35f);
        rtLockRing.sizeDelta = new Vector2(124f, 124f);
        Image imgLockRing = objLockRing.GetComponent<Image>();
        imgLockRing.sprite = sprCircle;
        imgLockRing.color = new Color(ColorLavender.r, ColorLavender.g, ColorLavender.b, 0.6f);
        imgLockRing.raycastTarget = false;

        // Vong tron 120x120 #1C2A5C
        GameObject objLockCircle = new GameObject("LockCircle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objLockCircle.transform.SetParent(objLockRing.transform, false);
        RectTransform rtLockCircle = objLockCircle.GetComponent<RectTransform>();
        rtLockCircle.anchorMin = new Vector2(0.5f, 0.5f);
        rtLockCircle.anchorMax = new Vector2(0.5f, 0.5f);
        rtLockCircle.pivot = new Vector2(0.5f, 0.5f);
        rtLockCircle.anchoredPosition = Vector2.zero;
        rtLockCircle.sizeDelta = new Vector2(120f, 120f);
        Image imgLockCircle = objLockCircle.GetComponent<Image>();
        imgLockCircle.sprite = sprCircle;
        imgLockCircle.color = ColorChip;
        imgLockCircle.raycastTarget = false;

        // Icon o khoa kem
        GameObject objLockIcon = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objLockIcon.transform.SetParent(objLockCircle.transform, false);
        RectTransform rtLockIcon = objLockIcon.GetComponent<RectTransform>();
        rtLockIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtLockIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtLockIcon.pivot = new Vector2(0.5f, 0.5f);
        rtLockIcon.anchoredPosition = Vector2.zero;
        rtLockIcon.sizeDelta = new Vector2(56f, 56f);
        Image imgLockIcon = objLockIcon.GetComponent<Image>();
        imgLockIcon.sprite = sprLocked;
        imgLockIcon.color = ColorCream;
        imgLockIcon.raycastTarget = false;

        // Chu "ĐANG BỊ KHÓA"
        GameObject objLockTitle = new GameObject("LockTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objLockTitle.transform.SetParent(objLock.transform, false);
        RectTransform rtLockTitle = objLockTitle.GetComponent<RectTransform>();
        rtLockTitle.anchorMin = new Vector2(0.5f, 0.5f);
        rtLockTitle.anchorMax = new Vector2(0.5f, 0.5f);
        rtLockTitle.pivot = new Vector2(0.5f, 0.5f);
        rtLockTitle.anchoredPosition = new Vector2(0f, -60f);
        rtLockTitle.sizeDelta = new Vector2(600f, 40f);
        TextMeshProUGUI tmpLockTitle = objLockTitle.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpLockTitle.font = font;
        tmpLockTitle.fontSize = 32f;
        tmpLockTitle.fontStyle = FontStyles.Bold;
        tmpLockTitle.color = ColorSubText;
        tmpLockTitle.alignment = TextAlignmentOptions.Center;
        tmpLockTitle.text = "ĐANG BỊ KHÓA";
        tmpLockTitle.raycastTarget = false;

        // Dong "Hoàn thành Hồi 2 để mở khóa"
        GameObject objLockDesc = new GameObject("LockText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objLockDesc.transform.SetParent(objLock.transform, false);
        RectTransform rtLockDesc = objLockDesc.GetComponent<RectTransform>();
        rtLockDesc.anchorMin = new Vector2(0.5f, 0.5f);
        rtLockDesc.anchorMax = new Vector2(0.5f, 0.5f);
        rtLockDesc.pivot = new Vector2(0.5f, 0.5f);
        rtLockDesc.anchoredPosition = new Vector2(0f, -100f);
        rtLockDesc.sizeDelta = new Vector2(700f, 36f);
        TextMeshProUGUI tmpLockDesc = objLockDesc.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpLockDesc.font = font;
        tmpLockDesc.fontSize = 26f;
        tmpLockDesc.color = new Color(ColorSubText.r, ColorSubText.g, ColorSubText.b, 0.85f);
        tmpLockDesc.alignment = TextAlignmentOptions.Center;
        tmpLockDesc.text = "Hoàn thành Hồi 2 để mở khóa";
        tmpLockDesc.raycastTarget = false;
        cardUI.lockText = tmpLockDesc;

        objLock.SetActive(false);

        // CanvasGroup cho animation intro
        cardUI.canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();

        // Luu Prefab
        PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[RebuildActSelectUI] Prefab saved successfully!");
    }

    private static GameObject CreateInfoChip(string chipName, Sprite sprPill, Sprite sprCircle, Sprite sprIcon, Color glowColor, string label, string val, TMP_FontAsset font, out TMP_Text outLabel, out TMP_Text outVal)
    {
        GameObject chip = new GameObject(chipName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup));
        Image imgChip = chip.GetComponent<Image>();
        imgChip.sprite = sprPill;
        imgChip.type = Image.Type.Sliced;
        imgChip.color = ColorChip;
        imgChip.raycastTarget = false;

        HorizontalLayoutGroup hlg = chip.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.spacing = 16f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Icon badge tron
        GameObject badge = new GameObject("IconBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badge.transform.SetParent(chip.transform, false);
        RectTransform rtBadge = badge.GetComponent<RectTransform>();
        rtBadge.sizeDelta = new Vector2(56f, 56f);
        Image imgBadge = badge.GetComponent<Image>();
        imgBadge.sprite = sprCircle;
        imgBadge.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.25f);
        imgBadge.raycastTarget = false;

        // Icon ben trong badge
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(badge.transform, false);
        RectTransform rtIcon = icon.GetComponent<RectTransform>();
        rtIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtIcon.pivot = new Vector2(0.5f, 0.5f);
        rtIcon.anchoredPosition = Vector2.zero;
        rtIcon.sizeDelta = new Vector2(34f, 34f);
        Image imgIcon = icon.GetComponent<Image>();
        imgIcon.sprite = sprIcon;
        imgIcon.color = ColorGold;
        imgIcon.raycastTarget = false;

        // Text Container (2 dong)
        GameObject textContainer = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup));
        textContainer.transform.SetParent(chip.transform, false);
        VerticalLayoutGroup vlg = textContainer.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f;
        vlg.childAlignment = TextAnchor.MiddleLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Dong nhan nho
        GameObject objLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objLabel.transform.SetParent(textContainer.transform, false);
        TextMeshProUGUI tmpLabel = objLabel.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpLabel.font = font;
        tmpLabel.fontSize = 22f;
        tmpLabel.color = ColorSubText;
        tmpLabel.text = label;
        tmpLabel.raycastTarget = false;
        outLabel = tmpLabel;

        // Dong gia tri lon
        GameObject objVal = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objVal.transform.SetParent(textContainer.transform, false);
        TextMeshProUGUI tmpVal = objVal.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpVal.font = font;
        tmpVal.fontSize = 34f;
        tmpVal.fontStyle = FontStyles.Bold;
        tmpVal.color = ColorCream;
        tmpVal.text = val;
        tmpVal.raycastTarget = false;
        outVal = tmpVal;

        return chip;
    }

    private static void RebuildScene(
        Sprite sprBgGrad, Sprite sprCircle, Sprite sprPill, Sprite sprRound36,
        Sprite sprRoundTop36, Sprite sprSao, Sprite sprKimcuong, Sprite sprGear,
        Sprite sprArrowLeft, Sprite sprBanner1, Sprite sprBanner2, Sprite sprBanner3,
        TMP_FontAsset font)
    {
        Debug.Log("[RebuildActSelectUI] Rebuilding Scene: " + SCENE_PATH);

        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.path != SCENE_PATH)
        {
            EditorSceneManager.OpenScene(SCENE_PATH);
        }

        // 1. Tim Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Khong tim thay Canvas trong scene!");
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 2. Tim ActSelectView
        Transform tActSelect = canvas.transform.Find("ActSelectView");
        if (tActSelect == null)
        {
            Debug.LogError("Khong tim thay ActSelectView trong Canvas!");
            return;
        }
        GameObject actSelectObj = tActSelect.gameObject;
        RectTransform rtActSelect = actSelectObj.GetComponent<RectTransform>();
        rtActSelect.anchorMin = Vector2.zero;
        rtActSelect.anchorMax = Vector2.one;
        rtActSelect.offsetMin = Vector2.zero;
        rtActSelect.offsetMax = Vector2.zero;

        // 3. MainBackground
        Transform tBg = actSelectObj.transform.Find("MainBackground");
        GameObject objBg = (tBg != null) ? tBg.gameObject : new GameObject("MainBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objBg.transform.SetParent(actSelectObj.transform, false);
        objBg.transform.SetSiblingIndex(0);

        RectTransform rtBg = objBg.GetComponent<RectTransform>();
        rtBg.anchorMin = Vector2.zero;
        rtBg.anchorMax = Vector2.one;
        rtBg.offsetMin = Vector2.zero;
        rtBg.offsetMax = Vector2.zero;

        Image imgBg = objBg.GetComponent<Image>();
        imgBg.sprite = sprBgGrad;
        imgBg.type = Image.Type.Simple;
        imgBg.color = Color.white;
        imgBg.raycastTarget = false;

        // BackgroundStars (16 cham sang nho rải ngau nhien)
        Transform tStars = objBg.transform.Find("BackgroundStars");
        GameObject objStars = (tStars != null) ? tStars.gameObject : new GameObject("BackgroundStars", typeof(RectTransform), typeof(BackgroundTwinkleStars));
        objStars.transform.SetParent(objBg.transform, false);
        RectTransform rtStars = objStars.GetComponent<RectTransform>();
        rtStars.anchorMin = Vector2.zero;
        rtStars.anchorMax = Vector2.one;
        rtStars.offsetMin = Vector2.zero;
        rtStars.offsetMax = Vector2.zero;

        // Xoa cac cham sao cu
        while (objStars.transform.childCount > 0)
        {
            Object.DestroyImmediate(objStars.transform.GetChild(0).gameObject);
        }

        // Tao 16 cham sang nho
        Vector2[] starPositions = new Vector2[]
        {
            new Vector2(-380f, 720f), new Vector2(350f, 800f), new Vector2(-200f, 600f), new Vector2(180f, 520f),
            new Vector2(-420f, 300f), new Vector2(400f, 250f), new Vector2(-150f, 100f), new Vector2(300f, -50f),
            new Vector2(-350f, -200f), new Vector2(220f, -280f), new Vector2(-180f, -450f), new Vector2(380f, -500f),
            new Vector2(-400f, -700f), new Vector2(150f, -750f), new Vector2(-100f, 850f), new Vector2(420f, -800f)
        };

        for (int i = 0; i < starPositions.Length; i++)
        {
            GameObject dot = new GameObject("StarDot_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dot.transform.SetParent(objStars.transform, false);
            RectTransform rtd = dot.GetComponent<RectTransform>();
            rtd.anchorMin = new Vector2(0.5f, 0.5f);
            rtd.anchorMax = new Vector2(0.5f, 0.5f);
            rtd.pivot = new Vector2(0.5f, 0.5f);
            rtd.anchoredPosition = starPositions[i];
            float sz = 4f + (i % 4) * 2f; // 4 to 10px
            rtd.sizeDelta = new Vector2(sz, sz);

            Image imgd = dot.GetComponent<Image>();
            imgd.sprite = sprCircle;
            float a = 0.15f + (i % 5) * 0.08f;
            imgd.color = new Color(ColorCream.r, ColorCream.g, ColorCream.b, a);
            imgd.raycastTarget = false;
        }

        // 4. SafeAreaRoot
        Transform tSafe = actSelectObj.transform.Find("SafeAreaRoot");
        GameObject objSafe = (tSafe != null) ? tSafe.gameObject : new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeArea));
        objSafe.transform.SetParent(actSelectObj.transform, false);
        objSafe.transform.SetSiblingIndex(1);

        RectTransform rtSafe = objSafe.GetComponent<RectTransform>();
        rtSafe.anchorMin = Vector2.zero;
        rtSafe.anchorMax = Vector2.one;
        rtSafe.offsetMin = Vector2.zero;
        rtSafe.offsetMax = Vector2.zero;
        if (objSafe.GetComponent<SafeArea>() == null) objSafe.AddComponent<SafeArea>();

        // 5. TopBar (trong SafeAreaRoot, cao 120, anchor top-stretch)
        Transform tTopBar = objSafe.transform.Find("TopBar");
        GameObject objTopBar = (tTopBar != null) ? tTopBar.gameObject : new GameObject("TopBar", typeof(RectTransform));
        objTopBar.transform.SetParent(objSafe.transform, false);

        RectTransform rtTopBar = objTopBar.GetComponent<RectTransform>();
        rtTopBar.anchorMin = new Vector2(0f, 1f);
        rtTopBar.anchorMax = new Vector2(1f, 1f);
        rtTopBar.pivot = new Vector2(0.5f, 1f);
        rtTopBar.anchoredPosition = Vector2.zero;
        rtTopBar.sizeDelta = new Vector2(0f, 120f);

        // Clear children cua TopBar
        while (objTopBar.transform.childCount > 0)
        {
            Object.DestroyImmediate(objTopBar.transform.GetChild(0).gameObject);
        }

        // StarPill (ui_pill, #1C2A5C alpha 0.9, icon sao + so kem 36 Bold, padding ngang 24)
        GameObject objStarPill = CreateResourcePill("StarPill", sprPill, sprSao, ColorGold, "348", 40f, font);
        objStarPill.transform.SetParent(objTopBar.transform, false);

        // DiamondPill (ui_pill, #1C2A5C alpha 0.9, icon kim cuong + so kem 36 Bold, padding ngang 24)
        GameObject objDiamondPill = CreateResourcePill("DiamondPill", sprPill, sprKimcuong, Color.white, "120", 270f, font);
        objDiamondPill.transform.SetParent(objTopBar.transform, false);

        // 6. HeaderBar (duoi TopBar, cao 120)
        Transform tHeader = objSafe.transform.Find("HeaderBar");
        GameObject objHeader = (tHeader != null) ? tHeader.gameObject : new GameObject("HeaderBar", typeof(RectTransform));
        objHeader.transform.SetParent(objSafe.transform, false);

        RectTransform rtHeader = objHeader.GetComponent<RectTransform>();
        rtHeader.anchorMin = new Vector2(0f, 1f);
        rtHeader.anchorMax = new Vector2(1f, 1f);
        rtHeader.pivot = new Vector2(0.5f, 1f);
        rtHeader.anchoredPosition = new Vector2(0f, -120f);
        rtHeader.sizeDelta = new Vector2(0f, 120f);

        while (objHeader.transform.childCount > 0)
        {
            Object.DestroyImmediate(objHeader.transform.GetChild(0).gameObject);
        }

        // BackBtn (ui_round_36 #FFD27A, icon arrowLeft #3A2200, 88x88, shadow nhe)
        GameObject objBackBtn = new GameObject("BackBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Shadow), typeof(Button));
        objBackBtn.transform.SetParent(objHeader.transform, false);
        RectTransform rtBack = objBackBtn.GetComponent<RectTransform>();
        rtBack.anchorMin = new Vector2(0f, 0.5f);
        rtBack.anchorMax = new Vector2(0f, 0.5f);
        rtBack.pivot = new Vector2(0f, 0.5f);
        rtBack.anchoredPosition = new Vector2(40f, 0f);
        rtBack.sizeDelta = new Vector2(88f, 88f);

        Image imgBack = objBackBtn.GetComponent<Image>();
        imgBack.sprite = sprRound36;
        imgBack.type = Image.Type.Sliced;
        imgBack.color = ColorGold;

        Shadow shBack = objBackBtn.GetComponent<Shadow>();
        shBack.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shBack.effectDistance = new Vector2(0f, -3f);

        GameObject objBackIcon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objBackIcon.transform.SetParent(objBackBtn.transform, false);
        RectTransform rtBackIcon = objBackIcon.GetComponent<RectTransform>();
        rtBackIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtBackIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtBackIcon.pivot = new Vector2(0.5f, 0.5f);
        rtBackIcon.anchoredPosition = Vector2.zero;
        rtBackIcon.sizeDelta = new Vector2(44f, 44f);
        Image imgBackIcon = objBackIcon.GetComponent<Image>();
        imgBackIcon.sprite = sprArrowLeft;
        imgBackIcon.color = ColorDarkBrown;
        imgBackIcon.raycastTarget = false;

        // Tieu de 2 dong can giua
        GameObject objTitles = new GameObject("Titles", typeof(RectTransform), typeof(VerticalLayoutGroup));
        objTitles.transform.SetParent(objHeader.transform, false);
        RectTransform rtTitles = objTitles.GetComponent<RectTransform>();
        rtTitles.anchorMin = new Vector2(0.5f, 0.5f);
        rtTitles.anchorMax = new Vector2(0.5f, 0.5f);
        rtTitles.pivot = new Vector2(0.5f, 0.5f);
        rtTitles.anchoredPosition = Vector2.zero;
        rtTitles.sizeDelta = new Vector2(620f, 110f);

        VerticalLayoutGroup vlgTitles = objTitles.GetComponent<VerticalLayoutGroup>();
        vlgTitles.spacing = 4f;
        vlgTitles.childAlignment = TextAnchor.MiddleCenter;
        vlgTitles.childControlWidth = true;
        vlgTitles.childControlHeight = false;
        vlgTitles.childForceExpandWidth = true;
        vlgTitles.childForceExpandHeight = false;

        // Dong nho: CHỌN HÀNH TRÌNH voi 2 cham tron ui_circle 8px 2 ben (tranh loi glyph)
        GameObject objSubRow = new GameObject("SubTitleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        objSubRow.transform.SetParent(objTitles.transform, false);
        HorizontalLayoutGroup hlgSub = objSubRow.GetComponent<HorizontalLayoutGroup>();
        hlgSub.spacing = 10f;
        hlgSub.childAlignment = TextAnchor.MiddleCenter;
        hlgSub.childControlWidth = false;
        hlgSub.childControlHeight = false;

        // Dot trai
        CreateSmallDecorDot("DotL", objSubRow, sprCircle);

        // Chu
        GameObject objSubHeader = new GameObject("SubHeader", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objSubHeader.transform.SetParent(objSubRow.transform, false);
        TextMeshProUGUI tmpSubH = objSubHeader.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpSubH.font = font;
        tmpSubH.fontSize = 26f;
        tmpSubH.fontStyle = FontStyles.Bold;
        tmpSubH.color = ColorGold;
        tmpSubH.alignment = TextAlignmentOptions.Center;
        tmpSubH.text = "CHỌN HÀNH TRÌNH";
        tmpSubH.raycastTarget = false;

        // Dot phai
        CreateSmallDecorDot("DotR", objSubRow, sprCircle);

        // Dong lon: HÀNH TRÌNH TRUNG THU
        GameObject objMainHeader = new GameObject("MainHeader", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objMainHeader.transform.SetParent(objTitles.transform, false);
        TextMeshProUGUI tmpMainH = objMainHeader.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpMainH.font = font;
        tmpMainH.fontSize = 48f;
        tmpMainH.fontStyle = FontStyles.Bold;
        tmpMainH.color = ColorCream;
        tmpMainH.alignment = TextAlignmentOptions.Center;
        tmpMainH.text = "HÀNH TRÌNH TRUNG THU";
        tmpMainH.raycastTarget = false;

        // TotalStarsPill ben phai (ui_pill, #1C2A5C, icon sao #FFD27A + "348/1860" kem co 30)
        GameObject objTotalStars = new GameObject("TotalStarsPill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        objTotalStars.transform.SetParent(objHeader.transform, false);
        RectTransform rtTotalStars = objTotalStars.GetComponent<RectTransform>();
        rtTotalStars.anchorMin = new Vector2(1f, 0.5f);
        rtTotalStars.anchorMax = new Vector2(1f, 0.5f);
        rtTotalStars.pivot = new Vector2(1f, 0.5f);
        rtTotalStars.anchoredPosition = new Vector2(-40f, 0f);
        rtTotalStars.sizeDelta = new Vector2(200f, 68f);

        Image imgTotalStars = objTotalStars.GetComponent<Image>();
        imgTotalStars.sprite = sprPill;
        imgTotalStars.type = Image.Type.Sliced;
        imgTotalStars.color = ColorChip;

        HorizontalLayoutGroup hlgTotalStars = objTotalStars.GetComponent<HorizontalLayoutGroup>();
        hlgTotalStars.padding = new RectOffset(20, 20, 0, 0);
        hlgTotalStars.spacing = 16f;
        hlgTotalStars.childAlignment = TextAnchor.MiddleCenter;
        hlgTotalStars.childControlWidth = false;
        hlgTotalStars.childControlHeight = false;

        ContentSizeFitter csfTotalStars = objTotalStars.GetComponent<ContentSizeFitter>();
        csfTotalStars.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csfTotalStars.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject objStarIco = new GameObject("StarIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objStarIco.transform.SetParent(objTotalStars.transform, false);
        RectTransform rtStarIco = objStarIco.GetComponent<RectTransform>();
        rtStarIco.sizeDelta = new Vector2(36f, 36f);
        Image imgStarIco = objStarIco.GetComponent<Image>();
        imgStarIco.sprite = sprSao;
        imgStarIco.color = ColorGold;
        imgStarIco.raycastTarget = false;

        GameObject objTotalStarsTxt = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objTotalStarsTxt.transform.SetParent(objTotalStars.transform, false);
        TextMeshProUGUI tmpTotalStars = objTotalStarsTxt.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpTotalStars.font = font;
        tmpTotalStars.fontSize = 30f;
        tmpTotalStars.fontStyle = FontStyles.Bold;
        tmpTotalStars.color = ColorCream;
        tmpTotalStars.alignment = TextAlignmentOptions.Center;
        tmpTotalStars.text = "22/45";
        tmpTotalStars.raycastTarget = false;

        // 7. Footer (o day SafeAreaRoot, cao 90, ui_round_top_36 #101A3C alpha 0.9)
        Transform tFooter = objSafe.transform.Find("Footer");
        GameObject objFooter = (tFooter != null) ? tFooter.gameObject : new GameObject("Footer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objFooter.transform.SetParent(objSafe.transform, false);

        RectTransform rtFooter = objFooter.GetComponent<RectTransform>();
        rtFooter.anchorMin = new Vector2(0f, 0f);
        rtFooter.anchorMax = new Vector2(1f, 0f);
        rtFooter.pivot = new Vector2(0.5f, 0f);
        rtFooter.anchoredPosition = Vector2.zero;
        rtFooter.sizeDelta = new Vector2(0f, 90f);

        Image imgFooter = objFooter.GetComponent<Image>();
        imgFooter.sprite = sprRoundTop36;
        imgFooter.type = Image.Type.Sliced;
        imgFooter.color = new Color(ColorInfoBar.r, ColorInfoBar.g, ColorInfoBar.b, 0.9f);
        imgFooter.raycastTarget = false;

        while (objFooter.transform.childCount > 0)
        {
            Object.DestroyImmediate(objFooter.transform.GetChild(0).gameObject);
        }

        GameObject objFooterTxt = new GameObject("TipText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objFooterTxt.transform.SetParent(objFooter.transform, false);
        RectTransform rtFooterTxt = objFooterTxt.GetComponent<RectTransform>();
        rtFooterTxt.anchorMin = Vector2.zero;
        rtFooterTxt.anchorMax = Vector2.one;
        rtFooterTxt.offsetMin = new Vector2(30f, 0f);
        rtFooterTxt.offsetMax = new Vector2(-30f, 0f);

        TextMeshProUGUI tmpFooter = objFooterTxt.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpFooter.font = font;
        tmpFooter.fontSize = 26f;
        tmpFooter.alignment = TextAlignmentOptions.Center;
        tmpFooter.textWrappingMode = TextWrappingModes.Normal;
        tmpFooter.text = "<color=#FFD27A>Mẹo:</color> <color=#A9B4D6>Đạt tối đa 3 sao mỗi màn để tích lũy rương kho báu Trung Thu!</color>";
        tmpFooter.raycastTarget = false;

        // 8. ActListScroll
        Transform tScroll = objSafe.transform.Find("ActListScroll");
        if (tScroll == null)
        {
            Debug.LogError("Khong tim thay ActListScroll!");
            return;
        }
        GameObject objScroll = tScroll.gameObject;
        RectTransform rtScroll = objScroll.GetComponent<RectTransform>();
        rtScroll.anchorMin = Vector2.zero;
        rtScroll.anchorMax = Vector2.one;
        // Khoang cach: TopBar(120) + HeaderBar(120) = 240, Footer = 90
        rtScroll.offsetMin = new Vector2(0f, 96f);
        rtScroll.offsetMax = new Vector2(0f, -244f);

        ScrollRect scrollRect = objScroll.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            if (scrollRect.verticalScrollbar != null)
            {
                scrollRect.verticalScrollbar.gameObject.SetActive(false);
                scrollRect.verticalScrollbar = null;
            }
        }

        // Xoa vung xam mac dinh
        Image imgScroll = objScroll.GetComponent<Image>();
        if (imgScroll != null)
        {
            imgScroll.color = new Color(0f, 0f, 0f, 0f);
            imgScroll.raycastTarget = false;
        }

        Transform tViewport = objScroll.transform.Find("Viewport");
        if (tViewport != null)
        {
            Image imgVp = tViewport.GetComponent<Image>();
            if (imgVp != null)
            {
                imgVp.color = new Color(0f, 0f, 0f, 0f);
                imgVp.raycastTarget = false;
            }

            Transform tContent = tViewport.Find("Content");
            if (tContent != null)
            {
                VerticalLayoutGroup vlgContent = tContent.GetComponent<VerticalLayoutGroup>();
                if (vlgContent == null) vlgContent = tContent.gameObject.AddComponent<VerticalLayoutGroup>();
                vlgContent.padding = new RectOffset(40, 40, 24, 24);
                vlgContent.spacing = 36f;
                vlgContent.childAlignment = TextAnchor.UpperCenter;
                vlgContent.childControlWidth = true;
                vlgContent.childControlHeight = false;
                vlgContent.childForceExpandWidth = true;
                vlgContent.childForceExpandHeight = false;

                ContentSizeFitter csfContent = tContent.GetComponent<ContentSizeFitter>();
                if (csfContent == null) csfContent = tContent.gameObject.AddComponent<ContentSizeFitter>();
                csfContent.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csfContent.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        // 9. PreviewController
        Transform tPrev = actSelectObj.transform.Find("PreviewController");
        GameObject objPrev = (tPrev != null) ? tPrev.gameObject : new GameObject("PreviewController");
        objPrev.transform.SetParent(actSelectObj.transform, false);

        ActSelectPreview previewComp = objPrev.GetComponent<ActSelectPreview>();
        if (previewComp == null) previewComp = objPrev.AddComponent<ActSelectPreview>();

        // Tim 3 the ActCard trong Content
        Transform tCard1 = tScroll.Find("Viewport/Content/ActCard_1");
        Transform tCard2 = tScroll.Find("Viewport/Content/ActCard_2");
        Transform tCard3 = tScroll.Find("Viewport/Content/ActCard_3");

        GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);

        if (tCard1 != null) previewComp.actCard1 = tCard1.GetComponent<ActCardUI>();
        if (tCard2 != null) previewComp.actCard2 = tCard2.GetComponent<ActCardUI>();
        if (tCard3 != null) previewComp.actCard3 = tCard3.GetComponent<ActCardUI>();

        previewComp.bannerAct1 = sprBanner1;
        previewComp.bannerAct2 = sprBanner2;
        previewComp.bannerAct3 = sprBanner3;
        previewComp.backButton = objBackBtn.GetComponent<Button>();

        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);
        Debug.Log("[RebuildActSelectUI] Scene saved successfully!");
    }

    private static GameObject CreateResourcePill(string name, Sprite sprPill, Sprite sprIcon, Color iconColor, string value, float posX, TMP_FontAsset font)
    {
        GameObject pill = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform rt = pill.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(posX, 0f);
        rt.sizeDelta = new Vector2(200f, 72f);

        Image img = pill.GetComponent<Image>();
        img.sprite = sprPill;
        img.type = Image.Type.Sliced;
        img.color = new Color(ColorChip.r, ColorChip.g, ColorChip.b, 0.9f);

        HorizontalLayoutGroup hlg = pill.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(24, 24, 0, 0);
        hlg.spacing = 14f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        ContentSizeFitter csf = pill.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject icoObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icoObj.transform.SetParent(pill.transform, false);
        RectTransform rtIco = icoObj.GetComponent<RectTransform>();
        rtIco.sizeDelta = new Vector2(42f, 42f);
        Image imgIco = icoObj.GetComponent<Image>();
        imgIco.sprite = sprIcon;
        imgIco.color = iconColor;
        imgIco.raycastTarget = false;

        GameObject valObj = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        valObj.transform.SetParent(pill.transform, false);
        TextMeshProUGUI tmpVal = valObj.GetComponent<TextMeshProUGUI>();
        if (font != null) tmpVal.font = font;
        tmpVal.fontSize = 36f;
        tmpVal.fontStyle = FontStyles.Bold;
        tmpVal.color = ColorCream;
        tmpVal.alignment = TextAlignmentOptions.Center;
        tmpVal.text = value;
        tmpVal.raycastTarget = false;

        return pill;
    }

    private static void CreateSmallDecorDot(string name, GameObject parent, Sprite sprCircle)
    {
        GameObject dot = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dot.transform.SetParent(parent.transform, false);
        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(8f, 8f);
        Image img = dot.GetComponent<Image>();
        img.sprite = sprCircle;
        img.color = ColorGold;
        img.raycastTarget = false;
    }
}
