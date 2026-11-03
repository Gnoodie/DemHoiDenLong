using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class Phase5AGameplaySetup
    {
        private const string SceneMainMenu = "Assets/Scenes/Main_Menu.unity";
        private const string SceneLevelSelect = "Assets/Scenes/LevelSelect.unity";
        private const string SceneGameplay = "Assets/Scenes/Gameplay.unity";

        private const string SprStarPath = "Assets/_Project/Art/Icons/star.png";
        private const string SprPillPath = "Assets/_Project/Sprites/UI/ui_pill.png";
        private const string SprRound36Path = "Assets/_Project/Sprites/UI/ui_round_36.png";

        private static readonly Color ColorBgDark = new Color(0.086f, 0.133f, 0.290f, 0.95f);    // #16224A
        private static readonly Color ColorChipBg = new Color(0.110f, 0.165f, 0.361f, 0.90f);    // #1C2A5C alpha 0.9
        private static readonly Color ColorBorder = new Color(0.227f, 0.275f, 0.463f, 1f);      // #3A4676
        private static readonly Color ColorCream = new Color(0.957f, 0.945f, 0.902f, 1f);       // #F4F1E6
        private static readonly Color ColorGold = new Color(1.0f, 0.824f, 0.478f, 1f);          // #FFD27A
        private static readonly Color ColorGreen = new Color(0.180f, 0.620f, 0.478f, 1f);       // #2E9E7A
        private static readonly Color ColorDimStar = new Color(0.290f, 0.333f, 0.502f, 1f);     // #4A5580
        private static readonly Color ColorSubText = new Color(0.663f, 0.706f, 0.839f, 1f);     // #A9B4D6

        public static Sprite LoadSpriteAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("DemHoiDenLong/Setup Phase 5A Gameplay")]
        public static void SetupAll()
        {
            Debug.Log("=== [Phase 5A] BẮT ĐẦU CẤU HÌNH TOÀN DIỆN GAMEPLAY & LEVEL SELECT ===");

            ConfigureBuildSettings();
            ConfigureLevelDataAndBosses();
            ConfigureLanPrefabs();
            ConfigureGameplayScene();

            Debug.Log("=== [Phase 5A] HOÀN TẤT CẤU HÌNH ===");
        }

        // =========================================================================
        // 1. CẤU HÌNH BUILD SETTINGS
        // =========================================================================
        public static void ConfigureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(SceneMainMenu, true),
                new EditorBuildSettingsScene(SceneLevelSelect, true),
                new EditorBuildSettingsScene(SceneGameplay, true)
            };

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[Phase 5A] Đã cập nhật Build Settings: 0={SceneMainMenu}, 1={SceneLevelSelect}, 2={SceneGameplay}");
        }

        // =========================================================================
        // 2. CẤU HÌNH DỮ LIỆU MÀN, WAVES & TRÙM
        // =========================================================================
        public static void ConfigureLevelDataAndBosses()
        {
            BossController boss1 = AssetDatabase.LoadAssetAtPath<BossController>("Assets/_Project/Prefabs/Bosses/PF_BossAct1.prefab");
            BossController boss2 = AssetDatabase.LoadAssetAtPath<BossController>("Assets/_Project/Prefabs/Bosses/PF_BossAct2.prefab");
            BossController boss3 = AssetDatabase.LoadAssetAtPath<BossController>("Assets/_Project/Prefabs/Bosses/PF_BossAct3.prefab");

            for (int id = 1; id <= 15; id++)
            {
                string path = $"Assets/_Project/Data/Levels/Level_{id:D2}.asset";
                LevelData lvl = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (lvl == null) continue;

                if (lvl.waves == null || lvl.waves.Count == 0)
                {
                    string soPath = $"Assets/_Project/Data/Levels/SO_Level_{id:D2}.asset";
                    LevelData soLvl = AssetDatabase.LoadAssetAtPath<LevelData>(soPath);
                    if (soLvl != null && soLvl.waves != null && soLvl.waves.Count > 0)
                    {
                        lvl.waves = new List<WaveData>(soLvl.waves);
                    }
                }

                if (id == 5)
                {
                    lvl.isBoss = true;
                    lvl.bossName = "Đèn Tàn Trùm Làng";
                    lvl.bossEnemyId = "boss_den_long_khong_lo";
                    lvl.bossPrefab = boss1;
                    lvl.bossHp = GameplayBalance.BossAct1Hp;
                    lvl.bossSpeed = GameplayBalance.BossAct1Speed;
                    lvl.bossDamage = GameplayBalance.BossAct1Damage;
                }
                else if (id == 10)
                {
                    lvl.isBoss = true;
                    lvl.bossName = "Rồng Đèn Phố Cổ";
                    lvl.bossEnemyId = "boss_rong_den_pho_co";
                    lvl.bossPrefab = boss2;
                    lvl.bossHp = GameplayBalance.BossAct2Hp;
                    lvl.bossSpeed = GameplayBalance.BossAct2Speed;
                    lvl.bossDamage = GameplayBalance.BossAct2Damage;
                }
                else if (id == 15)
                {
                    lvl.isBoss = true;
                    lvl.bossName = "Thỏ Ngọc Hóa Quái";
                    lvl.bossEnemyId = "boss_tho_ngoc_hoa_quai";
                    lvl.bossPrefab = boss3;
                    lvl.bossHp = GameplayBalance.BossAct3Hp;
                    lvl.bossSpeed = GameplayBalance.BossAct3Speed;
                    lvl.bossDamage = GameplayBalance.BossAct3Damage;
                }
                else
                {
                    lvl.isBoss = false;
                    lvl.bossPrefab = null;
                }

                EditorUtility.SetDirty(lvl);
            }

            ActData act2 = AssetDatabase.LoadAssetAtPath<ActData>("Assets/_Project/Data/Acts/Act_2.asset");
            if (act2 != null)
            {
                act2.battleDim = GameplayBalance.Act2BattleDimAlpha;
                EditorUtility.SetDirty(act2);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase 5A] Đã cập nhật 15 LevelData (Waves, Boss HP/Speed/Damage) & ActData thành công.");
        }

        // =========================================================================
        // 3. GÁN PREFAB CHO LANDATA
        // =========================================================================
        public static void ConfigureLanPrefabs()
        {
            string[] lanIds = { "1", "2", "3", "4" };
            string[] prefabNames = { "PF_LanKim", "PF_LanHoa", "PF_LanThuy", "PF_LanVang" };

            for (int i = 0; i < lanIds.Length; i++)
            {
                string dataPath = $"Assets/_Project/Data/Lans/{lanIds[i]}.asset";
                LanData data = AssetDatabase.LoadAssetAtPath<LanData>(dataPath);
                if (data == null) continue;

                string prefabPath = $"Assets/_Project/Prefabs/Lans/{prefabNames[i]}.prefab";
                GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (p != null)
                {
                    data.prefab = p;
                    EditorUtility.SetDirty(data);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase 5A] Đã gán prefab cho 4 LanData thành công.");
        }

        // =========================================================================
        // 4. CẤU HÌNH SCENE GAMEPLAY
        // =========================================================================
        public static void ConfigureGameplayScene()
        {
            var scene = EditorSceneManager.OpenScene(SceneGameplay, OpenSceneMode.Single);

            // 1. Background & DimOverlay
            GameObject bgObj = GameObject.Find("Background");
            if (bgObj != null)
            {
                BackgroundManager bm = bgObj.GetComponent<BackgroundManager>();
                if (bm == null) bm = bgObj.AddComponent<BackgroundManager>();

                Transform dimChild = bgObj.transform.Find("DimOverlay");
                if (dimChild == null)
                {
                    GameObject dim = new GameObject("DimOverlay");
                    dim.transform.SetParent(bgObj.transform, false);
                    dim.transform.localPosition = new Vector3(0, 0, -0.1f);
                    SpriteRenderer srDim = dim.AddComponent<SpriteRenderer>();
                    srDim.sortingOrder = -90;
                }
            }

            // 2. GameManager Components
            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj != null)
            {
                if (gmObj.GetComponent<LevelSessionBootstrap>() == null)
                {
                    gmObj.AddComponent<LevelSessionBootstrap>();
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (gmObj.GetComponent<GameplayDebugPanel>() == null)
                {
                    gmObj.AddComponent<GameplayDebugPanel>();
                }
#endif
            }

            // 3. EnemySpawner Prefabs
            EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                GameObject pPaper = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGiay.prefab");
                GameObject pSpike = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGai.prefab");
                GameObject pCarp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenCa.prefab");
                GameObject pGhost = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenQuy.prefab");

                SerializedObject soSpawner = new SerializedObject(spawner);
                if (pPaper != null) soSpawner.FindProperty("prefabPaper").objectReferenceValue = pPaper;
                if (pSpike != null) soSpawner.FindProperty("prefabSpike").objectReferenceValue = pSpike;
                if (pCarp != null) soSpawner.FindProperty("prefabCarp").objectReferenceValue = pCarp;
                if (pGhost != null) soSpawner.FindProperty("prefabGhost").objectReferenceValue = pGhost;
                soSpawner.ApplyModifiedProperties();
                EditorUtility.SetDirty(spawner);
            }

            // 4. Canvas & UI (CanvasScaler, Top HUD, VictoryPanel, GameOverPanel)
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                // Đặt CanvasScaler chuẩn 1080x1920 (ScaleWithScreenSize, match 0.5)
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080f, 1920f);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    EditorUtility.SetDirty(scaler);
                }

                GameplayHUD hud = canvas.GetComponentInChildren<GameplayHUD>(true);
                if (hud != null)
                {
                    SetupTopHUD(canvas.transform, hud);
                    SetupVictoryPanel(canvas.transform, hud);
                    UpdateGameOverPanelUI(canvas.transform, hud);
                    SetupBossHUD(canvas.transform);
                }

                if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject esObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Phase 5A] Đã lưu Scene Gameplay với HUD chuẩn không chồng lấn, CanvasScaler 1080x1920, và đầy đủ panels.");
        }

        private static void SetupTopHUD(Transform canvasT, GameplayHUD hud)
        {
            Sprite starSpr = LoadSpriteAsset(SprStarPath);
            Sprite pillSpr = LoadSpriteAsset(SprPillPath);
            Sprite round36 = LoadSpriteAsset(SprRound36Path);

            // 1. Phục hồi và bảo vệ BGM_Slider trong SettingsUI
            Transform settingsUI = canvasT.Find("SettingsUI");
            if (settingsUI != null)
            {
                Transform bgmRow = settingsUI.Find("BGM_Row");
                if (bgmRow != null)
                {
                    Slider s = bgmRow.GetComponentInChildren<Slider>(true);
                    if (s != null)
                    {
                        s.gameObject.name = "BGM_Slider";
                        RectTransform rt = s.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(60f, 0f);
                        rt.sizeDelta = new Vector2(440f, 40f);

                        // Xóa HpText nếu có bị gắn nhầm vào BGM slider
                        Transform strayHpTxt = s.transform.Find("HpText") ?? s.transform.Find("Text");
                        if (strayHpTxt != null && strayHpTxt.GetComponent<TextMeshProUGUI>() != null && strayHpTxt.GetComponent<TextMeshProUGUI>().text.Contains("/"))
                        {
                            Object.DestroyImmediate(strayHpTxt.gameObject);
                        }
                    }
                }
            }

            // 2. Xóa sạch mọi đối tượng chân dung Lân (ảnh Lân nhỏ, ảnh xám, khung)
            for (int i = canvasT.childCount - 1; i >= 0; i--)
            {
                var child = canvasT.GetChild(i);
                string n = child.name.ToLower();
                if (n.Contains("portrait") || n == "lanportrait")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            Transform statsP = canvasT.Find("PlayerStatsPanel");
            if (statsP != null)
            {
                for (int i = statsP.childCount - 1; i >= 0; i--)
                {
                    var child = statsP.GetChild(i);
                    string n = child.name.ToLower();
                    if (n.Contains("portrait") || n.Contains("avatar"))
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }
            }

            Transform oldTopBar = canvasT.Find("TopHUD_Bar");
            if (oldTopBar != null)
            {
                Object.DestroyImmediate(oldTopBar.gameObject);
            }

            // 3. Tạo hoặc lấy SafeAreaRoot
            Transform safeRootT = canvasT.Find("SafeAreaRoot");
            GameObject safeRootObj;
            if (safeRootT == null)
            {
                safeRootObj = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeArea));
                safeRootObj.transform.SetParent(canvasT, false);
            }
            else
            {
                safeRootObj = safeRootT.gameObject;
                if (safeRootObj.GetComponent<SafeArea>() == null)
                {
                    safeRootObj.AddComponent<SafeArea>();
                }
            }
            RectTransform rtSafe = safeRootObj.GetComponent<RectTransform>();
            rtSafe.anchorMin = Vector2.zero;
            rtSafe.anchorMax = Vector2.one;
            rtSafe.offsetMin = Vector2.zero;
            rtSafe.offsetMax = Vector2.zero;
            Transform hudParent = safeRootObj.transform;

            // Xóa mọi Slider mồ côi (như BossHPSlider) nằm trong SafeAreaRoot
            for (int i = hudParent.childCount - 1; i >= 0; i--)
            {
                var ch = hudParent.GetChild(i);
                if (ch.name == "BossHPSlider" || ch.name == "BossHUDPanel" || ch.name == "BossHUD")
                {
                    Object.DestroyImmediate(ch.gameObject);
                }
            }

            // 4. Thanh máu PlayerHpBar: Neo góc trái trên (24, -24), rộng 46% (496, 40)
            Transform existingHp = hudParent.Find("PlayerHpBar") ?? hudParent.Find("HPSlider");
            GameObject hpObj;
            Slider slider;
            if (existingHp != null)
            {
                hpObj = existingHp.gameObject;
                hpObj.name = "PlayerHpBar";
                slider = hpObj.GetComponent<Slider>();
            }
            else
            {
                hpObj = new GameObject("PlayerHpBar", typeof(RectTransform), typeof(Slider));
                hpObj.transform.SetParent(hudParent, false);
                slider = hpObj.GetComponent<Slider>();
            }

            RectTransform rtSlider = hpObj.GetComponent<RectTransform>();
            rtSlider.anchorMin = new Vector2(0f, 1f);
            rtSlider.anchorMax = new Vector2(0f, 1f);
            rtSlider.pivot = new Vector2(0f, 1f);
            rtSlider.anchoredPosition = new Vector2(24f, -24f);
            rtSlider.sizeDelta = new Vector2(496f, 40f);

            // Nền PlayerHpBar (#101A3C alpha 0.9)
            Image bgImg = hpObj.GetComponent<Image>();
            if (bgImg == null) bgImg = hpObj.AddComponent<Image>();
            if (round36 != null) { bgImg.sprite = round36; bgImg.type = Image.Type.Sliced; }
            bgImg.color = new Color(0.063f, 0.102f, 0.235f, 0.9f); // #101A3C alpha 0.9

            // Fill Area & Fill (#3DDC97)
            Transform fillArea = hpObj.transform.Find("Fill Area");
            GameObject fillAreaObj;
            if (fillArea == null)
            {
                fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
                fillAreaObj.transform.SetParent(hpObj.transform, false);
            }
            else
            {
                fillAreaObj = fillArea.gameObject;
            }
            RectTransform rtFillArea = fillAreaObj.GetComponent<RectTransform>();
            rtFillArea.anchorMin = Vector2.zero;
            rtFillArea.anchorMax = Vector2.one;
            rtFillArea.offsetMin = new Vector2(2f, 2f);
            rtFillArea.offsetMax = new Vector2(-2f, -2f);

            Transform fillT = fillAreaObj.transform.Find("Fill");
            GameObject fillObj;
            if (fillT == null)
            {
                fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(fillAreaObj.transform, false);
            }
            else
            {
                fillObj = fillT.gameObject;
            }
            RectTransform rtFill = fillObj.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.sizeDelta = Vector2.zero;
            Image fillImage = fillObj.GetComponent<Image>();
            if (round36 != null) { fillImage.sprite = round36; fillImage.type = Image.Type.Sliced; }
            fillImage.color = new Color(0.239f, 0.863f, 0.592f, 1f); // #3DDC97

            slider.fillRect = rtFill;
            slider.targetGraphic = fillImage;

            // HpText căn giữa, cỡ 24 Bold màu kem #F4F1E6
            Transform hpTextT = hpObj.transform.Find("HpText") ?? hpObj.transform.Find("Text") ?? hpObj.transform.Find("HPText");
            TextMeshProUGUI tmpHp;
            if (hpTextT == null)
            {
                GameObject hpTextObj = new GameObject("HpText", typeof(RectTransform), typeof(TextMeshProUGUI));
                hpTextObj.transform.SetParent(hpObj.transform, false);
                tmpHp = hpTextObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                hpTextT.gameObject.name = "HpText";
                tmpHp = hpTextT.GetComponent<TextMeshProUGUI>();
            }
            RectTransform rtHpTxt = tmpHp.GetComponent<RectTransform>();
            rtHpTxt.anchorMin = Vector2.zero;
            rtHpTxt.anchorMax = Vector2.one;
            rtHpTxt.sizeDelta = Vector2.zero;
            tmpHp.fontSize = 24;
            tmpHp.fontStyle = FontStyles.Bold;
            tmpHp.alignment = TextAlignmentOptions.Center;
            tmpHp.color = ColorCream; // #F4F1E6
            tmpHp.textWrappingMode = TextWrappingModes.NoWrap;
            tmpHp.text = "100 / 100";

            // 5. Hàng 3 tim: ngay dưới (24, -72), heart sprite 40x40, tim còn đỏ, tim mất xám #4A5580
            Transform livesT = null;
            foreach (var r in canvasT.GetComponentsInChildren<RectTransform>(true))
            {
                if (r.name == "LivesContainer")
                {
                    livesT = r;
                    break;
                }
            }
            GameObject livesObj;
            if (livesT == null)
            {
                livesObj = new GameObject("LivesContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                livesObj.transform.SetParent(hudParent, false);
            }
            else
            {
                livesObj = livesT.gameObject;
                livesObj.transform.SetParent(hudParent, false);
            }
            RectTransform rtLives = livesObj.GetComponent<RectTransform>();
            rtLives.anchorMin = new Vector2(0f, 1f);
            rtLives.anchorMax = new Vector2(0f, 1f);
            rtLives.pivot = new Vector2(0f, 1f);
            rtLives.anchoredPosition = new Vector2(24f, -72f);
            rtLives.sizeDelta = new Vector2(180f, 40f);

            HorizontalLayoutGroup hlg = livesObj.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = livesObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            livesT = livesObj.transform;

            // 6. Nút Cài Đặt: Góc phải trên (X=-24, Y=-24, size 64x64)
            var allButtons = canvasT.GetComponentsInChildren<Button>(true);
            Transform settingsT = null;
            for (int i = allButtons.Length - 1; i >= 0; i--)
            {
                var b = allButtons[i];
                if (b.name == "SettingsButton" || b.name == "Button_Settings" || b.name == "PauseButton")
                {
                    RectTransform rtB = b.GetComponent<RectTransform>();
                    if (rtB != null && rtB.sizeDelta.x > 100f)
                    {
                        Object.DestroyImmediate(b.gameObject);
                    }
                    else if (settingsT == null)
                    {
                        settingsT = b.transform;
                    }
                    else
                    {
                        Object.DestroyImmediate(b.gameObject);
                    }
                }
            }

            if (settingsT != null)
            {
                settingsT.SetParent(hudParent, false);
                RectTransform rtSettings = settingsT.GetComponent<RectTransform>();
                rtSettings.anchorMin = new Vector2(1f, 1f);
                rtSettings.anchorMax = new Vector2(1f, 1f);
                rtSettings.pivot = new Vector2(1f, 1f);
                rtSettings.anchoredPosition = new Vector2(-24f, -24f);
                rtSettings.sizeDelta = new Vector2(64f, 64f);
            }

            // 7. Chip mục tiêu: Giữ vị trí giữa-trên (Y=-128f, size 260x44)
            Transform existingObjChip = hudParent.Find("ObjectiveChip") ?? canvasT.Find("ObjectiveChip");
            GameObject objChip;
            if (existingObjChip == null)
            {
                objChip = new GameObject("ObjectiveChip", typeof(RectTransform), typeof(Image));
                objChip.transform.SetParent(hudParent, false);
            }
            else
            {
                objChip = existingObjChip.gameObject;
                objChip.transform.SetParent(hudParent, false);
            }

            RectTransform rtObj = objChip.GetComponent<RectTransform>();
            rtObj.anchorMin = new Vector2(0.5f, 1f);
            rtObj.anchorMax = new Vector2(0.5f, 1f);
            rtObj.pivot = new Vector2(0.5f, 1f);
            rtObj.anchoredPosition = new Vector2(0f, -128f);
            rtObj.sizeDelta = new Vector2(260f, 44f);

            Image imgObj = objChip.GetComponent<Image>();
            if (pillSpr != null) { imgObj.sprite = pillSpr; imgObj.type = Image.Type.Sliced; }
            imgObj.color = ColorChipBg;

            Transform objTextT = objChip.transform.Find("Text");
            TextMeshProUGUI tmpObj;
            if (objTextT == null)
            {
                GameObject objTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                objTextObj.transform.SetParent(objChip.transform, false);
                tmpObj = objTextObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                tmpObj = objTextT.GetComponent<TextMeshProUGUI>();
            }

            RectTransform rtText = tmpObj.GetComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero; rtText.anchorMax = Vector2.one; rtText.sizeDelta = Vector2.zero;
            tmpObj.fontSize = 22;
            tmpObj.fontStyle = FontStyles.Bold;
            tmpObj.color = ColorCream;
            tmpObj.alignment = TextAlignmentOptions.Center;
            tmpObj.textWrappingMode = TextWrappingModes.NoWrap;
            tmpObj.text = "Mục tiêu";

            // 8. Bộ đếm sao nhặt: Bên trái nút Cài Đặt (X=-104, Y=-34, size 130x44)
            Transform existingStarChip = hudParent.Find("StarsChip") ?? canvasT.Find("StarsChip");
            GameObject starChip;
            if (existingStarChip == null)
            {
                starChip = new GameObject("StarsChip", typeof(RectTransform), typeof(Image));
                starChip.transform.SetParent(hudParent, false);
            }
            else
            {
                starChip = existingStarChip.gameObject;
                starChip.transform.SetParent(hudParent, false);
            }

            RectTransform rtStar = starChip.GetComponent<RectTransform>();
            rtStar.anchorMin = new Vector2(1f, 1f);
            rtStar.anchorMax = new Vector2(1f, 1f);
            rtStar.pivot = new Vector2(1f, 1f);
            rtStar.anchoredPosition = new Vector2(-104f, -34f);
            rtStar.sizeDelta = new Vector2(130f, 44f);

            Image imgStar = starChip.GetComponent<Image>();
            if (pillSpr != null) { imgStar.sprite = pillSpr; imgStar.type = Image.Type.Sliced; }
            imgStar.color = ColorChipBg;

            Transform iconT = starChip.transform.Find("StarIcon");
            GameObject starIconObj;
            if (iconT == null)
            {
                starIconObj = new GameObject("StarIcon", typeof(RectTransform), typeof(Image));
                starIconObj.transform.SetParent(starChip.transform, false);
            }
            else
            {
                starIconObj = iconT.gameObject;
            }

            RectTransform rtIcon = starIconObj.GetComponent<RectTransform>();
            rtIcon.anchorMin = new Vector2(0f, 0.5f);
            rtIcon.anchorMax = new Vector2(0f, 0.5f);
            rtIcon.pivot = new Vector2(0f, 0.5f);
            rtIcon.anchoredPosition = new Vector2(10f, 0f);
            rtIcon.sizeDelta = new Vector2(28f, 28f);
            Image imgIcon = starIconObj.GetComponent<Image>();
            if (starSpr != null) imgIcon.sprite = starSpr;
            imgIcon.color = ColorGold;

            Transform starTextT = starChip.transform.Find("Text");
            TextMeshProUGUI tmpStar;
            if (starTextT == null)
            {
                GameObject starTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                starTextObj.transform.SetParent(starChip.transform, false);
                tmpStar = starTextObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                tmpStar = starTextT.GetComponent<TextMeshProUGUI>();
            }

            RectTransform rtStarTxt = tmpStar.GetComponent<RectTransform>();
            rtStarTxt.anchorMin = Vector2.zero;
            rtStarTxt.anchorMax = Vector2.one;
            rtStarTxt.offsetMin = new Vector2(44f, 0f);
            rtStarTxt.offsetMax = new Vector2(-8f, 0f);
            tmpStar.fontSize = 22;
            tmpStar.fontStyle = FontStyles.Bold;
            tmpStar.color = ColorCream;
            tmpStar.alignment = TextAlignmentOptions.MidlineLeft;
            tmpStar.textWrappingMode = TextWrappingModes.NoWrap;
            tmpStar.text = "0";

            // 9. Nút Skill ở góc dưới phải chừa vùng 150x150 (-36, 36)
            Transform skillT = null;
            foreach (var r in canvasT.GetComponentsInChildren<RectTransform>(true))
            {
                if (r.name == "SkillIcon")
                {
                    skillT = r;
                    break;
                }
            }
            if (skillT != null)
            {
                skillT.SetParent(hudParent, false);
                RectTransform rtSkill = skillT.GetComponent<RectTransform>();
                rtSkill.anchorMin = new Vector2(1f, 0f);
                rtSkill.anchorMax = new Vector2(1f, 0f);
                rtSkill.pivot = new Vector2(1f, 0f);
                rtSkill.anchoredPosition = new Vector2(-36f, 36f);
                rtSkill.sizeDelta = new Vector2(120f, 120f);
            }

            // Gán các trường vào GameplayHUD
            SerializedObject soHud = new SerializedObject(hud);
            soHud.FindProperty("hpSlider").objectReferenceValue = slider;
            soHud.FindProperty("hpFillImage").objectReferenceValue = fillImage;
            soHud.FindProperty("hpText").objectReferenceValue = tmpHp;
            soHud.FindProperty("livesContainer").objectReferenceValue = livesT.GetComponent<RectTransform>();
            soHud.FindProperty("objectiveText").objectReferenceValue = tmpObj;
            soHud.FindProperty("runStarsText").objectReferenceValue = tmpStar;
            if (starSpr != null) soHud.FindProperty("starSprite").objectReferenceValue = starSpr;
            soHud.ApplyModifiedProperties();
        }

        private static void SetupVictoryPanel(Transform canvasT, GameplayHUD hud)
        {
            Transform existingVictory = canvasT.Find("VictoryPanel");
            if (existingVictory != null)
            {
                Object.DestroyImmediate(existingVictory.gameObject);
            }

            Sprite round36 = LoadSpriteAsset(SprRound36Path);
            Sprite pill = LoadSpriteAsset(SprPillPath);
            Sprite starSpr = LoadSpriteAsset(SprStarPath);

            // Root Overlay
            GameObject victoryRoot = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster));
            victoryRoot.transform.SetParent(canvasT, false);
            RectTransform rtRoot = victoryRoot.GetComponent<RectTransform>();
            rtRoot.anchorMin = Vector2.zero; rtRoot.anchorMax = Vector2.one; rtRoot.sizeDelta = Vector2.zero;
            Image imgOverlay = victoryRoot.GetComponent<Image>();
            imgOverlay.color = new Color(0.04f, 0.06f, 0.16f, 0.85f); // Tối nền

            CanvasGroup cg = victoryRoot.GetComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;

            Canvas vCanvas = victoryRoot.GetComponent<Canvas>();
            vCanvas.overrideSorting = true;
            vCanvas.sortingOrder = 100;

            victoryRoot.transform.SetAsLastSibling();

            // Card Panel
            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(victoryRoot.transform, false);
            RectTransform rtCard = card.GetComponent<RectTransform>();
            rtCard.anchorMin = new Vector2(0.5f, 0.5f);
            rtCard.anchorMax = new Vector2(0.5f, 0.5f);
            rtCard.sizeDelta = new Vector2(560f, 680f);
            Image imgCard = card.GetComponent<Image>();
            if (round36 != null) { imgCard.sprite = round36; imgCard.type = Image.Type.Sliced; }
            imgCard.color = ColorBgDark;

            // 1. Tiêu đề "CHIẾN THẮNG"
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(card.transform, false);
            RectTransform rtTitle = titleObj.GetComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0.5f, 1f); rtTitle.anchorMax = new Vector2(0.5f, 1f);
            rtTitle.anchoredPosition = new Vector2(0f, -50f);
            rtTitle.sizeDelta = new Vector2(500f, 60f);
            TextMeshProUGUI tmpTitle = titleObj.GetComponent<TextMeshProUGUI>();
            tmpTitle.text = "CHIẾN THẮNG";
            tmpTitle.fontSize = 38;
            tmpTitle.fontStyle = FontStyles.Bold;
            tmpTitle.alignment = TextAlignmentOptions.Center;
            tmpTitle.color = ColorCream;

            // 2. Stars Container (3 sao)
            GameObject starsRow = new GameObject("StarsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            starsRow.transform.SetParent(card.transform, false);
            RectTransform rtStars = starsRow.GetComponent<RectTransform>();
            rtStars.anchorMin = new Vector2(0.5f, 1f); rtStars.anchorMax = new Vector2(0.5f, 1f);
            rtStars.anchoredPosition = new Vector2(0f, -125f);
            rtStars.sizeDelta = new Vector2(260f, 64f);
            HorizontalLayoutGroup hlg = starsRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;

            Image[] starImgs = new Image[3];
            for (int s = 0; s < 3; s++)
            {
                GameObject sObj = new GameObject($"Star_{s + 1}", typeof(RectTransform), typeof(Image));
                sObj.transform.SetParent(starsRow.transform, false);
                RectTransform rtS = sObj.GetComponent<RectTransform>();
                rtS.sizeDelta = new Vector2(56f, 56f);
                Image imgS = sObj.GetComponent<Image>();
                if (starSpr != null) imgS.sprite = starSpr;
                imgS.color = ColorGold;
                starImgs[s] = imgS;
            }

            // 3. New Record Badge
            GameObject recObj = new GameObject("RecordBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
            recObj.transform.SetParent(card.transform, false);
            RectTransform rtRec = recObj.GetComponent<RectTransform>();
            rtRec.anchorMin = new Vector2(0.5f, 1f); rtRec.anchorMax = new Vector2(0.5f, 1f);
            rtRec.anchoredPosition = new Vector2(0f, -185f);
            rtRec.sizeDelta = new Vector2(300f, 32f);
            TextMeshProUGUI tmpRec = recObj.GetComponent<TextMeshProUGUI>();
            tmpRec.text = "KỶ LỤC MỚI";
            tmpRec.fontSize = 20;
            tmpRec.fontStyle = FontStyles.Bold;
            tmpRec.alignment = TextAlignmentOptions.Center;
            tmpRec.color = ColorGreen;

            // 4. Run Stars Text
            GameObject runStarsObj = new GameObject("RunStarsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            runStarsObj.transform.SetParent(card.transform, false);
            RectTransform rtRunS = runStarsObj.GetComponent<RectTransform>();
            rtRunS.anchorMin = new Vector2(0.5f, 1f); rtRunS.anchorMax = new Vector2(0.5f, 1f);
            rtRunS.anchoredPosition = new Vector2(0f, -235f);
            rtRunS.sizeDelta = new Vector2(460f, 36f);
            TextMeshProUGUI tmpRunS = runStarsObj.GetComponent<TextMeshProUGUI>();
            tmpRunS.text = "Sao nhặt được: +0";
            tmpRunS.fontSize = 24;
            tmpRunS.alignment = TextAlignmentOptions.Center;
            tmpRunS.color = ColorCream;

            // 5. Milestone Reward Text
            GameObject msObj = new GameObject("MilestoneRewardText", typeof(RectTransform), typeof(TextMeshProUGUI));
            msObj.transform.SetParent(card.transform, false);
            RectTransform rtMs = msObj.GetComponent<RectTransform>();
            rtMs.anchorMin = new Vector2(0.5f, 1f); rtMs.anchorMax = new Vector2(0.5f, 1f);
            rtMs.anchoredPosition = new Vector2(0f, -280f);
            rtMs.sizeDelta = new Vector2(460f, 36f);
            TextMeshProUGUI tmpMs = msObj.GetComponent<TextMeshProUGUI>();
            tmpMs.text = "Thưởng mốc: +0 Sao";
            tmpMs.fontSize = 24;
            tmpMs.fontStyle = FontStyles.Bold;
            tmpMs.alignment = TextAlignmentOptions.Center;
            tmpMs.color = ColorGold;

            // 6. Next Act Banner
            GameObject nextActObj = new GameObject("NextActBanner", typeof(RectTransform), typeof(TextMeshProUGUI));
            nextActObj.transform.SetParent(card.transform, false);
            RectTransform rtAct = nextActObj.GetComponent<RectTransform>();
            rtAct.anchorMin = new Vector2(0.5f, 1f); rtAct.anchorMax = new Vector2(0.5f, 1f);
            rtAct.anchoredPosition = new Vector2(0f, -325f);
            rtAct.sizeDelta = new Vector2(460f, 32f);
            TextMeshProUGUI tmpAct = nextActObj.GetComponent<TextMeshProUGUI>();
            tmpAct.text = "Mở khóa Hồi mới";
            tmpAct.fontSize = 22;
            tmpAct.fontStyle = FontStyles.Bold;
            tmpAct.alignment = TextAlignmentOptions.Center;
            tmpAct.color = ColorGold;
            nextActObj.SetActive(false);

            // 7. Revive Capped Notice
            GameObject revObj = new GameObject("ReviveCappedNotice", typeof(RectTransform), typeof(TextMeshProUGUI));
            revObj.transform.SetParent(card.transform, false);
            RectTransform rtRev = revObj.GetComponent<RectTransform>();
            rtRev.anchorMin = new Vector2(0.5f, 1f); rtRev.anchorMax = new Vector2(0.5f, 1f);
            rtRev.anchoredPosition = new Vector2(0f, -365f);
            rtRev.sizeDelta = new Vector2(460f, 28f);
            TextMeshProUGUI tmpRev = revObj.GetComponent<TextMeshProUGUI>();
            tmpRev.text = "Đã dùng hồi sinh: tối đa 1 sao";
            tmpRev.fontSize = 18;
            tmpRev.alignment = TextAlignmentOptions.Center;
            tmpRev.color = ColorSubText;
            revObj.SetActive(false);

            // 8. Buttons Container
            GameObject btnCol = new GameObject("ButtonsColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            btnCol.transform.SetParent(card.transform, false);
            RectTransform rtBtns = btnCol.GetComponent<RectTransform>();
            rtBtns.anchorMin = new Vector2(0.5f, 0f); rtBtns.anchorMax = new Vector2(0.5f, 0f);
            rtBtns.anchoredPosition = new Vector2(0f, 140f);
            rtBtns.sizeDelta = new Vector2(380f, 200f);
            VerticalLayoutGroup vlg = btnCol.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;

            GameObject btnNextObj = CreatePanelButton("BtnNextLevel", btnCol.transform, "MÀN TIẾP", ColorGold, Color.black, pill);
            GameObject btnReplayObj = CreatePanelButton("BtnReplay", btnCol.transform, "CHƠI LẠI", ColorBgDark, ColorCream, pill, true);
            GameObject btnSelectObj = CreatePanelButton("BtnSelectLevel", btnCol.transform, "CHỌN MÀN", ColorBgDark, ColorCream, pill, true);

            SerializedObject soHud = new SerializedObject(hud);
            soHud.FindProperty("victoryPanel").objectReferenceValue = victoryRoot;
            soHud.FindProperty("victoryTitleText").objectReferenceValue = tmpTitle;
            soHud.FindProperty("victoryRecordBadge").objectReferenceValue = recObj;
            soHud.FindProperty("victoryRunStarsText").objectReferenceValue = tmpRunS;
            soHud.FindProperty("victoryMilestoneRewardText").objectReferenceValue = tmpMs;
            soHud.FindProperty("victoryNextActBanner").objectReferenceValue = nextActObj;
            soHud.FindProperty("victoryReviveCappedNotice").objectReferenceValue = revObj;
            soHud.FindProperty("victoryNextLevelButton").objectReferenceValue = btnNextObj;
            soHud.FindProperty("victoryReplayButton").objectReferenceValue = btnReplayObj.GetComponent<Button>();
            soHud.FindProperty("victorySelectLevelButton").objectReferenceValue = btnSelectObj.GetComponent<Button>();

            SerializedProperty starArrProp = soHud.FindProperty("victoryStarImages");
            starArrProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                starArrProp.GetArrayElementAtIndex(i).objectReferenceValue = starImgs[i];
            }
            if (starSpr != null) soHud.FindProperty("starSprite").objectReferenceValue = starSpr;
            soHud.ApplyModifiedProperties();

            victoryRoot.SetActive(false);
        }

        private static void UpdateGameOverPanelUI(Transform canvasT, GameplayHUD hud)
        {
            Transform goPanel = canvasT.Find("GameOverPanel");
            if (goPanel == null) return;

            TextMeshProUGUI tmpGo = goPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmpGo != null && (tmpGo.text.Contains("GAME") || tmpGo.text.Contains("OVER")))
            {
                tmpGo.text = "THUA CUỘC";
            }

            Sprite pill = LoadSpriteAsset(SprPillPath);

            Transform keptStarsT = goPanel.Find("KeptStarsText");
            TextMeshProUGUI tmpKept;
            if (keptStarsT == null)
            {
                GameObject ksObj = new GameObject("KeptStarsText", typeof(RectTransform), typeof(TextMeshProUGUI));
                ksObj.transform.SetParent(goPanel, false);
                RectTransform rtKs = ksObj.GetComponent<RectTransform>();
                rtKs.anchorMin = new Vector2(0.5f, 0.5f); rtKs.anchorMax = new Vector2(0.5f, 0.5f);
                rtKs.anchoredPosition = new Vector2(0f, 60f);
                rtKs.sizeDelta = new Vector2(480f, 36f);
                tmpKept = ksObj.GetComponent<TextMeshProUGUI>();
                tmpKept.fontSize = 22;
                tmpKept.alignment = TextAlignmentOptions.Center;
                tmpKept.color = ColorGold;
                tmpKept.text = "Sao nhặt được (giữ 50%): +0 Sao";
            }
            else
            {
                tmpKept = keptStarsT.GetComponent<TextMeshProUGUI>();
            }

            Button btnGiveUp = null;
            Transform guT = goPanel.Find("GiveUpButton");
            if (guT != null)
            {
                btnGiveUp = guT.GetComponent<Button>();
                Image imgGu = guT.GetComponent<Image>();
                if (imgGu != null && pill != null) { imgGu.sprite = pill; imgGu.type = Image.Type.Sliced; }
            }

            Button btnReplay = null;
            Transform repT = goPanel.Find("ReplayButton");
            if (repT == null)
            {
                GameObject repObj = CreatePanelButton("ReplayButton", goPanel, "CHƠI LẠI", ColorBgDark, ColorCream, pill, true);
                RectTransform rtRep = repObj.GetComponent<RectTransform>();
                rtRep.anchorMin = new Vector2(0.5f, 0.5f); rtRep.anchorMax = new Vector2(0.5f, 0.5f);
                rtRep.anchoredPosition = new Vector2(0f, -70f);
                btnReplay = repObj.GetComponent<Button>();
            }
            else
            {
                btnReplay = repT.GetComponent<Button>();
                Image imgRep = repT.GetComponent<Image>();
                if (imgRep != null && pill != null) { imgRep.sprite = pill; imgRep.type = Image.Type.Sliced; }
            }

            Transform revT = goPanel.Find("ReviveButton");
            if (revT != null)
            {
                Image imgRev = revT.GetComponent<Image>();
                if (imgRev != null && pill != null) { imgRev.sprite = pill; imgRev.type = Image.Type.Sliced; }
            }

            SerializedObject soHud = new SerializedObject(hud);
            if (tmpGo != null) soHud.FindProperty("gameOverTitleText").objectReferenceValue = tmpGo;
            soHud.FindProperty("gameOverRunStarsText").objectReferenceValue = tmpKept;
            if (btnReplay != null) soHud.FindProperty("gameOverReplayButton").objectReferenceValue = btnReplay;
            if (btnGiveUp != null) soHud.FindProperty("gameOverGiveUpButton").objectReferenceValue = btnGiveUp;
            soHud.ApplyModifiedProperties();
        }

        private static GameObject CreatePanelButton(string name, Transform parent, string label, Color bgColor, Color textColor, Sprite pillSprite, bool hasBorder = false)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(340f, 52f);

            Image img = btnObj.GetComponent<Image>();
            if (pillSprite != null) { img.sprite = pillSprite; img.type = Image.Type.Sliced; }
            img.color = bgColor;

            if (hasBorder)
            {
                Outline outline = btnObj.AddComponent<Outline>();
                outline.effectColor = ColorBorder;
                outline.effectDistance = new Vector2(1.5f, 1.5f);
            }

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform rtText = textObj.GetComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero; rtText.anchorMax = Vector2.one; rtText.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;

            return btnObj;
        }

        private static void SetupBossHUD(Transform canvasT)
        {
            Transform existingBossHUD = canvasT.Find("BossHUD");
            GameObject bossHudObj;
            if (existingBossHUD == null)
            {
                bossHudObj = new GameObject("BossHUD", typeof(RectTransform), typeof(BossHUD));
                bossHudObj.transform.SetParent(canvasT, false);
            }
            else
            {
                bossHudObj = existingBossHUD.gameObject;
                if (bossHudObj.GetComponent<BossHUD>() == null)
                    bossHudObj.AddComponent<BossHUD>();
            }

            RectTransform rtBossHUD = bossHudObj.GetComponent<RectTransform>();
            rtBossHUD.anchorMin = Vector2.zero;
            rtBossHUD.anchorMax = Vector2.one;
            rtBossHUD.offsetMin = Vector2.zero;
            rtBossHUD.offsetMax = Vector2.zero;
            BossHUD bossHudComp = bossHudObj.GetComponent<BossHUD>();

            // Panel: anchored top-center (0.5, 1), pos (0, -180), size (756, 28)
            Transform panelT = bossHudObj.transform.Find("BossHUDPanel");
            GameObject panelObj;
            if (panelT == null)
            {
                panelObj = new GameObject("BossHUDPanel", typeof(RectTransform), typeof(Image));
                panelObj.transform.SetParent(bossHudObj.transform, false);
            }
            else
            {
                panelObj = panelT.gameObject;
            }

            RectTransform rtPanel = panelObj.GetComponent<RectTransform>();
            rtPanel.anchorMin = new Vector2(0.5f, 1f);
            rtPanel.anchorMax = new Vector2(0.5f, 1f);
            rtPanel.pivot = new Vector2(0.5f, 1f);
            rtPanel.anchoredPosition = new Vector2(0f, -180f);
            rtPanel.sizeDelta = new Vector2(756f, 28f);

            Sprite round36 = LoadSpriteAsset(SprRound36Path);
            Image imgPanel = panelObj.GetComponent<Image>();
            if (round36 != null) { imgPanel.sprite = round36; imgPanel.type = Image.Type.Sliced; }
            imgPanel.color = new Color(0.063f, 0.102f, 0.235f, 0.9f); // #101A3C alpha 0.9

            // Slider: BossHPSlider
            Transform sliderT = panelObj.transform.Find("BossHPSlider");
            GameObject sliderObj;
            if (sliderT == null)
            {
                sliderObj = new GameObject("BossHPSlider", typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(panelObj.transform, false);
            }
            else
            {
                sliderObj = sliderT.gameObject;
            }
            Slider bossSlider = sliderObj.GetComponent<Slider>();
            RectTransform rtSlider = sliderObj.GetComponent<RectTransform>();
            rtSlider.anchorMin = Vector2.zero;
            rtSlider.anchorMax = Vector2.one;
            rtSlider.offsetMin = new Vector2(2f, 2f);
            rtSlider.offsetMax = new Vector2(-2f, -2f);

            // Fill Area & Fill (Red #FF5A5F)
            Transform fillArea = sliderObj.transform.Find("Fill Area");
            GameObject fillAreaObj;
            if (fillArea == null)
            {
                fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
                fillAreaObj.transform.SetParent(sliderObj.transform, false);
            }
            else
            {
                fillAreaObj = fillArea.gameObject;
            }
            RectTransform rtFA = fillAreaObj.GetComponent<RectTransform>();
            rtFA.anchorMin = Vector2.zero;
            rtFA.anchorMax = Vector2.one;
            rtFA.sizeDelta = Vector2.zero;

            Transform fillT = fillAreaObj.transform.Find("Fill");
            GameObject fillObj;
            if (fillT == null)
            {
                fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(fillAreaObj.transform, false);
            }
            else
            {
                fillObj = fillT.gameObject;
            }
            RectTransform rtFill = fillObj.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.sizeDelta = Vector2.zero;
            Image fillImg = fillObj.GetComponent<Image>();
            if (round36 != null) { fillImg.sprite = round36; fillImg.type = Image.Type.Sliced; }
            fillImg.color = new Color(1f, 0.353f, 0.373f, 1f); // #FF5A5F Red
            bossSlider.fillRect = rtFill;

            // Phase ticks at 66% and 33%
            Transform tick66T = panelObj.transform.Find("Tick66");
            GameObject tick66Obj;
            if (tick66T == null)
            {
                tick66Obj = new GameObject("Tick66", typeof(RectTransform), typeof(Image));
                tick66Obj.transform.SetParent(panelObj.transform, false);
            }
            else
            {
                tick66Obj = tick66T.gameObject;
            }
            RectTransform rt66 = tick66Obj.GetComponent<RectTransform>();
            rt66.anchorMin = new Vector2(0.5f, 0.5f);
            rt66.anchorMax = new Vector2(0.5f, 0.5f);
            rt66.anchoredPosition = new Vector2(121f, 0f);
            rt66.sizeDelta = new Vector2(3f, 28f);
            tick66Obj.GetComponent<Image>().color = ColorCream;

            Transform tick33T = panelObj.transform.Find("Tick33");
            GameObject tick33Obj;
            if (tick33T == null)
            {
                tick33Obj = new GameObject("Tick33", typeof(RectTransform), typeof(Image));
                tick33Obj.transform.SetParent(panelObj.transform, false);
            }
            else
            {
                tick33Obj = tick33T.gameObject;
            }
            RectTransform rt33 = tick33Obj.GetComponent<RectTransform>();
            rt33.anchorMin = new Vector2(0.5f, 0.5f);
            rt33.anchorMax = new Vector2(0.5f, 0.5f);
            rt33.anchoredPosition = new Vector2(-128.5f, 0f);
            rt33.sizeDelta = new Vector2(3f, 28f);
            tick33Obj.GetComponent<Image>().color = ColorCream;

            // BossNameText above slider: (0, 24), size 24 Bold, #F4F1E6
            Transform nameT = panelObj.transform.Find("BossNameText");
            GameObject nameObj;
            if (nameT == null)
            {
                nameObj = new GameObject("BossNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(panelObj.transform, false);
            }
            else
            {
                nameObj = nameT.gameObject;
            }
            RectTransform rtName = nameObj.GetComponent<RectTransform>();
            rtName.anchorMin = new Vector2(0.5f, 1f);
            rtName.anchorMax = new Vector2(0.5f, 1f);
            rtName.pivot = new Vector2(0.5f, 0f);
            rtName.anchoredPosition = new Vector2(0f, 6f);
            rtName.sizeDelta = new Vector2(756f, 32f);
            TextMeshProUGUI tmpName = nameObj.GetComponent<TextMeshProUGUI>();
            tmpName.fontSize = 24;
            tmpName.fontStyle = FontStyles.Bold;
            tmpName.alignment = TextAlignmentOptions.Center;
            tmpName.color = ColorCream;
            tmpName.text = "TRÙM";

            // Wire SerializedObject
            SerializedObject soBoss = new SerializedObject(bossHudComp);
            soBoss.FindProperty("hpSlider").objectReferenceValue = bossSlider;
            soBoss.FindProperty("hudPanel").objectReferenceValue = panelObj;
            soBoss.FindProperty("bossNameText").objectReferenceValue = tmpName;
            soBoss.FindProperty("phase66Tick").objectReferenceValue = rt66;
            soBoss.FindProperty("phase33Tick").objectReferenceValue = rt33;
            soBoss.ApplyModifiedProperties();

            panelObj.SetActive(false);
        }
    }
}
