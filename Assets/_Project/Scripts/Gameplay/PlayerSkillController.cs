using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.UI;
using DemHoiDenLong.VFX;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Điều khiển kỹ năng chủ động của người chơi (Giai đoạn 5A.5).
    /// Thực hiện 4 nhịp: Dồn lực (0.15-0.25s) -> Bùng nổ -> Đỉnh điểm trúng đòn -> Tàn dư (0.4-0.6s).
    /// Tích hợp phản hồi thị giác JuiceFx, sprite vẽ tay và vật liệu URP Additive / ScrollMask.
    /// </summary>
    public class PlayerSkillController : MonoBehaviour
    {
        private PlayerController player;
        private SkillConfig currentConfig;

        private float cooldownTimer = 0f;
        private float maxCooldown = 15f;
        private bool isSkillActive = false;
        private Coroutine activeSkillRoutine;

        // Telemetry & metrics
        private int currentSkillHits = 0;
        private float currentSkillTotalDamage = 0f;
        private float averageSkillFPS = 60f;
        public float AverageSkillFPS => averageSkillFPS;
        public static float LastSkillFPS = 60f;

        // VFX LAB (Debug F9 controls)
        public static bool ShowSkillGizmos = false;
        public static float VfxIntensity = 1.0f;
        public static float VfxScale = 1.0f;
        public static bool EnableAuraLayer = true;
        public static bool EnableWaveLayer = true;
        public static bool EnableParticleLayer = true;
        public static bool EnableJuiceLayer = true;
        public static bool AutoLoopSkill = false;

        private float debugHitboxCenterY = 0f;
        private float debugHitboxWidth = 0f;
        private float debugHitboxHeight = 0f;

        public bool IsSkillActive => isSkillActive;
        public float CooldownTimer => Mathf.Max(0f, cooldownTimer);
        public float MaxCooldown => maxCooldown;
        public SkillConfig CurrentConfig => currentConfig;

        public PlayerController Player
        {
            get
            {
                if (player == null) player = GetComponent<PlayerController>();
                return player;
            }
            set => player = value;
        }

        private void Awake()
        {
            if (player == null) player = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void Start()
        {
            EnsureSkillVFXPool();
            EnsureJuiceFx();
            EnsureLifetimeGuard();
            EnsureVfxSizeGuard();
            LoadCurrentSkillConfig();
        }

        private void OnDisable()
        {
            CancelAllSkills();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            CancelAllSkills();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Playing)
            {
                CancelAllSkills();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentRunState != RunState.Playing && isSkillActive)
            {
                CancelAllSkills();
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    cooldownTimer = 0f;
                    GameplayHUD.Instance?.OnSkillReadyFlash();

                    if (AutoLoopSkill && IsSkillReady())
                    {
                        ActivateSkill();
                    }
                }
            }
        }

        private void EnsureSkillVFXPool()
        {
            if (SkillVFXPool.Instance == null)
            {
                GameObject poolObj = new GameObject("SkillVFXPool");
                poolObj.AddComponent<SkillVFXPool>();
            }
        }

        private void EnsureJuiceFx()
        {
            if (JuiceFx.Instance == null)
            {
                GameObject juiceObj = new GameObject("JuiceFx");
                juiceObj.AddComponent<JuiceFx>();
            }
        }

                private void EnsureLifetimeGuard()
        {
            if (SkillVfxLifetimeGuard.Instance == null)
            {
                GameObject guardObj = new GameObject("SkillVfxLifetimeGuard");
                guardObj.AddComponent<SkillVfxLifetimeGuard>();
            }
        }

        private void EnsureVfxSizeGuard()
        {
            if (VfxSizeGuard.Instance == null)
            {
                GameObject guardObj = new GameObject("VfxSizeGuard");
                guardObj.AddComponent<VfxSizeGuard>();
            }
        }

public void LoadCurrentSkillConfig()
        {
            string id = "1";
            if (player != null)
            {
                if (player.RuntimeLanData != null)
                {
                    if (player.RuntimeLanData.skillConfig != null)
                    {
                        currentConfig = player.RuntimeLanData.skillConfig;
                        maxCooldown = currentConfig.cooldown;
                        return;
                    }
                    id = player.RuntimeLanData.lanId;
                }
                else if (player.RuntimeLanConfig != null)
                {
                    id = player.RuntimeLanConfig.LanId;
                }
            }

            currentConfig = LoadSkillConfigById(id);
            if (currentConfig != null)
            {
                maxCooldown = currentConfig.cooldown;
            }
        }

        public SkillConfig LoadSkillConfigById(string lanId)
        {
            string id = string.IsNullOrEmpty(lanId) ? "1" : lanId.ToLower();
            string assetName = "Skill_KimLan";

            if (id.Contains("lan2") || id.Contains("hoa") || id.Contains("hoả") || id == "2") assetName = "Skill_HoaLan";
            else if (id.Contains("lan3") || id.Contains("thuy") || id.Contains("thuỷ") || id == "3") assetName = "Skill_ThuyLan";
            else if (id.Contains("lan4") || id.Contains("vang") || id.Contains("vàng") || id == "4") assetName = "Skill_LanVang";

            SkillConfig cfg = Resources.Load<SkillConfig>($"Skills/{assetName}");
#if UNITY_EDITOR
            if (cfg == null)
            {
                cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillConfig>($"Assets/_Project/Data/Skills/{assetName}.asset");
            }
#endif
            return cfg;
        }

        public bool IsSkillReady()
        {
            if (player == null) player = GetComponent<PlayerController>();
            if (player == null || player.IsDead) return false;
            if (isSkillActive || cooldownTimer > 0f) return false;

            if (GameManager.Instance != null && Application.isPlaying)
            {
                if (GameManager.Instance.CurrentState != GameState.Playing) return false;
                if (GameManager.Instance.CurrentRunState != RunState.Playing) return false;
            }

            return true;
        }

        public float GetCooldownFillAmount()
        {
            if (maxCooldown <= 0f) return 0f;
            return Mathf.Clamp01(cooldownTimer / maxCooldown);
        }

        public void ResetCooldownForDebug()
        {
            cooldownTimer = 0f;
            isSkillActive = false;
        }

        public void ActivateSkill()
        {
            if (player == null) player = GetComponent<PlayerController>();
            if (!IsSkillReady()) return;

            if (currentConfig == null) LoadCurrentSkillConfig();
            if (currentConfig == null) return;

            maxCooldown = currentConfig.cooldown;
            cooldownTimer = maxCooldown;
            isSkillActive = true;
            currentSkillHits = 0;
            currentSkillTotalDamage = 0f;

            // Audio
            Audio.AudioManager.Instance?.PlaySkill(currentConfig.lanId);

            // Banner UI
            GameplayHUD.Instance?.ShowSkillBanner(currentConfig.skillName, currentConfig.primaryColor);

            // Khởi chạy routine skill tương ứng
            string id = currentConfig.lanId != null ? currentConfig.lanId.ToLower() : "1";
            bool isHoa = id.Contains("lan2") || id.Contains("hoa") || id == "2";

            // Arm VFX Lifetime Guard
            float guardDuration = isHoa ? Mathf.Max(currentConfig.duration, 3.5f) : (currentConfig.duration > 0f ? currentConfig.duration : 1.5f);
            SkillVfxLifetimeGuard.Instance?.ArmGuard(guardDuration);
            VfxSizeGuard.Instance?.StartMonitoring(currentConfig.lanId, currentConfig);

            // Log [VFXMAT] telemetry
            SkillVFXPool.Instance?.LogAllSkillRenderers(currentConfig.lanId != null ? currentConfig.lanId : "1");
            if (id.Contains("lan2") || id.Contains("hoa") || id == "2")
            {
                if (Application.isPlaying) activeSkillRoutine = StartCoroutine(Routine_HoaLan(currentConfig));
            }
            else if (id.Contains("lan3") || id.Contains("thuy") || id == "3")
            {
                if (Application.isPlaying) activeSkillRoutine = StartCoroutine(Routine_ThuyLan(currentConfig));
            }
            else if (id.Contains("lan4") || id.Contains("vang") || id == "4")
            {
                if (Application.isPlaying) activeSkillRoutine = StartCoroutine(Routine_LanVang(currentConfig));
            }
            else
            {
                player.TriggerInvincibility(currentConfig.duration > 0f ? currentConfig.duration : 1.5f);
                player.SkillSpeedMultiplier = currentConfig.dashSpeedMultiplier > 0f ? currentConfig.dashSpeedMultiplier : 1.8f;
                player.IsKimDashing = true;
                if (Application.isPlaying) activeSkillRoutine = StartCoroutine(Routine_KimLan(currentConfig));
            }
        }

        public void SwitchLanForDebug(string forcedLanId)
        {
            CancelAllSkills();
            cooldownTimer = 0f;
            currentConfig = LoadSkillConfigById(forcedLanId);

            string dataName = "Lan_Kim";
            if (forcedLanId == "2" || forcedLanId.Contains("hoa")) dataName = "Lan_Hoa";
            else if (forcedLanId == "3" || forcedLanId.Contains("thuy")) dataName = "Lan_Thuy";
            else if (forcedLanId == "4" || forcedLanId.Contains("vang")) dataName = "Lan_Vang";

            LanData newLan = Resources.Load<LanData>($"LanData/{dataName}");
            if (newLan != null)
            {
                LevelSession.lan = newLan;
                if (player != null)
                {
                    player.SetRuntimeLanDataForDebug(newLan);
                }
            }

            DemHoiDenLong.UI.GameplayHUD.Instance?.SendMessage("LoadDynamicSkillIcon", UnityEngine.SendMessageOptions.DontRequireReceiver);
        }

        public void ActivateSkillForDebug(string forcedLanId)
        {
            SwitchLanForDebug(forcedLanId);
            ActivateSkill();
        }

        public void ActivateSkill(LanConfig config)
        {
            if (config != null) currentConfig = LoadSkillConfigById(config.LanId);
            ActivateSkill();
        }

        public void ActivateSkill(LanData data)
        {
            if (data != null && data.skillConfig != null) currentConfig = data.skillConfig;
            else if (data != null) currentConfig = LoadSkillConfigById(data.lanId);
            ActivateSkill();
        }

        // =============================================================
        // 1. KIM LÂN: "Lướt nhanh bất tử"
        // =============================================================
        // =============================================================
        // 1. KIM LÂN: "Lướt nhanh bất tử" (GDD 5A.7)
        // =============================================================
        private IEnumerator Routine_KimLan(SkillConfig cfg)
        {
            float duration = cfg.duration > 0f ? cfg.duration : 1.5f;
            var pool = SkillVFXPool.Instance;
            Camera mainCam = player.MainCamera != null ? player.MainCamera : Camera.main;

            float screenH = LanDimensions.GetScreenHeightWorld(mainCam);
            float screenW = LanDimensions.GetScreenWidthWorld(mainCam);
            float L = LanDimensions.GetLanWidth(cfg.lanId);

            int frameCount = 0;
            float unscaledTimer = 0f;
            float peakCoverage = 0f;

            // Kích hoạt bất tử toàn diện 1.5s
            player.SetSkillInvincible(true);
            player.TriggerInvincibility(duration);
            player.IsKimDashing = true;

            // 1. Vòng hiệu ứng nở lúc bắt đầu tại CenterPoint (1.4L -> 2.2L trong 0.25s)
            if (pool != null && pool.kimEndRing != null)
            {
                pool.kimEndRing.SetActive(true);
                Transform cp = player.CenterPoint;
                pool.kimEndRing.transform.position = cp.position;
                if (pool.kimEndRingSR != null)
                {
                    pool.kimEndRingSR.color = new Color(0.85f, 0.89f, 0.94f, 0.8f);
                    string kimSpr = pool.kimEndRingSR.sprite != null ? pool.kimEndRingSR.sprite.name : "null";
                    Debug.Log($"[VFXMAT] KimRing sr={pool.kimEndRingSR.name} sprite={kimSpr} color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(pool.kimEndRingSR.color)}");
                }
                StartCoroutine(Routine_RingExpand(pool.kimEndRing, pool.kimEndRingSR, cp, 1.4f * L, 2.2f * L, cfg.ringExpandDur > 0f ? cfg.ringExpandDur : 0.25f));
            }

            // 2. Viền khiên mảnh quanh Lân đường kính 1.3L tại CenterPoint
            if (pool != null && pool.kimShieldRing != null)
            {
                pool.kimShieldRing.SetActive(true);
                pool.kimShieldRing.transform.position = player.CenterPoint.position;
                pool.kimShieldRing.transform.localScale = new Vector3(cfg.shieldAuraDiameterL * L, cfg.shieldAuraDiameterL * L, 1f);
            }

            // 3. Vệt gió bạc vfx_silver_streaks phía SAU Lân (rộng 0.6L, cao 18% H, chỉ pha 1 và 2)
            if (pool != null && pool.kimSilverStreak != null)
            {
                pool.kimSilverStreak.SetActive(true);
                pool.kimSilverStreak.transform.localScale = new Vector3(cfg.silverStreakWidthL * L, cfg.silverStreakHeightH * screenH, 1f);
            }

            float p1Dur = cfg.dashPhase1Dur > 0f ? cfg.dashPhase1Dur : 0.55f;
            float p2Dur = cfg.dashPhase2Dur > 0f ? cfg.dashPhase2Dur : 0.45f;
            float p3Dur = cfg.dashPhase3Dur > 0f ? cfg.dashPhase3Dur : 0.50f;

            float p1TargetOffset = cfg.dashPhase1OffsetH * screenH; // 40% H
            float p2ExtraDrift = cfg.dashPhase2DriftH * screenH;     // 3% H

            float elapsed = 0f;
            float ghostTimer = 0f;
            float ghostInterval = cfg.ghostInterval > 0f ? cfg.ghostInterval : 0.035f;
            SpriteRenderer playerSR = player.GetComponentInChildren<SpriteRenderer>();

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    unscaledTimer += Time.unscaledDeltaTime;
                    frameCount++;
                    ghostTimer += Time.deltaTime;

                    // --- CHUYỂN ĐỘNG THẬT TRÊN Y ---
                    float currentOffsetY = 0f;
                    if (elapsed <= p1Dur)
                    {
                        // Pha 1 (0.55s, ease-out): offset lên 40% H
                        float p = Mathf.Clamp01(elapsed / p1Dur);
                        float easeOut = Mathf.Sin(p * Mathf.PI * 0.5f);
                        currentOffsetY = p1TargetOffset * easeOut;
                    }
                    else if (elapsed <= p1Dur + p2Dur)
                    {
                        // Pha 2 (0.45s): giữ, trôi thêm 3% H
                        float p = Mathf.Clamp01((elapsed - p1Dur) / p2Dur);
                        currentOffsetY = p1TargetOffset + p2ExtraDrift * p;
                    }
                    else
                    {
                        // Pha 3 (0.5s, ease-in-out): trở về vị trí theo ngón tay
                        float p = Mathf.Clamp01((elapsed - p1Dur - p2Dur) / p3Dur);
                        float easeInOut = Mathf.SmoothStep(0f, 1f, p);
                        currentOffsetY = Mathf.Lerp(p1TargetOffset + p2ExtraDrift, 0f, easeInOut);

                        // Tắt vệt gió bạc ở pha 3
                        if (pool != null && pool.kimSilverStreak != null && pool.kimSilverStreak.activeSelf)
                        {
                            pool.kimSilverStreak.SetActive(false);
                        }
                    }
                    player.SkillOffsetY = currentOffsetY;

                    // Cập nhật vị trí khiên và vệt gió bám CenterPoint
                    Transform centerP = player.CenterPoint;
                    if (pool != null && pool.kimShieldRing != null && pool.kimShieldRing.activeSelf)
                    {
                        pool.kimShieldRing.transform.position = centerP.position;

                        // Alpha dao động 0.35 đến 0.6 nhịp chậm, 0.3s cuối nhấp nháy báo hết
                        float a = Mathf.Lerp(cfg.shieldAuraAlphaMin, cfg.shieldAuraAlphaMax, (Mathf.Sin(elapsed * 8f) + 1f) * 0.5f);
                        if (duration - elapsed <= cfg.shieldBlinkDur)
                        {
                            // Nhấp nháy 0.3s cuối
                            a = ((int)((duration - elapsed) * 20f) % 2 == 0) ? cfg.shieldAuraAlphaMax : 0.1f;
                        }
                        if (pool.kimShieldRingSR != null)
                        {
                            Color c = pool.kimShieldRingSR.color;
                            c.a = a;
                            pool.kimShieldRingSR.color = c;
                        }
                    }

                    if (pool != null && pool.kimSilverStreak != null && pool.kimSilverStreak.activeSelf)
                    {
                        pool.kimSilverStreak.transform.position = centerP.position - new Vector3(0f, 0.35f * L, 0f);
                    }

                    // Bóng mờ (Ghost trail) sao chép sprite mỗi 0.035s, mờ dần 0.25s
                    if (ghostTimer >= ghostInterval && playerSR != null && playerSR.sprite != null && pool != null)
                    {
                        ghostTimer = 0f;
                        var ghost = pool.GetGhostFromPool();
                        if (ghost != null)
                        {
                            ghost.gameObject.SetActive(true);
                            StartCoroutine(Routine_GhostFade(ghost, playerSR.sprite, player.transform.position, player.transform.localScale, cfg.ghostFadeTime));
                        }
                    }

                    // Đo độ phủ VFX
                    if (pool != null)
                    {
                        float cov = pool.CalculateVFXCoveragePercent(mainCam);
                        if (cov > peakCoverage) peakCoverage = cov;
                    }

                    yield return null;
                }
            }
            finally
            {
                // Kết thúc skill: dọn dẹp và trả trạng thái
                if (player != null)
                {
                    player.SetSkillInvincible(false);
                    player.SkillOffsetY = 0f;
                    player.IsKimDashing = false;
                    player.SkillSpeedMultiplier = 1.0f; // Khôi phục độ nhạy di chuyển về bình thường
                }

                if (pool != null)
                {
                    if (pool.kimShieldRing != null) pool.kimShieldRing.SetActive(false);
                    if (pool.kimSilverStreak != null) pool.kimSilverStreak.SetActive(false);
                }

                averageSkillFPS = frameCount / Mathf.Max(0.001f, unscaledTimer);
                LogSkillTelemetry("Lướt nhanh bất tử", 0, 0f, duration, peakCoverage);
                isSkillActive = false;
                activeSkillRoutine = null;
            }
        }

        private IEnumerator Routine_RingExpand(GameObject ring, SpriteRenderer sr, Transform followAnchor, float startD, float endD, float dur)
        {
            float el = 0f;
            while (el < dur)
            {
                el += Time.deltaTime;
                float p = Mathf.Clamp01(el / dur);
                float curD = Mathf.Lerp(startD, endD, p);
                ring.transform.localScale = new Vector3(curD, curD, 1f);
                if (followAnchor != null) ring.transform.position = followAnchor.position;

                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(0.75f, 0f, p);
                    sr.color = c;
                }
                yield return null;
            }
            ring.SetActive(false);
        }

        // =============================================================
        // 2. HỎA LÂN: "Phun lửa phía trước" (giữ 3 giây, ngọn lửa lớn)
        // =============================================================
        private IEnumerator Routine_HoaLan(SkillConfig cfg)
        {
            var pool = SkillVFXPool.Instance;
            Camera mainCam = player.MainCamera != null ? player.MainCamera : Camera.main;

            float screenH = LanDimensions.GetScreenHeightWorld(mainCam);
            float screenW = LanDimensions.GetScreenWidthWorld(mainCam);
            float L = LanDimensions.GetLanWidth(cfg.lanId);

            int frameCount = 0;
            float unscaledTimer = 0f;
            float peakCoverage = 0f;

            Transform mouth = player.MouthPoint;

            // --- PHA 1: DỒN LỰC (0.2s xoáy lửa tại miệng phình từ 0.4L lên 1.2L) ---
            float chgDur = cfg.chargeDur > 0f ? cfg.chargeDur : 0.2f;
            if (pool != null && pool.hoaMouthSwirl != null)
            {
                pool.hoaMouthSwirl.SetActive(true);
                pool.hoaMouthSwirl.transform.position = mouth.position;
                float elChg = 0f;
                while (elChg < chgDur)
                {
                    elChg += Time.deltaTime;
                    float p = Mathf.Clamp01(elChg / chgDur);
                    float s = Mathf.Lerp(0.4f * L, 1.2f * L, p) * VfxScale;
                    pool.hoaMouthSwirl.transform.localScale = new Vector3(s, s, 1f);
                    pool.hoaMouthSwirl.transform.position = mouth.position;
                    pool.hoaMouthSwirl.transform.Rotate(0f, 0f, -360f * Time.deltaTime);
                    yield return null;
                }
                pool.hoaMouthSwirl.SetActive(false);
            }

            // Rung camera nhẹ chỉ 0.2s lúc bắt đầu phun (tối đa 0.5% W)
            if (EnableJuiceLayer)
            {
                float shakeAmp = Mathf.Min(cfg.cameraShakeAmpW * screenW, 0.005f * screenW);
                JuiceFx.CameraShake(shakeAmp, cfg.cameraShakeDur > 0f ? cfg.cameraShakeDur : 0.2f);
            }

            // --- PHA 2: PHUN LỬA PHÍA TRƯỚC (Giữ trong 3 giây, ngọn lửa lớn) ---
            float flameDur = 3.0f; // Giữ trong vòng 3 giây
            float coneRadius = Mathf.Max(cfg.coneRadiusH * screenH, 0.75f * screenH);
            float halfAngle = Mathf.Max(cfg.coneHalfAngle, 22.5f); // Nón lửa lớn (nửa góc 22.5 độ, quạt 45 độ)

            // Bật ParticleSystem phun lửa, tàn lửa và điểm phát lửa tại miệng Lân
            if (pool != null)
            {
                if (pool.hoaFlameStreamPS != null)
                {
                    pool.hoaFlameStreamPS.gameObject.SetActive(true);
                    var mainPS = pool.hoaFlameStreamPS.main;
                    mainPS.duration = flameDur;
                    mainPS.startSpeed = coneRadius / 0.35f;
                    pool.hoaFlameStreamPS.transform.position = mouth.position;
                    pool.hoaFlameStreamPS.transform.rotation = Quaternion.identity; // Phun thẳng phía trước
                    pool.hoaFlameStreamPS.Play();
                }

                if (pool.hoaSparksPS != null)
                {
                    pool.hoaSparksPS.gameObject.SetActive(true);
                    var mainSpk = pool.hoaSparksPS.main;
                    mainSpk.duration = flameDur;
                    pool.hoaSparksPS.transform.position = mouth.position;
                    pool.hoaSparksPS.transform.rotation = Quaternion.identity; // Phun thẳng phía trước
                    pool.hoaSparksPS.Play();
                }

                // Điểm tụ lửa rực sáng liên tục tại miệng Lân
                if (pool.hoaMouthSwirl != null)
                {
                    pool.hoaMouthSwirl.transform.localScale = Vector3.one * (0.8f * L * VfxScale);
                    pool.hoaMouthSwirl.transform.position = mouth.position;
                    pool.hoaMouthSwirl.SetActive(true);
                }
            }

            Dictionary<Component, float> damageTracker = new Dictionary<Component, float>();
            float elapsed = 0f;
            float tickTimer = 0f;
            float tickInterval = cfg.tickInterval > 0f ? cfg.tickInterval : 0.1f;
            int totalTicksApplied = 0;
            Vector2 aimDir = Vector2.up; // Hướng thẳng phía trước

            try
            {
                while (elapsed < flameDur)
                {
                    elapsed += Time.deltaTime;
                    unscaledTimer += Time.unscaledDeltaTime;
                    frameCount++;
                    tickTimer += Time.deltaTime;

                    Vector3 mouthPos = mouth.position;

                    // Cập nhật vị trí bám sát miệng Lân khi di chuyển, luôn hướng thẳng về phía trước (+Y)
                    if (pool != null)
                    {
                        if (pool.hoaFlameStreamPS != null)
                        {
                            pool.hoaFlameStreamPS.transform.position = mouthPos;
                            pool.hoaFlameStreamPS.transform.rotation = Quaternion.identity;
                        }
                        if (pool.hoaSparksPS != null)
                        {
                            pool.hoaSparksPS.transform.position = mouthPos;
                            pool.hoaSparksPS.transform.rotation = Quaternion.identity;
                        }
                        if (pool.hoaMouthSwirl != null)
                        {
                            pool.hoaMouthSwirl.transform.position = mouthPos;
                            pool.hoaMouthSwirl.transform.Rotate(0f, 0f, -480f * Time.deltaTime);
                        }
                    }

                    // Cập nhật debug gizmo data
                    debugAimAngle = 0f;
                    debugConeOrigin = mouthPos;
                    debugConeRadius = coneRadius;
                    debugConeHalfAngle = halfAngle;

                    // Xử lý sát thương theo tick (mỗi 0.1s)
                    if (tickTimer >= tickInterval)
                    {
                        tickTimer = 0f;
                        totalTicksApplied++;

                        // 1. Quét quái thường
                        var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
                        foreach (var eb in enemies)
                        {
                            if (eb == null || !eb.gameObject.activeInHierarchy || eb.IsDead) continue;
                            Vector2 toEnemy = (Vector2)eb.transform.position - (Vector2)mouthPos;
                            float dist = toEnemy.magnitude;
                            if (dist <= coneRadius && toEnemy.y >= -0.2f)
                            {
                                float angleBetween = Vector2.Angle(aimDir, toEnemy.normalized);
                                if (angleBetween <= halfAngle || (dist <= 1.2f && Mathf.Abs(toEnemy.x) <= 0.8f))
                                {
                                    float curDmg = damageTracker.ContainsKey(eb) ? damageTracker[eb] : 0f;
                                    if (curDmg < cfg.damage) // cap 50
                                    {
                                        float toApply = Mathf.Min(cfg.damagePerTick, cfg.damage - curDmg);
                                        eb.TakeDamage(toApply, KillCause.PlayerSkill);
                                        damageTracker[eb] = curDmg + toApply;
                                        currentSkillHits++;
                                        currentSkillTotalDamage += toApply;

                                        // Lửa nhỏ bám địch 0.6s
                                        if (pool != null)
                                        {
                                            var burn = pool.GetBurnFromPool();
                                            if (burn != null)
                                            {
                                                burn.transform.localScale = new Vector3(cfg.enemyBurnSizeL * L, cfg.enemyBurnSizeL * L, 1f);
                                                StartCoroutine(Routine_EnemyBurn(burn, eb.transform, cfg.fireDebuffDuration));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // 2. Quét Boss
                        var bosses = FindObjectsByType<BossController>(FindObjectsSortMode.None);
                        foreach (var boss in bosses)
                        {
                            if (boss == null || !boss.gameObject.activeInHierarchy || boss.IsDead) continue;
                            Vector2 toBoss = (Vector2)boss.transform.position - (Vector2)mouthPos;
                            float dist = toBoss.magnitude;
                            if (dist <= coneRadius && toBoss.y >= -0.2f)
                            {
                                float angleBetween = Vector2.Angle(aimDir, toBoss.normalized);
                                if (angleBetween <= halfAngle || (dist <= 1.5f && Mathf.Abs(toBoss.x) <= 1.2f))
                                {
                                    float curDmg = damageTracker.ContainsKey(boss) ? damageTracker[boss] : 0f;
                                    if (curDmg < cfg.bossDamageCap) // cap 100
                                    {
                                        float toApply = Mathf.Min(cfg.damagePerTick, cfg.bossDamageCap - curDmg);
                                        boss.TakeDamage(toApply);
                                        damageTracker[boss] = curDmg + toApply;
                                        currentSkillHits++;
                                        currentSkillTotalDamage += toApply;
                                    }
                                }
                            }
                        }
                    }

                    // Đo độ phủ VFX
                    if (pool != null)
                    {
                        float cov = pool.CalculateVFXCoveragePercent(mainCam);
                        if (cov > peakCoverage) peakCoverage = cov;
                    }

                    yield return null;
                }
            }
            finally
            {
                // Tắt tia lửa, tàn lửa và điểm tụ miệng
                if (pool != null)
                {
                    if (pool.hoaFlameStreamPS != null) pool.hoaFlameStreamPS.Stop();
                    if (pool.hoaSparksPS != null) pool.hoaSparksPS.Stop();
                    if (pool.hoaMouthSwirl != null) pool.hoaMouthSwirl.SetActive(false);
                }
            }

            if (pool != null)
            {
                yield return new WaitForSeconds(cfg.fadeDur > 0f ? cfg.fadeDur : 0.2f);
                if (pool.hoaFlameStreamPS != null) pool.hoaFlameStreamPS.gameObject.SetActive(false);
                if (pool.hoaSparksPS != null) pool.hoaSparksPS.gameObject.SetActive(false);
            }

            averageSkillFPS = frameCount / Mathf.Max(0.001f, unscaledTimer);
            LogSkillTelemetry("Phun lửa phía trước", damageTracker.Count, currentSkillTotalDamage, flameDur + chgDur, peakCoverage);
            isSkillActive = false;
            activeSkillRoutine = null;
        }

        private IEnumerator Routine_EnemyBurn(GameObject burnObj, Transform enemyT, float dur)
        {
            burnObj.SetActive(true);
            float el = 0f;
            while (el < dur && enemyT != null && enemyT.gameObject.activeInHierarchy)
            {
                el += Time.deltaTime;
                burnObj.transform.position = enemyT.position;
                yield return null;
            }
            burnObj.SetActive(false);
        }

        // =============================================================
        // 3. THỦY LÂN: "Sóng nước đẩy lùi" (GDD 5A.7)
        // =============================================================
        private IEnumerator Routine_ThuyLan(SkillConfig cfg)
        {
            var pool = SkillVFXPool.Instance;
            Camera mainCam = player.MainCamera != null ? player.MainCamera : Camera.main;

            float screenH = LanDimensions.GetScreenHeightWorld(mainCam);
            float screenW = LanDimensions.GetScreenWidthWorld(mainCam);
            float L = LanDimensions.GetLanWidth(cfg.lanId);

            int frameCount = 0;
            float unscaledTimer = 0f;
            float peakCoverage = 0f;

            Transform mouth = player.MouthPoint;

            // 1. Vòng hiệu ứng tại miệng lúc bắt đầu (1.0L nở lên 1.8L trong 0.2s)
            if (pool != null && pool.thuyChargeRing != null)
            {
                pool.thuyChargeRing.SetActive(true);
                pool.thuyChargeRing.transform.position = mouth.position;
                if (pool.thuyChargeRingSR != null)
                {
                    pool.thuyChargeRingSR.color = new Color(0.50f, 0.91f, 0.96f, 0.85f);
                    string thuySpr = pool.thuyChargeRingSR.sprite != null ? pool.thuyChargeRingSR.sprite.name : "null";
                    Debug.Log($"[VFXMAT] ThuyRing sr={pool.thuyChargeRingSR.name} sprite={thuySpr} color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(pool.thuyChargeRingSR.color)}");
                }
                StartCoroutine(Routine_RingExpand(pool.thuyChargeRing, pool.thuyChargeRingSR, mouth, cfg.mouthRingStartL * L, cfg.mouthRingEndL * L, cfg.mouthRingDur > 0f ? cfg.mouthRingDur : 0.2f));
            }

            // 2. Mũi sóng nước lướt lên đỉnh màn hình trong 1.2s
            float travelDur = cfg.waveTravelDur > 0f ? cfg.waveTravelDur : 1.2f;
            float startX = mouth != null ? mouth.position.x : player.transform.position.x;
            float startY = mouth != null ? mouth.position.y : player.transform.position.y;
            float camCenterX = mainCam != null ? mainCam.transform.position.x : 0f;
            float endY = (mainCam != null ? mainCam.transform.position.y + screenH * 0.55f : 6.0f);
            float totalTravelDist = endY - startY;

            float startW = cfg.waveStartWidthL * L;
            float endW = cfg.waveEndWidthW * screenW;
            float hitboxH = cfg.hitboxHeightH * screenH; // dải cao 12% H

            if (pool != null && pool.thuyWaveFront != null)
            {
                pool.thuyWaveFront.SetActive(true);
                pool.thuyWaveFront.transform.position = new Vector3(startX, startY, 0f);
            }

            HashSet<Component> hitTargets = new HashSet<Component>();
            float elapsed = 0f;
            int bulletsCleared = 0;
            float currentWaveX = startX;
            float currentWaveY = startY;

            while (elapsed < travelDur)
            {
                elapsed += Time.deltaTime;
                unscaledTimer += Time.unscaledDeltaTime;
                frameCount++;

                float p = Mathf.Clamp01(elapsed / travelDur);
                // Vận tốc tăng dần từ 0.8 H/s lên 1.6 H/s (tích phân tăng tốc)
                float travelP = Mathf.Pow(p, 1.35f);
                currentWaveX = Mathf.Lerp(startX, camCenterX, p);
                currentWaveY = startY + totalTravelDist * travelP;
                float currentWidth = Mathf.Lerp(startW, endW, p);

                if (pool != null && pool.thuyWaveFront != null)
                {
                    pool.thuyWaveFront.transform.position = new Vector3(currentWaveX, currentWaveY, 0f);
                    // Tỉ lệ Y = 0.55 so với X
                    float maxWaveH = 0.12f * screenH;
                    float waveH = Mathf.Min(currentWidth * cfg.waveScaleYRatio, maxWaveH);
                    pool.thuyWaveFront.transform.localScale = new Vector3(currentWidth, waveH, 1f);
                }

                // Cập nhật debug gizmo
                debugWaveCenterX = currentWaveX;
                debugWaveCenterY = currentWaveY - hitboxH * 0.5f;
                debugWaveWidth = currentWidth;
                debugWaveHeight = hitboxH;

                // Vùng sát thương: dải cao 12% H ngay sau mũi sóng
                float hitBottom = currentWaveY - hitboxH;
                float hitTop = currentWaveY;
                float hitLeft = currentWaveX - currentWidth * 0.5f;
                float hitRight = currentWaveX + currentWidth * 0.5f;

                // Xóa đạn địch trong dải sóng
                bulletsCleared += ClearBulletsInBox(new Vector2(hitLeft, hitBottom), new Vector2(hitRight, hitTop));

                // Quét quái thường: trúng ĐÚNG 1 LẦN
                var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
                foreach (var eb in enemies)
                {
                    if (eb == null || !eb.gameObject.activeInHierarchy || eb.IsDead || hitTargets.Contains(eb)) continue;
                    Vector3 pos = eb.transform.position;
                    if (pos.x >= hitLeft && pos.x <= hitRight && pos.y >= hitBottom && pos.y <= hitTop)
                    {
                        hitTargets.Add(eb);
                        eb.TakeDamage(cfg.damage, KillCause.PlayerSkill); // 30 dmg
                        currentSkillHits++;
                        currentSkillTotalDamage += cfg.damage;

                        // Đẩy lùi lên trên 20% H trong 0.45s (ease-out), kẹp không vượt vùng spawn
                        float pushDist = cfg.pushDistanceRatio * screenH;
                        StartCoroutine(Routine_PushEnemy(eb.transform, pushDist, cfg.pushDuration, mainCam != null ? mainCam.transform.position.y + screenH * 0.48f : 5.5f));

                        // Làm chậm 50% trong 3.0s, bọt nước bám quanh
                        eb.ApplySlow(cfg.slowPercent, cfg.slowDuration);
                        if (pool != null)
                        {
                            var bub = pool.GetSlowBubbleFromPool();
                            if (bub != null)
                            {
                                bub.transform.localScale = new Vector3(cfg.bubbleSizeL * L, cfg.bubbleSizeL * L, 1f);
                                StartCoroutine(Routine_SlowBubbleFollow(bub, eb.transform, cfg.slowDuration));
                            }
                        }
                    }
                }

                // Quét Boss: trúng ĐÚNG 1 LẦN, 30 dmg, KHÔNG bị đẩy lùi, chậm 25%
                var bosses = FindObjectsByType<BossController>(FindObjectsSortMode.None);
                foreach (var boss in bosses)
                {
                    if (boss == null || !boss.gameObject.activeInHierarchy || boss.IsDead || hitTargets.Contains(boss)) continue;
                    Vector3 pos = boss.transform.position;
                    if (pos.x >= hitLeft && pos.x <= hitRight && pos.y >= hitBottom && pos.y <= hitTop)
                    {
                        hitTargets.Add(boss);
                        boss.TakeDamage(cfg.damage);
                        currentSkillHits++;
                        currentSkillTotalDamage += cfg.damage;

                        // Boss làm chậm 25% trong 3s, KHÔNG bị đẩy
                        StartCoroutine(Routine_ApplySlowBoss(boss, cfg.bossSlowPercent, cfg.slowDuration));
                    }
                }

                // Đo độ phủ VFX
                if (pool != null)
                {
                    float cov = pool.CalculateVFXCoveragePercent(mainCam);
                    if (cov > peakCoverage) peakCoverage = cov;
                }

                yield return null;
            }

            // Tắt sóng khi tới đỉnh màn hình
            if (pool != null && pool.thuyWaveFront != null)
            {
                pool.thuyWaveFront.SetActive(false);
            }

            averageSkillFPS = frameCount / Mathf.Max(0.001f, unscaledTimer);
            LogSkillTelemetry("Sóng nước đẩy lùi", hitTargets.Count, currentSkillTotalDamage, travelDur, peakCoverage, bulletsCleared);
            isSkillActive = false;
            activeSkillRoutine = null;
        }

        private IEnumerator Routine_PushEnemy(Transform enemyT, float pushDist, float pushDur, float maxSpawnY)
        {
            if (enemyT == null) yield break;
            Vector3 startP = enemyT.position;
            Vector3 targetP = startP + new Vector3(0f, pushDist, 0f);
            if (targetP.y > maxSpawnY) targetP.y = maxSpawnY;

            float el = 0f;
            while (el < pushDur && enemyT != null && enemyT.gameObject.activeInHierarchy)
            {
                el += Time.deltaTime;
                float p = Mathf.Clamp01(el / pushDur);
                float easeOut = Mathf.Sin(p * Mathf.PI * 0.5f);
                enemyT.position = Vector3.Lerp(startP, targetP, easeOut);
                yield return null;
            }
        }

        private IEnumerator Routine_SlowBubbleFollow(GameObject bub, Transform targetT, float dur)
        {
            bub.SetActive(true);
            float el = 0f;
            while (el < dur && targetT != null && targetT.gameObject.activeInHierarchy)
            {
                el += Time.deltaTime;
                bub.transform.position = targetT.position;
                yield return null;
            }
            bub.SetActive(false);
        }

        private IEnumerator Routine_ApplySlowBoss(BossController boss, float slowPct, float dur)
        {
            if (boss == null) yield break;
            boss.ApplySlow(slowPct);
            yield return new WaitForSeconds(dur);
            if (boss != null) boss.RemoveSlow();
        }

        // =============================================================
        // 4. LÂN VÀNG: "Mưa pháo hoa" (GDD 5A.7)
        // =============================================================
        private IEnumerator Routine_LanVang(SkillConfig cfg)
        {
            var pool = SkillVFXPool.Instance;
            Camera mainCam = player.MainCamera != null ? player.MainCamera : Camera.main;

            float screenH = LanDimensions.GetScreenHeightWorld(mainCam);
            float screenW = LanDimensions.GetScreenWidthWorld(mainCam);
            float L = LanDimensions.GetLanWidth(cfg.lanId);

            int frameCount = 0;
            float unscaledTimer = 0f;
            float peakCoverage = 0f;

            Transform mouth = player.MouthPoint;

            int rocketCount = cfg.rocketCount > 0 ? cfg.rocketCount : 16;
            float rocketSpeed = cfg.rocketSpeedH * screenH; // 1.5 H/s
            float maxDist = cfg.rocketMaxDistH * screenH;   // 110% H
            float flightDur = maxDist / rocketSpeed;

            float stagger = cfg.rocketStagger > 0f ? cfg.rocketStagger : 0.04f;
            float minAngle = cfg.fanAngleMin;
            float maxAngle = cfg.fanAngleMax;

            Dictionary<Component, float> damageTracker = new Dictionary<Component, float>();

            // Bắn 16 đầu đạn pháo hoa từ MouthPoint, tỏa hình quạt đều, so le 0.04s
            for (int i = 0; i < rocketCount; i++)
            {
                float angle = Mathf.Lerp(minAngle, maxAngle, (float)i / (rocketCount - 1));
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
                Vector3 currentSpawnPos = mouth != null ? mouth.position : player.transform.position;

                if (pool != null && i < pool.fireworkRockets.Count)
                {
                    var rk = pool.fireworkRockets[i];
                    StartCoroutine(Routine_FireworkRocketFlight(rk, i, currentSpawnPos, dir, flightDur, rocketSpeed, cfg, L, damageTracker, mainCam));
                }

                yield return new WaitForSeconds(stagger);
            }

            // Chờ các đầu đạn cuối nổ xong
            yield return new WaitForSeconds(flightDur);

            averageSkillFPS = frameCount / Mathf.Max(0.001f, unscaledTimer);
            LogSkillTelemetry("Mưa pháo hoa", damageTracker.Count, currentSkillTotalDamage, rocketCount * stagger + flightDur, peakCoverage);
            isSkillActive = false;
            activeSkillRoutine = null;
        }

        private IEnumerator Routine_FireworkRocketFlight(SkillVFXPool.FireworkRocket rk, int index, Vector3 startP, Vector2 dir, float maxDur, float speed, SkillConfig cfg, float L, Dictionary<Component, float> dmgTracker, Camera mainCam)
        {
            if (rk == null || rk.go == null) yield break;

            rk.go.SetActive(true);
            rk.go.transform.position = startP;
            rk.go.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
            rk.go.transform.localScale = new Vector3(cfg.sparkDiameterL * L, cfg.sparkDiameterL * L, 1f);

            if (rk.tr != null)
            {
                rk.tr.Clear();
                rk.tr.enabled = true;
                rk.tr.startWidth = cfg.trailWidthL * L;
            }

            float el = 0f;
            bool exploded = false;
            Vector3 currentPos = startP;
            float hitboxRadius = cfg.rocketRadiusL * L;

            while (el < maxDur && !exploded)
            {
                el += Time.deltaTime;
                currentPos += (Vector3)(dir * speed * Time.deltaTime);
                rk.go.transform.position = currentPos;

                // Kiểm tra va chạm với quái thường: gây 100 dmg cho địch ĐẦU TIÊN chạm phải, KHÔNG AOE
                var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
                foreach (var eb in enemies)
                {
                    if (eb == null || !eb.gameObject.activeInHierarchy || eb.IsDead) continue;
                    if (Vector2.Distance(currentPos, eb.transform.position) <= hitboxRadius + 0.3f)
                    {
                        float curD = dmgTracker.ContainsKey(eb) ? dmgTracker[eb] : 0f;
                        if (curD < cfg.maxDamagePerEnemy) // max 200/địch
                        {
                            float toApply = Mathf.Min(cfg.damage, cfg.maxDamagePerEnemy - curD);
                            eb.TakeDamage(toApply, KillCause.PlayerSkill);
                            dmgTracker[eb] = curD + toApply;
                            currentSkillHits++;
                            currentSkillTotalDamage += toApply;
                        }
                        exploded = true;
                        break;
                    }
                }

                // Kiểm tra va chạm với Boss
                if (!exploded)
                {
                    var bosses = FindObjectsByType<BossController>(FindObjectsSortMode.None);
                    foreach (var boss in bosses)
                    {
                        if (boss == null || !boss.gameObject.activeInHierarchy || boss.IsDead) continue;
                        if (Vector2.Distance(currentPos, boss.transform.position) <= hitboxRadius + 0.8f)
                        {
                            float curD = dmgTracker.ContainsKey(boss) ? dmgTracker[boss] : 0f;
                            if (curD < cfg.bossDamageCap) // max 300/Boss
                            {
                                float toApply = Mathf.Min(cfg.damage, cfg.bossDamageCap - curD);
                                boss.TakeDamage(toApply);
                                dmgTracker[boss] = curD + toApply;
                                currentSkillHits++;
                                currentSkillTotalDamage += toApply;
                            }
                            exploded = true;
                            break;
                        }
                    }
                }

                yield return null;
            }

            if (rk.tr != null) rk.tr.enabled = false;
            rk.go.SetActive(false);

            // Nổ nhỏ tại điểm chạm hoặc cuối tầm
            var pool = SkillVFXPool.Instance;
            if (pool != null && pool.fireworkExplosions.Count > 0)
            {
                int expIdx = index % pool.fireworkExplosions.Count;
                var exp = pool.fireworkExplosions[expIdx];
                StartCoroutine(Routine_PlayExplosion(exp, currentPos, index, cfg, L));
            }
        }

        private IEnumerator Routine_PlayExplosion(SkillVFXPool.FireworkExpItem exp, Vector3 pos, int index, SkillConfig cfg, float L)
        {
            if (exp == null || exp.go == null) yield break;

            // Bỏ ring
            if (exp.ring != null) exp.ring.SetActive(false);

            exp.go.SetActive(true);
            exp.go.transform.position = pos;

            // Kích thước nổ: 1 trong 4 dùng Star (0.9L), còn lại Burst (0.8L)
            bool isStar = (index % 4 == 0);
            float maxDiameter = isStar ? Mathf.Min(cfg.expStarDiameterL, 0.9f) : Mathf.Min(cfg.expBurstDiameterL, 0.8f);
            float targetDiameter = maxDiameter * L;

            // Màu sắc: cứ 5 vụ có 1 vụ hồng nhạt #FF8A7A, còn lại vàng #FFD27A
            Color expColor = (index % 5 == 0) ? new Color(1f, 0.54f, 0.48f, cfg.expAlpha) : new Color(1f, 0.82f, 0.48f, cfg.expAlpha);

            float expDur = cfg.expDur > 0f ? cfg.expDur : 0.35f;
            float el = 0f;

            while (el < expDur)
            {
                el += Time.deltaTime;
                float p = Mathf.Clamp01(el / expDur);
                float curD = Mathf.Lerp(targetDiameter * 0.3f, targetDiameter, p);
                exp.go.transform.localScale = new Vector3(curD, curD, 1f);

                if (exp.sr != null)
                {
                    Color c = expColor;
                    c.a = Mathf.Lerp(cfg.expAlpha, 0f, p);
                    exp.sr.color = c;
                }
                yield return null;
            }

            exp.go.SetActive(false);
        }

        private int ClearBulletsInBox(Vector2 min, Vector2 max)
        {
            int cleared = 0;
            var bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
            foreach (var b in bullets)
            {
                if (b == null || !b.gameObject.activeInHierarchy || b.IsPlayerBullet) continue;
                Vector3 pos = b.transform.position;
                if (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y)
                {
                    b.gameObject.SetActive(false);
                    cleared++;
                }
            }
            return cleared;
        }

        // Dữ liệu vẽ Gizmos
        private float debugAimAngle = 0f;
        private Vector3 debugConeOrigin = Vector3.zero;
        private float debugConeRadius = 0f;
        private float debugConeHalfAngle = 0f;

        private float debugWaveCenterX = 0f;
        private float debugWaveCenterY = 0f;
        private float debugWaveWidth = 0f;
        private float debugWaveHeight = 0f;

        private System.Collections.IEnumerator Routine_GhostFade(SpriteRenderer ghost, Sprite sprite, Vector3 pos, Vector3 scale, float fadeTime)
        {
            if (ghost == null) yield break;
            ghost.sprite = sprite;
            ghost.transform.position = pos;
            ghost.transform.localScale = scale;
            Color c = ghost.color;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(0.5f, 0f, elapsed / fadeTime);
                ghost.color = new Color(c.r, c.g, c.b, a);
                yield return null;
            }
            ghost.gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            if (!ShowSkillGizmos) return;

            // 1. Vẽ quạt lửa Hỏa Lân
            if (debugConeRadius > 0f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
                Vector3 leftDir = Quaternion.Euler(0f, 0f, debugAimAngle - debugConeHalfAngle) * Vector3.up;
                Vector3 rightDir = Quaternion.Euler(0f, 0f, debugAimAngle + debugConeHalfAngle) * Vector3.up;
                Gizmos.DrawRay(debugConeOrigin, leftDir * debugConeRadius);
                Gizmos.DrawRay(debugConeOrigin, rightDir * debugConeRadius);
                Gizmos.DrawLine(debugConeOrigin + leftDir * debugConeRadius, debugConeOrigin + rightDir * debugConeRadius);
            }

            // 2. Vẽ dải sóng Thủy Lân
            if (debugWaveWidth > 0f && debugWaveHeight > 0f)
            {
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.6f);
                Gizmos.DrawWireCube(new Vector3(debugWaveCenterX, debugWaveCenterY, 0f), new Vector3(debugWaveWidth, debugWaveHeight, 1f));
            }
        }

        private void LogSkillTelemetry(string sName, int hits, float totalDmg, float dur, float peakCoverage, int bulletsCleared = 0)
        {
            string lanId = currentConfig != null ? currentConfig.lanId : "1";
            float fps = averageSkillFPS > 0f ? averageSkillFPS : (1.0f / Mathf.Max(0.001f, Time.smoothDeltaTime));
            LastSkillFPS = fps;
            Debug.Log($"[SKILL] Lan={lanId} Skill={sName} Duration={dur:F2}s EnemiesHit={hits} TotalDamage={totalDmg:F0} BulletsCleared={bulletsCleared} PeakCoverage={peakCoverage:F1}% FPS={fps:F1}");
        }

                public void CancelAllSkills()
        {
            StopAllCoroutines();
            activeSkillRoutine = null;

            if (Time.timeScale != 1.0f)
            {
                Time.timeScale = 1.0f;
            }

            if (player != null)
            {
                player.SkillSpeedMultiplier = 1f;
                player.IsKimDashing = false;
                player.SetSkillInvincible(false);
                player.SkillOffsetY = 0f;
            }

            JuiceFx.ResetAll();
            SkillVfxLifetimeGuard.Instance?.DisarmGuard();
            VfxSizeGuard.Instance?.StopMonitoring();
            SkillVFXPool.Instance?.CleanAllVFX();

            isSkillActive = false;
        }
    }
}
