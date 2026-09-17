using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Meta;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    public class HangarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private LanSelectionItemUI itemPrefab;

        [Header("Details Panel")]
        [SerializeField] private TextMeshProUGUI lanNameText;
        [SerializeField] private Image lanIconLarge;
        [SerializeField] private TextMeshProUGUI skillDescText;
        
        [Header("Skill/Story UI")]
        [SerializeField] private TextMeshProUGUI storyChapterText;
        [SerializeField] private TextMeshProUGUI storyTitleText;
        [SerializeField] private TextMeshProUGUI storyDescriptionText;
        [SerializeField] private TextMeshProUGUI storyTokenText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI fireRateText;

        [Header("Upgrade Buttons")]
        [SerializeField] private Button upgradeHpBtn;
        [SerializeField] private TextMeshProUGUI upgradeHpCostText;
        
        [SerializeField] private Button upgradeDmgBtn;
        [SerializeField] private TextMeshProUGUI upgradeDmgCostText;
        
        [SerializeField] private Button upgradeFrBtn;
        [SerializeField] private TextMeshProUGUI upgradeFrCostText;

        [Header("Unlock Panel")]
        [SerializeField] private GameObject unlockPanel; // Shown if Lan is locked
        [SerializeField] private Button unlockBtn;
        [SerializeField] private TextMeshProUGUI unlockCostText;

        private LanConfig currentSelectedLan;
        private List<LanSelectionItemUI> spawnedItems = new List<LanSelectionItemUI>();

        private void Start()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradeSuccess += HandleUpgradeSuccess;
                UpgradeManager.Instance.OnLanUnlocked += HandleLanUnlocked;
            }

            PopulateList();

            // Select first by default
            if (LanManager.Instance != null && LanManager.Instance.GetAllLans().Count > 0)
            {
                SelectLan(LanManager.Instance.GetAllLans()[0]);
            }

            // Hook up buttons
            upgradeHpBtn.onClick.AddListener(() => TryUpgrade(UpgradeType.Hp));
            upgradeDmgBtn.onClick.AddListener(() => TryUpgrade(UpgradeType.Damage));
            upgradeFrBtn.onClick.AddListener(() => TryUpgrade(UpgradeType.FireRate));
            unlockBtn.onClick.AddListener(TryUnlock);
        }

        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradeSuccess -= HandleUpgradeSuccess;
                UpgradeManager.Instance.OnLanUnlocked -= HandleLanUnlocked;
            }
        }

        private void PopulateList()
        {
            if (LanManager.Instance == null) return;

            foreach (var lan in LanManager.Instance.GetAllLans())
            {
                var item = Instantiate(itemPrefab, listContainer);
                bool isUnlocked = LanManager.Instance.IsLanUnlocked(lan.LanId);
                item.Initialize(lan, isUnlocked, this);
                spawnedItems.Add(item);
            }
        }

        public void SelectLan(LanConfig config)
        {
            currentSelectedLan = config;
            UpdateDetailsPanel();
        }

        private void UpdateDetailsPanel()
        {
            if (currentSelectedLan == null || LanManager.Instance == null) return;

            lanNameText.text = currentSelectedLan.LanName;
            lanIconLarge.sprite = currentSelectedLan.LanIcon;
            if (skillDescText != null) skillDescText.text = $"Kỹ năng: {currentSelectedLan.SkillName} — {currentSelectedLan.SkillDescription}";
            
            if (storyChapterText != null) storyChapterText.text = currentSelectedLan.storyChapter;
            if (storyTitleText != null) storyTitleText.text = currentSelectedLan.storyTitle;
            if (storyDescriptionText != null) storyDescriptionText.text = currentSelectedLan.storyDescription;
            if (storyTokenText != null) storyTokenText.text = currentSelectedLan.storyToken;

            bool isUnlocked = LanManager.Instance.IsLanUnlocked(currentSelectedLan.LanId);

            if (isUnlocked)
            {
                unlockPanel.SetActive(false);
                
                // Show current stats
                hpText.text = $"HP: {LanManager.Instance.GetCurrentHp(currentSelectedLan.LanId)}";
                damageText.text = $"DMG: {LanManager.Instance.GetCurrentDamage(currentSelectedLan.LanId)}";
                fireRateText.text = $"FR: {LanManager.Instance.GetCurrentFireRate(currentSelectedLan.LanId):F1}";

                // Update Upgrade Costs & Buttons
                UpdateUpgradeButton(UpgradeType.Hp, upgradeHpBtn, upgradeHpCostText);
                UpdateUpgradeButton(UpgradeType.Damage, upgradeDmgBtn, upgradeDmgCostText);
                UpdateUpgradeButton(UpgradeType.FireRate, upgradeFrBtn, upgradeFrCostText);
                
                upgradeHpBtn.gameObject.SetActive(true);
                upgradeDmgBtn.gameObject.SetActive(true);
                upgradeFrBtn.gameObject.SetActive(true);
            }
            else
            {
                unlockPanel.SetActive(true);
                
                // Show base stats
                hpText.text = $"HP: {currentSelectedLan.BaseHp}";
                damageText.text = $"DMG: {currentSelectedLan.BaseDamage}";
                fireRateText.text = $"FR: {currentSelectedLan.BaseFireRate:F1}";

                // Hide upgrade buttons
                upgradeHpBtn.gameObject.SetActive(false);
                upgradeDmgBtn.gameObject.SetActive(false);
                upgradeFrBtn.gameObject.SetActive(false);

                // Setup unlock button
                if (UpgradeManager.Instance != null && UpgradeManager.Instance.CanUnlockLan(currentSelectedLan.LanId, out int starCost, out int diaCost))
                {
                    unlockBtn.interactable = true;
                }
                else
                {
                    unlockBtn.interactable = false;
                }
                
                string costStr = "";
                if (currentSelectedLan.UnlockCostStar > 0) costStr += $"{currentSelectedLan.UnlockCostStar} Star ";
                if (currentSelectedLan.UnlockCostDiamond > 0) costStr += $"{currentSelectedLan.UnlockCostDiamond} Dia";
                unlockCostText.text = costStr.Trim();
            }
        }

        private void UpdateUpgradeButton(UpgradeType type, Button btn, TextMeshProUGUI costText)
        {
            int cost = 0;
            bool canUpgrade = false;
            
            if (UpgradeManager.Instance != null)
            {
                canUpgrade = UpgradeManager.Instance.CanUpgrade(currentSelectedLan.LanId, type, out cost);
            }

            if (canUpgrade)
            {
                btn.interactable = true;
                costText.text = cost.ToString();
            }
            else
            {
                btn.interactable = false;
                if (UpgradeManager.Instance != null)
                {
                    costText.text = cost.ToString(); 
                }
            }
        }

        private void TryUpgrade(UpgradeType type)
        {
            if (currentSelectedLan == null || UpgradeManager.Instance == null) return;
            UpgradeManager.Instance.TryUpgrade(currentSelectedLan.LanId, type);
        }

        private void TryUnlock()
        {
            if (currentSelectedLan == null || UpgradeManager.Instance == null) return;
            UpgradeManager.Instance.TryUnlockLan(currentSelectedLan.LanId);
        }

        private void HandleUpgradeSuccess(string lanId, UpgradeType type, int newLevel)
        {
            if (currentSelectedLan != null && currentSelectedLan.LanId == lanId)
            {
                UpdateDetailsPanel();
            }
        }

        private void HandleLanUnlocked(string lanId)
        {
            // Update the list icon lock state
            foreach(var item in spawnedItems)
            {
                if (item.LanId == lanId)
                {
                    item.UpdateUnlockState(true);
                    break;
                }
            }

            if (currentSelectedLan != null && currentSelectedLan.LanId == lanId)
            {
                UpdateDetailsPanel();
            }
        }
    }
}
