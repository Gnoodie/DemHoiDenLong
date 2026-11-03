using UnityEngine;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Nguồn tính toán luật chơi và chỉ số sinh tồn của người chơi theo cấp độ.
    /// </summary>
    public static class LevelRules
    {
        /// <summary>
        /// Số trái tim (mạng) tối đa mặc định của người chơi.
        /// </summary>
        public const int MaxHearts = 3;

        /// <summary>
        /// Tính phần trăm tổng máu còn lại gộp cả thanh máu hiện tại và số mạng (trái tim).
        /// Quy ước: mỗi trái tim là MỘT thanh máu đầy. Tổng máu = máu tối đa × số tim tối đa.
        /// heartsLeft TÍNH CẢ tim đang dùng (bắt đầu = 3, currentHp = maxHp, kết quả 1.0).
        /// Máu tối đa là máu ĐÃ cộng nâng cấp (Gameplay truyền vào).
        /// Lưu ý: Trái tim không phạt riêng, ảnh hưởng số sao thông qua % tổng máu còn lại.
        /// Xử lý an toàn: maxHp <= 0, maxHearts <= 0 hoặc heartsLeft < 1 thì trả 0, không exception.
        /// </summary>
        public static float CalcHpRemaining01(float currentHp, float maxHp, int heartsLeft, int maxHearts = MaxHearts)
        {
            if (maxHp <= 0f || maxHearts <= 0 || heartsLeft < 1) return 0f;

            float totalMaxHp = maxHp * maxHearts;
            float totalCurrentHp = Mathf.Max(0f, currentHp) + (heartsLeft - 1) * maxHp;
            return Mathf.Clamp01(totalCurrentHp / totalMaxHp);
        }
    }
}
