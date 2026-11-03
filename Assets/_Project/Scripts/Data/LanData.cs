using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "SO_LanData", menuName = "DemHoiDenLong/Lan Data", order = 1)]
    public class LanData : ScriptableObject
    {
        [Header("Basic Info")]
        public string lanId;
        public string lanName;
        public string title => lanName; // Alias to satisfy LanData.title requirement
        public string nickname;
        public Sprite icon;
        public Sprite sprite => icon; // Alias to satisfy LanData.sprite requirement
        public GameObject prefab;

        [Header("Tilt Sprites")]
        public Sprite tiltLeftSprite;
        public Sprite tiltRightSprite;

        [Header("Unlock Cost")]
        public bool isUnlockedByDefault = false;
        public int unlockCostStars;
        public int unlockCostGems;
        public float upgradeCostMultiplier = 1.0f;

        [Header("Base Stats")]
        public float maxHp = 100f;
        public float moveSpeed = 300f; // px/s or scaled world units/s
        public float baseDamage = 5f;
        public float fireRate = 5f; // bullets per second

        [Header("Special Skill")]
        public SkillConfig skillConfig;
        public string skillName;
        public string skillDescription;
        public float skillDamage;
        public float skillCooldown = 10f;
        public Sprite skillIcon;

        [Header("Lore & UI Details")]
        [TextArea(3, 10)]
        public string storyDescription;
        public Color titleColor = Color.white;
        public Color skillTitleColor = Color.cyan;
    }
}
