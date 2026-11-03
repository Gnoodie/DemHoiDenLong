using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using DemHoiDenLong.UI;
using TMPro;
using DemHoiDenLong.Data;
using System.Collections.Generic;

namespace DemHoiDenLong.EditorTools
{
    public class ImpeccableMainMenuTool
    {
        // Danh sách các panel cha CŨ mà ta đã "rút ruột" (lấy con ra) trong quá trình setup.
        // Sau khi dựng xong toàn bộ layout, panel nào rỗng (không còn con) sẽ bị xoá
        // để tránh còn sót lại các khung/nền cũ đè lên UI mới (đây chính là nguyên nhân
        // gây ra khung xanh "HP/DMG" và khung nâu-xám nằm chồng lên nhau trong ảnh bạn gửi).
        private static readonly List<Transform> s_OrphanedParents = new List<Transform>();

        [MenuItem("Impeccable/Setup Main Menu (Multiplane Parallax)")]
        public static void SetupMainMenu()
        {
            s_OrphanedParents.Clear();

            // 1. Tìm Canvas_Hangar (Hỗ trợ cả trường hợp object đang bị tắt - inactive)
            HangarUI hangarUI = Object.FindAnyObjectByType<HangarUI>(FindObjectsInactive.Include);
            GameObject canvasObj = null;

            if (hangarUI != null)
            {
                // QUAN TRỌNG: hangarUI.gameObject có thể là một panel CON bên trong Canvas
                // (ví dụ "Hangar Panel"), KHÔNG phải Canvas gốc toàn màn hình.
                // Nếu dùng thẳng nó làm "canvasObj", mọi anchor % tính trong tool này
                // sẽ bị tính theo kích thước của panel con đó -> mọi phần tử bị nén dồn
                // vào 1 vùng nhỏ, chồng chéo lên nhau dù code không hề báo lỗi.
                // => Ta đi ngược lên tìm Canvas gốc thật sự chứa nó.
                Canvas rootCanvas = hangarUI.GetComponentInParent<Canvas>();
                if (rootCanvas != null)
                {
                    canvasObj = rootCanvas.gameObject;
                    if (canvasObj != hangarUI.gameObject)
                    {
                        Debug.Log($"[Impeccable] HangarUI nằm trên '{hangarUI.gameObject.name}', nhưng sẽ dùng Canvas gốc '{canvasObj.name}' làm khung tham chiếu chính.");
                    }
                }
                else
                {
                    canvasObj = hangarUI.gameObject;
                    Debug.LogWarning($"[Impeccable] Không tìm thấy Canvas cha nào của '{hangarUI.gameObject.name}'. Dùng tạm chính nó, layout có thể vẫn sai nếu nó không phải khung toàn màn hình.");
                }
            }
            else
            {
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject go in allObjects)
                {
                    if (go.name == "Canvas_Hangar" && go.scene.isLoaded)
                    {
                        canvasObj = go;
                        hangarUI = go.GetComponent<HangarUI>();
                        break;
                    }
                }
            }

            if (canvasObj == null || hangarUI == null)
            {
                Debug.LogError("[Impeccable] Không tìm thấy Canvas_Hangar hoặc component HangarUI trong Scene hiện tại!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(canvasObj, "Setup Impeccable Main Menu");

            // 2. Dọn dẹp UI cũ (List, ScrollRect, Scroll View)
            Transform listContainer = canvasObj.transform.Find("ListContainer");
            if (listContainer != null) Undo.DestroyObjectImmediate(listContainer.gameObject);

            Transform scrollRect = canvasObj.transform.Find("ScrollRect");
            if (scrollRect != null) Undo.DestroyObjectImmediate(scrollRect.gameObject);

            Transform scrollView = canvasObj.transform.Find("Scroll View");
            if (scrollView != null) Undo.DestroyObjectImmediate(scrollView.gameObject);

            // 3. Khởi tạo Parallax Layers (Background)
            Transform bgContainer = canvasObj.transform.Find("ParallaxBackgrounds");
            if (bgContainer == null)
            {
                GameObject bgObj = new GameObject("ParallaxBackgrounds", typeof(RectTransform));
                bgObj.transform.SetParent(canvasObj.transform, false);
                bgObj.transform.SetSiblingIndex(0);

                RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;

                CreateParallaxLayer(bgObj.transform, "Layer_0_Sky", ColorUtility.TryParseHtmlString("#0a1029", out Color skyCol) ? skyCol : Color.black);
                CreateParallaxLayer(bgObj.transform, "Layer_1_Stars", Color.white, true);
                CreateParallaxLayer(bgObj.transform, "Layer_2_Buildings", Color.white, true);
                CreateParallaxLayer(bgObj.transform, "Layer_3_Lanterns", Color.white, true);

                Debug.Log("[Impeccable] Đã tạo cấu trúc Multiplane Parallax.");
            }

            // 4. Khởi tạo Nút Tương Tác
            Button playBtn = CreateStyledButton(canvasObj.transform, "PlayButton", "PLAY", "#217d5e", new Vector2(0.5f, 0.06f), new Vector2(400, 120));
            Button prevBtn = CreateStyledButton(canvasObj.transform, "PrevLanButton", "<", "#e34233", new Vector2(0.08f, 0.58f), new Vector2(120, 120));
            Button nextBtn = CreateStyledButton(canvasObj.transform, "NextLanButton", ">", "#e34233", new Vector2(0.92f, 0.58f), new Vector2(120, 120));

            SerializedObject serializedHangarUI = new SerializedObject(hangarUI);
            serializedHangarUI.FindProperty("playBtn").objectReferenceValue = playBtn;
            serializedHangarUI.FindProperty("prevLanBtn").objectReferenceValue = prevBtn;
            serializedHangarUI.FindProperty("nextLanBtn").objectReferenceValue = nextBtn;

            // --- XÂY DỰNG LẠI TOÀN BỘ BỐ CỤC UI (PREMIUM NAVY & GOLD) ---
            
            Sprite roundedBox = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite circleKnob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            Transform detailsPanel = FindChildRecursive(canvasObj.transform, "DetailsPanel");
            if (detailsPanel != null)
            {
                foreach (var lg in detailsPanel.GetComponentsInChildren<LayoutGroup>(true)) { Undo.DestroyObjectImmediate(lg); }
                foreach (var fit in detailsPanel.GetComponentsInChildren<ContentSizeFitter>(true)) { Undo.DestroyObjectImmediate(fit); }
                SetRect(detailsPanel, new Vector2(0, 0), new Vector2(1, 1));
            }

            // 1. Tên & Subtitle (Có MOCKUP PREVIEW)
            TextMeshProUGUI lanNameText = (TextMeshProUGUI)serializedHangarUI.FindProperty("lanNameText").objectReferenceValue;
            if (lanNameText != null)
            {
                ReparentAndTrackOldParent(lanNameText.transform, canvasObj.transform);
                SetRect(lanNameText.transform, new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.98f));
                lanNameText.text = "LÂN ÁNH NGUYỆT";
                lanNameText.fontSize = 75;
                lanNameText.color = HexToColor("#FFD700");
                lanNameText.alignment = TextAlignmentOptions.Center;
                lanNameText.fontStyle = FontStyles.Bold;
            }
            TextMeshProUGUI storyTitleText = (TextMeshProUGUI)serializedHangarUI.FindProperty("storyTitleText").objectReferenceValue;
            if (storyTitleText != null)
            {
                ReparentAndTrackOldParent(storyTitleText.transform, canvasObj.transform);
                SetRect(storyTitleText.transform, new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.92f));
                storyTitleText.text = "Thần thú hộ giá dưới trăng rằm";
                storyTitleText.fontSize = 34;
                storyTitleText.color = HexToColor("#E0E0E0");
                storyTitleText.alignment = TextAlignmentOptions.Center;
                storyTitleText.fontStyle = FontStyles.Italic;
            }

            // Ẩn Chương truyện & Token
            TextMeshProUGUI storyChapterText = (TextMeshProUGUI)serializedHangarUI.FindProperty("storyChapterText")?.objectReferenceValue;
            if (storyChapterText != null) { storyChapterText.gameObject.SetActive(false); }
            TextMeshProUGUI storyTokenText = (TextMeshProUGUI)serializedHangarUI.FindProperty("storyTokenText")?.objectReferenceValue;
            if (storyTokenText != null) { storyTokenText.gameObject.SetActive(false); }

            // 2. Hình ảnh nhân vật + Viền + Glow
            Image lanIcon = (Image)serializedHangarUI.FindProperty("lanIconLarge").objectReferenceValue;
            if (lanIcon != null)
            {
                ReparentAndTrackOldParent(lanIcon.transform, canvasObj.transform);
                SetRect(lanIcon.transform, new Vector2(0.25f, 0.55f), new Vector2(0.75f, 0.85f));
                lanIcon.preserveAspect = true;

                Transform glow = canvasObj.transform.Find("SSR_Glow");
                if (glow == null)
                {
                    GameObject glowObj = new GameObject("SSR_Glow", typeof(RectTransform), typeof(Image));
                    glowObj.transform.SetParent(canvasObj.transform, false);
                    glowObj.transform.SetSiblingIndex(lanIcon.transform.GetSiblingIndex());
                    Image glowImg = glowObj.GetComponent<Image>();
                    glowImg.sprite = circleKnob; 
                    glowImg.color = HexToColor("#FFD70044"); 
                    glowImg.raycastTarget = false;
                    glow = glowObj.transform;
                }
                SetRect(glow, new Vector2(0.15f, 0.50f), new Vector2(0.85f, 0.90f));
            }

            // 3. Nút điều hướng < >
            if (prevBtn != null) SetRect(prevBtn.transform, new Vector2(0.04f, 0.65f), new Vector2(0.19f, 0.75f));
            if (nextBtn != null) SetRect(nextBtn.transform, new Vector2(0.81f, 0.65f), new Vector2(0.96f, 0.75f));

            // 4. Chỉ số và Nâng cấp
            Sprite upgradeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/normal_upgrade_btn.png");
            if (upgradeBtnSprite == null) upgradeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/upgrade.png");
            
            GameObject statsBg = CreateGlassPanel(canvasObj.transform, "StatsBG", new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.50f), roundedBox, HexToColor("#050A1FB2"));
            
            CreateUpgradeColumn(serializedHangarUI, statsBg.transform, "hpText", "1200", "upgradeHpBtn", "upgradeHpCostText", "150 G", "NÂNG HP", new Vector2(0.02f, 0.33f), upgradeBtnSprite, "#42F578");
            CreateUpgradeColumn(serializedHangarUI, statsBg.transform, "damageText", "250", "upgradeDmgBtn", "upgradeDmgCostText", "200 G", "NÂNG DMG", new Vector2(0.34f, 0.66f), upgradeBtnSprite, "#FF6B6B");
            CreateUpgradeColumn(serializedHangarUI, statsBg.transform, "fireRateText", "1.2s", "upgradeFrBtn", "upgradeFrCostText", "300 G", "TỐC BẮN", new Vector2(0.67f, 0.98f), upgradeBtnSprite, "#4DD0E1");

            // 5. Kỹ năng & Lời hứa
            GameObject loreBg = CreateGlassPanel(canvasObj.transform, "LoreBG", new Vector2(0.04f, 0.10f), new Vector2(0.96f, 0.25f), roundedBox, HexToColor("#050A1FB2"));

            TextMeshProUGUI skillDescText = (TextMeshProUGUI)serializedHangarUI.FindProperty("skillDescText").objectReferenceValue;
            if (skillDescText != null)
            {
                ReparentAndTrackOldParent(skillDescText.transform, loreBg.transform);
                SetRect(skillDescText.transform, new Vector2(0.02f, 0.50f), new Vector2(0.98f, 0.95f));
                skillDescText.text = "Kỹ năng: Nguyệt Quang Trấn Hộ\nTạo lá chắn vầng trăng bảo vệ đồng minh.";
                skillDescText.alignment = TextAlignmentOptions.Center;
                skillDescText.fontSize = 28;
                skillDescText.color = HexToColor("#AEE2FF");
                skillDescText.textWrappingMode = TextWrappingModes.Normal;
            }

            TextMeshProUGUI storyDescText = (TextMeshProUGUI)serializedHangarUI.FindProperty("storyDescriptionText").objectReferenceValue;
            if (storyDescText != null)
            {
                ReparentAndTrackOldParent(storyDescText.transform, loreBg.transform);
                SetRect(storyDescText.transform, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.45f));
                storyDescText.text = "Truyền thuyết: Sinh ra từ mảnh vỡ mặt trăng, mang sức mạnh xua đuổi tà ma.";
                storyDescText.alignment = TextAlignmentOptions.Center;
                storyDescText.fontSize = 26;
                storyDescText.fontStyle = FontStyles.Italic;
                storyDescText.color = HexToColor("#FADCA5");
                storyDescText.textWrappingMode = TextWrappingModes.Normal;
            }

            // 6. Nút Action (Play / Unlock)
            if (playBtn != null)
            {
                SetRect(playBtn.transform, new Vector2(0.20f, 0.01f), new Vector2(0.80f, 0.08f));
                playBtn.GetComponent<Image>().color = HexToColor("#1D8348");
                playBtn.GetComponent<Image>().sprite = roundedBox;
                playBtn.GetComponent<Image>().type = Image.Type.Sliced;
            }
            
            GameObject unlockPanelObj = (GameObject)serializedHangarUI.FindProperty("unlockPanel").objectReferenceValue;
            if (unlockPanelObj != null)
            {
                ReparentAndTrackOldParent(unlockPanelObj.transform, canvasObj.transform);
                SetRect(unlockPanelObj.transform, new Vector2(0.20f, 0.01f), new Vector2(0.80f, 0.08f));
                
                Button unlockBtn = (Button)serializedHangarUI.FindProperty("unlockBtn")?.objectReferenceValue;
                if (unlockBtn != null)
                {
                    ReparentAndTrackOldParent(unlockBtn.transform, unlockPanelObj.transform);
                    SetRect(unlockBtn.transform, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
                    Image img = unlockBtn.GetComponent<Image>();
                    if (img != null) { img.sprite = roundedBox; img.type = Image.Type.Sliced; img.color = HexToColor("#B8860B"); }
                }
                
                TextMeshProUGUI unlockCostText = (TextMeshProUGUI)serializedHangarUI.FindProperty("unlockCostText")?.objectReferenceValue;
                if (unlockCostText != null)
                {
                    ReparentAndTrackOldParent(unlockCostText.transform, unlockPanelObj.transform);
                    SetRect(unlockCostText.transform, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
                    unlockCostText.text = "MỞ KHÓA: 1000 G";
                    unlockCostText.alignment = TextAlignmentOptions.Center; 
                    unlockCostText.fontSize = 36; 
                    unlockCostText.color = Color.white;
                    unlockCostText.fontStyle = FontStyles.Bold;
                    unlockCostText.transform.SetAsLastSibling();
                }
            }

            AssignLanSpritesAutomatically();

            // Đưa các nút lên trên cùng
            if (lanIcon != null) lanIcon.transform.SetAsLastSibling();
            if (detailsPanel != null) detailsPanel.SetAsLastSibling();
            if (unlockPanelObj != null) unlockPanelObj.transform.SetAsLastSibling();
            playBtn.transform.SetAsLastSibling();
            prevBtn.transform.SetAsLastSibling();
            nextBtn.transform.SetAsLastSibling();
            if (lanNameText != null) lanNameText.transform.SetAsLastSibling();

            // --- DỌN RÁC: xoá các panel cha cũ đã bị "rút ruột" và giờ rỗng ---
            // Đây là bước xử lý nguyên nhân chính gây chồng chéo: các khung nền cũ
            // (panel HP/DMG cũ, khung ảnh cũ...) không còn con nào bên trong sẽ bị xoá.
            int removedCount = 0;
            foreach (Transform oldParent in s_OrphanedParents)
            {
                if (oldParent == null) continue;
                if (oldParent == canvasObj.transform || oldParent == detailsPanel) continue;
                if (oldParent.childCount == 0)
                {
                    Debug.Log($"[Impeccable] Xoá panel cũ rỗng còn sót lại: {GetPath(oldParent)}");
                    Undo.DestroyObjectImmediate(oldParent.gameObject);
                    removedCount++;
                }
                else
                {
                    // Vẫn còn con bên trong -> không tự ý xoá để tránh mất nội dung cần thiết,
                    // nhưng cảnh báo để bạn tự kiểm tra bằng mắt trong Hierarchy.
                    Debug.LogWarning($"[Impeccable] Panel cũ '{GetPath(oldParent)}' vẫn còn {oldParent.childCount} con, KHÔNG tự xoá. Hãy kiểm tra thủ công nếu nó vẫn đè lên UI mới.");
                }
            }

            serializedHangarUI.ApplyModifiedProperties();
            EditorUtility.SetDirty(hangarUI);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"[Impeccable] Hoàn tất thiết lập. Đã dọn {removedCount} panel rác, giãn lại toàn bộ vùng anchor để không còn chồng chéo.");
        }

        /// <summary>
        /// Reparent 1 phần tử sang canvas, đồng thời ghi nhớ parent cũ của nó để
        /// kiểm tra dọn dẹp (xoá nếu rỗng) ở cuối quá trình setup.
        /// </summary>
        private static void ReparentAndTrackOldParent(Transform child, Transform newParent)
        {
            Transform oldParent = child.parent;
            child.SetParent(newParent, true);
            if (oldParent != null && oldParent != newParent)
            {
                s_OrphanedParents.Add(oldParent);
            }
        }

        private static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private static void SetRect(Transform t, Vector2 min, Vector2 max)
        {
            RectTransform rt = t.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c)) return c;
            return Color.white;
        }

        private static void AssignLanSpritesAutomatically()
        {
            string[] guids = AssetDatabase.FindAssets("t:LanConfig", new[] { "Assets/_Project/Data/Lans" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LanConfig config = AssetDatabase.LoadAssetAtPath<LanConfig>(path);
                if (config != null && config.LanIcon == null)
                {
                    string spriteName = config.LanName.ToLower().Replace("lân", "lan").Replace(" ", "").Replace("á", "a").Replace("ý", "y").Replace("ệ", "e");
                    if (config.LanName.Contains("Nguyệt") || config.LanName.Contains("Vàng")) spriteName = "lanvang";
                    else if (config.LanName.Contains("Thủy")) spriteName = "lanthuy";
                    else if (config.LanName.Contains("Hỏa")) spriteName = "lanhoa";
                    else if (config.LanName.Contains("Kim")) spriteName = "lankim";

                    string[] spriteGuids = AssetDatabase.FindAssets(spriteName + " t:Sprite", new[] { "Assets/_Project/Sprites/Characters" });
                    if (spriteGuids.Length > 0)
                    {
                        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteGuids[0]));
                        config.LanIcon = spr;
                        EditorUtility.SetDirty(config);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static void CreateParallaxLayer(Transform parent, string name, Color color, bool transparent = false)
        {
            GameObject layer = new GameObject(name, typeof(RectTransform), typeof(Image));
            layer.transform.SetParent(parent, false);

            RectTransform rect = layer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = layer.GetComponent<Image>();
            img.raycastTarget = false;
            if (transparent)
            {
                color.a = 0f;
            }
            img.color = color;
        }

        private static GameObject CreateGlassPanel(Transform parent, string name, Vector2 min, Vector2 max, Sprite bgSprite, Color color)
        {
            Transform existing = parent.Find(name);
            if (existing != null) { Undo.DestroyObjectImmediate(existing.gameObject); }
            
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            SetRect(panel.transform, min, max);
            Image img = panel.GetComponent<Image>();
            img.sprite = bgSprite;
            img.type = bgSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            img.raycastTarget = false;
            return panel;
        }

        private static void CreateUpgradeColumn(SerializedObject serializedHangarUI, Transform parent, string valProp, string valMock, string btnProp, string costProp, string costMock, string labelStr, Vector2 xRange, Sprite btnSprite, string valColorHex)
        {
            TextMeshProUGUI valText = (TextMeshProUGUI)serializedHangarUI.FindProperty(valProp)?.objectReferenceValue;
            if (valText != null)
            {
                ReparentAndTrackOldParent(valText.transform, parent);
                SetRect(valText.transform, new Vector2(xRange.x, 0.70f), new Vector2(xRange.y, 1.00f));
                valText.text = valMock;
                valText.alignment = TextAlignmentOptions.Center; 
                valText.fontSize = 32; 
                valText.color = HexToColor(valColorHex);
                valText.fontStyle = FontStyles.Bold;
            }

            Transform labelTransform = parent.Find("Label_" + btnProp);
            if (labelTransform == null)
            {
                GameObject labelObj = new GameObject("Label_" + btnProp, typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(parent, false);
                labelTransform = labelObj.transform;
            }
            SetRect(labelTransform, new Vector2(xRange.x, 0.50f), new Vector2(xRange.y, 0.70f));
            TextMeshProUGUI labelTmp = labelTransform.GetComponent<TextMeshProUGUI>();
            if (labelTmp == null) labelTmp = labelTransform.gameObject.AddComponent<TextMeshProUGUI>();
            labelTmp.text = labelStr;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.fontSize = 24;
            labelTmp.color = HexToColor("#E0E0E0");
            labelTmp.fontStyle = FontStyles.Bold;

            Button upBtn = (Button)serializedHangarUI.FindProperty(btnProp)?.objectReferenceValue;
            if (upBtn != null)
            {
                ReparentAndTrackOldParent(upBtn.transform, parent);
                float midX = (xRange.x + xRange.y) / 2f;
                SetRect(upBtn.transform, new Vector2(midX - 0.12f, 0.20f), new Vector2(midX + 0.12f, 0.45f));
                upBtn.gameObject.SetActive(true);
                if (btnSprite != null)
                {
                    Image img = upBtn.GetComponent<Image>();
                    if (img != null) img.sprite = btnSprite;
                }
            }

            TextMeshProUGUI costText = (TextMeshProUGUI)serializedHangarUI.FindProperty(costProp)?.objectReferenceValue;
            if (costText != null)
            {
                ReparentAndTrackOldParent(costText.transform, parent);
                SetRect(costText.transform, new Vector2(xRange.x, 0.00f), new Vector2(xRange.y, 0.20f));
                costText.text = costMock;
                costText.alignment = TextAlignmentOptions.Center; 
                costText.fontSize = 26; 
                costText.color = HexToColor("#FFD700");
                costText.fontStyle = FontStyles.Bold;
            }
        }

        private static Button CreateStyledButton(Transform parent, string name, string text, string hexColor, Vector2 anchor, Vector2 size)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();

            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                img.color = color;
            }

            Button btn = btnObj.GetComponent<Button>();

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 40;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }
    }
}