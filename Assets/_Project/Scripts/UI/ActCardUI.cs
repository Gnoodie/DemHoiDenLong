using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum ActStatus { Locked, InProgress, Completed }

public class ActCardUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Original Required Fields")]
    public Image glowBorder;
    public Image bannerImage;
    public TMP_Text titleText;
    public TMP_Text progressText;
    public TMP_Text starsText;
    public GameObject statusBadge;
    public Image statusBadgeBg;
    public TMP_Text statusBadgeText;
    public GameObject lockOverlay;
    public TMP_Text lockText;
    public Button button;

    [Header("Màu theo Act (set trong Inspector hoặc qua data)")]
    public Color actGlowColor = Color.green;

    [Header("New Extended Fields")]
    public TMP_Text subtitleText;
    public GameObject starsRow;
    public Image[] starIcons;
    public TMP_Text progressLabelText;
    public TMP_Text starsLabelText;
    public GameObject actionChevron;
    public GameObject actionPill;
    public CanvasGroup canvasGroup;
    public Image statusBadgeDot;
    public Sprite checkSprite;
    public Sprite dotSprite;

    public System.Action<ActCardUI> onClickCallback;
    public System.Action<ActCardUI> onLockedClickCallback;
    public ActStatus CurrentStatus => currentStatus;
    public int ActIndex => actIndex;

    private ActStatus currentStatus;
    private int actIndex;
    private Coroutine glowPulseCoroutine;
    private Coroutine chevronFloatCoroutine;
    private Coroutine scaleCoroutine;
    private Coroutine shakeCoroutine;
    private Vector3 originalScale = Vector3.one;

    private readonly Color colorCream = new Color(0.957f, 0.945f, 0.902f, 1f);        // #F4F1E6
    private readonly Color colorDarkBrown = new Color(0.227f, 0.133f, 0f, 1f);          // #3A2200
    private readonly Color colorBadgeCompleted = new Color(0.18f, 0.62f, 0.478f, 1f);   // #2E9E7A
    private readonly Color colorBadgeProgress = new Color(0.961f, 0.62f, 0.259f, 1f);   // #F59E42
    private readonly Color colorStarAchieved = new Color(1f, 0.824f, 0.478f, 1f);       // #FFD27A
    private readonly Color colorStarUnachieved = new Color(0.29f, 0.333f, 0.502f, 1f);  // #4A5580

    private void Awake()
    {
        originalScale = transform.localScale;
        if (originalScale.sqrMagnitude < 0.01f)
        {
            originalScale = Vector3.one;
            transform.localScale = Vector3.one;
        }

        if (statusBadgeDot == null && statusBadge != null)
        {
            statusBadgeDot = statusBadge.transform.Find("BadgeDot")?.GetComponent<Image>();
        }

        // Đảm bảo click vào banner hoặc nút Play chuyển tiếp sự kiện click về button chính
        if (bannerImage != null)
        {
            Button bannerBtn = bannerImage.GetComponent<Button>();
            if (bannerBtn == null) bannerBtn = bannerImage.gameObject.AddComponent<Button>();
            bannerBtn.transition = Selectable.Transition.None;
            bannerBtn.onClick.RemoveAllListeners();
            bannerBtn.onClick.AddListener(() =>
            {
                if (button != null && button.interactable) button.onClick.Invoke();
            });
        }

        if (actionChevron != null)
        {
            Image chevronImg = actionChevron.GetComponent<Image>();
            if (chevronImg != null) chevronImg.raycastTarget = true;

            // Đảm bảo icon/hình bên trong không chặn raycast của button
            foreach (var img in actionChevron.GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject != actionChevron) img.raycastTarget = false;
            }

            Button chevronBtn = actionChevron.GetComponent<Button>();
            if (chevronBtn == null) chevronBtn = actionChevron.AddComponent<Button>();
            chevronBtn.transition = Selectable.Transition.None;
            chevronBtn.onClick.RemoveAllListeners();
            chevronBtn.onClick.AddListener(() =>
            {
                if (button != null && button.interactable) button.onClick.Invoke();
            });
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    private void OnEnable()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void Setup(string actTitle, Sprite banner, int levelsCleared, int totalLevels,
                       int starsEarned, int starsTotal, ActStatus status, string lockMessage,
                       Color glowColor)
    {
        Setup(actTitle, banner, levelsCleared, totalLevels, starsEarned, starsTotal, status, lockMessage, glowColor, null, null, null);
    }

    public void Setup(int actIndex, string actTitle, Sprite banner, int levelsCleared, int totalLevels,
                       int starsEarned, int starsTotal, ActStatus status, string lockMessage,
                       Color glowColor, string subtitle, string progressLabel, string starsLabel,
                       System.Action<ActCardUI> onClick, System.Action<ActCardUI> onLockedClick)
    {
        this.actIndex = actIndex;
        this.onClickCallback = onClick;
        this.onLockedClickCallback = onLockedClick;
        Setup(actTitle, banner, levelsCleared, totalLevels, starsEarned, starsTotal, status, lockMessage, glowColor, subtitle, progressLabel, starsLabel);
    }

    public void Setup(string actTitle, Sprite banner, int levelsCleared, int totalLevels,
                       int starsEarned, int starsTotal, ActStatus status, string lockMessage,
                       Color glowColor, string subtitle, string progressLabel, string starsLabel)
    {
        actGlowColor = glowColor;
        currentStatus = status;

        if (titleText != null) titleText.text = actTitle;
        if (bannerImage != null && banner != null) bannerImage.sprite = banner;
        if (progressText != null) progressText.text = $"{levelsCleared}/{totalLevels} Màn";
        if (starsText != null) starsText.text = $"{starsEarned}/{starsTotal} Sao";

        if (subtitleText != null && !string.IsNullOrEmpty(subtitle))
        {
            subtitleText.text = subtitle;
        }

        if (progressLabelText != null && !string.IsNullOrEmpty(progressLabel))
        {
            progressLabelText.text = progressLabel;
        }

        if (starsLabelText != null && !string.IsNullOrEmpty(starsLabel))
        {
            starsLabelText.text = starsLabel;
        }

        bool locked = status == ActStatus.Locked;

        // Tiêu đề và phụ đề: giữ nguyên màu, làm mờ nhẹ (alpha 0.7) khi khóa, 1.0 khi mở
        if (titleText != null)
        {
            Color c = titleText.color;
            c.a = locked ? 0.7f : 1.0f;
            titleText.color = c;
        }
        if (subtitleText != null)
        {
            Color c = subtitleText.color;
            c.a = locked ? 0.7f : 1.0f;
            subtitleText.color = c;
        }

        // Lớp khóa overlay: chỉ phủ tối banner tối đa alpha 0.45 để vẫn thấy ảnh; ẩn các chữ to ở giữa để không che tiêu đề
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(locked);
            if (locked)
            {
                Image lockImg = lockOverlay.GetComponent<Image>();
                if (lockImg != null) lockImg.color = new Color(0f, 0f, 0f, 0.45f);

                RectTransform lockRt = lockOverlay.GetComponent<RectTransform>();
                if (lockRt != null && bannerImage != null)
                {
                    lockRt.anchorMin = new Vector2(0f, 1f);
                    lockRt.anchorMax = new Vector2(1f, 1f);
                    lockRt.pivot = new Vector2(0.5f, 1f);
                    lockRt.anchoredPosition = Vector2.zero;
                    lockRt.sizeDelta = bannerImage.rectTransform.sizeDelta;
                }

                foreach (Transform child in lockOverlay.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        // Khung InfoBar: ẩn 2 chip tiến độ/sao của thẻ khóa, thay bằng LockedInfoRow căn giữa
        Transform infoBarTrans = transform.Find("InfoBar");
        if (infoBarTrans != null)
        {
            Transform leftInfoTrans = infoBarTrans.Find("LeftInfo");
            Transform rightInfoTrans = infoBarTrans.Find("RightInfo");
            if (leftInfoTrans != null) leftInfoTrans.gameObject.SetActive(!locked);
            if (rightInfoTrans != null) rightInfoTrans.gameObject.SetActive(!locked);

            if (locked)
            {
                UpdateLockedInfoRow(infoBarTrans, lockMessage);
            }
            else
            {
                Transform lockedRow = infoBarTrans.Find("LockedInfoRow");
                if (lockedRow != null) lockedRow.gameObject.SetActive(false);
            }
        }

        if (button != null)
        {
            button.interactable = true; // Cho phép tương tác để nhận click khi khóa (lắc thẻ)
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleCardClick);
        }

        if (statusBadge != null) statusBadge.SetActive(!locked);

        // Glow Border handling
        if (glowBorder != null)
        {
            if (locked)
            {
                glowBorder.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.35f);
            }
            else if (status == ActStatus.Completed)
            {
                glowBorder.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.95f);
            }
            else
            {
                glowBorder.color = new Color(glowColor.r, glowColor.g, glowColor.b, 1.0f);
            }
        }

        // Status Badge: icon và màu sắc
        if (!locked && statusBadge != null)
        {
            bool completed = (status == ActStatus.Completed);
            if (statusBadgeText != null)
            {
                statusBadgeText.text = completed ? "HOÀN THÀNH" : "ĐANG TIẾN CÔNG";
                statusBadgeText.color = new Color(1f, 0.99f, 0.96f, 1f); // Màu trắng kem cao cấp, tương phản nổi bật
                statusBadgeText.fontSize = 19f;
                statusBadgeText.fontStyle = FontStyles.Bold;
                statusBadgeText.textWrappingMode = TextWrappingModes.NoWrap;
                statusBadgeText.overflowMode = TextOverflowModes.Overflow;
                statusBadgeText.ForceMeshUpdate();
            }
            if (statusBadgeBg != null)
            {
                statusBadgeBg.color = completed 
                    ? new Color(0.18f, 0.62f, 0.48f, 0.95f)  // Emerald Mint
                    : new Color(0.95f, 0.45f, 0.08f, 0.95f); // Rich Warm Amber rực rỡ
            }
            if (statusBadgeDot != null)
            {
                if (completed)
                {
                    if (checkSprite != null) statusBadgeDot.sprite = checkSprite;
                    statusBadgeDot.color = new Color(1f, 0.99f, 0.96f, 1f);
                    statusBadgeDot.rectTransform.sizeDelta = new Vector2(14f, 14f);
                }
                else
                {
                    if (dotSprite != null) statusBadgeDot.sprite = dotSprite;
                    statusBadgeDot.color = new Color(1f, 0.95f, 0.85f, 1f);
                    statusBadgeDot.rectTransform.sizeDelta = new Vector2(10f, 10f);
                }
            }

            HorizontalLayoutGroup badgeHlg = statusBadge.GetComponent<HorizontalLayoutGroup>();
            if (badgeHlg != null)
            {
                badgeHlg.padding.left = 18;
                badgeHlg.padding.right = 18;
                badgeHlg.padding.top = 0;
                badgeHlg.padding.bottom = 0;
                badgeHlg.spacing = 8f;
                badgeHlg.childAlignment = TextAnchor.MiddleCenter;
            }

            // Tự động tính lại kích thước ôm trọn text với khoảng đệm thoải mái, không bao giờ tràn viền
            RectTransform badgeRect = statusBadge.GetComponent<RectTransform>();
            if (badgeRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(badgeRect);
                float minWidth = completed ? 200f : 255f;
                float targetW = Mathf.Max(minWidth, badgeRect.sizeDelta.x);
                badgeRect.sizeDelta = new Vector2(targetW, 40f);
            }
        }

        // Ẩn starsRow và actionPill trên banner (vì đã có thống kê ở dưới và thay bằng nút Play góc phải)
        if (starsRow != null) starsRow.SetActive(false);
        if (actionPill != null) actionPill.SetActive(false);

        // Cả Hồi hoàn thành và Đang tiến công đều có nút Play góc phải dưới banner
        bool canPlay = !locked;
        if (actionChevron != null)
        {
            actionChevron.SetActive(canPlay);
            actionChevron.transform.localScale = Vector3.one;
            Image chevronImg = actionChevron.GetComponent<Image>();
            if (chevronImg != null) chevronImg.raycastTarget = canPlay;

            foreach (var img in actionChevron.GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject != actionChevron) img.raycastTarget = false;
            }
        }

        // Animations cho Hồi Đang tiến công (InProgress)
        StopRunningAnimations();
        if (status == ActStatus.InProgress)
        {
            glowPulseCoroutine = StartCoroutine(GlowPulseRoutine());
            if (actionChevron != null)
            {
                chevronFloatCoroutine = StartCoroutine(ChevronPulseRoutine());
            }
        }
    }

    private void StopRunningAnimations()
    {
        if (glowPulseCoroutine != null)
        {
            StopCoroutine(glowPulseCoroutine);
            glowPulseCoroutine = null;
        }
        if (chevronFloatCoroutine != null)
        {
            StopCoroutine(chevronFloatCoroutine);
            chevronFloatCoroutine = null;
            if (actionChevron != null) actionChevron.transform.localScale = Vector3.one;
        }
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            Transform visual = transform.Find("Visual") ?? transform;
            visual.localRotation = Quaternion.identity;
        }
    }

    private void OnDisable()
    {
        StopRunningAnimations();
    }

    // Glow Border pulse alpha 0.65 <-> 1.0 cycle 1.6s
    private IEnumerator GlowPulseRoutine()
    {
        if (glowBorder == null) yield break;
        float elapsed = 0f;
        float cycle = 1.6f;
        while (true)
        {
            elapsed += Time.deltaTime;
            float t = (Mathf.Sin((elapsed / cycle) * Mathf.PI * 2f - Mathf.PI * 0.5f) + 1f) * 0.5f; // 0..1
            float alpha = Mathf.Lerp(0.65f, 1f, t);
            Color c = actGlowColor;
            c.a = alpha;
            glowBorder.color = c;
            yield return null;
        }
    }

    // Chevron pulse scale 1.0 <-> 1.08 nhẹ nhàng tại chỗ
    private IEnumerator ChevronPulseRoutine()
    {
        if (actionChevron == null) yield break;
        float elapsed = 0f;
        float cycle = 1.2f;
        Vector3 baseScale = Vector3.one;
        while (true)
        {
            elapsed += Time.deltaTime;
            float t = (Mathf.Sin((elapsed / cycle) * Mathf.PI * 2f) + 1f) * 0.5f; // 0..1
            float scale = Mathf.Lerp(1.0f, 1.08f, t);
            actionChevron.transform.localScale = baseScale * scale;
            yield return null;
        }
    }

    // Pointer press scale animation 0.97 on press, 1.0 on release
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        StartScaleTween(0.97f, 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartScaleTween(1.0f, 0.1f);
    }

    private int lastClickFrame = -1;

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleCardClick();
    }

    public void HandleCardClick()
    {
        if (Time.frameCount == lastClickFrame) return;
        lastClickFrame = Time.frameCount;

        if (currentStatus == ActStatus.Locked)
        {
            DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIError();
            PlayLockedShake();
            onLockedClickCallback?.Invoke(this);
        }
        else
        {
            DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIClick();
            onClickCallback?.Invoke(this);
        }
    }

    public void PlayLockedShake()
    {
        if (!gameObject.activeInHierarchy) return;
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Transform visual = transform.Find("Visual") ?? transform;
        float duration = 0.3f;
        float elapsed = 0f;
        float maxAngle = 3f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.Sin(t * Mathf.PI * 6f) * maxAngle * (1f - t);
            visual.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        visual.localRotation = Quaternion.identity;
        shakeCoroutine = null;
    }

    private void StartScaleTween(float targetScale, float duration)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTweenRoutine(targetScale, duration));
    }

    private IEnumerator ScaleTweenRoutine(float targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        transform.localScale = endScale;
    }

    private Coroutine introCoroutine;
    private Coroutine safetyTimeoutCoroutine;

    public void PlayIntroAnimation(float delay)
    {
        if (introCoroutine != null) StopCoroutine(introCoroutine);
        if (safetyTimeoutCoroutine != null) StopCoroutine(safetyTimeoutCoroutine);

        introCoroutine = StartCoroutine(IntroRoutine(delay));
        safetyTimeoutCoroutine = StartCoroutine(SafetyTimeoutRoutine(delay + 1.2f));
    }

    private IEnumerator SafetyTimeoutRoutine(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        transform.localScale = originalScale;
    }

    private IEnumerator IntroRoutine(float delay)
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        if (originalScale.sqrMagnitude < 0.01f)
        {
            originalScale = Vector3.one;
        }

        canvasGroup.alpha = 0f;
        transform.localScale = originalScale * 0.94f;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float duration = 0.35f;
        float elapsed = 0f;
        Vector3 startScale = originalScale * 0.94f;
        Vector3 endScale = originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-out quadratic
            float ease = 1f - (1f - t) * (1f - t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, ease);
            transform.localScale = Vector3.Lerp(startScale, endScale, ease);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = endScale;
    }

    private void UpdateLockedInfoRow(Transform infoBarTrans, string lockMessage)
    {
        Transform lockedRowTrans = infoBarTrans.Find("LockedInfoRow");
        GameObject rowObj;
        if (lockedRowTrans == null)
        {
            rowObj = new GameObject("LockedInfoRow", typeof(RectTransform));
            rowObj.transform.SetParent(infoBarTrans, false);
            RectTransform rt = rowObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(40f, 0f);
            rt.offsetMax = new Vector2(-40f, 0f);

            HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 1. Icon Container 56x56 (vòng tròn #1C2A5C, viền #A99BFF alpha 0.7)
            GameObject iconContainer = new GameObject("IconContainer", typeof(RectTransform));
            iconContainer.transform.SetParent(rowObj.transform, false);
            RectTransform iconRt = iconContainer.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(56f, 56f);

            // Ring viền ngoài #A99BFF alpha 0.7
            GameObject ringObj = new GameObject("Ring", typeof(RectTransform), typeof(Image));
            ringObj.transform.SetParent(iconContainer.transform, false);
            RectTransform ringRt = ringObj.GetComponent<RectTransform>();
            ringRt.anchorMin = Vector2.zero;
            ringRt.anchorMax = Vector2.one;
            ringRt.sizeDelta = Vector2.zero;
            Image ringImg = ringObj.GetComponent<Image>();
            ringImg.sprite = dotSprite != null ? dotSprite : checkSprite;
            ringImg.color = new Color(0.663f, 0.608f, 1f, 0.7f); // #A99BFF alpha 0.7
            ringImg.raycastTarget = false;

            // Circle nền trong 50x50 #1C2A5C
            GameObject circleObj = new GameObject("Circle", typeof(RectTransform), typeof(Image));
            circleObj.transform.SetParent(iconContainer.transform, false);
            RectTransform circleRt = circleObj.GetComponent<RectTransform>();
            circleRt.anchorMin = new Vector2(0.5f, 0.5f);
            circleRt.anchorMax = new Vector2(0.5f, 0.5f);
            circleRt.sizeDelta = new Vector2(50f, 50f);
            Image circleImg = circleObj.GetComponent<Image>();
            circleImg.sprite = ringImg.sprite;
            circleImg.color = new Color(0.11f, 0.165f, 0.361f, 1f); // #1C2A5C
            circleImg.raycastTarget = false;

            // Lock Icon 28x28 màu #E8E4FF
            GameObject lockIconObj = new GameObject("LockIcon", typeof(RectTransform), typeof(Image));
            lockIconObj.transform.SetParent(iconContainer.transform, false);
            RectTransform lockIconRt = lockIconObj.GetComponent<RectTransform>();
            lockIconRt.anchorMin = new Vector2(0.5f, 0.5f);
            lockIconRt.anchorMax = new Vector2(0.5f, 0.5f);
            lockIconRt.sizeDelta = new Vector2(28f, 28f);
            Image lockIconImg = lockIconObj.GetComponent<Image>();
#if UNITY_EDITOR
            Sprite uiLockSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_lock.png");
            if (uiLockSpr != null) lockIconImg.sprite = uiLockSpr;
            else lockIconImg.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/locked.png");
#endif
            lockIconImg.color = new Color(0.91f, 0.894f, 1f, 1f); // #E8E4FF
            lockIconImg.preserveAspect = true;
            lockIconImg.raycastTarget = false;

            // 2. Text Column
            GameObject textCol = new GameObject("TextCol", typeof(RectTransform));
            textCol.transform.SetParent(rowObj.transform, false);
            RectTransform textColRt = textCol.GetComponent<RectTransform>();
            textColRt.sizeDelta = new Vector2(700f, 68f);

            VerticalLayoutGroup vlg = textCol.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2f;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(textCol.transform, false);
            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            if (titleText != null) { titleTmp.font = titleText.font; titleTmp.fontSharedMaterial = titleText.fontSharedMaterial; }
            titleTmp.fontSize = 28f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = new Color(0.663f, 0.706f, 0.839f, 1f); // #A9B4D6
            titleTmp.text = "ĐANG BỊ KHÓA";
            titleTmp.raycastTarget = false;

            GameObject hintObj = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintObj.transform.SetParent(textCol.transform, false);
            TextMeshProUGUI hintTmp = hintObj.GetComponent<TextMeshProUGUI>();
            if (titleText != null) { hintTmp.font = titleText.font; hintTmp.fontSharedMaterial = titleText.fontSharedMaterial; }
            hintTmp.fontSize = 24f;
            hintTmp.fontStyle = FontStyles.Normal;
            hintTmp.color = new Color(0.663f, 0.706f, 0.839f, 0.85f); // #A9B4D6 alpha 0.85
            hintTmp.text = lockMessage;
            hintTmp.raycastTarget = false;
        }
        else
        {
            rowObj = lockedRowTrans.gameObject;
            rowObj.SetActive(true);

            // Đồng bộ kích thước và màu sắc nếu hàng đã được tạo trước đó
            Transform iconContainerT = rowObj.transform.Find("IconContainer");
            if (iconContainerT != null)
            {
                RectTransform irt = iconContainerT.GetComponent<RectTransform>();
                if (irt != null) irt.sizeDelta = new Vector2(56f, 56f);

                Transform ringT = iconContainerT.Find("Ring");
                if (ringT != null)
                {
                    Image rImg = ringT.GetComponent<Image>();
                    if (rImg != null) rImg.color = new Color(0.663f, 0.608f, 1f, 0.7f);
                }

                Transform circleT = iconContainerT.Find("Circle");
                if (circleT != null)
                {
                    RectTransform crt = circleT.GetComponent<RectTransform>();
                    if (crt != null) crt.sizeDelta = new Vector2(50f, 50f);
                    Image cImg = circleT.GetComponent<Image>();
                    if (cImg != null) cImg.color = new Color(0.11f, 0.165f, 0.361f, 1f);
                }

                Transform lockIconT = iconContainerT.Find("LockIcon");
                if (lockIconT != null)
                {
                    RectTransform lrt = lockIconT.GetComponent<RectTransform>();
                    if (lrt != null) lrt.sizeDelta = new Vector2(28f, 28f);
                    Image lImg = lockIconT.GetComponent<Image>();
                    if (lImg != null)
                    {
#if UNITY_EDITOR
                        Sprite uiLockSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_lock.png");
                        if (uiLockSpr != null) lImg.sprite = uiLockSpr;
#endif
                        lImg.color = new Color(0.91f, 0.894f, 1f, 1f); // #E8E4FF
                    }
                }
            }

            Transform hintT = rowObj.transform.Find("TextCol/Hint");
            if (hintT != null)
            {
                var hintTmp = hintT.GetComponent<TextMeshProUGUI>();
                if (hintTmp != null) hintTmp.text = lockMessage;
            }
        }
    }
}