using System.Collections.Generic;

using System.Collections;

using UnityEngine;

using UnityEngine.UI;

using TMPro;

using DemHoiDenLong.Data;

using DemHoiDenLong.Gameplay;

using DemHoiDenLong.Audio;

using UnityEngine.SceneManagement;



namespace DemHoiDenLong.UI

{

    public class HangarUI : MonoBehaviour

    {

        [Header("Data References")]

        [SerializeField] private List<LanData> lanDatabase; // Ensure to assign the 4 LanData assets here in the correct order (1, 2, 3, 4)



        [Header("Main Menu Controls")]

        [SerializeField] private Button playBtn;

        [SerializeField] private Button nextLanBtn;

        [SerializeField] private Button prevLanBtn;

        [SerializeField] private Button settingsBtn;



        [Header("Unlock Controls")]

        [SerializeField] private Button unlockBtn;

        [SerializeField] private TextMeshProUGUI unlockCostText;

        [SerializeField] private Image unlockCurrencyIcon;

        [SerializeField] private Sprite starSprite;

        [SerializeField] private Sprite diamondSprite;



        [Header("Economy UI")]

        [SerializeField] private TextMeshProUGUI starAmountText;

        [SerializeField] private TextMeshProUGUI diamondAmountText;



        [Header("Toast Notification")]

        [SerializeField] private GameObject toastPanel;

        [SerializeField] private TextMeshProUGUI toastText;

        [SerializeField] private Image toastBg; // to change color based on success/fail

        private Coroutine toastCoroutine;



        [Header("Character Showcase")]

        [SerializeField] private TextMeshProUGUI lanNameText;

        [SerializeField] private TextMeshProUGUI lanNameShadowText; // Object Shadow đè dưới chữ chính

        [SerializeField] private TextMeshProUGUI lanNicknameText;

        [SerializeField] private Image lanNicknameIcon; // The star icon next to subtitle

        [SerializeField] private Image lanIconLarge;

        

        private Material titleMaterialInstance;

        

        [Header("Stats Panel")]

        [SerializeField] private TextMeshProUGUI hpText;

        [SerializeField] private TextMeshProUGUI damageText;

        [SerializeField] private TextMeshProUGUI fireRateText;



        [Header("Upgrade Buttons")]

        [SerializeField] private Button upgradeHpBtn;

        [SerializeField] private Button upgradeDmgBtn;

        [SerializeField] private Button upgradeFrBtn;

        

        [Header("Upgrade Texts & Data")]

        [SerializeField] private UpgradeCostTable upgradeCostTable;

        [SerializeField] private TextMeshProUGUI upgradeHpCostText;

        [SerializeField] private TextMeshProUGUI upgradeDmgCostText;

        [SerializeField] private TextMeshProUGUI upgradeFrCostText;

        [SerializeField] private TextMeshProUGUI upgradeHpBtnText;

        [SerializeField] private TextMeshProUGUI upgradeDmgBtnText;

        [SerializeField] private TextMeshProUGUI upgradeFrBtnText;



        [Header("Skill Panel")]

        [SerializeField] private TextMeshProUGUI skillNameText;

        [SerializeField] private TextMeshProUGUI skillDescText;

        [SerializeField] private Image skillIconImage;

        [SerializeField] private Button readStoryBtn;
        [SerializeField] private TextMeshProUGUI readStoryBtnText;
        [SerializeField] private LorePanelUI lorePanelUI;



        [Header("Story Panel Overlay")]

        [SerializeField] private GameObject storyPanel;

        [System.Obsolete("Dùng LorePanelUI thay thế")]
        [SerializeField] private TextMeshProUGUI storyContentText;

        [System.Obsolete("Dùng LorePanelUI thay thế")]
        [SerializeField] private Image storyLanIcon;

        [System.Obsolete("Dùng LorePanelUI thay thế")]
        [SerializeField] private Button closeStoryBtn;



        [Header("Settings Prefab Reference")]

        [SerializeField] private GameObject settingsUIPrefab; // Assign the SettingsUI prefab here



        private int currentLanIndex = 0;

        private LanData currentSelectedLan;



        private void OnEnable()
        {
            Time.timeScale = 1f;

            if (DemHoiDenLong.Gameplay.GameManager.Instance != null)
            {
                DemHoiDenLong.Gameplay.GameManager.Instance.ChangeState(DemHoiDenLong.Gameplay.GameState.MainMenu);
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.LoadEconomy();
                UpdateStarUI(EconomyManager.Instance.StarCount);
                UpdateDiamondUI(EconomyManager.Instance.DiamondCount);
            }
            else
            {
                UpdateStarUI(PlayerPrefs.GetInt("Player_Stars", 1000));
                UpdateDiamondUI(PlayerPrefs.GetInt("Player_Diamonds", 500));
            }

            if (lanDatabase != null && lanDatabase.Count > 0)
            {
                string savedLanId = PlayerPrefs.GetString("SelectedLan", "1");
                currentLanIndex = 0;
                for (int i = 0; i < lanDatabase.Count; i++)
                {
                    if (lanDatabase[i] != null && lanDatabase[i].lanId == savedLanId)
                    {
                        currentLanIndex = i;
                        break;
                    }
                }
                SelectLan(lanDatabase[currentLanIndex]);
            }
        }

        private void Start()

        {

            if (lanDatabase == null || lanDatabase.Count == 0)

            {

                Debug.LogError("Lan Database is empty! Please assign the 4 LanData assets in HangarUI.");

                return;

            }



            if (upgradeHpBtn != null) upgradeHpBtn.onClick.AddListener(() => OnUpgradeClicked("HP"));

            if (upgradeDmgBtn != null) upgradeDmgBtn.onClick.AddListener(() => OnUpgradeClicked("Damage"));

            if (upgradeFrBtn != null) upgradeFrBtn.onClick.AddListener(() => OnUpgradeClicked("FireRate"));



            // Hook up buttons

            if (playBtn != null) playBtn.onClick.AddListener(OnPlayClicked);

            if (unlockBtn != null) unlockBtn.onClick.AddListener(OnUnlockClicked);

            if (nextLanBtn != null) nextLanBtn.onClick.AddListener(SelectNextLan);

            if (prevLanBtn != null) prevLanBtn.onClick.AddListener(SelectPrevLan);

            

            if (readStoryBtn != null)
            {
                readStoryBtn.onClick.RemoveListener(OpenStoryPanel);
                readStoryBtn.onClick.AddListener(OpenStoryPanel);
            }

            if (closeStoryBtn != null)
            {
                closeStoryBtn.onClick.RemoveListener(CloseStoryPanel);
                closeStoryBtn.onClick.AddListener(CloseStoryPanel);
            }

            if (settingsBtn != null) settingsBtn.onClick.AddListener(OpenSettings);



            // Hide story panel by default

            if (storyPanel != null) storyPanel.SetActive(false);



            // Load previously selected Lan if available

            string savedLanId = PlayerPrefs.GetString("SelectedLan", "1");

            currentLanIndex = 0;

            for (int i = 0; i < lanDatabase.Count; i++)

            {

                if (lanDatabase[i].lanId == savedLanId)

                {

                    currentLanIndex = i;

                    break;

                }

            }



            // Create material instance for Title Text to change outline/underlay

            if (lanNameText != null)

            {

                titleMaterialInstance = lanNameText.fontMaterial; // creates instance

            }



            // Hook Economy events

            if (EconomyManager.Instance != null)

            {

                EconomyManager.Instance.OnStarChanged += UpdateStarUI;

                EconomyManager.Instance.OnDiamondChanged += UpdateDiamondUI;

                // Init values

                UpdateStarUI(EconomyManager.Instance.StarCount);

                UpdateDiamondUI(EconomyManager.Instance.DiamondCount);

            }



            SelectLan(lanDatabase[currentLanIndex]);

        }



        private void OnDestroy()

        {

            if (EconomyManager.Instance != null)

            {

                EconomyManager.Instance.OnStarChanged -= UpdateStarUI;

                EconomyManager.Instance.OnDiamondChanged -= UpdateDiamondUI;

            }

        }



        private void UpdateStarUI(int amount)

        {

            if (starAmountText != null) starAmountText.text = amount.ToString();

        }



        private void UpdateDiamondUI(int amount)

        {

            if (diamondAmountText != null) diamondAmountText.text = amount.ToString();

        }



        private void SelectNextLan()

        {

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();

            currentLanIndex = (currentLanIndex + 1) % lanDatabase.Count;

            SelectLan(lanDatabase[currentLanIndex]);

        }



        private void SelectPrevLan()

        {

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();

            currentLanIndex--;

            if (currentLanIndex < 0) currentLanIndex = lanDatabase.Count - 1;

            SelectLan(lanDatabase[currentLanIndex]);

        }



        public void SelectLan(LanData data)

        {

            if (data == null) return;



            // Validate that we are picking the right data

            if (data.lanId != lanDatabase[currentLanIndex].lanId)

            {

                int foundIdx = lanDatabase.FindIndex(l => l != null && l.lanId == data.lanId);

                if (foundIdx >= 0)

                {

                    currentLanIndex = foundIdx;

                }

                else

                {

                    Debug.LogWarning("Validation Failed: Attempting to load wrong LanData index. Forcing correct data.");

                    data = lanDatabase[currentLanIndex];

                }

            }



            currentSelectedLan = data;

            UpdateUI();

            

            if (AudioManager.Instance != null)

            {

                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMainMenu);

            }

        }



        private void UpdateUI()

        {

            if (currentSelectedLan == null) return;



            // 1. Update Title and Subtitle

            if (lanNameText != null)

            {

                lanNameText.text = currentSelectedLan.lanName.ToUpper();

                lanNameText.color = currentSelectedLan.titleColor;

                

                // Darken the title color for outline/shadow (e.g. 50% darker)

                Color darkerColor = new Color(currentSelectedLan.titleColor.r * 0.4f, currentSelectedLan.titleColor.g * 0.4f, currentSelectedLan.titleColor.b * 0.4f, 1f);

                

                if (titleMaterialInstance != null)

                {

                    // Force enable Outline, Underlay, and Glow features in TMP Shader

                    titleMaterialInstance.EnableKeyword("OUTLINE_ON");

                    titleMaterialInstance.EnableKeyword("UNDERLAY_ON");

                    titleMaterialInstance.EnableKeyword("GLOW_ON");



                    Color faceColor = currentSelectedLan.titleColor;

                    Color outlineColor = Color.black;

                    float outlineWidth = 0.15f;

                    Color underlayColor = Color.black;

                    float underlaySoftness = 0.5f;



                    switch (currentSelectedLan.lanId)

                    {

                        case "1": // Kim Lân

                            ColorUtility.TryParseHtmlString("#EAF2FA", out faceColor);

                            ColorUtility.TryParseHtmlString("#5A6E85", out outlineColor);

                            ColorUtility.TryParseHtmlString("#FFFFFF", out underlayColor);

                            underlayColor.a = 0.45f;

                            outlineWidth = 0.15f;

                            underlaySoftness = 0.55f;

                            break;

                        case "2": // Hỏa Lân

                            ColorUtility.TryParseHtmlString("#FF8A66", out faceColor);

                            ColorUtility.TryParseHtmlString("#A8492F", out outlineColor);

                            ColorUtility.TryParseHtmlString("#FFE0CC", out underlayColor);

                            underlayColor.a = 0.45f;

                            outlineWidth = 0.16f;

                            underlaySoftness = 0.6f;

                            break;

                        case "3": // Thủy Lân

                            ColorUtility.TryParseHtmlString("#9EF2FC", out faceColor);

                            ColorUtility.TryParseHtmlString("#3A8A96", out outlineColor);

                            ColorUtility.TryParseHtmlString("#E8FDFF", out underlayColor);

                            underlayColor.a = 0.45f;

                            outlineWidth = 0.15f;

                            underlaySoftness = 0.6f;

                            break;

                        case "4": // Lân Vàng

                            ColorUtility.TryParseHtmlString("#FFE04D", out faceColor);

                            ColorUtility.TryParseHtmlString("#A8850A", out outlineColor);

                            ColorUtility.TryParseHtmlString("#FFFAE0", out underlayColor);

                            underlayColor.a = 0.50f;

                            outlineWidth = 0.18f;

                            underlaySoftness = 0.65f;

                            break;

                    }



                    // Ensure Font is Bold for better glow absorption

                    lanNameText.fontStyle = FontStyles.Bold;



                    // Apply to Vertex Color and Material Face Color

                    lanNameText.color = faceColor;

                    titleMaterialInstance.SetColor("_FaceColor", faceColor);

                    

                    // Apply Outline

                    titleMaterialInstance.SetColor("_OutlineColor", outlineColor);

                    titleMaterialInstance.SetFloat("_OutlineWidth", outlineWidth);

                    

                    // Apply Underlay (Glow/DropShadow)

                    titleMaterialInstance.SetColor("_UnderlayColor", underlayColor);

                    titleMaterialInstance.SetFloat("_UnderlaySoftness", underlaySoftness);

                    

                    // Apply Glow layer

                    titleMaterialInstance.SetColor("_GlowColor", faceColor);

                    titleMaterialInstance.SetFloat("_GlowPower", 0.5f);

                    titleMaterialInstance.SetFloat("_GlowOuter", 0.3f);

                }

                

                // Update Shadow Text object if the user has a duplicate object for drop shadow

                if (lanNameShadowText != null)

                {

                    lanNameShadowText.text = currentSelectedLan.lanName.ToUpper();

                    // Just a subtle darkening for the fallback shadow object if they still use it

                    lanNameShadowText.color = new Color(0, 0, 0, 0.5f);

                }

            }

            if (lanNicknameText != null)

            {

                lanNicknameText.text = currentSelectedLan.nickname;

                lanNicknameText.color = currentSelectedLan.skillTitleColor; // Use skillTitleColor for consistency

            }

            if (lanNicknameIcon != null)

            {

                lanNicknameIcon.color = currentSelectedLan.titleColor;

            }



            // 2. Update Avatar

            if (lanIconLarge != null)

            {

                lanIconLarge.sprite = currentSelectedLan.icon;

            }



            // 3. Update Stats and Upgrade Buttons

            UpdateUpgradeUI();



            // 4. Update Skill

            if (skillNameText != null)

            {

                skillNameText.text = $"KỸ NĂNG: {currentSelectedLan.skillName.ToUpper()}";

                skillNameText.color = currentSelectedLan.skillTitleColor;

            }

            if (skillDescText != null) skillDescText.text = currentSelectedLan.skillDescription;

            if (skillIconImage != null)
            {
                bool hasIcon = (currentSelectedLan.skillIcon != null);
                skillIconImage.gameObject.SetActive(hasIcon);
                if (hasIcon) skillIconImage.sprite = currentSelectedLan.skillIcon;
            }



            // 5. Check Unlock State

            bool isUnlocked = currentSelectedLan.isUnlockedByDefault || PlayerPrefs.GetInt("Unlocked_LanId_" + currentSelectedLan.lanId, 0) == 1;



            if (isUnlocked)

            {

                if (unlockBtn != null) unlockBtn.gameObject.SetActive(false);

                if (playBtn != null) playBtn.gameObject.SetActive(true);

            }

            else

            {

                if (unlockBtn != null) unlockBtn.gameObject.SetActive(true);

                if (playBtn != null) playBtn.gameObject.SetActive(false);



                // Setup Unlock button text and icon

                if (currentSelectedLan.unlockCostStars > 0)

                {

                    unlockCostText.text = $"MỞ KHÓA - {currentSelectedLan.unlockCostStars}";

                    if (unlockCurrencyIcon != null) unlockCurrencyIcon.sprite = starSprite;

                }

                else if (currentSelectedLan.unlockCostGems > 0)

                {

                    unlockCostText.text = $"MỞ KHÓA - {currentSelectedLan.unlockCostGems}";

                    if (unlockCurrencyIcon != null) unlockCurrencyIcon.sprite = diamondSprite;
                }
            }

            // 6. Update Read Story Button text dynamically
            if (readStoryBtn != null)
            {
                if (readStoryBtnText == null)
                    readStoryBtnText = readStoryBtn.GetComponentInChildren<TextMeshProUGUI>(true);

                if (readStoryBtnText != null && currentSelectedLan != null)
                {
                    readStoryBtnText.text = $"ĐỌC TRUYỀN THUYẾT {currentSelectedLan.title.ToUpper()}";
                }
            }




        }



        private int GetUpgradeLevel(string stat)

        {

            if (currentSelectedLan == null) return 1;

            int lvl = PlayerPrefs.GetInt($"UpgradeLevel_{currentSelectedLan.lanId}_{stat}", 1);

            return Mathf.Clamp(lvl, 1, 10);

        }



        private void SetUpgradeLevel(string stat, int level)

        {

            if (currentSelectedLan == null) return;

            PlayerPrefs.SetInt($"UpgradeLevel_{currentSelectedLan.lanId}_{stat}", level);

            PlayerPrefs.Save();

        }



        private void UpdateUpgradeUI()

        {

            if (currentSelectedLan == null) return;



            int hpLevel = GetUpgradeLevel("HP");

            int dmgLevel = GetUpgradeLevel("Damage");

            int frLevel = GetUpgradeLevel("FireRate");



            var computedStats = DemHoiDenLong.Gameplay.LanStats.Compute(currentSelectedLan.lanId, currentSelectedLan);

            float currentHp = computedStats.hp;

            float currentDmg = computedStats.damage;

            float currentFr = computedStats.fireRate;

            int capHp = Mathf.Clamp(hpLevel - 1, 0, 10);

            Debug.Log($"[STATS] Hangar lan={currentSelectedLan.lanId} level={capHp} hp={currentHp} dmg={currentDmg} fr={currentFr}");



            if (hpText != null) hpText.text = currentHp.ToString("0");

            if (damageText != null) damageText.text = currentDmg.ToString("0.##");

            if (fireRateText != null) fireRateText.text = currentFr.ToString("0.##");



            bool isUnlocked = currentSelectedLan.isUnlockedByDefault || PlayerPrefs.GetInt("Unlocked_LanId_" + currentSelectedLan.lanId, 0) == 1;



            UpdateSingleUpgradeButton(upgradeHpBtn, upgradeHpBtnText, upgradeHpCostText, hpLevel, "HP", isUnlocked);

            UpdateSingleUpgradeButton(upgradeDmgBtn, upgradeDmgBtnText, upgradeDmgCostText, dmgLevel, "Damage", isUnlocked);

            UpdateSingleUpgradeButton(upgradeFrBtn, upgradeFrBtnText, upgradeFrCostText, frLevel, "FireRate", isUnlocked);

        }



        private void UpdateSingleUpgradeButton(Button btn, TextMeshProUGUI btnText, TextMeshProUGUI costText, int currentLevel, string stat, bool isUnlocked)
        {
            if (btn == null) return;

            Transform cardParent = costText != null ? costText.transform.parent : (btn != null ? btn.transform.parent : null);
            Transform coinIcon = cardParent != null ? cardParent.Find("CoinIcon") : null;
            if (coinIcon == null && cardParent != null)
            {
                for (int i = 0; i < cardParent.childCount; i++)
                {
                    Transform child = cardParent.GetChild(i);
                    if (child.name.IndexOf("Coin", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        coinIcon = child;
                        break;
                    }
                }
            }

            if (!isUnlocked)
            {
                btn.interactable = false;
                if (btnText != null) btnText.text = "NÂNG CẤP";
                if (costText != null) costText.gameObject.SetActive(true);
                if (coinIcon != null) coinIcon.gameObject.SetActive(true);
                // Can Optionally set cost text here, but if it's locked we might still want to show the cost for Level 1
                if (costText != null)
                {
                    int cost = GetUpgradeCost(stat, currentLevel);
                    costText.text = cost > 0 ? cost.ToString() : "";
                }
                return;
            }

            if (currentLevel >= 10)
            {
                btn.interactable = false;
                if (btnText != null) btnText.text = "TỐI ĐA";
                if (costText != null) costText.gameObject.SetActive(false);
                if (coinIcon != null) coinIcon.gameObject.SetActive(false);
            }
            else
            {
                btn.interactable = true;
                if (btnText != null) btnText.text = "NÂNG CẤP";
                if (costText != null)
                {
                    costText.gameObject.SetActive(true);
                    int cost = GetUpgradeCost(stat, currentLevel);
                    costText.text = cost.ToString();
                }
                if (coinIcon != null) coinIcon.gameObject.SetActive(true);
            }
        }

private int GetUpgradeCost(string stat, int currentLevel)

        {

            if (upgradeCostTable == null || currentSelectedLan == null || currentLevel >= 10 || currentLevel < 1) return 0;



            int[] costs = null;

            if (stat == "HP") costs = upgradeCostTable.hpUpgradeCosts;

            else if (stat == "Damage") costs = upgradeCostTable.damageUpgradeCosts;

            else if (stat == "FireRate") costs = upgradeCostTable.fireRateUpgradeCosts;



            int index = currentLevel - 1;

            if (costs == null || index < 0 || index >= costs.Length) return 0;



            float rawCost = costs[index] * currentSelectedLan.upgradeCostMultiplier;

            return Mathf.RoundToInt(rawCost / 5f) * 5;

        }



        private void OnUpgradeClicked(string stat)

        {

            if (currentSelectedLan == null || EconomyManager.Instance == null) return;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();



            int currentLevel = GetUpgradeLevel(stat);

            if (currentLevel >= 10) return;



            int cost = GetUpgradeCost(stat, currentLevel);

            // Check if player has enough stars

            if (EconomyManager.Instance.StarCount >= cost)

            {

                EconomyManager.Instance.SpendStar(cost);

                SetUpgradeLevel(stat, currentLevel + 1);

                UpdateUpgradeUI();

            }

            else

            {

                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIError();

                ShowToast("Không đủ Sao để nâng cấp!", false);



                Button btnToShake = null;

                if (stat == "HP") btnToShake = upgradeHpBtn;

                else if (stat == "Damage") btnToShake = upgradeDmgBtn;

                else if (stat == "FireRate") btnToShake = upgradeFrBtn;



                if (btnToShake != null) StartCoroutine(ShakeButton(btnToShake.transform));

            }

        }



        private IEnumerator ShakeButton(Transform t)

        {

            if (t == null) yield break;

            Vector3 originalPos = t.localPosition;

            float duration = 0.3f;

            float elapsed = 0f;

            

            while (elapsed < duration)

            {

                elapsed += Time.deltaTime;

                float xOffset = Random.Range(-10f, 10f);

                t.localPosition = originalPos + new Vector3(xOffset, 0, 0);

                yield return null;

            }

            t.localPosition = originalPos;

        }



        private void OnUnlockClicked()

        {

            if (currentSelectedLan == null || EconomyManager.Instance == null) return;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();



            bool success = false;

            string currencyName = currentSelectedLan.unlockCostStars > 0 ? "Sao" : "Kim Cương";



            if (currentSelectedLan.unlockCostStars > 0)

            {

                success = EconomyManager.Instance.SpendStar(currentSelectedLan.unlockCostStars);

            }

            else if (currentSelectedLan.unlockCostGems > 0)

            {

                success = EconomyManager.Instance.SpendDiamond(currentSelectedLan.unlockCostGems);

            }



            if (success)

            {

                // Save unlock state

                PlayerPrefs.SetInt("Unlocked_LanId_" + currentSelectedLan.lanId, 1);

                PlayerPrefs.Save();

                UpdateUI(); // Refresh UI to hide Unlock and show Play

                ShowToast($"Mở khóa {currentSelectedLan.lanName} thành công!", true);

            }

            else

            {

                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIError();

                ShowToast($"Không đủ {currencyName} để mở khóa!", false);

                StartCoroutine(ShakeUnlockButton());

            }

        }



        private IEnumerator ShakeUnlockButton()

        {

            if (unlockBtn == null) yield break;

            Vector3 originalPos = unlockBtn.transform.localPosition;

            float duration = 0.3f;

            float elapsed = 0f;

            

            while (elapsed < duration)

            {

                elapsed += Time.deltaTime;

                float xOffset = Random.Range(-10f, 10f);

                unlockBtn.transform.localPosition = originalPos + new Vector3(xOffset, 0, 0);

                yield return null;

            }

            unlockBtn.transform.localPosition = originalPos;

        }



        private void ShowToast(string message, bool isSuccess)

        {

            if (toastPanel == null || toastText == null) return;



            if (toastCoroutine != null) StopCoroutine(toastCoroutine);

            

            toastText.text = message;

            if (toastBg != null)

            {

                Color successColor; ColorUtility.TryParseHtmlString("#4CAF50", out successColor);

                Color failColor; ColorUtility.TryParseHtmlString("#F44336", out failColor);

                toastBg.color = isSuccess ? successColor : failColor;

            }

            

            toastCoroutine = StartCoroutine(ToastRoutine());

        }



        private IEnumerator ToastRoutine()

        {

            toastPanel.SetActive(true);

            CanvasGroup cg = toastPanel.GetComponent<CanvasGroup>();

            if (cg == null) cg = toastPanel.AddComponent<CanvasGroup>();

            

            // Fade in

            cg.alpha = 0;

            while (cg.alpha < 1f)

            {

                cg.alpha += Time.deltaTime * 5f;

                yield return null;

            }



            yield return new WaitForSeconds(1.5f);



            // Fade out

            while (cg.alpha > 0f)

            {

                cg.alpha -= Time.deltaTime * 5f;

                yield return null;

            }

            toastPanel.SetActive(false);

        }



        private void OnPlayClicked()

        {

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();

            if (currentSelectedLan != null)

            {

                Debug.Log($"Play with {currentSelectedLan.lanName}");

                PlayerPrefs.SetString("SelectedLan", currentSelectedLan.lanId);

                PlayerPrefs.Save();

                if (int.TryParse(currentSelectedLan.lanId, out int idInt))

                {

                    LevelSelectContext.SelectedLanId = idInt;

                }

            }



            // Giai doan 5A: Chuyen huong sang LevelSelect thay vi vao Gameplay

            DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.LevelSelect, "Hangar_PlayButton");

        }



        public void OpenStoryPanel()

        {

            if (lorePanelUI != null && lorePanelUI.IsOpen) return;
            if (storyPanel != null && storyPanel.activeSelf) return;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();



            if (lorePanelUI != null)

            {

                lorePanelUI.Show(currentSelectedLan);

                return;

            }



            if (storyPanel != null)

            {

                LorePanelUI lp = storyPanel.GetComponent<LorePanelUI>();

                if (lp != null)

                {

                    lorePanelUI = lp;

                    lp.Show(currentSelectedLan);

                    return;

                }



                if (currentSelectedLan != null)

                {

                    if (storyContentText != null)

                    {

                        storyContentText.text = currentSelectedLan.storyDescription;

                    }

                    if (storyLanIcon != null)

                    {

                        storyLanIcon.sprite = currentSelectedLan.icon;

                    }

                    storyPanel.SetActive(true);

                }

            }

        }



        public void CloseStoryPanel()

        {

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();



            if (lorePanelUI != null)

            {

                lorePanelUI.Hide();

                return;

            }



            if (storyPanel != null)

            {

                LorePanelUI lp = storyPanel.GetComponent<LorePanelUI>();

                if (lp != null)

                {

                    lp.Hide();

                    return;

                }

                storyPanel.SetActive(false);

            }

        }



        private void OpenSettings()

        {

            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();

            

            if (SettingsUI.Instance != null)

            {

                SettingsUI.Instance.gameObject.SetActive(true);

            }

            else if (settingsUIPrefab != null)

            {

                // Instantiate setting UI if not already in scene

                Instantiate(settingsUIPrefab, transform.parent);

            }

            else

            {

                Debug.LogWarning("Settings UI Prefab is not assigned in HangarUI!");

            }

        }

    }

}

