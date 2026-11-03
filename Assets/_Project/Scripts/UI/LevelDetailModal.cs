using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Điều khiển popup chi tiết màn chơi (ModalOverlay): hiển thị nhân vật,
    /// 3 mốc quà, trạng thái nhận thưởng và nút Bắt đầu chơi.
    /// </summary>
    public class LevelDetailModal : MonoBehaviour
    {
        [Header("Modal & Backdrop")]
        public CanvasGroup modalCanvasGroup;
        public RectTransform popupCard;
        public Button backdropButton;
        public Image backdropImage;
        public Button closeButton;

        [Header("Header Texts")]
        public TMP_Text actSubheadText;
        public TMP_Text levelTitleText;

        [Header("Lan Character Card")]
        public Image lanAvatarImage;
        public TMP_Text lanNameText;
        public Image[] recordStarIcons; // 3 sao kỷ lục màn
        public LanData[] availableLans;

        [Header("Reward Rows")]
        public RewardRowUI[] rewardRows; // 3 hàng quà (mốc 1, 2, 3)

        [Header("Play Button")]
        public Button playButton;
        public RectTransform playButtonVisual;
        public Image playIconImage;
        public TMP_Text playButtonText;

        [Header("Objective Chip (Phase 3.5)")]
        public GameObject objectiveContainer;
        public TMP_Text objectiveText;

        [Header("Revive Notice (Phase 3.5)")]
        public TMP_Text reviveNoticeText;

        [Header("Assets for Rewards")]
        public Sprite starSprite;
        public Sprite diamondSprite;
        public Sprite lockSprite;
        public Sprite checkSprite;
        public Sprite playSprite;
        public Sprite pillSprite;

        // Bảng màu
        private static readonly Color ColorCyan = new Color(0.498f, 0.91f, 0.961f, 1f);      // #7FE8F5
        private static readonly Color ColorCream = new Color(0.957f, 0.945f, 0.902f, 1f);    // #F4F1E6
        private static readonly Color ColorGold = new Color(1f, 0.824f, 0.478f, 1f);         // #FFD27A
        private static readonly Color ColorGrey = new Color(0.557f, 0.596f, 0.737f, 1f);     // #8E98BC
        private static readonly Color ColorDarkGrey = new Color(0.29f, 0.333f, 0.502f, 1f);  // #4A5580
        private static readonly Color ColorGreen = new Color(0.18f, 0.62f, 0.478f, 1f);      // #2E9E7A
        private static readonly Color ColorDarkBrown = new Color(0.227f, 0.133f, 0f, 1f);    // #3A2200

        private LevelNodeData currentLevelData;
        private Action<int> onPlayCallback;
        private Coroutine transitionCoroutine;

        public bool IsOpen => gameObject.activeSelf && (modalCanvasGroup == null || modalCanvasGroup.alpha > 0.05f);

        private void Awake()
        {
            if (modalCanvasGroup == null) modalCanvasGroup = GetComponent<CanvasGroup>();
            if (backdropButton != null) backdropButton.onClick.AddListener(Close);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);

            if (rewardRows == null || rewardRows.Length < 3 || rewardRows[0] == null)
            {
                rewardRows = GetComponentsInChildren<RewardRowUI>(true);
            }

            // Đảm bảo GlowOutline trong PopupCard không bị VerticalLayoutGroup ép thành hàng
            if (popupCard != null)
            {
                Transform glow = popupCard.Find("GlowOutline");
                if (glow != null)
                {
                    LayoutElement le = glow.GetComponent<LayoutElement>();
                    if (le == null) le = glow.gameObject.AddComponent<LayoutElement>();
                    le.ignoreLayout = true;
                }
            }

            // Đảm bảo cụm nút BẮT ĐẦU CHƠI: (icon + chữ) căn giữa nút với spacing 14, chữ 1 dòng
            if (playButton != null)
            {
                Transform content = playButton.transform.Find("Content");
                if (content != null)
                {
                    RectTransform rtContent = content as RectTransform;
                    if (rtContent != null)
                    {
                        rtContent.anchorMin = new Vector2(0.5f, 0.5f);
                        rtContent.anchorMax = new Vector2(0.5f, 0.5f);
                        rtContent.pivot = new Vector2(0.5f, 0.5f);
                        rtContent.anchoredPosition = Vector2.zero;
                    }
                    var csf = content.GetComponent<ContentSizeFitter>();
                    if (csf == null) csf = content.gameObject.AddComponent<ContentSizeFitter>();
                    csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var hlg = content.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.childAlignment = TextAnchor.MiddleCenter;
                        hlg.spacing = 14f;
                        hlg.childControlWidth = true;
                        hlg.childControlHeight = true;
                        hlg.childForceExpandWidth = false;
                        hlg.childForceExpandHeight = false;
                    }
                }
                if (playButtonText != null)
                {
                    playButtonText.alignment = TextAlignmentOptions.Center;
                    playButtonText.textWrappingMode = TextWrappingModes.NoWrap;
                    playButtonText.overflowMode = TextOverflowModes.Ellipsis;
                    LayoutElement leTxt = playButtonText.GetComponent<LayoutElement>();
                    if (leTxt != null)
                    {
                        leTxt.flexibleWidth = 0f;
                        leTxt.preferredHeight = 44f;
                    }
                }
                if (playIconImage != null)
                {
                    LayoutElement leIcon = playIconImage.GetComponent<LayoutElement>();
                    if (leIcon != null)
                    {
                        leIcon.preferredWidth = 44f;
                        leIcon.preferredHeight = 44f;
                        leIcon.minWidth = 44f;
                        leIcon.minHeight = 44f;
                        leIcon.flexibleWidth = 0f;
                    }
                }
            }
        }

        private void Update()
        {
            if (IsOpen && IsEscapePressed())
            {
                Close();
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

        public void Open(string actName, LevelNodeData data, Action<int> onPlay)
        {
            Debug.Log($"[LevelDetailModal] Open called for act: '{actName}', level: '{data.levelName}', id: {data.levelId}");
            currentLevelData = data;
            onPlayCallback = onPlay;

            // Đưa Modal lên trên cùng trong phân cấp để che cả TopBar, HeaderBar và các node khác
            transform.SetAsLastSibling();

            if (backdropImage != null)
            {
                backdropImage.color = new Color(0f, 0f, 0f, 0.84f);
            }

            if (modalCanvasGroup != null)
            {
                modalCanvasGroup.interactable = true;
                modalCanvasGroup.blocksRaycasts = true;
            }

            // Header
            if (actSubheadText != null)
            {
                actSubheadText.text = actName.ToUpper();
                actSubheadText.color = ColorCyan;
            }

            // Định dạng tên màn chuẩn: MÀN {hồi}-{thứ tự} hoặc MÀN {hồi}-{thứ tự} · {TÊN BOSS}
            int actIndex = (data.actIndex >= 0) ? (data.actIndex + 1) : ((data.levelId > 0) ? ((data.levelId - 1) / 5 + 1) : 1);
            int nodeIndex = (data.indexInAct > 0) ? data.indexInAct : ((data.levelId > 0) ? ((data.levelId - 1) % 5 + 1) : 1);
            string formattedTitle;
            if (data.isBoss)
            {
                string bossName = !string.IsNullOrEmpty(data.bossName) ? data.bossName : "TRÙM CUỐI";
                if (string.IsNullOrEmpty(data.bossName) && !string.IsNullOrEmpty(data.levelName))
                {
                    int dotIdx = data.levelName.IndexOf('·');
                    if (dotIdx >= 0 && dotIdx + 1 < data.levelName.Length)
                        bossName = data.levelName.Substring(dotIdx + 1).Trim();
                    else if (data.levelName.Contains("-"))
                    {
                        int dashIdx = data.levelName.IndexOf('-');
                        bossName = data.levelName.Substring(dashIdx + 1).Trim();
                    }
                    else
                        bossName = data.levelName.Trim();
                }
                formattedTitle = $"MÀN {actIndex}-{nodeIndex} · {bossName}".ToUpper();
            }
            else
            {
                formattedTitle = $"MÀN {actIndex}-{nodeIndex}".ToUpper();
            }

            if (levelTitleText != null)
            {
                levelTitleText.text = formattedTitle;
                levelTitleText.color = ColorCream;
                levelTitleText.enableAutoSizing = true;
                levelTitleText.fontSizeMin = 30f;
                levelTitleText.fontSizeMax = 44f;
                levelTitleText.maxVisibleLines = 2;
                levelTitleText.textWrappingMode = TextWrappingModes.Normal;
                levelTitleText.overflowMode = TextOverflowModes.Ellipsis;
            }

            // Nạp nhân vật Lân đang chọn
            SetupLanCard(data.stars);

            // Nạp chip Mục tiêu màn chơi (Phase 3.5)
            SetupObjective(data);

            // Nạp 3 hàng phần thưởng
            SetupRewards(data);

            // Cập nhật nút Play icon
            if (playIconImage != null && playSprite != null)
            {
                playIconImage.sprite = playSprite;
                playIconImage.color = ColorDarkBrown;
            }
            if (playButtonText != null)
            {
                playButtonText.text = "BẮT ĐẦU CHƠI";
                playButtonText.color = ColorDarkBrown;
            }

            // Hoạt ảnh mở
            gameObject.SetActive(true);
            if (Application.isPlaying)
            {
                if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
                transitionCoroutine = StartCoroutine(AnimateOpen());
            }
            else
            {
                if (modalCanvasGroup != null) modalCanvasGroup.alpha = 1f;
                if (popupCard != null) popupCard.localScale = Vector3.one;
            }
        }

        public void Close()
        {
            if (!gameObject.activeSelf) return;
            DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIClick();
            if (Application.isPlaying)
            {
                if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
                transitionCoroutine = StartCoroutine(AnimateClose());
            }
            else
            {
                if (modalCanvasGroup != null) modalCanvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        }

        private void SetupLanCard(int currentRecordStars)
        {
            LanData lan = LanCatalog.GetPlayableLan();
            if (lan == null && availableLans != null && availableLans.Length > 0)
            {
                lan = availableLans[0];
            }

            if (lan != null)
            {
                if (lanNameText != null) lanNameText.text = lan.lanName;
                if (lanAvatarImage != null && lan.icon != null) lanAvatarImage.sprite = lan.icon;
            }
            else
            {
                if (lanNameText != null) lanNameText.text = "Kim Lân";
            }

            // Kỷ lục sao đạt được
            if (recordStarIcons != null)
            {
                for (int i = 0; i < recordStarIcons.Length; i++)
                {
                    if (recordStarIcons[i] != null)
                    {
                        recordStarIcons[i].color = (i < currentRecordStars) ? ColorGold : ColorDarkGrey;
                    }
                }
            }
        }

        private void SetupObjective(LevelNodeData data)
        {
            if (objectiveText == null) return;

            WinCondition cond = data.winCondition;
            int killCount = data.targetKillCount;
            float survive = data.surviveSeconds;
            string bName = data.bossName;

            LevelData ld = null;
            if (LevelProgressManager.Instance != null && LevelProgressManager.Instance.Database != null)
            {
                ld = LevelProgressManager.Instance.Database.GetLevel(data.levelId);
            }

            if (ld != null)
            {
                cond = ld.winCondition;
                killCount = ld.targetKillCount > 0 ? ld.targetKillCount : ld.enemyCount;
                survive = ld.surviveSeconds;
                if (!string.IsNullOrEmpty(ld.bossName)) bName = ld.bossName;
            }

            // Căn chỉnh kích thước và font auto-sizing chống gãy 3 dòng (Giai đoạn 5A.6)
            RectTransform textRt = objectiveText.GetComponent<RectTransform>();
            if (textRt != null)
            {
                textRt.sizeDelta = new Vector2(560f, textRt.sizeDelta.y);
            }
            if (objectiveContainer != null)
            {
                RectTransform contRt = objectiveContainer.GetComponent<RectTransform>();
                if (contRt != null)
                {
                    contRt.sizeDelta = new Vector2(Mathf.Max(contRt.sizeDelta.x, 560f), contRt.sizeDelta.y);
                }
            }

            objectiveText.enableAutoSizing = true;
            objectiveText.fontSizeMin = 20f;
            objectiveText.fontSizeMax = 26f;
            objectiveText.alignment = TextAlignmentOptions.Center;

            string objectiveStr;
            if (data.isBoss || cond == WinCondition.BossKill)
            {
                string displayBoss = !string.IsNullOrEmpty(bName) ? bName : "Trùm";
                // Tối đa 2 dòng: nếu tên dài thì dòng 1 'Mục tiêu: Hạ trùm', dòng 2 tên Boss; nếu ngắn thì 1 dòng
                if (displayBoss.Length > 12)
                {
                    objectiveStr = $"Mục tiêu: Hạ trùm\n{displayBoss}";
                }
                else
                {
                    objectiveStr = $"Mục tiêu: Hạ {displayBoss}";
                }
            }
            else if (cond == WinCondition.SurviveTime)
            {
                int sec = Mathf.RoundToInt(survive > 0f ? survive : (45f + 3f * data.levelId));
                objectiveStr = $"Mục tiêu: Sống sót {sec} giây";
            }
            else // KillCount
            {
                int count = killCount > 0 ? killCount : 25;
                objectiveStr = $"Mục tiêu: Hạ {count} địch";
            }

            objectiveText.text = objectiveStr;
        }

        private void SetupRewards(LevelNodeData data)
        {
            if (rewardRows == null || rewardRows.Length < 3 || rewardRows[0] == null)
            {
                rewardRows = GetComponentsInChildren<RewardRowUI>(true);
            }
            if (rewardRows == null || rewardRows.Length < 3 || rewardRows[0] == null)
            {
                var list = new System.Collections.Generic.List<RewardRowUI>();
                for (int m = 1; m <= 3; m++)
                {
                    Transform t = null;
                    var allTransforms = GetComponentsInChildren<Transform>(true);
                    foreach (var tr in allTransforms)
                    {
                        if (tr.name == $"RewardRow_{m}") { t = tr; break; }
                    }
                    if (t != null)
                    {
                        var rowComp = t.GetComponent<RewardRowUI>();
                        if (rowComp == null) rowComp = t.gameObject.AddComponent<RewardRowUI>();
                        list.Add(rowComp);
                    }
                }
                if (list.Count == 3)
                {
                    rewardRows = list.ToArray();
                }
            }
            if (rewardRows == null || rewardRows.Length < 3) return;

            for (int i = 0; i < 3; i++)
            {
                int milestone = i + 1;
                RewardRowUI row = rewardRows[i];
                if (row == null) continue;

                int starAmount = LevelRewards.GetStarReward(data.levelId, milestone);
                int diamondAmount = LevelRewards.GetDiamondReward(data.levelId, milestone);
                string condition = LevelRewards.GetConditionText(milestone);

                MilestoneState mState;
                if (data.milestoneStates != null && i < data.milestoneStates.Length)
                {
                    mState = data.milestoneStates[i];
                }
                else if (data.claimed != null && i < data.claimed.Length && data.claimed[i])
                {
                    mState = MilestoneState.Claimed;
                }
                else if (milestone <= data.stars)
                {
                    mState = MilestoneState.Pending;
                }
                else
                {
                    mState = MilestoneState.Unachieved;
                }

                row.Setup(milestone, condition, starAmount, diamondAmount, mState, starSprite, diamondSprite, lockSprite, checkSprite, pillSprite);
            }
        }

        private void OnPlayClicked()
        {
            DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIClick();
            LanData playableLan = LanCatalog.GetPlayableLan();
            string lanIdStr = playableLan != null ? playableLan.lanId : "1";
            int lanIdInt = 1;
            int.TryParse(lanIdStr, out lanIdInt);

            string lanName = lanNameText != null ? lanNameText.text : (playableLan != null ? playableLan.lanName : "Kim Lân");
            Debug.Log($"Play level {currentLevelData.levelId} with Lan {lanName} (id={lanIdStr})");

            LevelSelectContext.SelectedLevelId = currentLevelData.levelId;
            LevelSelectContext.LastPlayedLevelId = currentLevelData.levelId;
            LevelSelectContext.SelectedLanId = lanIdInt;

            // Khởi tạo LevelSession đồng bộ trực tiếp với Lân đang chọn
            DemHoiDenLong.Gameplay.LevelSession.Initialize(currentLevelData.levelId, lanIdStr);

            // Hoạt ảnh nhấn nhả nút
            StartCoroutine(PlayButtonPressRoutine(() =>
            {
                if (onPlayCallback != null)
                {
                    onPlayCallback.Invoke(currentLevelData.levelId);
                }
                else
                {
                    Time.timeScale = 1f;
                    DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.Gameplay, "LevelDetailModal_PlayButton");
                }
            }));
        }

        private IEnumerator PlayButtonPressRoutine(Action onComplete)
        {
            if (playButtonVisual != null)
                playButtonVisual.localScale = new Vector3(0.97f, 0.97f, 1f);

            yield return new WaitForSecondsRealtime(0.1f);

            if (playButtonVisual != null)
                playButtonVisual.localScale = Vector3.one;

            onComplete?.Invoke();
        }

        private IEnumerator AnimateOpen()
        {
            if (modalCanvasGroup != null) modalCanvasGroup.alpha = 0f;
            if (popupCard != null) popupCard.localScale = new Vector3(0.92f, 0.92f, 1f);

            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3f); // Ease Out

                if (modalCanvasGroup != null) modalCanvasGroup.alpha = ease;
                if (popupCard != null)
                {
                    float s = Mathf.Lerp(0.92f, 1f, ease);
                    popupCard.localScale = new Vector3(s, s, 1f);
                }
                yield return null;
            }

            if (modalCanvasGroup != null) modalCanvasGroup.alpha = 1f;
            if (popupCard != null) popupCard.localScale = Vector3.one;
            transitionCoroutine = null;
        }

        private IEnumerator AnimateClose()
        {
            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (modalCanvasGroup != null) modalCanvasGroup.alpha = 1f - t;
                if (popupCard != null)
                {
                    float s = Mathf.Lerp(1f, 0.92f, t);
                    popupCard.localScale = new Vector3(s, s, 1f);
                }
                yield return null;
            }

            if (modalCanvasGroup != null) modalCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            transitionCoroutine = null;
        }

        // ================= CÔNG CỤ TEST NHANH 5 TRẠNG THÁI =================

        [ContextMenu("Test/Demo: Hồi 2 - Màn 2-3 (Màn 8: 0 sao, nhãn MỚI, CHƯA ĐẠT)")]
        public void Test_Act2_Node3_Demo()
        {
            OpenTestMock("HỒI 2 · PHỐ CỔ ĐÈN LỒNG", 8, "Màn 2-3", 0, new bool[] { false, false, false });
        }

        [ContextMenu("Test/Demo: Hồi 2 - Màn 2-2 (Màn 7: 2 sao, 2 ĐÃ NHẬN, 1 CHƯA ĐẠT)")]
        public void Test_Act2_Node2_Demo()
        {
            OpenTestMock("HỒI 2 · PHỐ CỔ ĐÈN LỒNG", 7, "Màn 2-2", 2, new bool[] { true, true, false });
        }

        [ContextMenu("Test/1. Màn thường 0 sao")]
        public void Test_Normal_0Star()
        {
            OpenTestMock("HỒI 1 · LÀNG HOA TRĂNG KHUYẾT", 1, "Màn 1-1", 0, new bool[] { false, false, false });
        }

        [ContextMenu("Test/2. Màn thường 2 sao")]
        public void Test_Normal_2Star()
        {
            OpenTestMock("HỒI 1 · LÀNG HOA TRĂNG KHUYẾT", 2, "Màn 1-2", 2, new bool[] { true, true, false });
        }

        [ContextMenu("Test/3. Màn thường 3 sao")]
        public void Test_Normal_3Star()
        {
            OpenTestMock("HỒI 1 · LÀNG HOA TRĂNG KHUYẾT", 3, "Màn 1-3", 3, new bool[] { true, true, true });
        }

        [ContextMenu("Test/4. Màn Boss 0 sao")]
        public void Test_Boss_0Star()
        {
            OpenTestMock("HỒI 1 · LÀNG HOA TRĂNG KHUYẾT", 5, "MÀN 1-5 · ĐÈN TÀN TRÙM LÀNG", 0, new bool[] { false, false, false });
        }

        [ContextMenu("Test/5. Màn Boss 3 sao")]
        public void Test_Boss_3Star()
        {
            OpenTestMock("HỒI 1 · LÀNG HOA TRĂNG KHUYẾT", 5, "MÀN 1-5 · ĐÈN TÀN TRÙM LÀNG", 3, new bool[] { true, true, true });
        }

        public void OpenTestMock(string actName, int levelId, string levelName, int stars, bool[] claimed)
        {
            LevelNodeData mockData = new LevelNodeData
            {
                levelId = levelId,
                levelName = levelName,
                stars = stars,
                claimed = claimed,
                state = NodeState.Unlocked,
                anchorPos = Vector2.zero,
                isBoss = (levelId % 5 == 0)
            };

            Open(actName, mockData, (id) => Debug.Log($"[Test] Clicked Play for Level {id}"));

            if (!Application.isPlaying)
            {
                if (modalCanvasGroup != null) modalCanvasGroup.alpha = 1f;
                if (popupCard != null) popupCard.localScale = Vector3.one;
            }
        }
    }
}
