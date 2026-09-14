using UnityEngine;
using System.Collections.Generic;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Meta
{
    public class LanManager : MonoBehaviour
    {
        public static LanManager Instance { get; private set; }

        [SerializeField] private GlobalUpgradeConfig globalUpgradeConfig;
        [SerializeField] private List<LanConfig> allLans = new List<LanConfig>();

        private Dictionary<string, LanConfig> lanDictionary = new Dictionary<string, LanConfig>();

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

            InitializeDictionary();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeDictionary()
        {
            lanDictionary.Clear();
            foreach (var lan in allLans)
            {
                if (lan != null && !string.IsNullOrEmpty(lan.LanId))
                {
                    lanDictionary[lan.LanId] = lan;
                }
            }
        }

        public IReadOnlyList<LanConfig> GetAllLans() => allLans;

        public LanConfig GetLanConfig(string lanId)
        {
            if (lanDictionary.TryGetValue(lanId, out var config))
            {
                return config;
            }
            Debug.LogError($"LanConfig not found for ID: {lanId}");
            return null;
        }

        public GlobalUpgradeConfig GetGlobalConfig() => globalUpgradeConfig;

        // --- Calculate Stats based on Current Level ---

        public LanSaveData GetLanSaveData(string lanId)
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return null;
            
            // Find in save data
            foreach (var lanSave in SaveManager.Instance.CurrentData.UnlockedLans)
            {
                if (lanSave.LanId == lanId)
                {
                    return lanSave;
                }
            }
            return null;
        }

        public bool IsLanUnlocked(string lanId)
        {
            // Default "Kim Lân" is always unlocked (its cost is 0 in Excel, but it's given by default in SaveManager)
            return GetLanSaveData(lanId) != null;
        }

        public int GetCurrentHp(string lanId)
        {
            var config = GetLanConfig(lanId);
            if (config == null) return 0;
            
            var saveData = GetLanSaveData(lanId);
            int level = saveData != null ? saveData.HpLevel : 0;
            
            int addedHp = globalUpgradeConfig != null ? globalUpgradeConfig.HpPerLevel * level : 0;
            return config.BaseHp + addedHp;
        }

        public int GetCurrentDamage(string lanId)
        {
            var config = GetLanConfig(lanId);
            if (config == null) return 0;

            var saveData = GetLanSaveData(lanId);
            int level = saveData != null ? saveData.DamageLevel : 0;
            
            int addedDamage = globalUpgradeConfig != null ? globalUpgradeConfig.DamagePerLevel * level : 0;
            return config.BaseDamage + addedDamage;
        }

        public float GetCurrentFireRate(string lanId)
        {
            var config = GetLanConfig(lanId);
            if (config == null) return 0;

            var saveData = GetLanSaveData(lanId);
            int level = saveData != null ? saveData.FireRateLevel : 0;
            
            float addedFireRate = globalUpgradeConfig != null ? globalUpgradeConfig.FireRatePerLevel * level : 0;
            return config.BaseFireRate + addedFireRate;
        }
    }
}
