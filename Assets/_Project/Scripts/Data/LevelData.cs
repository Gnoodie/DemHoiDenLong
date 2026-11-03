using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.Data
{
    public enum WinCondition
    {
        KillCount,
        SurviveTime,
        BossKill
    }

    [CreateAssetMenu(fileName = "SO_LevelData", menuName = "DemHoiDenLong/Level Data", order = 3)]
    public class LevelData : ScriptableObject
    {
        [Header("Identity & Progression")]
        public int levelId;             // 1..15 (Toàn game)
        public int actIndex;            // 0..2
        public int indexInAct;          // 1..5
        public string displayName;      // "MÀN {actIndex+1}-{indexInAct}"
        public bool isBoss;
        public string bossName;         // Tên trùm hiển thị
        public string bossEnemyId;      // ID cấu hình của Boss

        [Header("Win Condition & Rules (Phase 3.5)")]
        public WinCondition winCondition;
        public int targetKillCount;
        public float surviveSeconds;
        public int enemySpawnBudget;
        public bool winRuleInitialized;

        [Header("Difficulty Scaling (From GDD Excel)")]
        public float hpMultiplier = 1.0f;
        public float speedMultiplier = 1.0f;
        public float damageMultiplier = 1.0f;
        public int enemyCount = 25;

        [Header("Level Waves Timeline (Gameplay)")]
        public List<WaveData> waves;

        [Header("Legacy Multipliers (WaveManager Compatibility)")]
        public float baseHpMultiplier = 1.0f;
        public float baseSpeedMultiplier = 1.0f;
        public float baseDamageMultiplier = 1.0f;

        [Header("Boss Fight (Gameplay)")]
        public BossController bossPrefab;
        [Tooltip("Time to wait after the last wave completes before spawning the boss")]
        public float bossSpawnDelay = 3.0f;

        [Header("Boss Stats Override (Phase 5A)")]
        public float bossHp = 500f;
        public float bossSpeed = 20f;
        public float bossDamage = 30f;

        /// <summary>
        /// Sao tiền tệ (Star) nhận được cho mốc milestone (1, 2, 3). Thưởng tính từ LevelRewards.
        /// </summary>
        public int GetStarReward(int milestone) => LevelRewards.StarReward(levelId, milestone);

        /// <summary>
        /// Kim Cương (Diamond) nhận được cho mốc milestone (mốc 3 Boss). Thưởng tính từ LevelRewards.
        /// </summary>
        public int GetDiamondReward(int milestone) => LevelRewards.DiamondReward(levelId, milestone);
    }
}
