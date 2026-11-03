using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace DemHoiDenLong.EditorTools
{
    public static class FixActSelectView
    {
        private const string SCENE_PATH = "Assets/Scenes/LevelSelect.unity";
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/ActCard_Template.prefab";

        [MenuItem("DemHoiDenLong/Fix ActSelectView All")]
        public static void FixAll()
        {
            Debug.Log("=== BẮT ĐẦU FIX ACTSELECTVIEW TOÀN DIỆN ===");

            // 1. Mở Scene LevelSelect nếu chưa mở
            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.path != SCENE_PATH)
            {
                currentScene = EditorSceneManager.OpenScene(SCENE_PATH);
            }

            // 2. Sửa Prefab ActCard_Template trước
            FixActCardTemplatePrefab();

            // 3. Sửa Hierarchy và Components trong Scene
            FixSceneHierarchy();

            // 4. Lưu Scene và Prefab
            EditorSceneManager.SaveScene(currentScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== ĐÃ FIX XONG ACTSELECTVIEW! ĐANG CHẠY CHẨN ĐOÁN LẠI... ===");

            // 5. Chạy lại chẩn đoán
            DiagnoseActSelectView.RunDiagnosis();
        }

        private static void FixActCardTemplatePrefab()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (contents == null)
            {
                Debug.LogError("Không thể load prefab: " + PREFAB_PATH);
                return;
            }

            Debug.Log("1. Đang sửa Prefab ActCard_Template...");

            // Xóa mọi missing script trên prefab
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(contents);
            Transform[] allChildren = contents.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
            }
            Debug.Log($" - Đã xóa {removed} missing script(s) trên prefab ActCard_Template.");

            // Đảm bảo Image và Mask trên root
            Image rootImg = contents.GetComponent<Image>();
            if (rootImg == null) rootImg = contents.AddComponent<Image>();
            rootImg.type = Image.Type.Sliced;
            rootImg.color = new Color(0.086f, 0.133f, 0.29f, 1f); // #16224A

            Mask rootMask = contents.GetComponent<Mask>();
            if (rootMask == null) rootMask = contents.AddComponent<Mask>();
            rootMask.showMaskGraphic = true;

            // Thêm lại component Shadow chuẩn bằng Unity API
            Shadow shadow = contents.GetComponent<Shadow>();
            if (shadow == null) shadow = contents.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 120f / 255f);
            shadow.effectDistance = new Vector2(0f, -6f);

            // Đảm bảo CanvasGroup
            CanvasGroup cg = contents.GetComponent<CanvasGroup>();
            if (cg == null) cg = contents.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            // Đảm bảo Button
            Button btn = contents.GetComponent<Button>();
            if (btn == null) btn = contents.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // Đảm bảo LayoutElement
            LayoutElement le = contents.GetComponent<LayoutElement>();
            if (le == null) le = contents.AddComponent<LayoutElement>();
            le.preferredHeight = 440f;
            le.minHeight = 440f;

            // Đảm bảo component ActCardUI
            ActCardUI cardUI = contents.GetComponent<ActCardUI>();
            if (cardUI == null) cardUI = contents.AddComponent<ActCardUI>();

            // Nối toàn bộ tham chiếu Inspector cho ActCardUI
            cardUI.glowBorder = contents.transform.Find("GlowBorder")?.GetComponent<Image>();
            if (cardUI.glowBorder != null)
            {
                // Đưa GlowBorder lên trên BannerImage và InfoBar (trước LockOverlay)
                cardUI.glowBorder.transform.SetSiblingIndex(2);
                RectTransform gbRect = cardUI.glowBorder.rectTransform;
                gbRect.anchorMin = Vector2.zero;
                gbRect.anchorMax = Vector2.one;
                gbRect.anchoredPosition = Vector2.zero;
                gbRect.sizeDelta = Vector2.zero;
            }

            cardUI.bannerImage = contents.transform.Find("BannerImage")?.GetComponent<Image>();

            // TitleText: font 38, width 840, overflowMode Overflow để không bao giờ bị cắt
            Transform titleT = contents.transform.Find("BannerImage/TitleText");
            if (titleT != null)
            {
                cardUI.titleText = titleT.GetComponent<TMP_Text>();
                if (cardUI.titleText != null)
                {
                    cardUI.titleText.fontSize = 38f;
                    cardUI.titleText.enableAutoSizing = false;
                    cardUI.titleText.overflowMode = TextOverflowModes.Overflow;
                }
                RectTransform ttRect = titleT.GetComponent<RectTransform>();
                if (ttRect != null)
                {
                    ttRect.anchoredPosition = new Vector2(28f, 20f);
                    ttRect.sizeDelta = new Vector2(840f, 54f);
                }
            }

            cardUI.subtitleText = contents.transform.Find("BannerImage/SubtitleText")?.GetComponent<TMP_Text>();

            Transform starsRowT = contents.transform.Find("BannerImage/StarsRow");
            if (starsRowT != null)
            {
                cardUI.starsRow = starsRowT.gameObject;
                List<Image> starsList = new List<Image>();
                for (int i = 1; i <= 3; i++)
                {
                    Transform s = starsRowT.Find("Star_" + i);
                    if (s != null)
                    {
                        Image sImg = s.GetComponent<Image>();
                        if (sImg != null) starsList.Add(sImg);
                    }
                }
                cardUI.starIcons = starsList.ToArray();
            }

            // StatusBadge & BadgeDot & StatusTxt
            Transform statusBadgeT = contents.transform.Find("BannerImage/StatusBadge");
            if (statusBadgeT != null)
            {
                cardUI.statusBadge = statusBadgeT.gameObject;
                cardUI.statusBadgeBg = statusBadgeT.GetComponent<Image>();
                cardUI.statusBadgeText = statusBadgeT.Find("StatusTxt")?.GetComponent<TMP_Text>();
                cardUI.statusBadgeDot = statusBadgeT.Find("BadgeDot")?.GetComponent<Image>();

                cardUI.checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_check.png");
                cardUI.dotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_circle.png");

                RectTransform badgeRect = statusBadgeT.GetComponent<RectTransform>();
                if (badgeRect != null) badgeRect.sizeDelta = new Vector2(255f, 40f);

                Transform statusTxtT = statusBadgeT.Find("StatusTxt");
                if (statusTxtT != null)
                {
                    RectTransform stRect = statusTxtT.GetComponent<RectTransform>();
                    if (stRect != null) stRect.sizeDelta = new Vector2(190f, 32f);
                    if (cardUI.statusBadgeText != null)
                    {
                        cardUI.statusBadgeText.fontSize = 19f;
                        cardUI.statusBadgeText.fontStyle = FontStyles.Bold;
                        cardUI.statusBadgeText.textWrappingMode = TextWrappingModes.NoWrap;
                        cardUI.statusBadgeText.overflowMode = TextOverflowModes.Overflow;
                    }
                }

                HorizontalLayoutGroup bHlg = statusBadgeT.GetComponent<HorizontalLayoutGroup>();
                if (bHlg != null)
                {
                    bHlg.padding = new RectOffset(18, 18, 0, 0);
                    bHlg.spacing = 8f;
                    bHlg.childAlignment = TextAnchor.MiddleCenter;
                }

                if (cardUI.statusBadgeDot != null)
                {
                    cardUI.statusBadgeDot.rectTransform.sizeDelta = new Vector2(14f, 14f);
                    if (cardUI.checkSprite != null) cardUI.statusBadgeDot.sprite = cardUI.checkSprite;
                }
            }

            // ActionChevron: nút Play tròn góc dưới-phải banner
            Transform actionChevronT = contents.transform.Find("BannerImage/ActionChevron");
            if (actionChevronT != null)
            {
                cardUI.actionChevron = actionChevronT.gameObject;
                RectTransform acRect = actionChevronT.GetComponent<RectTransform>();
                if (acRect != null)
                {
                    acRect.anchorMin = new Vector2(1f, 0f);
                    acRect.anchorMax = new Vector2(1f, 0f);
                    acRect.pivot = new Vector2(1f, 0f);
                    acRect.anchoredPosition = new Vector2(-48f, 20f);
                    acRect.sizeDelta = new Vector2(56f, 56f);
                }
            }

            cardUI.actionPill = contents.transform.Find("BannerImage/ActionPill")?.gameObject;

            // InfoBar & FlagIcon & StarIcon bố cục sang trọng, cân đối
            Transform infoBar = contents.transform.Find("InfoBar");
            if (infoBar != null)
            {
                RectTransform ibRect = infoBar.GetComponent<RectTransform>();
                if (ibRect != null) ibRect.sizeDelta = new Vector2(0f, 152f);

                HorizontalLayoutGroup ibHlg = infoBar.GetComponent<HorizontalLayoutGroup>();
                if (ibHlg != null)
                {
                    ibHlg.padding = new RectOffset(22, 22, 16, 16);
                    ibHlg.spacing = 20f;
                    ibHlg.childAlignment = TextAnchor.MiddleCenter;
                    ibHlg.childControlWidth = true;
                    ibHlg.childControlHeight = true;
                    ibHlg.childForceExpandWidth = true;
                    ibHlg.childForceExpandHeight = true;
                }

                // Cấu hình ô LeftInfo (Tiến độ ải)
                Transform leftInfo = infoBar.Find("LeftInfo");
                if (leftInfo != null)
                {
                    Image liImg = leftInfo.GetComponent<Image>();
                    if (liImg != null) liImg.color = new Color(0.094f, 0.149f, 0.33f, 0.95f);

                    HorizontalLayoutGroup liHlg = leftInfo.GetComponent<HorizontalLayoutGroup>();
                    if (liHlg != null)
                    {
                        liHlg.padding = new RectOffset(20, 20, 10, 10);
                        liHlg.spacing = 16f;
                        liHlg.childAlignment = TextAnchor.MiddleCenter;
                    }

                    Transform iconBadge = leftInfo.Find("IconBadge");
                    if (iconBadge != null)
                    {
                        (iconBadge as RectTransform).sizeDelta = new Vector2(58f, 58f);
                        Image bImg = iconBadge.GetComponent<Image>();
                        if (bImg != null) bImg.color = new Color(0.2f, 0.85f, 0.65f, 0.18f);
                    }

                    Transform flagIconT = leftInfo.Find("IconBadge/FlagIcon");
                    if (flagIconT != null)
                    {
                        (flagIconT as RectTransform).sizeDelta = new Vector2(36f, 36f);
                        Image flagImg = flagIconT.GetComponent<Image>();
                        if (flagImg != null)
                        {
                            Sprite flagSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/flag_square.png");
                            if (flagSprite != null) flagImg.sprite = flagSprite;
                            flagImg.color = Color.white;
                        }
                    }

                    Transform textsT = leftInfo.Find("Texts");
                    if (textsT != null)
                    {
                        (textsT as RectTransform).sizeDelta = new Vector2(290f, 70f);
                    }

                    cardUI.progressText = leftInfo.Find("Texts/ProgressText")?.GetComponent<TMP_Text>();
                    if (cardUI.progressText != null)
                    {
                        cardUI.progressText.fontSize = 32f;
                        cardUI.progressText.fontStyle = FontStyles.Bold;
                        cardUI.progressText.color = new Color(0.96f, 0.97f, 0.99f, 1f);
                    }

                    cardUI.progressLabelText = leftInfo.Find("Texts/Label")?.GetComponent<TMP_Text>();
                    if (cardUI.progressLabelText != null)
                    {
                        cardUI.progressLabelText.fontSize = 20f;
                        cardUI.progressLabelText.color = new Color(0.58f, 0.65f, 0.78f, 1f);
                    }
                }

                // Cấu hình ô RightInfo (Thu thập sao)
                Transform rightInfo = infoBar.Find("RightInfo");
                if (rightInfo != null)
                {
                    Image riImg = rightInfo.GetComponent<Image>();
                    if (riImg != null) riImg.color = new Color(0.094f, 0.149f, 0.33f, 0.95f);

                    HorizontalLayoutGroup riHlg = rightInfo.GetComponent<HorizontalLayoutGroup>();
                    if (riHlg != null)
                    {
                        riHlg.padding = new RectOffset(20, 20, 10, 10);
                        riHlg.spacing = 16f;
                        riHlg.childAlignment = TextAnchor.MiddleCenter;
                    }

                    Transform iconBadge = rightInfo.Find("IconBadge");
                    if (iconBadge != null)
                    {
                        (iconBadge as RectTransform).sizeDelta = new Vector2(58f, 58f);
                        Image bImg = iconBadge.GetComponent<Image>();
                        if (bImg != null) bImg.color = new Color(1f, 0.82f, 0.48f, 0.18f);
                    }

                    Transform starIconT = rightInfo.Find("IconBadge/StarIcon");
                    if (starIconT != null)
                    {
                        (starIconT as RectTransform).sizeDelta = new Vector2(36f, 36f);
                        Image starImg = starIconT.GetComponent<Image>();
                        if (starImg != null)
                        {
                            Sprite starSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/star.png");
                            if (starSprite != null) starImg.sprite = starSprite;
                            starImg.color = new Color(1f, 0.824f, 0.478f, 1f);
                        }
                    }

                    Transform textsT = rightInfo.Find("Texts");
                    if (textsT != null)
                    {
                        (textsT as RectTransform).sizeDelta = new Vector2(290f, 70f);
                    }

                    cardUI.starsText = rightInfo.Find("Texts/StarsText")?.GetComponent<TMP_Text>();
                    if (cardUI.starsText != null)
                    {
                        cardUI.starsText.fontSize = 32f;
                        cardUI.starsText.fontStyle = FontStyles.Bold;
                        cardUI.starsText.color = new Color(0.96f, 0.97f, 0.99f, 1f);
                    }

                    cardUI.starsLabelText = rightInfo.Find("Texts/Label")?.GetComponent<TMP_Text>();
                    if (cardUI.starsLabelText != null)
                    {
                        cardUI.starsLabelText.fontSize = 20f;
                        cardUI.starsLabelText.color = new Color(0.58f, 0.65f, 0.78f, 1f);
                    }
                }
            }

            // LockOverlay
            Transform lockOverlayT = contents.transform.Find("LockOverlay");
            if (lockOverlayT != null)
            {
                cardUI.lockOverlay = lockOverlayT.gameObject;
                cardUI.lockText = lockOverlayT.Find("LockText")?.GetComponent<TMP_Text>();
            }

            cardUI.button = btn;
            cardUI.canvasGroup = cg;

            // Lưu lại prefab
            PrefabUtility.SaveAsPrefabAsset(contents, PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(contents);
            Debug.Log(" - Đã cập nhật xong Prefab ActCard_Template!");
        }

        private static void FixSceneHierarchy()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform actSelectView = canvas.transform.Find("ActSelectView");
            if (actSelectView == null) return;

            Debug.Log("2. Đang sửa Scene Hierarchy...");

            // Xóa missing scripts trên toàn bộ scene
            Transform[] allObjects = actSelectView.GetComponentsInChildren<Transform>(true);
            int removedScene = 0;
            foreach (var t in allObjects)
            {
                removedScene += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
            Debug.Log($" - Đã xóa {removedScene} missing script(s) trong Scene.");

            Transform safeArea = actSelectView.Find("SafeAreaRoot");
            Transform actListScroll = safeArea != null ? safeArea.Find("ActListScroll") : null;
            Transform viewport = actListScroll != null ? actListScroll.Find("Viewport") : null;
            Transform content = viewport != null ? viewport.Find("Content") : null;

            // GIẢ THUYẾT 1: Thay Mask + Image alpha 0 trên Viewport bằng RectMask2D
            if (viewport != null)
            {
                Mask vMask = viewport.GetComponent<Mask>();
                if (vMask != null)
                {
                    Object.DestroyImmediate(vMask);
                    Debug.Log(" - Đã gỡ bỏ Mask khỏi Viewport.");
                }

                Image vImg = viewport.GetComponent<Image>();
                if (vImg != null)
                {
                    Object.DestroyImmediate(vImg);
                    Debug.Log(" - Đã gỡ bỏ Image alpha 0 khỏi Viewport.");
                }

                RectMask2D rm = viewport.GetComponent<RectMask2D>();
                if (rm == null)
                {
                    rm = viewport.gameObject.AddComponent<RectMask2D>();
                    Debug.Log(" - Đã thêm RectMask2D vào Viewport!");
                }
            }

            // ActListScroll: Xóa Image (để tránh nền xám) và cấu hình ScrollRect chuẩn
            if (actListScroll != null)
            {
                Image scrollImg = actListScroll.GetComponent<Image>();
                if (scrollImg != null)
                {
                    Object.DestroyImmediate(scrollImg);
                    Debug.Log(" - Đã gỡ bỏ Image khỏi ActListScroll.");
                }

                ScrollRect sr = actListScroll.GetComponent<ScrollRect>();
                if (sr != null)
                {
                    if (content != null) sr.content = content as RectTransform;
                    if (viewport != null) sr.viewport = viewport as RectTransform;
                    sr.movementType = ScrollRect.MovementType.Elastic;
                    sr.vertical = true;
                    sr.horizontal = false;
                    Debug.Log(" - Đã chuẩn hóa ScrollRect trên ActListScroll.");
                }
            }

            // Sửa BackBtn: thêm Shadow hợp lệ
            Transform backBtn = safeArea != null ? safeArea.Find("HeaderBar/BackBtn") : null;
            if (backBtn != null)
            {
                Shadow btnShadow = backBtn.GetComponent<Shadow>();
                if (btnShadow == null) btnShadow = backBtn.gameObject.AddComponent<Shadow>();
                btnShadow.effectColor = new Color(0f, 0f, 0f, 80f / 255f);
                btnShadow.effectDistance = new Vector2(0f, -4f);
            }

            // Sửa MainHeader: Đảm bảo font size 44, màu kem, không bị 0
            Transform mainHeaderT = safeArea != null ? safeArea.Find("HeaderBar/Titles/MainHeader") : null;
            if (mainHeaderT != null)
            {
                TMP_Text mainHeader = mainHeaderT.GetComponent<TMP_Text>();
                if (mainHeader != null)
                {
                    mainHeader.fontSize = 44f;
                    mainHeader.enableAutoSizing = false;
                    mainHeader.color = new Color(0.957f, 0.945f, 0.902f, 1f); // #F4F1E6 kem
                    mainHeader.text = "HÀNH TRÌNH TRUNG THU";
                    Debug.Log(" - Đã chuẩn hóa MainHeader font size 44 (màu kem).");
                }
            }

            // Sửa TotalStarsPill: text 22/45, spacing 16, padding 20
            Transform totalStarsPill = safeArea != null ? safeArea.Find("HeaderBar/TotalStarsPill") : null;
            if (totalStarsPill != null)
            {
                TMP_Text[] starTexts = totalStarsPill.GetComponentsInChildren<TMP_Text>(true);
                foreach (var st in starTexts)
                {
                    st.text = "22/45";
                    st.fontSize = 30f;
                    st.color = new Color(0.957f, 0.945f, 0.902f, 1f);
                    st.fontStyle = FontStyles.Bold;
                    st.textWrappingMode = TextWrappingModes.NoWrap;
                }
                HorizontalLayoutGroup hlg = totalStarsPill.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.spacing = 16f;
                    hlg.padding.left = 20;
                    hlg.padding.right = 20;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(totalStarsPill as RectTransform);
                Debug.Log(" - Đã chuẩn hóa TotalStarsPill (22/45, spacing 16px).");
            }

            // Đảm bảo nút Play (ActionChevron) trên các card cách lề phải 48px
            if (content != null)
            {
                foreach (Transform card in content)
                {
                    Transform chev = card.Find("BannerImage/ActionChevron");
                    if (chev != null)
                    {
                        RectTransform crt = chev.GetComponent<RectTransform>();
                        if (crt != null)
                        {
                            crt.anchorMin = new Vector2(1f, 0f);
                            crt.anchorMax = new Vector2(1f, 0f);
                            crt.pivot = new Vector2(1f, 0f);
                            crt.anchoredPosition = new Vector2(-48f, 20f);
                            crt.sizeDelta = new Vector2(56f, 56f);
                        }
                    }
                }
            }

            // Đảm bảo PreviewController kích hoạt và liên kết đúng
            Transform previewObj = actSelectView.Find("PreviewController");
            if (previewObj != null)
            {
                previewObj.gameObject.SetActive(true);
                ActSelectPreview preview = previewObj.GetComponent<ActSelectPreview>();
                if (preview != null && content != null)
                {
                    Transform c1 = content.Find("ActCard_1");
                    Transform c2 = content.Find("ActCard_2");
                    Transform c3 = content.Find("ActCard_3");
                    if (c1 != null) preview.actCard1 = c1.GetComponent<ActCardUI>();
                    if (c2 != null) preview.actCard2 = c2.GetComponent<ActCardUI>();
                    if (c3 != null) preview.actCard3 = c3.GetComponent<ActCardUI>();
                    if (backBtn != null) preview.backButton = backBtn.GetComponent<Button>();

                    preview.bannerAct1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_village_map_ngang.png");
                    preview.bannerAct2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_oldtown_map_ngang.png");
                    preview.bannerAct3 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Backgrounds/bg_moon_map_ngang.png");
                }
                Debug.Log(" - Đã kích hoạt và nối tham chiếu PreviewController (bao gồm 3 map ngang).");
            }

            Debug.Log(" - Đã chuẩn hóa Scene Hierarchy thành công!");
        }
    }
}
