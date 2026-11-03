using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Editor
{
    public static class SetupUIPhase1
    {
        private static bool hasExecuted = false;

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            EditorApplication.update += RunOnce;
        }

        private static void RunOnce()
        {
            if (hasExecuted) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

            string doneFlag = "Temp/ui_phase1_executed.txt";
            if (File.Exists(doneFlag))
            {
                hasExecuted = true;
                EditorApplication.update -= RunOnce;
                return;
            }

            hasExecuted = true;
            EditorApplication.update -= RunOnce;

            try
            {
                File.WriteAllText(doneFlag, DateTime.Now.ToString());
                RunAllSetup();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SetupUIPhase1] Lỗi khi chạy: {ex}");
            }
        }

        [MenuItem("DemHoiDenLong/UI/Setup Phase 1 UI")]
        public static void RunAllSetup()
        {
            Debug.Log("==================================================");
            Debug.Log("   BẮT ĐẦU THIẾT LẬP GIAI ĐOẠN UI-1 TOÀN DIỆN    ");
            Debug.Log("==================================================");

            // BƯỚC 0: Import tài nguyên & kiểm tra nền trắng
            ButtonAssetTools.ImportButtonAssets();
            ButtonAssetTools.RemoveWhiteBackground();

            // BƯỚC 1: Cấu hình Scene Main_Menu (Hangar, Nút mới, Dựng lại Bảng truyền thuyết)
            SetupMainMenuScene();

            // BƯỚC 2: Cấu hình Scene LevelSelect (Popup LevelDetailModal)
            SetupLevelSelectScene();

            // BƯỚC 3: Cấu hình Scene Gameplay (Cài Đặt giữa trận, Bảng Thắng/Thua)
            SetupGameplayScene();

            // BƯỚC 4: Đồng bộ Prefab SettingsUI
            SetupSettingsUIPrefab();

            // BƯỚC 5: Chạy kiểm tra Missing Glyphs & Self Test
            FindMissingGlyphs.ScanMissingGlyphs();
            DemHoiDenLong.EditorTools.LevelProgressSelfTest.RunAllTests();

            Debug.Log("==================================================");
            Debug.Log("   HOÀN TẤT THIẾT LẬP GIAI ĐOẠN UI-1 THÀNH CÔNG! ");
            Debug.Log("==================================================");
        }

        // ==========================================
        // 1. MAIN MENU SCENE SETUP
        // ==========================================
        private static void SetupMainMenuScene()
        {
            string scenePath = "Assets/Scenes/Main_Menu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            HangarUI hangarUI = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
            if (hangarUI == null)
            {
                Debug.LogError("[Setup Main_Menu] Không tìm thấy HangarUI trong Scene!");
                return;
            }

            SerializedObject soHangar = new SerializedObject(hangarUI);

            // 1.1 Nút CHƠI (GoldPrimary, icon_play đứng trước chữ)
            var playBtnProp = soHangar.FindProperty("playBtn");
            if (playBtnProp != null && playBtnProp.objectReferenceValue != null)
            {
                Button playBtn = playBtnProp.objectReferenceValue as Button;
                SetupPlayButton(playBtn);
            }

            // 1.2 Nút MỞ KHÓA (GoldPrimary, giữ icon tiền tệ và ổ khóa hiện có)
            var unlockBtnProp = soHangar.FindProperty("unlockBtn");
            if (unlockBtnProp != null && unlockBtnProp.objectReferenceValue != null)
            {
                Button unlockBtn = unlockBtnProp.objectReferenceValue as Button;
                SetupUnlockButton(unlockBtn);
            }

            // 1.3 Nút ĐỌC TRUYỀN THUYẾT (NavySecondary, icon_book)
            var readStoryBtnProp = soHangar.FindProperty("readStoryBtn");
            if (readStoryBtnProp != null && readStoryBtnProp.objectReferenceValue != null)
            {
                Button readStoryBtn = readStoryBtnProp.objectReferenceValue as Button;
                SetupReadStoryButton(readStoryBtn);

                var txtProp = soHangar.FindProperty("readStoryBtnText");
                if (txtProp != null)
                {
                    txtProp.objectReferenceValue = readStoryBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                UnityEditor.Events.UnityEventTools.RemovePersistentListener(readStoryBtn.onClick, hangarUI.OpenStoryPanel);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(readStoryBtn.onClick, hangarUI.OpenStoryPanel);
                readStoryBtn.onClick.SetPersistentListenerState(readStoryBtn.onClick.GetPersistentEventCount() - 1, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
            }

            // 1.4 DỰNG LẠI BẢNG TRUYỀN THUYẾT (StoryPanel -> LorePanelUI)
            Transform canvasTr = hangarUI.transform.parent != null ? hangarUI.transform.parent : hangarUI.transform;
            Transform safeAreaTr = canvasTr.Find("SafeArea");
            Transform parentContainer = safeAreaTr != null ? safeAreaTr : canvasTr;

            var storyPanelProp = soHangar.FindProperty("storyPanel");
            GameObject existingStoryPanel = storyPanelProp != null ? storyPanelProp.objectReferenceValue as GameObject : null;
            if (existingStoryPanel == null)
            {
                Transform found = parentContainer.Find("StoryPanel");
                if (found != null) existingStoryPanel = found.gameObject;
            }

            LorePanelUI lorePanel = RebuildLorePanel(parentContainer, existingStoryPanel);
            if (lorePanel != null)
            {
                var loreProp = soHangar.FindProperty("lorePanelUI");
                if (loreProp != null) loreProp.objectReferenceValue = lorePanel;

                if (storyPanelProp != null) storyPanelProp.objectReferenceValue = lorePanel.gameObject;

                var txtOld = soHangar.FindProperty("storyContentText");
                if (txtOld != null) txtOld.objectReferenceValue = lorePanel.transform.Find("StoryCard/ScrollArea/Viewport/Content/StoryBodyText")?.GetComponent<TextMeshProUGUI>();

                var closeOld = soHangar.FindProperty("closeStoryBtn");
                if (closeOld != null) closeOld.objectReferenceValue = lorePanel.transform.Find("ClosePillButton")?.GetComponent<Button>() ?? lorePanel.transform.Find("StoryCard/CloseButtonBottom")?.GetComponent<Button>();
            }

            // 1.5 Cài đặt ở Main_Menu (SettingsUI)
            SettingsUI settingsUI = UnityEngine.Object.FindAnyObjectByType<SettingsUI>();
            if (settingsUI != null)
            {
                SetupSettingsUI(settingsUI, isGameplay: false);
            }

            soHangar.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Setup Main_Menu] Đã cập nhật xong Scene Main_Menu.");
        }

        private static void SetupPlayButton(Button playBtn)
        {
            if (playBtn == null) return;
            GameObject go = playBtn.gameObject;

            // Kiểm tra và gỡ icon tối nhỏ sót lại từ template cũ (Assets/_Project/Art/Icons/gamepad.png)
            var allImages = go.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img.gameObject != go && (img.sprite == null || img.sprite.name.Contains("gamepad") || img.name == "Icon"))
                {
                    Debug.Log($"[Hangar PlayBtn] Đã gỡ icon lạ bên phải: '{img.gameObject.name}', Sprite: '{(img.sprite != null ? img.sprite.name : "null")}' (Tàn dư gamepad từ template ban đầu).");
                    UnityEngine.Object.DestroyImmediate(img.gameObject);
                }
            }

            // Gắn component StyledButton
            StyledButton sb = go.GetComponent<StyledButton>();
            if (sb == null) sb = go.AddComponent<StyledButton>();
            sb.StyleType = ButtonStyleType.GoldPrimary;

            // Cấu hình Icon icon_play đứng trước chữ
            Transform iconTr = go.transform.Find("Icon_Play");
            if (iconTr == null)
            {
                GameObject iconObj = new GameObject("Icon_Play", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                iconObj.transform.SetParent(go.transform, false);
                iconTr = iconObj.transform;
            }
            iconTr.SetAsFirstSibling();

            Image icImg = iconTr.GetComponent<Image>();
            icImg.sprite = ButtonAssetTools.LoadSprite("icon_play");
            icImg.preserveAspect = true;
            icImg.raycastTarget = false;

            RectTransform iconRt = iconTr.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(36f, 36f);

            LayoutElement le = iconTr.GetComponent<LayoutElement>();
            le.preferredWidth = 36f;
            le.preferredHeight = 36f;
            le.minWidth = 36f;
            le.minHeight = 36f;
            le.flexibleWidth = 0f;
            le.flexibleHeight = 0f;

            // Nhãn TMP
            var txt = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
            {
                txt.text = "CHƠI";
            }

            sb.AutoFindReferences();
            sb.ApplyStyle();
        }

        private static void SetupUnlockButton(Button unlockBtn)
        {
            if (unlockBtn == null) return;
            GameObject go = unlockBtn.gameObject;

            StyledButton sb = go.GetComponent<StyledButton>();
            if (sb == null) sb = go.AddComponent<StyledButton>();
            sb.StyleType = ButtonStyleType.GoldPrimary;

            sb.AutoFindReferences();
            sb.ApplyStyle();
        }

        private static void SetupReadStoryButton(Button readStoryBtn)
        {
            if (readStoryBtn == null) return;
            GameObject go = readStoryBtn.gameObject;

            StyledButton sb = go.GetComponent<StyledButton>();
            if (sb == null) sb = go.AddComponent<StyledButton>();
            sb.StyleType = ButtonStyleType.NavySecondary;

            // Cấu hình icon_book đứng trước chữ
            Transform iconTr = go.transform.Find("Icon_Book");
            if (iconTr == null)
            {
                GameObject iconObj = new GameObject("Icon_Book", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                iconObj.transform.SetParent(go.transform, false);
                iconTr = iconObj.transform;
            }
            iconTr.SetAsFirstSibling();

            Image icImg = iconTr.GetComponent<Image>();
            icImg.sprite = ButtonAssetTools.LoadSprite("icon_book");
            icImg.preserveAspect = true;
            icImg.raycastTarget = false;

            RectTransform iconRt = iconTr.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(34f, 34f);

            LayoutElement le = iconTr.GetComponent<LayoutElement>();
            le.preferredWidth = 34f;
            le.preferredHeight = 34f;
            le.minWidth = 34f;
            le.minHeight = 34f;
            le.flexibleWidth = 0f;
            le.flexibleHeight = 0f;

            var txt = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
            {
                txt.text = "ĐỌC TRUYỀN THUYẾT KIM LÂN";
            }

            sb.AutoFindReferences();
            sb.ApplyStyle();
        }

        // ==========================================
        // DỰNG LẠI BẢNG TRUYỀN THUYẾT (LOREPANELUI)
        // ==========================================
        private static LorePanelUI RebuildLorePanel(Transform parent, GameObject oldPanel)
        {
            GameObject panelObj;
            if (oldPanel != null)
            {
                panelObj = oldPanel;
                while (panelObj.transform.childCount > 0)
                {
                    UnityEngine.Object.DestroyImmediate(panelObj.transform.GetChild(0).gameObject);
                }
            }
            else
            {
                panelObj = new GameObject("StoryPanel", typeof(RectTransform), typeof(CanvasGroup));
                panelObj.transform.SetParent(parent, false);
            }

            panelObj.name = "StoryPanel";
            RectTransform pRt = panelObj.GetComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero;
            pRt.anchorMax = Vector2.one;
            pRt.offsetMin = Vector2.zero;
            pRt.offsetMax = Vector2.zero;
            pRt.pivot = new Vector2(0.5f, 0.5f);

            // Nested Canvas & GraphicRaycaster (Tối ưu mục 9: không rebuild Canvas chính khi cuộn)
            Canvas nCanvas = panelObj.GetComponent<Canvas>();
            if (nCanvas == null) nCanvas = panelObj.AddComponent<Canvas>();
            nCanvas.overrideSorting = true;
            nCanvas.sortingOrder = 50;

            GraphicRaycaster gr = panelObj.GetComponent<GraphicRaycaster>();
            if (gr == null) gr = panelObj.AddComponent<GraphicRaycaster>();

            CanvasGroup panelCg = panelObj.GetComponent<CanvasGroup>();
            if (panelCg == null) panelCg = panelObj.AddComponent<CanvasGroup>();

            // 1. Backdrop: Image full-screen màu #0A1029 alpha 0.80, Button đóng bảng (Mục 3, 4)
            GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(panelObj.transform, false);
            RectTransform bdRt = backdrop.GetComponent<RectTransform>();
            bdRt.anchorMin = Vector2.zero;
            bdRt.anchorMax = Vector2.one;
            bdRt.offsetMin = Vector2.zero;
            bdRt.offsetMax = Vector2.zero;

            Image bdImg = backdrop.GetComponent<Image>();
            ColorUtility.TryParseHtmlString("#0A1029", out Color bdColor);
            bdColor.a = 0.80f;
            bdImg.color = bdColor;
            bdImg.raycastTarget = true;

            Button bdBtn = backdrop.GetComponent<Button>();
            bdBtn.transition = Selectable.Transition.None;

            // 2. Thẻ story_card: rộng 960, cao 1140 (khung 9-slice, Mục 6)
            GameObject cardObj = new GameObject("StoryCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            cardObj.transform.SetParent(panelObj.transform, false);
            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(960f, 1140f);
            cardRt.anchoredPosition = new Vector2(0f, 35f);

            Image cardImg = cardObj.GetComponent<Image>();
            cardImg.sprite = ButtonAssetTools.LoadSprite("story_card");
            cardImg.type = Image.Type.Sliced;
            cardImg.pixelsPerUnitMultiplier = 1f;
            cardImg.raycastTarget = true;

            CanvasGroup cardCg = cardObj.GetComponent<CanvasGroup>();

            // 3. Ảnh Lân trong vòng story_medallion đường kính 300 vắt qua mép trên thẻ (Mục 6, 7 idle bob)
            GameObject medallionObj = new GameObject("StoryMedallion", typeof(RectTransform));
            medallionObj.transform.SetParent(cardObj.transform, false);
            RectTransform medRt = medallionObj.GetComponent<RectTransform>();
            medRt.anchorMin = new Vector2(0.5f, 1f);
            medRt.anchorMax = new Vector2(0.5f, 1f);
            medRt.pivot = new Vector2(0.5f, 0.5f);
            medRt.anchoredPosition = new Vector2(0f, 0f);
            medRt.sizeDelta = new Vector2(300f, 300f);

            // 3.1 Avatar Mask bo tròn ~250
            GameObject avatarMaskObj = new GameObject("AvatarMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            avatarMaskObj.transform.SetParent(medallionObj.transform, false);
            RectTransform amRt = avatarMaskObj.GetComponent<RectTransform>();
            amRt.anchorMin = new Vector2(0.5f, 0.5f);
            amRt.anchorMax = new Vector2(0.5f, 0.5f);
            amRt.pivot = new Vector2(0.5f, 0.5f);
            amRt.sizeDelta = new Vector2(250f, 250f);

            Image maskImg = avatarMaskObj.GetComponent<Image>();
            Sprite circleSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_circle.png");
            if (circleSpr != null) maskImg.sprite = circleSpr;
            maskImg.raycastTarget = false;

            Mask amMask = avatarMaskObj.GetComponent<Mask>();
            amMask.showMaskGraphic = false;

            GameObject avatarImgObj = new GameObject("LanAvatar", typeof(RectTransform), typeof(Image));
            avatarImgObj.transform.SetParent(avatarMaskObj.transform, false);
            RectTransform avRt = avatarImgObj.GetComponent<RectTransform>();
            avRt.anchorMin = Vector2.zero;
            avRt.anchorMax = Vector2.one;
            avRt.sizeDelta = Vector2.zero;
            Image avImg = avatarImgObj.GetComponent<Image>();
            avImg.preserveAspect = true;
            avImg.raycastTarget = false;

            // 3.2 Medallion Ring đè lên trên Avatar Mask
            GameObject ringObj = new GameObject("MedallionRing", typeof(RectTransform), typeof(Image));
            ringObj.transform.SetParent(medallionObj.transform, false);
            RectTransform rRt = ringObj.GetComponent<RectTransform>();
            rRt.anchorMin = Vector2.zero;
            rRt.anchorMax = Vector2.one;
            rRt.sizeDelta = Vector2.zero;
            Image ringImg = ringObj.GetComponent<Image>();
            ringImg.sprite = ButtonAssetTools.LoadSprite("story_medallion");
            ringImg.preserveAspect = true;
            ringImg.raycastTarget = false;

            // 4. Nhãn TRUYỀN THUYẾT (cỡ 30 bold, #6B4A2B, Mục 5)
            GameObject subheadObj = new GameObject("SubheadText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subheadObj.transform.SetParent(cardObj.transform, false);
            RectTransform shRt = subheadObj.GetComponent<RectTransform>();
            shRt.anchorMin = new Vector2(0.5f, 1f);
            shRt.anchorMax = new Vector2(0.5f, 1f);
            shRt.pivot = new Vector2(0.5f, 1f);
            shRt.anchoredPosition = new Vector2(0f, -145f);
            shRt.sizeDelta = new Vector2(600f, 38f);

            var shText = subheadObj.GetComponent<TextMeshProUGUI>();
            shText.text = "TRUYỀN THUYẾT";
            shText.fontSize = 30f;
            shText.fontStyle = FontStyles.Bold;
            shText.alignment = TextAlignmentOptions.Center;
            shText.overflowMode = TextOverflowModes.Ellipsis;
            shText.enableWordWrapping = false;
            shText.raycastTarget = false;
            ColorUtility.TryParseHtmlString("#6B4A2B", out Color shColor);
            shText.color = shColor;

            // 5. Tên Lân trên dải story_ribbon (640x140, nhuộm màu nhấn, Mục 6, 8)
            GameObject ribbonObj = new GameObject("StoryRibbon", typeof(RectTransform), typeof(Image));
            ribbonObj.transform.SetParent(cardObj.transform, false);
            RectTransform ribRt = ribbonObj.GetComponent<RectTransform>();
            ribRt.anchorMin = new Vector2(0.5f, 1f);
            ribRt.anchorMax = new Vector2(0.5f, 1f);
            ribRt.pivot = new Vector2(0.5f, 0.5f);
            ribRt.anchoredPosition = new Vector2(0f, -220f);
            ribRt.sizeDelta = new Vector2(640f, 140f);

            Image ribImg = ribbonObj.GetComponent<Image>();
            ribImg.sprite = ButtonAssetTools.LoadSprite("story_ribbon");
            ribImg.preserveAspect = true;
            ribImg.raycastTarget = false;

            GameObject titleObj = new GameObject("LanTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(ribbonObj.transform, false);
            RectTransform titRt = titleObj.GetComponent<RectTransform>();
            titRt.anchorMin = Vector2.zero;
            titRt.anchorMax = Vector2.one;
            titRt.sizeDelta = Vector2.zero;

            var titText = titleObj.GetComponent<TextMeshProUGUI>();
            titText.text = "KIM LÂN";
            titText.fontSize = 54f;
            titText.fontStyle = FontStyles.Bold;
            titText.alignment = TextAlignmentOptions.Center;
            titText.raycastTarget = false;
            ColorUtility.TryParseHtmlString("#2B1B0E", out Color titColor);
            titText.color = titColor;

            // 6. Dòng biệt danh (cỡ 30, #6B4A2B, Mục 5)
            GameObject nickObj = new GameObject("NicknameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            nickObj.transform.SetParent(cardObj.transform, false);
            RectTransform nkRt = nickObj.GetComponent<RectTransform>();
            nkRt.anchorMin = new Vector2(0.5f, 1f);
            nkRt.anchorMax = new Vector2(0.5f, 1f);
            nkRt.pivot = new Vector2(0.5f, 1f);
            nkRt.anchoredPosition = new Vector2(0f, -295f);
            nkRt.sizeDelta = new Vector2(600f, 40f);

            var nkText = nickObj.GetComponent<TextMeshProUGUI>();
            nkText.text = "Lân Nhỏ giữ trống";
            nkText.fontSize = 30f;
            nkText.alignment = TextAlignmentOptions.Center;
            nkText.raycastTarget = false;
            ColorUtility.TryParseHtmlString("#6B4A2B", out Color nkColor);
            nkText.color = nkColor;

            // 7. Vạch ngăn story_divider (Mục 8)
            GameObject divObj = new GameObject("StoryDivider", typeof(RectTransform), typeof(Image));
            divObj.transform.SetParent(cardObj.transform, false);
            RectTransform divRt = divObj.GetComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0.5f, 1f);
            divRt.anchorMax = new Vector2(0.5f, 1f);
            divRt.pivot = new Vector2(0.5f, 0.5f);
            divRt.anchoredPosition = new Vector2(0f, -340f);
            divRt.sizeDelta = new Vector2(816f, 54f);

            Image divImg = divObj.GetComponent<Image>();
            divImg.sprite = ButtonAssetTools.LoadSprite("story_divider");
            divImg.preserveAspect = true;
            divImg.raycastTarget = false;

            // 8. VÙNG CUỘN LORE (Mục 1: ScrollRect + RectMask2D, không dùng Mask)
            GameObject scrollObj = new GameObject("ScrollArea", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(cardObj.transform, false);
            RectTransform scRt = scrollObj.GetComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 0.5f);
            scRt.anchorMax = new Vector2(0.5f, 0.5f);
            scRt.pivot = new Vector2(0.5f, 0.5f);
            scRt.anchoredPosition = new Vector2(0f, -40f);
            scRt.sizeDelta = new Vector2(760f, 480f);

            ScrollRect sRect = scrollObj.GetComponent<ScrollRect>();
            sRect.horizontal = false;
            sRect.vertical = true;
            sRect.movementType = ScrollRect.MovementType.Elastic;
            sRect.inertia = true;
            sRect.scrollSensitivity = 25f;

            // 8.1 Viewport (RectMask2D)
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform vpRt = viewportObj.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.sizeDelta = new Vector2(-18f, 0f);
            vpRt.anchoredPosition = new Vector2(-9f, 0f);

            var rMask = viewportObj.GetComponent<RectMask2D>();
            if (rMask != null) rMask.softness = new Vector2Int(0, 40);
            sRect.viewport = vpRt;

            // 8.2 Content (VerticalLayoutGroup + ContentSizeFitter, padding >= 24px)
            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform cRt = contentObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, 0f);

            var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 24, 24); // Padding trên dưới để không bị che
            vlg.spacing = 16f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sRect.content = cRt;

            // 8.3 Nội dung TextMeshPro (cỡ 35, màu #3B2A1A, LineSpacing 20, ParagraphSpacing 20, căn trái)
            GameObject storyTextObj = new GameObject("StoryBodyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            storyTextObj.transform.SetParent(contentObj.transform, false);
            var sbText = storyTextObj.GetComponent<TextMeshProUGUI>();
            sbText.fontSize = 35f;
            sbText.lineSpacing = 20f;
            sbText.paragraphSpacing = 20f;
            sbText.alignment = TextAlignmentOptions.TopLeft;
            ColorUtility.TryParseHtmlString("#3B2A1A", out Color txtColor);
            sbText.color = txtColor;
            sbText.textWrappingMode = TextWrappingModes.Normal;
            sbText.enableAutoSizing = false;
            sbText.raycastTarget = false;

            // 8.4 Mờ dần ở mép trên và dưới: dùng RectMask2D.softness = (0, 40) trên Viewport (Mục 1)
            // 8.5 Scrollbar: rộng 10px, tay cầm vàng #D9A441 bo tròn (Mục 1)
            GameObject scrollbarObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObj.transform.SetParent(scrollObj.transform, false);
            RectTransform sbRt = scrollbarObj.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1f, 0f);
            sbRt.anchorMax = new Vector2(1f, 1f);
            sbRt.pivot = new Vector2(1f, 0.5f);
            sbRt.sizeDelta = new Vector2(10f, 0f);
            sbRt.anchoredPosition = Vector2.zero;

            Image trackImg = scrollbarObj.GetComponent<Image>();
            trackImg.color = new Color(0, 0, 0, 0);

            Scrollbar sBar = scrollbarObj.GetComponent<Scrollbar>();
            sBar.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarObj.transform, false);
            RectTransform saRt = slidingArea.GetComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(slidingArea.transform, false);
            RectTransform hRt = handle.GetComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.sizeDelta = Vector2.zero;

            Image hImg = handle.GetComponent<Image>();
            Sprite pillSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_pill.png");
            if (pillSpr != null)
            {
                hImg.sprite = pillSpr;
                hImg.type = Image.Type.Sliced;
            }
            ColorUtility.TryParseHtmlString("#D9A441", out Color goldHandle);
            hImg.color = goldHandle;

            sBar.handleRect = hRt;
            sBar.targetGraphic = hImg;
            sRect.verticalScrollbar = sBar;
            sRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            // 9. KHỐI KỸ NĂNG: Tách khỏi vùng cuộn, cố định ở đáy thẻ, nền đặc #152042 bo tròn (Mục 2)
            GameObject skillBlockObj = new GameObject("SkillBlock", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            skillBlockObj.transform.SetParent(cardObj.transform, false);
            RectTransform scpRt = skillBlockObj.GetComponent<RectTransform>();
            scpRt.anchorMin = new Vector2(0.5f, 0f);
            scpRt.anchorMax = new Vector2(0.5f, 0f);
            scpRt.pivot = new Vector2(0.5f, 0.5f);
            scpRt.anchoredPosition = new Vector2(0f, 85f);
            scpRt.sizeDelta = new Vector2(760f, 110f);

            Image scpBg = skillBlockObj.GetComponent<Image>();
            Sprite uiRound36 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_round_36.png") ?? pillSpr;
            if (uiRound36 != null)
            {
                scpBg.sprite = uiRound36;
                scpBg.type = Image.Type.Sliced;
            }
            ColorUtility.TryParseHtmlString("#152042", out Color chipBgColor);
            chipBgColor.a = 1.0f; // Nền đặc không trong suốt
            scpBg.color = chipBgColor;
            scpBg.raycastTarget = false;

            var chipHlg = skillBlockObj.GetComponent<HorizontalLayoutGroup>();
            chipHlg.childAlignment = TextAnchor.MiddleLeft;
            chipHlg.spacing = 18f;
            chipHlg.childControlWidth = true;
            chipHlg.childControlHeight = true;
            chipHlg.childForceExpandWidth = false;
            chipHlg.childForceExpandHeight = false;
            chipHlg.padding = new RectOffset(24, 24, 12, 12);

            // Icon kỹ năng (60x60)
            GameObject chipIconObj = new GameObject("SkillIcon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            chipIconObj.transform.SetParent(skillBlockObj.transform, false);
            Image ciImg = chipIconObj.GetComponent<Image>();
            ciImg.preserveAspect = true;
            ciImg.raycastTarget = false;
            LayoutElement ciLe = chipIconObj.GetComponent<LayoutElement>();
            ciLe.preferredWidth = 60f;
            ciLe.preferredHeight = 60f;
            ciLe.minWidth = 60f;
            ciLe.minHeight = 60f;

            // Cột Text: Tên kỹ năng (1 dòng) + Mô tả ngắn (tối đa 2 dòng)
            GameObject textColObj = new GameObject("TextColumn", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textColObj.transform.SetParent(skillBlockObj.transform, false);
            LayoutElement colLe = textColObj.GetComponent<LayoutElement>();
            colLe.flexibleWidth = 1f;

            var colVlg = textColObj.GetComponent<VerticalLayoutGroup>();
            colVlg.childAlignment = TextAnchor.MiddleLeft;
            colVlg.spacing = 4f;
            colVlg.childControlWidth = true;
            colVlg.childControlHeight = true;
            colVlg.childForceExpandWidth = true;
            colVlg.childForceExpandHeight = false;

            GameObject skillNameObj = new GameObject("SkillNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            skillNameObj.transform.SetParent(textColObj.transform, false);
            var snText = skillNameObj.GetComponent<TextMeshProUGUI>();
            snText.text = "KỸ NĂNG · LƯỚT NHANH BẤT TỬ";
            snText.fontSize = 26f;
            snText.fontStyle = FontStyles.Bold;
            snText.raycastTarget = false;

            GameObject skillDescObj = new GameObject("SkillDescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            skillDescObj.transform.SetParent(textColObj.transform, false);
            var sdText = skillDescObj.GetComponent<TextMeshProUGUI>();
            sdText.text = "Lân lướt nhanh về phía trước, bất tử tạm thời và hất tung kẻ địch.";
            sdText.fontSize = 22f;
            sdText.textWrappingMode = TextWrappingModes.Normal;
            ColorUtility.TryParseHtmlString("#E8D9B0", out Color sdColor);
            sdText.color = sdColor;
            sdText.raycastTarget = false;

            // 10. GỘP NÚT ĐÓNG: Nút "ĐÓNG" duy nhất dạng pill đỏ #E34233 (360x110) nằm ngoài dưới thẻ (Mục 3)
            GameObject closeBtnObj = new GameObject("ClosePillButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(panelObj.transform, false);
            RectTransform cbRt = closeBtnObj.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(0.5f, 0.5f);
            cbRt.anchorMax = new Vector2(0.5f, 0.5f);
            cbRt.pivot = new Vector2(0.5f, 0.5f);
            cbRt.anchoredPosition = new Vector2(0f, -600f); // Nằm trong SafeArea ngay dưới StoryCard, KHÔNG đè lên viền
            cbRt.sizeDelta = new Vector2(360f, 100f);

            Image closeImg = closeBtnObj.GetComponent<Image>();
            if (pillSpr != null)
            {
                closeImg.sprite = pillSpr;
                closeImg.type = Image.Type.Sliced;
            }
            ColorUtility.TryParseHtmlString("#E34233", out Color closeRed);
            closeImg.color = closeRed;
            closeImg.raycastTarget = true;

            Button cbBtn = closeBtnObj.GetComponent<Button>();
            cbBtn.targetGraphic = closeImg;
            cbBtn.transition = Selectable.Transition.ColorTint;

            GameObject closeTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform ctRt = closeTextObj.GetComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.sizeDelta = Vector2.zero;

            var cbText = closeTextObj.GetComponent<TextMeshProUGUI>();
            cbText.text = "ĐÓNG";
            cbText.fontSize = 34f;
            cbText.fontStyle = FontStyles.Bold;
            cbText.color = Color.white;
            cbText.alignment = TextAlignmentOptions.Center;
            cbText.raycastTarget = false;

            // GẮN COMPONENT LOREPANELUI VÀ BIND REFERENCES
            LorePanelUI lorePanel = panelObj.GetComponent<LorePanelUI>();
            if (lorePanel == null) lorePanel = panelObj.AddComponent<LorePanelUI>();

            SerializedObject so = new SerializedObject(lorePanel);
            so.FindProperty("nestedCanvas").objectReferenceValue = nCanvas;
            so.FindProperty("graphicRaycaster").objectReferenceValue = gr;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = panelCg;
            so.FindProperty("backdropImage").objectReferenceValue = bdImg;
            so.FindProperty("backdropButton").objectReferenceValue = bdBtn;
            so.FindProperty("cardRect").objectReferenceValue = cardRt;
            so.FindProperty("cardCanvasGroup").objectReferenceValue = cardCg;

            so.FindProperty("medallionRect").objectReferenceValue = medRt;
            so.FindProperty("medallionImage").objectReferenceValue = ringImg;
            so.FindProperty("lanAvatarImage").objectReferenceValue = avImg;

            so.FindProperty("loreSubheadText").objectReferenceValue = shText;
            so.FindProperty("ribbonImage").objectReferenceValue = ribImg;
            so.FindProperty("lanTitleText").objectReferenceValue = titText;
            so.FindProperty("nicknameText").objectReferenceValue = nkText;
            so.FindProperty("dividerImage").objectReferenceValue = divImg;

            so.FindProperty("scrollRect").objectReferenceValue = sRect;
            so.FindProperty("contentRect").objectReferenceValue = cRt;
            so.FindProperty("storyBodyText").objectReferenceValue = sbText;
            so.FindProperty("scrollbar").objectReferenceValue = sBar;
            so.FindProperty("topGradientFade").objectReferenceValue = null;
            so.FindProperty("bottomGradientFade").objectReferenceValue = null;

            so.FindProperty("skillBlockBg").objectReferenceValue = scpBg;
            so.FindProperty("skillBlockIcon").objectReferenceValue = ciImg;
            so.FindProperty("skillNameText").objectReferenceValue = snText;
            so.FindProperty("skillDescText").objectReferenceValue = sdText;

            var pChipBg = so.FindProperty("skillChipBg");
            if (pChipBg != null) pChipBg.objectReferenceValue = scpBg;
            var pChipIcon = so.FindProperty("skillChipIcon");
            if (pChipIcon != null) pChipIcon.objectReferenceValue = ciImg;
            var pChipText = so.FindProperty("skillChipText");
            if (pChipText != null) pChipText.objectReferenceValue = snText;

            so.FindProperty("closePillBtn").objectReferenceValue = cbBtn;
            so.FindProperty("closeBottomBtn").objectReferenceValue = cbBtn;

            so.ApplyModifiedProperties();

            if (bdBtn != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(bdBtn.onClick, lorePanel.Hide);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(bdBtn.onClick, lorePanel.Hide);
                bdBtn.onClick.SetPersistentListenerState(bdBtn.onClick.GetPersistentEventCount() - 1, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
            }
            if (cbBtn != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(cbBtn.onClick, lorePanel.Hide);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(cbBtn.onClick, lorePanel.Hide);
                cbBtn.onClick.SetPersistentListenerState(cbBtn.onClick.GetPersistentEventCount() - 1, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
            }

            // Mặc định ẩn
            panelObj.SetActive(false);

            Debug.Log("[Rebuild LorePanel] Dựng lại Bảng truyền thuyết hoàn tất thành công theo chuẩn 9 tiêu chí!");
            return lorePanel;
        }

        // ==========================================
        private static void SetupLevelSelectScene()
        {
            string scenePath = "Assets/Scenes/LevelSelect.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            LevelDetailModal modal = UnityEngine.Object.FindAnyObjectByType<LevelDetailModal>(FindObjectsInactive.Include);
            if (modal != null)
            {
                // Nút BẮT ĐẦU CHƠI (GoldPrimary, icon_play thay tam giác hiện có)
                if (modal.playButton != null)
                {
                    GameObject pGo = modal.playButton.gameObject;
                    StyledButton sb = pGo.GetComponent<StyledButton>();
                    if (sb == null) sb = pGo.AddComponent<StyledButton>();
                    sb.StyleType = ButtonStyleType.GoldPrimary;

                    if (modal.playIconImage != null)
                    {
                        modal.playIconImage.sprite = ButtonAssetTools.LoadSprite("icon_play");
                        modal.playIconImage.preserveAspect = true;
                    }

                    if (modal.playButtonText != null)
                    {
                        modal.playButtonText.text = "BẮT ĐẦU CHƠI";
                    }

                    sb.AutoFindReferences();
                    sb.ApplyStyle();
                }

                // Nút đóng X (btn_close)
                if (modal.closeButton != null)
                {
                    Image img = modal.closeButton.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = ButtonAssetTools.LoadSprite("btn_close");
                        img.preserveAspect = true;
                    }
                    RectTransform cRt = modal.closeButton.GetComponent<RectTransform>();
                    if (cRt != null) cRt.sizeDelta = new Vector2(84f, 84f);
                }

                EditorUtility.SetDirty(modal);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Setup LevelSelect] Đã cập nhật xong Scene LevelSelect.");
        }

                // ==========================================
        // 3. GAMEPLAY SCENE SETUP
        // ==========================================
        private static void SetupGameplayScene()
        {
            string scenePath = "Assets/Scenes/Gameplay.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 3.1 Cài Đặt trong trận (SettingsUI)
            SettingsUI settingsUI = UnityEngine.Object.FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Include);
            if (settingsUI != null)
            {
                SetupSettingsUI(settingsUI, isGameplay: true);
            }

            // 3.2 GameplayHUD (Bảng Chiến Thắng và Thua Cuộc)
            GameplayHUD hud = UnityEngine.Object.FindAnyObjectByType<GameplayHUD>(FindObjectsInactive.Include);
            if (hud != null)
            {
                SerializedObject soHud = new SerializedObject(hud);

                // Victory Panel: MÀN TIẾP (GoldPrimary), CHƠI LẠI và CHỌN MÀN (NavySecondary)
                var nextBtnProp = soHud.FindProperty("victoryNextLevelButton");
                if (nextBtnProp != null && nextBtnProp.objectReferenceValue != null)
                {
                    GameObject go = nextBtnProp.objectReferenceValue as GameObject;
                    SetupButtonWithStyle(go, ButtonStyleType.GoldPrimary, "MÀN TIẾP");
                }

                var vicReplayProp = soHud.FindProperty("victoryReplayButton");
                if (vicReplayProp != null && vicReplayProp.objectReferenceValue != null)
                {
                    Button btn = vicReplayProp.objectReferenceValue as Button;
                    SetupButtonWithStyle(btn.gameObject, ButtonStyleType.NavySecondary, "CHƠI LẠI");
                }

                var vicSelectProp = soHud.FindProperty("victorySelectLevelButton");
                if (vicSelectProp != null && vicSelectProp.objectReferenceValue != null)
                {
                    Button btn = vicSelectProp.objectReferenceValue as Button;
                    SetupButtonWithStyle(btn.gameObject, ButtonStyleType.NavySecondary, "CHỌN MÀN");
                }

                // GameOver Panel: CHƠI LẠI và CHỌN MÀN (NavySecondary)
                var goPanelProp = soHud.FindProperty("gameOverPanel");
                GameObject goPanel = goPanelProp != null ? goPanelProp.objectReferenceValue as GameObject : null;
                if (goPanel != null)
                {
                    Transform replayTr = goPanel.transform.Find("ReplayButton");
                    Transform giveUpTr = goPanel.transform.Find("GiveUpButton");

                    if (replayTr == null && giveUpTr != null)
                    {
                        GameObject newReplay = UnityEngine.Object.Instantiate(giveUpTr.gameObject, goPanel.transform);
                        newReplay.name = "ReplayButton";
                        replayTr = newReplay.transform;
                        replayTr.SetSiblingIndex(giveUpTr.GetSiblingIndex());
                    }

                    if (replayTr != null)
                    {
                        Button repBtn = replayTr.GetComponent<Button>();
                        SetupButtonWithStyle(replayTr.gameObject, ButtonStyleType.NavySecondary, "CHƠI LẠI");
                        var goReplayProp = soHud.FindProperty("gameOverReplayButton");
                        if (goReplayProp != null)
                        {
                            goReplayProp.objectReferenceValue = repBtn;
                        }
                    }

                    if (giveUpTr != null)
                    {
                        Button guBtn = giveUpTr.GetComponent<Button>();
                        SetupButtonWithStyle(giveUpTr.gameObject, ButtonStyleType.NavySecondary, "CHỌN MÀN");
                        var goGiveUpProp = soHud.FindProperty("gameOverGiveUpButton");
                        if (goGiveUpProp != null)
                        {
                            goGiveUpProp.objectReferenceValue = guBtn;
                        }
                    }
                }

                // Nút "HỒI SINH · XEM QUẢNG CÁO" (xanh lá hiện có) giữ nguyên màu và sprite, chỉ bỏ ký tự đặc biệt nếu có
                var reviveProp = soHud.FindProperty("reviveButtonObj");
                if (reviveProp != null && reviveProp.objectReferenceValue != null)
                {
                    GameObject rGo = reviveProp.objectReferenceValue as GameObject;
                    var rText = rGo.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (rText != null)
                    {
                        rText.text = "HỒI SINH · XEM QUẢNG CÁO";
                    }
                }

                soHud.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Setup Gameplay] Đã cập nhật xong Scene Gameplay.");
        }

        private static void SetupButtonWithStyle(GameObject go, ButtonStyleType style, string label)
        {
            if (go == null) return;
            StyledButton sb = go.GetComponent<StyledButton>();
            if (sb == null) sb = go.AddComponent<StyledButton>();
            sb.StyleType = style;

            var txt = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
            {
                txt.text = label;
            }

            sb.AutoFindReferences();
            sb.ApplyStyle();
        }

        private static void SetupSettingsUI(SettingsUI settingsUI, bool isGameplay)
        {
            if (settingsUI == null) return;
            SerializedObject so = new SerializedObject(settingsUI);

            // Nút Exit: CoralExit, icon_exit
            var exitProp = so.FindProperty("exitButton");
            if (exitProp != null && exitProp.objectReferenceValue != null)
            {
                Button exitBtn = exitProp.objectReferenceValue as Button;
                GameObject go = exitBtn.gameObject;
                RectTransform rt = go.GetComponent<RectTransform>();

                if (isGameplay)
                {
                    rt.sizeDelta = new Vector2(560f, 110f); // preferredWidth 560, cao 110, vừa MỘT dòng
                }
                else
                {
                    rt.sizeDelta = new Vector2(480f, 100f);
                }

                StyledButton sb = go.GetComponent<StyledButton>();
                if (sb == null) sb = go.AddComponent<StyledButton>();
                sb.StyleType = ButtonStyleType.CoralExit;

                Transform iconTr = go.transform.Find("Icon_Exit");
                if (iconTr == null)
                {
                    GameObject iconObj = new GameObject("Icon_Exit", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                    iconObj.transform.SetParent(go.transform, false);
                    iconTr = iconObj.transform;
                }
                iconTr.SetAsFirstSibling();

                Image icImg = iconTr.GetComponent<Image>();
                icImg.sprite = ButtonAssetTools.LoadSprite("icon_exit");
                icImg.preserveAspect = true;
                icImg.raycastTarget = false;

                RectTransform iconRt = iconTr.GetComponent<RectTransform>();
                iconRt.sizeDelta = new Vector2(64f, 64f);

                LayoutElement le = iconTr.GetComponent<LayoutElement>();
                le.preferredWidth = 64f;
                le.preferredHeight = 64f;
                le.minWidth = 64f;
                le.minHeight = 64f;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;

                var txt = go.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null)
                {
                    txt.text = isGameplay ? "VỀ CHỌN MÀN" : "THOÁT GAME";
                }

                sb.AutoFindReferences();
                sb.ApplyStyle();
            }

            // Nút đóng X (btn_close)
            var closeProp = so.FindProperty("closeButton");
            if (closeProp != null && closeProp.objectReferenceValue != null)
            {
                Button closeBtn = closeProp.objectReferenceValue as Button;
                Image img = closeBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = ButtonAssetTools.LoadSprite("btn_close");
                    img.preserveAspect = true;
                }
                RectTransform cRt = closeBtn.GetComponent<RectTransform>();
                cRt.sizeDelta = new Vector2(84f, 84f);
            }

            so.ApplyModifiedProperties();
        }

        private static void SetupSettingsUIPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/SettingsUI.prefab";
            if (!File.Exists(prefabPath)) return;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            SettingsUI settingsUI = root.GetComponent<SettingsUI>();
            if (settingsUI != null)
            {
                SetupSettingsUI(settingsUI, isGameplay: false);
            }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[Setup SettingsUI Prefab] Đã đồng bộ Prefab SettingsUI.");
        }
    }
}

// trigger recompile
