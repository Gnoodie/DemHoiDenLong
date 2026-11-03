using System;

using System.Collections;

using System.Collections.Generic;

using TMPro;

using UnityEngine;

using UnityEngine.UI;



namespace DemHoiDenLong.UI

{

    /// <summary>

    /// Điều khiển toàn bộ màn Chọn Màn (LevelDetailView): hiển thị bản đồ Hồi,

    /// các node màn chơi, đường nối nét đứt, HeaderBar tiến độ và xử lý mở modal.

    /// </summary>

    public class LevelDetailView : MonoBehaviour

    {

        [Header("Root Components")]

        public CanvasGroup canvasGroup;

        public Image backgroundImage;

        public AspectRatioFitter backgroundFitter;

        public RectTransform nodesContainer;

        public PathDrawer pathDrawer;



        [Header("Prefabs for Dynamic Nodes")]

        public GameObject levelNodePrefab;

        public GameObject bossNodePrefab;



        [Header("TopBar & HeaderBar")]

        public Button backButton;

        public TMP_Text actSubheadText;

        public TMP_Text actTitleText;

        public TMP_Text actProgressText;

        public TMP_Text starCurrencyText;

        public TMP_Text diamondCurrencyText;



        [Header("Toast Notification")]

        public RectTransform toastRoot;

        public CanvasGroup toastCanvasGroup;

        public TMP_Text toastText;



        [Header("Modal Overlay")]

        public LevelDetailModal detailModal;



        [Header("Scene Assets")]

        public Sprite nodeBaseSprite;

        public Sprite bgVillageMap;

        public Sprite bgOldtownMap;

        public Sprite bgMoonMap;

        public Sprite bossAct1;

        public Sprite bossAct2;

        public Sprite bossAct3;



        private int currentActIndex = 2;

        private ILevelDataProvider currentProvider;

        private Action onBackCallback;



        private readonly List<LevelNodeUI> activeNodeUIs = new List<LevelNodeUI>();

        private Coroutine entranceCoroutine;

        private Coroutine toastCoroutine;



        private void Awake()

        {

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            if (toastCanvasGroup != null) toastCanvasGroup.alpha = 0f;

            if (backgroundImage != null) backgroundImage.raycastTarget = false;



            AutoBindCurrencies();

            AdjustHeaderLayout();

        }



        private void OnEnable()

        {

            SubscribeCurrencies();

            UpdateCurrencies();

        }



        private void OnDisable()

        {

            UnsubscribeCurrencies();

        }



        private void SubscribeCurrencies()

        {

            UnsubscribeCurrencies();

            if (DemHoiDenLong.Gameplay.EconomyManager.Instance != null)

            {

                DemHoiDenLong.Gameplay.EconomyManager.Instance.OnStarChanged += HandleStarChanged;

                DemHoiDenLong.Gameplay.EconomyManager.Instance.OnDiamondChanged += HandleDiamondChanged;

            }

        }



        private void UnsubscribeCurrencies()

        {

            if (DemHoiDenLong.Gameplay.EconomyManager.Instance != null)

            {

                DemHoiDenLong.Gameplay.EconomyManager.Instance.OnStarChanged -= HandleStarChanged;

                DemHoiDenLong.Gameplay.EconomyManager.Instance.OnDiamondChanged -= HandleDiamondChanged;

            }

        }



        private void HandleStarChanged(int stars) => UpdateCurrencies();

        private void HandleDiamondChanged(int diamonds) => UpdateCurrencies();



        private void AutoBindCurrencies()

        {

            if (starCurrencyText == null)

            {

                Transform pill = transform.Find("SafeAreaRoot/TopBar/StarPill") ?? transform.Find("TopBar/StarPill");

                if (pill != null)

                {

                    Transform amt = pill.Find("Amount") ?? pill.Find("Text");

                    starCurrencyText = (amt != null) ? amt.GetComponent<TMP_Text>() : pill.GetComponentInChildren<TMP_Text>();

                }

            }

            if (diamondCurrencyText == null)

            {

                Transform pill = transform.Find("SafeAreaRoot/TopBar/DiamondPill") ?? transform.Find("TopBar/DiamondPill");

                if (pill != null)

                {

                    Transform amt = pill.Find("Amount") ?? pill.Find("Text");

                    diamondCurrencyText = (amt != null) ? amt.GetComponent<TMP_Text>() : pill.GetComponentInChildren<TMP_Text>();

                }

            }

        }



        private void UpdateCurrencies()

        {

            int stars = DemHoiDenLong.Gameplay.EconomyManager.Instance != null 

                ? DemHoiDenLong.Gameplay.EconomyManager.Instance.StarCount 

                : PlayerPrefs.GetInt("Player_Stars", PlayerPrefs.GetInt("PlayerStar", 1000));



            int diamonds = DemHoiDenLong.Gameplay.EconomyManager.Instance != null 

                ? DemHoiDenLong.Gameplay.EconomyManager.Instance.DiamondCount 

                : PlayerPrefs.GetInt("Player_Diamonds", PlayerPrefs.GetInt("PlayerDiamond", 500));



            if (starCurrencyText != null) starCurrencyText.text = stars.ToString("N0");

            if (diamondCurrencyText != null) diamondCurrencyText.text = diamonds.ToString("N0");

        }



        private void Start()

        {

            if (currentProvider == null)

            {

                var provider = LevelDataService.Provider;

                if (provider != null)

                {

                    int act = LevelSelectContext.SelectedActIndex > 0 ? LevelSelectContext.SelectedActIndex : 2;

                    Initialize(act, provider, null);

                }

            }

        }



        private void AdjustHeaderLayout()

        {

            // 1. Căn giữa chuẩn xác mũi tên trong nút Back và cấu hình HorizontalLayoutGroup cho HeaderBar

            if (backButton != null)

            {

                LayoutElement leBack = backButton.GetComponent<LayoutElement>();

                if (leBack == null) leBack = backButton.gameObject.AddComponent<LayoutElement>();

                leBack.preferredWidth = 88f;

                leBack.minWidth = 88f;

                leBack.flexibleWidth = 0f;



                Transform arrowTrans = backButton.transform.Find("Arrow");

                if (arrowTrans != null)

                {

                    RectTransform rtArrow = arrowTrans as RectTransform;

                    if (rtArrow != null)

                    {

                        rtArrow.anchorMin = new Vector2(0.5f, 0.5f);

                        rtArrow.anchorMax = new Vector2(0.5f, 0.5f);

                        rtArrow.pivot = new Vector2(0.5f, 0.5f);

                        // Ký tự font '‹' có baseline thấp nên cần nâng lên +7px để đúng tâm hình tròn

                        rtArrow.anchoredPosition = new Vector2(-2f, 7f);

                    }

                }



                if (backButton.transform.parent != null)

                {

                    HorizontalLayoutGroup headerHlg = backButton.transform.parent.GetComponent<HorizontalLayoutGroup>();

                    if (headerHlg != null)

                    {

                        headerHlg.padding = new RectOffset(40, 40, 0, 0);

                        headerHlg.spacing = 16f;

                        headerHlg.childAlignment = TextAnchor.MiddleLeft;

                        headerHlg.childControlWidth = true;

                        headerHlg.childControlHeight = false;

                        headerHlg.childForceExpandWidth = false;

                        headerHlg.childForceExpandHeight = false;

                    }

                }

            }



            // 2. TitleGroup flexibleWidth 1, tiêu đề không xuống dòng

            if (actTitleText != null)

            {

                actTitleText.enableAutoSizing = true;

                actTitleText.fontSizeMin = 20f;

                actTitleText.fontSizeMax = 44f;

                actTitleText.textWrappingMode = TextWrappingModes.NoWrap;

                actTitleText.overflowMode = TextOverflowModes.Ellipsis;

                actTitleText.margin = new Vector4(0f, 0f, 24f, 0f);



                Transform titleGroup = actTitleText.transform.parent;

                if (titleGroup != null)

                {

                    LayoutElement leTitle = titleGroup.GetComponent<LayoutElement>();

                    if (leTitle == null) leTitle = titleGroup.gameObject.AddComponent<LayoutElement>();

                    leTitle.flexibleWidth = 1f;

                    leTitle.minWidth = 100f;

                }

            }



            // 3. Pill tiến độ "x/y" ở HeaderBar: thêm nền ui_pill #1C2A5C alpha 0.95, chữ cỡ 28 Bold màu kem, icon star.png 32x32, preferredWidth 190

            if (actProgressText != null)

            {

                RectTransform pillRt = actProgressText.transform.parent as RectTransform;

                if (pillRt != null && pillRt.name == "Fill" && pillRt.parent is RectTransform parentRt)

                {

                    pillRt = parentRt;

                }



                if (pillRt != null)

                {

                    LayoutElement lePill = pillRt.GetComponent<LayoutElement>();

                    if (lePill == null) lePill = pillRt.gameObject.AddComponent<LayoutElement>();

                    lePill.preferredWidth = 190f;

                    lePill.minWidth = 190f;

                    lePill.flexibleWidth = 0f;



                    pillRt.sizeDelta = new Vector2(190f, 48f);



                    Image pillBg = pillRt.GetComponent<Image>();

                    if (pillBg == null) pillBg = pillRt.gameObject.AddComponent<Image>();

#if UNITY_EDITOR

                    if (pillBg.sprite == null)

                    {

                        pillBg.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_pill.png");

                    }

#endif

                    pillBg.color = new Color(0.11f, 0.165f, 0.361f, 0.95f); // #1C2A5C alpha 0.95

                    pillBg.type = Image.Type.Sliced;



                    HorizontalLayoutGroup hlg = pillRt.GetComponent<HorizontalLayoutGroup>();

                    if (hlg != null)

                    {

                        hlg.padding = new RectOffset(14, 16, 0, 0);

                        hlg.spacing = 8f;

                        hlg.childAlignment = TextAnchor.MiddleCenter;

                    }

                    Transform starObj = pillRt.Find("Star") ?? pillRt.Find("Fill/Star");

                    if (starObj != null)

                    {

                        RectTransform rtStar = starObj as RectTransform;

                        if (rtStar != null) rtStar.sizeDelta = new Vector2(32f, 32f); // 32x32

                        Image imgStar = starObj.GetComponent<Image>();

                        if (imgStar != null)

                        {

#if UNITY_EDITOR

                            if (imgStar.sprite == null)

                            {

                                imgStar.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/star.png");

                            }

#endif

                            imgStar.preserveAspect = true;

                        }

                    }

                    actProgressText.fontSize = 28f;

                    actProgressText.fontStyle = FontStyles.Bold;

                    actProgressText.color = new Color(0.957f, 0.945f, 0.902f, 1f); // #F4F1E6 kem

                    actProgressText.alignment = TextAlignmentOptions.Center;

                }

            }

        }



        public void Initialize(int actIndex, ILevelDataProvider provider, Action onBack)

        {
            if (actIndex <= 0) actIndex = 1;

            currentActIndex = actIndex;

            currentProvider = provider;

            onBackCallback = onBack;



            if (detailModal != null && detailModal.gameObject.activeSelf)

            {

                detailModal.gameObject.SetActive(false);

            }



            // Cập nhật nền Hồi

            Sprite bgSprite = provider.GetActBackground(actIndex);

            if (backgroundImage != null && bgSprite != null)

            {

                backgroundImage.sprite = bgSprite;

                if (backgroundFitter != null)

                {

                    backgroundFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

                    backgroundFitter.aspectRatio = (float)bgSprite.rect.width / bgSprite.rect.height;

                }

            }



            // HeaderBar

            Color accentColor = provider.GetActAccentColor(actIndex);

            if (actSubheadText != null)

            {

                actSubheadText.text = provider.GetActSubtitle(actIndex).ToUpper();

                actSubheadText.color = accentColor;

            }



            if (actTitleText != null)

            {

                actTitleText.text = provider.GetActTitle(actIndex).ToUpper();

            }



            if (actProgressText != null)

            {

                actProgressText.text = $"{provider.GetActStars(actIndex)}/{provider.GetActMaxStars(actIndex)}";

            }



            // Nạp danh sách Node

            SetupNodes(actIndex, provider);



            // Chạy hoạt ảnh vào màn

            if (gameObject.activeInHierarchy)

            {

                if (entranceCoroutine != null) StopCoroutine(entranceCoroutine);

                entranceCoroutine = StartCoroutine(EntranceSequence());

            }

        }



        private void SetupNodes(int actIndex, ILevelDataProvider provider)

        {

            List<LevelNodeData> levels = provider.GetLevelsForAct(actIndex);

            if (levels == null || nodesContainer == null) return;



            activeNodeUIs.Clear();



            List<Transform> nodeTransforms = new List<Transform>();

            List<NodeState> nodeStates = new List<NodeState>();



            // Thu thập các node sẵn có trong NodesContainer

            List<Transform> existingNodes = new List<Transform>();

            for (int i = 0; i < nodesContainer.childCount; i++)

            {

                Transform child = nodesContainer.GetChild(i);

                if (child == null || (pathDrawer != null && child == pathDrawer.transform)) continue;

                existingNodes.Add(child);

            }



            Color accentColor = provider.GetActAccentColor(actIndex);

            Sprite bossSprite = provider.GetBossSprite(actIndex);

            Sprite nodeBase = (nodeBaseSprite != null) ? nodeBaseSprite : provider.GetNodeBaseSprite();



            for (int i = 0; i < levels.Count; i++)

            {

                LevelNodeData data = levels[i];

                Transform nodeTransform = null;

                LevelNodeUI nodeUI = null;



                if (i < existingNodes.Count)

                {

                    nodeTransform = existingNodes[i];

                    nodeUI = nodeTransform.GetComponent<LevelNodeUI>();

                }



                // Nếu chưa có hoặc cần tạo mới

                if (nodeTransform == null)

                {

                    GameObject prefab = data.isBoss ? bossNodePrefab : levelNodePrefab;

                    if (prefab != null)

                    {

                        GameObject go = Instantiate(prefab, nodesContainer);

                        go.name = data.isBoss ? "Node_Boss" : $"Node_{i + 1}";

                        nodeTransform = go.transform;

                        nodeUI = go.GetComponent<LevelNodeUI>();



                        // Set anchor theo data

                        RectTransform rt = nodeTransform as RectTransform;

                        if (rt != null)

                        {

                            rt.anchorMin = data.anchorPos;

                            rt.anchorMax = data.anchorPos;

                            rt.pivot = new Vector2(0.5f, 0.5f);

                            rt.anchoredPosition = Vector2.zero;

                        }

                    }

                }

                if (nodeTransform != null)

                {

                    nodeTransform.gameObject.SetActive(true);



                    // Cập nhật anchor theo dữ liệu của Hồi tương ứng

                    RectTransform rt = nodeTransform as RectTransform;

                    if (rt != null)

                    {

                        rt.anchorMin = data.anchorPos;

                        rt.anchorMax = data.anchorPos;

                        rt.pivot = new Vector2(0.5f, 0.5f);

                        rt.anchoredPosition = Vector2.zero;

                    }



                    if (nodeUI != null)

                    {

                        if (data.isBoss)

                        {

                            // Đảm bảo lấy đúng sprite node boss mới theo Act

                            Sprite bSpr = bossSprite != null ? bossSprite : provider.GetBossSprite(actIndex);
                            string source = (bossSprite != null) ? "DirectProvider" : (provider != null ? "ActData" : "Fallback");
                            string entryPath = (LevelSelectContext.OpenActOnStart >= 0) ? "OpenActOnStart" : (LevelSelectContext.SelectedActIndex > 0 ? "SelectedActIndex" : "Direct");
                            string sprName = (bSpr != null) ? bSpr.name : "null";
                            Debug.Log($"[BOSSNODE] actIndex={actIndex} sprite={sprName} source={source} path={entryPath} contextAct={LevelSelectContext.SelectedActIndex} openActOnStart={LevelSelectContext.OpenActOnStart}");

                            if (bSpr != null)

                            {

                                // Tắt các layer thừa từ cấu trúc cũ (MaskContainer, InnerRing) để lộ toàn bộ khung hoa văn của node boss mới

                                Transform mask = nodeTransform.Find("Visual/MaskContainer") ?? nodeTransform.Find("MaskContainer");

                                if (mask != null) mask.gameObject.SetActive(false);



                                Transform inner = nodeTransform.Find("Visual/InnerRing") ?? nodeTransform.Find("InnerRing");

                                if (inner != null) inner.gameObject.SetActive(false);



                                Transform outer = nodeTransform.Find("Visual/OuterFrame") ?? nodeTransform.Find("OuterFrame");

                                if (outer != null)

                                {

                                    Image imgO = outer.GetComponent<Image>();

                                    if (imgO != null)

                                    {

                                        imgO.sprite = bSpr;

                                        imgO.color = Color.white;

                                    }

                                }



                                Transform bossImg = nodeTransform.Find("Visual/BossImage") ?? nodeTransform.Find("BossImage");

                                if (bossImg != null)

                                {

                                    Image imgB = bossImg.GetComponent<Image>();

                                    if (imgB != null)

                                    {

                                        imgB.sprite = bSpr;

                                        imgB.color = Color.white;

                                    }

                                }



                                if (nodeUI.bossPortrait != null)

                                {

                                    nodeUI.bossPortrait.sprite = bSpr;

                                    nodeUI.bossPortrait.color = Color.white;

                                }

                                if (nodeUI.baseImage != null)

                                {

                                    nodeUI.baseImage.sprite = bSpr;

                                    nodeUI.baseImage.color = Color.white;

                                }

                            }

                        }

                        else if (!data.isBoss && nodeBase != null)

                        {

                            if (nodeUI.baseImage != null) nodeUI.baseImage.sprite = nodeBase;

                        }



                        nodeUI.EnsureComponents();

                        nodeUI.Setup(data, accentColor, OnNodeClicked, OnLockedNodeClicked);

                        activeNodeUIs.Add(nodeUI);

                    }



                    nodeTransforms.Add(nodeTransform);

                    nodeStates.Add(data.state);

                }

            }



            // Tắt các node thừa nếu có

            for (int i = levels.Count; i < existingNodes.Count; i++)

            {

                existingNodes[i].gameObject.SetActive(false);

            }



            // Cập nhật đường nét đứt

            if (pathDrawer != null)

            {

                pathDrawer.SetNodes(nodeTransforms, nodeStates);

            }

        }



        private void OnNodeClicked(LevelNodeUI node)

        {

            Debug.Log($"[LevelDetailView] OnNodeClicked node: {node.name}, levelId: {node.nodeData.levelId}");

            if (detailModal == null)

            {

                detailModal = GetComponentInChildren<LevelDetailModal>(true);

                if (detailModal == null)

                    detailModal = FindAnyObjectByType<LevelDetailModal>(FindObjectsInactive.Include);

            }

            if (currentProvider == null)

            {

                currentProvider = LevelDataService.Provider;

            }



            if (detailModal != null && currentProvider != null)

            {

                string actName = currentProvider.GetActTitle(currentActIndex);

                detailModal.Open(actName, node.nodeData, (levelId) =>

                {

                    // Callback bắt đầu chơi màn (Giai đoạn 5A)

                    Debug.Log($"Bắt đầu chơi màn {levelId} của Hồi {currentActIndex}");

                    LevelSelectContext.SelectedLevelId = levelId;

                    LevelSelectContext.LastPlayedLevelId = levelId;

                    Time.timeScale = 1f;

                    DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.Gameplay, "LevelDetailView_PlayButton");

                });

            }

            else

            {

                Debug.LogError($"[LevelDetailView] Không thể mở popup! detailModal={(detailModal != null)}, provider={(currentProvider != null)}");

            }

        }



        private void OnLockedNodeClicked(LevelNodeUI node)

        {

            ShowToast("Hoàn thành màn trước để mở khóa");

        }



        public void ShowToast(string message)

        {

            if (!gameObject.activeInHierarchy) return;

            if (toastText != null) toastText.text = message;

            if (toastCoroutine != null) StopCoroutine(toastCoroutine);

            toastCoroutine = StartCoroutine(ToastSequence());

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



            // Giữ hiển thị 1.5s

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



        private IEnumerator EntranceSequence()

        {

            // Các node xuất hiện lần lượt từ node 1 lên trên (scale 0.6 -> 1 và alpha, mỗi node 0.3s, so le 0.07s)

            float delay = 0f;

            for (int i = 0; i < activeNodeUIs.Count; i++)

            {

                LevelNodeUI ui = activeNodeUIs[i];

                if (ui != null)

                {

                    ui.PlayEntranceAnimation(delay);

                    delay += 0.07f;

                }

            }



            // Fallback an toàn sau 1.2s: đảm bảo mọi thứ đều hiển thị trọn vẹn

            yield return new WaitForSecondsRealtime(1.2f);



            for (int i = 0; i < activeNodeUIs.Count; i++)

            {

                LevelNodeUI ui = activeNodeUIs[i];

                if (ui != null)

                {

                    if (ui.canvasGroup != null) ui.canvasGroup.alpha = 1f;

                    if (ui.visualContainer != null) ui.visualContainer.localScale = Vector3.one;

                }

            }



            entranceCoroutine = null;

        }



        private void OnBackClicked()
        {
            DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIClick();
            onBackCallback?.Invoke();
        }



#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private bool showDebugPanel = false;

        private Rect debugRect = new Rect(20, 100, 320, 400);

        private bool debugUsedRevive = false;



        private void Update()

        {

            if (IsF9Pressed())

            {

                showDebugPanel = !showDebugPanel;

            }

        }



        private bool IsF9Pressed()

        {

#if ENABLE_INPUT_SYSTEM

            if (UnityEngine.InputSystem.Keyboard.current != null &&

                UnityEngine.InputSystem.Keyboard.current.f9Key.wasPressedThisFrame)

            {

                return true;

            }

#endif

#if ENABLE_LEGACY_INPUT_MANAGER

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F9))

            {

                return true;

            }

#endif

            return false;

        }



        private void OnGUI()

        {

            if (!showDebugPanel) return;

            GUI.skin.window.fontSize = 14;

            debugRect = GUI.Window(9999, debugRect, DrawDebugWindow, "LevelSelect Debug (F9)");

        }



        private void DrawDebugWindow(int windowId)

        {

            GUILayout.Space(8);

            GUILayout.Label($"Current Act: {currentActIndex}");



            var mgr = DemHoiDenLong.Data.LevelProgressManager.Instance;

            var eco = DemHoiDenLong.Gameplay.EconomyManager.Instance;



            if (GUILayout.Button("★ Unlock All Levels", GUILayout.Height(36)))

            {

                if (mgr != null) mgr.UnlockAll();

                Initialize(currentActIndex, currentProvider, onBackCallback);

            }



            if (GUILayout.Button("↺ Reset All Progress", GUILayout.Height(36)))

            {

                if (mgr != null) mgr.ResetAll();

                Initialize(currentActIndex, currentProvider, onBackCallback);

            }



            debugUsedRevive = GUILayout.Toggle(debugUsedRevive, "Đã dùng hồi sinh");



            if (GUILayout.Button("✓ Win Current Level", GUILayout.Height(36)))

            {

                int currentLvl = (currentProvider != null) ? currentProvider.GetCurrentLevelId() : 1;

                if (mgr != null) mgr.ReportLevelResultForDebug(currentLvl, true, 0.9f, force: true, usedRevive: debugUsedRevive);

                Initialize(currentActIndex, currentProvider, onBackCallback);

            }



            if (GUILayout.Button("+ 500 Stars, + 50 Diamonds", GUILayout.Height(36)))

            {

                if (eco != null)

                {

                    eco.AddStar(500);

                    eco.AddDiamond(50);

                }

                else

                {

                    int s = PlayerPrefs.GetInt("Player_Stars", 1000) + 500;

                    int d = PlayerPrefs.GetInt("Player_Diamonds", 500) + 50;

                    PlayerPrefs.SetInt("Player_Stars", s);

                    PlayerPrefs.SetInt("Player_Diamonds", d);

                    PlayerPrefs.Save();

                }

                UpdateCurrencies();

            }



            GUILayout.Space(8);

            if (GUILayout.Button("Close (F9)", GUILayout.Height(30)))

            {

                showDebugPanel = false;

            }



            GUI.DragWindow();

        }

#endif

    }

}

