using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Script tạo dữ liệu mẫu và điều khiển hiển thị màn ActSelectView (LevelSelect scene).
/// GHI CHÚ: Đây là DỮ LIỆU MẪU để xem trước và kiểm tra giao diện,
/// sẽ được thay thế bằng dữ liệu thật từ LevelProgressManager ở bước phát triển tiếp theo.
/// </summary>
public class ActSelectPreview : MonoBehaviour
{
    [Header("3 Act Cards")]
    public ActCardUI actCard1;
    public ActCardUI actCard2;
    public ActCardUI actCard3;

    [Header("Act Banner Sprites")]
    public Sprite bannerAct1;
    public Sprite bannerAct2;
    public Sprite bannerAct3;

    [Header("Navigation Buttons")]
    public Button backButton;

    // Bảng màu theo thiết kế
    private readonly Color colorMint = new Color(0.498f, 0.89f, 0.753f, 1f);       // #7FE3C0 (Hồi 1)
    private readonly Color colorApricot = new Color(1f, 0.706f, 0.42f, 1f);         // #FFB46B (Hồi 2)
    private readonly Color colorLavender = new Color(0.663f, 0.608f, 1f, 1f);        // #A99BFF (Hồi 3)

    private void Awake()
    {
        // ActSelectPreview is deprecated in Stage 4, real data is handled by ActSelectView / LevelSelectFlow.
        enabled = false;
    }

    private System.Collections.IEnumerator Start()
    {
        AutoFindReferences();
        SetupHeaderBarStars();
        SetupPreviewData();
        SetupNavigation();

        // Chờ layout hoàn tất trước khi chạy animation
        Canvas.ForceUpdateCanvases();
        var content = GetComponentInParent<Canvas>()?.transform.Find("ActSelectView/SafeAreaRoot/ActListScroll/Viewport/Content") as RectTransform;
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        yield return null; // Chờ 1 frame để layout của VerticalLayoutGroup hoàn toàn ổn định

        PlayIntroAnimations();
    }

    private void SetupHeaderBarStars()
    {
        // Đảm bảo TotalStarsPill luôn hiển thị chính xác 22/45 với khoảng cách 16px
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        Transform totalStarsPill = rootCanvas?.transform.Find("ActSelectView/SafeAreaRoot/HeaderBar/TotalStarsPill");
        if (totalStarsPill == null)
        {
            GameObject go = GameObject.Find("TotalStarsPill");
            if (go != null) totalStarsPill = go.transform;
        }

        if (totalStarsPill != null)
        {
            TMPro.TMP_Text totalStarsText = totalStarsPill.GetComponentInChildren<TMPro.TMP_Text>();
            if (totalStarsText != null)
            {
                totalStarsText.text = "22/45";
                totalStarsText.ForceMeshUpdate();
            }

            HorizontalLayoutGroup hlg = totalStarsPill.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.spacing = 16f;
                hlg.padding.left = 20;
                hlg.padding.right = 20;
            }

            RectTransform pillRect = totalStarsPill as RectTransform;
            if (pillRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(pillRect);
            }
        }
    }

    private void AutoFindReferences()
    {
        if (actCard1 == null || actCard2 == null || actCard3 == null)
        {
            ActCardUI[] cards = FindObjectsByType<ActCardUI>(FindObjectsInactive.Include);
            foreach (var c in cards)
            {
                if (c.gameObject.name.EndsWith("1") || c.gameObject.name.Contains("ActCard_1") || c.gameObject.name.Contains("ActCard (0)")) actCard1 = c;
                else if (c.gameObject.name.EndsWith("2") || c.gameObject.name.Contains("ActCard_2") || c.gameObject.name.Contains("ActCard (1)")) actCard2 = c;
                else if (c.gameObject.name.EndsWith("3") || c.gameObject.name.Contains("ActCard_3") || c.gameObject.name.Contains("ActCard (2)")) actCard3 = c;
            }
        }

        if (backButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
            foreach (var b in buttons)
            {
                if (b.gameObject.name == "BackBtn")
                {
                    backButton = b;
                    break;
                }
            }
        }
    }

    private void SetupPreviewData()
    {
        // 1. HỒI 1 - LÀNG QUÊ THANH BÌNH (Completed)
        if (actCard1 != null)
        {
            actCard1.Setup(
                actTitle: "HỒI 1 - LÀNG QUÊ THANH BÌNH",
                banner: bannerAct1,
                levelsCleared: 5,
                totalLevels: 5,
                starsEarned: 15,
                starsTotal: 15,
                status: ActStatus.Completed,
                lockMessage: "",
                glowColor: colorMint,
                subtitle: "KHỞI ĐẦU ĐÊM RẰM",
                progressLabel: "TIẾN ĐỘ ẢI",
                starsLabel: "THU THẬP SAO"
            );

            if (actCard1.button != null)
            {
                actCard1.button.onClick.RemoveAllListeners();
                actCard1.button.onClick.AddListener(() =>
                {
                    Debug.Log("[ActSelectPreview] Clicked Hồi 1 - Làng Quê Thanh Bình -> Open LevelDetailView");
                    var flow = Object.FindAnyObjectByType<DemHoiDenLong.UI.LevelSelectFlow>();
                    if (flow != null) flow.TransitionToLevelDetail(1);
                    else LoadGameplayScene();
                });
            }
        }

        // 2. HỒI 2 - PHỐ CỔ ĐÈN LỒNG (InProgress)
        if (actCard2 != null)
        {
            actCard2.Setup(
                actTitle: "HỒI 2 - PHỐ CỔ ĐÈN LỒNG",
                banner: bannerAct2,
                levelsCleared: 3,
                totalLevels: 5,
                starsEarned: 7,
                starsTotal: 15,
                status: ActStatus.InProgress,
                lockMessage: "",
                glowColor: colorApricot,
                subtitle: "TÂM ĐIỂM LỄ HỘI",
                progressLabel: "MÀN HIỆN TẠI",
                starsLabel: "SAO ĐÃ THU THẬP"
            );

            if (actCard2.button != null)
            {
                actCard2.button.onClick.RemoveAllListeners();
                actCard2.button.onClick.AddListener(() =>
                {
                    Debug.Log("[ActSelectPreview] Clicked Hồi 2 - Phố Cổ Đèn Lồng -> Open LevelDetailView");
                    var flow = Object.FindAnyObjectByType<DemHoiDenLong.UI.LevelSelectFlow>();
                    if (flow != null) flow.TransitionToLevelDetail(2);
                    else LoadGameplayScene();
                });
            }
        }

        // 3. HỒI 3 - CUNG TRĂNG HUYỀN ẢO (Locked)
        if (actCard3 != null)
        {
            actCard3.Setup(
                actTitle: "HỒI 3 - CUNG TRĂNG HUYỀN ẢO",
                banner: bannerAct3,
                levelsCleared: 0,
                totalLevels: 5,
                starsEarned: 0,
                starsTotal: 15,
                status: ActStatus.Locked,
                lockMessage: "Hoàn thành Hồi 2 để mở khóa",
                glowColor: colorLavender,
                subtitle: "BÍ ẨN CUNG TRĂNG",
                progressLabel: "TIẾN ĐỘ ẢI",
                starsLabel: "THU THẬP SAO"
            );

            if (actCard3.button != null)
            {
                actCard3.button.onClick.RemoveAllListeners();
                actCard3.button.onClick.AddListener(() =>
                {
                    Debug.Log("[ActSelectPreview] Clicked Hồi 3 - Cung Trăng Huyền Ảo (Locked)");
                });
            }
        }
    }

    private void SetupNavigation()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnBackClicked()
    {
        DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.MainMenu, "ActSelectPreview_Back");
    }

    private void LoadGameplayScene()
    {
        DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.Gameplay, "ActSelectPreview_Play");
    }

    private void PlayIntroAnimations()
    {
        // Khi vào màn: 3 thẻ trượt từ dưới lên 60px và fade 0 -> 1, mỗi thẻ trễ 0.08s, mỗi thẻ 0.35s, ease-out
        if (actCard1 != null) actCard1.PlayIntroAnimation(0.00f);
        if (actCard2 != null) actCard2.PlayIntroAnimation(0.08f);
        if (actCard3 != null) actCard3.PlayIntroAnimation(0.16f);
    }
}
