using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;
using DemHoiDenLong.VFX;

namespace DemHoiDenLong.EditorTools
{
    public static class VerifySkillPhase5A4
    {
        [MenuItem("DemHoiDenLong/Verify Skill Phase 5A4")]
        public static void RunVerification()
        {
            // Auto run setup bindings first
            ApplySkillUIBindings.Apply();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=================================================================");
            sb.AppendLine("=== XÁC MINH TOÀN DIỆN KỸ NĂNG VÀ NÚT SKILL GIAI ĐOẠN 5A.4 ===");
            sb.AppendLine("=================================================================");

            int passCount = 0;
            int totalChecks = 6;

            // 1. KIỂM TRA 8 SPRITE VFX ĐƯỢC SINH BẰNG CODE
            {
                sb.AppendLine("\n--- 1. KIỂM TRA SPRITE VFX SINH BẰNG CODE ---");
                string[] expectedSprites = new string[]
                {
                    "vfx_soft_circle.png", "vfx_spark4.png", "vfx_streak.png", "vfx_ring.png",
                    "vfx_flame.png", "vfx_wave_band.png", "vfx_burst.png", "vfx_droplet.png"
                };

                bool allSpritesFound = true;
                foreach (var sName in expectedSprites)
                {
                    string p = $"Assets/_Project/Art/VFX/{sName}";
                    var spr = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    if (spr != null)
                    {
                        sb.AppendLine($"  [PASS] {sName} (W={spr.rect.width}, H={spr.rect.height})");
                    }
                    else
                    {
                        allSpritesFound = false;
                        sb.AppendLine($"  [FAIL] Không tìm thấy sprite: {p}");
                    }
                }

                if (allSpritesFound) passCount++;
            }

            // 2. KIỂM TRA 4 SKILLCONFIG SCRIPTABLEOBJECT VÀ GDD STATS
            {
                sb.AppendLine("\n--- 2. KIỂM TRA DỮ LIỆU SKILL CONFIG (GDD) ---");
                AssetDatabase.Refresh();
                bool configsOk = true;

                // Kim
                var rawKim = AssetDatabase.LoadMainAssetAtPath("Assets/_Project/Data/Skills/Skill_KimLan.asset");
                sb.AppendLine($"  [DEBUG] rawKim: {(rawKim != null ? rawKim.GetType().FullName : "null")}");
                var kim = rawKim as SkillConfig;
                if (kim != null && Mathf.Approximately(kim.cooldown, 15f) && Mathf.Approximately(kim.duration, 1.5f) && Mathf.Approximately(kim.damage, 0f) && Mathf.Approximately(kim.bossDamageCap, 0f))
                {
                    sb.AppendLine($"  [PASS] Kim Lân: {kim.skillName} (CD={kim.cooldown}s, Dur={kim.duration}s, Dmg={kim.damage}, BossCap={kim.bossDamageCap})");
                }
                else
                {
                    configsOk = false;
                    sb.AppendLine(kim == null ? "  [FAIL] Kim: null" : $"  [FAIL] Kim: cd={kim.cooldown}, dur={kim.duration}, dmg={kim.damage}, cap={kim.bossDamageCap}");
                }

                // Hỏa
                var hoa = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_HoaLan.asset");
                if (hoa != null && Mathf.Approximately(hoa.cooldown, 20f) && Mathf.Approximately(hoa.duration, 1.1f) && Mathf.Approximately(hoa.damage, 50f) && Mathf.Approximately(hoa.bossDamageCap, 100f))
                {
                    sb.AppendLine($"  [PASS] Hỏa Lân: {hoa.skillName} (CD={hoa.cooldown}s, Dur={hoa.duration}s, Dmg={hoa.damage}, BossCap={hoa.bossDamageCap})");
                }
                else
                {
                    configsOk = false;
                    sb.AppendLine(hoa == null ? "  [FAIL] Hỏa: null" : $"  [FAIL] Hỏa: cd={hoa.cooldown}, dur={hoa.duration}, dmg={hoa.damage}, cap={hoa.bossDamageCap}");
                }

                // Thủy
                var thuy = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_ThuyLan.asset");
                if (thuy != null && Mathf.Approximately(thuy.cooldown, 18f) && Mathf.Approximately(thuy.duration, 1.0f) && Mathf.Approximately(thuy.damage, 30f) && Mathf.Approximately(thuy.slowPercent, 0.5f) && Mathf.Approximately(thuy.bossDamageCap, 60f))
                {
                    sb.AppendLine($"  [PASS] Thủy Lân: {thuy.skillName} (CD={thuy.cooldown}s, Dur={thuy.duration}s, Dmg={thuy.damage}, Slow={thuy.slowPercent:P0}, BossCap={thuy.bossDamageCap})");
                }
                else
                {
                    configsOk = false;
                    sb.AppendLine(thuy == null ? "  [FAIL] Thủy: null" : $"  [FAIL] Thủy: cd={thuy.cooldown}, dur={thuy.duration}, dmg={thuy.damage}, cap={thuy.bossDamageCap}");
                }

                // Vàng
                var vang = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_LanVang.asset");
                if (vang != null && Mathf.Approximately(vang.cooldown, 25f) && Mathf.Approximately(vang.duration, 1.5f) && Mathf.Approximately(vang.damage, 100f) && vang.fireworkCount == 12 && Mathf.Approximately(vang.bossDamageCap, 300f))
                {
                    sb.AppendLine($"  [PASS] Lân Vàng: {vang.skillName} (CD={vang.cooldown}s, Dur={vang.duration}s, Dmg={vang.damage}, Fireworks={vang.fireworkCount}, BossCap={vang.bossDamageCap})");
                }
                else
                {
                    configsOk = false;
                    sb.AppendLine(vang == null ? "  [FAIL] Vàng: null" : $"  [FAIL] Vàng: cd={vang.cooldown}, dur={vang.duration}, dmg={vang.damage}, cap={vang.bossDamageCap}");
                }

                if (configsOk) passCount++;
            }

            // 3. KIỂM TRA SCENE GAMEPLAY HIERARCHY
            {
                sb.AppendLine("\n--- 3. KIỂM TRA SCENE GAMEPLAY HIERARCHY ---");
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
                Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                bool hierarchyOk = true;

                if (canvas != null)
                {
                    Transform safe = canvas.transform.Find("SafeAreaRoot");
                    if (safe != null)
                    {
                        Transform sBtn = safe.Find("SkillButton");
                        if (sBtn != null)
                        {
                            RectTransform rt = sBtn.GetComponent<RectTransform>();
                            bool posOk = Mathf.Approximately(rt.anchoredPosition.x, -36f) && Mathf.Approximately(rt.anchoredPosition.y, 36f);
                            bool sizeOk = Mathf.Approximately(rt.sizeDelta.x, 132f) && Mathf.Approximately(rt.sizeDelta.y, 132f);

                            if (posOk && sizeOk)
                            {
                                sb.AppendLine("  [PASS] SkillButton: kích thước 132x132 tại vị trí (-36, 36) trong SafeAreaRoot.");
                            }
                            else
                            {
                                hierarchyOk = false;
                                sb.AppendLine($"  [FAIL] SkillButton rect sai: pos=({rt.anchoredPosition.x}, {rt.anchoredPosition.y}), size=({rt.sizeDelta.x}, {rt.sizeDelta.y})");
                            }

                            // Components
                            bool hasBg = sBtn.Find("SkillBg") != null;
                            bool hasRing = sBtn.Find("BorderRing") != null;
                            bool hasIcon = sBtn.Find("SkillIcon") != null;
                            bool hasCd = sBtn.Find("CooldownOverlay") != null;
                            bool hasText = sBtn.Find("CooldownText") != null;

                            if (hasBg && hasRing && hasIcon && hasCd && hasText)
                            {
                                sb.AppendLine("  [PASS] SkillButton có đủ 5 thành phần: SkillBg, BorderRing, SkillIcon, CooldownOverlay, CooldownText.");
                            }
                            else
                            {
                                hierarchyOk = false;
                                sb.AppendLine($"  [FAIL] Thiếu thành phần nút: Bg={hasBg}, Ring={hasRing}, Icon={hasIcon}, Cd={hasCd}, Text={hasText}");
                            }
                        }
                        else
                        {
                            hierarchyOk = false;
                            sb.AppendLine("  [FAIL] Không tìm thấy SkillButton dưới SafeAreaRoot!");
                        }

                        // Banner
                        Transform banner = safe.Find("SkillBanner");
                        if (banner != null && banner.GetComponent<CanvasGroup>() != null && banner.GetComponentInChildren<TextMeshProUGUI>(true) != null)
                        {
                            sb.AppendLine("  [PASS] SkillBanner tồn tại với CanvasGroup và TextMeshProUGUI.");
                        }
                        else
                        {
                            hierarchyOk = false;
                            sb.AppendLine("  [FAIL] SkillBanner không hợp lệ!");
                        }
                    }
                    else
                    {
                        hierarchyOk = false;
                        sb.AppendLine("  [FAIL] Không tìm thấy SafeAreaRoot!");
                    }

                    // ScreenBorderFlash
                    Transform flash = canvas.transform.Find("ScreenBorderFlash");
                    if (flash != null && flash.GetComponent<Image>() != null)
                    {
                        sb.AppendLine("  [PASS] ScreenBorderFlash tồn tại trên Canvas.");
                    }
                    else
                    {
                        hierarchyOk = false;
                        sb.AppendLine("  [FAIL] Không tìm thấy ScreenBorderFlash!");
                    }
                }
                else
                {
                    hierarchyOk = false;
                    sb.AppendLine("  [FAIL] Không tìm thấy Canvas!");
                }

                if (hierarchyOk) passCount++;
            }

            // 4. KIỂM TRA GAMEPLAYHUD BINDINGS
            {
                sb.AppendLine("\n--- 4. KIỂM TRA THAM CHIẾU GAMEPLAYHUD ---");
                Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                GameplayHUD hud = canvas != null ? canvas.GetComponentInChildren<GameplayHUD>(true) : null;
                bool bindingsOk = true;

                if (hud != null)
                {
                    SerializedObject so = new SerializedObject(hud);
                    string[] props = new string[]
                    {
                        "skillButton", "skillBgImage", "skillBorderRing", "skillIcon",
                        "skillCooldownOverlay", "skillCooldownText", "skillBannerGroup",
                        "skillBannerText", "screenBorderFlashImage"
                    };

                    foreach (var prop in props)
                    {
                        var p = so.FindProperty(prop);
                        if (p != null && p.objectReferenceValue != null)
                        {
                            sb.AppendLine($"  [PASS] GameplayHUD.{prop} -> {p.objectReferenceValue.name}");
                        }
                        else
                        {
                            bindingsOk = false;
                            sb.AppendLine($"  [FAIL] GameplayHUD.{prop} bị null!");
                        }
                    }
                }
                else
                {
                    bindingsOk = false;
                    sb.AppendLine("  [FAIL] Không tìm thấy GameplayHUD!");
                }

                if (bindingsOk) passCount++;
            }

            // 5. KIỂM TRA RUNTIME SKILL SIMULATION VÀ TELEMETRY
            {
                sb.AppendLine("\n--- 5. KIỂM TRA MÔ PHỎNG SKILL & TELEMETRY ---");
                bool simOk = true;

                LevelSession.Reset();
                LevelSession.Initialize(1);

                PlayerController pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                GameObject tempEnv = null;
                if (pc == null)
                {
                    tempEnv = new GameObject("TempTestEnv");
                    pc = tempEnv.AddComponent<PlayerController>();
                }

                pc.LivesCount = 3;
                pc.InitializeStats();

                PlayerSkillController psc = pc.GetComponent<PlayerSkillController>();
                if (psc == null) psc = pc.gameObject.AddComponent<PlayerSkillController>();
                psc.Player = pc;
                psc.ResetCooldownForDebug();
                psc.LoadCurrentSkillConfig();

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeState(GameState.Playing);
                    GameManager.Instance.ChangeRunState(RunState.Playing);
                }

                try
                {

                    // Test Ready State
                    if (psc.IsSkillReady())
                    {
                        sb.AppendLine("  [PASS] IsSkillReady() ban đầu = True.");
                    }
                    else
                    {
                        simOk = false;
                        sb.AppendLine("  [FAIL] IsSkillReady() ban đầu không đúng!");
                    }

                    // Test Kim Lân
                    psc.ActivateSkillForDebug("1");
                    if (psc.IsSkillActive && pc.IsKimDashing && pc.SkillSpeedMultiplier >= 1.79f)
                    {
                        sb.AppendLine($"  [PASS] Kích hoạt Kim Lân: IsSkillActive=True, IsKimDashing=True, SpeedMultiplier={pc.SkillSpeedMultiplier}x.");
                    }
                    else
                    {
                        simOk = false;
                        sb.AppendLine($"  [FAIL] Kim Lân không kích hoạt đúng: active={psc.IsSkillActive}, dash={pc.IsKimDashing}, mult={pc.SkillSpeedMultiplier}");
                    }

                    // Test Cooldown & Ready
                    if (!psc.IsSkillReady())
                    {
                        sb.AppendLine("  [PASS] Khi skill đang kích hoạt, IsSkillReady() = False.");
                    }
                    else
                    {
                        simOk = false;
                        sb.AppendLine("  [FAIL] Khi skill active, IsSkillReady() phải = False!");
                    }

                    // Test Cancel on State Change
                    psc.CancelAllSkills();
                    if (!psc.IsSkillActive && !pc.IsKimDashing && pc.SkillSpeedMultiplier == 1.0f)
                    {
                        sb.AppendLine("  [PASS] CancelAllSkills() reset hoàn toàn: IsSkillActive=False, IsKimDashing=False, SpeedMultiplier=1.0x.");
                    }
                    else
                    {
                        simOk = false;
                        sb.AppendLine("  [FAIL] CancelAllSkills() không reset đầy đủ trạng thái!");
                    }
                }
                catch (Exception ex)
                {
                    simOk = false;
                    sb.AppendLine($"  [FAIL] Ngoại lệ khi test skill: {ex.Message}");
                }
                finally
                {
                    if (tempEnv != null) UnityEngine.Object.DestroyImmediate(tempEnv);
                }

                if (simOk) passCount++;
            }

            // 6. TỰ ĐỘNG CHẠY LEVEL PROGRESS SELF TEST (26/26)
            {
                sb.AppendLine("\n--- 6. CHẠY LEVEL PROGRESS SELF TEST ---");
                try
                {
                    LevelProgressSelfTest.RunAllTests();
                    sb.AppendLine("  [PASS] LevelProgressSelfTest.RunAllTests() hoàn tất sạch sẽ.");
                    passCount++;
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  [FAIL] Lỗi khi chạy SelfTest: {ex.Message}");
                }
            }

            sb.AppendLine("\n=================================================================");
            sb.AppendLine($"=== KẾT QUẢ XÁC MINH SKILL 5A.4: {passCount}/{totalChecks} MỤC PASS ===");
            sb.AppendLine("=================================================================");

            string report = sb.ToString();
            Debug.Log(report);
            string outPath = "D:/Dev_Game/DemHoiDenLong/Logs/verify_skill_phase5a4.txt";
            File.WriteAllText(outPath, report, Encoding.UTF8);
        }
    }
}
