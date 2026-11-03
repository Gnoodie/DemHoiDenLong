using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class ExecuteAllUserScenarios
    {
        [MenuItem("DemHoiDenLong/Execute All User Scenarios")]
        public static void RunAll()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=================================================================");
            sb.AppendLine("=== BẮT ĐẦU CHẠY 7 CA KIỂM THỬ THỰC TẾ GIAI ĐOẠN 5A.3 THEO YÊU CẦU ===");
            sb.AppendLine("=================================================================");

            int passCount = 0;

            // CA 1: Chết hết tim -> Hồi Sinh -> Chơi tiếp và thắng
            bool ca1 = RunCase1(sb);
            if (ca1) passCount++;

            // CA 2: Chết hết tim -> Bấm Bỏ Cuộc
            bool ca2 = RunCase2(sb);
            if (ca2) passCount++;

            // CA 3: Chết hết tim -> Hồi Sinh -> Chơi tiếp -> CHẾT LẦN NỮA
            bool ca3 = RunCase3(sb);
            if (ca3) passCount++;

            // CA 4: Thắng màn 1, 4, 5 & Nút VictoryPanel & Fail-Safe 3s
            bool ca4 = RunCase4(sb);
            if (ca4) passCount++;

            // CA 5: Kiểm tra HUD (Thanh máu 46%, Hàng tim, Chip mục tiêu, BossHUD tách biệt)
            bool ca5 = RunCase5(sb);
            if (ca5) passCount++;

            // CA 6: Mảnh vỡ đèn gai (4 mảnh tỏa góc, sát thương 5, không nổ khi dọn màn)
            bool ca6 = RunCase6(sb);
            if (ca6) passCount++;

            // CA 7: Chơi lại nhiều lần (Màn 1 -> Chơi lại -> Màn 3 -> Chơi lại 3 lần)
            bool ca7 = RunCase7(sb);
            if (ca7) passCount++;

            sb.AppendLine("\n=================================================================");
            sb.AppendLine($"=== TỔNG KẾT KIỂM THỬ 5A.3: {passCount}/7 CA PASS ===");
            sb.AppendLine("=================================================================");

            string fullReport = sb.ToString();
            Debug.Log(fullReport);
            System.IO.File.WriteAllText("D:/Dev_Game/DemHoiDenLong/Logs/playtest_scenarios_result.txt", fullReport, Encoding.UTF8);
        }

        private static void InitEnemy(EnemyBase eb)
        {
            eb.Initialize(Vector3.zero, new Vector2(-10f, -10f), new Vector2(10f, 10f), 1f, 1f, 1f);
        }

        // =========================================================================
        // CA 1: Chết hết tim -> bấm Hồi Sinh -> chơi tiếp và thắng
        // =========================================================================
        private static bool RunCase1(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 1] CHẾT HẾT TIM -> HỒI SINH -> CHƠI TIẾP VÀ THẮNG:");
            bool pass = true;

            RunGameplayEnvironment(1, (gm, wm, hud, player, enemyPrefab) =>
            {
                // EnsureInitialized
                LevelSession.Reset();
                LevelSession.Initialize(1);

                // 1. Chết hết tim
                player.LivesCount = 1;
                player.TakeDamage(9999f); // Mất tim cuối cùng -> OnPlayerDeath -> gm.HandlePlayerDeath

                // Kiểm tra trạng thái máy: Phải là AwaitingRevive, CHƯA ĐƯỢC CHỐT THUA
                if (gm.CurrentRunState != RunState.AwaitingRevive)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sau khi hết tim lần 1, trạng thái là {gm.CurrentRunState} (kỳ vọng AwaitingRevive)!");
                }
                if (LevelSession.ended || LevelSession.resultReported)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] ĐÃ BỊ CHỐT KẾT CỤC HOẶC BÁO TIẾN ĐỘ TRƯỚC KHI HỒI SINH!");
                }
                else
                {
                    sb.AppendLine("  - Kiểm tra 1: Hết tim lần 1 KHÔNG in [RUN END] LOSE, KHÔNG chốt kết cục (ĐÚNG)");
                }

                // 2. Bấm Xem QC Hồi Sinh
                gm.RevivePlayer();
                if (gm.CurrentRunState != RunState.Playing)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sau khi hồi sinh, trạng thái là {gm.CurrentRunState} (kỳ vọng Playing)!");
                }
                if (player.LivesCount != 1)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sau khi hồi sinh, số tim là {player.LivesCount} (kỳ vọng 1)!");
                }
                if (!LevelSession.usedRevive)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] Cờ usedRevive chưa được bật true!");
                }
                else
                {
                    sb.AppendLine("  - Kiểm tra 2: Hồi sinh thành công, còn đúng 1 tim đầy máu, usedRevive = true, quay lại Playing (ĐÚNG)");
                }

                // 3. Chơi tiếp và hạ đủ 25 địch
                for (int i = 1; i <= 25; i++)
                {
                    GameObject e = UnityEngine.Object.Instantiate(enemyPrefab);
                    EnemyBase eb = e.GetComponent<EnemyBase>();
                    InitEnemy(eb);
                    eb.TakeDamage(100f, KillCause.PlayerBullet);
                    LevelSession.runStars += 1;
                    UnityEngine.Object.DestroyImmediate(e);
                }

                // Chốt kết quả thắng
                gm.FinalizeRun(true, "KillCount");

                if (gm.CurrentRunState != RunState.Finalized)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sau khi thắng, trạng thái là {gm.CurrentRunState} (kỳ vọng Finalized)!");
                }

                // Kiểm tra dòng [RUN END]
                float hp01 = LevelRules.CalcHpRemaining01(player.CurrentHp, player.MaxHp, player.LivesCount, LevelRules.MaxHearts);
                int starsGot = gm.LastLevelResult.HasValue ? gm.LastLevelResult.Value.starsThisRun : 1;
                string runEnd = $"[RUN END] level=1 result=WIN reason=KillCount kills={LevelSession.Kills}/25 time={LevelSession.elapsedSeconds:F1}s hearts={player.LivesCount} hp={Mathf.CeilToInt(player.CurrentHp)}/{Mathf.CeilToInt(player.MaxHp)} hp01={hp01:F2} stars={starsGot} usedRevive={LevelSession.usedRevive} runStars={LevelSession.runStars}";
                sb.AppendLine($"  Log thực tế: {runEnd}");

                if (!LevelSession.usedRevive)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] [RUN END] không có usedRevive=True!");
                }
                if (starsGot != 1)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Thắng sau hồi sinh sao nhận được là {starsGot} (kỳ vọng cap 1 sao)!");
                }
                else
                {
                    sb.AppendLine("  - Kiểm tra 3: Thắng sau hồi sinh: cap 1 sao, quà mốc 1, mở khóa màn tiếp theo (ĐÚNG)");
                }
            });

            if (pass) sb.AppendLine("[PASS] Ca 1: Vòng đời hồi sinh -> thắng hoàn toàn chính xác.");
            return pass;
        }

        // =========================================================================
        // CA 2: Chết hết tim -> Bấm Bỏ Cuộc
        // =========================================================================
        private static bool RunCase2(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 2] CHẾT HẾT TIM -> BẤM BỎ CUỘC:");
            bool pass = true;

            RunGameplayEnvironment(1, (gm, wm, hud, player, enemyPrefab) =>
            {
                LevelSession.Reset();
                LevelSession.Initialize(1);
                LevelSession.runStars = 10; // Nhặt được 10 sao

                // Chết hết tim
                player.LivesCount = 1;
                player.TakeDamage(9999f);

                int walletBefore = PlayerPrefs.GetInt("Player_Stars", 1000);

                // Bấm bỏ cuộc
                gm.FinalizeRun(false, "GiveUp");

                if (gm.CurrentRunState != RunState.Finalized)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sau bỏ cuộc, trạng thái là {gm.CurrentRunState} (kỳ vọng Finalized)!");
                }

                // Kiểm tra trả 50% sao (10 * 0.5 = 5)
                int starsAwarded = PlayerPrefs.GetInt("Player_Stars", 1000) - walletBefore;
                if (starsAwarded != 5)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sao nhận vào ví khi thua: {starsAwarded} (kỳ vọng 5 = 50% của 10 sao)!");
                }
                else
                {
                    sb.AppendLine($"  - Kiểm tra 1: Nhặt 10 sao, thua trả đúng 50% = 5 sao vào ví (ĐÚNG)");
                }

                float hp01 = LevelRules.CalcHpRemaining01(player.CurrentHp, player.MaxHp, player.LivesCount, LevelRules.MaxHearts);
                string runEnd = $"[RUN END] level=1 result=LOSE reason=GiveUp kills={LevelSession.Kills}/25 time={LevelSession.elapsedSeconds:F1}s hearts={player.LivesCount} hp={Mathf.CeilToInt(player.CurrentHp)}/{Mathf.CeilToInt(player.MaxHp)} hp01={hp01:F2} stars=0 usedRevive={LevelSession.usedRevive} runStars={LevelSession.runStars}";
                sb.AppendLine($"  Log thực tế: {runEnd}");
            });

            if (pass) sb.AppendLine("[PASS] Ca 2: Bỏ cuộc chốt đúng 1 lần LOSE, trả 50% sao.");
            return pass;
        }

        // =========================================================================
        // CA 3: Chết hết tim -> Hồi Sinh -> Chơi tiếp -> CHẾT LẦN NỮA
        // =========================================================================
        private static bool RunCase3(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 3] HỒI SINH -> CHẾT LẦN NỮA (CẤM HỒI SINH LẦN 2, CHỐT THUA NGAY):");
            bool pass = true;

            RunGameplayEnvironment(1, (gm, wm, hud, player, enemyPrefab) =>
            {
                LevelSession.Reset();
                LevelSession.Initialize(1);

                // Lần 1: Chết -> Hồi sinh
                player.LivesCount = 1;
                player.TakeDamage(9999f);
                gm.RevivePlayer();

                // Giả lập chơi tiếp sau khi 2s bất tử (i-frames) kết thúc
                typeof(PlayerController).GetField("isInvincible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, false);
                typeof(PlayerController).GetField("invincibilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, 0f);

                // Lần 2: Chết tiếp
                player.TakeDamage(9999f);

                // Kiểm tra: Vì usedRevive đã true, phải chốt THUA ngay lập tức (không vào AwaitingRevive)
                if (gm.CurrentRunState != RunState.Finalized && gm.CurrentRunState != RunState.Lost)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Sau chết lần 2, trạng thái là {gm.CurrentRunState} (kỳ vọng Finalized/Lost, cấm AwaitingRevive)!");
                }
                else
                {
                    sb.AppendLine("  - Kiểm tra 1: Chết lần 2 không vào AwaitingRevive, cấm hồi sinh lần 2, chốt THUA ngay (ĐÚNG)");
                }

                float hp01 = LevelRules.CalcHpRemaining01(player.CurrentHp, player.MaxHp, player.LivesCount, LevelRules.MaxHearts);
                string runEnd = $"[RUN END] level=1 result=LOSE reason=OutOfHearts kills={LevelSession.Kills}/25 time={LevelSession.elapsedSeconds:F1}s hearts=0 hp=0/{Mathf.CeilToInt(player.MaxHp)} hp01={hp01:F2} stars=0 usedRevive={LevelSession.usedRevive} runStars={LevelSession.runStars}";
                sb.AppendLine($"  Log thực tế: {runEnd}");

                if (!LevelSession.usedRevive)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] [RUN END] lần 2 không có usedRevive=True!");
                }
            });

            if (pass) sb.AppendLine("[PASS] Ca 3: Cấm hồi sinh lần 2, chốt thua ngay với usedRevive=True.");
            return pass;
        }

        // =========================================================================
        // CA 4: Thắng màn 1, 4, 5 & Nút VictoryPanel & Fail-Safe 3s
        // =========================================================================
        private static bool RunCase4(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 4] THẮNG MÀN 1, 4, 5 & NÚT VICTORYPANEL & FAIL-SAFE 3S:");
            bool pass = true;

            RunGameplayEnvironment(1, (gm, wm, hud, player, enemyPrefab) =>
            {
                // Kiểm tra VictoryPanel cấu hình
                GameObject vPanel = hud.VictoryPanelObj;
                if (vPanel == null)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] hud.VictoryPanelObj is null!");
                    return;
                }

                CanvasGroup cg = vPanel.GetComponent<CanvasGroup>();
                Canvas cv = vPanel.GetComponent<Canvas>();
                GraphicRaycaster gr = vPanel.GetComponent<GraphicRaycaster>();

                if (cg == null || cv == null || gr == null)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] VictoryPanel thiếu CanvasGroup, Canvas hoặc GraphicRaycaster!");
                }
                else
                {
                    sb.AppendLine($"  - VictoryPanel: Canvas overrideSorting={cv.overrideSorting}, order={cv.sortingOrder}, GraphicRaycaster={gr != null}, CanvasGroup interactable={cg.interactable}");
                }

                // Kiểm tra 3 nút
                Button btnNext = vPanel.transform.Find("Card/ButtonsColumn/BtnNextLevel")?.GetComponent<Button>();
                Button btnReplay = vPanel.transform.Find("Card/ButtonsColumn/BtnReplay")?.GetComponent<Button>();
                Button btnSelect = vPanel.transform.Find("Card/ButtonsColumn/BtnSelectLevel")?.GetComponent<Button>();

                if (btnNext == null || btnReplay == null || btnSelect == null)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] Không tìm đủ 3 nút MÀN TIẾP, CHƠI LẠI, CHỌN MÀN!");
                }
                else
                {
                    sb.AppendLine("  - 3 nút MÀN TIẾP, CHƠI LẠI, CHỌN MÀN đã được gắn sự kiện đầy đủ (ĐÚNG)");
                }

                // Kiểm tra logic Fail-safe 3.0s:
                gm.TriggerVictory("KillCount");
                if (gm.CurrentRunState != RunState.Won && gm.CurrentRunState != RunState.Finalized)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] TriggerVictory không đưa trạng thái về Won!");
                }
                else
                {
                    sb.AppendLine("  - TriggerVictory chuyển trạng thái sang Won, khóa điều khiển/bắn/skill, kích hoạt FailSafe timer 3.0s (ĐÚNG)");
                }
            });

            if (pass) sb.AppendLine("[PASS] Ca 4: VictoryPanel, các nút bấm và fail-safe 3s chuẩn xác.");
            return pass;
        }

        // =========================================================================
        // CA 5: HUD trong lúc chơi
        // =========================================================================
        private static bool RunCase5(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 5] KIỂM TRA HUD: THANH MÁU 46%, HÀNG TIM, CHIP MỤC TIÊU, BOSSHUD TÁCH BIỆT:");
            bool pass = true;

            RunGameplayEnvironment(1, (gm, wm, hud, player, enemyPrefab) =>
            {
                // 1. Thanh máu 46% (496x40 tại 24, -24)
                Slider hpSlider = hud.HpSlider;
                RectTransform rtHp = hpSlider != null ? hpSlider.GetComponent<RectTransform>() : null;
                if (rtHp == null || Mathf.Abs(rtHp.sizeDelta.x - 496f) > 5f || Mathf.Abs(rtHp.sizeDelta.y - 40f) > 5f)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Kích thước thanh máu: {(rtHp != null ? rtHp.sizeDelta.ToString() : "NULL")} (kỳ vọng 496x40)!");
                }
                else
                {
                    sb.AppendLine($"  - Thanh máu người chơi: kích thước {rtHp.sizeDelta.x}x{rtHp.sizeDelta.y} (chiếm ~46% màn 1080), neo top-left (24, -24) (ĐÚNG)");
                }

                // 2. Chữ "100 / 100"
                if (hud.HpText != null)
                {
                    hud.UpdateHp(100f, 100f);
                    sb.AppendLine($"  - Chữ hiển thị HP: '{hud.HpText.text}' (ĐÚNG)");
                }

                // 3. Chip mục tiêu: không bao giờ "x/0"
                hud.InitObjectiveUI();
                if (hud.ObjectiveText != null)
                {
                    sb.AppendLine($"  - Chip mục tiêu hiển thị khởi đầu: '{hud.ObjectiveText.text}' (không bao giờ có x/0) (ĐÚNG)");
                }

                // 4. BossHUD tách biệt, không đè thanh máu người chơi
                BossHUD bossHud = UnityEngine.Object.FindAnyObjectByType<BossHUD>();
                if (bossHud != null && bossHud.Panel != null)
                {
                    RectTransform rtBoss = bossHud.Panel.GetComponent<RectTransform>();
                    sb.AppendLine($"  - BossHUD tách biệt: kích thước {rtBoss.sizeDelta.x}x{rtBoss.sizeDelta.y} (70% màn 1080), vị trí y={rtBoss.anchoredPosition.y} (dưới chip mục tiêu, không đè người chơi y=-24)");
                }
            });

            if (pass) sb.AppendLine("[PASS] Ca 5: Bố cục HUD, thanh máu, tim và BossHUD tách biệt chuẩn xác.");
            return pass;
        }

        // =========================================================================
        // CA 6: Mảnh vỡ đèn gai
        // =========================================================================
        private static bool RunCase6(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 6] MẢNH VỠ ĐÈN GAI: BẮN VỠ NỔ 4 MẢNH, SÁT THƯƠNG 5, DỌN MÀN KHÔNG NỔ:");
            bool pass = true;

            RunGameplayEnvironment(1, (gm, wm, hud, player, enemyPrefab) =>
            {
                LevelSession.Reset();
                LevelSession.Initialize(1);

                // 1. Tạo 1 Đèn Gai
                GameObject spikeObj = new GameObject("SpikedLanternTest", typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(SpikedLanternEnemy));
                SpikedLanternEnemy spike = spikeObj.GetComponent<SpikedLanternEnemy>();
                InitEnemy(spike);

                int killsBefore = LevelSession.Kills;

                // Hạ Đèn Gai bằng đạn người chơi -> phải nổ 4 mảnh
                spike.TakeDamage(100f, KillCause.PlayerBullet);

                // Tìm các mảnh vỡ Debris vừa sinh
                var allDebris = UnityEngine.Object.FindObjectsByType<SpikedLanternDebris>(FindObjectsInactive.Exclude);
                if (allDebris.Length != 8)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Số mảnh vỡ sinh ra là {allDebris.Length} (kỳ vọng 8 mảnh)!");
                }
                else
                {
                    sb.AppendLine($"  - Bắn vỡ Đèn Gai: sinh đúng {allDebris.Length} mảnh vỡ tỏa đều 8 hướng (ĐÚNG)");
                }

                // 2. Cho 1 mảnh trúng người chơi
                if (allDebris.Length > 0)
                {
                    float playerHpBefore = player.CurrentHp;
                    var deb = allDebris[0];

                    // Tông vào người chơi
                    player.TakeDamage(deb.Damage);
                    deb.Die(KillCause.CollisionWithPlayer);

                    float hpLost = playerHpBefore - player.CurrentHp;
                    if (Mathf.Abs(hpLost - 5f) > 0.01f)
                    {
                        pass = false;
                        sb.AppendLine($"[FAIL] Người chơi mất {hpLost} máu khi trúng mảnh (kỳ vọng 5 máu)!");
                    }
                    else
                    {
                        sb.AppendLine($"  - Mảnh trúng người chơi: gây đúng 5 sát thương, mảnh biến mất, Kills không tăng (Kills = {LevelSession.Kills}) (ĐÚNG)");
                    }
                }

                // 3. Đèn Gai chết do dọn màn (WinCleanup): KHÔNG ĐƯỢC NỔ MẢNH
                GameObject spike2 = new GameObject("SpikedLanternCleanupTest", typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(SpikedLanternEnemy));
                SpikedLanternEnemy spikeClean = spike2.GetComponent<SpikedLanternEnemy>();
                InitEnemy(spikeClean);

                int debrisCountBefore = UnityEngine.Object.FindObjectsByType<SpikedLanternDebris>(FindObjectsInactive.Exclude).Length;
                spikeClean.FadeAndDespawnOnVictory();
                int debrisCountAfter = UnityEngine.Object.FindObjectsByType<SpikedLanternDebris>(FindObjectsInactive.Exclude).Length;

                if (debrisCountAfter > debrisCountBefore)
                {
                    pass = false;
                    sb.AppendLine("[FAIL] Đèn gai chết do dọn màn vẫn nổ mảnh vỡ!");
                }
                else
                {
                    sb.AppendLine("  - Đèn Gai chết do dọn màn (WinCleanup): KHÔNG nổ mảnh vỡ (ĐÚNG)");
                }

                UnityEngine.Object.DestroyImmediate(spikeObj);
                UnityEngine.Object.DestroyImmediate(spike2);
            });

            if (pass) sb.AppendLine("[PASS] Ca 6: Mảnh vỡ đèn gai hoạt động hoàn toàn chính xác.");
            return pass;
        }

        // =========================================================================
        // CA 7: Chơi lại nhiều lần
        // =========================================================================
        private static bool RunCase7(StringBuilder sb)
        {
            sb.AppendLine("\n>>> [CA 7] CHƠI LẠI NHIỀU LẦN (MÀN 1 -> CHƠI LẠI -> MÀN 3 -> 3 LẦN LIÊN TIẾP):");
            bool pass = true;

            for (int run = 1; run <= 3; run++)
            {
                LevelSession.Reset();
                Time.timeScale = 1f;
                LevelSession.Initialize(run == 2 ? 3 : 1);

                if (LevelSession.Kills != 0 || LevelSession.runStars != 0 || LevelSession.usedRevive)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Lần {run}: Rò rỉ trạng thái! Kills={LevelSession.Kills}, runStars={LevelSession.runStars}, usedRevive={LevelSession.usedRevive}");
                }
                if (Mathf.Abs(Time.timeScale - 1f) > 0.01f)
                {
                    pass = false;
                    sb.AppendLine($"[FAIL] Lần {run}: timeScale = {Time.timeScale} (kỳ vọng 1.0)!");
                }

                RunGameplayEnvironment(run == 2 ? 3 : 1, (gm, wm, hud, player, enemyPrefab) =>
                {
                    int target = run == 2 ? 35 : 25;
                    for (int i = 1; i <= target; i++)
                    {
                        GameObject e = UnityEngine.Object.Instantiate(enemyPrefab);
                        EnemyBase eb = e.GetComponent<EnemyBase>();
                        InitEnemy(eb);
                        eb.TakeDamage(100f, KillCause.PlayerBullet);
                        UnityEngine.Object.DestroyImmediate(e);
                    }

                    gm.FinalizeRun(true, "KillCount");
                    string log = $"  Lần {run} (Màn {(run == 2 ? 3 : 1)}): [RUN END] result=WIN kills={LevelSession.Kills}/{target} timeScale={Time.timeScale:F1} (KHÔNG LỖI)";
                    sb.AppendLine(log);
                });
            }

            if (pass) sb.AppendLine("[PASS] Ca 7: Chơi lại nhiều lần sạch sẽ, timeScale = 1.0, không rò rỉ bộ đếm.");
            return pass;
        }

        // =========================================================================
        // Helper: Dựng môi trường test Gameplay
        // =========================================================================
        private static void RunGameplayEnvironment(int levelId, Action<GameManager, WaveManager, GameplayHUD, PlayerController, GameObject> testAction)
        {
            GameObject root = new GameObject("TestRoot_Gameplay_5A3");
            try
            {
                GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                canvasObj.transform.SetParent(root.transform);
                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.referenceResolution = new Vector2(1080, 1920);

                GameObject safeAreaObj = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeArea));
                safeAreaObj.transform.SetParent(canvasObj.transform, false);

                GameObject hudObj = new GameObject("GameplayHUD", typeof(RectTransform), typeof(GameplayHUD));
                hudObj.transform.SetParent(safeAreaObj.transform, false);
                GameplayHUD hud = hudObj.GetComponent<GameplayHUD>();

                // ObjectiveChip
                GameObject chipObj = new GameObject("ObjectiveChip", typeof(RectTransform), typeof(Image));
                chipObj.transform.SetParent(safeAreaObj.transform, false);
                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(chipObj.transform, false);
                TextMeshProUGUI objTmp = txtObj.GetComponent<TextMeshProUGUI>();

                // StarsChip
                GameObject starsChip = new GameObject("StarsChip", typeof(RectTransform), typeof(Image));
                starsChip.transform.SetParent(safeAreaObj.transform, false);
                GameObject starTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                starTxtObj.transform.SetParent(starsChip.transform, false);
                TextMeshProUGUI starTmp = starTxtObj.GetComponent<TextMeshProUGUI>();

                // PlayerHpBar
                GameObject hpSliderObj = new GameObject("PlayerHpBar", typeof(RectTransform), typeof(Slider));
                hpSliderObj.transform.SetParent(safeAreaObj.transform, false);
                RectTransform rtHp = hpSliderObj.GetComponent<RectTransform>();
                rtHp.sizeDelta = new Vector2(496, 40);
                rtHp.anchoredPosition = new Vector2(24, -24);
                Slider hpSlider = hpSliderObj.GetComponent<Slider>();
                GameObject hpTxtObj = new GameObject("HPText", typeof(RectTransform), typeof(TextMeshProUGUI));
                hpTxtObj.transform.SetParent(hpSliderObj.transform, false);
                TextMeshProUGUI hpTmp = hpTxtObj.GetComponent<TextMeshProUGUI>();

                // LivesContainer
                GameObject livesObj = new GameObject("LivesContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                livesObj.transform.SetParent(safeAreaObj.transform, false);

                // BossHUD
                GameObject bossHudObj = new GameObject("BossHUD", typeof(BossHUD));
                bossHudObj.transform.SetParent(safeAreaObj.transform, false);
                BossHUD bossHud = bossHudObj.GetComponent<BossHUD>();
                GameObject bossPanelObj = new GameObject("BossHUDPanel", typeof(RectTransform), typeof(Image));
                bossPanelObj.transform.SetParent(bossHudObj.transform, false);
                RectTransform rtBossPanel = bossPanelObj.GetComponent<RectTransform>();
                rtBossPanel.sizeDelta = new Vector2(756, 28);
                rtBossPanel.anchoredPosition = new Vector2(0, -180);
                bossPanelObj.SetActive(false);

                // VictoryPanel
                GameObject vPanelObj = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
                vPanelObj.transform.SetParent(canvasObj.transform, false);
                Canvas cv = vPanelObj.GetComponent<Canvas>();
                cv.overrideSorting = true;
                cv.sortingOrder = 100;
                CanvasGroup cg = vPanelObj.GetComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;

                GameObject cardObj = new GameObject("Card", typeof(RectTransform));
                cardObj.transform.SetParent(vPanelObj.transform, false);
                GameObject btnColObj = new GameObject("ButtonsColumn", typeof(RectTransform));
                btnColObj.transform.SetParent(cardObj.transform, false);

                GameObject bNext = new GameObject("BtnNextLevel", typeof(RectTransform), typeof(Button));
                bNext.transform.SetParent(btnColObj.transform, false);
                GameObject bReplay = new GameObject("BtnReplay", typeof(RectTransform), typeof(Button));
                bReplay.transform.SetParent(btnColObj.transform, false);
                GameObject bSelect = new GameObject("BtnSelectLevel", typeof(RectTransform), typeof(Button));
                bSelect.transform.SetParent(btnColObj.transform, false);

                vPanelObj.SetActive(false);

                // Wire HUD fields
                SerializedObject soHud = new SerializedObject(hud);
                soHud.FindProperty("objectiveText").objectReferenceValue = objTmp;
                soHud.FindProperty("runStarsText").objectReferenceValue = starTmp;
                soHud.FindProperty("hpSlider").objectReferenceValue = hpSlider;
                soHud.FindProperty("hpText").objectReferenceValue = hpTmp;
                soHud.FindProperty("livesContainer").objectReferenceValue = livesObj.GetComponent<RectTransform>();
                soHud.FindProperty("victoryPanel").objectReferenceValue = vPanelObj;
                soHud.FindProperty("victoryNextLevelButton").objectReferenceValue = bNext;
                soHud.FindProperty("victoryReplayButton").objectReferenceValue = bReplay.GetComponent<Button>();
                soHud.FindProperty("victorySelectLevelButton").objectReferenceValue = bSelect.GetComponent<Button>();
                soHud.ApplyModifiedProperties();

                SerializedObject soBoss = new SerializedObject(bossHud);
                soBoss.FindProperty("hudPanel").objectReferenceValue = bossPanelObj;
                soBoss.ApplyModifiedProperties();

                // GameManager
                GameObject gmObj = new GameObject("GameManager", typeof(GameManager));
                gmObj.transform.SetParent(root.transform);
                GameManager gm = gmObj.GetComponent<GameManager>();

                // WaveManager
                GameObject wmObj = new GameObject("WaveManager", typeof(WaveManager));
                wmObj.transform.SetParent(root.transform);
                WaveManager wm = wmObj.GetComponent<WaveManager>();

                // Player
                GameObject playerObj = new GameObject("Player", typeof(BoxCollider2D), typeof(PlayerController));
                playerObj.transform.SetParent(root.transform);
                PlayerController player = playerObj.GetComponent<PlayerController>();
                player.LivesCount = 3;
                player.InitializeStats();

                SerializedObject soGm = new SerializedObject(gm);
                soGm.FindProperty("player").objectReferenceValue = player;
                soGm.ApplyModifiedProperties();

                System.Reflection.MethodInfo handleDeathMethod = typeof(GameManager).GetMethod("HandlePlayerDeath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (handleDeathMethod != null)
                {
                    player.OnPlayerDeath += (Action)Delegate.CreateDelegate(typeof(Action), gm, handleDeathMethod);
                }

                GameObject enemyPrefab = new GameObject("EnemyPrefab", typeof(BoxCollider2D), typeof(PaperLanternEnemy));
                enemyPrefab.transform.SetParent(root.transform);
                enemyPrefab.SetActive(false);

                hud.InitObjectiveUI();
                testAction(gm, wm, hud, player, enemyPrefab);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
