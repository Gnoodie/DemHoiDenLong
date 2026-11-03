using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.VFX
{
    /// <summary>
    /// Hệ thống phản hồi thị giác & nhịp điệu (Juice):
    /// Hit-stop, Camera shake, Screen flash, Vignette pulse, Enemy hit reaction.
    /// Hoạt động an toàn theo thời gian thực (unscaled time) và tự động dọn sạch khi kết thúc ván.
    /// </summary>
    public class JuiceFx : MonoBehaviour
    {
        public static JuiceFx Instance { get; private set; }

        private Vector3 originalCameraPos = new Vector3(0f, 0f, -10f);
        private Camera targetCamera;
        private Coroutine shakeCoroutine;
        private Coroutine hitStopCoroutine;
        private Coroutine flashCoroutine;
        private Coroutine borderPulseCoroutine;

        private Image fullScreenFlashImage;
        private Image borderFlashImage;

        // Giới hạn số hiệu ứng phản hồi địch đồng thời
        private int activeEnemyReactions = 0;
        private const int MaxConcurrentEnemyReactions = 6;

        public static int QualityLevel
        {
            get => PlayerPrefs.GetInt("VfxQuality", 1);
            set => PlayerPrefs.SetInt("VfxQuality", Mathf.Clamp(value, 0, 1));
        }

        public static bool IsLowQuality => QualityLevel == 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            FindCamera();
            EnsureFlashOverlays();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            ResetAll();
            if (Instance == this) Instance = null;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void FindCamera()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            if (targetCamera != null)
            {
                originalCameraPos = targetCamera.transform.position;
            }
        }

        private void EnsureFlashOverlays()
        {
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Full screen flash
            Transform flashT = canvas.transform.Find("JuiceScreenFlash");
            if (flashT == null)
            {
                GameObject fObj = new GameObject("JuiceScreenFlash", typeof(RectTransform), typeof(Image));
                fObj.transform.SetParent(canvas.transform, false);
                fObj.transform.SetAsLastSibling();

                RectTransform rt = fObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;

                fullScreenFlashImage = fObj.GetComponent<Image>();
                fullScreenFlashImage.color = new Color(1f, 1f, 1f, 0f);
                fullScreenFlashImage.raycastTarget = false;
            }
            else
            {
                fullScreenFlashImage = flashT.GetComponent<Image>();
            }

            // Border flash
            if (GameplayHUD.Instance != null && GameplayHUD.Instance.ScreenBorderFlashImage != null)
            {
                borderFlashImage = GameplayHUD.Instance.ScreenBorderFlashImage;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Playing)
            {
                ResetAll();
            }
        }

        public static void HitStop(float duration = 0.04f)
        {
            if (IsLowQuality || Instance == null || duration <= 0f) return;
            if (Instance.hitStopCoroutine != null) Instance.StopCoroutine(Instance.hitStopCoroutine);
            Instance.hitStopCoroutine = Instance.StartCoroutine(Instance.Routine_HitStop(duration));
        }

        private IEnumerator Routine_HitStop(float duration)
        {
            float prevScale = Time.timeScale;
            Time.timeScale = 0.05f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Time.timeScale = 1.0f;
            hitStopCoroutine = null;
        }

        public static void CameraShake(float amplitude = 0.15f, float duration = 0.25f)
        {
            if (IsLowQuality || Instance == null || amplitude <= 0f || duration <= 0f) return;
            Instance.FindCamera();
            if (Instance.targetCamera == null) return;

            if (Instance.shakeCoroutine != null)
            {
                Instance.StopCoroutine(Instance.shakeCoroutine);
                Instance.targetCamera.transform.position = Instance.originalCameraPos;
            }
            Instance.shakeCoroutine = Instance.StartCoroutine(Instance.Routine_CameraShake(amplitude, duration));
        }

        private IEnumerator Routine_CameraShake(float amplitude, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float currentAmp = Mathf.Lerp(amplitude, 0f, progress); // Decay

                float offsetX = UnityEngine.Random.Range(-1f, 1f) * currentAmp;
                float offsetY = UnityEngine.Random.Range(-1f, 1f) * currentAmp;
                targetCamera.transform.position = originalCameraPos + new Vector3(offsetX, offsetY, 0f);

                yield return null;
            }

            targetCamera.transform.position = originalCameraPos;
            shakeCoroutine = null;
        }

        public static void ScreenFlash(Color color, float maxAlpha = 0.2f, float duration = 0.15f)
        {
            if (Instance == null || duration <= 0f) return;
            Instance.EnsureFlashOverlays();
            if (Instance.fullScreenFlashImage == null) return;

            if (Instance.flashCoroutine != null) Instance.StopCoroutine(Instance.flashCoroutine);
            Instance.flashCoroutine = Instance.StartCoroutine(Instance.Routine_ScreenFlash(color, maxAlpha, duration));
        }

        private IEnumerator Routine_ScreenFlash(Color color, float maxAlpha, float duration)
        {
            float elapsed = 0f;
            color.a = maxAlpha;
            fullScreenFlashImage.color = color;
            fullScreenFlashImage.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                color.a = Mathf.Lerp(maxAlpha, 0f, t);
                fullScreenFlashImage.color = color;
                yield return null;
            }

            color.a = 0f;
            fullScreenFlashImage.color = color;
            flashCoroutine = null;
        }

        public static void VignettePulse(Color color, float maxAlpha = 0.6f, float duration = 0.25f)
        {
            if (Instance == null || duration <= 0f) return;
            Instance.EnsureFlashOverlays();
            if (Instance.borderFlashImage == null) return;

            if (Instance.borderPulseCoroutine != null) Instance.StopCoroutine(Instance.borderPulseCoroutine);
            Instance.borderPulseCoroutine = Instance.StartCoroutine(Instance.Routine_BorderPulse(color, maxAlpha, duration));
        }

        private IEnumerator Routine_BorderPulse(Color color, float maxAlpha, float duration)
        {
            float elapsed = 0f;
            borderFlashImage.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // Ping-pong pulse
                float a = Mathf.Sin(t * Mathf.PI) * maxAlpha;
                Color c = color;
                c.a = a;
                borderFlashImage.color = c;
                yield return null;
            }

            Color endCol = color;
            endCol.a = 0f;
            borderFlashImage.color = endCol;
            borderFlashImage.gameObject.SetActive(false);
            borderPulseCoroutine = null;
        }

        public static void EnemyHitReaction(EnemyBase enemy, Vector2 hitPoint, float knockback = 0.2f)
        {
            if (enemy == null || Instance == null) return;
            if (Instance.activeEnemyReactions >= MaxConcurrentEnemyReactions) return;

            Instance.StartCoroutine(Instance.Routine_EnemyHitReaction(enemy, hitPoint, knockback));
        }

        private IEnumerator Routine_EnemyHitReaction(EnemyBase enemy, Vector2 hitPoint, float knockback)
        {
            activeEnemyReactions++;
            SpriteRenderer sr = enemy != null ? enemy.GetComponentInChildren<SpriteRenderer>() : null;
            Color origColor = sr != null ? sr.color : Color.white;
            Vector3 origPos = enemy != null ? enemy.transform.position : Vector3.zero;

            // 1. Nháy trắng 0.08s
            if (sr != null)
            {
                sr.color = new Color(2f, 2f, 2f, 1f); // White bloom
            }

            // 2. Lùi nhẹ / rung
            if (enemy != null && knockback > 0f)
            {
                enemy.transform.position += Vector3.up * knockback;
            }

            yield return new WaitForSeconds(0.08f);

            if (sr != null)
            {
                sr.color = origColor;
            }
            if (enemy != null && knockback > 0f)
            {
                enemy.transform.position = Vector3.Lerp(enemy.transform.position, origPos, 0.5f);
            }

            activeEnemyReactions = Mathf.Max(0, activeEnemyReactions - 1);
        }

        public static void EnemyDeathReaction(EnemyBase enemy)
        {
            if (enemy == null || Instance == null) return;
            Instance.StartCoroutine(Instance.Routine_EnemyDeathReaction(enemy));
        }

        private IEnumerator Routine_EnemyDeathReaction(EnemyBase enemy)
        {
            SpriteRenderer sr = enemy != null ? enemy.GetComponentInChildren<SpriteRenderer>() : null;
            if (sr == null) yield break;

            Vector3 origScale = enemy.transform.localScale;
            float elapsed = 0f;
            float popDur = 0.12f;

            while (elapsed < popDur && enemy != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popDur;
                float s = Mathf.Lerp(1.0f, 1.35f, Mathf.Sin(t * Mathf.PI));
                enemy.transform.localScale = origScale * s;
                yield return null;
            }
        }

        public static void ResetAll()
        {
            if (Time.timeScale != 1.0f)
            {
                Time.timeScale = 1.0f;
            }

            if (Instance != null)
            {
                Instance.StopAllCoroutines();
                Instance.hitStopCoroutine = null;
                Instance.shakeCoroutine = null;
                Instance.flashCoroutine = null;
                Instance.borderPulseCoroutine = null;
                Instance.activeEnemyReactions = 0;

                if (Instance.targetCamera != null)
                {
                    Instance.targetCamera.transform.position = Instance.originalCameraPos;
                }

                if (Instance.fullScreenFlashImage != null)
                {
                    Color c = Instance.fullScreenFlashImage.color;
                    c.a = 0f;
                    Instance.fullScreenFlashImage.color = c;
                    Instance.fullScreenFlashImage.gameObject.SetActive(false);
                }

                if (Instance.borderFlashImage != null)
                {
                    Color c = Instance.borderFlashImage.color;
                    c.a = 0f;
                    Instance.borderFlashImage.color = c;
                    Instance.borderFlashImage.gameObject.SetActive(false);
                }
            }
        }
    }
}
