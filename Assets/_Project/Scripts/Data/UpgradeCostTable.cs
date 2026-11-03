using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "SO_UpgradeCostTable", menuName = "DemHoiDenLong/Upgrade Cost Table", order = 2)]
    public class UpgradeCostTable : ScriptableObject
    {
        [Header("Upgrade Costs (Index = Current Level - 1)")]
        [Tooltip("Cost to upgrade to the NEXT level")]
        public int[] damageUpgradeCosts = new int[] { 50, 60, 70, 80, 90, 105, 120, 135, 155, 180 };
        public int[] hpUpgradeCosts = new int[] { 40, 50, 55, 65, 70, 85, 95, 110, 125, 145 };
        public int[] fireRateUpgradeCosts = new int[] { 60, 70, 80, 95, 105, 125, 140, 160, 185, 215 };
    }
}
