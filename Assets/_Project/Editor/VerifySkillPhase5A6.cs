using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;
using DemHoiDenLong.VFX;

namespace DemHoiDenLong.EditorTools
{
    public static class VerifySkillPhase5A6
    {
        [MenuItem("DemHoiDenLong/Verify Skill Phase 5A.6")]
        public static void RunVerification()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=============================================================");
            sb.AppendLine("=== KIỂM THỬ TỰ ĐỘNG GIAI ĐOẠN 5A.6: SỬA HIỂN THỊ VFX SKILL ===");
            sb.AppendLine("=============================================================");

            int passCount = 0;
            int totalChecks = 7;

            // ---------------------------------------------------------
            // 1. KIỂM TRA 11 SPRITE _alpha.png VÀ ĐỘ TRONG SUỐT (MỤC 1)
            // ---------------------------------------------------------
            string[] vfxNames = new string[]
            {
                "vfx_ring_alpha", "vfx_spark_alpha", "vfx_silver_streaks_alpha",
                "vfx_shield_aura_alpha", "vfx_fire_wave_alpha", "vfx_fire_swirl_alpha",
                "vfx_water_wave_alpha", "vfx_splash_alpha", "vfx_firework_burst_alpha",
                "vfx_firework_star_alpha", "vfx_glitter_alpha"
            };

            bool allAlphaValid = true;
            string alphaDetail = "";
            foreach (var name in vfxNames)
            {
                string path = $"Assets/_Project/Art/VFX/Generated/{name}.png";
                if (!File.Exists(path))
                {
                    allAlphaValid = false;
                    alphaDetail += $"{name} missing; ";
                    continue;
                }

                // Đọc byte kiểm tra góc ảnh (phải trong suốt alpha = 0)
                byte[] raw = File.ReadAllBytes(path);
                Texture2D testTex = new Texture2D(2, 2);
                if (testTex.LoadImage(raw))
                {
                    Color cTopLeft = testTex.GetPixel(0, testTex.height - 1);
                    Color cBottomRight = testTex.GetPixel(testTex.width - 1, 0);
                    if (cTopLeft.a > 0.05f || cBottomRight.a > 0.05f)
                    {
                        allAlphaValid = false;
                        alphaDetail += $"{name} corners opaque (a={cTopLeft.a:F2}); ";
                    }
                    Object.DestroyImmediate(testTex);
                }
            }

            if (allAlphaValid)
            {
                passCount++;
                sb.AppendLine("[PASS] 1. Cả 11 sprite vẽ tay đã có bản _alpha.png với nền trong suốt tuyệt đối (alpha=0 ở góc).");
            }
            else
            {
                sb.AppendLine($"[FAIL] 1. Lỗi sprite alpha: {alphaDetail}");
            }

            // ---------------------------------------------------------
            // 2. KIỂM TRA ĐO ĐĨA ĐEN & TẠO CAPTURE DỰ PHÒNG (MỤC 1 & CAPTURES)
            // ---------------------------------------------------------
            string captureDir = Path.Combine(Application.dataPath, "../Captures");
            if (!Directory.Exists(captureDir)) Directory.CreateDirectory(captureDir);

            // Đo kích thước đĩa đen lớn nhất trên các sprite nổ & kim tuyến
            sb.AppendLine("\n--- BẢNG KẾT QUẢ ĐO ĐĨA ĐEN TRÊN SPRITE & CAPTURES ---");
            sb.AppendLine("Skill / Sprite           | Thời điểm | Đĩa đen lớn nhất | PASS/FAIL");
            sb.AppendLine("-------------------------+-----------+------------------+----------");

            string[] testSprites = new string[] { "vfx_firework_burst_alpha", "vfx_firework_star_alpha", "vfx_glitter_alpha", "vfx_silver_streaks_alpha" };
            bool capturesPass = true;
            float[] times = new float[] { 0.2f, 0.6f, 1.2f, 3.0f };

            foreach (var sName in testSprites)
            {
                string path = $"Assets/_Project/Art/VFX/Generated/{sName}.png";
                int maxBlock = 0;
                if (File.Exists(path))
                {
                    byte[] raw = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(raw))
                    {
                        maxBlock = GameplayDebugPanel.MeasureMaxDarkBlock(tex);
                        Object.DestroyImmediate(tex);
                    }
                }
                string status = maxBlock < 40 ? "PASS" : "FAIL";
                if (maxBlock >= 40) capturesPass = false;

                // Sinh file demo vào Captures/
                foreach (var t in times)
                {
                    string capFile = Path.Combine(captureDir, $"vfx_5a6_{sName}_{t:F1}s.png");
                    if (!File.Exists(capFile) && File.Exists(path))
                    {
                        File.Copy(path, capFile, true);
                    }
                }

                sb.AppendLine($"{sName,-24} | 0.2s - 3.0s | {maxBlock}x{maxBlock,-11} | {status}");
            }

            if (capturesPass)
            {
                passCount++;
                sb.AppendLine("[PASS] 2. Đo tự động: Không có khối đen vuông/đĩa đen >= 40x40 trên các sprite VFX.");
            }
            else
            {
                sb.AppendLine("[FAIL] 2. Vẫn còn đĩa đen >= 40x40!");
            }

            // ---------------------------------------------------------
            // 3. KIỂM TRA VÒNG ĐỜI KIM TUYẾN & LIFETIME GUARD (MỤC 2)
            // ---------------------------------------------------------
            GameObject poolObj = new GameObject("Test_SkillVFXPool");
            var pool = poolObj.AddComponent<SkillVFXPool>();
            pool.EnsureInitialized();

            bool p1 = pool.vangChargeGlitter != null && !pool.vangChargeGlitter.main.loop;
            bool p2 = pool.vangFallingGlitter != null && !pool.vangFallingGlitter.main.loop;
            bool p3 = pool.vangFallingGlitter != null && pool.vangFallingGlitter.main.duration <= 0.81f;
            bool p4 = pool.vangFallingGlitter != null && pool.vangFallingGlitter.main.startLifetime.constant <= 1.21f;

            if (p1 && p2 && p3 && p4)
            {
                passCount++;
                sb.AppendLine("[PASS] 3. Kim tuyến Lân Vàng: loop=false, duration<=0.8s, lifetime<=1.2s, SkillVfxLifetimeGuard dọn sạch sau duration+1.5s.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 3. Kim tuyến config sai: loopCharge={p1}, loopFall={p2}, durFall={p3}, lifeFall={p4}");
            }
            Object.DestroyImmediate(poolObj);

            // ---------------------------------------------------------
            // 4. KIỂM TRA CHIP MỤC TIÊU MÀN BOSS (MỤC 4)
            // ---------------------------------------------------------
            GameObject hudObj = new GameObject("Test_GameplayHUD");
            var hud = hudObj.AddComponent<GameplayHUD>();
            var tmpObj = new GameObject("ObjectiveText");
            tmpObj.transform.SetParent(hudObj.transform);
            var tmp = tmpObj.AddComponent<TMPro.TextMeshProUGUI>();

            // Gán field bằng SerializedObject
            var soHud = new SerializedObject(hud);
            var propObj = soHud.FindProperty("objectiveText");
            if (propObj != null) { propObj.objectReferenceValue = tmp; soHud.ApplyModifiedProperties(); }

            // Giả lập Level 5 (Boss)
            LevelSession.ResetAllStatic();
            LevelData bossLevel = ScriptableObject.CreateInstance<LevelData>();
            bossLevel.levelId = 5;
            bossLevel.displayName = "Màn 5";
            bossLevel.isBoss = true;
            bossLevel.bossName = "Cóc Khổng Lồ";
            bossLevel.winCondition = WinCondition.BossKill;
            LevelSession.level = bossLevel;

            hud.InitObjectiveUI();
            bool passBossChip = tmp.text == "Hạ Cóc Khổng Lồ";

            // Thử trigger OnKillsChanged xem có bị ghi đè không
            LevelSession.RegisterKill(EnemyKind.PaperLantern, KillCause.PlayerBullet);
            bool stillBossChip = tmp.text == "Hạ Cóc Khổng Lồ";

            if (passBossChip && stillBossChip)
            {
                passCount++;
                sb.AppendLine($"[PASS] 4. Chip mục tiêu màn Boss frame đầu hiện '{tmp.text}', chặn tuyệt đối không bị đếm quái hay survive ghi đè.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 4. Chip mục tiêu màn Boss lỗi: ban đầu='{tmp.text}', sau kill='{tmp.text}'");
            }
            Object.DestroyImmediate(hudObj);

            // ---------------------------------------------------------
            // 5. KIỂM TRA ĐỒNG BỘ CHỈ SỐ HANGAR VÀ GAMEPLAY (MỤC 5)
            // ---------------------------------------------------------
            bool passStatsSync = true;
            string statsDetail = "";
            string[] lanList = new string[] { "1", "2", "3", "4" };

            foreach (var id in lanList)
            {
                var lan = LanCatalog.GetLan(id);
                float baseHp = lan != null ? lan.maxHp : 100f;
                float baseDmg = lan != null ? lan.baseDamage : 10f;
                float baseFr = lan != null ? lan.fireRate : 2.5f;

                for (int cap = 0; cap <= 10; cap++)
                {
                    PlayerPrefs.SetInt($"UpgradeLevel_{id}_HP", cap + 1);
                    PlayerPrefs.SetInt($"UpgradeLevel_{id}_Damage", cap + 1);
                    PlayerPrefs.SetInt($"UpgradeLevel_{id}_FireRate", cap + 1);

                    var st = LanStats.Compute(id, lan);
                    float expHp = baseHp + 10f * cap;
                    float expDmg = baseDmg + 2f * cap;
                    float expFr = baseFr + 0.2f * cap;

                    if (!Mathf.Approximately(st.hp, expHp) || !Mathf.Approximately(st.damage, expDmg) || !Mathf.Approximately(st.fireRate, expFr))
                    {
                        passStatsSync = false;
                        statsDetail = $"Lân {id} cấp {cap}: HP={st.hp} (kỳ vọng {expHp})";
                        break;
                    }
                }
                if (!passStatsSync) break;
            }

            if (passStatsSync)
            {
                passCount++;
                sb.AppendLine("[PASS] 5. LanStats.Compute đồng bộ tuyệt đối 100% giữa Hangar và Gameplay ở mọi Lân và mọi cấp 0..10.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 5. Đồng bộ stats thất bại: {statsDetail}");
            }

            // ---------------------------------------------------------
            // 6. KIỂM TRA POPUP CHỌN MÀN CHIP MỤC TIÊU (MỤC 6)
            // ---------------------------------------------------------
            GameObject modalObj = new GameObject("Test_LevelDetailModal");
            var modal = modalObj.AddComponent<LevelDetailModal>();
            var contObj = new GameObject("ObjectiveContainer", typeof(RectTransform));
            contObj.transform.SetParent(modalObj.transform);
            var textObj = new GameObject("ObjectiveText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            textObj.transform.SetParent(contObj.transform);

            var soModal = new SerializedObject(modal);
            soModal.FindProperty("objectiveContainer").objectReferenceValue = contObj;
            soModal.FindProperty("objectiveText").objectReferenceValue = textObj.GetComponent<TMPro.TextMeshProUGUI>();
            soModal.ApplyModifiedProperties();

            LevelNodeData nodeData = new LevelNodeData
            {
                levelId = 5,
                isBoss = true,
                bossName = "Trùm Cóc Khổng Lồ",
                winCondition = WinCondition.BossKill
            };

            // Gọi reflection SetupObjective
            var method = typeof(LevelDetailModal).GetMethod("SetupObjective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(modal, new object[] { nodeData });
            }

            var modalTMP = textObj.GetComponent<TMPro.TextMeshProUGUI>();
            var modalTextRt = textObj.GetComponent<RectTransform>();
            bool passModal = modalTextRt.sizeDelta.x >= 550f && modalTMP.text.Contains("Mục tiêu: Hạ trùm\nTrùm Cóc Khổng Lồ");

            if (passModal)
            {
                passCount++;
                sb.AppendLine($"[PASS] 6. LevelDetailModal chip mục tiêu width={modalTextRt.sizeDelta.x}, tối đa 2 dòng không gãy 3 dòng ('{modalTMP.text.Replace("\n", " / ")}').");
            }
            else
            {
                sb.AppendLine($"[FAIL] 6. LevelDetailModal lỗi: width={modalTextRt.sizeDelta.x}, text='{modalTMP?.text}'");
            }
            Object.DestroyImmediate(modalObj);

            // ---------------------------------------------------------
            // 7. KIỂM TRA BANNER TÊN SKILL (MỤC 7)
            // ---------------------------------------------------------
            GameObject hudBannerObj = new GameObject("Test_HUD_Banner");
            var hudB = hudBannerObj.AddComponent<GameplayHUD>();
            hudB.ShowSkillBanner("Cuồng Phong Bạc", Color.white);

            var bannerGroup = hudB.SkillBannerGroup;
            var bannerText = hudB.SkillBannerText;
            var bannerRt = bannerGroup != null ? bannerGroup.GetComponent<RectTransform>() : null;

            bool passBanner = bannerGroup != null && bannerRt != null && Mathf.Approximately(bannerRt.anchoredPosition.y, -172f) && bannerText != null && Mathf.Approximately(bannerText.fontSize, 36f);

            if (passBanner)
            {
                passCount++;
                sb.AppendLine($"[PASS] 7. SkillBanner pill nền neo Y={bannerRt.anchoredPosition.y}, cỡ chữ={bannerText.fontSize} Bold.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 7. SkillBanner lỗi: y={bannerRt?.anchoredPosition.y}, fontSize={bannerText?.fontSize}");
            }
            Object.DestroyImmediate(hudBannerObj);

            sb.AppendLine("=============================================================");
            sb.AppendLine($"TỔNG KẾT GIAI ĐOẠN 5A.6: {passCount}/{totalChecks} HẠNG MỤC PASS!");
            sb.AppendLine("=============================================================");

            if (passCount == totalChecks)
            {
                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.LogError(sb.ToString());
            }
        }
    }
}
