using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class ApplySkillUIBindings
    {
        [MenuItem("DemHoiDenLong/Setup Skill UI Bindings")]
        public static void Apply()
        {
            SetupSkillData.SetupAll();

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ApplySkillUIBindings] Canvas not found in Gameplay scene!");
                return;
            }

            Transform safeRoot = canvas.transform.Find("SafeAreaRoot");
            if (safeRoot == null) safeRoot = canvas.transform;

            // Load UI Sprites
            Sprite uiCircle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_circle.png");
            Sprite uiGlowRing = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/ui_glow_ring.png");
            Sprite vfxRing = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/VFX/vfx_ring.png");
            if (uiGlowRing == null) uiGlowRing = vfxRing;

            // Remove old orphan SkillIcon if it exists directly under safeRoot
            Transform oldIcon = safeRoot.Find("SkillIcon");
            if (oldIcon != null)
            {
                Object.DestroyImmediate(oldIcon.gameObject);
            }

            // 1. SkillButton Container (132x132, anchor bottom-right, pos -36, 36)
            Transform existingBtnT = safeRoot.Find("SkillButton");
            GameObject btnObj = existingBtnT != null ? existingBtnT.gameObject : new GameObject("SkillButton", typeof(RectTransform), typeof(Button));
            btnObj.transform.SetParent(safeRoot, false);

            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1f, 0f);
            btnRt.anchorMax = new Vector2(1f, 0f);
            btnRt.pivot = new Vector2(1f, 0f);
            btnRt.anchoredPosition = new Vector2(-36f, 36f);
            btnRt.sizeDelta = new Vector2(132f, 132f);

            // Child 1: SkillBg
            Transform bgT = btnObj.transform.Find("SkillBg");
            GameObject bgObj = bgT != null ? bgT.gameObject : new GameObject("SkillBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(btnObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = uiCircle;
            bgImg.type = Image.Type.Simple;
            bgImg.color = new Color32(0x16, 0x22, 0x4A, 0xFF);
            bgImg.raycastTarget = true;

            // Child 2: BorderRing
            Transform ringT = btnObj.transform.Find("BorderRing");
            GameObject ringObj = ringT != null ? ringT.gameObject : new GameObject("BorderRing", typeof(RectTransform), typeof(Image));
            ringObj.transform.SetParent(btnObj.transform, false);
            RectTransform ringRt = ringObj.GetComponent<RectTransform>();
            ringRt.anchorMin = Vector2.zero;
            ringRt.anchorMax = Vector2.one;
            ringRt.sizeDelta = Vector2.zero;
            Image ringImg = ringObj.GetComponent<Image>();
            ringImg.sprite = uiGlowRing != null ? uiGlowRing : uiCircle;
            ringImg.type = Image.Type.Simple;
            ringImg.color = new Color32(0xE6, 0xF0, 0xFF, 0xFF); // Kim default
            ringImg.raycastTarget = false;

            // Child 3: SkillIcon
            Transform iconT = btnObj.transform.Find("SkillIcon");
            GameObject iconObj = iconT != null ? iconT.gameObject : new GameObject("SkillIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(96f, 96f);
            Image iconImg = iconObj.GetComponent<Image>();
            Sprite kimIcon = Resources.Load<Sprite>("Icons/icon_skill_kim");
            if (kimIcon == null) kimIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Lân Kim/topview_lankim.png");
            iconImg.sprite = kimIcon;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // Child 4: CooldownOverlay
            Transform cdT = btnObj.transform.Find("CooldownOverlay");
            GameObject cdObj = cdT != null ? cdT.gameObject : new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
            cdObj.transform.SetParent(btnObj.transform, false);
            RectTransform cdRt = cdObj.GetComponent<RectTransform>();
            cdRt.anchorMin = Vector2.zero;
            cdRt.anchorMax = Vector2.one;
            cdRt.sizeDelta = Vector2.zero;
            Image cdImg = cdObj.GetComponent<Image>();
            cdImg.sprite = uiCircle;
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = (int)Image.Origin360.Top;
            cdImg.fillClockwise = false;
            cdImg.fillAmount = 0f;
            cdImg.color = new Color(0f, 0f, 0f, 0.65f);
            cdImg.raycastTarget = false;
            cdObj.SetActive(false);

            // Child 5: CooldownText
            Transform textT = btnObj.transform.Find("CooldownText");
            GameObject textObj = textT != null ? textT.gameObject : new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            textObj.transform.SetAsLastSibling(); // Luôn trên cùng để không bị che bởi CooldownOverlay
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.anchoredPosition = Vector2.zero;
            TextMeshProUGUI cdText = textObj.GetComponent<TextMeshProUGUI>();
            cdText.fontSize = 44f;
            cdText.fontStyle = FontStyles.Bold;
            cdText.alignment = TextAlignmentOptions.Center;
            cdText.color = Color.white;
            if (cdText.font == null) cdText.font = TMPro.TMP_Settings.defaultFontAsset;
            cdText.text = "";
            cdText.raycastTarget = false;
            textObj.SetActive(false);

            // Button Component
            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.transition = Selectable.Transition.None;

            // 2. SkillBanner (Centered TMP 48 Bold, fade out 0.7s)
            Transform bannerT = safeRoot.Find("SkillBanner");
            if (bannerT == null) bannerT = canvas.transform.Find("SkillBanner");
            GameObject bannerObj = bannerT != null ? bannerT.gameObject : new GameObject("SkillBanner", typeof(RectTransform), typeof(CanvasGroup));
            bannerObj.transform.SetParent(safeRoot, false);
            RectTransform bannerRt = bannerObj.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1f);
            bannerRt.anchorMax = new Vector2(0.5f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0f, -172f);
            bannerRt.sizeDelta = new Vector2(400f, 52f);
            CanvasGroup cg = bannerObj.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            Transform bannerTextT = bannerObj.transform.Find("Text");
            GameObject bannerTextObj = bannerTextT != null ? bannerTextT.gameObject : new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            bannerTextObj.transform.SetParent(bannerObj.transform, false);
            RectTransform btRt = bannerTextObj.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI bannerTMP = bannerTextObj.GetComponent<TextMeshProUGUI>();
            bannerTMP.fontSize = 36f;
            bannerTMP.fontStyle = FontStyles.Bold;
            bannerTMP.alignment = TextAlignmentOptions.Center;
            bannerTMP.color = Color.white;
            bannerTMP.text = "";
            bannerTMP.raycastTarget = false;
            bannerObj.SetActive(false);

            // 3. ScreenBorderFlash Image
            Transform flashT = canvas.transform.Find("ScreenBorderFlash");
            GameObject flashObj = flashT != null ? flashT.gameObject : new GameObject("ScreenBorderFlash", typeof(RectTransform), typeof(Image));
            flashObj.transform.SetParent(canvas.transform, false);
            flashObj.transform.SetSiblingIndex(0); // Under HUD panels
            RectTransform flashRt = flashObj.GetComponent<RectTransform>();
            flashRt.anchorMin = Vector2.zero;
            flashRt.anchorMax = Vector2.one;
            flashRt.offsetMin = Vector2.zero;
            flashRt.offsetMax = Vector2.zero;
            Image flashImg = flashObj.GetComponent<Image>();
            flashImg.color = new Color(1f, 1f, 1f, 0f);
            flashImg.raycastTarget = false;
            flashObj.SetActive(false);

            // 4. Wire up to GameplayHUD
            GameplayHUD hud = canvas.GetComponentInChildren<GameplayHUD>(true);
            if (hud != null)
            {
                SerializedObject so = new SerializedObject(hud);
                so.FindProperty("skillButton").objectReferenceValue = btn;
                so.FindProperty("skillBgImage").objectReferenceValue = bgImg;
                so.FindProperty("skillBorderRing").objectReferenceValue = ringImg;
                so.FindProperty("skillIcon").objectReferenceValue = iconImg;
                so.FindProperty("skillCooldownOverlay").objectReferenceValue = cdImg;
                so.FindProperty("skillCooldownText").objectReferenceValue = cdText;
                so.FindProperty("skillBannerGroup").objectReferenceValue = cg;
                so.FindProperty("skillBannerText").objectReferenceValue = bannerTMP;
                so.FindProperty("screenBorderFlashImage").objectReferenceValue = flashImg;
                so.ApplyModifiedPropertiesWithoutUndo();

                while (btn.onClick.GetPersistentEventCount() > 0)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                }
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, hud.OnSkillButtonClicked);
                EditorUtility.SetDirty(hud);
            }

            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ApplySkillUIBindings] Successfully built & wired Phase 5A.4 Skill UI in Gameplay.unity!");
        }
    }
}
