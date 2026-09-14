using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "GlobalUpgradeConfig", menuName = "DemHoiDenLong/GlobalUpgradeConfig")]
    public class GlobalUpgradeConfig : ScriptableObject
    {
        [Header("Base Upgrade Costs (Star Lanterns)")]
        public int BaseDamageCost = 50;
        public int BaseHpCost = 40;
        public int BaseFireRateCost = 60;

        [Header("Cost Scaling")]
        [Tooltip("Hệ số tăng chi phí mỗi cấp (ví dụ 0.15 nghĩa là mỗi cấp tăng 15% so với chi phí gốc)")]
        public float CostMultiplierPerLevel = 0.15f;

        [Header("Stat Increase Per Level")]
        public int DamagePerLevel = 2;
        public int HpPerLevel = 10;
        public float FireRatePerLevel = 0.2f;

        public int GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            // Level 0 is max level (or level 1 is base). Assuming currentLevel is the number of upgrades done.
            // If currentLevel = 0, cost is BaseCost.
            // Formula: BaseCost + (BaseCost * CostMultiplier * currentLevel)
            
            float baseCost = 0;
            switch (type)
            {
                case UpgradeType.Damage: baseCost = BaseDamageCost; break;
                case UpgradeType.Hp: baseCost = BaseHpCost; break;
                case UpgradeType.FireRate: baseCost = BaseFireRateCost; break;
            }

            return Mathf.RoundToInt(baseCost * (1f + CostMultiplierPerLevel * currentLevel));
        }
    }

    public enum UpgradeType
    {
        Damage,
        Hp,
        FireRate
    }
}
