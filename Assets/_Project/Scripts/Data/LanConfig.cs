using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "LanConfig", menuName = "DemHoiDenLong/LanConfig")]
    public class LanConfig : ScriptableObject
    {
        [Header("Identity")]
        public string LanId;
        public string LanName;
        public Sprite LanIcon;

        [Header("Unlock Requirements")]
        public int UnlockCostStar = 0;
        public int UnlockCostDiamond = 0;

        [Header("Base Stats")]
        public int BaseHp = 100;
        public float BaseSpeed = 300f;
        public int BaseDamage = 5;
        public float BaseFireRate = 5f;

        [Header("Skill Information")]
        public string SkillName;
        [TextArea]
        public string SkillDescription;
        public int SkillDamage = 0;
        public float SkillCooldown = 15f;

        [Header("Story Lore — Nhân vật")]
        [Tooltip("Biệt danh dân gian, ví dụ: 'Lân Nhỏ giữ trống'")]
        public string lanNickname;

        [Tooltip("Hồi nào trong cốt truyện Lân này đại diện, ví dụ: HỒI 01 — LÀNG QUÊ")]
        public string storyChapter = "HỒI 01 - NGƯỜI GIỮ ĐÈN";

        [Tooltip("Tiêu đề câu chuyện riêng của Lân")]
        public string storyTitle = "Lời hứa dưới trăng";

        [Tooltip("Cốt truyện chi tiết của nhân vật này (nguồn gốc, tính cách, hành trình)")]
        [TextArea(5, 15)]
        public string storyDescription = "Từng là chú lân nhỏ canh giữ phố đèn...";

        [Tooltip("Vai trò của nhân vật này trong mạch truyện chính (Act nào, Boss nào, ý nghĩa gì)")]
        [TextArea(2, 6)]
        public string lanRole;

        [Tooltip("Tín vật / câu slogan đặc trưng của nhân vật")]
        public string storyToken = "Tín vật: Chuông Nguyệt Quang";
    }
}
