using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    [InitializeOnLoad]
    public static class DiagnoseGameplayScene
    {
        static DiagnoseGameplayScene()
        {
            EditorApplication.update += RunDiagnosisOnce;
        }

        [InitializeOnLoadMethod]
        private static void OnInit()
        {
            EditorApplication.update += RunDiagnosisOnce;
        }

        private static bool hasExecuted = false;

        private static void RunDiagnosisOnce()
        {
            if (hasExecuted) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            hasExecuted = true;
            EditorApplication.update -= RunDiagnosisOnce;

            try
            {
                RunDiagnosis();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DiagnoseGameplayScene] Exception during RunDiagnosis: {ex}");
            }
        }

        [MenuItem("DemHoiDenLong/Diagnose and Fix Gameplay Scene")]
        public static void RunDiagnosis()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=================================================================");
            sb.AppendLine("=== BƯỚC 1: ĐO TRƯỚC, KHÔNG ĐOÁN - DIAGNOSIS REPORT ===");
            sb.AppendLine("=================================================================");

            // --- 1. ENTER PLAY MODE SETTINGS ---
            sb.AppendLine("\n--- [1.E] ENTER PLAY MODE SETTINGS ---");
            bool enterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            EnterPlayModeOptions options = EditorSettings.enterPlayModeOptions;
            bool reloadDomain = !enterPlayModeOptionsEnabled || (options & EnterPlayModeOptions.DisableDomainReload) == 0;
            bool reloadScene = !enterPlayModeOptionsEnabled || (options & EnterPlayModeOptions.DisableSceneReload) == 0;
            sb.AppendLine($"EnterPlayModeOptionsEnabled: {enterPlayModeOptionsEnabled}");
            sb.AppendLine($"EnterPlayModeOptions: {options}");
            sb.AppendLine($"Reload Domain: {(reloadDomain ? "BẬT (ON)" : "TẮT (OFF - Static fields persist!)")}");
            sb.AppendLine($"Reload Scene: {(reloadScene ? "BẬT (ON)" : "TẮT (OFF)")}");

            // --- 2. STATIC FIELDS & EVENTS IN GAMEPLAY ---
            sb.AppendLine("\n--- [1.E] STATIC FIELDS & EVENTS TRONG GAMEPLAY & UI ---");
            Type[] types = new Type[]
            {
                typeof(LevelSession),
                typeof(WaveManager),
                typeof(EnemyBase),
                typeof(PlayerController),
                typeof(PlayerSkillController),
                typeof(GameManager),
                typeof(GameplayBalance),
                typeof(GameplayHUD),
                typeof(LevelSelectContext),
                typeof(BulletPool),
                typeof(EnemySpawner)
            };

            foreach (var t in types)
            {
                var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var events = t.GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (fields.Length > 0 || events.Length > 0)
                {
                    sb.AppendLine($"Class: {t.FullName}");
                    foreach (var f in fields)
                    {
                        object val = null;
                        try { val = f.GetValue(null); } catch { val = "<error>"; }
                        sb.AppendLine($"  - Static Field: {f.Name} (Type: {f.FieldType.Name}, Value: {val})");
                    }
                    foreach (var ev in events)
                    {
                        sb.AppendLine($"  - Static Event: {ev.Name} (Type: {ev.EventHandlerType.Name})");
                    }
                }
            }

            // --- 3. ALL TMP OBJECTS IN GAMEPLAY SCENE ---
            sb.AppendLine("\n--- [1.D] LIỆT KÊ MỌI TMP OBJECT TRONG SCENE GAMEPLAY ---");
            var allTmps = Resources.FindObjectsOfTypeAll<TMP_Text>();
            int tmpCount = 0;
            foreach (var tmp in allTmps)
            {
                if (tmp.gameObject.scene != scene) continue; // Only scene objects
                tmpCount++;
                string path = GetHierarchyPath(tmp.transform);
                sb.AppendLine($"TMP #{tmpCount}: \"{tmp.name}\" | Path: {path} | Active: {tmp.gameObject.activeInHierarchy} | InstanceID: {tmp.GetEntityId()} | Text: \"{tmp.text}\" | Size: {tmp.fontSize}");
            }

            // --- 4. GAMEPLAY HUD COMPONENT & SERIALIZED REFERENCES ---
            sb.AppendLine("\n--- [1.C] GAMEPLAY HUD BINDINGS & REFERENCES (TRƯỚC KHI SỬA) ---");
            GameplayHUD hud = UnityEngine.Object.FindAnyObjectByType<GameplayHUD>();
            if (hud == null)
            {
                sb.AppendLine("ERROR: GameplayHUD NOT FOUND in scene!");
            }
            else
            {
                SerializedObject so = new SerializedObject(hud);
                SerializedProperty prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        string targetName = prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "null";
                        string targetPath = "null";
                        string instId = "0";
                        if (prop.objectReferenceValue is Component c)
                        {
                            targetPath = GetHierarchyPath(c.transform);
                            instId = c.GetEntityId().ToString();
                        }
                        else if (prop.objectReferenceValue is GameObject go)
                        {
                            targetPath = GetHierarchyPath(go.transform);
                            instId = go.GetEntityId().ToString();
                        }
                        sb.AppendLine($"  HUD Field: {prop.name} -> {targetName} (InstanceID: {instId}, Path: {targetPath})");
                    }
                }
            }

            // =========================================================================
            // BƯỚC 3: SỬA LAYOUT SCENE THEO YÊU CẦU
            // =========================================================================
            sb.AppendLine("\n=================================================================");
            sb.AppendLine("=== BƯỚC 3: THỰC HIỆN SỬA GIAO DIỆN SCENE GAMEPLAY ===");
            sb.AppendLine("=================================================================");

            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas != null && hud != null)
            {
                Transform canvasT = canvas.transform;

                // 1. Sửa SettingsButton & SettingsUI trùng lặp
                var allButtons = canvasT.GetComponentsInChildren<Button>(true);
                List<GameObject> toDestroy = new List<GameObject>();
                GameObject keptSettingsBtn = null;

                foreach (var btn in allButtons)
                {
                    if (btn.name == "SettingsButton")
                    {
                        RectTransform rt = btn.GetComponent<RectTransform>();
                        if (rt != null && rt.sizeDelta.x > 100f) // Bản cũ 150x150
                        {
                            sb.AppendLine($"[BƯỚC 3.1] Xóa SettingsButton trùng 150x150: {GetHierarchyPath(btn.transform)}");
                            toDestroy.Add(btn.gameObject);
                        }
                        else
                        {
                            keptSettingsBtn = btn.gameObject;
                        }
                    }
                }

                // Xóa SettingsUI trùng
                var allSettingsUI = new List<Transform>();
                for (int i = 0; i < canvasT.childCount; i++)
                {
                    var child = canvasT.GetChild(i);
                    if (child.name == "SettingsUI") allSettingsUI.Add(child);
                }
                if (allSettingsUI.Count > 1)
                {
                    for (int i = 1; i < allSettingsUI.Count; i++)
                    {
                        sb.AppendLine($"[BƯỚC 3.1] Xóa SettingsUI trùng #{i}: {GetHierarchyPath(allSettingsUI[i])}");
                        toDestroy.Add(allSettingsUI[i].gameObject);
                    }
                }

                foreach (var go in toDestroy)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }

                // Đảm bảo SettingsButton chuẩn ở (-30, -30) size 64x64
                if (keptSettingsBtn != null)
                {
                    RectTransform rtSet = keptSettingsBtn.GetComponent<RectTransform>();
                    rtSet.anchorMin = new Vector2(1f, 1f);
                    rtSet.anchorMax = new Vector2(1f, 1f);
                    rtSet.pivot = new Vector2(1f, 1f);
                    rtSet.anchoredPosition = new Vector2(-30f, -30f);
                    rtSet.sizeDelta = new Vector2(64f, 64f);
                    sb.AppendLine($"[BƯỚC 3.1] Giữ SettingsButton chuẩn: pos=(-30, -30), size=(64, 64)");
                }

                // 2. Dời StarsChip sang bên trái SettingsButton (không chồng với nút cài đặt)
                Transform starChipT = canvasT.Find("StarsChip");
                if (starChipT != null)
                {
                    RectTransform rtStar = starChipT.GetComponent<RectTransform>();
                    rtStar.anchorMin = new Vector2(1f, 1f);
                    rtStar.anchorMax = new Vector2(1f, 1f);
                    rtStar.pivot = new Vector2(1f, 1f);
                    // Đặt tại X = -114 (cách SettingsButton mép trái -94 thêm 20px padding), Y = -40
                    rtStar.anchoredPosition = new Vector2(-114f, -40f);
                    rtStar.sizeDelta = new Vector2(130f, 44f);
                    sb.AppendLine($"[BƯỚC 3.1] Dời StarsChip ra ngoài nút cài đặt: pos=(-114, -40), size=(130, 44) bên trái nút cài đặt");
                }

                // 3. Sửa chân dung Lân: Chuyển SkillIcon (Kim Lân) ra góc phải dưới làm nút skill, chỉ giữ 1 ảnh Lân trong LanPortrait góc trái
                Transform statsP = canvasT.Find("PlayerStatsPanel");
                Transform portraitT = canvasT.Find("LanPortrait");
                if (portraitT == null && statsP != null) portraitT = statsP.Find("LanPortrait");

                Transform skillIconT = null;
                if (statsP != null) skillIconT = statsP.Find("SkillIcon");
                if (skillIconT == null) skillIconT = canvasT.Find("SkillIcon");

                if (skillIconT != null)
                {
                    // Di chuyển SkillIcon ra góc phải dưới làm indicator cooldown / skill button
                    skillIconT.SetParent(canvasT, false);
                    RectTransform rtSkill = skillIconT.GetComponent<RectTransform>();
                    rtSkill.anchorMin = new Vector2(1f, 0f);
                    rtSkill.anchorMax = new Vector2(1f, 0f);
                    rtSkill.pivot = new Vector2(1f, 0f);
                    rtSkill.anchoredPosition = new Vector2(-40f, 160f);
                    rtSkill.sizeDelta = new Vector2(96f, 96f);
                    sb.AppendLine($"[BƯỚC 3.2] Di chuyển SkillIcon ra góc phải dưới: pos=(-40, 160), size=(96, 96)");
                }

                if (portraitT != null)
                {
                    portraitT.SetParent(canvasT, false);
                    RectTransform rtPort = portraitT.GetComponent<RectTransform>();
                    rtPort.anchorMin = new Vector2(0f, 1f);
                    rtPort.anchorMax = new Vector2(0f, 1f);
                    rtPort.pivot = new Vector2(0f, 1f);
                    rtPort.anchoredPosition = new Vector2(30f, -30f);
                    rtPort.sizeDelta = new Vector2(76f, 76f);

                    Image imgPort = portraitT.GetComponent<Image>();
                    if (imgPort != null)
                    {
                        imgPort.preserveAspect = true;
                    }
                    sb.AppendLine($"[BƯỚC 3.2] Đảm bảo LanPortrait duy nhất ở góc trái: pos=(30, -30), size=(76, 76), preserveAspect=true");
                }

                // 4. Thanh máu HPSlider không bị che bởi chân dung (bắt đầu tại X=120)
                Slider slider = canvasT.GetComponentInChildren<Slider>(true);
                if (slider != null)
                {
                    RectTransform rtSlider = slider.GetComponent<RectTransform>();
                    rtSlider.anchorMin = new Vector2(0f, 1f);
                    rtSlider.anchorMax = new Vector2(0f, 1f);
                    rtSlider.pivot = new Vector2(0f, 1f);
                    rtSlider.anchoredPosition = new Vector2(120f, -36f);
                    rtSlider.sizeDelta = new Vector2(420f, 34f);

                    TextMeshProUGUI tmpHp = slider.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmpHp != null)
                    {
                        tmpHp.alignment = TextAlignmentOptions.Center;
                        tmpHp.textWrappingMode = TextWrappingModes.NoWrap;
                        tmpHp.fontSize = 22;
                    }
                    sb.AppendLine($"[BƯỚC 3.3] HPSlider đặt cạnh chân dung: pos=(120, -36), size=(420, 34), chữ máu NoWrap căn giữa");
                }

                // 5. Wire lại toàn bộ serialized fields của GameplayHUD
                SerializedObject soHud = new SerializedObject(hud);
                                if (slider != null) soHud.FindProperty("hpSlider").objectReferenceValue = slider;
                if (slider != null) soHud.FindProperty("hpText").objectReferenceValue = slider.GetComponentInChildren<TextMeshProUGUI>(true);
                
                Transform livesT = canvasT.Find("LivesContainer");
                if (livesT == null && statsP != null) livesT = statsP.Find("LivesContainer");
                if (livesT != null)
                {
                    livesT.SetParent(canvasT, false);
                    RectTransform rtLives = livesT.GetComponent<RectTransform>();
                    rtLives.anchorMin = new Vector2(0f, 1f);
                    rtLives.anchorMax = new Vector2(0f, 1f);
                    rtLives.pivot = new Vector2(0f, 1f);
                    rtLives.anchoredPosition = new Vector2(120f, -78f);
                    soHud.FindProperty("livesContainer").objectReferenceValue = rtLives;
                }

                if (skillIconT != null)
                {
                    soHud.FindProperty("skillIcon").objectReferenceValue = skillIconT.GetComponent<Image>();
                    Transform cdOverlay = skillIconT.Find("CooldownOverlay");
                    if (cdOverlay != null) soHud.FindProperty("skillCooldownOverlay").objectReferenceValue = cdOverlay.GetComponent<Image>();
                }

                Transform objChipT = canvasT.Find("ObjectiveChip");
                if (objChipT != null)
                {
                    Transform txt = objChipT.Find("Text");
                    if (txt != null) soHud.FindProperty("objectiveText").objectReferenceValue = txt.GetComponent<TextMeshProUGUI>();
                }

                if (starChipT != null)
                {
                    Transform txt = starChipT.Find("Text");
                    if (txt != null) soHud.FindProperty("runStarsText").objectReferenceValue = txt.GetComponent<TextMeshProUGUI>();
                }

                Sprite starSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/star.png");
                if (starSpr != null) soHud.FindProperty("starSprite").objectReferenceValue = starSpr;

                soHud.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                sb.AppendLine("[BƯỚC 3] Đã lưu scene Gameplay.unity thành công!");
            }

            // =========================================================================
            // CHẠY SELF TEST 26/26
            // =========================================================================
            sb.AppendLine("\n=================================================================");
            sb.AppendLine("=== CHẠY LEVEL PROGRESS SELF TEST ===");
            sb.AppendLine("=================================================================");
            try
            {
                LevelProgressSelfTest.RunAllTests();
                sb.AppendLine("Self Test: Đã gọi LevelProgressSelfTest.RunAllTests() thành công!");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Self Test LỖI: {ex}");
            }

            sb.AppendLine("=================================================================");
            string output = sb.ToString();
            Debug.Log(output);
            System.IO.File.WriteAllText("D:/Dev_Game/DemHoiDenLong/Logs/diagnose_step1.txt", output, Encoding.UTF8);
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "";
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
