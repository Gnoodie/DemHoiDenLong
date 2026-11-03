using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "SO_ActData", menuName = "DemHoiDenLong/Act Data", order = 2)]
    public class ActData : ScriptableObject
    {
        [Header("Act Identity")]
        public int actIndex;            // 0..2
        public string title;            // "HỒI 1 - LÀNG QUÊ THANH BÌNH"
        public string subtitle;         // "KHỞI ĐẦU ĐÊM RẰM"
        public string unlockHint;       // "" hoặc "Đánh bại trùm Hồi 1 để mở khóa"
        public Color accentColor = Color.white;

        [Header("Visual Assets")]
        public Sprite banner;           // Banner ngang hiển thị trên ActCard
        public Sprite mapBackground;    // Nền dọc bản đồ Hồi trong LevelDetailView
        public Sprite battleBackground; // Nền chiến đấu Gameplay cho Hồi này (Phase 3.5, gán tay)
        public Sprite bossSprite;       // Ảnh Boss của Hồi dùng cho Node Boss và UI (Giai đoạn 5A.8)
        [Range(0f, 1f)]
        public float battleDim = 0.18f; // Độ tối của lớp phủ nền (Hồi 1: 0.18, Hồi 2: 0.25)

        [Header("Enemies In Act")]
        public string[] enemyTypeIds;

        [Header("Levels (Sequential)")]
        public List<LevelData> levels = new List<LevelData>();

        public int LevelCount => levels != null ? levels.Count : 0;
    }
}
