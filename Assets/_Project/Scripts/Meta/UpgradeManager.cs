using UnityEngine;
using System;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Meta
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public event Action<string, UpgradeType, int> OnUpgradeSuccess; // LanId, Type, NewLevel
        public event Action<string> OnLanUnlocked; // LanId

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool CanUpgrade(string lanId, UpgradeType upgradeType, out int cost)
        {
            cost = 0;
            
            if (LanManager.Instance == null || LanManager.Instance.GetGlobalConfig() == null) return false;
            
            var lanSave = LanManager.Instance.GetLanSaveData(lanId);
            if (lanSave == null) return false; // Not unlocked

            int currentLevel = 0;
            switch (upgradeType)
            {
                case UpgradeType.Damage: currentLevel = lanSave.DamageLevel; break;
                case UpgradeType.Hp: currentLevel = lanSave.HpLevel; break;
                case UpgradeType.FireRate: currentLevel = lanSave.FireRateLevel; break;
            }

            cost = LanManager.Instance.GetGlobalConfig().GetUpgradeCost(upgradeType, currentLevel);
            
            if (CurrencyManager.Instance == null) return false;
            
            // All upgrades currently use Star Lanterns (đèn ông sao) based on Excel
            return CurrencyManager.Instance.HasEnoughStarLanterns(cost);
        }

        public bool TryUpgrade(string lanId, UpgradeType upgradeType)
        {
            if (CanUpgrade(lanId, upgradeType, out int cost))
            {
                if (CurrencyManager.Instance.SpendStarLanterns(cost))
                {
                    var lanSave = LanManager.Instance.GetLanSaveData(lanId);
                    int newLevel = 0;

                    switch (upgradeType)
                    {
                        case UpgradeType.Damage: 
                            lanSave.DamageLevel++; 
                            newLevel = lanSave.DamageLevel;
                            break;
                        case UpgradeType.Hp: 
                            lanSave.HpLevel++; 
                            newLevel = lanSave.HpLevel;
                            break;
                        case UpgradeType.FireRate: 
                            lanSave.FireRateLevel++; 
                            newLevel = lanSave.FireRateLevel;
                            break;
                    }

                    SaveManager.Instance.SaveData();
                    OnUpgradeSuccess?.Invoke(lanId, upgradeType, newLevel);
                    return true;
                }
            }
            return false;
        }

        public bool CanUnlockLan(string lanId, out int starCost, out int diamondCost)
        {
            starCost = 0;
            diamondCost = 0;

            if (LanManager.Instance == null || CurrencyManager.Instance == null) return false;
            
            if (LanManager.Instance.IsLanUnlocked(lanId)) return false; // Already unlocked

            var config = LanManager.Instance.GetLanConfig(lanId);
            if (config == null) return false;

            starCost = config.UnlockCostStar;
            diamondCost = config.UnlockCostDiamond;

            bool hasStar = CurrencyManager.Instance.HasEnoughStarLanterns(starCost);
            bool hasDiamond = CurrencyManager.Instance.HasEnoughDiamonds(diamondCost);

            return hasStar && hasDiamond;
        }

        public bool TryUnlockLan(string lanId)
        {
            if (CanUnlockLan(lanId, out int starCost, out int diamondCost))
            {
                if (CurrencyManager.Instance.SpendStarLanterns(starCost) && 
                    CurrencyManager.Instance.SpendDiamonds(diamondCost))
                {
                    // Add to save data
                    var newLan = new LanSaveData(lanId, 1, 1, 1);
                    SaveManager.Instance.CurrentData.UnlockedLans.Add(newLan);
                    SaveManager.Instance.SaveData();
                    
                    OnLanUnlocked?.Invoke(lanId);
                    return true;
                }
            }
            return false;
        }
    }
}
