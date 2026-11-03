using UnityEngine;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Cấu hình dữ liệu Skill cho từng Lân theo GDD:
    /// - Kim Lân: "Lướt nhanh bất tử" (1.5s, hồi 15s, dmg 0, bossDamageCap 0)
    /// - Hỏa Lân: "Phun lửa quét ngang" (1.1s, hồi 20s, dmg 50, bossDamageCap 100)
    /// - Thủy Lân: "Sóng nước đẩy lùi địch" (1.0s, hồi 18s, dmg 30, slow 50% trong 3s, bossDamageCap 60)
    /// - Lân Vàng: "Mưa pháo hoa toàn màn" (1.5s, hồi 25s, dmg 100, max/địch 200, bossDamageCap 300)
    /// </summary>
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "DemHoiDenLong/Skill Config")]
    public class SkillConfig : ScriptableObject
    {
        [Header("Thông tin cơ bản (GDD)")]
        public string skillName = "Kỹ năng";
        public string lanId = "1";
        [TextArea(2, 3)] public string description = "";

        [Header("Chỉ số cốt lõi (GDD)")]
        public float cooldown = 15f;
        public float duration = 1.5f;
        public float damage = 0f;
        public Color primaryColor = Color.white;

        [Header("Cân bằng Boss")]
        [Tooltip("Giới hạn sát thương skill gây lên Boss mỗi lần kích hoạt (ponytail: cần playtest để cân bằng độ khó trùm)")]
        public float bossDamageCap = 0f;

        [Header("Giới hạn kích thước hiệu ứng (GDD 5A.8)")]
        [Tooltip("Bề rộng tối đa (% chiều ngang màn hình W)")]
        public float maxWidthPercentW = 100f;
        [Tooltip("Bề cao tối đa (% chiều dọc màn hình H)")]
        public float maxHeightPercentH = 100f;
        [Tooltip("Diện tích bao phủ tối đa tại một thời điểm (% diện tích màn hình W x H)")]
        public float maxCoveragePercent = 30f;

        [Header("Kim Lân - Lướt nhanh bất tử (GDD 5A.7)")]
        public float dashSpeedMultiplier = 1.8f;
        public float dashPhase1Dur = 0.55f;
        public float dashPhase1OffsetH = 0.40f; // 40% H
        public float dashPhase2Dur = 0.45f;
        public float dashPhase2DriftH = 0.03f;  // thêm 3% H
        public float dashPhase3Dur = 0.50f;     // trở về vị trí ngón tay
        public float ghostInterval = 0.035f;
        public float ghostFadeTime = 0.25f;
        public float ghostAlpha = 0.4f;
        public Color ghostColor = new Color(0.85f, 0.89f, 0.94f, 0.4f); // #D9E4F0
        public float silverStreakWidthL = 0.6f;
        public float silverStreakHeightH = 0.18f;
        public float shieldAuraDiameterL = 1.3f;
        public float shieldAuraAlphaMin = 0.35f;
        public float shieldAuraAlphaMax = 0.6f;
        public float shieldBlinkDur = 0.3f;
        public float ringStartDiameterL = 1.4f;
        public float ringEndDiameterL = 2.2f;
        public float ringExpandDur = 0.25f;
        public float camPunchPercent = 0.03f;

        [Header("Hỏa Lân - Phun lửa phía trước")]
        public float chargeDur = 0.2f;
        public float sweepDur = 3.0f;
        public float fadeDur = 0.2f;
        public float sweepAngleMin = 0f;
        public float sweepAngleMax = 0f;
        public float coneRadiusH = 0.75f;       // 75% H
        public float coneHalfAngle = 22.5f;     // nửa góc 22.5 độ (quạt 45 độ)
        public float damagePerTick = 20f;
        public float tickInterval = 0.1f;
        public float fireDebuffDuration = 0.6f;
        public int fireParticlesPerSec = 140;
        public int maxFireParticles = 200;
        public float particleLifetime = 0.45f;
        public float particleStartSizeL = 1.0f;
        public float particleEndSizeL = 2.2f;
        public float enemyBurnSizeL = 0.6f;
        public float cameraShakeDur = 0.2f;
        public float cameraShakeAmpW = 0.005f;  // 0.5% W

        [Header("Thủy Lân - Sóng nước đẩy lùi (GDD 5A.7)")]
        public float waveTravelDur = 1.2f;
        public float waveStartWidthL = 1.0f;
        public float waveEndWidthW = 1.05f;     // 105% W
        public float waveSpeedMinH = 0.8f;      // 0.8 H/s
        public float waveSpeedMaxH = 1.6f;      // 1.6 H/s
        public float waveScaleYRatio = 0.55f;
        public float hitboxHeightH = 0.12f;     // dải cao 12% H
        public float pushDistanceRatio = 0.20f; // Đẩy lùi 20% H
        public float pushDuration = 0.45f;
        public float slowPercent = 0.5f;        // Làm chậm 50%
        public float slowDuration = 3.0f;
        public float bossSlowPercent = 0.25f;
        public float mouthRingStartL = 1.0f;
        public float mouthRingEndL = 1.8f;
        public float mouthRingDur = 0.2f;
        public float splashInterval = 0.1f;
        public float splashMinL = 0.4f;
        public float splashMaxL = 0.6f;
        public float bubbleSizeL = 0.3f;
        public int bubbleCount = 12;

        [Header("Lân Vàng - Mưa pháo hoa (GDD 5A.7)")]
        public int rocketCount = 16;
        public float rocketStagger = 0.04f;
        public float fanAngleMin = -75f;
        public float fanAngleMax = 75f;
        public float rocketSpeedH = 1.5f;       // 1.5 H/s
        public float rocketMaxDistH = 1.10f;    // 110% H
        public float rocketRadiusL = 0.25f;
        public float sparkDiameterL = 0.3f;
        public float trailLengthH = 0.08f;
        public float trailWidthL = 0.12f;
        public float expBurstDiameterL = 0.8f;
        public float expStarDiameterL = 0.9f;
        public float expDur = 0.35f;
        public float expAlpha = 0.7f;
        public int maxConcurrentExp = 4;
        public float maxDamagePerEnemy = 200f;  // Tối đa 200 mỗi địch
        public float bulletTimeScale = 1.0f;    // KHÔNG làm chậm thời gian
        public float bulletTimeDuration = 0f;

        // Backward compatibility properties
        public float flameSweepDuration => sweepDur;
        public float waveRiseDuration => waveTravelDur;
        public int fireworkCount => rocketCount;
        public float explosionRadius => expBurstDiameterL;

        [Header("Giai đoạn 5A.5: 4 Nhịp & Juice (VFX)")]
        public float chargeDuration = 0.2f;       // Nhịp 1: dồn lực (0.15 - 0.25s)
        public float burstDuration = 1.0f;        // Nhịp 2: bùng nổ
        public float lingerDuration = 0.5f;       // Nhịp 4: tàn dư (0.4 - 0.6s)
        public float screenShakeAmp = 0.15f;      // Biên độ rung camera
        public float screenShakeDur = 0.25f;      // Thời gian rung camera
        public float hitStopDuration = 0.03f;     // Hit stop
        public float screenFlashAlpha = 0.2f;     // Nháy màn hình
        public float vfxScale = 1.0f;             // Kích thước hiệu ứng
        public float vfxIntensity = 1.0f;         // Cường độ hiệu ứng
    }
}
