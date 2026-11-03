using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Điều khiển màn Chọn Hồi (ActSelectView): hiển thị danh sách thẻ Hồi từ ILevelDataProvider,
    /// TopBar tiền tệ, thanh tiến độ tổng sao, và thông báo Toast khi bấm thẻ bị khóa.
    /// </summary>
    public class ActSelectView : MonoBehaviour
    {
        [Header("Scroll & Cards Container")]
        public RectTransform contentContainer;
        public GameObject actCardTemplate;

        [Header("TopBar & HeaderBar")]
        public TMP_Text totalStarsText;
        public TMP_Text starCurrencyText;
        public TMP_Text diamondCurrencyText;
        public Button backButton;

        [Header("Toast Notification")]
        public RectTransform toastRoot;
        public CanvasGroup toastCanvasGroup;
        public TMP_Text toastText;

        private ILevelDataProvider currentProvider;
        private Action<int> onActSelectedCallback;
        private Action onBackCallback;
        private readonly List<ActCardUI> activeCards = new List<ActCardUI>();
        private Coroutine toastCoroutine;

        private void Awake()
        {
            AutoBindReferences();
            BindBackButtonListener();
        }

        private void BindBackButtonListener()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackButtonClicked);
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
                toastCoroutine = null;
            }
        }

        public void AutoBindReferences()
        {
            if (contentContainer == null)
            {
                Transform ct = transform.Find("SafeAreaRoot/ActListScroll/Viewport/Content");
                if (ct != null) contentContainer = ct as RectTransform;
            }

            if (totalStarsText == null)
            {
                Transform pill = transform.Find("SafeAreaRoot/HeaderBar/TotalStarsPill")
                              ?? transform.Find("HeaderBar/TotalStarsPill");
                if (pill != null) totalStarsText = pill.GetComponentInChildren<TMP_Text>();
            }

            if (starCurrencyText == null)
            {
                Transform pill = transform.Find("SafeAreaRoot/TopBar/StarPill")
                              ?? transform.Find("TopBar/StarPill");
                if (pill != null)
                {
                    Transform amt = pill.Find("Amount") ?? pill.Find("Text");
                    starCurrencyText = (amt != null) ? amt.GetComponent<TMP_Text>() : pill.GetComponentInChildren<TMP_Text>();
                }
            }

            if (diamondCurrencyText == null)
            {
                Transform pill = transform.Find("SafeAreaRoot/TopBar/DiamondPill")
                              ?? transform.Find("TopBar/DiamondPill");
                if (pill != null)
                {
                    Transform amt = pill.Find("Amount") ?? pill.Find("Text");
                    diamondCurrencyText = (amt != null) ? amt.GetComponent<TMP_Text>() : pill.GetComponentInChildren<TMP_Text>();
                }
            }

            if (backButton == null)
            {
                Transform btn = transform.Find("SafeAreaRoot/HeaderBar/BackBtn")
                             ?? transform.Find("SafeAreaRoot/HeaderBar/BackButton")
                             ?? transform.Find("HeaderBar/BackBtn")
                             ?? transform.Find("HeaderBar/BackButton")
                             ?? transform.Find("SafeAreaRoot/TopBar/BackBtn")
                             ?? transform.Find("SafeAreaRoot/TopBar/BackButton");

                if (btn == null)
                {
                    Button[] allButtons = GetComponentsInChildren<Button>(true);
                    foreach (var b in allButtons)
                    {
                        if (b != null && b.gameObject.name.IndexOf("Back", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            btn = b.transform;
                            break;
                        }
                    }
                }

                if (btn != null) backButton = btn.GetComponent<Button>();
            }

            BindBackButtonListener();

            if (toastRoot == null)
            {
                Transform t = transform.Find("ToastRoot") ?? transform.Find("SafeAreaRoot/ToastRoot");
                if (t != null)
                {
                    toastRoot = t as RectTransform;
                    toastCanvasGroup = toastRoot.GetComponent<CanvasGroup>();
                    toastText = toastRoot.GetComponentInChildren<TMP_Text>();
                }
            }
        }

        public void Initialize(ILevelDataProvider provider, Action<int> onActSelected, Action onBack)
        {
            currentProvider = provider;
            onActSelectedCallback = onActSelected;
            onBackCallback = onBack;

            DemHoiDenLong.Audio.AudioManager.Instance?.PlayBGM(DemHoiDenLong.Audio.AudioManager.Instance?.bgmMainMenu);

            AutoBindReferences();
            BindBackButtonListener();
            SubscribeEvents();
            RefreshDisplay();
        }

        private void SubscribeEvents()
        {
            UnsubscribeEvents();

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnStarChanged += HandleStarChanged;
                EconomyManager.Instance.OnDiamondChanged += HandleDiamondChanged;
            }

            if (currentProvider != null)
            {
                currentProvider.OnDataChanged += HandleDataChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnStarChanged -= HandleStarChanged;
                EconomyManager.Instance.OnDiamondChanged -= HandleDiamondChanged;
            }

            if (currentProvider != null)
            {
                currentProvider.OnDataChanged -= HandleDataChanged;
            }
        }

        private void HandleStarChanged(int stars) => UpdateCurrencies();
        private void HandleDiamondChanged(int diamonds) => UpdateCurrencies();
        private void HandleDataChanged() => RefreshDisplay();

        public void RefreshDisplay()
        {
            if (currentProvider == null)
            {
                currentProvider = LevelDataService.Provider;
            }

            UpdateCurrencies();
            UpdateTotalStarsPill();
            PopulateActCards();
        }

        private void UpdateCurrencies()
        {
            int stars = EconomyManager.Instance != null 
                ? EconomyManager.Instance.StarCount 
                : PlayerPrefs.GetInt("Player_Stars", PlayerPrefs.GetInt("PlayerStar", 1000));

            int diamonds = EconomyManager.Instance != null 
                ? EconomyManager.Instance.DiamondCount 
                : PlayerPrefs.GetInt("Player_Diamonds", PlayerPrefs.GetInt("PlayerDiamond", 500));

            if (starCurrencyText != null)
            {
                starCurrencyText.text = stars.ToString("N0");
            }

            if (diamondCurrencyText != null)
            {
                diamondCurrencyText.text = diamonds.ToString("N0");
            }
        }

        private void UpdateTotalStarsPill()
        {
            if (totalStarsText != null && currentProvider != null)
            {
                int earned = currentProvider.GetTotalStars();
                int max = currentProvider.GetMaxTotalStars();
                totalStarsText.text = $"{earned}/{max}";
            }
        }

        private void PopulateActCards()
        {
            if (contentContainer == null || currentProvider == null) return;

            int actCount = currentProvider.GetActCount();
            if (actCount <= 0) actCount = 3;

            // Thu thập thẻ sẵn có trong contentContainer
            List<Transform> existingCards = new List<Transform>();
            for (int i = 0; i < contentContainer.childCount; i++)
            {
                Transform child = contentContainer.GetChild(i);
                if (child != null && child.GetComponent<ActCardUI>() != null)
                {
                    existingCards.Add(child);
                }
            }

            activeCards.Clear();

            // Nếu số thẻ hiện tại ít hơn actCount, nhân bản từ thẻ mẫu hoặc thẻ đầu tiên
            if (existingCards.Count < actCount && existingCards.Count > 0)
            {
                GameObject template = actCardTemplate != null ? actCardTemplate : existingCards[0].gameObject;
                while (existingCards.Count < actCount)
                {
                    GameObject newCard = Instantiate(template, contentContainer);
                    newCard.name = $"ActCard_{existingCards.Count + 1}";
                    existingCards.Add(newCard.transform);
                }
            }

            // Gán dữ liệu cho từng thẻ
            for (int i = 0; i < existingCards.Count; i++)
            {
                Transform cardTrans = existingCards[i];
                if (i < actCount)
                {
                    cardTrans.gameObject.SetActive(true);
                    int actIndex = i + 1; // 1-based cho UI

                    ActCardUI cardUI = cardTrans.GetComponent<ActCardUI>();
                    if (cardUI != null)
                    {
                        string title = currentProvider.GetActTitle(actIndex);
                        string subtitle = currentProvider.GetActSubtitle(actIndex);
                        Sprite banner = currentProvider.GetActBanner(actIndex);
                        int cleared = currentProvider.GetActClearedLevels(actIndex);
                        int total = currentProvider.GetActLevelCount(actIndex);
                        int stars = currentProvider.GetActStars(actIndex);
                        int maxStars = currentProvider.GetActMaxStars(actIndex);
                        ActStatus status = currentProvider.GetActStatus(actIndex);
                        string unlockHint = currentProvider.GetUnlockHint(actIndex);
                        Color accent = currentProvider.GetActAccentColor(actIndex);

                        if (string.IsNullOrEmpty(unlockHint))
                        {
                            unlockHint = $"Hoàn thành Hồi {actIndex - 1} để mở khóa!";
                        }

                        cardUI.Setup(actIndex, title, banner, cleared, total, stars, maxStars, status,
                                     unlockHint, accent, subtitle, "Tiến độ:", "Sao:",
                                     OnCardClicked, OnCardLockedClicked);

                        // Đảm bảo starsRow tắt theo quy ước thiết kế
                        if (cardUI.starsRow != null) cardUI.starsRow.SetActive(false);

                        activeCards.Add(cardUI);
                    }
                }
                else
                {
                    cardTrans.gameObject.SetActive(false);
                }
            }
        }

        private void OnCardClicked(ActCardUI card)
        {
            if (card == null) return;
            onActSelectedCallback?.Invoke(card.ActIndex);
        }

        private void OnCardLockedClicked(ActCardUI card)
        {
            if (card == null) return;
            string hint = currentProvider != null ? currentProvider.GetUnlockHint(card.ActIndex) : null;
            if (string.IsNullOrEmpty(hint))
            {
                hint = $"Hoàn thành Hồi {card.ActIndex - 1} để mở khóa!";
            }
            ShowToast(hint);
        }

        public void ShowToast(string message)
        {
            if (toastRoot == null)
            {
                var ldToast = FindAnyObjectByType<LevelDetailView>(FindObjectsInactive.Include);
                if (ldToast != null && ldToast.toastRoot != null)
                {
                    Transform parent = transform.Find("SafeAreaRoot") ?? transform;
                    GameObject go = Instantiate(ldToast.toastRoot.gameObject, parent);
                    go.name = "ToastRoot";
                    toastRoot = go.GetComponent<RectTransform>();
                    toastCanvasGroup = go.GetComponent<CanvasGroup>();
                    toastText = go.GetComponentInChildren<TMP_Text>();
                }
            }

            if (toastText != null) toastText.text = message;

            if (toastRoot != null && gameObject.activeInHierarchy)
            {
                if (toastCoroutine != null) StopCoroutine(toastCoroutine);
                toastCoroutine = StartCoroutine(ToastSequence());
            }
        }

        private IEnumerator ToastSequence()
        {
            if (toastRoot != null) toastRoot.gameObject.SetActive(true);
            if (toastCanvasGroup != null) toastCanvasGroup.alpha = 0f;

            // Fade in 0.15s
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.unscaledDeltaTime;
                if (toastCanvasGroup != null) toastCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.15f);
                yield return null;
            }
            if (toastCanvasGroup != null) toastCanvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(1.5f);

            // Fade out 0.3s
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.unscaledDeltaTime;
                if (toastCanvasGroup != null) toastCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.3f);
                yield return null;
            }

            if (toastCanvasGroup != null) toastCanvasGroup.alpha = 0f;
            if (toastRoot != null) toastRoot.gameObject.SetActive(false);
            toastCoroutine = null;
        }

        private void Update()
        {
            // Neu khong co controller LevelSelectFlow quan ly, tu bat Escape de fallback
            if (onBackCallback == null && IsEscapePressed())
            {
                OnBackButtonClicked();
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

        private void OnBackButtonClicked()
        {
            DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIClick();
            Debug.Log("[NAV] ActSelectView OnBackButtonClicked invoked");
            if (onBackCallback != null)
            {
                onBackCallback.Invoke();
            }
            else
            {
                DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.MainMenu, "ActSelectView_Back_Fallback");
            }
        }
    }
}
