using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// DỮ LIỆU MẪU: thay bằng LevelProgressManager ở giai đoạn 3-4.
    /// Cung cấp dữ liệu mẫu cho bản đồ chọn màn của từng Hồi.
    /// </summary>
    public class MockLevelDataProvider : MonoBehaviour, ILevelDataProvider
    {
        [Header("Background Sprites")]
        public Sprite bgAct1;
        public Sprite bgAct2;
        public Sprite bgAct3;

        [Header("Boss Sprites")]
        public Sprite bossAct1;
        public Sprite bossAct2;
        public Sprite bossAct3;

        [Header("Node Sprites")]
        public Sprite nodeBase;

        private void Awake()
        {
#if UNITY_EDITOR
            nodeBase = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/LevelSelect/node_base.png");
            bossAct1 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/LevelSelect/node_boss_act1.png");
            bossAct2 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/LevelSelect/node_boss_act2.png");
            bossAct3 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/LevelSelect/node_boss_act3.png");
#endif
        }

        private void Start()
        {
            ValidateLevelsRule();
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của dữ liệu mẫu theo luật chơi:
        /// 1. Không có node mở sau node khóa.
        /// 2. Mỗi Hồi tối đa 1 node mang nhãn 'MỚI' (NodeState.Current).
        /// </summary>
        public void ValidateLevelsRule()
        {
            for (int act = 1; act <= 3; act++)
            {
                var levels = GetLevelsForAct(act);
                if (levels == null || levels.Count == 0) continue;

                bool foundLocked = false;
                int newCount = 0;

                for (int i = 0; i < levels.Count; i++)
                {
                    var node = levels[i];
                    if (node.state == NodeState.Locked)
                    {
                        foundLocked = true;
                    }
                    else if (node.state == NodeState.Unlocked || node.state == NodeState.Current)
                    {
                        if (foundLocked)
                        {
                            Debug.LogWarning($"[MockLevelDataProvider] CẢNH BÁO VI PHẠM LUẬT: Hồi {act} có node {node.levelId} ({node.levelName}) mở sau node khóa!");
                        }
                    }

                    if (node.state == NodeState.Current)
                    {
                        newCount++;
                    }
                }

                if (newCount > 1)
                {
                    Debug.LogWarning($"[MockLevelDataProvider] CẢNH BÁO VI PHẠM LUẬT: Hồi {act} có {newCount} node cùng mang nhãn 'MỚI'!");
                }
            }
        }

        // Bảng màu nhấn của từng Hồi
        private static readonly Color ColorMint = new Color(0.498f, 0.89f, 0.753f, 1f);     // #7FE3C0 (Hồi 1)
        private static readonly Color ColorApricot = new Color(1f, 0.706f, 0.42f, 1f);      // #FFB46B (Hồi 2)
        private static readonly Color ColorLavender = new Color(0.663f, 0.608f, 1f, 1f);    // #A99BFF (Hồi 3)

        public List<LevelNodeData> GetLevelsForAct(int actIndex)
        {
            List<LevelNodeData> list = new List<LevelNodeData>();

            switch (actIndex)
            {
                case 1:
                    list.Add(new LevelNodeData { levelId = 1, levelName = "Màn 1-1", isBoss = false, state = NodeState.Unlocked, stars = 3, claimed = new bool[] { true, true, true }, anchorPos = new Vector2(0.32f, 0.16f) });
                    list.Add(new LevelNodeData { levelId = 2, levelName = "Màn 1-2", isBoss = false, state = NodeState.Unlocked, stars = 3, claimed = new bool[] { true, true, true }, anchorPos = new Vector2(0.68f, 0.32f) });
                    list.Add(new LevelNodeData { levelId = 3, levelName = "Màn 1-3", isBoss = false, state = NodeState.Unlocked, stars = 3, claimed = new bool[] { true, true, true }, anchorPos = new Vector2(0.36f, 0.48f) });
                    list.Add(new LevelNodeData { levelId = 4, levelName = "Màn 1-4", isBoss = false, state = NodeState.Unlocked, stars = 3, claimed = new bool[] { true, true, true }, anchorPos = new Vector2(0.65f, 0.62f) });
                    list.Add(new LevelNodeData { levelId = 5, levelName = "MÀN 1-5 · ĐÈN TÀN TRÙM LÀNG", isBoss = true, state = NodeState.Unlocked, stars = 3, claimed = new bool[] { true, true, true }, anchorPos = new Vector2(0.48f, 0.78f) });
                    break;

                case 2:
                    list.Add(new LevelNodeData { levelId = 6, levelName = "Màn 2-1", isBoss = false, state = NodeState.Unlocked, stars = 3, claimed = new bool[] { true, true, true }, anchorPos = new Vector2(0.42f, 0.15f) });
                    list.Add(new LevelNodeData { levelId = 7, levelName = "Màn 2-2", isBoss = false, state = NodeState.Unlocked, stars = 2, claimed = new bool[] { true, true, false }, anchorPos = new Vector2(0.64f, 0.30f) });
                    list.Add(new LevelNodeData { levelId = 8, levelName = "Màn 2-3", isBoss = false, state = NodeState.Current, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.38f, 0.46f) });
                    list.Add(new LevelNodeData { levelId = 9, levelName = "Màn 2-4", isBoss = false, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.66f, 0.63f) });
                    list.Add(new LevelNodeData { levelId = 10, levelName = "MÀN 2-5 · RỒNG ĐÈN PHỐ CỔ", isBoss = true, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.50f, 0.80f) });
                    break;

                case 3:
                    list.Add(new LevelNodeData { levelId = 11, levelName = "Màn 3-1", isBoss = false, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.50f, 0.14f) });
                    list.Add(new LevelNodeData { levelId = 12, levelName = "Màn 3-2", isBoss = false, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.68f, 0.28f) });
                    list.Add(new LevelNodeData { levelId = 13, levelName = "Màn 3-3", isBoss = false, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.34f, 0.44f) });
                    list.Add(new LevelNodeData { levelId = 14, levelName = "Màn 3-4", isBoss = false, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.64f, 0.60f) });
                    list.Add(new LevelNodeData { levelId = 15, levelName = "MÀN 3-5 · THỎ NGỌC HÓA QUÁI", isBoss = true, state = NodeState.Locked, stars = 0, claimed = new bool[] { false, false, false }, anchorPos = new Vector2(0.50f, 0.78f) });
                    break;
            }

            return list;
        }

        public int GetActStars(int actIndex)
        {
            var levels = GetLevelsForAct(actIndex);
            int total = 0;
            if (levels != null)
            {
                foreach (var l in levels) total += l.stars;
            }
            return total;
        }

        public int GetActMaxStars(int actIndex) => 15;

        public string GetActTitle(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return "HỒI 1 - LÀNG QUÊ THANH BÌNH";
                case 2: return "HỒI 2 - PHỐ CỔ ĐÈN LỒNG";
                case 3: return "HỒI 3 - CUNG TRĂNG HUYỀN ẢO";
                default: return "HỒI 2 - PHỐ CỔ ĐÈN LỒNG";
            }
        }

        public string GetActSubtitle(int actIndex) => "BẢN ĐỒ VƯỢT ẢI";

        public Color GetActAccentColor(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return ColorMint;
                case 2: return ColorApricot;
                case 3: return ColorLavender;
                default: return ColorApricot;
            }
        }

        public Sprite GetActBackground(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return bgAct1;
                case 2: return bgAct2;
                case 3: return bgAct3;
                default: return bgAct2;
            }
        }

        public string GetBossName(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return "Đèn Tàn Trùm Làng";
                case 2: return "Rồng Đèn Phố Cổ";
                case 3: return "Thỏ Ngọc Hóa Quái";
                default: return "Rồng Đèn Phố Cổ";
            }
        }

        public Sprite GetBossSprite(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return bossAct1;
                case 2: return bossAct2;
                case 3: return bossAct3;
                default: return bossAct2;
            }
        }

        public Sprite GetNodeBaseSprite() => nodeBase;

        public ActStatus GetActStatus(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return ActStatus.Completed;
                case 2: return ActStatus.InProgress;
                case 3: return ActStatus.Locked;
                default: return ActStatus.InProgress;
            }
        }

        public string GetUnlockHint(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return string.Empty;
                case 2: return "Đánh bại trùm Hồi 1 để mở khóa";
                case 3: return "Đánh bại trùm Hồi 2 để mở khóa";
                default: return string.Empty;
            }
        }

        public int GetTotalStars() => GetActStars(1) + GetActStars(2) + GetActStars(3);

        public int GetMaxTotalStars() => 45;

        // Mở rộng cho Giai đoạn 4
        public int GetActCount() => 3;

        public Sprite GetActBanner(int actIndex)
        {
#if UNITY_EDITOR
            switch (actIndex)
            {
                case 1: return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_village_map_ngang.png");
                case 2: return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_oldtown_map_ngang.png");
                case 3: return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_moon_map_ngang.png");
            }
#endif
            return null;
        }

        public int GetActClearedLevels(int actIndex)
        {
            switch (actIndex)
            {
                case 1: return 5;
                case 2: return 2;
                case 3: return 0;
                default: return 0;
            }
        }

        public int GetActLevelCount(int actIndex) => 5;

        public int GetCurrentLevelId() => 8;

#pragma warning disable 0067
        public event System.Action OnDataChanged;
#pragma warning restore 0067
    }
}
