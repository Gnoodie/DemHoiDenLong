using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Điều khiển hiển thị và hoạt ảnh của một Node màn chơi (thường hoặc Boss).
    /// </summary>
    public class LevelNodeUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Components")]
        public Button button;
        public RectTransform visualContainer;
        public CanvasGroup canvasGroup;

        [Header("Visual Elements")]
        public Image glowRing;
        public Image baseImage;
        public TMP_Text numberText;
        public Image bossPortrait;
        public GameObject bossBadge;
        public GameObject lockIcon;
        public GameObject newBadge;
        public GameObject starsRow;
        public Image[] starIcons; // 3 sao dưới node

        [Header("Settings & State")]
        public bool isBoss;
        public LevelNodeData nodeData;

        // Bảng màu trạng thái
        private static readonly Color ColorBaseLocked = new Color(0.353f, 0.373f, 0.502f, 1f);   // #5A5F80
        private static readonly Color ColorBossLocked = new Color(0.227f, 0.247f, 0.353f, 1f);   // #3A3F5A
        private static readonly Color ColorBaseUnlocked = Color.white;
        private static readonly Color ColorStarAchieved = new Color(1f, 0.824f, 0.478f, 1f);     // #FFD27A
        private static readonly Color ColorStarUnachieved = new Color(0.29f, 0.333f, 0.502f, 1f);  // #4A5580

        private Action<LevelNodeUI> onClickCallback;
        private Action<LevelNodeUI> onLockedCallback;

        private Coroutine pulseCoroutine;
        private Coroutine shakeCoroutine;
        private Coroutine entranceCoroutine;

        private void Awake()
        {
            EnsureComponents();
        }

        public void EnsureComponents()
        {
            if (button == null) button = GetComponent<Button>();
            if (visualContainer == null)
            {
                Transform v = transform.Find("Visual");
                if (v != null) visualContainer = v as RectTransform;
                else visualContainer = transform as RectTransform;
            }
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            // Tự động tìm baseImage / bossPortrait nếu chưa được gán
            if (baseImage == null)
            {
                Transform baseT = transform.Find("Visual/Base") ?? transform.Find("Visual/BossImage") ?? transform.Find("Base");
                if (baseT != null) baseImage = baseT.GetComponent<Image>();
            }
            if (isBoss && bossPortrait == null)
            {
                Transform bossT = transform.Find("Visual/BossImage") ?? transform.Find("Visual/OuterFrame") ?? transform.Find("BossImage");
                if (bossT != null) bossPortrait = bossT.GetComponent<Image>();
            }

            // Đảm bảo Graphic trên chính root GameObject nhận raycast chuẩn xác, không phụ thuộc object con
            Image rootImg = GetComponent<Image>();
            if (rootImg == null)
            {
                rootImg = gameObject.AddComponent<Image>();
                rootImg.color = Color.clear;
            }
            rootImg.raycastTarget = true;

            // Đảm bảo cả baseImage và bossPortrait cũng bật raycastTarget
            if (baseImage != null) baseImage.raycastTarget = true;
            if (bossPortrait != null) bossPortrait.raycastTarget = true;

            if (button != null)
            {
                button.transition = Selectable.Transition.None;
                button.targetGraphic = rootImg;
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnNodeClicked);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Cần thiết để InputSystemUIInputModule nhận diện đối tượng nhận tương tác
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Cần thiết để kích hoạt pointerClickHandler
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnNodeClicked();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (visualContainer != null)
            {
                visualContainer.localRotation = Quaternion.identity;
                visualContainer.localScale = Vector3.one;
            }
        }

        public void Setup(LevelNodeData data, Color accentColor, Action<LevelNodeUI> onClick, Action<LevelNodeUI> onLockedClick)
        {
            EnsureComponents();
            nodeData = data;
            isBoss = data.isBoss;
            onClickCallback = onClick;
            onLockedCallback = onLockedClick;

            StopPulse();
            StopShake();

            // Áp dụng trạng thái
            switch (data.state)
            {
                case NodeState.Locked:
                    ApplyLockedState();
                    break;
                case NodeState.Unlocked:
                    ApplyUnlockedState();
                    break;
                case NodeState.Current:
                    ApplyCurrentState(accentColor);
                    break;
            }
        }

        private void ApplyLockedState()
        {
            if (baseImage != null)
            {
                baseImage.color = ColorBaseLocked;
                baseImage.raycastTarget = true;
            }
            if (bossPortrait != null)
            {
                bossPortrait.color = ColorBossLocked;
                bossPortrait.raycastTarget = true;
            }
            if (numberText != null) numberText.gameObject.SetActive(false);
            if (lockIcon != null) lockIcon.SetActive(true);
            if (newBadge != null) newBadge.SetActive(false);
            if (glowRing != null) glowRing.gameObject.SetActive(false);
            if (starsRow != null) starsRow.SetActive(false);
            if (bossBadge != null) bossBadge.SetActive(isBoss);
        }

        private void ApplyUnlockedState()
        {
            if (baseImage != null)
            {
                baseImage.color = ColorBaseUnlocked;
                baseImage.raycastTarget = true;
            }
            if (bossPortrait != null)
            {
                bossPortrait.color = Color.white;
                bossPortrait.raycastTarget = true;
            }

            if (numberText != null)
            {
                numberText.gameObject.SetActive(!isBoss);
                int displayNum = (nodeData.levelId > 0) ? ((nodeData.levelId - 1) % 5 + 1) : 1;
                numberText.text = displayNum.ToString();
            }

            if (lockIcon != null) lockIcon.SetActive(false);
            if (newBadge != null) newBadge.SetActive(false);
            if (glowRing != null) glowRing.gameObject.SetActive(false);

            if (starsRow != null)
            {
                starsRow.SetActive(true);
                UpdateStars(nodeData.stars);
            }
            if (bossBadge != null) bossBadge.SetActive(isBoss);
        }

        private void ApplyCurrentState(Color accentColor)
        {
            ApplyUnlockedState();

            if (newBadge != null) newBadge.SetActive(true);

            if (glowRing != null)
            {
                glowRing.gameObject.SetActive(true);
                glowRing.color = accentColor;
            }

            // Bắt đầu nhấp nháy GlowRing và scale Visual
            if (gameObject.activeInHierarchy)
            {
                pulseCoroutine = StartCoroutine(PulseRoutine(accentColor));
            }
        }

        private void UpdateStars(int stars)
        {
            if (starIcons == null) return;
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].color = (i < stars) ? ColorStarAchieved : ColorStarUnachieved;
                }
            }
        }

        private int lastClickFrame = -1;

        private void OnNodeClicked()
        {
            if (Time.frameCount == lastClickFrame) return;
            lastClickFrame = Time.frameCount;

            Debug.Log($"[LevelNodeUI] OnNodeClicked -> levelId: {nodeData.levelId}, state: {nodeData.state}");
            if (nodeData.state == NodeState.Locked)
            {
                DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIError();
                PlayShakeAnimation();
                onLockedCallback?.Invoke(this);
            }
            else
            {
                DemHoiDenLong.Audio.AudioManager.Instance?.PlayUIClick();
                onClickCallback?.Invoke(this);
            }
        }

        public void PlayEntranceAnimation(float delay)
        {
            if (!gameObject.activeInHierarchy) return;
            if (entranceCoroutine != null) StopCoroutine(entranceCoroutine);
            entranceCoroutine = StartCoroutine(EntranceRoutine(delay));
        }

        private IEnumerator EntranceRoutine(float delay)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (visualContainer != null) visualContainer.localScale = new Vector3(0.6f, 0.6f, 1f);

            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3f); // Ease Out Cubic

                if (canvasGroup != null) canvasGroup.alpha = ease;
                if (visualContainer != null)
                {
                    float s = Mathf.Lerp(0.6f, 1f, ease);
                    visualContainer.localScale = new Vector3(s, s, 1f);
                }
                yield return null;
            }

            // Đảm bảo kết thúc hoàn chỉnh
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (visualContainer != null) visualContainer.localScale = Vector3.one;
            entranceCoroutine = null;
        }

        public void PlayShakeAnimation()
        {
            if (!gameObject.activeInHierarchy) return;
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            if (visualContainer == null) yield break;

            float duration = 0.3f;
            float elapsed = 0f;
            float maxAngle = 6f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float decay = 1f - progress;
                // Dao động sin tần số cao tắt dần
                float angle = Mathf.Sin(progress * Mathf.PI * 6f) * maxAngle * decay;
                visualContainer.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            visualContainer.localRotation = Quaternion.identity;
            shakeCoroutine = null;
        }

        private IEnumerator PulseRoutine(Color accentColor)
        {
            float glowTime = 0f;
            float scaleTime = 0f;

            while (true)
            {
                glowTime += Time.unscaledDeltaTime;
                scaleTime += Time.unscaledDeltaTime;

                // Chu kỳ Glow alpha 0.55 <-> 1.0 trong 1.6s
                if (glowRing != null)
                {
                    float tGlow = (Mathf.Sin(glowTime * (Mathf.PI * 2f / 1.6f)) + 1f) * 0.5f;
                    float alpha = Mathf.Lerp(0.55f, 1f, tGlow);
                    Color c = accentColor;
                    c.a = alpha;
                    glowRing.color = c;
                }

                // Chu kỳ Visual scale 1.0 <-> 1.06 trong 1.2s (chỉ khi không đang shake)
                if (visualContainer != null && shakeCoroutine == null)
                {
                    float tScale = (Mathf.Sin(scaleTime * (Mathf.PI * 2f / 1.2f)) + 1f) * 0.5f;
                    float s = Mathf.Lerp(1.0f, 1.06f, tScale);
                    visualContainer.localScale = new Vector3(s, s, 1f);
                }

                yield return null;
            }
        }

        private void StopPulse()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            if (visualContainer != null && shakeCoroutine == null)
            {
                visualContainer.localScale = Vector3.one;
            }
        }

        private void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            if (visualContainer != null)
            {
                visualContainer.localRotation = Quaternion.identity;
            }
        }
    }
}
