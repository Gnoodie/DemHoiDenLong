using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "SO_LanData", menuName = "DemHoiDenLong/Lan Data", order = 1)]
    public class LanData : ScriptableObject
    {
        [Header("Basic Info")]
        public string lanId;
        public string lanName;
        public Sprite icon;
        public GameObject prefab;

        [Header("Unlock Cost")]
        public int unlockCostStars;
        public int unlockCostGems;

        [Header("Base Stats")]
        public float maxHp = 100f;
        public float moveSpeed = 300f; // px/s or scaled world units/s
        public float baseDamage = 5f;
        public float fireRate = 5f; // bullets per second

        [Header("Special Skill")]
        public string skillName;
        public float skillDamage;
        public float skillCooldown = 10f;
        public Sprite skillIcon;
    }
}
