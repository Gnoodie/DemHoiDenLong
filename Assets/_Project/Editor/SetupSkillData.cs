using System.IO;
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public static class SetupSkillData
    {
        private const string SkillsDir = "Assets/_Project/Data/Skills";

        [MenuItem("DemHoiDenLong/Setup Skill Data")]
        public static void SetupAll()
        {
            if (!Directory.Exists(SkillsDir))
            {
                Directory.CreateDirectory(SkillsDir);
            }

            Debug.Log("[SetupSkillData] Khởi tạo 4 SkillConfig theo GDD...");

            // 1. Kim Lân
            SkillConfig kimSkill = GetOrCreateSkill("Skill_KimLan", "1", "Lướt nhanh bất tử", 15f, 1.5f, 0f, 0f, 
                new Color(0.85f, 0.89f, 0.94f, 1f)); // #D9E4F0
            kimSkill.dashSpeedMultiplier = 1.8f;
            kimSkill.dashPhase1Dur = 0.55f;
            kimSkill.dashPhase1OffsetH = 0.40f;
            kimSkill.dashPhase2Dur = 0.45f;
            kimSkill.dashPhase2DriftH = 0.03f;
            kimSkill.dashPhase3Dur = 0.50f;
            kimSkill.ghostInterval = 0.035f;
            kimSkill.ghostFadeTime = 0.25f;
            kimSkill.ghostAlpha = 0.4f;
            kimSkill.silverStreakWidthL = 0.6f;
            kimSkill.silverStreakHeightH = 0.18f;
            kimSkill.shieldAuraDiameterL = 1.3f;
            kimSkill.shieldAuraAlphaMin = 0.35f;
            kimSkill.shieldAuraAlphaMax = 0.6f;
            kimSkill.shieldBlinkDur = 0.3f;
            kimSkill.ringStartDiameterL = 1.4f;
            kimSkill.ringEndDiameterL = 2.2f;
            kimSkill.ringExpandDur = 0.25f;
            kimSkill.camPunchPercent = 0.03f;
            kimSkill.maxWidthPercentW = 22f;
            kimSkill.maxHeightPercentH = 25f;
            kimSkill.maxCoveragePercent = 30f;
            kimSkill.screenShakeAmp = 0f;
            kimSkill.screenFlashAlpha = 0f;
            EditorUtility.SetDirty(kimSkill);

            // 2. Hỏa Lân
            SkillConfig hoaSkill = GetOrCreateSkill("Skill_HoaLan", "2", "Phun lửa phía trước", 20f, 3.2f, 50f, 100f, 
                new Color(1.0f, 0.54f, 0.24f, 1f)); // #FF8A3D
            hoaSkill.chargeDur = 0.2f;
            hoaSkill.sweepDur = 3.0f;
            hoaSkill.fadeDur = 0.2f;
            hoaSkill.sweepAngleMin = 0f;
            hoaSkill.sweepAngleMax = 0f;
            hoaSkill.coneRadiusH = 0.75f;
            hoaSkill.coneHalfAngle = 22.5f;
            hoaSkill.damagePerTick = 20f;
            hoaSkill.tickInterval = 0.1f;
            hoaSkill.fireDebuffDuration = 0.6f;
            hoaSkill.fireParticlesPerSec = 140;
            hoaSkill.maxFireParticles = 200;
            hoaSkill.particleLifetime = 0.45f;
            hoaSkill.particleStartSizeL = 1.0f;
            hoaSkill.particleEndSizeL = 2.2f;
            hoaSkill.enemyBurnSizeL = 0.6f;
            hoaSkill.cameraShakeDur = 0.2f;
            hoaSkill.cameraShakeAmpW = 0.005f;
            hoaSkill.maxWidthPercentW = 100f;
            hoaSkill.maxHeightPercentH = 80f;
            hoaSkill.maxCoveragePercent = 45f;
            hoaSkill.screenShakeAmp = 0.05f;
            hoaSkill.screenFlashAlpha = 0f;
            EditorUtility.SetDirty(hoaSkill);

            // 3. Thủy Lân
            SkillConfig thuySkill = GetOrCreateSkill("Skill_ThuyLan", "3", "Sóng nước đẩy lùi địch", 18f, 1.2f, 30f, 60f, 
                new Color(0.50f, 0.91f, 0.96f, 1f)); // #7FE8F5
            thuySkill.waveTravelDur = 1.2f;
            thuySkill.waveStartWidthL = 1.0f;
            thuySkill.waveEndWidthW = 1.05f;
            thuySkill.waveSpeedMinH = 0.8f;
            thuySkill.waveSpeedMaxH = 1.6f;
            thuySkill.waveScaleYRatio = 0.55f;
            thuySkill.hitboxHeightH = 0.12f;
            thuySkill.pushDistanceRatio = 0.20f;
            thuySkill.pushDuration = 0.45f;
            thuySkill.slowPercent = 0.5f;
            thuySkill.slowDuration = 3.0f;
            thuySkill.bossSlowPercent = 0.25f;
            thuySkill.mouthRingStartL = 1.0f;
            thuySkill.mouthRingEndL = 1.8f;
            thuySkill.mouthRingDur = 0.2f;
            thuySkill.splashInterval = 0.1f;
            thuySkill.splashMinL = 0.4f;
            thuySkill.splashMaxL = 0.6f;
            thuySkill.bubbleSizeL = 0.3f;
            thuySkill.bubbleCount = 12;
            thuySkill.maxWidthPercentW = 105f;
            thuySkill.maxHeightPercentH = 14f;
            thuySkill.maxCoveragePercent = 30f;
            thuySkill.screenShakeAmp = 0f;
            thuySkill.screenFlashAlpha = 0f;
            EditorUtility.SetDirty(thuySkill);

            // 4. Lân Vàng
            SkillConfig vangSkill = GetOrCreateSkill("Skill_LanVang", "4", "Mưa pháo hoa toàn màn", 25f, 1.5f, 100f, 300f, 
                new Color(1.0f, 0.84f, 0.0f, 1f)); // #FFD700
            vangSkill.rocketCount = 16;
            vangSkill.rocketStagger = 0.04f;
            vangSkill.fanAngleMin = -75f;
            vangSkill.fanAngleMax = 75f;
            vangSkill.rocketSpeedH = 1.5f;
            vangSkill.rocketMaxDistH = 1.10f;
            vangSkill.rocketRadiusL = 0.25f;
            vangSkill.sparkDiameterL = 0.3f;
            vangSkill.trailLengthH = 0.08f;
            vangSkill.trailWidthL = 0.05f; // Giảm còn ~40% (0.12 -> 0.05) theo 5A.8
            vangSkill.expBurstDiameterL = 0.8f;
            vangSkill.expStarDiameterL = 0.9f;
            vangSkill.expDur = 0.35f;
            vangSkill.expAlpha = 0.7f;
            vangSkill.maxConcurrentExp = 4;
            vangSkill.maxDamagePerEnemy = 200f;
            vangSkill.maxWidthPercentW = 105f;
            vangSkill.maxHeightPercentH = 105f;
            vangSkill.maxCoveragePercent = 30f;
            vangSkill.bulletTimeScale = 1.0f;
            vangSkill.bulletTimeDuration = 0f;
            vangSkill.screenShakeAmp = 0f;
            vangSkill.screenFlashAlpha = 0f;
            EditorUtility.SetDirty(vangSkill);
            // Nạp và liên kết vào 4 LanData
            LinkLanData("Assets/_Project/Data/Lans/1.asset", kimSkill, "icon_skill_kim");
            LinkLanData("Assets/_Project/Data/Lans/2.asset", hoaSkill, "icon_skill_hoa");
            LinkLanData("Assets/_Project/Data/Lans/3.asset", thuySkill, "icon_skill_thuy");
            LinkLanData("Assets/_Project/Data/Lans/4.asset", vangSkill, "icon_skill_vang");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SetupSkillData] Hoàn tất thiết lập 4 SkillConfig và gán vào LanData!");
        }

        private static SkillConfig GetOrCreateSkill(string assetName, string lanId, string sName, float cd, float dur, float dmg, float bossCap, Color col)
        {
            string path = $"{SkillsDir}/{assetName}.asset";
            SkillConfig config = AssetDatabase.LoadAssetAtPath<SkillConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SkillConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.lanId = lanId;
            config.skillName = sName;
            config.cooldown = cd;
            config.duration = dur;
            config.damage = dmg;
            config.bossDamageCap = bossCap;
            config.primaryColor = col;

            return config;
        }

        private static void LinkLanData(string lanPath, SkillConfig skill, string iconName)
        {
            LanData lan = AssetDatabase.LoadAssetAtPath<LanData>(lanPath);
            if (lan == null) return;

            lan.skillConfig = skill;
            lan.skillName = skill.skillName;
            lan.skillDamage = skill.damage;
            lan.skillCooldown = skill.cooldown;

            Sprite icon = Resources.Load<Sprite>($"Icons/{iconName}");
            if (icon == null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/Icons/{iconName}.png");
            }
            if (icon != null) lan.skillIcon = icon;

            EditorUtility.SetDirty(lan);
        }
    }
}
