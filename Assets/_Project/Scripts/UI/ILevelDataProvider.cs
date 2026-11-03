using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    public enum NodeState
    {
        Locked,
        Unlocked,
        Current
    }

    public enum MilestoneState
    {
        Unachieved, // Chưa đạt sao (icon khóa xám #8E98BC, chữ CHƯA ĐẠT)
        Pending,    // Đã đạt sao nhưng chưa nhận (tag amber #F59E42, chữ CHỜ NHẬN)
        Claimed     // Đã nhận quà (tag xanh #2E9E7A, icon ui_check, chữ ĐÃ NHẬN)
    }

    [System.Serializable]
    public struct LevelNodeData
    {
        public int levelId;
        public string levelName;
        public bool isBoss;
        public NodeState state;
        public int stars;           // 0 đến 3 (kỷ lục đạt được)
        public bool[] claimed;      // độ dài 3, true nếu đã nhận quà mốc tương ứng
        public Vector2 anchorPos;   // Tọa độ tỉ lệ (fx, fy) so với BackgroundRoot

        // Mở rộng cho Giai đoạn 4
        public int actIndex;
        public int indexInAct;
        public string displayName;  // "MÀN 2-3"
        public string bossName;
        public int maxStars;
        public MilestoneState[] milestoneStates; // độ dài 3

        // Mở rộng cho Giai đoạn 3.5 (Luật thắng màn chơi)
        public WinCondition winCondition;
        public int targetKillCount;
        public float surviveSeconds;
    }

    public interface ILevelDataProvider
    {
        List<LevelNodeData> GetLevelsForAct(int actIndex);
        int GetActStars(int actIndex);
        int GetActMaxStars(int actIndex);
        string GetActTitle(int actIndex);
        string GetActSubtitle(int actIndex);
        Color GetActAccentColor(int actIndex);
        Sprite GetActBackground(int actIndex);
        string GetBossName(int actIndex);
        Sprite GetBossSprite(int actIndex);
        Sprite GetNodeBaseSprite();

        // Mở rộng thêm cho giai đoạn 3-4 (tiến độ Hồi và toàn game)
        ActStatus GetActStatus(int actIndex);
        string GetUnlockHint(int actIndex);
        int GetTotalStars();
        int GetMaxTotalStars();

        // Mở rộng cho Giai đoạn 4 (UI ActSelectView & LevelDetailView)
        int GetActCount();
        Sprite GetActBanner(int actIndex);
        int GetActClearedLevels(int actIndex);
        int GetActLevelCount(int actIndex);
        int GetCurrentLevelId();
        event System.Action OnDataChanged;
    }
}
