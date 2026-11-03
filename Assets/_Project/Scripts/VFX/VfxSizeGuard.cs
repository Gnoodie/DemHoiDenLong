using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.VFX
{
    /// <summary>
    /// VfxSizeGuard: Bộ đo và bảo vệ kích thước hiệu ứng kỹ năng tự động theo GDD 5A.8.
    /// Giám sát AABB của tất cả ParticleSystemRenderer và SpriteRenderer khi skill active mỗi 0.1s.
    /// In log [VFXSIZE] và tự động kẹp scale nếu vượt quá trần cấu hình.
    /// </summary>
    public class VfxSizeGuard : MonoBehaviour
    {
        public static VfxSizeGuard Instance { get; private set; }

        private Coroutine monitorCoroutine;
        private string currentLanId;
        private SkillConfig currentConfig;

        public float LastWPercent { get; private set; }
        public float LastHPercent { get; private set; }
        public float LastAreaPercent { get; private set; }
        public float PeakCoveragePercent { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void StartMonitoring(string lanId, SkillConfig config)
        {
            if (monitorCoroutine != null) StopCoroutine(monitorCoroutine);
            currentLanId = lanId ?? "1";
            currentConfig = config;
            PeakCoveragePercent = 0f;
            if (gameObject.activeInHierarchy)
            {
                monitorCoroutine = StartCoroutine(MonitorRoutine());
            }
        }

        public void StopMonitoring()
        {
            if (monitorCoroutine != null)
            {
                StopCoroutine(monitorCoroutine);
                monitorCoroutine = null;
            }
        }

        private IEnumerator MonitorRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                MeasureAndGuard();
            }
        }

        public void MeasureAndGuard()
        {
            if (currentConfig == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;

            float screenH = cam.orthographicSize * 2f;
            float screenW = screenH * cam.aspect;
            float screenArea = screenW * screenH;
            if (screenArea <= 0f) return;

            // Thu thập bounds hợp từ tất cả Renderer đang active của SkillVFXPool
            if (SkillVFXPool.Instance == null) return;
            var renderers = SkillVFXPool.Instance.GetComponentsInChildren<Renderer>(false);

            Bounds combined = new Bounds();
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || !r.gameObject.activeInHierarchy || !r.enabled) continue;

                // Bỏ qua renderer nếu particle system chưa phát hạt nào
                if (r is ParticleSystemRenderer psr)
                {
                    ParticleSystem ps = psr.GetComponent<ParticleSystem>();
                    if (ps != null && ps.particleCount == 0) continue;
                }

                Bounds b = r.bounds;
                if (b.size.x <= 0.001f && b.size.y <= 0.001f) continue;

                if (!hasBounds)
                {
                    combined = b;
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(b);
                }
            }

            if (!hasBounds) return;

            float wPercent = (combined.size.x / screenW) * 100f;
            float hPercent = (combined.size.y / screenH) * 100f;
            float area = combined.size.x * combined.size.y;
            float areaPercent = (area / screenArea) * 100f;

            LastWPercent = wPercent;
            LastHPercent = hPercent;
            LastAreaPercent = areaPercent;
            if (areaPercent > PeakCoveragePercent) PeakCoveragePercent = areaPercent;

            Debug.Log($"[VFXSIZE] Lan={currentLanId} W={wPercent:F1}% H={hPercent:F1}% Area={areaPercent:F1}% MaxConfig=(W={currentConfig.maxWidthPercentW:F1}%, H={currentConfig.maxHeightPercentH:F1}%, Area={currentConfig.maxCoveragePercent:F1}%)");

            // Kiểm tra và tự động kẹp scale nếu vượt quá trần cấu hình
            float scaleX = (currentConfig.maxWidthPercentW > 0f && wPercent > currentConfig.maxWidthPercentW)
                ? currentConfig.maxWidthPercentW / wPercent : 1f;
            float scaleY = (currentConfig.maxHeightPercentH > 0f && hPercent > currentConfig.maxHeightPercentH)
                ? currentConfig.maxHeightPercentH / hPercent : 1f;
            float scaleArea = (currentConfig.maxCoveragePercent > 0f && areaPercent > currentConfig.maxCoveragePercent)
                ? Mathf.Sqrt(currentConfig.maxCoveragePercent / areaPercent) : 1f;

            float minScale = Mathf.Min(scaleX, scaleY, scaleArea);
            if (minScale < 0.98f)
            {
                Debug.LogWarning($"[VFXSIZE_EXCEEDED] Lan={currentLanId} vượt trần kích thước! Tự động scale nhỏ lại theo tỉ lệ {minScale:F2} (W: {wPercent:F1}%->{wPercent*minScale:F1}%, H: {hPercent:F1}%->{hPercent*minScale:F1}%, Area: {areaPercent:F1}%->{areaPercent*minScale*minScale:F1}%)");
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer r = renderers[i];
                    if (r != null && r.gameObject.activeInHierarchy && r.enabled)
                    {
                        r.transform.localScale *= minScale;
                    }
                }
            }
        }
    }
}
