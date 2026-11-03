using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Component điều khiển hiển thị 1 hàng mốc thưởng trong popup chi tiết màn.
    /// Thiết kế cân đối hoàn hảo giữa trạng thái ĐÃ NHẬN và CHƯA NHẬN.
    /// </summary>
    public class RewardRowUI : MonoBehaviour
    {
        [Header("Components")]
        public CanvasGroup rowCanvasGroup;
        public TMP_Text milestoneChipText;   // Số mốc "1", "2", "3"
        public Image milestoneStarIcon;      // Icon sao trong chip mốc
        public TMP_Text conditionText;

        [Header("Rewards Display")]
        public Image starIcon;
        public TMP_Text starAmountText;
        public GameObject diamondContainer;
        public Image diamondIcon;
        public TMP_Text diamondAmountText;

        [Header("Status Display - ĐÃ NHẬN")]
        public GameObject claimedBadge;          // Pill xanh ngọc đậm "ĐÃ NHẬN"
        public Image claimedBadgeBg;             // Nền pill
        public Image claimedCheckIcon;           // Icon tick trong badge
        public TMP_Text claimedBadgeText;        // Text "ĐÃ NHẬN"

        [Header("Status Display - CHƯA NHẬN")]
        public GameObject unclaimedBadge;        // Pill "CHƯA ĐẠT" viền #3A4676 nền #101A3C
        public Image unclaimedBadgeBorder;       // Viền mảnh #3A4676
        public Image unclaimedBadgeBg;           // Nền pill #101A3C
        public Image unclaimedLockIcon;          // Icon khóa 30x30 màu #8E98BC
        public TMP_Text unclaimedBadgeText;      // Text "CHƯA ĐẠT" cỡ 22

        [Header("Backward Compatibility")]
        public GameObject lockedStatus;          // Tương thích ngược
        public Image lockedIcon;                 // Tương thích ngược

        // Bảng màu chuẩn
        private static readonly Color ColorMintText = new Color(0.431f, 0.906f, 0.718f, 1f);     // #6EE7B7 (Chữ & tick ĐÃ NHẬN)
        private static readonly Color ColorEmeraldBg = new Color(0.078f, 0.231f, 0.176f, 1f);    // #143B2D (Nền pill ĐÃ NHẬN)
        
        private static readonly Color ColorSlateText = new Color(0.773f, 0.8f, 0.902f, 1f);     // #C5CCE6 (Chữ & lock CHƯA ĐẠT)
        private static readonly Color ColorBadgeBg = new Color(0.11f, 0.165f, 0.361f, 1f);       // #1C2A5C (Nền CHƯA ĐẠT)
        private static readonly Color ColorBadgeBorder = new Color(0.227f, 0.275f, 0.463f, 1f);  // #3A4676 (Viền mảnh CHƯA ĐẠT)

        private static readonly Color ColorCream = new Color(0.957f, 0.945f, 0.902f, 1f);        // #F4F1E6
        private static readonly Color ColorGold = new Color(1f, 0.824f, 0.478f, 1f);             // #FFD27A
        private static readonly Color ColorCyan = new Color(0.498f, 0.91f, 0.961f, 1f);          // #7FE8F5

        public void AutoBind()
        {
            if (rowCanvasGroup == null) rowCanvasGroup = GetComponent<CanvasGroup>();

            if (milestoneChipText == null)
            {
                Transform t = transform.Find("MilestoneChip/Text");
                if (t != null) milestoneChipText = t.GetComponent<TMP_Text>();
            }
            if (milestoneStarIcon == null)
            {
                Transform t = transform.Find("MilestoneChip/StarIcon");
                if (t != null) milestoneStarIcon = t.GetComponent<Image>();
            }

            if (conditionText == null)
            {
                Transform t = transform.Find("Condition");
                if (t != null) conditionText = t.GetComponent<TMP_Text>();
            }

            if (starIcon == null)
            {
                Transform t = transform.Find("RewardCluster/StarReward/Icon");
                if (t != null) starIcon = t.GetComponent<Image>();
            }
            if (starAmountText == null)
            {
                Transform t = transform.Find("RewardCluster/StarReward/Amount");
                if (t != null) starAmountText = t.GetComponent<TMP_Text>();
            }
            if (diamondContainer == null)
            {
                Transform t = transform.Find("RewardCluster/DiamondReward");
                if (t != null) diamondContainer = t.gameObject;
            }
            if (diamondIcon == null && diamondContainer != null)
            {
                Transform t = diamondContainer.transform.Find("Icon");
                if (t != null) diamondIcon = t.GetComponent<Image>();
            }
            if (diamondAmountText == null && diamondContainer != null)
            {
                Transform t = diamondContainer.transform.Find("Amount");
                if (t != null) diamondAmountText = t.GetComponent<TMP_Text>();
            }

            Transform statusGroup = transform.Find("StatusGroup");
            if (statusGroup != null)
            {
                if (claimedBadge == null)
                {
                    Transform t = statusGroup.Find("ClaimedBadge");
                    if (t != null) claimedBadge = t.gameObject;
                }
                if (unclaimedBadge == null)
                {
                    Transform t = statusGroup.Find("UnclaimedBadge");
                    if (t != null) unclaimedBadge = t.gameObject;
                }
            }

            if (claimedBadge != null)
            {
                if (claimedBadgeBg == null) claimedBadgeBg = claimedBadge.GetComponent<Image>();
                if (claimedCheckIcon == null)
                {
                    Transform t = claimedBadge.transform.Find("Icon");
                    if (t != null) claimedCheckIcon = t.GetComponent<Image>();
                }
                if (claimedBadgeText == null)
                {
                    Transform t = claimedBadge.transform.Find("Text");
                    if (t != null) claimedBadgeText = t.GetComponent<TMP_Text>();
                }
            }

            if (unclaimedBadge != null)
            {
                if (unclaimedBadgeBorder == null) unclaimedBadgeBorder = unclaimedBadge.GetComponent<Image>();
                Transform fillTrans = unclaimedBadge.transform.Find("Fill");
                if (fillTrans != null && unclaimedBadgeBg == null) unclaimedBadgeBg = fillTrans.GetComponent<Image>();
                if (unclaimedBadgeBg == null) unclaimedBadgeBg = unclaimedBadge.GetComponent<Image>();

                if (unclaimedLockIcon == null)
                {
                    Transform t = unclaimedBadge.transform.Find("Icon") ?? (fillTrans != null ? fillTrans.Find("Icon") : null);
                    if (t != null) unclaimedLockIcon = t.GetComponent<Image>();
                }
                if (unclaimedBadgeText == null)
                {
                    Transform t = unclaimedBadge.transform.Find("Text") ?? (fillTrans != null ? fillTrans.Find("Text") : null);
                    if (t != null) unclaimedBadgeText = t.GetComponent<TMP_Text>();
                }
            }

            if (lockedStatus == null) lockedStatus = (unclaimedBadge != null) ? unclaimedBadge : (statusGroup != null ? statusGroup.Find("LockedIcon")?.gameObject : null);
            if (lockedIcon == null) lockedIcon = (unclaimedLockIcon != null) ? unclaimedLockIcon : (lockedStatus != null ? lockedStatus.GetComponent<Image>() : null);
        }

        public void Setup(int milestone, string condition, int starAmount, int diamondAmount, bool isClaimed, Sprite sprStar, Sprite sprDiamond, Sprite sprLock, Sprite sprCheck, Sprite sprPill = null)
        {
            Setup(milestone, condition, starAmount, diamondAmount, isClaimed ? MilestoneState.Claimed : MilestoneState.Unachieved, sprStar, sprDiamond, sprLock, sprCheck, sprPill);
        }

        public void Setup(int milestone, string condition, int starAmount, int diamondAmount, MilestoneState state, Sprite sprStar, Sprite sprDiamond, Sprite sprLock, Sprite sprCheck, Sprite sprPill = null)
        {
            AutoBind();

            // 1. Chip mốc
            if (milestoneChipText != null)
            {
                milestoneChipText.text = milestone.ToString();
                milestoneChipText.color = ColorGold;
            }

            if (milestoneStarIcon != null)
            {
                if (sprStar != null) milestoneStarIcon.sprite = sprStar;
                milestoneStarIcon.color = ColorGold;
            }

            // 2. Nội dung điều kiện
            if (conditionText != null)
            {
                conditionText.text = condition;
                conditionText.color = (state == MilestoneState.Claimed) ? ColorSlateText : ColorCream;
            }

            // 3. Quà Sao
            if (starIcon != null && sprStar != null) starIcon.sprite = sprStar;
            if (starAmountText != null)
            {
                starAmountText.text = $"+{starAmount}";
                starAmountText.color = ColorGold;
            }

            // 4. Quà Kim Cương (Mốc 3 Boss)
            if (diamondContainer != null)
            {
                bool hasDiamonds = diamondAmount > 0;
                diamondContainer.SetActive(hasDiamonds);
                if (hasDiamonds)
                {
                    if (diamondIcon != null && sprDiamond != null) diamondIcon.sprite = sprDiamond;
                    if (diamondAmountText != null)
                    {
                        diamondAmountText.text = $"+{diamondAmount}";
                        diamondAmountText.color = ColorCyan;
                    }
                }
            }

            // 5. Cụm trạng thái cân đối: ĐÃ NHẬN vs CHỜ NHẬN vs CHƯA ĐẠT
            if (state == MilestoneState.Claimed)
            {
                if (claimedBadge != null)
                {
                    claimedBadge.SetActive(true);
                    if (claimedBadgeBg != null)
                    {
                        if (sprPill != null) claimedBadgeBg.sprite = sprPill;
                        claimedBadgeBg.color = ColorEmeraldBg;
                    }
                }
                if (claimedCheckIcon != null)
                {
                    claimedCheckIcon.gameObject.SetActive(true);
                    if (sprCheck != null) claimedCheckIcon.sprite = sprCheck;
                    claimedCheckIcon.color = ColorMintText;
                }
                if (claimedBadgeText != null)
                {
                    claimedBadgeText.text = "ĐÃ NHẬN";
                    claimedBadgeText.color = ColorMintText;
                }

                if (unclaimedBadge != null) unclaimedBadge.SetActive(false);
                if (lockedStatus != null && lockedStatus != unclaimedBadge) lockedStatus.SetActive(false);

                // Hàng đã nhận: mờ dịu đi bằng CanvasGroup alpha 0.68f
                if (rowCanvasGroup != null) rowCanvasGroup.alpha = 0.68f;
            }
            else if (state == MilestoneState.Pending)
            {
                if (claimedBadge != null)
                {
                    claimedBadge.SetActive(true);
                    if (claimedBadgeBg != null)
                    {
                        if (sprPill != null) claimedBadgeBg.sprite = sprPill;
                        claimedBadgeBg.color = new Color(0.961f, 0.62f, 0.259f, 0.95f); // #F59E42
                    }
                }
                if (claimedCheckIcon != null)
                {
                    claimedCheckIcon.gameObject.SetActive(false);
                }
                if (claimedBadgeText != null)
                {
                    claimedBadgeText.text = "CHỜ NHẬN";
                    claimedBadgeText.color = ColorCream;
                }

                if (unclaimedBadge != null) unclaimedBadge.SetActive(false);
                if (lockedStatus != null && lockedStatus != unclaimedBadge) lockedStatus.SetActive(false);

                if (rowCanvasGroup != null) rowCanvasGroup.alpha = 1.0f;
            }
            else
            {
                if (claimedBadge != null) claimedBadge.SetActive(false);

                if (unclaimedBadge != null)
                {
                    unclaimedBadge.SetActive(true);
                    if (unclaimedBadgeBorder != null)
                    {
                        if (sprPill != null) unclaimedBadgeBorder.sprite = sprPill;
                        unclaimedBadgeBorder.color = ColorBadgeBorder; // #3A4676
                    }
                    if (unclaimedBadgeBg != null)
                    {
                        if (sprPill != null) unclaimedBadgeBg.sprite = sprPill;
                        unclaimedBadgeBg.color = ColorBadgeBg; // #101A3C
                    }
                }
                if (unclaimedLockIcon != null)
                {
                    if (sprLock != null) unclaimedLockIcon.sprite = sprLock;
                    unclaimedLockIcon.color = ColorSlateText; // #C5CCE6
                    unclaimedLockIcon.preserveAspect = true;
                    LayoutElement leLock = unclaimedLockIcon.GetComponent<LayoutElement>();
                    if (leLock != null)
                    {
                        leLock.preferredWidth = 28f;
                        leLock.preferredHeight = 28f;
                        leLock.minWidth = 28f;
                        leLock.minHeight = 28f;
                    }
                    else
                    {
                        RectTransform rtLock = unclaimedLockIcon.GetComponent<RectTransform>();
                        if (rtLock != null) rtLock.sizeDelta = new Vector2(28f, 28f);
                    }
                }
                if (unclaimedBadgeText != null)
                {
                    unclaimedBadgeText.text = "CHƯA ĐẠT";
                    unclaimedBadgeText.enableAutoSizing = true;
                    unclaimedBadgeText.fontSizeMin = 18f;
                    unclaimedBadgeText.fontSizeMax = 22f;
                    unclaimedBadgeText.color = ColorSlateText; // #C5CCE6
                }

                if (lockedStatus != null && lockedStatus != unclaimedBadge)
                {
                    lockedStatus.SetActive(unclaimedBadge == null);
                    if (lockedIcon != null && sprLock != null) lockedIcon.sprite = sprLock;
                }

                // Hàng chưa nhận: sắc nét, nổi bật phần thưởng với alpha 1.0f
                if (rowCanvasGroup != null) rowCanvasGroup.alpha = 1.0f;
            }
        }
    }
}
