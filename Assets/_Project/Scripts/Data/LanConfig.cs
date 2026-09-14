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
    }
}
