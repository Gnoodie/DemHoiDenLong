using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private GameManager gameManager;

        [Header("Player Stats UI")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image hpFillImage;
        public GameObject VictoryPanelObj => victoryPanel;
        public UnityEngine.UI.Slider HpSlider => hpSlider;
        public TMPro.TextMeshProUGUI HpText => hpText;
        public TMPro.TextMeshProUGUI ObjectiveText => objectiveText;
        public void UpdateHp(float currentHp, float maxHp) => UpdatePlayerStats(3, currentHp, maxHp);
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private RectTransform livesContainer;

        [Header("Skill UI")]
        [SerializeField] private Button skillButton;
        [SerializeField] private Image skillBgImage;
        [SerializeField] private Image skillBorderRing;
        [SerializeField] private Image skillIcon;
        [SerializeField] private Image skillCooldownOverlay;
        [SerializeField] private TextMeshProUGUI skillCooldownText;

        [Header("Skill Banner & Screen Flash")]
        [SerializeField] private CanvasGroup skillBannerGroup;
        [SerializeField] private TextMeshProUGUI skillBannerText;
        [SerializeField] private Image screenBorderFlashImage;

        public Button SkillButton => skillButton;
        public Image SkillBorderRing => skillBorderRing;
        public TextMeshProUGUI SkillCooldownText => skillCooldownText;
        public CanvasGroup SkillBannerGroup => skillBannerGroup;
        public TextMeshProUGUI SkillBannerText => skillBannerText;
        public Image ScreenBorderFlashImage => screenBorderFlashImage;

        [Header("In-Game Objectives & Currency (Phase 5A)")]
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI runStarsText;
        [SerializeField] private Sprite starSprite;

        [Header("State Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject reviveButtonObj;

        [Header("Victory Panel Bindings (Phase 5A)")]
        [SerializeField] private TextMeshProUGUI victoryTitleText;
        [SerializeField] private Image[] victoryStarImages;
        [SerializeField] private GameObject victoryRecordBadge;
        [SerializeField] private TextMeshProUGUI victoryRunStarsText;
        [SerializeField] private TextMeshProUGUI victoryMilestoneRewardText;
        [SerializeField] private GameObject victoryNextActBanner;
        [SerializeField] private GameObject victoryReviveCappedNotice;
        [SerializeField] private GameObject victoryNextLevelButton;
        [SerializeField] private Button victoryReplayButton;
        [SerializeField] private Button victorySelectLevelButton;

        [Header("GameOver Panel Bindings (Phase 5A)")]
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI gameOverRunStarsText;
        [SerializeField] private Button gameOverReplayButton;
        [SerializeField] private Button gameOverGiveUpButton;

        private static readonly Color ColorGold = new Color(1.0f, 0.824f, 0.478f, 1f);   // #FFD27A
        private static readonly Color ColorDimStar = new Color(0.29f, 0.33f, 0.50f, 1f); // #4A5580
        private float backupPollTimer = 0f;
        private static int hudTraceCount = 0;
        private Coroutine hpFlashRoutine;
        private Color currentHpColor = new Color32(0x3D, 0xDC, 0x97, 0xFF);
        private float lastKnownHp = -1f;

        public void FlashHpFillWhite()
        {
            if (hpFlashRoutine != null) StopCoroutine(hpFlashRoutine);
            hpFlashRoutine = StartCoroutine(HpFlashWhiteRoutine());
        }

        private System.Collections.IEnumerator HpFlashWhiteRoutine()
        {
            if (hpFillImage != null) hpFillImage.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
            if (hpFillImage != null) hpFillImage.color = currentHpColor;
            hpFlashRoutine = null;
        }

        private Color currentLanPrimaryColor = new Color32(0xE8, 0xD0, 0x8D, 0xFF);
        private Coroutine skillButtonBounceRoutine;
        private Coroutine skillButtonShakeRoutine;
        private Coroutine skillReadyFlashRoutine;
        private Coroutine skillBannerRoutine;
        private Coroutine screenBorderFlashRoutine;
        private bool lastSkillWasReady = true;
        private GameObject skillSparksContainer;
        private Image[] skillSparksImages;

        public void FlashScreenBorder(Color color, float duration)
        {
            if (screenBorderFlashRoutine != null) StopCoroutine(screenBorderFlashRoutine);
            screenBorderFlashRoutine = StartCoroutine(ScreenBorderFlashRoutine(color, duration));
        }

        private System.Collections.IEnumerator ScreenBorderFlashRoutine(Color color, float duration)
        {
            if (screenBorderFlashImage == null) yield break;

            screenBorderFlashImage.gameObject.SetActive(true);
            float el = 0f;
            while (el < duration)
            {
                el += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(el / duration);
                Color c = color;
                c.a = Mathf.Lerp(color.a, 0f, t);
                screenBorderFlashImage.color = c;
                yield return null;
            }

            screenBorderFlashImage.gameObject.SetActive(false);
            screenBorderFlashRoutine = null;
        }

        public void ShowSkillBanner(string name, Color color)
        {
            if (skillBannerRoutine != null) StopCoroutine(skillBannerRoutine);
            skillBannerRoutine = StartCoroutine(SkillBannerRoutine(name, color));
        }

        private System.Collections.IEnumerator SkillBannerRoutine(string name, Color color)
        {
            EnsureSkillBannerUI();
            Color primaryColor = (color != default && color.a > 0.1f) ? color : currentLanPrimaryColor;

            if (skillBannerText != null)
            {
                skillBannerText.text = name;
                skillBannerText.color = new Color(244f / 255f, 241f / 255f, 230f / 255f, 1f); // #F4F1E6
                skillBannerText.outlineColor = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.85f);
                skillBannerText.outlineWidth = 0.22f;
            }

            if (skillBannerGroup != null)
            {
                Outline outline = skillBannerGroup.GetComponent<Outline>() ?? skillBannerGroup.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.6f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                skillBannerGroup.gameObject.SetActive(true);
                skillBannerGroup.alpha = 0f;

                // 1. Fade in 0.15s
                float el = 0f;
                float inDur = 0.15f;
                while (el < inDur)
                {
                    el += Time.unscaledDeltaTime;
                    skillBannerGroup.alpha = Mathf.Lerp(0f, 1f, el / inDur);
                    yield return null;
                }
                skillBannerGroup.alpha = 1f;

                // 2. Hiện rõ 0.75s (tổng 0.9s hiển thị rõ)
                yield return new WaitForSecondsRealtime(0.75f);

                // 3. Fade out 0.25s (tổng 1.15s <= 1.2s)
                el = 0f;
                float outDur = 0.25f;
                while (el < outDur)
                {
                    el += Time.unscaledDeltaTime;
                    skillBannerGroup.alpha = Mathf.Lerp(1f, 0f, el / outDur);
                    yield return null;
                }

                skillBannerGroup.alpha = 0f;
                skillBannerGroup.gameObject.SetActive(false);
            }
            skillBannerRoutine = null;
        }

        private void EnsureSkillBannerUI()
        {
            if (skillBannerGroup == null)
            {
                Transform safe = transform.Find("SafeArea") ?? transform;
                Transform banT = safe.Find("SkillBanner") ?? transform.Find("SkillBanner");
                if (banT == null)
                {
                    GameObject bObj = new GameObject("SkillBanner", typeof(RectTransform), typeof(CanvasGroup));
                    bObj.transform.SetParent(safe, false);
                    banT = bObj.transform;
                }
                skillBannerGroup = banT.GetComponent<CanvasGroup>() ?? banT.gameObject.AddComponent<CanvasGroup>();
            }

            RectTransform rt = skillBannerGroup.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                // Vị trí: 16% H tính từ đỉnh (~ -307px trên Canvas chuẩn 1920)
                rt.anchoredPosition = new Vector2(0f, -307f);
            }

            Image bgImg = skillBannerGroup.GetComponent<Image>();
            if (bgImg == null) bgImg = skillBannerGroup.gameObject.AddComponent<Image>();
            Sprite pillSpr = Resources.Load<Sprite>("UI/ui_pill") ?? Resources.Load<Sprite>("UI/ui_round_36");
            if (pillSpr != null)
            {
                bgImg.sprite = pillSpr;
                bgImg.type = Image.Type.Sliced;
            }
            bgImg.color = new Color(16f / 255f, 26f / 255f, 60f / 255f, 0.75f); // #101A3C alpha 0.75
            bgImg.raycastTarget = false;

            // Content Size Fitter & Layout co giãn theo chữ
            var hlg = skillBannerGroup.GetComponent<HorizontalLayoutGroup>() ?? skillBannerGroup.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(36, 36, 8, 8);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var csf = skillBannerGroup.GetComponent<ContentSizeFitter>() ?? skillBannerGroup.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (skillBannerText == null)
            {
                skillBannerText = skillBannerGroup.GetComponentInChildren<TextMeshProUGUI>(true);
                if (skillBannerText == null)
                {
                    GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    tObj.transform.SetParent(skillBannerGroup.transform, false);
                    skillBannerText = tObj.GetComponent<TextMeshProUGUI>();
                }
            }

            skillBannerText.textWrappingMode = TextWrappingModes.NoWrap;
            skillBannerText.enableAutoSizing = true;
            skillBannerText.fontSizeMin = 28f;
            skillBannerText.fontSizeMax = 40f;
            skillBannerText.fontSize = 36f;
            skillBannerText.fontStyle = FontStyles.Bold;
            skillBannerText.alignment = TextAlignmentOptions.Center;
            skillBannerText.raycastTarget = false;
        }

        public void OnSkillReadyFlash()
        {
            if (skillReadyFlashRoutine != null) StopCoroutine(skillReadyFlashRoutine);
            skillReadyFlashRoutine = StartCoroutine(SkillReadyFlashRoutine());
        }

        private void EnsureSkillSparksUI()
        {
            if (skillButton == null) return;
            Transform existing = skillButton.transform.Find("SkillSparks");
            if (existing != null)
            {
                skillSparksContainer = existing.gameObject;
                skillSparksImages = skillSparksContainer.GetComponentsInChildren<Image>(true);
                return;
            }

            skillSparksContainer = new GameObject("SkillSparks", typeof(RectTransform));
            skillSparksContainer.transform.SetParent(skillButton.transform, false);
            RectTransform cRt = skillSparksContainer.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = Vector2.zero;

            Sprite sparkSpr = Resources.Load<Sprite>("VFX/Generated/vfx_spark_proc");
            if (sparkSpr == null) sparkSpr = Resources.Load<Sprite>("VFX/vfx_spark4");

            skillSparksImages = new Image[4];
            Vector2[] offsets = new Vector2[] { new Vector2(0, 68), new Vector2(68, 0), new Vector2(0, -68), new Vector2(-68, 0) };

            for (int i = 0; i < 4; i++)
            {
                GameObject spObj = new GameObject($"Spark_{i}", typeof(RectTransform), typeof(Image));
                spObj.transform.SetParent(skillSparksContainer.transform, false);
                RectTransform spRt = spObj.GetComponent<RectTransform>();
                spRt.anchorMin = new Vector2(0.5f, 0.5f);
                spRt.anchorMax = new Vector2(0.5f, 0.5f);
                spRt.pivot = new Vector2(0.5f, 0.5f);
                spRt.anchoredPosition = offsets[i];
                spRt.sizeDelta = new Vector2(24f, 24f);

                Image img = spObj.GetComponent<Image>();
                img.sprite = sparkSpr;
                img.color = Color.white;
                img.raycastTarget = false;
                skillSparksImages[i] = img;
            }

            skillSparksContainer.SetActive(false);
        }

        private void EnsureSkillCooldownUI()
        {
            if (skillButton == null) return;

            if (skillCooldownText == null)
            {
                Transform cdT = skillButton.transform.Find("CooldownText");
                if (cdT != null)
                {
                    skillCooldownText = cdT.GetComponent<TextMeshProUGUI>();
                }
            }

            if (skillCooldownText == null)
            {
                GameObject textObj = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(skillButton.transform, false);
                skillCooldownText = textObj.GetComponent<TextMeshProUGUI>();
            }

            if (skillCooldownText != null)
            {
                RectTransform textRt = skillCooldownText.rectTransform;
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;
                textRt.anchoredPosition = Vector2.zero;

                if (skillCooldownText.font == null && skillBannerText != null && skillBannerText.font != null)
                {
                    skillCooldownText.font = skillBannerText.font;
                }

                skillCooldownText.fontSize = 44f;
                skillCooldownText.fontStyle = FontStyles.Bold;
                skillCooldownText.alignment = TextAlignmentOptions.Center;
                skillCooldownText.color = Color.white;
                if (skillCooldownText.font == null) skillCooldownText.font = TMPro.TMP_Settings.defaultFontAsset;
                skillCooldownText.raycastTarget = false;

                // Luôn là child cuối cùng để render trên Overlay và Ring
                if (skillCooldownText.transform.GetSiblingIndex() != skillButton.transform.childCount - 1)
                {
                    skillCooldownText.transform.SetAsLastSibling();
                }
            }
        }

        private System.Collections.IEnumerator SkillReadyFlashRoutine()
        {
            if (skillBorderRing == null) yield break;

            Color origColor = currentLanPrimaryColor;
            skillBorderRing.color = Color.white;
            Vector3 origScale = Vector3.one;

            EnsureSkillSparksUI();
            if (skillSparksContainer != null)
            {
                skillSparksContainer.SetActive(true);
            }

            float elapsed = 0f;
            float dur = 0.5f;
            Vector2[] baseDirs = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);

                // Ring bounce (1.15x -> 1.0x trong 0.25s)
                if (elapsed <= 0.25f)
                {
                    float bounceT = elapsed / 0.25f;
                    float s = 1.0f + 0.15f * Mathf.Sin(bounceT * Mathf.PI);
                    skillBorderRing.transform.localScale = new Vector3(s, s, 1f);
                    skillBorderRing.color = Color.Lerp(Color.white, origColor, bounceT);
                }
                else
                {
                    skillBorderRing.transform.localScale = origScale;
                    skillBorderRing.color = origColor;
                }

                // Sparks phóng ra (68px -> 92px) và mờ dần trong 0.5s
                if (skillSparksImages != null)
                {
                    for (int i = 0; i < skillSparksImages.Length; i++)
                    {
                        if (skillSparksImages[i] != null)
                        {
                            float dist = Mathf.Lerp(68f, 92f, t);
                            skillSparksImages[i].rectTransform.anchoredPosition = baseDirs[i] * dist;
                            Color sc = origColor;
                            sc.a = Mathf.Lerp(1.0f, 0f, t);
                            skillSparksImages[i].color = sc;
                        }
                    }
                }

                yield return null;
            }

            if (skillSparksContainer != null)
            {
                skillSparksContainer.SetActive(false);
            }

            if (skillBorderRing != null)
            {
                skillBorderRing.transform.localScale = origScale;
                skillBorderRing.color = origColor;
            }

            skillReadyFlashRoutine = null;
        }

        public void OnSkillButtonClicked()
        {
            PlayerSkillController pSkill = player != null ? player.GetComponent<PlayerSkillController>() : null;
            bool isReady = pSkill != null ? pSkill.IsSkillReady() : (skillCooldownOverlay == null || skillCooldownOverlay.fillAmount <= 0f);

            if (!isReady)
            {
                // Chưa hồi xong: rung nhẹ 4px trong 0.15s, phát âm thanh click
                Audio.AudioManager.Instance?.PlayUIClick();
                if (skillButtonShakeRoutine != null) StopCoroutine(skillButtonShakeRoutine);
                skillButtonShakeRoutine = StartCoroutine(SkillButtonShakeRoutine());
                return;
            }

            Audio.AudioManager.Instance?.PlayUIClick();

            if (skillButtonBounceRoutine != null) StopCoroutine(skillButtonBounceRoutine);
            skillButtonBounceRoutine = StartCoroutine(SkillButtonBounceRoutine());

            if (player != null)
            {
                player.TriggerSkillFromButton();
            }
            else if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                GameManager.Instance.Player.TriggerSkillFromButton();
            }
        }

        private System.Collections.IEnumerator SkillButtonShakeRoutine()
        {
            Transform t = skillButton != null ? skillButton.transform : (skillIcon != null ? skillIcon.transform : null);
            if (t == null) yield break;

            Vector3 origPos = t.localPosition;
            float elapsed = 0f;
            float dur = 0.15f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float x = Mathf.Sin(elapsed * 60f) * 4f;
                t.localPosition = origPos + new Vector3(x, 0f, 0f);
                yield return null;
            }
            t.localPosition = origPos;
            skillButtonShakeRoutine = null;
        }

        private System.Collections.IEnumerator SkillButtonBounceRoutine()
        {
            Transform t = skillButton != null ? skillButton.transform : (skillIcon != null ? skillIcon.transform : null);
            if (t == null) yield break;

            Vector3 origScale = Vector3.one;
            float dur = 0.08f;
            float el = 0f;
            while (el < dur)
            {
                el += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(1.0f, 0.92f, el / dur);
                t.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            el = 0f;
            while (el < dur)
            {
                el += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(0.92f, 1.0f, el / dur);
                t.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            t.localScale = origScale;
            skillButtonBounceRoutine = null;
        }

        public static GameplayHUD Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            AutoBindComponents();
        }

        public void AutoBindComponents()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();

            Transform pBar = null;
            if (canvas != null)
            {
                Transform safe = canvas.transform.Find("SafeAreaRoot");
                if (safe != null) pBar = safe.Find("PlayerHpBar");
                if (pBar == null) pBar = canvas.transform.Find("PlayerHpBar");

                if (pBar == null)
                {
                    Slider[] sliders = canvas.GetComponentsInChildren<Slider>(true);
                    foreach (var s in sliders)
                    {
                        if (s.name == "PlayerHpBar")
                        {
                            pBar = s.transform;
                            break;
                        }
                    }
                    if (pBar == null)
                    {
                        foreach (var s in sliders)
                        {
                            if (!s.name.Contains("Boss"))
                            {
                                pBar = s.transform;
                                break;
                            }
                        }
                    }
                }
            }

            if (pBar != null)
            {
                hpSlider = pBar.GetComponent<Slider>();

                RectTransform rtP = pBar.GetComponent<RectTransform>();
                if (rtP != null)
                {
                    rtP.anchorMin = new Vector2(0f, 1f);
                    rtP.anchorMax = new Vector2(0f, 1f);
                    rtP.pivot = new Vector2(0f, 1f);
                    rtP.anchoredPosition = new Vector2(24f, -24f);
                    rtP.sizeDelta = new Vector2(496f, 40f);
                }

                Transform pFillArea = pBar.Find("Fill Area");
                Transform pFill = pFillArea != null ? pFillArea.Find("Fill") : null;
                if (pFillArea != null && pFill != null)
                {
                    RectTransform rtPFA = pFillArea.GetComponent<RectTransform>();
                    rtPFA.anchorMin = Vector2.zero;
                    rtPFA.anchorMax = Vector2.one;
                    rtPFA.offsetMin = new Vector2(2f, 2f);
                    rtPFA.offsetMax = new Vector2(-2f, -2f);

                    RectTransform rtPF = pFill.GetComponent<RectTransform>();
                    rtPF.anchorMin = Vector2.zero;
                    rtPF.anchorMax = Vector2.one;
                    rtPF.offsetMin = Vector2.zero;
                    rtPF.offsetMax = Vector2.zero;

                    hpFillImage = pFill.GetComponent<Image>();
                    if (hpFillImage != null && hpFlashRoutine == null)
                    {
                        hpFillImage.color = currentHpColor;
                    }
                    if (hpSlider != null)
                    {
                        hpSlider.fillRect = rtPF;
                        hpSlider.targetGraphic = hpFillImage;
                    }
                }

                Transform pText = pBar.Find("HpText");
                if (pText != null)
                {
                    hpText = pText.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    hpText = pBar.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }

            if (canvas != null)
            {
                Transform safe = canvas.transform.Find("SafeAreaRoot");
                if (safe == null) safe = canvas.transform;

                if (skillButton == null)
                {
                    Transform sBtnT = safe.Find("SkillButton");
                    if (sBtnT != null) skillButton = sBtnT.GetComponent<Button>();
                }
                if (skillButton != null)
                {
                    if (skillBgImage == null)
                    {
                        Transform bgT = skillButton.transform.Find("SkillBg");
                        if (bgT != null) skillBgImage = bgT.GetComponent<Image>();
                    }
                    if (skillBorderRing == null)
                    {
                        Transform rT = skillButton.transform.Find("BorderRing");
                        if (rT != null) skillBorderRing = rT.GetComponent<Image>();
                    }
                    if (skillIcon == null)
                    {
                        Transform iconT = skillButton.transform.Find("SkillIcon");
                        if (iconT != null) skillIcon = iconT.GetComponent<Image>();
                    }
                    if (skillCooldownOverlay == null)
                    {
                        Transform ovT = skillButton.transform.Find("CooldownOverlay");
                        if (ovT != null) skillCooldownOverlay = ovT.GetComponent<Image>();
                    }
                    if (skillCooldownText == null)
                    {
                        Transform cdTextT = skillButton.transform.Find("CooldownText");
                        if (cdTextT != null) skillCooldownText = cdTextT.GetComponent<TextMeshProUGUI>();
                    }
                }
                else if (skillIcon == null)
                {
                    Transform iconT = safe.Find("SkillIcon");
                    if (iconT != null)
                    {
                        skillIcon = iconT.GetComponent<Image>();
                        Transform ovT = iconT.Find("CooldownOverlay");
                        if (ovT != null) skillCooldownOverlay = ovT.GetComponent<Image>();
                    }
                }

                if (skillBannerGroup == null)
                {
                    Transform banT = safe.Find("SkillBanner");
                    if (banT == null) banT = canvas.transform.Find("SkillBanner");
                    if (banT != null)
                    {
                        skillBannerGroup = banT.GetComponent<CanvasGroup>();
                        skillBannerText = banT.GetComponentInChildren<TextMeshProUGUI>(true);
                    }
                }

                if (screenBorderFlashImage == null)
                {
                    Transform flashT = canvas.transform.Find("ScreenBorderFlash");
                    if (flashT != null) screenBorderFlashImage = flashT.GetComponent<Image>();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
            hudTraceCount = 0;
        }

        private void OnEnable()
        {
            PlayerController.OnRunStarsChanged += UpdateRunStarsUI;
            LevelSession.OnKillsChanged += UpdateKillsUI;
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnKillsChanged += UpdateKillsUI;
                WaveManager.Instance.OnSurviveTimerChanged += UpdateSurviveUI;
            }
        }

        private void OnDisable()
        {
            PlayerController.OnRunStarsChanged -= UpdateRunStarsUI;
            LevelSession.OnKillsChanged -= UpdateKillsUI;
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnKillsChanged -= UpdateKillsUI;
                WaveManager.Instance.OnSurviveTimerChanged -= UpdateSurviveUI;
            }
        }

        private void Start()
        {
            AutoBindComponents();

            if (player == null)
            {
                player = Object.FindAnyObjectByType<PlayerController>();
            }

            if (player != null)
            {
                player.OnPlayerHealthChanged += UpdatePlayerStats;
                player.OnSkillCooldownChanged += UpdateSkillCooldown;
                UpdatePlayerStats(player.LivesCount, player.CurrentHp, player.MaxHp);
                LoadDynamicSkillIcon();
            }
            else
            {
                UpdatePlayerStats(3, 100f, 100f);
            }

            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
                HandleGameStateChanged(gameManager.CurrentState);
            }
            else
            {
                HideAllPanels();
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnKillsChanged -= UpdateKillsUI;
                WaveManager.Instance.OnKillsChanged += UpdateKillsUI;
                WaveManager.Instance.OnSurviveTimerChanged -= UpdateSurviveUI;
                WaveManager.Instance.OnSurviveTimerChanged += UpdateSurviveUI;
            }

            LevelSession.OnKillsChanged -= UpdateKillsUI;
            LevelSession.OnKillsChanged += UpdateKillsUI;

            UpdateRunStarsUI(LevelSession.runStars);
            InitObjectiveUI();
            BindPanelButtons();
            EnsureSkillCooldownUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (player != null)
            {
                player.OnPlayerHealthChanged -= UpdatePlayerStats;
                player.OnSkillCooldownChanged -= UpdateSkillCooldown;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void Update()
        {
            // Auto re-link if player is swapped during runtime
            if (player == null)
            {
                player = Object.FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    player.OnPlayerHealthChanged += UpdatePlayerStats;
                    player.OnSkillCooldownChanged += UpdateSkillCooldown;
                    UpdatePlayerStats(player.LivesCount, player.CurrentHp, player.MaxHp);
                    LoadDynamicSkillIcon();
                }
            }

            // Ẩn nút skill và dừng đếm hồi chiêu khi không ở trạng thái Playing (Won / Lost / AwaitingRevive)
            bool isGameplayActive = gameManager != null && gameManager.CurrentRunState == RunState.Playing && gameManager.CurrentState == GameState.Playing;
            if (!isGameplayActive)
            {
                if (skillButton != null && skillButton.gameObject.activeSelf) skillButton.gameObject.SetActive(false);
                if (skillBannerGroup != null && skillBannerGroup.gameObject.activeSelf)
                {
                    skillBannerGroup.alpha = 0f;
                    skillBannerGroup.gameObject.SetActive(false);
                }
                return;
            }
            else
            {
                if (skillButton != null && !skillButton.gameObject.activeSelf) skillButton.gameObject.SetActive(true);
            }

            // Skill Ready & Cooldown feedback
            PlayerSkillController pSkill = player != null ? player.GetComponent<PlayerSkillController>() : null;
            float remainSec = pSkill != null ? pSkill.CooldownTimer : 0f;
            bool isReady = pSkill != null ? pSkill.IsSkillReady() : (remainSec <= 0.05f);
            bool isCasting = pSkill != null && pSkill.IsSkillActive;

            // Kích hoạt flash khi chuyển từ cooldown sang ready
            if (!lastSkillWasReady && isReady)
            {
                OnSkillReadyFlash();
            }
            lastSkillWasReady = isReady;

            EnsureSkillCooldownUI();

            if (isReady && !isCasting)
            {
                if (skillIcon != null) skillIcon.color = Color.white;
                if (skillCooldownOverlay != null && skillCooldownOverlay.gameObject.activeSelf) skillCooldownOverlay.gameObject.SetActive(false);
                if (skillCooldownText != null && skillCooldownText.gameObject.activeSelf) skillCooldownText.gameObject.SetActive(false);

                // Viền pulse nhẹ (scale 1.0 -> 1.08 lặp chu kỳ 1.2s)
                if (skillBorderRing != null && skillReadyFlashRoutine == null)
                {
                    float t = (Time.time % 1.2f) / 1.2f;
                    float pulse = 1.0f + 0.08f * Mathf.Sin(t * Mathf.PI * 2f);
                    skillBorderRing.transform.localScale = new Vector3(pulse, pulse, 1f);
                    skillBorderRing.color = currentLanPrimaryColor;
                }
            }
            else
            {
                // Khi đang trong thời gian hồi chiêu HOẶC đang thi triển:
                if (skillCooldownOverlay != null)
                {
                    if (!skillCooldownOverlay.gameObject.activeSelf) skillCooldownOverlay.gameObject.SetActive(true);
                    if (pSkill != null) skillCooldownOverlay.fillAmount = pSkill.GetCooldownFillAmount();
                }

                // Luôn cập nhật số giây đếm ngược to rõ khi cooldown > 0
                if (skillCooldownText != null)
                {
                    if (remainSec > 0.05f)
                    {
                        if (!skillCooldownText.gameObject.activeSelf) skillCooldownText.gameObject.SetActive(true);
                        skillCooldownText.text = Mathf.CeilToInt(remainSec).ToString();
                        skillCooldownText.transform.SetAsLastSibling();
                    }
                    else
                    {
                        if (skillCooldownText.gameObject.activeSelf) skillCooldownText.gameObject.SetActive(false);
                    }
                }

                if (isCasting)
                {
                    if (skillIcon != null) skillIcon.color = Color.white;
                    if (skillBorderRing != null && skillReadyFlashRoutine == null)
                    {
                        skillBorderRing.transform.Rotate(0f, 0f, 180f * Time.unscaledDeltaTime);
                        float pulse = 1.0f + 0.12f * Mathf.Sin(Time.unscaledTime * 12f);
                        skillBorderRing.transform.localScale = new Vector3(pulse, pulse, 1f);
                        skillBorderRing.color = Color.Lerp(currentLanPrimaryColor, Color.white, 0.45f);
                    }
                }
                else
                {
                    if (skillIcon != null) skillIcon.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    if (skillBorderRing != null && skillReadyFlashRoutine == null)
                    {
                        skillBorderRing.transform.localScale = Vector3.one;
                        skillBorderRing.color = currentLanPrimaryColor;
                    }
                }
            }

            // Low HP pulsate under 30%
            if (hpFillImage != null && hpFlashRoutine == null && hpSlider != null && hpSlider.maxValue > 0f)
            {
                float r = hpSlider.value / hpSlider.maxValue;
                if (r < 0.30f && r > 0f)
                {
                    float wave = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
                    Color dim = new Color32(0xC0, 0x30, 0x35, 0xFF);
                    hpFillImage.color = Color.Lerp(dim, new Color32(0xFF, 0x5A, 0x5F, 0xFF), wave);
                }
            }

            backupPollTimer += Time.deltaTime;
            if (backupPollTimer >= 0.25f)
            {
                backupPollTimer = 0f;
                PollBackupObjective();
            }
        }

        private void PollBackupObjective()
        {
            if (objectiveText == null) return;
            LevelData data = LevelSession.level;
            if (data == null && WaveManager.Instance != null) data = WaveManager.Instance.CurrentLevelData;
            if (data == null) return;
            if (data.winCondition == WinCondition.BossKill || data.isBoss) return;

            if (data.winCondition == WinCondition.KillCount)
            {
                int target = data.targetKillCount;
                int current = LevelSession.Kills;
                string expected = $"Hạ {current}/{target}";
                if (objectiveText.text != expected)
                {
                    UpdateKillsUI(current, target);
                }
            }
        }

        public void InitObjectiveUI()
        {
            if (objectiveText == null) return;
            LevelData data = LevelSession.level;
            if (data == null && WaveManager.Instance != null) data = WaveManager.Instance.CurrentLevelData;

            if (data != null)
            {
                if (data.winCondition == WinCondition.KillCount)
                {
                    int curKills = LevelSession.Kills;
                    int target = data.targetKillCount > 0 ? data.targetKillCount : 25;
                    objectiveText.text = $"Hạ {curKills}/{target}";
                }
                else if (data.winCondition == WinCondition.SurviveTime)
                {
                    int totalSecs = Mathf.RoundToInt(data.surviveSeconds);
                    int mins = totalSecs / 60;
                    int secs = totalSecs % 60;
                    objectiveText.text = $"Sống sót {mins:D2}:{secs:D2}";
                }
                else if (data.winCondition == WinCondition.BossKill || data.isBoss)
                {
                    string bName = !string.IsNullOrEmpty(data.bossName) ? data.bossName : "Trùm";
                    objectiveText.text = $"Hạ {bName}";
                }
            }
            else
            {
                objectiveText.text = "Mục tiêu";
            }
        }

        private void UpdateKillsUI(int current, int target)
        {
            LevelData data = LevelSession.level;
            if (data == null && WaveManager.Instance != null) data = WaveManager.Instance.CurrentLevelData;
            if (data != null && (data.winCondition == WinCondition.BossKill || data.isBoss)) return;

            if (objectiveText != null)
            {
                if (target > 0)
                {
                    objectiveText.text = $"Hạ {current}/{target}";
                }
                else
                {
                    objectiveText.text = $"Hạ {current}";
                }
                if (hudTraceCount < 5)
                {
                    hudTraceCount++;
                    string instId = objectiveText.GetEntityId().ToString();
                    Debug.Log($"[KILLTRACE] HUD Update #{hudTraceCount}: text=\"{objectiveText.text}\", path={GetHierarchyPath(objectiveText.transform)}, instanceID={instId}");
                }
            }
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "null";
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private void UpdateSurviveUI(float timer, float target)
        {
            LevelData data = LevelSession.level;
            if (data == null && WaveManager.Instance != null) data = WaveManager.Instance.CurrentLevelData;
            if (data != null && (data.winCondition == WinCondition.BossKill || data.isBoss || data.winCondition != WinCondition.SurviveTime)) return;

            if (objectiveText != null)
            {
                float remaining = Mathf.Max(0f, target - timer);
                int totalSecs = Mathf.CeilToInt(remaining);
                int mins = totalSecs / 60;
                int secs = totalSecs % 60;
                objectiveText.text = $"Sống sót {mins:D2}:{secs:D2}";
            }
        }

        private void UpdateRunStarsUI(int amount)
        {
            if (runStarsText != null)
            {
                runStarsText.text = amount.ToString();
            }
        }

        private void UpdatePlayerStats(int lives, float currentHp, float maxHp)
        {
            if (hpSlider == null || hpSlider.name.Contains("Boss") || hpText == null)
            {
                AutoBindComponents();
            }

            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHp;
                hpSlider.value = currentHp;

                if (hpFillImage == null && hpSlider.fillRect != null)
                {
                    hpFillImage = hpSlider.fillRect.GetComponent<Image>();
                }
            }

            if (hpText != null)
            {
                hpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
            }

            // Hit flash
            if (lastKnownHp > 0f && currentHp < lastKnownHp)
            {
                FlashHpFillWhite();
            }
            lastKnownHp = currentHp;

            // Fill color based on % HP
            float ratio = maxHp > 0f ? (currentHp / maxHp) : 0f;
            Color targetColor;
            if (ratio >= 0.60f)
            {
                targetColor = new Color32(0x3D, 0xDC, 0x97, 0xFF); // #3DDC97 Green
            }
            else if (ratio >= 0.30f)
            {
                targetColor = new Color32(0xF5, 0x9E, 0x42, 0xFF); // #F59E42 Orange
            }
            else
            {
                targetColor = new Color32(0xFF, 0x5A, 0x5F, 0xFF); // #FF5A5F Red
            }

            currentHpColor = targetColor;
            if (hpFillImage != null && hpFlashRoutine == null)
            {
                hpFillImage.color = targetColor;
            }

            UpdateHearts(lives);
        }

        private void UpdateHearts(int lives)
        {
            if (livesContainer == null) return;

            int totalHearts = LevelRules.MaxHearts; // 3
            Sprite activeHeart = Resources.Load<Sprite>("Icons/HP_available");
            Sprite lostHeart = Resources.Load<Sprite>("Icons/Hp_lost");

            while (livesContainer.childCount < totalHearts)
            {
                GameObject heartObj = new GameObject("Heart", typeof(RectTransform), typeof(Image));
                heartObj.transform.SetParent(livesContainer, false);
                RectTransform rt = heartObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(40f, 40f);
            }

            for (int i = 0; i < livesContainer.childCount; i++)
            {
                var child = livesContainer.GetChild(i);
                Image heartImage = child.GetComponent<Image>();
                if (heartImage != null)
                {
                    if (i < lives)
                    {
                        heartImage.sprite = activeHeart;
                        heartImage.color = Color.white;
                    }
                    else
                    {
                        heartImage.sprite = lostHeart != null ? lostHeart : activeHeart;
                        heartImage.color = lostHeart != null ? Color.white : new Color(0.29f, 0.33f, 0.50f, 1f); // #4A5580
                    }
                    child.gameObject.SetActive(i < totalHearts);
                }
            }
        }

        public void UpdateSkillCooldown(float fillAmount)
        {
            if (skillCooldownOverlay != null)
            {
                skillCooldownOverlay.fillAmount = fillAmount;
            }
        }

        private void LoadDynamicSkillIcon()
        {
            if (player == null) return;
            
            string id = "";
            if (player.RuntimeLanConfig != null) id = player.RuntimeLanConfig.LanId;
            else if (player.RuntimeLanData != null) id = player.RuntimeLanData.lanId;
            
            if (string.IsNullOrEmpty(id)) return;
            
            id = id.ToLower();
            string iconName = "icon_skill_kim";
            string vfxSpriteName = "vfx_spark_proc";
            Color primaryCol = new Color32(0xE8, 0xD0, 0x8D, 0xFF); // Kim (#E8D08D)
            
            if (id.Contains("lan2") || id.Contains("hoa") || id.Contains("hoả") || id == "2")
            {
                iconName = "icon_skill_hoa";
                vfxSpriteName = "vfx_fire_swirl_proc";
                primaryCol = new Color32(0xD4, 0x42, 0x1E, 0xFF); // Hỏa (#D4421E)
            }
            else if (id.Contains("lan3") || id.Contains("thuy") || id.Contains("thuỷ") || id == "3")
            {
                iconName = "icon_skill_thuy";
                vfxSpriteName = "vfx_water_wave_alpha";
                primaryCol = new Color32(0x3A, 0x86, 0xC8, 0xFF); // Thủy (#3A86C8)
            }
            else if (id.Contains("lan4") || id.Contains("vang") || id.Contains("vàng") || id == "4")
            {
                iconName = "icon_skill_vang";
                vfxSpriteName = "vfx_firework_star_proc";
                primaryCol = new Color32(0xE8, 0xB0, 0x30, 0xFF); // Vàng (#E8B030)
            }

            currentLanPrimaryColor = primaryCol;
            if (skillBorderRing != null && skillReadyFlashRoutine == null)
            {
                skillBorderRing.color = primaryCol;
            }

            if (skillIcon != null)
            {
                Sprite loadedSprite = Resources.Load<Sprite>($"Icons/{iconName}");
                if (loadedSprite == null)
                {
                    loadedSprite = Resources.Load<Sprite>($"VFX/Generated/{vfxSpriteName}");
                }
                if (loadedSprite != null)
                {
                    skillIcon.sprite = loadedSprite;
                    skillIcon.color = Color.white;
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            HideAllPanels();

            switch (state)
            {
                case GameState.GameOver:
                    UpdateGameOverPanel();
                    if (gameOverPanel != null)
                    {
                        if (gameOverPanel.transform.parent != null) gameOverPanel.transform.parent.gameObject.SetActive(true);
                        gameOverPanel.SetActive(true);
                    }
                    break;
                case GameState.Victory:
                    UpdateVictoryPanel();
                    if (victoryPanel != null)
                    {
                        if (victoryPanel.transform.parent != null) victoryPanel.transform.parent.gameObject.SetActive(true);
                        victoryPanel.SetActive(true);
                    }
                    break;
                case GameState.Paused:
                    if (pausePanel != null)
                    {
                        if (pausePanel.transform.parent != null) pausePanel.transform.parent.gameObject.SetActive(true);
                        pausePanel.SetActive(true);
                    }
                    break;
            }
        }

        private void UpdateGameOverPanel()
        {
            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = LevelSession.usedRevive ? "THUA CUỘC" : "HẾT TIM";
            }

            int keptStars = Mathf.FloorToInt(LevelSession.runStars * GameplayBalance.DefeatStarsKeptRatio);
            if (gameOverRunStarsText != null)
            {
                gameOverRunStarsText.text = $"Sao nhặt được (giữ 50%): +{keptStars} Sao";
            }

            // Chỉ cho phép hồi sinh quảng cáo tối đa 1 lần mỗi lượt chơi
            if (reviveButtonObj != null)
            {
                reviveButtonObj.SetActive(!LevelSession.usedRevive);
            }
        }

        private void UpdateVictoryPanel()
        {
            LevelResult? res = gameManager != null ? gameManager.LastLevelResult : null;
            bool hasRes = res.HasValue;
            int levelId = LevelSession.level != null ? LevelSession.level.levelId : 1;

            if (victoryTitleText != null)
            {
                victoryTitleText.text = "CHIẾN THẮNG";
            }

            // 3 Sao đánh giá màn
            int starsCount = hasRes ? res.Value.starsThisRun : 1;
            if (victoryStarImages != null)
            {
                for (int i = 0; i < victoryStarImages.Length; i++)
                {
                    if (victoryStarImages[i] != null)
                    {
                        if (starSprite != null)
                        {
                            victoryStarImages[i].sprite = starSprite;
                        }
                        victoryStarImages[i].color = (i < starsCount) ? ColorGold : ColorDimStar;
                    }
                }
            }

            // Kỷ lục mới
            if (victoryRecordBadge != null)
            {
                victoryRecordBadge.SetActive(hasRes && res.Value.isNewRecord);
            }

            // Sao nhặt được (100%)
            if (victoryRunStarsText != null)
            {
                victoryRunStarsText.text = $"Sao nhặt được: +{LevelSession.runStars}";
            }

            // Thưởng mốc
            if (victoryMilestoneRewardText != null)
            {
                if (hasRes && (res.Value.starsGranted > 0 || res.Value.diamondsGranted > 0))
                {
                    string rewardStr = $"Thưởng mốc: +{res.Value.starsGranted} Sao";
                    if (res.Value.diamondsGranted > 0) rewardStr += $" +{res.Value.diamondsGranted} Kim Cương";
                    victoryMilestoneRewardText.text = rewardStr;
                }
                else
                {
                    victoryMilestoneRewardText.text = "Không có quà mới";
                }
            }

            // Thông báo mở khóa Hồi mới
            if (victoryNextActBanner != null)
            {
                victoryNextActBanner.SetActive(hasRes && res.Value.unlockedNextAct);
            }

            // Chú thích giới hạn hồi sinh
            if (victoryReviveCappedNotice != null)
            {
                victoryReviveCappedNotice.SetActive(hasRes && res.Value.cappedByRevive);
            }

            // Nút MÀN TIẾP (chỉ hiện khi còn màn tiếp theo <= 15 và đã mở)
            if (victoryNextLevelButton != null)
            {
                int nextId = levelId + 1;
                bool canNext = (nextId <= 15);
                if (canNext && LevelProgressManager.Instance != null)
                {
                    canNext = LevelProgressManager.Instance.IsLevelUnlocked(nextId);
                }
                victoryNextLevelButton.SetActive(canNext);
            }
        }

        private void BindPanelButtons()
        {
            if (victoryNextLevelButton != null)
            {
                Button nextBtn = victoryNextLevelButton.GetComponent<Button>();
                if (nextBtn != null)
                {
                    nextBtn.onClick.RemoveAllListeners();
                    nextBtn.onClick.AddListener(OnNextLevelButtonClicked);
                }
            }
            if (victoryReplayButton != null)
            {
                victoryReplayButton.onClick.RemoveAllListeners();
                victoryReplayButton.onClick.AddListener(OnRestartButtonClicked);
            }
            if (victorySelectLevelButton != null)
            {
                victorySelectLevelButton.onClick.RemoveAllListeners();
                victorySelectLevelButton.onClick.AddListener(OnSelectLevelButtonClicked);
            }
            if (gameOverReplayButton != null)
            {
                gameOverReplayButton.onClick.RemoveAllListeners();
                gameOverReplayButton.onClick.AddListener(OnRestartButtonClicked);
            }
            if (gameOverGiveUpButton != null)
            {
                gameOverGiveUpButton.onClick.RemoveAllListeners();
                gameOverGiveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
            }
            if (skillButton != null)
            {
                skillButton.onClick.RemoveAllListeners();
                skillButton.onClick.AddListener(OnSkillButtonClicked);
            }
        }

        private void HideAllPanels()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void OnRestartButtonClicked()
        {
            Audio.AudioManager.Instance?.PlayUIClick();
            if (gameManager != null)
            {
                gameManager.RestartLevel();
            }
        }

        public void OnResumeButtonClicked()
        {
            Audio.AudioManager.Instance?.PlayUIClick();
            if (gameManager != null)
            {
                gameManager.TogglePause();
            }
        }

        public void OnNextLevelButtonClicked()
        {
            Audio.AudioManager.Instance?.PlayUIClick();
            if (gameManager != null)
            {
                gameManager.NextLevel();
            }
        }

        public void OnSelectLevelButtonClicked()
        {
            Audio.AudioManager.Instance?.PlayUIClick();
            if (gameManager != null)
            {
                gameManager.ReturnToLevelSelect();
            }
        }

        public void OnGiveUpButtonClicked()
        {
            Audio.AudioManager.Instance?.PlayUIClick();
            if (gameManager != null)
            {
                gameManager.ReturnToLevelSelect();
            }
        }

        public void OnMainMenuButtonClicked()
        {
            OnSelectLevelButtonClicked();
        }

        public void OnReviveButtonClicked()
        {
            Audio.AudioManager.Instance?.PlayUIClick();
            if (Monetization.MonetizationManager.Instance != null)
            {
                Monetization.MonetizationManager.Instance.ShowRewardedAd(success => 
                {
                    if (success)
                    {
                        if (reviveButtonObj != null) reviveButtonObj.SetActive(false); // Chỉ được hồi sinh 1 lần
                        if (gameManager != null) gameManager.RevivePlayer();
                    }
                });
            }
            else
            {
                // Fallback nếu không có MonetizationManager trong test
                Debug.Log("[GameplayHUD] MonetizationManager không có sẵn, tự động kích hoạt hồi sinh giả lập.");
                if (reviveButtonObj != null) reviveButtonObj.SetActive(false);
                if (gameManager != null) gameManager.RevivePlayer();
            }
        }
    }
}
