namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Tất cả hằng số cân bằng Gameplay giai đoạn 5A gom tại đây (theo GDD Trung_Thu_Shooter_Balance.xlsx).
    /// </summary>
    public static class GameplayBalance
    {
        // =========================================================================
        // 1. CHỈ SỐ VÀ NÂNG CẤP LÂN (Sheet "Nâng cấp & Kinh tế")
        // =========================================================================
        public const float LanHpPerLevel = 10f;             // HP = baseHp + 10 * lvlHP
        public const float LanDamagePerLevel = 2f;          // Damage = baseDamage + 2 * lvlDamage
        public const float LanFireRatePerLevel = 0.2f;      // FireRate = baseFireRate + 0.2 * lvlFireRate
        public const int LanMaxUpgradeLevel = 10;           // Cấp nâng cấp kẹp 0..10

        // =========================================================================
        // 2. CHỈ SỐ TRÙM THEO HỒI (Sheet "Địch - Đèn lồng" & "Độ khó theo màn")
        // Lưu ý: Chỉ số Boss KHÔNG nhân hệ số độ khó của màn.
        // =========================================================================
        // Hồi 1 - Màn 5 (Đèn Tàn Trùm Làng)
        public const float BossAct1Hp = 1800f;
        public const float BossAct1Speed = 20f;
        public const float BossAct1Damage = 30f;

        // Hồi 2 - Màn 10 (Rồng Đèn Phố Cổ)
        public const float BossAct2Hp = 3200f;
        public const float BossAct2Speed = 30f;
        public const float BossAct2Damage = 35f;

        // Hồi 3 - Màn 15 (Thỏ Ngọc Hóa Quái)
        public const float BossAct3Hp = 6500f;
        public const float BossAct3Speed = 25f;
        public const float BossAct3Damage = 40f;

        // =========================================================================
        // 3. THƯỞNG SAO NHẶT TRONG TRẬN (Decision 36)
        // =========================================================================
        public const float VictoryStarsKeptRatio = 1.0f;    // Thắng nhận 100% sao nhặt
        public const float DefeatStarsKeptRatio = 0.5f;     // Thua giữ 50% sao nhặt (làm tròn xuống)

        // =========================================================================
        // 4. LỚP TỐI NỀN CHIẾN ĐẤU (ActData.battleDim)
        // =========================================================================
        public const float DefaultBattleDimAlpha = 0.18f;   // Lớp tối mặc định
        public const float Act2BattleDimAlpha = 0.25f;      // Lớp tối cho Hồi 2

        // =========================================================================
        // 5. CẤU HÌNH SAO RƠI VÀ TRỌNG SỐ XUẤT HIỆN THEO EXCEL
        // Nguồn: Sheet "Địch - Đèn lồng" của Trung_Thu_Shooter_Balance.xlsx
        // =========================================================================
        // Tỉ lệ và hệ số rơi sao
        public const float StarDropChance = 1.0f;           // 100% địch rơi sao (Nguồn: sheet "Địch - Đèn lồng", cột "Đèn ông sao thưởng")
        public const float StarValueMultiplier = 1.0f;       // Hệ số nhân giá trị sao (Nguồn: sheet "Địch - Đèn lồng")

        // Giá trị sao theo từng loại địch (Sheet "Địch - Đèn lồng", cột "Đèn ông sao thưởng")
        public const int StarValuePaper = 1;                // Đèn lồng giấy (thường): 1 sao
        public const int StarValueSpike = 2;                // Đèn lồng gai: 2 sao
        public const int StarValueCarp = 3;                 // Đèn cá chép: 3 sao
        public const int StarValueGhost = 4;                // Đèn ma (đèn quỷ): 4 sao

        // Thưởng sao khi Boss chết (cộng thẳng vào runStars, sheet "Địch - Đèn lồng")
        public const int BossAct1StarReward = 50;           // Boss Màn 5: 50 sao
        public const int BossAct2StarReward = 80;           // Boss Màn 10: 80 sao
        public const int BossAct3StarReward = 150;          // Boss Màn 15: 150 sao

        // Trọng số xuất hiện (Sheet "Địch - Đèn lồng", cột "Tỷ lệ xuất hiện (%)")
        public const float SpawnWeightPaper = 0.50f;        // Đèn giấy: 50% (0.5)
        public const float SpawnWeightSpike = 0.20f;        // Đèn gai: 20% (0.2)
        public const float SpawnWeightCarp = 0.15f;         // Đèn cá chép: 15% (0.15)
        public const float SpawnWeightGhost = 0.10f;        // Đèn ma: 10% (0.1)

        public static int GetBossStarReward(int levelId)
        {
            if (levelId <= 5) return BossAct1StarReward;
            if (levelId <= 10) return BossAct2StarReward;
            return BossAct3StarReward;
        }

        public static int GetEnemyStarValue(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return StarValuePaper;
            string lower = enemyId.ToLower();
            if (lower.Contains("gai") || lower.Contains("spike")) return StarValueSpike;
            if (lower.Contains("ca") || lower.Contains("carp")) return StarValueCarp;
            if (lower.Contains("ma") || lower.Contains("quy") || lower.Contains("ghost")) return StarValueGhost;
            return StarValuePaper;
        }

        public static float GetEnemySpawnWeight(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return SpawnWeightPaper;
            string lower = enemyId.ToLower();
            if (lower.Contains("gai") || lower.Contains("spike")) return SpawnWeightSpike;
            if (lower.Contains("ca") || lower.Contains("carp")) return SpawnWeightCarp;
            if (lower.Contains("ma") || lower.Contains("quy") || lower.Contains("ghost")) return SpawnWeightGhost;
            return SpawnWeightPaper;
        }
    }
}
