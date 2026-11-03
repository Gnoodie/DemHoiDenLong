using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Điều phối chuyển cảnh giữa màn Chọn Hồi (ActSelectView) và màn Chọn Màn (LevelDetailView),
    /// xử lý nút Back và phím Escape trên Android.
    /// </summary>
    public class LevelSelectFlow : MonoBehaviour
    {
        [Header("Views")]
        public GameObject actSelectView;
        public CanvasGroup actSelectCanvasGroup;
        public LevelDetailView levelDetailView;
        public ActSelectView actSelectViewController;

        [Header("Data Provider Configuration")]
        [SerializeField] private bool useMockData = false;
        public bool UseMockData { get => useMockData; set => useMockData = value; }
        public MockLevelDataProvider dataProvider;

        private Coroutine transitionCoroutine;

        private void Awake()
        {
            // Tắt ActSelectPreview nếu có trong scene để không ghi đè dữ liệu thật
            var preview = FindAnyObjectByType<ActSelectPreview>(FindObjectsInactive.Include);
            if (preview != null) preview.enabled = false;

            if (useMockData)
            {
                if (dataProvider == null)
                {
                    dataProvider = GetComponent<MockLevelDataProvider>();
                    if (dataProvider == null) dataProvider = gameObject.AddComponent<MockLevelDataProvider>();
                }
                LevelDataService.SetCustomProvider(dataProvider);
            }
            else
            {
                LevelDataService.SetCustomProvider(null);
            }
        }

        private System.Collections.IEnumerator Start()
        {
#if ENABLE_INPUT_SYSTEM
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es != null)
            {
                var module = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                if (module != null && (module.actionsAsset == null || module.point == null || module.point.action == null))
                {
                    Debug.LogWarning("[LevelSelectFlow] Tự động sửa InputSystemUIInputModule bằng AssignDefaultActions()...");
                    module.AssignDefaultActions();
                }
            }
#endif

            // Đảm bảo ban đầu ActSelectView bật, LevelDetailView tắt
            if (actSelectView != null)
            {
                actSelectView.SetActive(true);
                if (actSelectCanvasGroup == null)
                    actSelectCanvasGroup = actSelectView.GetComponent<CanvasGroup>();
                if (actSelectCanvasGroup != null)
                    actSelectCanvasGroup.alpha = 1f;

                if (actSelectViewController == null)
                    actSelectViewController = actSelectView.GetComponent<ActSelectView>() ?? actSelectView.AddComponent<ActSelectView>();

                actSelectViewController.Initialize(LevelDataService.Provider, OnActCardClicked, HandleBackNavigation);
            }

            if (levelDetailView != null)
            {
                levelDetailView.gameObject.SetActive(false);
                if (levelDetailView.canvasGroup != null)
                    levelDetailView.canvasGroup.alpha = 0f;
            }

            // Quét và trả bù phần thưởng kỷ lục còn nợ (nếu có)
            if (!useMockData && DemHoiDenLong.Data.LevelProgressManager.Instance != null)
            {
                var claim = DemHoiDenLong.Data.LevelProgressManager.Instance.ClaimPendingRewards();
                if (!claim.IsEmpty && (claim.totalStarsGranted > 0 || claim.totalDiamondsGranted > 0))
                {
                    string msg = claim.totalDiamondsGranted > 0
                        ? $"Đã nhận bù {claim.totalStarsGranted} Sao và {claim.totalDiamondsGranted} Kim Cương!"
                        : $"Đã nhận bù {claim.totalStarsGranted} Sao!";
                    if (actSelectViewController != null)
                        actSelectViewController.ShowToast(msg);
                    else if (levelDetailView != null)
                        levelDetailView.ShowToast(msg);
                }
            }

            // Kiểm tra mở thẳng Hồi theo yêu cầu chuyển cảnh
            if (LevelSelectContext.OpenActOnStart >= 0)
            {
                int actToOpen = LevelSelectContext.OpenActOnStart;
                LevelSelectContext.OpenActOnStart = -1;
                if (actToOpen <= 0) actToOpen = 1;
                TransitionToLevelDetail(actToOpen);
                yield break;
            }

            // Chờ 1 frame và refresh thẻ
            yield return null;
            if (actSelectViewController != null)
            {
                actSelectViewController.RefreshDisplay();
            }
        }

        private void OnActCardClicked(int actIndex)
        {
            Debug.Log($"[LevelSelectFlow] OnActCardClicked actIndex: {actIndex}");
            ILevelDataProvider provider = LevelDataService.Provider;
            if (provider != null)
            {
                ActStatus status = provider.GetActStatus(actIndex);
                if (status == ActStatus.Locked)
                {
                    string hint = provider.GetUnlockHint(actIndex);
                    if (string.IsNullOrEmpty(hint)) hint = $"Hoàn thành Hồi {actIndex - 1} để mở khóa!";
                    if (actSelectViewController != null) actSelectViewController.ShowToast(hint);
                    return;
                }
            }

            TransitionToLevelDetail(actIndex);
        }

        public void TransitionToLevelDetail(int actIndex)
        {
            Debug.Log($"[LevelSelectFlow] TransitionToLevelDetail -> Bắt đầu chuyển cảnh sang Hồi {actIndex}");
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(DoTransitionToLevelDetail(actIndex));
        }

        public void TransitionToActSelect()
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(DoTransitionToActSelect());
        }

        private IEnumerator DoTransitionToLevelDetail(int actIndex)
        {
            LevelSelectContext.SelectedActIndex = actIndex;

            // 1. Fade out ActSelectView (0.2s)
            float elapsed = 0f;
            float durationOut = 0.2f;
            while (elapsed < durationOut)
            {
                elapsed += Time.unscaledDeltaTime;
                if (actSelectCanvasGroup != null)
                    actSelectCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / durationOut);
                yield return null;
            }

            if (actSelectView != null) actSelectView.SetActive(false);

            // 2. Bật LevelDetailView và khởi tạo
            if (levelDetailView != null)
            {
                levelDetailView.gameObject.SetActive(true);
                levelDetailView.Initialize(actIndex, LevelDataService.Provider, TransitionToActSelect);

                if (levelDetailView.canvasGroup != null)
                    levelDetailView.canvasGroup.alpha = 0f;

                // 3. Fade in LevelDetailView (0.25s)
                elapsed = 0f;
                float durationIn = 0.25f;
                while (elapsed < durationIn)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (levelDetailView.canvasGroup != null)
                        levelDetailView.canvasGroup.alpha = Mathf.Clamp01(elapsed / durationIn);
                    yield return null;
                }

                if (levelDetailView.canvasGroup != null)
                    levelDetailView.canvasGroup.alpha = 1f;
            }

            transitionCoroutine = null;
        }

        private IEnumerator DoTransitionToActSelect()
        {
            // 1. Fade out LevelDetailView (0.2s)
            float elapsed = 0f;
            float durationOut = 0.2f;
            while (elapsed < durationOut)
            {
                elapsed += Time.unscaledDeltaTime;
                if (levelDetailView != null && levelDetailView.canvasGroup != null)
                    levelDetailView.canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / durationOut);
                yield return null;
            }

            if (levelDetailView != null) levelDetailView.gameObject.SetActive(false);

            // 2. Bật ActSelectView và làm mới dữ liệu
            if (actSelectView != null)
            {
                actSelectView.SetActive(true);
                if (actSelectViewController != null)
                    actSelectViewController.RefreshDisplay();

                if (actSelectCanvasGroup != null)
                    actSelectCanvasGroup.alpha = 0f;

                // 3. Fade in ActSelectView (0.25s)
                elapsed = 0f;
                float durationIn = 0.25f;
                while (elapsed < durationIn)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (actSelectCanvasGroup != null)
                        actSelectCanvasGroup.alpha = Mathf.Clamp01(elapsed / durationIn);
                    yield return null;
                }

                if (actSelectCanvasGroup != null)
                    actSelectCanvasGroup.alpha = 1f;
            }

            transitionCoroutine = null;
        }

        private void Update()
        {
            if (IsEscapePressed())
            {
                HandleBackNavigation();
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

        private void HandleBackNavigation()
        {
            // 1. Nếu modal đang mở -> đóng modal
            if (levelDetailView != null && levelDetailView.gameObject.activeSelf &&
                levelDetailView.detailModal != null && levelDetailView.detailModal.IsOpen)
            {
                levelDetailView.detailModal.Close();
                return;
            }

            // 2. Nếu đang ở LevelDetailView -> quay về ActSelectView
            if (levelDetailView != null && levelDetailView.gameObject.activeSelf)
            {
                TransitionToActSelect();
                return;
            }

            // 3. Nếu đang ở ActSelectView -> quay về MainMenu (Hangar)
            if (actSelectView != null && actSelectView.activeSelf)
            {
                DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.MainMenu, "LevelSelectFlow_Back");
            }
        }
    }
}
