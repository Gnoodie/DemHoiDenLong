using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    /// <summary>
    /// Script Editor tự động xây dựng prefabs LevelNode, BossNode và lắp ráp toàn bộ màn LevelDetailView
    /// trong Scene LevelSelect theo đúng đặc tả thiết kế.
    /// </summary>
    public static class BuildLevelDetailView
    {
        private const string ScenePath = "Assets/Scenes/LevelSelect.unity";
        private const string PrefabDir = "Assets/_Project/Prefabs/UI";
        private const string LevelNodePrefabPath = "Assets/_Project/Prefabs/UI/LevelNode.prefab";
        private const string BossNodePrefabPath = "Assets/_Project/Prefabs/UI/BossNode.prefab";

        // Asset paths
        private const string SprRound36Path = "Assets/_Project/Sprites/UI/ui_round_36.png";
        private const string SprPillPath = "Assets/_Project/Sprites/UI/ui_pill.png";
        private const string SprCirclePath = "Assets/_Project/Sprites/UI/ui_circle.png";
        private const string SprGlowRingPath = "Assets/_Project/Sprites/UI/ui_glow_ring.png";
        private const string SprGradFadePath = "Assets/_Project/Sprites/UI/ui_gradient_fade.png";
        private const string SprKimCuongPath = "Assets/_Project/Sprites/UI/kimcuong.png";
        private const string SprCheckPath = "Assets/_Project/Sprites/UI/ui_check.png";
        private const string SprPlayPath = "Assets/_Project/Sprites/UI/ui_play.png";
        private const string SprStarPath = "Assets/_Project/Art/Icons/star.png";
        private const string SprLockPath = "Assets/_Project/Sprites/UI/ui_lock.png";
        private const string SprClosePath = "Assets/_Project/Art/Icons/cross.png";

        private const string BgVillagePath = "Assets/_Project/Art/Backgrounds/bg_village_map.png";
        private const string BgOldtownPath = "Assets/_Project/Art/Backgrounds/bg_oldtown_map.png";
        private const string BgMoonPath = "Assets/_Project/Art/Backgrounds/bg_moon_map.png";

        private const string NodeBasePath = "Assets/_Project/Art/LevelSelect/node_base.png";
        private const string BossAct1Path = "Assets/_Project/Art/LevelSelect/node_boss_act1.png";
        private const string BossAct2Path = "Assets/_Project/Art/LevelSelect/node_boss_act2.png";
        private const string BossAct3Path = "Assets/_Project/Art/LevelSelect/node_boss_act3.png";

        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        // Colors
        private static readonly Color ColorGold = new Color(1f, 0.824f, 0.478f, 1f);        // #FFD27A
        private static readonly Color ColorApricot = new Color(1f, 0.706f, 0.42f, 1f);      // #FFB46B
        private static readonly Color ColorCream = new Color(0.957f, 0.945f, 0.902f, 1f);   // #F4F1E6
        private static readonly Color ColorDarkBrown = new Color(0.227f, 0.133f, 0f, 1f);   // #3A2200
        private static readonly Color ColorCyan = new Color(0.498f, 0.91f, 0.961f, 1f);     // #7FE8F5
        private static readonly Color ColorChipBg = new Color(0.11f, 0.165f, 0.361f, 1f);   // #1C2A5C
        private static readonly Color ColorCardBg = new Color(0.086f, 0.133f, 0.29f, 1f);   // #16224A
        private static readonly Color ColorRowBg = new Color(0.063f, 0.102f, 0.235f, 1f);   // #101A3C
        private static readonly Color ColorBadgeRed = new Color(0.949f, 0.329f, 0.176f, 1f); // #F2542D
        private static readonly Color ColorGreen = new Color(0.18f, 0.62f, 0.478f, 1f);     // #2E9E7A

        [InitializeOnLoadMethod]
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AutoRunOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying)
                {
                    GenerateUISprites.GenerateAllSprites();
                    BuildAll();
                    DemHoiDenLong.Editor.FindMissingGlyphs.ScanMissingGlyphs();
                }
            };
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ConfigureTextureIfNeeded(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool needReimport = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    needReimport = true;
                }
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    needReimport = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    needReimport = true;
                }
                if (importer.maxTextureSize < 2048)
                {
                    importer.maxTextureSize = 2048;
                    needReimport = true;
                }
                if (needReimport)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        [MenuItem("Tools/Build LevelDetailView")]
        [MenuItem("Tools/DemHoiDenLong/Build LevelDetailView")]
        [MenuItem("DemHoiDenLong/Build LevelDetailView")]
        public static void BuildAll()
        {
            Debug.Log("[BuildLevelDetailView] === BẮT ĐẦU XÂY DỰNG LEVELDETAILVIEW ===");

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
            }

            // 1. Tải toàn bộ tài nguyên bằng LoadSprite an toàn cho sub-assets
            ConfigureTextureIfNeeded(NodeBasePath);
            ConfigureTextureIfNeeded(BossAct1Path);
            ConfigureTextureIfNeeded(BossAct2Path);
            ConfigureTextureIfNeeded(BossAct3Path);

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            Sprite sprRound36 = LoadSprite(SprRound36Path);
            Sprite sprPill = LoadSprite(SprPillPath);
            Sprite sprCircle = LoadSprite(SprCirclePath);
            Sprite sprGlowRing = LoadSprite(SprGlowRingPath);
            Sprite sprGradFade = LoadSprite(SprGradFadePath);
            Sprite sprKimCuong = LoadSprite(SprKimCuongPath);
            Sprite sprCheck = LoadSprite(SprCheckPath);
            Sprite sprPlay = LoadSprite(SprPlayPath);
            Sprite sprStar = LoadSprite(SprStarPath);
            Sprite sprLock = LoadSprite(SprLockPath);
            Sprite sprClose = LoadSprite(SprClosePath);

            Sprite sprNodeBase = LoadSprite(NodeBasePath);

            Sprite bgVillage = LoadSprite(BgVillagePath);
            Sprite bgOldtown = LoadSprite(BgOldtownPath);
            Sprite bgMoon = LoadSprite(BgMoonPath);

            Sprite boss1 = LoadSprite(BossAct1Path);
            Sprite boss2 = LoadSprite(BossAct2Path);
            Sprite boss3 = LoadSprite(BossAct3Path);

            LanData[] lans = new LanData[]
            {
                AssetDatabase.LoadAssetAtPath<LanData>("Assets/_Project/Data/Lans/1.asset"),
                AssetDatabase.LoadAssetAtPath<LanData>("Assets/_Project/Data/Lans/2.asset"),
                AssetDatabase.LoadAssetAtPath<LanData>("Assets/_Project/Data/Lans/3.asset"),
                AssetDatabase.LoadAssetAtPath<LanData>("Assets/_Project/Data/Lans/4.asset")
            };

            // 2. Tạo hoặc cập nhật prefab LevelNode với sprite node_base thật
            GameObject levelNodePrefab = BuildLevelNodePrefab(sprNodeBase, sprGlowRing, sprStar, sprLock, sprPill, font);

            // 3. Tạo hoặc cập nhật prefab BossNode với sprite node_boss thật
            GameObject bossNodePrefab = BuildBossNodePrefab(boss1, sprGlowRing, sprStar, sprLock, sprPill, font);

            // 4. Mở Scene LevelSelect
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            // Dọn dẹp missing scripts và chuẩn hóa EventSystem cho New Input System
            EnsureEventSystemAndCleanMissingScripts(activeScene);

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[BuildLevelDetailView] Không tìm thấy Canvas trong Scene LevelSelect!");
                return;
            }

            Transform actSelectViewTrans = canvas.transform.Find("ActSelectView");
            if (actSelectViewTrans == null)
            {
                Debug.LogError("[BuildLevelDetailView] Không tìm thấy ActSelectView trong Canvas!");
                return;
            }

            CanvasGroup actSelectCg = actSelectViewTrans.GetComponent<CanvasGroup>();
            if (actSelectCg == null)
            {
                actSelectCg = actSelectViewTrans.gameObject.AddComponent<CanvasGroup>();
            }

            // 5. Xây dựng hoặc tái sử dụng LevelDetailView dưới Canvas
            Transform detailViewTrans = canvas.transform.Find("LevelDetailView");
            GameObject objDetailView = (detailViewTrans != null) ? detailViewTrans.gameObject : new GameObject("LevelDetailView");
            objDetailView.transform.SetParent(canvas.transform, false);

            RectTransform rtDetailView = objDetailView.GetComponent<RectTransform>();
            if (rtDetailView == null) rtDetailView = objDetailView.AddComponent<RectTransform>();
            rtDetailView.anchorMin = Vector2.zero;
            rtDetailView.anchorMax = Vector2.one;
            rtDetailView.sizeDelta = Vector2.zero;
            rtDetailView.anchoredPosition = Vector2.zero;

            CanvasGroup detailCg = objDetailView.GetComponent<CanvasGroup>();
            if (detailCg == null) detailCg = objDetailView.AddComponent<CanvasGroup>();

            LevelDetailView view = objDetailView.GetComponent<LevelDetailView>();
            if (view == null) view = objDetailView.AddComponent<LevelDetailView>();

            view.canvasGroup = detailCg;
            view.levelNodePrefab = levelNodePrefab;
            view.bossNodePrefab = bossNodePrefab;
            view.bgVillageMap = bgVillage;
            view.bgOldtownMap = bgOldtown;
            view.bgMoonMap = bgMoon;
            view.bossAct1 = boss1;
            view.bossAct2 = boss2;
            view.bossAct3 = boss3;

            // Xóa các con cũ để tái xây dựng chuẩn xác
            while (objDetailView.transform.childCount > 0)
            {
                Object.DestroyImmediate(objDetailView.transform.GetChild(0).gameObject);
            }

            // 5.1. BackgroundRoot
            GameObject objBgRoot = new GameObject("BackgroundRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(AspectRatioFitter));
            objBgRoot.transform.SetParent(objDetailView.transform, false);
            RectTransform rtBgRoot = objBgRoot.GetComponent<RectTransform>();
            rtBgRoot.anchorMin = new Vector2(0.5f, 1f);
            rtBgRoot.anchorMax = new Vector2(0.5f, 1f);
            rtBgRoot.pivot = new Vector2(0.5f, 1f);
            rtBgRoot.sizeDelta = new Vector2(1080f, 1920f);
            rtBgRoot.anchoredPosition = Vector2.zero;

            Image imgBg = objBgRoot.GetComponent<Image>();
            imgBg.sprite = bgOldtown; // Mặc định Hồi 2
            imgBg.type = Image.Type.Simple;
            imgBg.raycastTarget = false;

            AspectRatioFitter arf = objBgRoot.GetComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            if (bgOldtown != null) arf.aspectRatio = (float)bgOldtown.rect.width / bgOldtown.rect.height;

            view.backgroundImage = imgBg;
            view.backgroundFitter = arf;

            // 5.1.1. Dim (lớp đen alpha 0.22)
            GameObject objDim = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objDim.transform.SetParent(objBgRoot.transform, false);
            RectTransform rtDim = objDim.GetComponent<RectTransform>();
            rtDim.anchorMin = Vector2.zero;
            rtDim.anchorMax = Vector2.one;
            rtDim.sizeDelta = Vector2.zero;
            Image imgDim = objDim.GetComponent<Image>();
            imgDim.color = new Color(0f, 0f, 0f, 0.22f);
            imgDim.raycastTarget = false;

            // 5.1.2. NodesContainer (con của BackgroundRoot, stretch full)
            GameObject objNodes = new GameObject("NodesContainer", typeof(RectTransform));
            objNodes.transform.SetParent(objBgRoot.transform, false);
            RectTransform rtNodes = objNodes.GetComponent<RectTransform>();
            rtNodes.anchorMin = Vector2.zero;
            rtNodes.anchorMax = Vector2.one;
            rtNodes.sizeDelta = Vector2.zero;
            view.nodesContainer = rtNodes;

            // 5.1.2.1. PathRoot (vẽ trước các node)
            GameObject objPathRoot = new GameObject("PathRoot", typeof(RectTransform), typeof(PathDrawer));
            objPathRoot.transform.SetParent(objNodes.transform, false);
            RectTransform rtPath = objPathRoot.GetComponent<RectTransform>();
            rtPath.anchorMin = Vector2.zero;
            rtPath.anchorMax = Vector2.one;
            rtPath.sizeDelta = Vector2.zero;

            PathDrawer pathDrawer = objPathRoot.GetComponent<PathDrawer>();
            pathDrawer.dashSprite = sprPill;
            view.pathDrawer = pathDrawer;

            // Tạo sẵn 5 Node con mẫu của Hồi 2
            Vector2[] defaultAnchors = new Vector2[]
            {
                new Vector2(0.42f, 0.15f), // Node 1 (Màn 6)
                new Vector2(0.64f, 0.30f), // Node 2 (Màn 7)
                new Vector2(0.38f, 0.46f), // Node 3 (Màn 8)
                new Vector2(0.66f, 0.63f), // Node 4 (Màn 9)
                new Vector2(0.50f, 0.80f)  // Node 5 (Boss Màn 10)
            };

            List<Transform> initialNodes = new List<Transform>();
            for (int i = 0; i < 5; i++)
            {
                bool isBossNode = (i == 4);
                GameObject prefab = isBossNode ? bossNodePrefab : levelNodePrefab;
                GameObject nodeObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, objNodes.transform);
                nodeObj.name = isBossNode ? "Node_Boss" : $"Node_{i + 1}";

                RectTransform rtN = nodeObj.GetComponent<RectTransform>();
                rtN.anchorMin = defaultAnchors[i];
                rtN.anchorMax = defaultAnchors[i];
                rtN.pivot = new Vector2(0.5f, 0.5f);
                rtN.anchoredPosition = Vector2.zero;

                initialNodes.Add(nodeObj.transform);
            }

            pathDrawer.nodeTransforms = initialNodes;
            pathDrawer.nodeStates = new List<NodeState>
            {
                NodeState.Unlocked,
                NodeState.Unlocked,
                NodeState.Current,
                NodeState.Locked,
                NodeState.Locked
            };

            // 5.2. TopFade (ui_gradient_fade đen alpha 0.85, cao ~340, đảo chiều)
            GameObject objTopFade = new GameObject("TopFade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objTopFade.transform.SetParent(objDetailView.transform, false);
            RectTransform rtTopFade = objTopFade.GetComponent<RectTransform>();
            rtTopFade.anchorMin = new Vector2(0f, 1f);
            rtTopFade.anchorMax = new Vector2(1f, 1f);
            rtTopFade.pivot = new Vector2(0.5f, 1f);
            rtTopFade.anchoredPosition = Vector2.zero;
            rtTopFade.sizeDelta = new Vector2(0f, 340f);
            rtTopFade.localScale = new Vector3(1f, -1f, 1f); // Đảo chiều gradient

            Image imgTopFade = objTopFade.GetComponent<Image>();
            imgTopFade.sprite = sprGradFade;
            imgTopFade.color = new Color(0f, 0f, 0f, 0.85f);
            imgTopFade.raycastTarget = false;

            // 5.3. SafeAreaRoot
            GameObject objSafeArea = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeArea));
            objSafeArea.transform.SetParent(objDetailView.transform, false);
            RectTransform rtSafeArea = objSafeArea.GetComponent<RectTransform>();
            rtSafeArea.anchorMin = Vector2.zero;
            rtSafeArea.anchorMax = Vector2.one;
            rtSafeArea.sizeDelta = Vector2.zero;

            // 5.3.1. TopBar
            BuildTopBar(objSafeArea, sprPill, sprStar, sprKimCuong, sprCircle, font);

            // 5.3.2. HeaderBar
            BuildHeaderBar(objSafeArea, view, sprRound36, sprPill, sprStar, font);

            // 5.4. ToastRoot
            BuildToast(objDetailView, view, sprPill, font);

            // 5.5. ModalOverlay
            BuildModalOverlay(objDetailView, view, sprRound36, sprPill, sprCircle, sprGlowRing, sprStar, sprKimCuong, sprLock, sprCheck, sprPlay, sprClose, font, lans);

            // 6. LevelSelectController và LevelSelectFlow
            Transform controllerTrans = canvas.transform.Find("LevelSelectController");
            GameObject objController = (controllerTrans != null) ? controllerTrans.gameObject : new GameObject("LevelSelectController");
            objController.transform.SetParent(canvas.transform, false);

            LevelSelectFlow flow = objController.GetComponent<LevelSelectFlow>();
            if (flow == null) flow = objController.AddComponent<LevelSelectFlow>();

            MockLevelDataProvider provider = objController.GetComponent<MockLevelDataProvider>();
            if (provider == null) provider = objController.AddComponent<MockLevelDataProvider>();

            provider.bgAct1 = bgVillage;
            provider.bgAct2 = bgOldtown;
            provider.bgAct3 = bgMoon;
            provider.bossAct1 = boss1;
            provider.bossAct2 = boss2;
            provider.bossAct3 = boss3;

            view.bgVillageMap = bgVillage;
            view.bgOldtownMap = bgOldtown;
            view.bgMoonMap = bgMoon;
            view.bossAct1 = boss1;
            view.bossAct2 = boss2;
            view.bossAct3 = boss3;
            view.levelNodePrefab = levelNodePrefab;
            view.bossNodePrefab = bossNodePrefab;

            flow.actSelectView = actSelectViewTrans.gameObject;
            flow.actSelectCanvasGroup = actSelectCg;
            flow.levelDetailView = view;
            flow.dataProvider = provider;

            // Ban đầu ẩn LevelDetailView
            objDetailView.SetActive(false);
            detailCg.alpha = 0f;

            // Đảm bảo dọn sạch missing scripts lần cuối trước khi lưu scene
            EnsureEventSystemAndCleanMissingScripts(EditorSceneManager.GetActiveScene());

            // Đánh dấu Scene bẩn và lưu
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("[BuildLevelDetailView] === HOÀN TẤT XÂY DỰNG LEVELDETAILVIEW THÀNH CÔNG ===");
        }

        [MenuItem("Tools/Clean Missing Scripts and Fix EventSystem")]
        [MenuItem("DemHoiDenLong/Clean Missing Scripts and Fix EventSystem")]
        public static void CleanMissingScriptsMenu()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            EnsureEventSystemAndCleanMissingScripts(activeScene);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[CleanMissingScripts] === HOÀN TẤT DỌN DẸP MISSING SCRIPTS VÀ SỬA EVENTSYSTEM ===");
        }

        private static void EnsureEventSystemAndCleanMissingScripts(UnityEngine.SceneManagement.Scene scene)
        {
            // 1. Quét và loại bỏ missing scripts trên toàn bộ GameObject trong Scene
            int removedSceneScripts = 0;
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                {
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if (count > 0)
                    {
                        removedSceneScripts += count;
                        Debug.LogWarning($"[CleanMissingScripts] Xóa {count} missing script khỏi: {t.gameObject.name}");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    }
                }
            }
            if (removedSceneScripts > 0)
            {
                Debug.Log($"[CleanMissingScripts] Đã xóa tổng cộng {removedSceneScripts} missing scripts trong Scene.");
            }

            // 2. Quét và loại bỏ missing scripts trong Prefabs UI
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = PrefabUtility.LoadPrefabContents(path);
                bool modified = false;
                foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
                {
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if (count > 0)
                    {
                        modified = true;
                        Debug.LogWarning($"[CleanMissingScripts] Xóa {count} missing script khỏi Prefab {path} -> {t.gameObject.name}");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    }
                }
                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }
                PrefabUtility.UnloadPrefabContents(prefab);
            }

            // 3. Đảm bảo EventSystem được thiết lập chuẩn xác với New Input System
            UnityEngine.EventSystems.EventSystem es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            // Gỡ bỏ StandaloneInputModule nếu có vì xung đột với New Input System
            var standalone = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (standalone != null)
            {
                Object.DestroyImmediate(standalone);
            }

#if ENABLE_INPUT_SYSTEM
            var inputModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            inputModule.AssignDefaultActions();
            inputModule.enabled = true;
#endif
            es.enabled = true;
            EditorUtility.SetDirty(es.gameObject);
        }

        private static GameObject BuildLevelNodePrefab(Sprite sprBase, Sprite sprGlowRing, Sprite sprStar, Sprite sprLock, Sprite sprPill, TMP_FontAsset font)
        {
            GameObject root = new GameObject("LevelNode", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(LevelNodeUI));
            RectTransform rtRoot = root.GetComponent<RectTransform>();
            rtRoot.sizeDelta = new Vector2(150f, 150f);

            Image rootImg = root.GetComponent<Image>();
            rootImg.color = Color.clear;
            rootImg.raycastTarget = true;

            Button btn = root.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = rootImg;

            CanvasGroup cg = root.GetComponent<CanvasGroup>();
            LevelNodeUI ui = root.GetComponent<LevelNodeUI>();
            ui.button = btn;
            ui.canvasGroup = cg;
            ui.isBoss = false;

            // Visual container
            GameObject objVisual = new GameObject("Visual", typeof(RectTransform));
            objVisual.transform.SetParent(root.transform, false);
            RectTransform rtVisual = objVisual.GetComponent<RectTransform>();
            rtVisual.anchorMin = new Vector2(0.5f, 0.5f);
            rtVisual.anchorMax = new Vector2(0.5f, 0.5f);
            rtVisual.pivot = new Vector2(0.5f, 0.5f);
            rtVisual.sizeDelta = new Vector2(150f, 150f);
            ui.visualContainer = rtVisual;

            // GlowRing (180x180)
            GameObject objGlow = new GameObject("GlowRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objGlow.transform.SetParent(objVisual.transform, false);
            RectTransform rtGlow = objGlow.GetComponent<RectTransform>();
            rtGlow.sizeDelta = new Vector2(180f, 180f);
            Image imgGlow = objGlow.GetComponent<Image>();
            imgGlow.sprite = sprGlowRing;
            imgGlow.raycastTarget = false;
            ui.glowRing = imgGlow;
            objGlow.SetActive(false);

            // Base Image (node_base.png, 150x150)
            GameObject objBase = new GameObject("Base", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objBase.transform.SetParent(objVisual.transform, false);
            RectTransform rtBase = objBase.GetComponent<RectTransform>();
            rtBase.sizeDelta = new Vector2(150f, 150f);
            Image imgBase = objBase.GetComponent<Image>();
            imgBase.sprite = sprBase;
            imgBase.color = Color.white;
            imgBase.raycastTarget = true;
            ui.baseImage = imgBase;

            // NumberText (TMP_Text 64 Bold, kem hoặc nâu #3A2200)
            GameObject objNum = new GameObject("NumberText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objNum.transform.SetParent(objVisual.transform, false);
            RectTransform rtNum = objNum.GetComponent<RectTransform>();
            rtNum.sizeDelta = new Vector2(120f, 80f);
            TextMeshProUGUI tmpNum = objNum.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpNum.font = font;
            tmpNum.fontSize = 64f;
            tmpNum.fontStyle = FontStyles.Bold;
            tmpNum.color = ColorDarkBrown;
            tmpNum.alignment = TextAlignmentOptions.Center;
            tmpNum.text = "1";
            tmpNum.raycastTarget = false;
            ui.numberText = tmpNum;

            // LockIcon (Image locked.png, 48x48)
            GameObject objLock = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objLock.transform.SetParent(objVisual.transform, false);
            RectTransform rtLock = objLock.GetComponent<RectTransform>();
            rtLock.sizeDelta = new Vector2(48f, 48f);
            Image imgLock = objLock.GetComponent<Image>();
            imgLock.sprite = sprLock;
            imgLock.raycastTarget = false;
            ui.lockIcon = objLock;
            objLock.SetActive(false);

            // NewBadge (ui_pill đỏ cam #F2542D, chữ MỚI 20 Bold kem)
            GameObject objNewBadge = new GameObject("NewBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objNewBadge.transform.SetParent(objVisual.transform, false);
            RectTransform rtNew = objNewBadge.GetComponent<RectTransform>();
            rtNew.anchorMin = new Vector2(1f, 1f);
            rtNew.anchorMax = new Vector2(1f, 1f);
            rtNew.pivot = new Vector2(0.5f, 0.5f);
            rtNew.anchoredPosition = new Vector2(-10f, -10f);
            rtNew.sizeDelta = new Vector2(68f, 32f);
            Image imgNew = objNewBadge.GetComponent<Image>();
            imgNew.sprite = sprPill;
            imgNew.type = Image.Type.Sliced;
            imgNew.color = ColorBadgeRed;
            imgNew.raycastTarget = false;

            GameObject objNewTxt = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objNewTxt.transform.SetParent(objNewBadge.transform, false);
            RectTransform rtNewTxt = objNewTxt.GetComponent<RectTransform>();
            rtNewTxt.sizeDelta = new Vector2(68f, 32f);
            TextMeshProUGUI tmpNew = objNewTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpNew.font = font;
            tmpNew.fontSize = 20f;
            tmpNew.fontStyle = FontStyles.Bold;
            tmpNew.color = ColorCream;
            tmpNew.alignment = TextAlignmentOptions.Center;
            tmpNew.text = "MỚI";
            tmpNew.raycastTarget = false;

            ui.newBadge = objNewBadge;
            objNewBadge.SetActive(false);

            // StarsRow (3 star icons 38x38)
            GameObject objStarsRow = new GameObject("StarsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            objStarsRow.transform.SetParent(objVisual.transform, false);
            RectTransform rtStars = objStarsRow.GetComponent<RectTransform>();
            rtStars.anchorMin = new Vector2(0.5f, 0f);
            rtStars.anchorMax = new Vector2(0.5f, 0f);
            rtStars.pivot = new Vector2(0.5f, 1f);
            rtStars.anchoredPosition = new Vector2(0f, -8f);
            rtStars.sizeDelta = new Vector2(130f, 40f);

            HorizontalLayoutGroup hlg = objStarsRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            Image[] starImgs = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject($"Star_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                starObj.transform.SetParent(objStarsRow.transform, false);
                RectTransform rtS = starObj.GetComponent<RectTransform>();
                rtS.sizeDelta = new Vector2(38f, 38f);
                Image imgS = starObj.GetComponent<Image>();
                imgS.sprite = sprStar;
                imgS.color = ColorGold;
                imgS.raycastTarget = false;
                starImgs[i] = imgS;
            }
            ui.starsRow = objStarsRow;
            ui.starIcons = starImgs;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, LevelNodePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildBossNodePrefab(Sprite sprBossDefault, Sprite sprGlowRing, Sprite sprStar, Sprite sprLock, Sprite sprPill, TMP_FontAsset font)
        {
            GameObject root = new GameObject("BossNode", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(LevelNodeUI));
            RectTransform rtRoot = root.GetComponent<RectTransform>();
            rtRoot.sizeDelta = new Vector2(200f, 200f);

            Image rootImg = root.GetComponent<Image>();
            rootImg.color = Color.clear;
            rootImg.raycastTarget = true;

            Button btn = root.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = rootImg;

            CanvasGroup cg = root.GetComponent<CanvasGroup>();
            LevelNodeUI ui = root.GetComponent<LevelNodeUI>();
            ui.button = btn;
            ui.canvasGroup = cg;
            ui.isBoss = true;

            // Visual container
            GameObject objVisual = new GameObject("Visual", typeof(RectTransform));
            objVisual.transform.SetParent(root.transform, false);
            RectTransform rtVisual = objVisual.GetComponent<RectTransform>();
            rtVisual.anchorMin = new Vector2(0.5f, 0.5f);
            rtVisual.anchorMax = new Vector2(0.5f, 0.5f);
            rtVisual.pivot = new Vector2(0.5f, 0.5f);
            rtVisual.sizeDelta = new Vector2(200f, 200f);
            ui.visualContainer = rtVisual;

            // GlowRing (240x240)
            GameObject objGlow = new GameObject("GlowRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objGlow.transform.SetParent(objVisual.transform, false);
            RectTransform rtGlow = objGlow.GetComponent<RectTransform>();
            rtGlow.sizeDelta = new Vector2(240f, 240f);
            Image imgGlow = objGlow.GetComponent<Image>();
            imgGlow.sprite = sprGlowRing;
            imgGlow.raycastTarget = false;
            ui.glowRing = imgGlow;
            objGlow.SetActive(false);

            // Boss Image hợp nhất cả khung và boss (node_boss_actX, 200x200)
            GameObject objBossImg = new GameObject("BossImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objBossImg.transform.SetParent(objVisual.transform, false);
            RectTransform rtBossImg = objBossImg.GetComponent<RectTransform>();
            rtBossImg.sizeDelta = new Vector2(200f, 200f);
            Image imgBoss = objBossImg.GetComponent<Image>();
            imgBoss.sprite = sprBossDefault;
            imgBoss.color = Color.white;
            imgBoss.raycastTarget = true;
            ui.baseImage = imgBoss;
            ui.bossPortrait = imgBoss;

            // BossBadge ("TRÙM" 24 Bold kem, #F2542D)
            GameObject objBossBadge = new GameObject("BossBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objBossBadge.transform.SetParent(objVisual.transform, false);
            RectTransform rtBadge = objBossBadge.GetComponent<RectTransform>();
            rtBadge.anchorMin = new Vector2(0.5f, 1f);
            rtBadge.anchorMax = new Vector2(0.5f, 1f);
            rtBadge.pivot = new Vector2(0.5f, 0.5f);
            rtBadge.anchoredPosition = new Vector2(0f, 12f);
            rtBadge.sizeDelta = new Vector2(88f, 36f);
            Image imgBadge = objBossBadge.GetComponent<Image>();
            imgBadge.sprite = sprPill;
            imgBadge.type = Image.Type.Sliced;
            imgBadge.color = ColorBadgeRed;
            imgBadge.raycastTarget = false;

            GameObject objBadgeTxt = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objBadgeTxt.transform.SetParent(objBossBadge.transform, false);
            RectTransform rtBadgeTxt = objBadgeTxt.GetComponent<RectTransform>();
            rtBadgeTxt.sizeDelta = new Vector2(88f, 36f);
            TextMeshProUGUI tmpBadge = objBadgeTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpBadge.font = font;
            tmpBadge.fontSize = 24f;
            tmpBadge.fontStyle = FontStyles.Bold;
            tmpBadge.color = ColorCream;
            tmpBadge.alignment = TextAlignmentOptions.Center;
            tmpBadge.text = "TRÙM";
            tmpBadge.raycastTarget = false;

            ui.bossBadge = objBossBadge;

            // LockIcon (Image locked.png, 64x64)
            GameObject objLock = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objLock.transform.SetParent(objVisual.transform, false);
            RectTransform rtLock = objLock.GetComponent<RectTransform>();
            rtLock.sizeDelta = new Vector2(64f, 64f);
            Image imgLock = objLock.GetComponent<Image>();
            imgLock.sprite = sprLock;
            imgLock.raycastTarget = false;
            ui.lockIcon = objLock;
            objLock.SetActive(false);

            // StarsRow (3 star icons 38x38)
            GameObject objStarsRow = new GameObject("StarsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            objStarsRow.transform.SetParent(objVisual.transform, false);
            RectTransform rtStars = objStarsRow.GetComponent<RectTransform>();
            rtStars.anchorMin = new Vector2(0.5f, 0f);
            rtStars.anchorMax = new Vector2(0.5f, 0f);
            rtStars.pivot = new Vector2(0.5f, 1f);
            rtStars.anchoredPosition = new Vector2(0f, -12f);
            rtStars.sizeDelta = new Vector2(130f, 40f);

            HorizontalLayoutGroup hlg = objStarsRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            Image[] starImgs = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject starObj = new GameObject($"Star_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                starObj.transform.SetParent(objStarsRow.transform, false);
                RectTransform rtS = starObj.GetComponent<RectTransform>();
                rtS.sizeDelta = new Vector2(38f, 38f);
                Image imgS = starObj.GetComponent<Image>();
                imgS.sprite = sprStar;
                imgS.color = ColorGold;
                imgS.raycastTarget = false;
                starImgs[i] = imgS;
            }
            ui.starsRow = objStarsRow;
            ui.starIcons = starImgs;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BossNodePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildTopBar(GameObject safeArea, Sprite sprPill, Sprite sprStar, Sprite sprKimCuong, Sprite sprCircle, TMP_FontAsset font)
        {
            GameObject objTopBar = new GameObject("TopBar", typeof(RectTransform));
            objTopBar.transform.SetParent(safeArea.transform, false);
            RectTransform rtTop = objTopBar.GetComponent<RectTransform>();
            rtTop.anchorMin = new Vector2(0f, 1f);
            rtTop.anchorMax = new Vector2(1f, 1f);
            rtTop.pivot = new Vector2(0.5f, 1f);
            rtTop.anchoredPosition = Vector2.zero;
            rtTop.sizeDelta = new Vector2(0f, 120f);

            // StarPill (pos X: 40)
            CreateTopBarPill("StarPill", objTopBar, 40f, sprPill, sprStar, ColorGold, "348", font);

            // DiamondPill (pos X: 270)
            CreateTopBarPill("DiamondPill", objTopBar, 270f, sprPill, sprKimCuong, Color.white, "120", font);
        }

        private static void CreateTopBarPill(string name, GameObject parent, float posX, Sprite sprPill, Sprite sprIcon, Color iconColor, string val, TMP_FontAsset font)
        {
            GameObject pill = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            pill.transform.SetParent(parent.transform, false);
            RectTransform rt = pill.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(posX, 0f);
            rt.sizeDelta = new Vector2(200f, 72f);

            Image img = pill.GetComponent<Image>();
            img.sprite = sprPill;
            img.type = Image.Type.Sliced;
            img.color = new Color(ColorChipBg.r, ColorChipBg.g, ColorChipBg.b, 0.9f);

            HorizontalLayoutGroup hlg = pill.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(24, 24, 0, 0);
            hlg.spacing = 14f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            ContentSizeFitter csf = pill.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            GameObject ico = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ico.transform.SetParent(pill.transform, false);
            RectTransform rtIco = ico.GetComponent<RectTransform>();
            rtIco.sizeDelta = new Vector2(42f, 42f);
            Image imgIco = ico.GetComponent<Image>();
            imgIco.sprite = sprIcon;
            imgIco.color = iconColor;
            imgIco.raycastTarget = false;

            GameObject txt = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txt.transform.SetParent(pill.transform, false);
            TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = 36f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColorCream;
            tmp.text = val;
            tmp.raycastTarget = false;
        }

        private static void BuildHeaderBar(GameObject safeArea, LevelDetailView view, Sprite sprRound36, Sprite sprPill, Sprite sprStar, TMP_FontAsset font)
        {
            GameObject objHeader = new GameObject("HeaderBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            objHeader.transform.SetParent(safeArea.transform, false);
            RectTransform rtHeader = objHeader.GetComponent<RectTransform>();
            rtHeader.anchorMin = new Vector2(0f, 1f);
            rtHeader.anchorMax = new Vector2(1f, 1f);
            rtHeader.pivot = new Vector2(0.5f, 1f);
            rtHeader.anchoredPosition = new Vector2(0f, -120f);
            rtHeader.sizeDelta = new Vector2(0f, 120f);

            HorizontalLayoutGroup hlg = objHeader.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(40, 40, 0, 0);
            hlg.spacing = 16f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 1. BackBtn (88x88 cố định, ui_round_36 #FFD27A, mũi tên #3A2200)
            GameObject objBack = new GameObject("BackBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            objBack.transform.SetParent(objHeader.transform, false);
            RectTransform rtBack = objBack.GetComponent<RectTransform>();
            rtBack.sizeDelta = new Vector2(88f, 88f);
            LayoutElement leBack = objBack.GetComponent<LayoutElement>();
            leBack.preferredWidth = 88f;
            leBack.minWidth = 88f;
            leBack.flexibleWidth = 0f;
            Image imgBack = objBack.GetComponent<Image>();
            imgBack.sprite = sprRound36;
            imgBack.type = Image.Type.Sliced;
            imgBack.color = ColorGold;
            Button btnBack = objBack.GetComponent<Button>();
            btnBack.transition = Selectable.Transition.None;
            view.backButton = btnBack;

            // Icon chevron/arrow trong BackBtn (nâng nhẹ +7px để ký tự '‹' nằm ngay tâm nút tròn)
            GameObject objArrow = new GameObject("Arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objArrow.transform.SetParent(objBack.transform, false);
            RectTransform rtArrow = objArrow.GetComponent<RectTransform>();
            rtArrow.anchorMin = new Vector2(0.5f, 0.5f);
            rtArrow.anchorMax = new Vector2(0.5f, 0.5f);
            rtArrow.pivot = new Vector2(0.5f, 0.5f);
            rtArrow.anchoredPosition = new Vector2(-2f, 7f);
            rtArrow.sizeDelta = new Vector2(88f, 88f);
            TextMeshProUGUI tmpArrow = objArrow.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpArrow.font = font;
            tmpArrow.fontSize = 54f;
            tmpArrow.fontStyle = FontStyles.Bold;
            tmpArrow.color = ColorDarkBrown;
            tmpArrow.alignment = TextAlignmentOptions.Center;
            tmpArrow.text = "‹"; // Mũi tên trái chuẩn Unicode
            tmpArrow.raycastTarget = false;

            // 2. TitleGroup (flexibleWidth 1)
            GameObject objTitleGroup = new GameObject("TitleGroup", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            objTitleGroup.transform.SetParent(objHeader.transform, false);
            RectTransform rtTitleGroup = objTitleGroup.GetComponent<RectTransform>();
            rtTitleGroup.sizeDelta = new Vector2(620f, 100f);
            LayoutElement leTitle = objTitleGroup.GetComponent<LayoutElement>();
            leTitle.flexibleWidth = 1f;
            leTitle.minWidth = 100f;

            VerticalLayoutGroup vlg = objTitleGroup.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 2f;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Subhead ("BẢN ĐỒ VƯỢT ẢI", cỡ 26)
            GameObject objSubhead = new GameObject("SubheadText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objSubhead.transform.SetParent(objTitleGroup.transform, false);
            TextMeshProUGUI tmpSub = objSubhead.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpSub.font = font;
            tmpSub.fontSize = 26f;
            tmpSub.fontStyle = FontStyles.Bold;
            tmpSub.color = ColorApricot;
            tmpSub.text = "BẢN ĐỒ VƯỢT ẢI";
            tmpSub.raycastTarget = false;
            view.actSubheadText = tmpSub;

            // ActTitle ("HỒI 2 - PHỐ CỔ ĐÈN LỒNG", cỡ 44 Bold, NoWrap, Ellipsis)
            GameObject objActTitle = new GameObject("ActTitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objActTitle.transform.SetParent(objTitleGroup.transform, false);
            TextMeshProUGUI tmpTitle = objActTitle.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpTitle.font = font;
            tmpTitle.fontSize = 44f;
            tmpTitle.fontStyle = FontStyles.Bold;
            tmpTitle.color = ColorCream;
            tmpTitle.enableAutoSizing = true;
            tmpTitle.fontSizeMin = 22f;
            tmpTitle.fontSizeMax = 44f;
            tmpTitle.textWrappingMode = TextWrappingModes.NoWrap;
            tmpTitle.overflowMode = TextOverflowModes.Ellipsis;
            tmpTitle.text = "HỒI 2 - PHỐ CỔ ĐÈN LỒNG";
            tmpTitle.raycastTarget = false;
            view.actTitleText = tmpTitle;

            // 3. ActProgressPill ("15/15" kèm icon star - preferredWidth 190, nền #1C2A5C alpha 0.95, viền mảnh #3A4676, chữ 28 Bold kem, icon star 32x32)
            GameObject objProgress = new GameObject("ProgressPill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            objProgress.transform.SetParent(objHeader.transform, false);
            RectTransform rtProg = objProgress.GetComponent<RectTransform>();
            rtProg.sizeDelta = new Vector2(190f, 44f);
            LayoutElement leProg = objProgress.GetComponent<LayoutElement>();
            leProg.preferredWidth = 190f;
            leProg.minWidth = 190f;
            leProg.flexibleWidth = 0f;

            Image imgProgBorder = objProgress.GetComponent<Image>();
            imgProgBorder.sprite = sprPill;
            imgProgBorder.type = Image.Type.Sliced;
            imgProgBorder.color = new Color(0.227f, 0.275f, 0.463f, 1f); // #3A4676 (Viền mảnh)
            imgProgBorder.raycastTarget = false;

            GameObject objProgFill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup));
            objProgFill.transform.SetParent(objProgress.transform, false);
            RectTransform rtProgFill = objProgFill.GetComponent<RectTransform>();
            rtProgFill.anchorMin = Vector2.zero;
            rtProgFill.anchorMax = Vector2.one;
            rtProgFill.offsetMin = new Vector2(1.5f, 1.5f);
            rtProgFill.offsetMax = new Vector2(-1.5f, -1.5f);

            Image imgProgFill = objProgFill.GetComponent<Image>();
            imgProgFill.sprite = sprPill;
            imgProgFill.type = Image.Type.Sliced;
            imgProgFill.color = new Color(0.11f, 0.165f, 0.361f, 0.95f); // #1C2A5C alpha 0.95
            imgProgFill.raycastTarget = false;

            HorizontalLayoutGroup hlgProg = objProgFill.GetComponent<HorizontalLayoutGroup>();
            hlgProg.padding = new RectOffset(12, 14, 0, 0);
            hlgProg.spacing = 8f;
            hlgProg.childAlignment = TextAnchor.MiddleCenter;
            hlgProg.childControlWidth = false;
            hlgProg.childControlHeight = false;

            GameObject objStarProg = new GameObject("Star", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objStarProg.transform.SetParent(objProgFill.transform, false);
            RectTransform rtStarProg = objStarProg.GetComponent<RectTransform>();
            rtStarProg.sizeDelta = new Vector2(32f, 32f); // 32x32
            Image imgStarProg = objStarProg.GetComponent<Image>();
            imgStarProg.sprite = sprStar;
            imgStarProg.color = ColorGold;
            imgStarProg.raycastTarget = false;

            GameObject objProgTxt = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objProgTxt.transform.SetParent(objProgFill.transform, false);
            TextMeshProUGUI tmpProg = objProgTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpProg.font = font;
            tmpProg.fontSize = 28f; // Cỡ 28 Bold kem
            tmpProg.fontStyle = FontStyles.Bold;
            tmpProg.color = ColorCream;
            tmpProg.text = "15/15";
            tmpProg.raycastTarget = false;
            view.actProgressText = tmpProg;
        }

        private static void BuildToast(GameObject parent, LevelDetailView view, Sprite sprPill, TMP_FontAsset font)
        {
            GameObject objToast = new GameObject("ToastRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            objToast.transform.SetParent(parent.transform, false);
            RectTransform rt = objToast.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -220f);
            rt.sizeDelta = new Vector2(640f, 80f);

            Image img = objToast.GetComponent<Image>();
            img.sprite = sprPill;
            img.type = Image.Type.Sliced;
            img.color = new Color(ColorChipBg.r, ColorChipBg.g, ColorChipBg.b, 0.96f);

            CanvasGroup cg = objToast.GetComponent<CanvasGroup>();
            cg.alpha = 0f;

            HorizontalLayoutGroup hlg = objToast.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(36, 36, 16, 16);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            ContentSizeFitter csf = objToast.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(objToast.transform, false);
            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = 28f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColorCream;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = "Hoàn thành màn trước để mở khóa";
            tmp.raycastTarget = false;

            view.toastRoot = rt;
            view.toastCanvasGroup = cg;
            view.toastText = tmp;
            objToast.SetActive(false);
        }

        private static void BuildModalOverlay(GameObject parent, LevelDetailView view, Sprite sprRound36, Sprite sprPill, Sprite sprCircle, Sprite sprGlowRing, Sprite sprStar, Sprite sprKimCuong, Sprite sprLock, Sprite sprCheck, Sprite sprPlay, Sprite sprClose, TMP_FontAsset font, LanData[] lans)
        {
            GameObject objModal = new GameObject("ModalOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(LevelDetailModal));
            objModal.transform.SetParent(parent.transform, false);
            RectTransform rtModal = objModal.GetComponent<RectTransform>();
            rtModal.anchorMin = Vector2.zero;
            rtModal.anchorMax = Vector2.one;
            rtModal.sizeDelta = Vector2.zero;
            rtModal.anchoredPosition = Vector2.zero;

            CanvasGroup modalCg = objModal.GetComponent<CanvasGroup>();
            LevelDetailModal modal = objModal.GetComponent<LevelDetailModal>();
            modal.modalCanvasGroup = modalCg;
            modal.availableLans = lans;
            modal.starSprite = sprStar;
            modal.diamondSprite = sprKimCuong;
            modal.lockSprite = sprLock;
            modal.checkSprite = sprCheck;
            modal.playSprite = sprPlay;
            modal.pillSprite = sprPill;

            view.detailModal = modal;

            // 1. Backdrop (đen alpha 0.84, là Button)
            GameObject objBackdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            objBackdrop.transform.SetParent(objModal.transform, false);
            RectTransform rtBackdrop = objBackdrop.GetComponent<RectTransform>();
            rtBackdrop.anchorMin = Vector2.zero;
            rtBackdrop.anchorMax = Vector2.one;
            rtBackdrop.sizeDelta = Vector2.zero;
            rtBackdrop.anchoredPosition = Vector2.zero;
            Image imgBackdrop = objBackdrop.GetComponent<Image>();
            imgBackdrop.color = new Color(0f, 0f, 0f, 0.84f);
            modal.backdropImage = imgBackdrop;
            Button btnBackdrop = objBackdrop.GetComponent<Button>();
            btnBackdrop.transition = Selectable.Transition.None;
            modal.backdropButton = btnBackdrop;

            // 2. PopupCard (rộng 900, nền #16224A, bo góc ui_round_36)
            GameObject objCard = new GameObject("PopupCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            objCard.transform.SetParent(objModal.transform, false);
            RectTransform rtCard = objCard.GetComponent<RectTransform>();
            rtCard.anchorMin = new Vector2(0.5f, 0.5f);
            rtCard.anchorMax = new Vector2(0.5f, 0.5f);
            rtCard.pivot = new Vector2(0.5f, 0.5f);
            rtCard.sizeDelta = new Vector2(900f, 1080f);
            rtCard.anchoredPosition = Vector2.zero;
            modal.popupCard = rtCard;

            Image imgCard = objCard.GetComponent<Image>();
            imgCard.sprite = sprRound36;
            imgCard.type = Image.Type.Sliced;
            imgCard.color = ColorCardBg;

            VerticalLayoutGroup vlg = objCard.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(36, 36, 36, 36);
            vlg.spacing = 18f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = objCard.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // GlowBorder outline cho Card
            GameObject objCardGlow = new GameObject("GlowOutline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            objCardGlow.transform.SetParent(objCard.transform, false);
            LayoutElement leGlow = objCardGlow.GetComponent<LayoutElement>();
            leGlow.ignoreLayout = true;
            RectTransform rtCardGlow = objCardGlow.GetComponent<RectTransform>();
            rtCardGlow.anchorMin = Vector2.zero;
            rtCardGlow.anchorMax = Vector2.one;
            rtCardGlow.sizeDelta = new Vector2(24f, 24f);
            rtCardGlow.anchoredPosition = Vector2.zero;
            Image imgCardGlow = objCardGlow.GetComponent<Image>();
            imgCardGlow.sprite = sprGlowRing;
            imgCardGlow.type = Image.Type.Sliced;
            imgCardGlow.color = new Color(ColorCyan.r, ColorCyan.g, ColorCyan.b, 0.9f);
            imgCardGlow.raycastTarget = false;

            // 2.1. HeaderRow (Subtitle + LevelName + Nút X)
            GameObject objHeaderRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            objHeaderRow.transform.SetParent(objCard.transform, false);
            RectTransform rtHeaderRow = objHeaderRow.GetComponent<RectTransform>();
            rtHeaderRow.sizeDelta = new Vector2(828f, 100f);

            HorizontalLayoutGroup hlgH = objHeaderRow.GetComponent<HorizontalLayoutGroup>();
            hlgH.childAlignment = TextAnchor.MiddleLeft;
            hlgH.childControlWidth = false;
            hlgH.childControlHeight = false;
            hlgH.childForceExpandWidth = false;
            hlgH.childForceExpandHeight = false;

            GameObject objTitles = new GameObject("Titles", typeof(RectTransform), typeof(VerticalLayoutGroup));
            objTitles.transform.SetParent(objHeaderRow.transform, false);
            RectTransform rtTitles = objTitles.GetComponent<RectTransform>();
            rtTitles.sizeDelta = new Vector2(730f, 100f);

            VerticalLayoutGroup vlgT = objTitles.GetComponent<VerticalLayoutGroup>();
            vlgT.spacing = 4f;
            vlgT.childAlignment = TextAnchor.MiddleLeft;
            vlgT.childControlWidth = true;
            vlgT.childControlHeight = false;
            vlgT.childForceExpandWidth = true;
            vlgT.childForceExpandHeight = false;

            GameObject objSub = new GameObject("Subhead", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objSub.transform.SetParent(objTitles.transform, false);
            TextMeshProUGUI tmpSub = objSub.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpSub.font = font;
            tmpSub.fontSize = 26f;
            tmpSub.fontStyle = FontStyles.Bold;
            tmpSub.color = ColorCyan;
            tmpSub.text = "HỒI 2 · PHỐ CỔ ĐÈN LỒNG";
            tmpSub.raycastTarget = false;
            modal.actSubheadText = tmpSub;

            GameObject objName = new GameObject("LevelTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objName.transform.SetParent(objTitles.transform, false);
            TextMeshProUGUI tmpName = objName.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpName.font = font;
            tmpName.fontSize = 44f;
            tmpName.fontStyle = FontStyles.Bold;
            tmpName.color = ColorCream;
            tmpName.text = "MÀN 2-3";
            tmpName.enableAutoSizing = true;
            tmpName.fontSizeMin = 30f;
            tmpName.fontSizeMax = 44f;
            tmpName.maxVisibleLines = 2;
            tmpName.textWrappingMode = TextWrappingModes.Normal;
            tmpName.overflowMode = TextOverflowModes.Ellipsis;
            tmpName.raycastTarget = false;
            modal.levelTitleText = tmpName;

            // Nút đóng X (72x72, ui_circle #1C2A5C, icon X)
            GameObject objClose = new GameObject("CloseBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            objClose.transform.SetParent(objHeaderRow.transform, false);
            RectTransform rtClose = objClose.GetComponent<RectTransform>();
            rtClose.sizeDelta = new Vector2(72f, 72f);
            Image imgClose = objClose.GetComponent<Image>();
            imgClose.sprite = sprCircle;
            imgClose.color = ColorChipBg;
            Button btnClose = objClose.GetComponent<Button>();
            btnClose.transition = Selectable.Transition.None;
            modal.closeButton = btnClose;

            GameObject objX = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objX.transform.SetParent(objClose.transform, false);
            RectTransform rtX = objX.GetComponent<RectTransform>();
            rtX.sizeDelta = new Vector2(36f, 36f);
            Image imgX = objX.GetComponent<Image>();
            imgX.sprite = sprClose;
            imgX.color = ColorCream;
            imgX.raycastTarget = false;

            // 2.2. CharacterCard (LanCard, nền #101A3C, cao ~170)
            GameObject objLanCard = new GameObject("LanCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup));
            objLanCard.transform.SetParent(objCard.transform, false);
            RectTransform rtLan = objLanCard.GetComponent<RectTransform>();
            rtLan.sizeDelta = new Vector2(828f, 170f);

            Image imgLanCard = objLanCard.GetComponent<Image>();
            imgLanCard.sprite = sprRound36;
            imgLanCard.type = Image.Type.Sliced;
            imgLanCard.color = ColorRowBg;

            HorizontalLayoutGroup hlgLan = objLanCard.GetComponent<HorizontalLayoutGroup>();
            hlgLan.padding = new RectOffset(24, 24, 16, 16);
            hlgLan.spacing = 24f;
            hlgLan.childAlignment = TextAnchor.MiddleLeft;
            hlgLan.childControlWidth = false;
            hlgLan.childControlHeight = false;
            hlgLan.childForceExpandWidth = false;
            hlgLan.childForceExpandHeight = false;

            // Avatar frame (136x136, viền vàng)
            GameObject objAvatarFrame = new GameObject("AvatarFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objAvatarFrame.transform.SetParent(objLanCard.transform, false);
            RectTransform rtAvf = objAvatarFrame.GetComponent<RectTransform>();
            rtAvf.sizeDelta = new Vector2(136f, 136f);
            Image imgAvf = objAvatarFrame.GetComponent<Image>();
            imgAvf.sprite = sprCircle;
            imgAvf.color = ColorGold;

            GameObject objAvMask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            objAvMask.transform.SetParent(objAvatarFrame.transform, false);
            RectTransform rtAvm = objAvMask.GetComponent<RectTransform>();
            rtAvm.sizeDelta = new Vector2(126f, 126f);
            Image imgAvm = objAvMask.GetComponent<Image>();
            imgAvm.sprite = sprCircle;
            Mask maskAv = objAvMask.GetComponent<Mask>();
            maskAv.showMaskGraphic = false;

            GameObject objAvatar = new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objAvatar.transform.SetParent(objAvMask.transform, false);
            RectTransform rtAv = objAvatar.GetComponent<RectTransform>();
            rtAv.sizeDelta = new Vector2(136f, 136f);
            Image imgAv = objAvatar.GetComponent<Image>();
            if (lans != null && lans.Length > 0 && lans[0] != null) imgAv.sprite = lans[0].icon;
            modal.lanAvatarImage = imgAv;

            // Lan Info (Tên + Kỷ lục)
            GameObject objLanInfo = new GameObject("LanInfo", typeof(RectTransform), typeof(VerticalLayoutGroup));
            objLanInfo.transform.SetParent(objLanCard.transform, false);
            RectTransform rtLanInfo = objLanInfo.GetComponent<RectTransform>();
            rtLanInfo.sizeDelta = new Vector2(600f, 130f);

            VerticalLayoutGroup vlgLanInfo = objLanInfo.GetComponent<VerticalLayoutGroup>();
            vlgLanInfo.spacing = 8f;
            vlgLanInfo.childAlignment = TextAnchor.MiddleLeft;
            vlgLanInfo.childControlWidth = false;
            vlgLanInfo.childControlHeight = false;
            vlgLanInfo.childForceExpandWidth = false;
            vlgLanInfo.childForceExpandHeight = false;

            GameObject objLanName = new GameObject("LanName", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objLanName.transform.SetParent(objLanInfo.transform, false);
            TextMeshProUGUI tmpLanName = objLanName.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpLanName.font = font;
            tmpLanName.fontSize = 34f;
            tmpLanName.fontStyle = FontStyles.Bold;
            tmpLanName.color = ColorCream;
            tmpLanName.text = "Kim Lân";
            tmpLanName.raycastTarget = false;
            modal.lanNameText = tmpLanName;

            // Row Kỷ lục màn + 3 sao ngay dưới tên, căn trái
            GameObject objRecordRow = new GameObject("RecordRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            objRecordRow.transform.SetParent(objLanInfo.transform, false);
            RectTransform rtRec = objRecordRow.GetComponent<RectTransform>();
            rtRec.sizeDelta = new Vector2(400f, 44f);

            HorizontalLayoutGroup hlgRec = objRecordRow.GetComponent<HorizontalLayoutGroup>();
            hlgRec.spacing = 10f;
            hlgRec.childAlignment = TextAnchor.MiddleLeft;
            hlgRec.childControlWidth = false;
            hlgRec.childControlHeight = false;
            hlgRec.childForceExpandWidth = false;
            hlgRec.childForceExpandHeight = false;

            ContentSizeFitter csfRec = objRecordRow.GetComponent<ContentSizeFitter>();
            csfRec.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csfRec.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject objRecLbl = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objRecLbl.transform.SetParent(objRecordRow.transform, false);
            TextMeshProUGUI tmpRec = objRecLbl.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpRec.font = font;
            tmpRec.fontSize = 26f;
            tmpRec.color = new Color(0.663f, 0.706f, 0.839f, 1f); // #A9B4D6
            tmpRec.text = "Kỷ lục màn:";
            tmpRec.raycastTarget = false;

            Image[] recStars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject st = new GameObject($"Star_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                st.transform.SetParent(objRecordRow.transform, false);
                RectTransform rtS = st.GetComponent<RectTransform>();
                rtS.sizeDelta = new Vector2(40f, 40f);
                Image imgS = st.GetComponent<Image>();
                imgS.sprite = sprStar;
                // Demo Màn 8: 2 sao vàng, 1 sao xám
                imgS.color = (i < 2) ? ColorGold : new Color(0.29f, 0.333f, 0.502f, 1f);
                imgS.preserveAspect = true;
                imgS.raycastTarget = false;
                recStars[i] = imgS;
            }
            modal.recordStarIcons = recStars;

            // 2.2b. Objective Chip (Phase 3.5: "Mục tiêu: Hạ {n} địch" / "Sống sót {s} giây" / "Hạ {bossName}")
            // Style: ui_pill #1C2A5C, chữ kem cỡ 26 Bold, căn giữa
            GameObject objObjRow = new GameObject("ObjectiveRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            objObjRow.transform.SetParent(objCard.transform, false);
            LayoutElement leObjRow = objObjRow.GetComponent<LayoutElement>();
            leObjRow.preferredHeight = 48f;
            leObjRow.minHeight = 44f;
            leObjRow.flexibleWidth = 1f;

            HorizontalLayoutGroup hlgObjRow = objObjRow.GetComponent<HorizontalLayoutGroup>();
            hlgObjRow.childAlignment = TextAnchor.MiddleCenter;
            hlgObjRow.childControlWidth = false;
            hlgObjRow.childControlHeight = false;
            hlgObjRow.childForceExpandWidth = false;
            hlgObjRow.childForceExpandHeight = false;

            GameObject objChip = new GameObject("ObjectiveChip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            objChip.transform.SetParent(objObjRow.transform, false);
            Image imgChip = objChip.GetComponent<Image>();
            imgChip.sprite = sprPill;
            imgChip.type = Image.Type.Sliced;
            imgChip.color = ColorChipBg; // #1C2A5C

            HorizontalLayoutGroup hlgChip = objChip.GetComponent<HorizontalLayoutGroup>();
            hlgChip.padding = new RectOffset(26, 26, 8, 8);
            hlgChip.childAlignment = TextAnchor.MiddleCenter;
            hlgChip.childControlWidth = false;
            hlgChip.childControlHeight = false;
            hlgChip.childForceExpandWidth = false;
            hlgChip.childForceExpandHeight = false;

            ContentSizeFitter csfChip = objChip.GetComponent<ContentSizeFitter>();
            csfChip.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csfChip.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject objObjTxt = new GameObject("ObjectiveText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objObjTxt.transform.SetParent(objChip.transform, false);
            TextMeshProUGUI tmpObj = objObjTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpObj.font = font;
            tmpObj.fontSize = 26f;
            tmpObj.fontStyle = FontStyles.Bold;
            tmpObj.color = ColorCream;
            tmpObj.alignment = TextAlignmentOptions.Center;
            tmpObj.text = "Mục tiêu: Hạ 25 địch";
            tmpObj.raycastTarget = false;

            modal.objectiveContainer = objObjRow;
            modal.objectiveText = tmpObj;

            // 2.3. SectionTitleRow ("MỐC SAO & QUÀ" vs "PHẦN THƯỞNG")
            GameObject objSecRow = new GameObject("SectionTitleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            objSecRow.transform.SetParent(objCard.transform, false);
            RectTransform rtSec = objSecRow.GetComponent<RectTransform>();
            rtSec.sizeDelta = new Vector2(828f, 40f);

            HorizontalLayoutGroup hlgSec = objSecRow.GetComponent<HorizontalLayoutGroup>();
            hlgSec.childAlignment = TextAnchor.MiddleLeft;
            hlgSec.childControlWidth = false;
            hlgSec.childControlHeight = false;
            hlgSec.childForceExpandWidth = false;
            hlgSec.childForceExpandHeight = false;

            GameObject objSecLeft = new GameObject("LeftTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objSecLeft.transform.SetParent(objSecRow.transform, false);
            RectTransform rtSecL = objSecLeft.GetComponent<RectTransform>();
            rtSecL.sizeDelta = new Vector2(500f, 40f);
            TextMeshProUGUI tmpSecL = objSecLeft.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpSecL.font = font;
            tmpSecL.fontSize = 28f;
            tmpSecL.fontStyle = FontStyles.Bold;
            tmpSecL.color = ColorCyan;
            tmpSecL.text = "MỐC SAO & QUÀ";
            tmpSecL.raycastTarget = false;

            GameObject objSecRight = new GameObject("RightTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objSecRight.transform.SetParent(objSecRow.transform, false);
            RectTransform rtSecR = objSecRight.GetComponent<RectTransform>();
            rtSecR.sizeDelta = new Vector2(328f, 40f);
            TextMeshProUGUI tmpSecR = objSecRight.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpSecR.font = font;
            tmpSecR.fontSize = 28f;
            tmpSecR.fontStyle = FontStyles.Bold;
            tmpSecR.color = ColorCyan;
            tmpSecR.alignment = TextAlignmentOptions.Right;
            tmpSecR.text = "PHẦN THƯỞNG";
            tmpSecR.raycastTarget = false;

            // 2.4. Ba hàng quà RewardRow (Mặc định demo Hồi 3 của Act 2 - Màn 8: 2 Đã nhận, 1 Chưa nhận)
            RewardRowUI[] rows = new RewardRowUI[3];
            rows[0] = BuildRewardRow("RewardRow_1", objCard, sprRound36, sprPill, sprStar, sprKimCuong, sprCheck, sprLock, font, 1, "Vượt qua màn chơi", 52, 0, true);
            rows[1] = BuildRewardRow("RewardRow_2", objCard, sprRound36, sprPill, sprStar, sprKimCuong, sprCheck, sprLock, font, 2, "Tổng máu còn lại ≥ 50%", 68, 0, true);
            rows[2] = BuildRewardRow("RewardRow_3", objCard, sprRound36, sprPill, sprStar, sprKimCuong, sprCheck, sprLock, font, 3, "Tổng máu còn lại ≥ 85%", 84, 0, false);
            modal.rewardRows = rows;

            // 2.5. Nút BẮT ĐẦU CHƠI
            // LayoutElement: preferredHeight 120, minHeight 110, flexibleWidth 1
            GameObject objPlayBtn = new GameObject("PlayButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            objPlayBtn.transform.SetParent(objCard.transform, false);
            RectTransform rtPlay = objPlayBtn.GetComponent<RectTransform>();
            rtPlay.sizeDelta = new Vector2(828f, 120f);

            LayoutElement lePlay = objPlayBtn.GetComponent<LayoutElement>();
            lePlay.preferredHeight = 120f;
            lePlay.minHeight = 110f;
            lePlay.flexibleWidth = 1f;

            Image imgPlay = objPlayBtn.GetComponent<Image>();
            imgPlay.sprite = sprRound36;
            imgPlay.type = Image.Type.Sliced;
            imgPlay.color = ColorGold;

            Button btnPlay = objPlayBtn.GetComponent<Button>();
            btnPlay.transition = Selectable.Transition.None;
            modal.playButton = btnPlay;
            modal.playButtonVisual = rtPlay;

            // Container nội dung nút (Icon ui_play 44x44 trước TMP, cả cụm căn giữa nút, spacing 14)
            GameObject objPlayContent = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            objPlayContent.transform.SetParent(objPlayBtn.transform, false);
            RectTransform rtPlayContent = objPlayContent.GetComponent<RectTransform>();
            rtPlayContent.anchorMin = new Vector2(0.5f, 0.5f);
            rtPlayContent.anchorMax = new Vector2(0.5f, 0.5f);
            rtPlayContent.pivot = new Vector2(0.5f, 0.5f);
            rtPlayContent.anchoredPosition = Vector2.zero;

            ContentSizeFitter csfPlay = objPlayContent.GetComponent<ContentSizeFitter>();
            csfPlay.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csfPlay.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup hlgPlay = objPlayContent.GetComponent<HorizontalLayoutGroup>();
            hlgPlay.childAlignment = TextAnchor.MiddleCenter;
            hlgPlay.spacing = 14f;
            hlgPlay.childControlWidth = true;
            hlgPlay.childControlHeight = true;
            hlgPlay.childForceExpandWidth = false;
            hlgPlay.childForceExpandHeight = false;

            // Icon ui_play đứng trước
            GameObject objPlayIcon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            objPlayIcon.transform.SetParent(objPlayContent.transform, false);
            RectTransform rtPI = objPlayIcon.GetComponent<RectTransform>();
            rtPI.sizeDelta = new Vector2(44f, 44f);
            LayoutElement lePI = objPlayIcon.GetComponent<LayoutElement>();
            lePI.preferredWidth = 44f;
            lePI.preferredHeight = 44f;
            lePI.minWidth = 44f;
            lePI.minHeight = 44f;
            lePI.flexibleWidth = 0f;
            Image imgPI = objPlayIcon.GetComponent<Image>();
            imgPI.sprite = sprPlay;
            imgPI.color = ColorDarkBrown;
            imgPI.preserveAspect = true;
            imgPI.raycastTarget = false;
            modal.playIconImage = imgPI;

            // Text "BẮT ĐẦU CHƠI" (1 dòng, nằm ngay sau icon, căn giữa)
            GameObject objPlayTxt = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objPlayTxt.transform.SetParent(objPlayContent.transform, false);
            TextMeshProUGUI tmpPlay = objPlayTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpPlay.font = font;
            tmpPlay.fontSize = 44f;
            tmpPlay.fontStyle = FontStyles.Bold;
            tmpPlay.color = ColorDarkBrown;
            tmpPlay.text = "BẮT ĐẦU CHƠI";
            tmpPlay.alignment = TextAlignmentOptions.Center;
            tmpPlay.enableAutoSizing = true;
            tmpPlay.fontSizeMin = 30f;
            tmpPlay.fontSizeMax = 44f;
            tmpPlay.textWrappingMode = TextWrappingModes.NoWrap;
            tmpPlay.overflowMode = TextOverflowModes.Ellipsis;
            tmpPlay.raycastTarget = false;

            LayoutElement leTxt = objPlayTxt.GetComponent<LayoutElement>();
            leTxt.preferredHeight = 44f;
            leTxt.flexibleWidth = 0f;
            modal.playButtonText = tmpPlay;

            // 2.6. Revive Notice (Phase 3.5: dòng nhỏ dưới nút CHƠI, cỡ 22, #A9B4D6)
            GameObject objReviveNotice = new GameObject("ReviveNotice", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objReviveNotice.transform.SetParent(objCard.transform, false);
            LayoutElement leRevive = objReviveNotice.GetComponent<LayoutElement>();
            leRevive.preferredHeight = 28f;
            leRevive.minHeight = 24f;
            leRevive.flexibleWidth = 1f;

            TextMeshProUGUI tmpRevive = objReviveNotice.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpRevive.font = font;
            tmpRevive.fontSize = 22f;
            tmpRevive.color = new Color(0.663f, 0.706f, 0.839f, 1f); // #A9B4D6
            tmpRevive.alignment = TextAlignmentOptions.Center;
            tmpRevive.text = "Dùng hồi sinh quảng cáo: tối đa 1 sao";
            tmpRevive.raycastTarget = false;
            modal.reviveNoticeText = tmpRevive;

            objModal.SetActive(false);
        }

        private static RewardRowUI BuildRewardRow(string name, GameObject parent, Sprite sprRound36, Sprite sprPill, Sprite sprStar, Sprite sprKimCuong, Sprite sprCheck, Sprite sprLock, TMP_FontAsset font, int initialMilestone = 1, string initialCondition = "Vượt qua màn chơi", int initialStar = 52, int initialDiamond = 0, bool initialClaimed = true)
        {
            GameObject row = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(RewardRowUI));
            row.transform.SetParent(parent.transform, false);
            RectTransform rtRow = row.GetComponent<RectTransform>();
            rtRow.sizeDelta = new Vector2(828f, 104f);

            LayoutElement leRow = row.GetComponent<LayoutElement>();
            leRow.preferredHeight = 104f;
            leRow.minHeight = 104f;
            leRow.flexibleWidth = 1f;

            Image imgRow = row.GetComponent<Image>();
            imgRow.sprite = sprRound36;
            imgRow.type = Image.Type.Sliced;
            imgRow.color = ColorRowBg;

            CanvasGroup cg = row.GetComponent<CanvasGroup>();
            cg.alpha = initialClaimed ? 0.68f : 1.0f;

            RewardRowUI rowUI = row.GetComponent<RewardRowUI>();
            rowUI.rowCanvasGroup = cg;

            HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(22, 22, 0, 0);
            hlg.spacing = 14f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 1. Chip mốc: preferredWidth 96, HLG MiddleCenter spacing 6: TMP "1" + Image star.png 30x30
            GameObject objChip = new GameObject("MilestoneChip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            objChip.transform.SetParent(row.transform, false);
            RectTransform rtChip = objChip.GetComponent<RectTransform>();
            rtChip.sizeDelta = new Vector2(96f, 52f);

            LayoutElement leChip = objChip.GetComponent<LayoutElement>();
            leChip.preferredWidth = 96f;
            leChip.minWidth = 96f;
            leChip.preferredHeight = 52f;
            leChip.minHeight = 52f;
            leChip.flexibleWidth = 0f;

            Image imgChip = objChip.GetComponent<Image>();
            imgChip.sprite = sprPill;
            imgChip.type = Image.Type.Sliced;
            imgChip.color = ColorChipBg;
            imgChip.raycastTarget = false;

            HorizontalLayoutGroup hlgChip = objChip.GetComponent<HorizontalLayoutGroup>();
            hlgChip.childAlignment = TextAnchor.MiddleCenter;
            hlgChip.spacing = 6f;
            hlgChip.childControlWidth = false;
            hlgChip.childControlHeight = false;
            hlgChip.childForceExpandWidth = false;
            hlgChip.childForceExpandHeight = false;

            GameObject objChipTxt = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objChipTxt.transform.SetParent(objChip.transform, false);
            RectTransform rtChipTxt = objChipTxt.GetComponent<RectTransform>();
            rtChipTxt.sizeDelta = new Vector2(24f, 36f);
            TextMeshProUGUI tmpChip = objChipTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpChip.font = font;
            tmpChip.fontSize = 28f;
            tmpChip.fontStyle = FontStyles.Bold;
            tmpChip.color = ColorGold;
            tmpChip.alignment = TextAlignmentOptions.Center;
            tmpChip.text = initialMilestone.ToString();
            tmpChip.raycastTarget = false;
            rowUI.milestoneChipText = tmpChip;

            GameObject objChipStar = new GameObject("StarIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objChipStar.transform.SetParent(objChip.transform, false);
            RectTransform rtChipStar = objChipStar.GetComponent<RectTransform>();
            rtChipStar.sizeDelta = new Vector2(30f, 30f);
            Image imgChipStar = objChipStar.GetComponent<Image>();
            imgChipStar.sprite = sprStar;
            imgChipStar.color = ColorGold;
            imgChipStar.preserveAspect = true;
            imgChipStar.raycastTarget = false;
            rowUI.milestoneStarIcon = imgChipStar;

            // 2. Điều kiện mốc: flexibleWidth 1, TMP 24, Wrapping BẬT, max 2 dòng, Overflow Ellipsis
            GameObject objCond = new GameObject("Condition", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objCond.transform.SetParent(row.transform, false);
            RectTransform rtCond = objCond.GetComponent<RectTransform>();
            rtCond.sizeDelta = new Vector2(220f, 60f);

            LayoutElement leCond = objCond.GetComponent<LayoutElement>();
            leCond.flexibleWidth = 1f;
            leCond.minWidth = 180f;
            leCond.preferredHeight = 60f;

            TextMeshProUGUI tmpCond = objCond.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpCond.font = font;
            tmpCond.fontSize = 24f;
            tmpCond.color = initialClaimed ? new Color(0.557f, 0.596f, 0.737f, 1f) : ColorCream;
            tmpCond.alignment = TextAlignmentOptions.MidlineLeft;
            tmpCond.textWrappingMode = TextWrappingModes.Normal;
            tmpCond.overflowMode = TextOverflowModes.Ellipsis;
            tmpCond.text = initialCondition;
            tmpCond.raycastTarget = false;
            rowUI.conditionText = tmpCond;

            // 3. Cụm quà RewardCluster: HLG MiddleRight, spacing 12, minWidth 130
            GameObject objRewards = new GameObject("RewardCluster", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
            objRewards.transform.SetParent(row.transform, false);
            RectTransform rtRewards = objRewards.GetComponent<RectTransform>();
            rtRewards.sizeDelta = new Vector2(140f, 44f);

            LayoutElement leRewards = objRewards.GetComponent<LayoutElement>();
            leRewards.minWidth = 130f;
            leRewards.preferredHeight = 44f;
            leRewards.flexibleWidth = 0f;

            ContentSizeFitter csfRew = objRewards.GetComponent<ContentSizeFitter>();
            csfRew.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csfRew.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup hlgRew = objRewards.GetComponent<HorizontalLayoutGroup>();
            hlgRew.spacing = 12f;
            hlgRew.childAlignment = TextAnchor.MiddleRight;
            hlgRew.childControlWidth = false;
            hlgRew.childControlHeight = false;
            hlgRew.childForceExpandWidth = false;
            hlgRew.childForceExpandHeight = false;

            // Cụm sao
            GameObject objStarRew = new GameObject("StarReward", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            objStarRew.transform.SetParent(objRewards.transform, false);
            RectTransform rtStarRew = objStarRew.GetComponent<RectTransform>();
            rtStarRew.sizeDelta = new Vector2(80f, 44f);

            ContentSizeFitter csfSR = objStarRew.GetComponent<ContentSizeFitter>();
            csfSR.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csfSR.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup hlgSR = objStarRew.GetComponent<HorizontalLayoutGroup>();
            hlgSR.spacing = 6f;
            hlgSR.childAlignment = TextAnchor.MiddleRight;
            hlgSR.childControlWidth = false;
            hlgSR.childControlHeight = false;
            hlgSR.childForceExpandWidth = false;
            hlgSR.childForceExpandHeight = false;

            GameObject objStarIco = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objStarIco.transform.SetParent(objStarRew.transform, false);
            RectTransform rtSI = objStarIco.GetComponent<RectTransform>();
            rtSI.sizeDelta = new Vector2(30f, 30f);
            Image imgStarIco = objStarIco.GetComponent<Image>();
            imgStarIco.sprite = sprStar;
            imgStarIco.color = ColorGold;
            imgStarIco.preserveAspect = true;
            imgStarIco.raycastTarget = false;
            rowUI.starIcon = imgStarIco;

            GameObject objStarVal = new GameObject("Amount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objStarVal.transform.SetParent(objStarRew.transform, false);
            TextMeshProUGUI tmpStarVal = objStarVal.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpStarVal.font = font;
            tmpStarVal.fontSize = 26f;
            tmpStarVal.fontStyle = FontStyles.Bold;
            tmpStarVal.color = ColorGold;
            tmpStarVal.text = $"+{initialStar}";
            tmpStarVal.textWrappingMode = TextWrappingModes.NoWrap;
            tmpStarVal.alignment = TextAlignmentOptions.MidlineLeft;
            tmpStarVal.raycastTarget = false;
            rowUI.starAmountText = tmpStarVal;

            // Cụm kim cương
            GameObject objDiaRew = new GameObject("DiamondReward", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            objDiaRew.transform.SetParent(objRewards.transform, false);
            RectTransform rtDiaRew = objDiaRew.GetComponent<RectTransform>();
            rtDiaRew.sizeDelta = new Vector2(80f, 44f);

            ContentSizeFitter csfDR = objDiaRew.GetComponent<ContentSizeFitter>();
            csfDR.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csfDR.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup hlgDR = objDiaRew.GetComponent<HorizontalLayoutGroup>();
            hlgDR.spacing = 6f;
            hlgDR.childAlignment = TextAnchor.MiddleRight;
            hlgDR.childControlWidth = false;
            hlgDR.childControlHeight = false;
            hlgDR.childForceExpandWidth = false;
            hlgDR.childForceExpandHeight = false;

            GameObject objDiaIco = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objDiaIco.transform.SetParent(objDiaRew.transform, false);
            RectTransform rtDI = objDiaIco.GetComponent<RectTransform>();
            rtDI.sizeDelta = new Vector2(30f, 30f);
            Image imgDiaIco = objDiaIco.GetComponent<Image>();
            imgDiaIco.sprite = sprKimCuong;
            imgDiaIco.preserveAspect = true;
            imgDiaIco.raycastTarget = false;
            rowUI.diamondIcon = imgDiaIco;

            GameObject objDiaVal = new GameObject("Amount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            objDiaVal.transform.SetParent(objDiaRew.transform, false);
            TextMeshProUGUI tmpDiaVal = objDiaVal.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpDiaVal.font = font;
            tmpDiaVal.fontSize = 26f;
            tmpDiaVal.fontStyle = FontStyles.Bold;
            tmpDiaVal.color = ColorCyan;
            tmpDiaVal.text = $"+{initialDiamond}";
            tmpDiaVal.textWrappingMode = TextWrappingModes.NoWrap;
            tmpDiaVal.alignment = TextAlignmentOptions.MidlineLeft;
            tmpDiaVal.raycastTarget = false;
            rowUI.diamondAmountText = tmpDiaVal;

            rowUI.diamondContainer = objDiaRew;
            objDiaRew.SetActive(initialDiamond > 0);

            // 4. Trạng thái: preferredWidth 160 cố định (thiết kế cân đối 152x42 giữa ĐÃ NHẬN và CHƯA NHẬN)
            GameObject objStatusGroup = new GameObject("StatusGroup", typeof(RectTransform), typeof(LayoutElement));
            objStatusGroup.transform.SetParent(row.transform, false);
            RectTransform rtStatus = objStatusGroup.GetComponent<RectTransform>();
            rtStatus.sizeDelta = new Vector2(160f, 44f);

            LayoutElement leStatus = objStatusGroup.GetComponent<LayoutElement>();
            leStatus.preferredWidth = 160f;
            leStatus.minWidth = 160f;
            leStatus.preferredHeight = 44f;
            leStatus.flexibleWidth = 0f;

            // 4.1. Pill xanh ngọc đậm "ĐÃ NHẬN" (minWidth 150)
            GameObject objClaimed = new GameObject("ClaimedBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            objClaimed.transform.SetParent(objStatusGroup.transform, false);
            RectTransform rtCl = objClaimed.GetComponent<RectTransform>();
            rtCl.anchorMin = new Vector2(0.5f, 0.5f);
            rtCl.anchorMax = new Vector2(0.5f, 0.5f);
            rtCl.pivot = new Vector2(0.5f, 0.5f);
            rtCl.sizeDelta = new Vector2(150f, 44f);
            rtCl.anchoredPosition = Vector2.zero;

            LayoutElement leCl = objClaimed.GetComponent<LayoutElement>();
            leCl.minWidth = 150f;
            leCl.preferredWidth = 150f;
            leCl.preferredHeight = 44f;
            leCl.flexibleWidth = 0f;

            Image imgCl = objClaimed.GetComponent<Image>();
            imgCl.sprite = sprPill;
            imgCl.type = Image.Type.Sliced;
            imgCl.color = new Color(0.078f, 0.231f, 0.176f, 1f); // #143B2D (Dark Emerald)
            imgCl.raycastTarget = false;

            HorizontalLayoutGroup hlgCl = objClaimed.GetComponent<HorizontalLayoutGroup>();
            hlgCl.padding = new RectOffset(14, 14, 6, 6);
            hlgCl.spacing = 8f;
            hlgCl.childAlignment = TextAnchor.MiddleCenter;
            hlgCl.childControlWidth = true;
            hlgCl.childControlHeight = true;
            hlgCl.childForceExpandWidth = false;
            hlgCl.childForceExpandHeight = false;

            GameObject objCheckIco = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            objCheckIco.transform.SetParent(objClaimed.transform, false);
            LayoutElement leCheckIco = objCheckIco.GetComponent<LayoutElement>();
            leCheckIco.minWidth = 28f;
            leCheckIco.minHeight = 28f;
            leCheckIco.preferredWidth = 28f;
            leCheckIco.preferredHeight = 28f;
            leCheckIco.flexibleWidth = 0f;
            leCheckIco.flexibleHeight = 0f;

            Image imgCheck = objCheckIco.GetComponent<Image>();
            imgCheck.sprite = sprCheck;
            imgCheck.color = new Color(0.431f, 0.906f, 0.718f, 1f); // #6EE7B7 (Mint)
            imgCheck.preserveAspect = true;
            imgCheck.raycastTarget = false;

            GameObject objClTxt = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objClTxt.transform.SetParent(objClaimed.transform, false);
            LayoutElement leClTxt = objClTxt.GetComponent<LayoutElement>();
            leClTxt.minWidth = 40f;
            leClTxt.flexibleWidth = 1f;

            TextMeshProUGUI tmpCl = objClTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpCl.font = font;
            tmpCl.fontSize = 22f;
            tmpCl.enableAutoSizing = true;
            tmpCl.fontSizeMin = 18f;
            tmpCl.fontSizeMax = 22f;
            tmpCl.fontStyle = FontStyles.Bold;
            tmpCl.color = new Color(0.431f, 0.906f, 0.718f, 1f); // #6EE7B7
            tmpCl.text = "ĐÃ NHẬN";
            tmpCl.alignment = TextAlignmentOptions.Center;
            tmpCl.textWrappingMode = TextWrappingModes.NoWrap;
            tmpCl.raycastTarget = false;

            rowUI.claimedBadge = objClaimed;
            rowUI.claimedBadgeBg = imgCl;
            rowUI.claimedCheckIcon = imgCheck;
            rowUI.claimedBadgeText = tmpCl;
            objClaimed.SetActive(initialClaimed);

            // 4.2. Pill slate/silver "CHƯA ĐẠT" (minWidth 150, viền #3A4676, nền #1C2A5C, icon ui_lock 28x28, text cỡ 18-22 #C5CCE6)
            GameObject objUnclaimed = new GameObject("UnclaimedBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            objUnclaimed.transform.SetParent(objStatusGroup.transform, false);
            RectTransform rtUncl = objUnclaimed.GetComponent<RectTransform>();
            rtUncl.anchorMin = new Vector2(0.5f, 0.5f);
            rtUncl.anchorMax = new Vector2(0.5f, 0.5f);
            rtUncl.pivot = new Vector2(0.5f, 0.5f);
            rtUncl.sizeDelta = new Vector2(150f, 44f);
            rtUncl.anchoredPosition = Vector2.zero;

            LayoutElement leUncl = objUnclaimed.GetComponent<LayoutElement>();
            leUncl.minWidth = 150f;
            leUncl.preferredWidth = 150f;
            leUncl.preferredHeight = 44f;
            leUncl.flexibleWidth = 0f;

            Image imgBorder = objUnclaimed.GetComponent<Image>();
            imgBorder.sprite = sprPill;
            imgBorder.type = Image.Type.Sliced;
            imgBorder.color = new Color(0.227f, 0.275f, 0.463f, 1f); // #3A4676 (Viền mảnh)
            imgBorder.raycastTarget = false;

            GameObject objFill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup));
            objFill.transform.SetParent(objUnclaimed.transform, false);
            RectTransform rtFill = objFill.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.offsetMin = new Vector2(1.5f, 1.5f);
            rtFill.offsetMax = new Vector2(-1.5f, -1.5f);

            Image imgFill = objFill.GetComponent<Image>();
            imgFill.sprite = sprPill;
            imgFill.type = Image.Type.Sliced;
            imgFill.color = new Color(0.11f, 0.165f, 0.361f, 1f); // #1C2A5C (Nền tag)
            imgFill.raycastTarget = false;

            HorizontalLayoutGroup hlgUncl = objFill.GetComponent<HorizontalLayoutGroup>();
            hlgUncl.padding = new RectOffset(14, 14, 6, 6);
            hlgUncl.spacing = 8f;
            hlgUncl.childAlignment = TextAnchor.MiddleCenter;
            hlgUncl.childControlWidth = true;
            hlgUncl.childControlHeight = true;
            hlgUncl.childForceExpandWidth = false;
            hlgUncl.childForceExpandHeight = false;

            // Icon khóa 28x28 đứng TRƯỚC text
            GameObject objLockIco = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            objLockIco.transform.SetParent(objFill.transform, false);
            LayoutElement leLockIco = objLockIco.GetComponent<LayoutElement>();
            leLockIco.minWidth = 28f;
            leLockIco.minHeight = 28f;
            leLockIco.preferredWidth = 28f;
            leLockIco.preferredHeight = 28f;
            leLockIco.flexibleWidth = 0f;
            leLockIco.flexibleHeight = 0f;

            Image imgLock = objLockIco.GetComponent<Image>();
            imgLock.sprite = sprLock;
            imgLock.color = new Color(0.773f, 0.8f, 0.902f, 1f); // #C5CCE6
            imgLock.preserveAspect = true;
            imgLock.raycastTarget = false;

            // Text "CHƯA ĐẠT" đặt SAU icon
            GameObject objUnclTxt = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objUnclTxt.transform.SetParent(objFill.transform, false);
            LayoutElement leUnclTxt = objUnclTxt.GetComponent<LayoutElement>();
            leUnclTxt.minWidth = 40f;
            leUnclTxt.flexibleWidth = 1f;

            TextMeshProUGUI tmpUncl = objUnclTxt.GetComponent<TextMeshProUGUI>();
            if (font != null) tmpUncl.font = font;
            tmpUncl.fontSize = 22f;
            tmpUncl.enableAutoSizing = true;
            tmpUncl.fontSizeMin = 18f;
            tmpUncl.fontSizeMax = 22f;
            tmpUncl.fontStyle = FontStyles.Bold;
            tmpUncl.color = new Color(0.773f, 0.8f, 0.902f, 1f); // #C5CCE6
            tmpUncl.text = "CHƯA ĐẠT";
            tmpUncl.alignment = TextAlignmentOptions.Center;
            tmpUncl.textWrappingMode = TextWrappingModes.NoWrap;
            tmpUncl.raycastTarget = false;

            rowUI.unclaimedBadge = objUnclaimed;
            rowUI.unclaimedBadgeBorder = imgBorder;
            rowUI.unclaimedBadgeBg = imgFill;
            rowUI.unclaimedLockIcon = imgLock;
            rowUI.unclaimedBadgeText = tmpUncl;
            rowUI.lockedStatus = objUnclaimed;
            rowUI.lockedIcon = imgLock;
            objUnclaimed.SetActive(!initialClaimed);

            return rowUI;
        }
    }
}
