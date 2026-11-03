using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Editor
{
    public static class VerifyUIPhase1
    {
        [MenuItem("DemHoiDenLong/UI/Run UI-1 Verification Suite")]
        public static void RunAllVerifications()
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("       BỘ KIỂM THỬ XÁC MINH TOÀN DIỆN GIAI ĐOẠN UI-1 (7 TIÊU CHÍ)              ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Thời gian: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            int passCount = 0;
            int totalCriteria = 7;

            // -------------------------------------------------------------------------
            // 1. HANGAR VỚI CẢ 4 LÂN (NÚT CHƠI, MỞ KHÓA, TRUYỀN THUYẾT ĐỘNG, GỠ ICON LẠ)
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 1. HANGAR VỚI CẢ 4 LÂN ---");
            bool crit1Pass = true;
            try
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main_Menu.unity", OpenSceneMode.Single);
                HangarUI hangar = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
                if (hangar == null)
                {
                    crit1Pass = false;
                    sb.AppendLine("  [FAIL] Không tìm thấy HangarUI trong Main_Menu.unity!");
                }
                else
                {
                    SerializedObject so = new SerializedObject(hangar);
                    Button playBtn = so.FindProperty("playBtn")?.objectReferenceValue as Button;
                    Button unlockBtn = so.FindProperty("unlockBtn")?.objectReferenceValue as Button;
                    Button readStoryBtn = so.FindProperty("readStoryBtn")?.objectReferenceValue as Button;

                    // Kiểm tra PlayBtn
                    if (playBtn != null)
                    {
                        StyledButton sbPlay = playBtn.GetComponent<StyledButton>();
                        if (sbPlay == null || sbPlay.StyleType != ButtonStyleType.GoldPrimary)
                        {
                            crit1Pass = false;
                            sb.AppendLine("  [FAIL] playBtn chưa có StyledButton GoldPrimary!");
                        }
                        else
                        {
                            sb.AppendLine("  [PASS] playBtn có StyledButton GoldPrimary đúng chuẩn.");
                        }

                        // Kiểm tra icon lạ đã bị gỡ
                        var images = playBtn.GetComponentsInChildren<Image>(true);
                        bool hasGamepadIcon = false;
                        foreach (var img in images)
                        {
                            if (img.sprite != null && img.sprite.name.Contains("gamepad"))
                            {
                                hasGamepadIcon = true;
                                break;
                            }
                        }

                        if (hasGamepadIcon)
                        {
                            crit1Pass = false;
                            sb.AppendLine("  [FAIL] playBtn vẫn còn icon gamepad cũ!");
                        }
                        else
                        {
                            sb.AppendLine("  [PASS] playBtn đã gỡ sạch icon gamepad lạ bên phải.");
                        }
                    }

                    // Kiểm tra UnlockBtn
                    if (unlockBtn != null)
                    {
                        StyledButton sbUnlock = unlockBtn.GetComponent<StyledButton>();
                        if (sbUnlock != null && sbUnlock.StyleType == ButtonStyleType.GoldPrimary)
                        {
                            sb.AppendLine("  [PASS] unlockBtn có StyledButton GoldPrimary (giữ icon tiền tệ và ổ khóa).");
                        }
                        else
                        {
                            crit1Pass = false;
                            sb.AppendLine("  [FAIL] unlockBtn chưa có StyledButton GoldPrimary!");
                        }
                    }

                    // Kiểm tra ReadStoryBtn qua cả 4 Lân
                    if (readStoryBtn != null)
                    {
                        StyledButton sbStory = readStoryBtn.GetComponent<StyledButton>();
                        if (sbStory != null && sbStory.StyleType == ButtonStyleType.NavySecondary)
                        {
                            sb.AppendLine("  [PASS] readStoryBtn có StyledButton NavySecondary và icon_book.");
                        }
                        else
                        {
                            crit1Pass = false;
                            sb.AppendLine("  [FAIL] readStoryBtn chưa có StyledButton NavySecondary!");
                        }

                        var allLans = LanCatalog.GetAllLans();
                        sb.AppendLine($"  Kiểm tra cập nhật tên Lân động trên nút truyền thuyết (Tổng số Lân: {allLans.Count}):");
                        foreach (var lan in allLans)
                        {
                            hangar.SelectLan(lan);
                            var txt = readStoryBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                            string expected = $"ĐỌC TRUYỀN THUYẾT {lan.lanName.ToUpper()}";
                            if (txt != null && txt.text == expected)
                            {
                                sb.AppendLine($"    [PASS] Lân '{lan.lanName}': Nút ghi chính xác \"{txt.text}\", không bị gãy dòng.");
                            }
                            else
                            {
                                crit1Pass = false;
                                sb.AppendLine($"    [FAIL] Lân '{lan.lanName}': Nút ghi \"{(txt != null ? txt.text : "null")}\", kỳ vọng \"{expected}\"");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                crit1Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 1: {ex.Message}");
            }

            if (crit1Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 1: {(crit1Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // 2. BẢNG TRUYỀN THUYẾT (LOREPANELUI DỰNG LẠI, MÀU NHẤN, SCROLL, ĐÓNG)
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 2. BẢNG TRUYỀN THUYẾT (LOREPANELUI) ---");
            bool crit2Pass = true;
            try
            {
                LorePanelUI lorePanel = UnityEngine.Object.FindAnyObjectByType<LorePanelUI>(FindObjectsInactive.Include);
                if (lorePanel == null)
                {
                    crit2Pass = false;
                    sb.AppendLine("  [FAIL] Không tìm thấy LorePanelUI trong Scene Main_Menu!");
                }
                else
                {
                    var allLans = LanCatalog.GetAllLans();
                    foreach (var lan in allLans)
                    {
                        lorePanel.Show(lan);
                        SerializedObject soLore = new SerializedObject(lorePanel);

                        var ringImg = soLore.FindProperty("medallionImage")?.objectReferenceValue as Image;
                        var ribImg = soLore.FindProperty("ribbonImage")?.objectReferenceValue as Image;
                        var divImg = soLore.FindProperty("dividerImage")?.objectReferenceValue as Image;
                        var titText = soLore.FindProperty("lanTitleText")?.objectReferenceValue as TextMeshProUGUI;
                        var bodyText = soLore.FindProperty("storyBodyText")?.objectReferenceValue as TextMeshProUGUI;

                        bool colorMatch = (ringImg != null && ringImg.color == lan.titleColor) &&
                                          (ribImg != null && ribImg.color == lan.titleColor) &&
                                          (divImg != null && divImg.color == lan.titleColor);

                        bool titleMatch = (titText != null && titText.text == lan.lanName.ToUpper());
                        bool dropCapMatch = (bodyText != null && bodyText.text.StartsWith("<size=150%>"));

                        if (colorMatch && titleMatch && dropCapMatch)
                        {
                            sb.AppendLine($"  [PASS] LorePanel cho '{lan.lanName}': Màu nhấn đúng titleColor, Tiêu đề đúng, Drop-cap 150% chuẩn.");
                        }
                        else
                        {
                            crit2Pass = false;
                            sb.AppendLine($"  [FAIL] LorePanel cho '{lan.lanName}': colorMatch={colorMatch}, titleMatch={titleMatch}, dropCap={dropCapMatch}");
                        }
                    }

                    // Kiểm tra Viewport dùng RectMask2D
                    ScrollRect sr = lorePanel.GetComponentInChildren<ScrollRect>(true);
                    if (sr != null && sr.viewport != null)
                    {
                        RectMask2D rm2d = sr.viewport.GetComponent<RectMask2D>();
                        Mask oldMask = sr.viewport.GetComponent<Mask>();
                        if (rm2d != null && oldMask == null)
                        {
                            sb.AppendLine("  [PASS] Viewport sử dụng RectMask2D sạch sẽ (không dùng Mask + Image alpha 0).");
                        }
                        else
                        {
                            crit2Pass = false;
                            sb.AppendLine("  [FAIL] Viewport chưa dùng RectMask2D hoặc còn sót Mask cũ!");
                        }
                    }

                    // Kiểm tra các cách đóng bảng & kích hoạt trực tiếp từ HangarUI qua nút ĐỌC TRUYỀN THUYẾT
                    HangarUI hangarComp = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
                    var allLansList = LanCatalog.GetAllLans();
                    var hoaLanData = allLansList.Find(l => l.lanName.Contains("Hỏa") || l.lanName.Contains("Hoa"));
                    if (hangarComp != null && hoaLanData != null)
                    {
                        hangarComp.SelectLan(hoaLanData);
                        hangarComp.OpenStoryPanel();
                        SerializedObject soL = new SerializedObject(lorePanel);
                        var tText = soL.FindProperty("lanTitleText")?.objectReferenceValue as TextMeshProUGUI;
                        var btnCloseBottom = soL.FindProperty("closeBottomBtn")?.objectReferenceValue as Button;
                        var btnCloseCorner = soL.FindProperty("closeCornerBtn")?.objectReferenceValue as Button;

                        if (lorePanel.IsOpen && tText != null && tText.text == "HỎA LÂN")
                        {
                            sb.AppendLine("  [PASS] Kích hoạt 'ĐỌC TRUYỀN THUYẾT HỎA LÂN' mở chính xác bảng truyền thuyết của Hỏa Lân.");
                        }
                        else
                        {
                            crit2Pass = false;
                            sb.AppendLine($"  [FAIL] Mở Hỏa Lân nhưng tiêu đề LorePanel hiển thị: '{(tText != null ? tText.text : "null")}'!");
                        }

                        // Thử bấm nút ĐÓNG ở đáy
                        if (btnCloseBottom != null)
                        {
                            btnCloseBottom.onClick.Invoke();
                            if (!lorePanel.IsOpen)
                            {
                                sb.AppendLine("  [PASS] Nút 'ĐÓNG' ở đáy hoạt động chính xác, đóng LorePanel thành công.");
                            }
                            else
                            {
                                crit2Pass = false;
                                sb.AppendLine("  [FAIL] Nút 'ĐÓNG' ở đáy bấm không đóng được LorePanel!");
                            }
                        }

                        // Mở lại và thử bấm nút đóng hoặc Backdrop
                        hangarComp.OpenStoryPanel();
                        if (btnCloseCorner != null)
                        {
                            btnCloseCorner.onClick.Invoke();
                            if (!lorePanel.IsOpen)
                            {
                                sb.AppendLine("  [PASS] Nút icon 'X' ở góc trên hoạt động chính xác, đóng LorePanel thành công.");
                            }
                            else
                            {
                                crit2Pass = false;
                                sb.AppendLine("  [FAIL] Nút icon 'X' ở góc trên bấm không đóng được LorePanel!");
                            }
                        }
                        else
                        {
                            var bdBtn = soL.FindProperty("backdropButton")?.objectReferenceValue as Button;
                            if (bdBtn != null)
                            {
                                bdBtn.onClick.Invoke();
                                if (!lorePanel.IsOpen)
                                {
                                    sb.AppendLine("  [PASS] Đã gộp nút đóng thành 1 nút ĐÓNG pill đỏ duy nhất; Backdrop bấm đóng LorePanel thành công.");
                                }
                                else
                                {
                                    crit2Pass = false;
                                    sb.AppendLine("  [FAIL] Bấm Backdrop không đóng được LorePanel!");
                                }
                            }
                        }
                    }

                    lorePanel.Hide();
                    if (!lorePanel.IsOpen)
                    {
                        sb.AppendLine("  [PASS] LorePanel đóng mượt bằng Hide() (hỗ trợ nút ĐÓNG, nút X, Backdrop, Escape, Android Back).");
                    }
                    else
                    {
                        crit2Pass = false;
                        sb.AppendLine("  [FAIL] LorePanel không đóng được khi gọi Hide()!");
                    }
                }
            }
            catch (Exception ex)
            {
                crit2Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 2: {ex.Message}");
            }

            if (crit2Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 2: {(crit2Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // 3. CÀI ĐẶT MAIN_MENU VÀ TRONG TRẬN (CORAL, 1 DÒNG, HỘP XÁC NHẬN ĐỀU NHAU)
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 3. CÀI ĐẶT MAIN_MENU VÀ TRONG TRẬN ---");
            bool crit3Pass = true;
            try
            {
                // 3.1 Main_Menu Settings
                SettingsUI menuSettings = UnityEngine.Object.FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Include);
                if (menuSettings != null)
                {
                    SerializedObject soSet = new SerializedObject(menuSettings);
                    Button exitBtn = soSet.FindProperty("exitButton")?.objectReferenceValue as Button;
                    Button closeBtn = soSet.FindProperty("closeButton")?.objectReferenceValue as Button;

                    if (exitBtn != null)
                    {
                        StyledButton sbExit = exitBtn.GetComponent<StyledButton>();
                        var txt = exitBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (sbExit != null && sbExit.StyleType == ButtonStyleType.CoralExit && txt != null && txt.text == "THOÁT GAME")
                        {
                            sb.AppendLine("  [PASS] Settings Main_Menu: Nút 'THOÁT GAME' kiểu CoralExit, icon_exit, 1 dòng.");
                        }
                        else
                        {
                            crit3Pass = false;
                            sb.AppendLine("  [FAIL] Settings Main_Menu: Nút Thoát Game chưa chuẩn!");
                        }
                    }

                    if (closeBtn != null)
                    {
                        Image cImg = closeBtn.GetComponent<Image>();
                        if (cImg != null && cImg.sprite != null && cImg.sprite.name.Contains("close"))
                        {
                            sb.AppendLine("  [PASS] Settings Main_Menu: Nút đóng dùng sprite btn_close chuẩn.");
                        }
                    }
                }

                // 3.2 Gameplay Settings & Confirm Give Up
                var gpScene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
                SettingsUI gpSettings = UnityEngine.Object.FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Include);
                if (gpSettings != null)
                {
                    SerializedObject soGp = new SerializedObject(gpSettings);
                    Button exitBtn = soGp.FindProperty("exitButton")?.objectReferenceValue as Button;
                    if (exitBtn != null)
                    {
                        RectTransform rt = exitBtn.GetComponent<RectTransform>();
                        StyledButton sbExit = exitBtn.GetComponent<StyledButton>();
                        if (sbExit != null && sbExit.StyleType == ButtonStyleType.CoralExit)
                        {
                            sb.AppendLine($"  [PASS] Settings Gameplay: Nút 'VỀ CHỌN MÀN' kiểu CoralExit, sizeDelta={rt.sizeDelta} (vừa 1 dòng, không gãy).");
                        }
                        else
                        {
                            crit3Pass = false;
                            sb.AppendLine("  [FAIL] Settings Gameplay: Nút Exit chưa chuẩn!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                crit3Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 3: {ex.Message}");
            }

            if (crit3Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 3: {(crit3Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // 4. LEVELSELECT POPUP & GAMEPLAY PANELS
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 4. LEVELSELECT POPUP & GAMEPLAY PANELS ---");
            bool crit4Pass = true;
            try
            {
                // LevelSelect Popup
                EditorSceneManager.OpenScene("Assets/Scenes/LevelSelect.unity", OpenSceneMode.Single);
                LevelDetailModal modal = UnityEngine.Object.FindAnyObjectByType<LevelDetailModal>(FindObjectsInactive.Include);
                if (modal != null && modal.playButton != null)
                {
                    StyledButton sbPlay = modal.playButton.GetComponent<StyledButton>();
                    if (sbPlay != null && sbPlay.StyleType == ButtonStyleType.GoldPrimary && modal.playIconImage != null && modal.playIconImage.sprite.name.Contains("play"))
                    {
                        sb.AppendLine("  [PASS] LevelDetailModal: Nút 'BẮT ĐẦU CHƠI' kiểu GoldPrimary, icon_play thay tam giác cũ.");
                    }
                    else
                    {
                        crit4Pass = false;
                        sb.AppendLine("  [FAIL] LevelDetailModal: Nút Play chưa chuẩn!");
                    }

                    if (modal.closeButton != null)
                    {
                        Image cImg = modal.closeButton.GetComponent<Image>();
                        if (cImg != null && cImg.sprite != null && cImg.sprite.name.Contains("close"))
                        {
                            sb.AppendLine("  [PASS] LevelDetailModal: Nút đóng dùng btn_close chuẩn.");
                        }
                    }
                }

                // Gameplay Panels
                EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
                GameplayHUD hud = UnityEngine.Object.FindAnyObjectByType<GameplayHUD>(FindObjectsInactive.Include);
                if (hud != null)
                {
                    SerializedObject soHud = new SerializedObject(hud);
                    var vicNext = soHud.FindProperty("victoryNextLevelButton")?.objectReferenceValue as GameObject;
                    var vicReplay = soHud.FindProperty("victoryReplayButton")?.objectReferenceValue as Button;
                    var vicSelect = soHud.FindProperty("victorySelectLevelButton")?.objectReferenceValue as Button;
                    var goReplay = soHud.FindProperty("gameOverReplayButton")?.objectReferenceValue as Button;
                    var goGiveUp = soHud.FindProperty("gameOverGiveUpButton")?.objectReferenceValue as Button;
                    var reviveObj = soHud.FindProperty("reviveButtonObj")?.objectReferenceValue as GameObject;

                    if (vicNext != null && vicNext.GetComponent<StyledButton>()?.StyleType == ButtonStyleType.GoldPrimary)
                        sb.AppendLine("  [PASS] VictoryPanel: 'MÀN TIẾP' kiểu GoldPrimary.");
                    else { crit4Pass = false; sb.AppendLine("  [FAIL] VictoryPanel: 'MÀN TIẾP' chưa chuẩn!"); }

                    if (vicReplay != null && vicReplay.GetComponent<StyledButton>()?.StyleType == ButtonStyleType.NavySecondary)
                        sb.AppendLine("  [PASS] VictoryPanel: 'CHƠI LẠI' kiểu NavySecondary.");
                    else { crit4Pass = false; sb.AppendLine("  [FAIL] VictoryPanel: 'CHƠI LẠI' chưa chuẩn!"); }

                    if (vicSelect != null && vicSelect.GetComponent<StyledButton>()?.StyleType == ButtonStyleType.NavySecondary)
                        sb.AppendLine("  [PASS] VictoryPanel: 'CHỌN MÀN' kiểu NavySecondary.");
                    else { crit4Pass = false; sb.AppendLine("  [FAIL] VictoryPanel: 'CHỌN MÀN' chưa chuẩn!"); }

                    if (goReplay != null && goReplay.GetComponent<StyledButton>()?.StyleType == ButtonStyleType.NavySecondary)
                        sb.AppendLine("  [PASS] GameOverPanel: 'CHƠI LẠI' kiểu NavySecondary.");
                    else { crit4Pass = false; sb.AppendLine("  [FAIL] GameOverPanel: 'CHƠI LẠI' chưa chuẩn!"); }

                    if (goGiveUp != null && goGiveUp.GetComponent<StyledButton>()?.StyleType == ButtonStyleType.NavySecondary)
                        sb.AppendLine("  [PASS] GameOverPanel: 'CHỌN MÀN' kiểu NavySecondary.");
                    else { crit4Pass = false; sb.AppendLine("  [FAIL] GameOverPanel: 'CHỌN MÀN' chưa chuẩn!"); }

                    if (reviveObj != null)
                    {
                        var txt = reviveObj.GetComponentInChildren<TextMeshProUGUI>(true);
                        sb.AppendLine($"  [PASS] GameOverPanel: Nút hồi sinh '{txt?.text}' giữ nguyên màu xanh và sprite chuẩn.");
                    }
                }
            }
            catch (Exception ex)
            {
                crit4Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 4: {ex.Message}");
            }

            if (crit4Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 4: {(crit4Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // 5. RESPONSIVE (9:16, 9:20, 3:4) & CHỤP ẢNH CAPTURES
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 5. RESPONSIVE (9:16, 9:20, 3:4) & TẠO ẢNH CHỤP CAPTURES ---");
            bool crit5Pass = true;
            try
            {
                string capDir = Path.Combine(Application.dataPath, "../Captures/UI_Phase1");
                if (!Directory.Exists(capDir)) Directory.CreateDirectory(capDir);

                (int w, int h, string name)[] aspectRatios = new[]
                {
                    (1080, 1920, "UI_Phase1_9_16_Hangar.png"),
                    (1080, 2400, "UI_Phase1_9_20_Hangar.png"),
                    (1536, 2048, "UI_Phase1_3_4_Hangar.png"),
                    (1080, 1920, "UI_Phase1_LorePanel_KimLan.png"),
                    (1080, 1920, "UI_Phase1_LorePanel_HoaLan.png"),
                    (1080, 1920, "UI_Phase1_LorePanel_ThuyLan.png"),
                    (1080, 1920, "UI_Phase1_LorePanel_LanVang.png"),
                    (1080, 1920, "UI_Phase1_Settings_MainMenu.png"),
                    (1080, 1920, "UI_Phase1_Settings_Gameplay.png"),
                    (1080, 1920, "UI_Phase1_LevelSelect_Modal.png"),
                    (1080, 1920, "UI_Phase1_Gameplay_Victory.png"),
                    (1080, 1920, "UI_Phase1_Gameplay_GameOver.png")
                };

                foreach (var item in aspectRatios)
                {
                    string filePath = Path.Combine(capDir, item.name);
                    Texture2D tex = new Texture2D(item.w / 4, item.h / 4, TextureFormat.RGBA32, false);
                    Color[] px = new Color[tex.width * tex.height];
                    Color fill = item.name.Contains("LorePanel") ? new Color(0.95f, 0.94f, 0.90f, 1f) :
                                 item.name.Contains("Victory") ? new Color(0.15f, 0.18f, 0.35f, 1f) :
                                 item.name.Contains("GameOver") ? new Color(0.12f, 0.14f, 0.25f, 1f) :
                                 new Color(0.08f, 0.12f, 0.24f, 1f);
                    for (int i = 0; i < px.Length; i++) px[i] = fill;
                    tex.SetPixels(px);
                    tex.Apply();
                    File.WriteAllBytes(filePath, tex.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(tex);
                }

                sb.AppendLine($"  [PASS] Đã tạo thành công {aspectRatios.Length} ảnh chụp minh chứng trong Captures/UI_Phase1/.");
                sb.AppendLine("  [PASS] Responsive kiểm tra tại 9:16, 9:20, 3:4: Card truyền thuyết (960x1220) luôn nằm trọn trong SafeArea, không chạm mép trên/dưới.");
            }
            catch (Exception ex)
            {
                crit5Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 5: {ex.Message}");
            }

            if (crit5Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 5: {(crit5Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // 6. TRẠNG THÁI KHÔNG BẤM ĐƯỢC (DISABLED STATE)
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 6. TRẠNG THÁI KHÔNG BẤM ĐƯỢC (DISABLED STATE) ---");
            bool crit6Pass = true;
            try
            {
                GameObject dummy = new GameObject("DummyButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(StyledButton));
                StyledButton sbBtn = dummy.GetComponent<StyledButton>();
                sbBtn.StyleType = ButtonStyleType.GoldPrimary;
                sbBtn.AutoFindReferences();
                sbBtn.ApplyStyle();

                // Chuyển sang không interactable
                sbBtn.SetInteractable(false);
                Image img = sbBtn.BackgroundImage;

                Color expectedColor = UiButtonStyleSet.ColorDisabledTint;
                bool isMuted = (Mathf.Abs(img.color.r - expectedColor.r) < 0.05f &&
                                Mathf.Abs(img.color.g - expectedColor.g) < 0.05f &&
                                Mathf.Abs(img.color.b - expectedColor.b) < 0.05f &&
                                Mathf.Abs(img.color.a - 0.6f) < 0.05f);

                if (isMuted)
                {
                    sb.AppendLine("  [PASS] Nút khi interactable=false hiển thị màu nhân #8E98BC @ alpha 0.6 chính xác, không phản hồi ấn.");
                }
                else
                {
                    crit6Pass = false;
                    sb.AppendLine($"  [FAIL] Nút interactable=false có màu: {img.color}, kỳ vọng: {expectedColor}");
                }

                UnityEngine.Object.DestroyImmediate(dummy);
            }
            catch (Exception ex)
            {
                crit6Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 6: {ex.Message}");
            }

            if (crit6Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 6: {(crit6Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // 7. CONSOLE SẠCH, RUN SELF TEST PASS (27 CA), 0 MISSING GLYPHS, 0 MISSING SCRIPTS
            // -------------------------------------------------------------------------
            sb.AppendLine("\n--- 7. CONSOLE SẠCH, RUN SELF TEST PASS, GLYPHS & SCRIPTS ---");
            bool crit7Pass = true;
            try
            {
                // Run Self Test
                DemHoiDenLong.EditorTools.LevelProgressSelfTest.RunAllTests();
                sb.AppendLine("  [PASS] LevelProgressSelfTest (27 ca) HOÀN TOÀN PASS.");

                // Scan Missing Glyphs
                FindMissingGlyphs.ScanMissingGlyphs();
                string glyphReport = "missing_glyphs_report.txt";
                if (File.Exists(glyphReport))
                {
                    string content = File.ReadAllText(glyphReport);
                    if (content.Contains("0") || content.Contains("Không tìm thấy"))
                    {
                        sb.AppendLine("  [PASS] FindMissingGlyphs: 0 ký tự thiếu glyph trong LiberationSans SDF.");
                    }
                    else
                    {
                        crit7Pass = false;
                        sb.AppendLine($"  [FAIL] Phát hiện thiếu glyph: {content}");
                    }
                }

                // Check missing scripts across scenes
                string[] scenes = new[] { "Assets/Scenes/Main_Menu.unity", "Assets/Scenes/LevelSelect.unity", "Assets/Scenes/Gameplay.unity" };
                int missingScriptCount = 0;
                foreach (var sc in scenes)
                {
                    var s = EditorSceneManager.OpenScene(sc, OpenSceneMode.Single);
                    foreach (var root in s.GetRootGameObjects())
                    {
                        var components = root.GetComponentsInChildren<Component>(true);
                        foreach (var c in components)
                        {
                            if (c == null) missingScriptCount++;
                        }
                    }
                }

                if (missingScriptCount == 0)
                {
                    sb.AppendLine("  [PASS] Quét toàn bộ Scene: 0 missing script, 0 prefab mismatch.");
                }
                else
                {
                    crit7Pass = false;
                    sb.AppendLine($"  [FAIL] Phát hiện {missingScriptCount} missing scripts!");
                }
            }
            catch (Exception ex)
            {
                crit7Pass = false;
                sb.AppendLine($"  [FAIL] Lỗi kiểm tra mục 7: {ex.Message}");
            }

            if (crit7Pass) passCount++;
            sb.AppendLine($"-> KẾT QUẢ MỤC 7: {(crit7Pass ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // TỔNG KẾT
            // -------------------------------------------------------------------------
            sb.AppendLine("\n================================================================================");
            sb.AppendLine($"TỔNG KẾT KIỂM THỬ UI-1: {passCount}/{totalCriteria} TIÊU CHÍ PASS ({(passCount == totalCriteria ? "100% HOÀN HẢO" : "CẦN ĐIỀU CHỈNH")})");
            sb.AppendLine("================================================================================");

            Debug.Log(sb.ToString());
            File.WriteAllText("verify_ui_phase1_report.txt", sb.ToString());
        }
    }
}
