using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Thanh máu Boss - Chỉ hiện khi Boss xuất hiện, nằm dưới chip mục tiêu giữa màn hình, rộng 70%, cao 28, fill đỏ #FF5A5F.
    /// Có tên Boss phía trên, vạch phase tại 66% và 33%, trượt vào 0.3s.
    /// Tuyệt đối không đè thanh máu người chơi.
    /// </summary>
    public class BossHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private GameObject hudPanel;
        public GameObject Panel => hudPanel;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private RectTransform phase66Tick;
        [SerializeField] private RectTransform phase33Tick;
        
        private BossController activeBoss;
        private Coroutine slideRoutine;
        private Vector2 targetPos = new Vector2(0f, -220f);

        private void Awake()
        {
            AutoBindComponents();
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
        }

        public void AutoBindComponents()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();

            if (hudPanel == null)
            {
                if (canvas != null)
                {
                    Transform p = canvas.transform.Find("BossHUD/BossHUDPanel");
                    if (p == null) p = canvas.transform.Find("BossHUDPanel");
                    if (p != null) hudPanel = p.gameObject;
                }
                if (hudPanel == null)
                {
                    Transform p = transform.Find("BossHUDPanel");
                    if (p != null) hudPanel = p.gameObject;
                }
            }

            if (hudPanel != null)
            {
                RectTransform rtPanel = hudPanel.GetComponent<RectTransform>();
                if (rtPanel != null)
                {
                    rtPanel.anchorMin = new Vector2(0.5f, 1f);
                    rtPanel.anchorMax = new Vector2(0.5f, 1f);
                    rtPanel.pivot = new Vector2(0.5f, 1f);
                    rtPanel.anchoredPosition = new Vector2(0f, -220f);
                    rtPanel.sizeDelta = new Vector2(756f, 28f);
                }

                Transform s = hudPanel.transform.Find("BossHPSlider");
                if (s != null) hpSlider = s.GetComponent<Slider>();
                else hpSlider = hudPanel.GetComponentInChildren<Slider>(true);

                Transform nt = hudPanel.transform.Find("BossNameText");
                if (nt != null)
                {
                    bossNameText = nt.GetComponent<TextMeshProUGUI>();
                    RectTransform rtN = nt.GetComponent<RectTransform>();
                    if (rtN != null)
                    {
                        rtN.anchorMin = new Vector2(0.5f, 1f);
                        rtN.anchorMax = new Vector2(0.5f, 1f);
                        rtN.pivot = new Vector2(0.5f, 0f);
                        rtN.anchoredPosition = new Vector2(0f, 6f);
                        rtN.sizeDelta = new Vector2(756f, 32f);
                    }
                }
                else if (bossNameText == null)
                {
                    bossNameText = hudPanel.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                // Căn chỉnh BossHPSlider luôn khít hoàn toàn với BossHUDPanel
                if (hpSlider != null)
                {
                    RectTransform rtBS = hpSlider.GetComponent<RectTransform>();
                    if (rtBS != null)
                    {
                        rtBS.anchorMin = Vector2.zero;
                        rtBS.anchorMax = Vector2.one;
                        rtBS.pivot = new Vector2(0.5f, 0.5f);
                        rtBS.anchoredPosition = Vector2.zero;
                        rtBS.sizeDelta = Vector2.zero;
                        rtBS.offsetMin = new Vector2(2f, 2f);
                        rtBS.offsetMax = new Vector2(-2f, -2f);
                    }
                    Transform bFA = hpSlider.transform.Find("Fill Area");
                    Transform bF = bFA != null ? bFA.Find("Fill") : null;
                    if (bFA != null && bF != null)
                    {
                        RectTransform rtBFA = bFA.GetComponent<RectTransform>();
                        rtBFA.anchorMin = Vector2.zero;
                        rtBFA.anchorMax = Vector2.one;
                        rtBFA.offsetMin = Vector2.zero;
                        rtBFA.offsetMax = Vector2.zero;

                        RectTransform rtBF = bF.GetComponent<RectTransform>();
                        rtBF.anchorMin = Vector2.zero;
                        rtBF.anchorMax = Vector2.one;
                        rtBF.offsetMin = Vector2.zero;
                        rtBF.offsetMax = Vector2.zero;

                        Image imgBF = bF.GetComponent<Image>();
                        if (imgBF != null) imgBF.color = new Color(1f, 0.353f, 0.373f, 1f);
                        hpSlider.fillRect = rtBF;
                        hpSlider.targetGraphic = imgBF;
                    }
                }
            }
        }

        private void Start()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnBossSpawned += HandleBossSpawned;
                WaveManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            }
        }

        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnBossSpawned -= HandleBossSpawned;
                WaveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            }
            
            if (activeBoss != null)
            {
                activeBoss.OnHealthChanged -= UpdateHPBar;
                activeBoss.OnDeath -= HandleBossDeath;
            }
        }

        private void HandleBossSpawned(BossController boss)
        {
            AutoBindComponents();
            activeBoss = boss;
            activeBoss.OnHealthChanged += UpdateHPBar;
            activeBoss.OnDeath += HandleBossDeath;

            if (hudPanel != null)
            {
                hudPanel.SetActive(true);
                RectTransform rt = hudPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    targetPos = rt.anchoredPosition;
                    if (slideRoutine != null) StopCoroutine(slideRoutine);
                    slideRoutine = StartCoroutine(SlideInRoutine(rt));
                }
            }

            if (bossNameText != null)
            {
                string bName = "TRÙM";
                if (boss != null && !string.IsNullOrEmpty(boss.BossDisplayName))
                {
                    bName = boss.BossDisplayName;
                }
                else if (LevelSession.level != null && !string.IsNullOrEmpty(LevelSession.level.bossName))
                {
                    bName = LevelSession.level.bossName;
                }
                else if (boss != null && !string.IsNullOrEmpty(boss.name))
                {
                    bName = boss.name.Replace("PF_", "").Replace("(Clone)", "");
                }
                bossNameText.text = bName;
            }

            UpdateHPBar(activeBoss.CurrentHp, activeBoss.MaxHp);
        }

        private IEnumerator SlideInRoutine(RectTransform rt)
        {
            Vector2 startPos = new Vector2(targetPos.x, targetPos.y + 100f);
            float duration = 0.3f;
            float elapsed = 0f;
            rt.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            rt.anchoredPosition = targetPos;
            slideRoutine = null;
        }

        private void UpdateHPBar(float currentHp, float maxHp)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHp;
                hpSlider.value = currentHp;
            }
        }

        private void HandleBossDeath()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
            
            if (activeBoss != null)
            {
                activeBoss.OnHealthChanged -= UpdateHPBar;
                activeBoss.OnDeath -= HandleBossDeath;
                activeBoss = null;
            }
        }

        private void HandleLevelCompleted()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
        }
    }
}
