using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    [DisallowMultipleComponent]
    public class LorePanelUI : MonoBehaviour
    {
        [Header("Canvas & Raycaster (Nested Canvas Optimization)")]
        [SerializeField] private Canvas nestedCanvas;
        [SerializeField] private GraphicRaycaster graphicRaycaster;

        [Header("Backdrop & Modal Container")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private Image backdropImage;
        [SerializeField] private Button backdropButton;
        [SerializeField] private RectTransform cardRect;
        [SerializeField] private CanvasGroup cardCanvasGroup;

        [Header("Medallion & Avatar (with Idle Bob)")]
        [SerializeField] private RectTransform medallionRect;
        [SerializeField] private Image medallionImage;
        [SerializeField] private Image lanAvatarImage;

        [Header("Header Elements")]
        [SerializeField] private TMP_Text loreSubheadText;
        [SerializeField] private Image ribbonImage;
        [SerializeField] private TMP_Text lanTitleText;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private Image dividerImage;

        [Header("Scroll Area & Story Content")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private TMP_Text storyBodyText;
        [SerializeField] private Scrollbar scrollbar;
        [System.Obsolete("Đã thay bằng RectMask2D.softness = (0, 40) trên Viewport")]
        [SerializeField] private Image topGradientFade;
        [System.Obsolete("Đã thay bằng RectMask2D.softness = (0, 40) trên Viewport")]
        [SerializeField] private Image bottomGradientFade;

        [Header("Skill Block (Fixed at Bottom, Solid Dark Bg)")]
        [SerializeField] private Image skillBlockBg;
        [SerializeField] private Image skillBlockIcon;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillDescText;

        // Backward compatibility references
        [SerializeField] private Image skillChipBg;
        [SerializeField] private Image skillChipIcon;
        [SerializeField] private TMP_Text skillChipText;

        [Header("Close Button (Red Pill)")]
        [SerializeField] private Button closePillBtn;
        [SerializeField] private Button closeBottomBtn;
        [System.Obsolete("Đã gộp vào nút closePillBtn duy nhất")]
        [SerializeField] private Button closeCornerBtn;

        private Coroutine animCoroutine;
        private Coroutine bobCoroutine;
        private Coroutine fallbackCoroutine;
        private bool isOpen = false;
        private LanData currentLan;
        private Vector2 medallionBasePos = Vector2.zero;

        // Lưu trạng thái các đối tượng MainMenu phía sau để ẩn/hiện
        private readonly List<GameObject> hiddenBackgroundObjects = new List<GameObject>();

        public bool IsOpen => isOpen;

        private void Awake()
        {
            EnsureComponents();

            if (medallionRect != null)
            {
                medallionBasePos = medallionRect.anchoredPosition;
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(Hide);
                backdropButton.onClick.AddListener(Hide);
            }
            if (closePillBtn != null)
            {
                closePillBtn.onClick.RemoveListener(Hide);
                closePillBtn.onClick.AddListener(Hide);
            }
            if (closeBottomBtn != null && closeBottomBtn != closePillBtn)
            {
                closeBottomBtn.onClick.RemoveListener(Hide);
                closeBottomBtn.onClick.AddListener(Hide);
            }
        }

        private void OnValidate()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (nestedCanvas == null) nestedCanvas = GetComponent<Canvas>();
            if (nestedCanvas == null)
            {
                nestedCanvas = gameObject.AddComponent<Canvas>();
            }
            nestedCanvas.overrideSorting = true;
            nestedCanvas.sortingOrder = 50;

            if (graphicRaycaster == null) graphicRaycaster = GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }

            if (panelCanvasGroup == null) panelCanvasGroup = GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (cardRect == null)
            {
                Transform ct = transform.Find("StoryCard");
                if (ct != null) cardRect = ct.GetComponent<RectTransform>();
            }

            if (cardCanvasGroup == null && cardRect != null)
            {
                cardCanvasGroup = cardRect.GetComponent<CanvasGroup>();
                if (cardCanvasGroup == null) cardCanvasGroup = cardRect.gameObject.AddComponent<CanvasGroup>();
            }

            // 6. Cấu hình Backdrop #0a1029 alpha 0.8 & raycast target (Mục 6)
            if (backdropButton == null)
            {
                Transform bdTr = transform.Find("Backdrop");
                if (bdTr != null) backdropButton = bdTr.GetComponent<Button>();
            }
            if (backdropButton != null)
            {
                backdropImage = backdropButton.GetComponent<Image>();
                backdropButton.onClick.RemoveListener(Hide);
                backdropButton.onClick.AddListener(Hide);
            }
            if (backdropImage != null)
            {
                ColorUtility.TryParseHtmlString("#0A1029", out Color bdColor);
                bdColor.a = 0.80f;
                backdropImage.color = bdColor;
                backdropImage.raycastTarget = true;
            }

            // 1. ScrollArea & Viewport softness = (0, 40), width = 760 (Mục 1)
            if (scrollRect == null && cardRect != null)
            {
                Transform sa = cardRect.Find("ScrollArea");
                if (sa != null) scrollRect = sa.GetComponent<ScrollRect>();
            }
            if (scrollRect != null)
            {
                RectTransform sRt = scrollRect.GetComponent<RectTransform>();
                if (sRt != null)
                {
                    sRt.sizeDelta = new Vector2(760f, 600f);
                    sRt.anchoredPosition = new Vector2(15f, -60f);
                }
                if (scrollRect.viewport != null)
                {
                    var rMask = scrollRect.viewport.GetComponent<RectMask2D>();
                    if (rMask != null)
                    {
                        rMask.softness = new Vector2Int(0, 40);
                    }
                }

                // Tắt/ẩn TopFade và BottomFade nếu có trong ScrollArea
                Transform tf = scrollRect.transform.Find("TopFade");
                if (tf != null) tf.gameObject.SetActive(false);
                Transform bf = scrollRect.transform.Find("BottomFade");
                if (bf != null) bf.gameObject.SetActive(false);
            }

            // 2. Đảm bảo padding Content (40, 40, 24, 24) (Mục 2)
            if (contentRect == null && scrollRect != null && scrollRect.viewport != null)
            {
                Transform c = scrollRect.viewport.Find("Content");
                if (c != null) contentRect = c.GetComponent<RectTransform>();
            }
            if (contentRect != null)
            {
                var vlg = contentRect.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.padding = new RectOffset(40, 40, 24, 24);
                }
            }

            // 3. Khối KỸ NĂNG: Tách khỏi ScrollRect, cố định ở đáy thẻ, nền đặc #152042 bo tròn (Mục 3)
            Transform sbTr = cardRect != null ? (cardRect.Find("SkillBlock") ?? cardRect.Find("SkillChip")) : null;
            if (sbTr != null)
            {
                if (sbTr.parent != cardRect) sbTr.SetParent(cardRect, false);
                RectTransform sbRt = sbTr.GetComponent<RectTransform>();
                if (sbRt != null)
                {
                    sbRt.anchorMin = new Vector2(0.5f, 0f);
                    sbRt.anchorMax = new Vector2(0.5f, 0f);
                    sbRt.pivot = new Vector2(0.5f, 0.5f);
                    sbRt.sizeDelta = new Vector2(760f, 110f);
                    sbRt.anchoredPosition = new Vector2(0f, 85f);
                }
                if (skillBlockBg == null) skillBlockBg = sbTr.GetComponent<Image>();
                if (skillBlockIcon == null)
                {
                    var iconTr = sbTr.Find("SkillIcon") ?? sbTr.Find("Icon");
                    if (iconTr != null) skillBlockIcon = iconTr.GetComponent<Image>();
                }
                if (skillNameText == null)
                {
                    var nameTr = sbTr.Find("TextColumn/SkillNameText") ?? sbTr.Find("SkillNameText") ?? sbTr.Find("Text");
                    if (nameTr != null) skillNameText = nameTr.GetComponent<TMP_Text>();
                }
                if (skillDescText == null)
                {
                    var descTr = sbTr.Find("TextColumn/SkillDescText") ?? sbTr.Find("SkillDescText") ?? sbTr.Find("Desc");
                    if (descTr != null) skillDescText = descTr.GetComponent<TMP_Text>();
                }
            }

            if (skillBlockBg != null)
            {
                ColorUtility.TryParseHtmlString("#152042", out Color chipBg);
                chipBg.a = 1.0f;
                skillBlockBg.color = chipBg;
                skillBlockBg.raycastTarget = false;
            }

            // 5. Xóa/ẩn nút X đỏ ở góc, giữ nút ĐÓNG pill đỏ #e34233 không đè viền khung (Mục 5)
            Transform cornerBtn = cardRect != null ? cardRect.Find("CloseButtonCorner") : null;
            if (cornerBtn != null) cornerBtn.gameObject.SetActive(false);
#pragma warning disable CS0618
            if (closeCornerBtn != null) closeCornerBtn.gameObject.SetActive(false);
#pragma warning restore CS0618

            Transform closeTr = transform.Find("ClosePillButton") ?? (cardRect != null ? cardRect.Find("CloseButtonBottom") : null);
            if (closeTr != null)
            {
                if (closeTr.parent == cardRect)
                {
                    closeTr.SetParent(transform, false);
                }
                RectTransform cbrt = closeTr.GetComponent<RectTransform>();
                if (cbrt != null)
                {
                    cbrt.anchorMin = new Vector2(0.5f, 0.5f);
                    cbrt.anchorMax = new Vector2(0.5f, 0.5f);
                    cbrt.pivot = new Vector2(0.5f, 0.5f);
                    cbrt.sizeDelta = new Vector2(360f, 100f);
                    cbrt.anchoredPosition = new Vector2(0f, -600f);
                }
                Image cImg = closeTr.GetComponent<Image>();
                if (cImg != null)
                {
                    ColorUtility.TryParseHtmlString("#E34233", out Color closeRed);
                    cImg.color = closeRed;
                    cImg.raycastTarget = true;
                }
                Button b = closeTr.GetComponent<Button>();
                if (b != null)
                {
                    b.onClick.RemoveListener(Hide);
                    b.onClick.AddListener(Hide);
                    closePillBtn = b;
                }
                TMP_Text cTxt = closeTr.GetComponentInChildren<TMP_Text>(true);
                if (cTxt != null)
                {
                    cTxt.text = "ĐÓNG";
                    cTxt.fontSize = 34f;
                    cTxt.fontStyle = FontStyles.Bold;
                    cTxt.color = Color.white;
                    cTxt.alignment = TextAlignmentOptions.Center;
                    cTxt.raycastTarget = false;
                }
            }

            // Tắt hai dải Image gradient cũ nếu còn sót lại (Mục 1)
#pragma warning disable CS0618
            if (topGradientFade != null) topGradientFade.gameObject.SetActive(false);
            if (bottomGradientFade != null) bottomGradientFade.gameObject.SetActive(false);
#pragma warning restore CS0618
        }

        private void OnEnable()
        {
            if (currentLan == null)
            {
                currentLan = LanCatalog.GetSelectedLan();
            }

            if (!isOpen)
            {
                isOpen = true;
                if (currentLan != null)
                {
                    UpdateContent(currentLan);
                }
            }
        }

        private void OnDisable()
        {
            RestoreBackgroundObjects();
            if (bobCoroutine != null) StopCoroutine(bobCoroutine);
        }

        private void Update()
        {
            if (!isOpen && !gameObject.activeSelf) return;

            // Đóng bằng phím Escape hoặc nút Back của Android
            if (IsEscapePressed())
            {
                Hide();
            }
        }

        private bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
            {
                return true;
            }
#endif
            return false;
        }

        public void Show(LanData lan)
        {
            if (lan == null) return;
            EnsureComponents();

            currentLan = lan;
            isOpen = true;
            gameObject.SetActive(true);

            // Ẩn các nút điều hướng và chữ phía sau trên MainMenu (Mục 6)
            HideBackgroundObjects();

            // Cập nhật nội dung theo Lân
            UpdateContent(lan);

            // Bật hoạt ảnh mở popup & idle bobbing
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            if (fallbackCoroutine != null) StopCoroutine(fallbackCoroutine);
            if (bobCoroutine != null) StopCoroutine(bobCoroutine);

            if (Application.isPlaying)
            {
                animCoroutine = StartCoroutine(OpenAnimationRoutine());
                bobCoroutine = StartCoroutine(MedallionBobRoutine());
                fallbackCoroutine = StartCoroutine(FallbackRoutine(0.6f, targetOpen: true));
            }
            else
            {
                SetFinalState(targetOpen: true);
            }
        }

        public void Hide()
        {
            if (!isOpen && !gameObject.activeSelf) return;
            isOpen = false;

            if (DemHoiDenLong.Audio.AudioManager.Instance != null)
            {
                DemHoiDenLong.Audio.AudioManager.Instance.PlayUIClick();
            }

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            if (fallbackCoroutine != null) StopCoroutine(fallbackCoroutine);
            if (bobCoroutine != null)
            {
                StopCoroutine(bobCoroutine);
                bobCoroutine = null;
                if (medallionRect != null) medallionRect.anchoredPosition = medallionBasePos;
            }

            if (Application.isPlaying)
            {
                animCoroutine = StartCoroutine(CloseAnimationRoutine());
                fallbackCoroutine = StartCoroutine(FallbackRoutine(0.6f, targetOpen: false));
            }
            else
            {
                SetFinalState(targetOpen: false);
            }
        }

        public void UpdateContent(LanData lan)
        {
            if (lan == null) return;
            EnsureComponents();

            // Màu nhấn theo hệ Lân
            Color accentColor = GetSystemAccentColor(lan);

            // 1. Vòng huy chương & Ảnh đại diện Lân
            if (medallionImage != null)
            {
                medallionImage.color = accentColor;
                medallionImage.raycastTarget = false;
            }

            if (lanAvatarImage != null)
            {
                lanAvatarImage.sprite = lan.sprite != null ? lan.sprite : lan.icon;
                lanAvatarImage.raycastTarget = false;
            }

            // 2. Nhãn TRUYỀN THUYẾT (cỡ 30, #6B4A2B)
            if (loreSubheadText != null)
            {
                loreSubheadText.text = "TRUYỀN THUYẾT";
                loreSubheadText.fontSize = 30f;
                loreSubheadText.fontStyle = FontStyles.Bold;
                ColorUtility.TryParseHtmlString("#6B4A2B", out Color subColor);
                loreSubheadText.color = subColor;
                loreSubheadText.raycastTarget = false;
            }

            // Dải Ribbon tên Lân nhuộm màu nhấn
            if (ribbonImage != null)
            {
                ribbonImage.color = accentColor;
                ribbonImage.raycastTarget = false;
            }

            if (lanTitleText != null)
            {
                lanTitleText.text = lan.lanName.ToUpper();
                lanTitleText.fontSize = 54f;
                lanTitleText.fontStyle = FontStyles.Bold;
                ColorUtility.TryParseHtmlString("#2B1B0E", out Color titleColor);
                lanTitleText.color = titleColor;
                lanTitleText.raycastTarget = false;
            }

            // Dòng biệt danh (cỡ 30, #6B4A2B)
            if (nicknameText != null)
            {
                nicknameText.text = lan.nickname;
                nicknameText.fontSize = 45f;
                nicknameText.fontStyle = FontStyles.Bold;
                ColorUtility.TryParseHtmlString("#6B4A2B", out Color nickColor);
                nicknameText.color = nickColor;
                nicknameText.raycastTarget = false;
            }

            // 3. Vạch ngăn nhuộm màu nhấn
            if (dividerImage != null)
            {
                dividerImage.color = accentColor;
                dividerImage.raycastTarget = false;
            }

            // 4. Nội dung truyện (cỡ 35, màu #3B2A1A, LineSpacing +20%, ParagraphSpacing 20, căn trái)
            if (storyBodyText != null)
            {
                storyBodyText.fontSize = 35f;
                storyBodyText.lineSpacing = 15f;
                storyBodyText.paragraphSpacing = -10f;
                storyBodyText.alignment = TextAlignmentOptions.TopLeft;
                storyBodyText.enableAutoSizing = false;
                storyBodyText.raycastTarget = false;

                ColorUtility.TryParseHtmlString("#3B2A1A", out Color baseTextColor);
                storyBodyText.color = baseTextColor;

                Color dropCapColor = Color.Lerp(accentColor, baseTextColor, 0.55f);
                string hexDropCap = ColorUtility.ToHtmlStringRGB(dropCapColor);

                string rawStory = lan.storyDescription ?? "";
                if (rawStory.Length > 0)
                {
                    char firstChar = rawStory[0];
                    string remaining = rawStory.Substring(1);
                    storyBodyText.text = $"<size=150%><color=#{hexDropCap}><b>{firstChar}</b></color></size>{remaining}";
                }
                else
                {
                    storyBodyText.text = "";
                }
            }

            // 5. Khối kỹ năng tách rời, nền đặc bo tròn (#152042 alpha 1.0) (Mục 3)
            if (skillBlockBg != null)
            {
                ColorUtility.TryParseHtmlString("#152042", out Color chipBg);
                chipBg.a = 1.0f;
                skillBlockBg.color = chipBg;
                skillBlockBg.raycastTarget = false;
            }
            if (skillChipBg != null && skillChipBg != skillBlockBg)
            {
                ColorUtility.TryParseHtmlString("#152042", out Color chipBg);
                chipBg.a = 1.0f;
                skillChipBg.color = chipBg;
                skillChipBg.raycastTarget = false;
            }

            // Mục 4: Nếu data Lân không có icon kỹ năng thì ẩn Image icon đi (tránh ô vuông trắng ở Hỏa Lân)
            bool hasSkillIcon = (lan != null && lan.skillIcon != null);
            if (skillBlockIcon != null)
            {
                skillBlockIcon.gameObject.SetActive(hasSkillIcon);
                if (hasSkillIcon)
                {
                    skillBlockIcon.sprite = lan.skillIcon;
                    skillBlockIcon.preserveAspect = true;
                    skillBlockIcon.raycastTarget = false;
                }
            }
            if (skillChipIcon != null && skillChipIcon != skillBlockIcon)
            {
                skillChipIcon.gameObject.SetActive(hasSkillIcon);
                if (hasSkillIcon)
                {
                    skillChipIcon.sprite = lan.skillIcon;
                    skillChipIcon.preserveAspect = true;
                    skillChipIcon.raycastTarget = false;
                }
            }

            if (skillNameText != null)
            {
                skillNameText.text = $"KỸ NĂNG · {lan.skillName.ToUpper()}";
                skillNameText.fontSize = 26f;
                skillNameText.fontStyle = FontStyles.Bold;
                skillNameText.color = accentColor;
                skillNameText.raycastTarget = false;
            }
            if (skillChipText != null && skillChipText != skillNameText)
            {
                skillChipText.text = $"KỸ NĂNG · {lan.skillName.ToUpper()}";
                skillChipText.fontSize = 26f;
                skillChipText.fontStyle = FontStyles.Bold;
                skillChipText.color = accentColor;
                skillChipText.raycastTarget = false;
            }

            if (skillDescText != null)
            {
                string desc = !string.IsNullOrEmpty(lan.skillDescription) ? lan.skillDescription : "Kích hoạt chiêu thức đặc trưng giải cứu Phố Cổ.";
                skillDescText.text = desc;
                skillDescText.fontSize = 22f;
                ColorUtility.TryParseHtmlString("#E8D9B0", out Color descColor);
                skillDescText.color = descColor;
                skillDescText.raycastTarget = false;
            }

            // 6. Reset vị trí cuộn về đầu trang
            if (scrollRect != null)
            {
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public static Color GetSystemAccentColor(LanData lan)
        {
            if (lan == null) return new Color(0.96f, 0.84f, 0.43f, 1f);

            string id = lan.lanId?.Trim() ?? "";
            string name = lan.lanName?.ToLower() ?? "";

            if (id == "3" || name.Contains("thủy") || name.Contains("thuy") || name.Contains("water"))
            {
                ColorUtility.TryParseHtmlString("#26C6DA", out Color cyan);
                return cyan;
            }
            if (id == "2" || name.Contains("hỏa") || name.Contains("hoa") || name.Contains("fire"))
            {
                ColorUtility.TryParseHtmlString("#FF5722", out Color orange);
                return orange;
            }
            if (id == "1" || name.Contains("kim") || name.Contains("metal"))
            {
                ColorUtility.TryParseHtmlString("#CFD8DC", out Color silver);
                return silver;
            }
            if (id == "4" || name.Contains("vàng") || name.Contains("vang") || name.Contains("gold"))
            {
                ColorUtility.TryParseHtmlString("#FFD54F", out Color gold);
                return gold;
            }

            return lan.titleColor.a > 0.1f ? lan.titleColor : new Color(0.96f, 0.84f, 0.43f, 1f);
        }

        // ==========================================
        // HOẠT ẢNH MỞ & ĐÓNG (ANIMATIONS)
        // ==========================================
        private IEnumerator OpenAnimationRoutine()
        {
            if (panelCanvasGroup == null) panelCanvasGroup = GetComponent<CanvasGroup>();
            if (cardCanvasGroup == null && cardRect != null) cardCanvasGroup = cardRect.GetComponent<CanvasGroup>();

            panelCanvasGroup.alpha = 0f;
            if (cardCanvasGroup != null) cardCanvasGroup.alpha = 0f;
            if (cardRect != null) cardRect.localScale = Vector3.one * 0.85f;

            float elapsed = 0f;
            float duration = 0.25f;

            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

                float easeOutBack = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                if (cardCanvasGroup != null) cardCanvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
                if (cardRect != null) cardRect.localScale = Vector3.LerpUnclamped(Vector3.one * 0.85f, Vector3.one, easeOutBack);

                yield return null;
            }

            SetFinalState(targetOpen: true);
            animCoroutine = null;
        }

        private IEnumerator CloseAnimationRoutine()
        {
            if (panelCanvasGroup == null) panelCanvasGroup = GetComponent<CanvasGroup>();
            if (cardCanvasGroup == null && cardRect != null) cardCanvasGroup = cardRect.GetComponent<CanvasGroup>();

            float elapsed = 0f;
            float duration = 0.15f;

            float startAlpha = panelCanvasGroup.alpha;
            Vector3 startScale = cardRect != null ? cardRect.localScale : Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                if (cardCanvasGroup != null) cardCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                if (cardRect != null) cardRect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.90f, t);

                yield return null;
            }

            SetFinalState(targetOpen: false);
            animCoroutine = null;
        }

        private IEnumerator MedallionBobRoutine()
        {
            if (medallionRect == null) yield break;
            Vector2 basePos = medallionBasePos;

            while (isOpen)
            {
                float offset = Mathf.Sin(Time.unscaledTime * Mathf.PI) * 6f;
                medallionRect.anchoredPosition = new Vector2(basePos.x, basePos.y + offset);
                yield return null;
            }

            medallionRect.anchoredPosition = basePos;
        }

        private IEnumerator FallbackRoutine(float timeout, bool targetOpen)
        {
            yield return new WaitForSecondsRealtime(timeout);
            SetFinalState(targetOpen);
        }

        private void SetFinalState(bool targetOpen)
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = targetOpen ? 1f : 0f;
                panelCanvasGroup.interactable = targetOpen;
                panelCanvasGroup.blocksRaycasts = targetOpen;
            }

            if (cardCanvasGroup != null)
            {
                cardCanvasGroup.alpha = targetOpen ? 1f : 0f;
            }

            if (cardRect != null)
            {
                cardRect.localScale = targetOpen ? Vector3.one : Vector3.one * 0.90f;
            }

            if (!targetOpen)
            {
                RestoreBackgroundObjects();
                gameObject.SetActive(false);
            }
        }

        // ==========================================
        // ẨN / HIỆN CÁC ĐỐI TƯỢNG MAINMENU PHÍA SAU (Mục 6)
        // ==========================================
        private void HideBackgroundObjects()
        {
            // Tránh quét lại nếu các đối tượng đã được ẩn và ghi nhận (ngăn việc bị xóa danh sách khi Show() gọi 2 lần)
            if (hiddenBackgroundObjects.Count > 0) return;

            Transform root = transform.parent;
            if (root == null) return;

            string[] targetNames = new string[]
            {
                "PlayBtn", "PlayButton", "UnlockBtn", "UnlockButton", "LoreBtn", "StoryButton", "ReadStoryButton",
                "PrevBtn", "PrevButton", "NextBtn", "NextButton", "LeftArrow", "RightArrow", "SettingsButton",
                "TitleText", "Title_Mat", "LanTitleText", "LanDescText", "SubtitlePill",
                "SkillPanel", "SkillNameText", "SkillDescText", "StatsPanel"
            };

            var allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t == null || t == transform || t.IsChildOf(transform)) continue;
                for (int i = 0; i < targetNames.Length; i++)
                {
                    if (string.Equals(t.gameObject.name, targetNames[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (t.gameObject.activeSelf && !hiddenBackgroundObjects.Contains(t.gameObject))
                        {
                            hiddenBackgroundObjects.Add(t.gameObject);
                            t.gameObject.SetActive(false);
                        }
                        break;
                    }
                }
            }
        }

        private void RestoreBackgroundObjects()
        {
            if (hiddenBackgroundObjects.Count > 0)
            {
                foreach (var obj in hiddenBackgroundObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
                hiddenBackgroundObjects.Clear();
            }

            // Cơ chế bảo vệ fail-safe: Khôi phục chắc chắn các thành phần chính của MainMenu khi đóng popup
            Transform root = transform.parent;
            if (root != null)
            {
                string[] coreControls = new string[]
                {
                    "TitleText", "Title_Mat", "SubtitlePill", "PrevBtn", "NextBtn",
                    "StatsPanel", "SkillPanel", "LoreBtn", "PlayBtn", "SettingsButton"
                };

                var allTransforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t == null || t == transform || t.IsChildOf(transform)) continue;
                    for (int i = 0; i < coreControls.Length; i++)
                    {
                        if (string.Equals(t.gameObject.name, coreControls[i], System.StringComparison.OrdinalIgnoreCase))
                        {
                            t.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
            }

            // Gọi HangarUI đồng bộ lại trạng thái nút Play / Unlock theo Lân đang chọn
            HangarUI hangar = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
            if (hangar != null && currentLan != null)
            {
                hangar.SelectLan(currentLan);
            }
        }
    }
}
