using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Audio;

namespace DemHoiDenLong.UI
{
    public class SettingsUI : MonoBehaviour
    {
        public static SettingsUI Instance { get; private set; }

        private const string PREV_BGM_KEY = "Settings_Prev_BGM_Vol";
        private const string PREV_SFX_KEY = "Settings_Prev_SFX_Vol";

        [Header("UI References")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button exitButton;

        private float previousTimeScale = 1f;
        private GameObject confirmPopup;
        private GameObject dimBackdrop;

        // Visual controls & state
        private Image bgmIconImage;
        private Image sfxIconImage;
        private Button bgmIconButton;
        private Button sfxIconButton;
        private float prevBGMVolume = 0.8f;
        private float prevSFXVolume = 0.8f;

        private Coroutine scaleCoroutine;
        private bool isClosing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            prevBGMVolume = PlayerPrefs.GetFloat(PREV_BGM_KEY, 0.8f);
            prevSFXVolume = PlayerPrefs.GetFloat(PREV_SFX_KEY, 0.8f);

            EnsureControlsAndLayout();
        }

        private void OnEnable()
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isClosing = false;

            var gm = DemHoiDenLong.Gameplay.GameManager.Instance ?? Object.FindAnyObjectByType<DemHoiDenLong.Gameplay.GameManager>();
            if (exitButton != null)
            {
                var txt = exitButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                if (txt != null)
                {
                    txt.text = (gm != null) ? "VỀ CHỌN MÀN" : "THOÁT GAME";
                    txt.enableWordWrapping = false;
                    txt.fontSize = 28f;
                }
            }

            if (confirmPopup != null) confirmPopup.SetActive(false);

            if (AudioManager.Instance != null)
            {
                if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.MasterBGMVolume;
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.MasterSFXVolume;
            }

            EnsureDimBackdrop();
            if (dimBackdrop != null)
            {
                dimBackdrop.transform.SetParent(transform.parent, false);
                dimBackdrop.transform.SetSiblingIndex(Mathf.Max(0, transform.GetSiblingIndex()));
                transform.SetAsLastSibling();
                dimBackdrop.SetActive(true);
            }

            EnsureControlsAndLayout();
            UpdateMuteVisuals();

            PlayPopInAnimation();
        }

        private void OnDisable()
        {
            Time.timeScale = previousTimeScale;
            if (dimBackdrop != null) dimBackdrop.SetActive(false);
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
            transform.localScale = Vector3.one;
            isClosing = false;
        }

        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                if (bgmSlider != null)
                {
                    bgmSlider.value = AudioManager.Instance.MasterBGMVolume;
                    bgmSlider.onValueChanged.RemoveAllListeners();
                    bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
                }

                if (sfxSlider != null)
                {
                    sfxSlider.value = AudioManager.Instance.MasterSFXVolume;
                    sfxSlider.onValueChanged.RemoveAllListeners();
                    sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseSettings);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ExitGame);
            }

            EnsureControlsAndLayout();
            UpdateMuteVisuals();
        }

        private void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }
            if (value > 0.05f)
            {
                prevBGMVolume = value;
                PlayerPrefs.SetFloat(PREV_BGM_KEY, prevBGMVolume);
            }
            UpdateMuteVisuals();
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
            if (value > 0.05f)
            {
                prevSFXVolume = value;
                PlayerPrefs.SetFloat(PREV_SFX_KEY, prevSFXVolume);
            }
            UpdateMuteVisuals();
        }

        public void CloseSettings()
        {
            if (confirmPopup != null && confirmPopup.activeSelf)
            {
                confirmPopup.SetActive(false);
                return;
            }

            if (isClosing) return;
            isClosing = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }

            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleRoutine(transform.localScale, Vector3.one * 0.85f, 0.12f, () =>
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.one;
                isClosing = false;
            }));
        }

        private void ExitGame()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }

            var gm = DemHoiDenLong.Gameplay.GameManager.Instance ?? Object.FindAnyObjectByType<DemHoiDenLong.Gameplay.GameManager>();
            if (gm != null)
            {
                ShowConfirmGiveUpPopup(gm);
            }
            else
            {
                Time.timeScale = 1f;
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        #region Animations & Backdrop

        private void PlayPopInAnimation()
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            transform.localScale = Vector3.one * 0.85f;
            scaleCoroutine = StartCoroutine(ScaleRoutine(Vector3.one * 0.85f, Vector3.one, 0.15f, null));
        }

        private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                transform.localScale = Vector3.LerpUnclamped(from, to, smoothT);
                yield return null;
            }
            transform.localScale = to;
            scaleCoroutine = null;
            onComplete?.Invoke();
        }

        private void EnsureDimBackdrop()
        {
            if (dimBackdrop != null) return;
            Transform parentTr = transform.parent;
            if (parentTr == null) return;

            Transform existingBackdrop = parentTr.Find("Settings_DimBackdrop");
            if (existingBackdrop != null)
            {
                dimBackdrop = existingBackdrop.gameObject;
            }
            else
            {
                dimBackdrop = new GameObject("Settings_DimBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                dimBackdrop.transform.SetParent(parentTr, false);

                RectTransform rt = dimBackdrop.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);

                Image img = dimBackdrop.GetComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.72f);
                img.raycastTarget = true;

                Button btn = dimBackdrop.GetComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(CloseSettings);
            }
        }

        #endregion

        #region Additional Controls & Layout

        private void EnsureControlsAndLayout()
        {
            // Remove Shake_Row if it exists
            Transform shakeTr = transform.Find("Shake_Row");
            if (shakeTr != null)
            {
                Destroy(shakeTr.gameObject);
            }

            // Setup Icon mute buttons
            SetupIconButtons();

            // Normalize slider positions and identical thickness
            NormalizeSliders();

            // Ensure Version text
            EnsureVersionText();
        }

        private void SetupIconButtons()
        {
            // BGM Icon
            Transform bgmIconTr = transform.Find("BGM_Row/Icon");
            if (bgmIconTr != null)
            {
                bgmIconImage = bgmIconTr.GetComponent<Image>();
                if (bgmIconImage != null)
                {
                    bgmIconImage.raycastTarget = true;
                    bgmIconImage.preserveAspect = true;
                }
                bgmIconButton = bgmIconTr.GetComponent<Button>();
                if (bgmIconButton == null) bgmIconButton = bgmIconTr.gameObject.AddComponent<Button>();
                bgmIconButton.transition = Selectable.Transition.None;
                var nav = bgmIconButton.navigation;
                nav.mode = Navigation.Mode.None;
                bgmIconButton.navigation = nav;

                bgmIconButton.onClick.RemoveAllListeners();
                bgmIconButton.onClick.AddListener(ToggleBGMMute);
            }

            // SFX Icon
            Transform sfxIconTr = transform.Find("SFX_Row/Icon");
            if (sfxIconTr != null)
            {
                sfxIconImage = sfxIconTr.GetComponent<Image>();
                if (sfxIconImage != null)
                {
                    sfxIconImage.raycastTarget = true;
                    sfxIconImage.preserveAspect = true;
                }
                sfxIconButton = sfxIconTr.GetComponent<Button>();
                if (sfxIconButton == null) sfxIconButton = sfxIconTr.gameObject.AddComponent<Button>();
                sfxIconButton.transition = Selectable.Transition.None;
                var nav = sfxIconButton.navigation;
                nav.mode = Navigation.Mode.None;
                sfxIconButton.navigation = nav;

                sfxIconButton.onClick.RemoveAllListeners();
                sfxIconButton.onClick.AddListener(ToggleSFXMute);
            }
        }

        private void ToggleBGMMute()
        {
            if (bgmSlider == null) return;

            if (bgmSlider.value > 0.01f)
            {
                // Mute
                prevBGMVolume = bgmSlider.value;
                PlayerPrefs.SetFloat(PREV_BGM_KEY, prevBGMVolume);
                bgmSlider.value = 0f;
                if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(0f);
            }
            else
            {
                // Unmute
                float targetVol = prevBGMVolume > 0.05f ? prevBGMVolume : PlayerPrefs.GetFloat(PREV_BGM_KEY, 0.8f);
                if (targetVol <= 0.05f) targetVol = 0.8f;
                bgmSlider.value = targetVol;
                if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(targetVol);
            }

            UpdateMuteVisuals();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
        }

        private void ToggleSFXMute()
        {
            if (sfxSlider == null) return;

            if (sfxSlider.value > 0.01f)
            {
                // Mute
                prevSFXVolume = sfxSlider.value;
                PlayerPrefs.SetFloat(PREV_SFX_KEY, prevSFXVolume);
                sfxSlider.value = 0f;
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(0f);
            }
            else
            {
                // Unmute
                float targetVol = prevSFXVolume > 0.05f ? prevSFXVolume : PlayerPrefs.GetFloat(PREV_SFX_KEY, 0.8f);
                if (targetVol <= 0.05f) targetVol = 0.8f;
                sfxSlider.value = targetVol;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetSFXVolume(targetVol);
                    AudioManager.Instance.PlayUIClick();
                }
            }

            UpdateMuteVisuals();
        }

        private void UpdateMuteVisuals()
        {
            if (bgmIconImage != null && bgmSlider != null)
            {
                bool isMuted = bgmSlider.value <= 0.01f;
                bgmIconImage.color = isMuted ? new Color(1f, 1f, 1f, 0.38f) : Color.white;
            }

            if (sfxIconImage != null && sfxSlider != null)
            {
                bool isMuted = sfxSlider.value <= 0.01f;
                sfxIconImage.color = isMuted ? new Color(1f, 1f, 1f, 0.38f) : Color.white;
            }
        }

        private void NormalizeSliders()
        {
            // Position BGM_Row and SFX_Row centered and evenly spaced
            Transform bgmTr = transform.Find("BGM_Row");
            if (bgmTr != null)
            {
                var rt = bgmTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 85f);
                rt.sizeDelta = new Vector2(700f, 120f);
                rt.localScale = Vector3.one;

                Transform iconTr = bgmTr.Find("Icon");
                if (iconTr != null)
                {
                    RectTransform iRt = iconTr.GetComponent<RectTransform>();
                    iRt.anchoredPosition = new Vector2(-260f, 0f);
                    iRt.sizeDelta = new Vector2(100f, 100f);
                }

                NormalizeSingleSlider(bgmTr.GetComponentInChildren<Slider>(true));
            }

            Transform sfxTr = transform.Find("SFX_Row");
            if (sfxTr != null)
            {
                var rt = sfxTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -85f);
                rt.sizeDelta = new Vector2(700f, 120f);
                rt.localScale = Vector3.one;

                Transform iconTr = sfxTr.Find("Icon");
                if (iconTr != null)
                {
                    RectTransform iRt = iconTr.GetComponent<RectTransform>();
                    iRt.anchoredPosition = new Vector2(-260f, 0f);
                    iRt.sizeDelta = new Vector2(100f, 100f);
                }

                NormalizeSingleSlider(sfxTr.GetComponentInChildren<Slider>(true));
            }

            Transform exitTr = transform.Find("ExitButton");
            if (exitTr != null)
            {
                var rt = exitTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 115f);

                var gm = DemHoiDenLong.Gameplay.GameManager.Instance ?? Object.FindAnyObjectByType<DemHoiDenLong.Gameplay.GameManager>();
                if (gm != null)
                {
                    rt.sizeDelta = new Vector2(560f, 110f); // PreferredWidth 560, cao 110, vừa MỘT dòng
                }
                else
                {
                    rt.sizeDelta = new Vector2(480f, 100f);
                }

                var txt = exitTr.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                if (txt != null)
                {
                    txt.enableWordWrapping = false;
                    txt.fontSize = 32f;
                }
            }
        }

        private void NormalizeSingleSlider(Slider sl)
        {
            if (sl == null) return;

            sl.minValue = 0f;
            sl.maxValue = 1f;
            sl.wholeNumbers = false;

            RectTransform sRt = sl.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.pivot = new Vector2(0.5f, 0.5f);
            sRt.anchoredPosition = new Vector2(65f, 0f);
            sRt.sizeDelta = new Vector2(440f, 40f);
            sRt.localScale = Vector3.one;

            // Background: stretch perfectly to slider height
            Transform bgTr = sl.transform.Find("Background");
            if (bgTr != null)
            {
                RectTransform bgRt = bgTr.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                bgRt.sizeDelta = Vector2.zero;
                bgRt.localScale = Vector3.one;

                Image bgImg = bgTr.GetComponent<Image>();
                if (bgImg != null) bgImg.type = Image.Type.Sliced;
            }

            // Fill Area: stretch perfectly
            Transform faTr = sl.transform.Find("Fill Area");
            if (faTr != null)
            {
                RectTransform faRt = faTr.GetComponent<RectTransform>();
                faRt.anchorMin = Vector2.zero;
                faRt.anchorMax = Vector2.one;
                faRt.offsetMin = Vector2.zero;
                faRt.offsetMax = Vector2.zero;
                faRt.sizeDelta = Vector2.zero;
                faRt.localScale = Vector3.one;

                Transform fillTr = faTr.Find("Fill");
                if (fillTr != null)
                {
                    Image fImg = fillTr.GetComponent<Image>();
                    if (fImg != null) fImg.type = Image.Type.Sliced;
                }
            }

            // Handle Slide Area & Handle: ensure perfect 1:1 circular/square handle without stretch distortion
            Transform hsaTr = sl.transform.Find("Handle Slide Area");
            if (hsaTr != null)
            {
                RectTransform hsaRt = hsaTr.GetComponent<RectTransform>();
                hsaRt.anchorMin = Vector2.zero;
                hsaRt.anchorMax = Vector2.one;
                hsaRt.offsetMin = new Vector2(20f, 0f);
                hsaRt.offsetMax = new Vector2(-20f, 0f);
                hsaRt.sizeDelta = new Vector2(-40f, 0f);
                hsaRt.localScale = Vector3.one;

                Transform handleTr = hsaTr.Find("Handle");
                if (handleTr != null)
                {
                    RectTransform hRt = handleTr.GetComponent<RectTransform>();
                    hRt.pivot = new Vector2(0.5f, 0.5f);
                    hRt.localScale = Vector3.one;
                    hRt.sizeDelta = new Vector2(80f, 40f);

                    Image hImg = handleTr.GetComponent<Image>();
                    if (hImg != null)
                    {
                        hImg.preserveAspect = true; // Key protection against squishing!
                    }
                }
            }
        }

        private void EnsureVersionText()
        {
            Transform verTr = transform.Find("VersionText");
            if (verTr == null)
            {
                GameObject verObj = new GameObject("VersionText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                verObj.transform.SetParent(transform, false);
                RectTransform rt = verObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0, 36);
                rt.sizeDelta = new Vector2(500, 32);

                var txt = verObj.GetComponent<TMPro.TextMeshProUGUI>();
                txt.text = "v1.0.0 · Đêm Hội Đèn Lồng";
                txt.fontSize = 20;
                txt.fontStyle = TMPro.FontStyles.Normal;
                txt.color = new Color(0.65f, 0.72f, 0.82f, 0.85f);
                txt.alignment = TMPro.TextAlignmentOptions.Center;
            }
            else
            {
                RectTransform rt = verTr.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 36);
            }
        }

        #endregion

        #region Confirm Give Up Modal

        private void ShowConfirmGiveUpPopup(DemHoiDenLong.Gameplay.GameManager gm)
        {
            if (confirmPopup == null)
            {
                confirmPopup = CreateConfirmGiveUpPopup(gm);
            }
            confirmPopup.SetActive(true);
        }

        private GameObject CreateConfirmGiveUpPopup(DemHoiDenLong.Gameplay.GameManager gm)
        {
            GameObject popup = new GameObject("ConfirmGiveUpModal", typeof(RectTransform));
            popup.transform.SetParent(transform, false);

            RectTransform rt = popup.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            // Dim background overlay
            GameObject overlay = new GameObject("DimOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(popup.transform, false);
            RectTransform ovRt = overlay.GetComponent<RectTransform>();
            ovRt.anchorMin = Vector2.zero;
            ovRt.anchorMax = Vector2.one;
            ovRt.sizeDelta = Vector2.zero;
            Image ovImg = overlay.GetComponent<Image>();
            ovImg.color = new Color(0f, 0f, 0f, 0.6f);
            ovImg.raycastTarget = true;

            // Dialog Panel
            GameObject panel = new GameObject("DialogPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(popup.transform, false);
            RectTransform pRt = panel.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.5f, 0.5f);
            pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.pivot = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(620f, 330f);
            pRt.anchoredPosition = Vector2.zero;

            Image pImg = panel.GetComponent<Image>();
            Sprite roundSpr = Resources.Load<Sprite>("UI/ui_round_36") ?? Resources.Load<Sprite>("UI/ui_pill");
            if (roundSpr != null)
            {
                pImg.sprite = roundSpr;
                pImg.type = Image.Type.Sliced;
            }
            pImg.color = new Color(22f / 255f, 34f / 255f, 74f / 255f, 0.98f);
            pImg.raycastTarget = true;

            // Title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 1f);
            tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(440f, 45f);
            tRt.anchoredPosition = new Vector2(0f, -22f);
            var tText = titleObj.GetComponent<TMPro.TextMeshProUGUI>();
            tText.text = "BỎ CUỘC?";
            tText.fontStyle = TMPro.FontStyles.Bold;
            tText.fontSize = 32f;
            tText.alignment = TMPro.TextAlignmentOptions.Center;
            tText.color = new Color(1f, 0.82f, 0.2f, 1f);

            // Message
            GameObject msgObj = new GameObject("Message", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            msgObj.transform.SetParent(panel.transform, false);
            RectTransform mRt = msgObj.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0.5f, 0.5f);
            mRt.anchorMax = new Vector2(0.5f, 0.5f);
            mRt.pivot = new Vector2(0.5f, 0.5f);
            mRt.sizeDelta = new Vector2(440f, 80f);
            mRt.anchoredPosition = new Vector2(0f, 10f);
            var mText = msgObj.GetComponent<TMPro.TextMeshProUGUI>();
            mText.text = "Bạn sẽ chỉ giữ 50% số Sao đã nhặt trong lượt này.";
            mText.fontSize = 22f;
            mText.alignment = TMPro.TextAlignmentOptions.Center;
            mText.color = new Color(0.9f, 0.92f, 0.96f, 1f);
            mText.enableWordWrapping = true;

            // Button Container
            GameObject btnRow = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnRow.transform.SetParent(panel.transform, false);
            RectTransform bRt = btnRow.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0f);
            bRt.anchorMax = new Vector2(0.5f, 0f);
            bRt.pivot = new Vector2(0.5f, 0f);
            bRt.sizeDelta = new Vector2(560f, 96f);
            bRt.anchoredPosition = new Vector2(0f, 25f);

            var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Button 1: TIẾP TỤC CHƠI (GoldPrimary, cao 96)
            CreateStyledConfirmButton(btnRow.transform, "TIẾP TỤC CHƠI", ButtonStyleType.GoldPrimary, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
                popup.SetActive(false);
            });

            // Button 2: BỎ CUỘC (CoralExit, cao 96)
            CreateStyledConfirmButton(btnRow.transform, "BỎ CUỘC", ButtonStyleType.CoralExit, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
                Time.timeScale = 1f;
                gameObject.SetActive(false);
                popup.SetActive(false);
                gm.ReturnToLevelSelect();
            });

            return popup;
        }

        private GameObject CreateStyledConfirmButton(Transform parent, string text, ButtonStyleType style, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject("Btn_" + text.Replace(" ", "_"), typeof(RectTransform), typeof(Image), typeof(Button), typeof(StyledButton));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260f, 96f);

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 96f;
            le.minHeight = 96f;
            le.flexibleWidth = 1f;

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = btnObj.GetComponent<Image>();
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);

            var tmpro = textObj.GetComponent<TMPro.TextMeshProUGUI>();
            tmpro.text = text;
            tmpro.raycastTarget = false;

            StyledButton sb = btnObj.GetComponent<StyledButton>();
            sb.StyleType = style;
            sb.AutoFindReferences();
            sb.ApplyStyle();

            return btnObj;
        }

        #endregion
    }
}
