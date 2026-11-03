using System.Collections.Generic;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Nguồn sự thật duy nhất (single source of truth) về công thức tính quà và sao đánh giá màn chơi.
    /// Phân biệt rõ:
    /// - LevelStars: sao đánh giá màn chơi (0-3 sao).
    /// - Star: sao tiền tệ (Economy currency).
    /// </summary>
    public static class LevelRewards
    {
        // Hằng số tính sao tiền tệ (Currency Stars): Base + level * Multiplier
        public const int BaseStarReward = 20;
        public const int StarMultiplierM1 = 4;
        public const int StarMultiplierM2 = 6;
        public const int StarMultiplierM3 = 8;

        // Ngưỡng tính LevelStars theo HP còn lại (0.0 đến 1.0)
        public const float HpThresholdStar3 = 0.85f;
        public const float HpThresholdStar2 = 0.50f;
        // Epsilon 5e-5 để phân biệt chính xác giữa 0.85f và 0.8499f (ngưỡng Ca 5) đồng thời đạt đúng các biên 270 HP (Ca 24)
        public const float FloatEpsilon = 0.00005f;

        // Bảng phần thưởng Kim Cương (chỉ áp dụng mốc 3 của Boss màn 5, 10, 15)
        private static readonly Dictionary<int, int> BossDiamondRewards = new Dictionary<int, int>
        {
            { 5, 5 },
            { 10, 10 },
            { 15, 15 }
            // { 20, 30 } // Boss màn 20 Hồi 4: 30 Kim Cương (dành cho Hồi 4 sau này, hiện chưa kích hoạt)
        };

        /// <summary>
        /// Tính LevelStars (0-3 sao) dựa trên kết quả thắng/thua, tỉ lệ HP còn lại, và cờ đã dùng hồi sinh quảng cáo.
        /// Thua -> 0 sao.
        /// Thắng có dùng hồi sinh (usedRevive = true) -> tối đa 1 sao (cap).
        /// Thắng bình thường: HP >= 85% -> 3 sao; HP >= 50% -> 2 sao; khác -> 1 sao.
        /// </summary>
        public static int CalcStars(bool won, float hpRemaining01, bool usedRevive)
        {
            if (!won) return 0;
            if (usedRevive) return 1;

            if (hpRemaining01 >= HpThresholdStar3 - FloatEpsilon)
                return 3;
            if (hpRemaining01 >= HpThresholdStar2 - FloatEpsilon)
                return 2;
            return 1;
        }

        /// <summary>
        /// Tương thích ngược: mặc định không dùng hồi sinh (usedRevive = false).
        /// </summary>
        public static int CalcStars(bool won, float hpRemaining01) => CalcStars(won, hpRemaining01, false);

        /// <summary>
        /// Tính lượng Sao tiền tệ (Star) nhận được cho mốc milestone (1, 2, 3) của màn levelId.
        /// </summary>
        public static int StarReward(int levelId, int milestone)
        {
            if (levelId <= 0) return 0;

            switch (milestone)
            {
                case 1:
                    return BaseStarReward + levelId * StarMultiplierM1;
                case 2:
                    return BaseStarReward + levelId * StarMultiplierM2;
                case 3:
                    return BaseStarReward + levelId * StarMultiplierM3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Tính lượng Kim Cương (Diamond) nhận được cho mốc milestone (chỉ mốc 3 của Boss 5, 10, 15).
        /// </summary>
        public static int DiamondReward(int levelId, int milestone)
        {
            if (milestone != 3) return 0;

            if (BossDiamondRewards.TryGetValue(levelId, out int diamonds))
            {
                return diamonds;
            }
            return 0;
        }

        /// <summary>
        /// Tính tổng phần thưởng nhận lần đầu khi vượt qua toàn bộ các màn 3 sao.
        /// Với 15 màn: kỳ vọng 3060 Sao và 30 Kim Cương.
        /// </summary>
        public static (int totalStars, int totalDiamonds) GetTotalFirstClearRewards(int totalLevels = 15)
        {
            int stars = 0;
            int diamonds = 0;

            for (int lvl = 1; lvl <= totalLevels; lvl++)
            {
                for (int m = 1; m <= 3; m++)
                {
                    stars += StarReward(lvl, m);
                    diamonds += DiamondReward(lvl, m);
                }
            }

            return (stars, diamonds);
        }

        // Tương thích ngược với các caller hiện có
        public static int GetStarReward(int levelId, int milestone) => StarReward(levelId, milestone);
        public static int GetDiamondReward(int levelId, int milestone) => DiamondReward(levelId, milestone);

        public static string GetConditionText(int milestone)
        {
            switch (milestone)
            {
                case 1:
                    return "Vượt qua màn chơi";
                case 2:
                    return "Tổng máu còn lại ≥ 50%";
                case 3:
                    return "Tổng máu còn lại ≥ 85%";
                default:
                    return string.Empty;
            }
        }

        public static float CalcHpRemaining01(float currentHp, float maxHp, int heartsLeft, int maxHearts = DemHoiDenLong.Data.LevelRules.MaxHearts)
            => DemHoiDenLong.Data.LevelRules.CalcHpRemaining01(currentHp, maxHp, heartsLeft, maxHearts);
    }
}
