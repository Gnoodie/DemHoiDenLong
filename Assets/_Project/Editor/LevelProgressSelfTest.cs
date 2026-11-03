using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.UI;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.EditorTools
{
    public static class LevelProgressSelfTest
    {
        public class FakeRewardSink : IRewardSink
        {
            public bool isReady = true;
            public bool IsReady
            {
                get => isReady;
                set => isReady = value;
            }

            public bool IsAvailable
            {
                get => isReady;
                set => isReady = value;
            }

            public int TotalStars { get; private set; }
            public int TotalDiamonds { get; private set; }

            public bool failOnDiamond = false;
            public bool failOnStar = false;
            public bool failMidway = false;

            public void AddStar(int amount)
            {
                if (!IsReady || failOnStar) throw new InvalidOperationException("RewardSink failed on AddStar!");
                TotalStars += amount;
            }

            public void AddDiamond(int amount)
            {
                if (!IsReady || failOnDiamond) throw new InvalidOperationException("RewardSink failed on AddDiamond!");
                TotalDiamonds += amount;
            }

            public bool TryGrantReward(int starAmount, int diamondAmount)
            {
                if (!IsReady) return false;
                if (failOnStar && starAmount > 0) return false;
                if (failOnDiamond && diamondAmount > 0) return false;

                if (failMidway && starAmount > 0 && diamondAmount > 0)
                {
                    // Giả lập lỗi giữa chừng: nếu có cả sao lẫn kim cương thì từ chối cấp
                    // Đảm bảo không cấp một nửa (0 sao, 0 kim cương)
                    return false;
                }

                TotalStars += starAmount;
                TotalDiamonds += diamondAmount;
                return true;
            }

            public void Reset()
            {
                TotalStars = 0;
                TotalDiamonds = 0;
                isReady = true;
                failOnDiamond = false;
                failOnStar = false;
                failMidway = false;
            }
        }

        [MenuItem("DemHoiDenLong/Progress/Run Self Test")]
        public static void RunAllTests()
        {
            // Đảm bảo dữ liệu LevelDatabase đã được khởi tạo trước khi test
            SetupLevelData.SetupAll();

            LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");
            if (db == null)
            {
                Debug.LogError("[SelfTest] Không thể tải LevelDatabase asset để chạy test!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== BẮT ĐẦU CHẠY PROGRESS SELF TEST (27 CA) ===");

            int passedCount = 0;
            int totalTests = 27;

            // Helper tạo Manager độc lập trong bộ nhớ
            LevelProgressManager CreateTestManager(IProgressStorage storage, FakeRewardSink sink)
            {
                GameObject go = new GameObject("Test_LevelProgressManager");
                LevelProgressManager mgr = go.AddComponent<LevelProgressManager>();
                mgr.Initialize(db, storage, sink);
                return mgr;
            }

            void CleanupManager(LevelProgressManager mgr)
            {
                if (mgr != null && mgr.gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(mgr.gameObject);
                }
            }

            // -------------------------------------------------------------
            // Ca 1: Mới tinh: chỉ màn 1 mở, Hồi 1 mở, Hồi 2 và 3 khóa; Current = màn 1.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                bool c1 = mgr.IsLevelUnlocked(1) &&
                          !mgr.IsLevelUnlocked(2) &&
                          mgr.IsActUnlocked(0) &&
                          !mgr.IsActUnlocked(1) &&
                          !mgr.IsActUnlocked(2) &&
                          mgr.GetCurrentLevelId() == 1;

                if (c1) { passedCount++; sb.AppendLine("[PASS] Ca 1: Mới tinh -> Màn 1 mở, Hồi 1 mở, Hồi 2/3 khóa, Current = Màn 1."); }
                else { sb.AppendLine($"[FAIL] Ca 1: Mới tinh không đúng. Lvl1={mgr.IsLevelUnlocked(1)}, Lvl2={mgr.IsLevelUnlocked(2)}, Act0={mgr.IsActUnlocked(0)}, Act1={mgr.IsActUnlocked(1)}, Current={mgr.GetCurrentLevelId()}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 2: Thắng màn 1 với HP 40%: 1 sao, nhận 24 Sao; màn 2 mở; Current = màn 2.
            // -------------------------------------------------------------
            InMemoryProgressStorage sharedStorage234 = new InMemoryProgressStorage();
            FakeRewardSink sharedSink234 = new FakeRewardSink();
            LevelProgressManager mgr234 = CreateTestManager(sharedStorage234, sharedSink234);

            {
                var res = mgr234.ReportLevelResult(1, won: true, hpRemaining01: 0.40f);

                bool c2 = res.won &&
                          res.starsThisRun == 1 &&
                          res.starsGranted == 24 &&
                          sharedSink234.TotalStars == 24 &&
                          mgr234.IsLevelUnlocked(2) &&
                          mgr234.GetCurrentLevelId() == 2;

                if (c2) { passedCount++; sb.AppendLine("[PASS] Ca 2: Thắng màn 1 HP 40% -> 1 sao, nhận 24 Sao, màn 2 mở, Current = 2."); }
                else { sb.AppendLine($"[FAIL] Ca 2: starsThisRun={res.starsThisRun}, starsGranted={res.starsGranted}, sink={sharedSink234.TotalStars}, Lvl2Unlocked={mgr234.IsLevelUnlocked(2)}, Current={mgr234.GetCurrentLevelId()}"); }
            }

            // -------------------------------------------------------------
            // Ca 3: Chơi lại màn 1 với HP 90%: 3 sao, nhận thêm 26+28 = 54 Sao (tổng 78), newMax 3, isNewRecord.
            // -------------------------------------------------------------
            {
                var res = mgr234.ReportLevelResult(1, won: true, hpRemaining01: 0.90f);

                bool c3 = res.starsThisRun == 3 &&
                          res.previousMaxStars == 1 &&
                          res.newMaxStars == 3 &&
                          res.isNewRecord &&
                          res.starsGranted == 54 &&
                          sharedSink234.TotalStars == 78;

                if (c3) { passedCount++; sb.AppendLine("[PASS] Ca 3: Chơi lại màn 1 HP 90% -> 3 sao, nhận thêm 54 Sao (tổng 78), newMax=3, isNewRecord."); }
                else { sb.AppendLine($"[FAIL] Ca 3: starsThisRun={res.starsThisRun}, newMax={res.newMaxStars}, isNewRecord={res.isNewRecord}, granted={res.starsGranted}, sink={sharedSink234.TotalStars}"); }
            }

            // -------------------------------------------------------------
            // Ca 4: Chơi lại màn 1 với HP 30%: starsThisRun = 1 nhưng max vẫn 3, thưởng 0.
            // -------------------------------------------------------------
            {
                var res = mgr234.ReportLevelResult(1, won: true, hpRemaining01: 0.30f);

                bool c4 = res.starsThisRun == 1 &&
                          res.previousMaxStars == 3 &&
                          res.newMaxStars == 3 &&
                          !res.isNewRecord &&
                          res.starsGranted == 0 &&
                          sharedSink234.TotalStars == 78;

                if (c4) { passedCount++; sb.AppendLine("[PASS] Ca 4: Chơi lại màn 1 HP 30% -> starsThisRun=1, max vẫn 3, thưởng 0, sink giữ 78."); }
                else { sb.AppendLine($"[FAIL] Ca 4: starsThisRun={res.starsThisRun}, newMax={res.newMaxStars}, granted={res.starsGranted}, sink={sharedSink234.TotalStars}"); }

                CleanupManager(mgr234);
            }

            // -------------------------------------------------------------
            // Ca 5: Biên: HP 0.85 -> 3 sao; 0.8499 -> 2 sao; 0.50 -> 2; 0.4999 -> 1.
            // -------------------------------------------------------------
            {
                int s1 = LevelRewards.CalcStars(true, 0.85f);
                int s2 = LevelRewards.CalcStars(true, 0.8499f);
                int s3 = LevelRewards.CalcStars(true, 0.50f);
                int s4 = LevelRewards.CalcStars(true, 0.4999f);

                bool c5 = (s1 == 3) && (s2 == 2) && (s3 == 2) && (s4 == 1);
                if (c5) { passedCount++; sb.AppendLine("[PASS] Ca 5: Ngưỡng HP biên -> 0.85=3 sao, 0.8499=2 sao, 0.50=2 sao, 0.4999=1 sao."); }
                else { sb.AppendLine($"[FAIL] Ca 5: Sai ngưỡng: 0.85={s1}, 0.8499={s2}, 0.50={s3}, 0.4999={s4}"); }
            }

            // -------------------------------------------------------------
            // Ca 6: Thua: 0 sao, 0 thưởng, không mở gì thêm.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                var res = mgr.ReportLevelResult(1, won: false, hpRemaining01: 0.80f);

                bool c6 = !res.won &&
                          res.starsThisRun == 0 &&
                          res.starsGranted == 0 &&
                          res.diamondsGranted == 0 &&
                          mgr.GetStars(1) == 0 &&
                          !mgr.IsLevelUnlocked(2);

                if (c6) { passedCount++; sb.AppendLine("[PASS] Ca 6: Thua -> 0 sao, 0 thưởng, không mở khóa màn 2."); }
                else { sb.AppendLine($"[FAIL] Ca 6: won={res.won}, stars={res.starsThisRun}, granted={res.starsGranted}, Lvl2Unlocked={mgr.IsLevelUnlocked(2)}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 7: Màn 5 (Boss) thắng 3 sao lần đầu: nhận 40+50+60 = 150 Sao và 5 Kim Cương; unlockedNextAct = true; màn 6 mở; Hồi 2 mở.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                // Giả lập vượt màn 1..4 (1 sao mỗi màn) để mở tới màn 5
                for (int i = 1; i <= 4; i++)
                {
                    mgr.ReportLevelResult(i, won: true, hpRemaining01: 0.40f);
                }

                int starsBefore = sink.TotalStars;
                var res = mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.90f);

                bool c7 = res.starsThisRun == 3 &&
                          res.starsGranted == 150 &&
                          res.diamondsGranted == 5 &&
                          res.unlockedNextAct &&
                          mgr.IsLevelUnlocked(6) &&
                          mgr.IsActUnlocked(1);

                if (c7) { passedCount++; sb.AppendLine("[PASS] Ca 7: Boss màn 5 thắng 3 sao lần đầu -> nhận 150 Sao, 5 Kim Cương, unlockedNextAct=true, Màn 6 mở, Hồi 2 mở."); }
                else { sb.AppendLine($"[FAIL] Ca 7: starsGranted={res.starsGranted}, diaGranted={res.diamondsGranted}, nextAct={res.unlockedNextAct}, Lvl6={mgr.IsLevelUnlocked(6)}, Act1={mgr.IsActUnlocked(1)}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 8: Màn 5 thắng chỉ 1 sao: nhận 40 Sao, 0 Kim Cương, vẫn mở Hồi 2. Sau đó thắng 3 sao: nhận 50+60 = 110 Sao và 5 Kim Cương.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                for (int i = 1; i <= 4; i++) mgr.ReportLevelResult(i, won: true, hpRemaining01: 0.40f);

                var res1 = mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.40f); // 1 sao
                bool part1 = res1.starsThisRun == 1 &&
                             res1.starsGranted == 40 &&
                             res1.diamondsGranted == 0 &&
                             mgr.IsActUnlocked(1);

                var res2 = mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.90f); // 3 sao
                bool part2 = res2.starsThisRun == 3 &&
                             res2.starsGranted == 110 &&
                             res2.diamondsGranted == 5;

                if (part1 && part2) { passedCount++; sb.AppendLine("[PASS] Ca 8: Boss màn 5 thắng 1 sao nhận 40 Sao 0 Kim Cương vẫn mở Hồi 2; sau đó 3 sao nhận 110 Sao và 5 Kim Cương."); }
                else { sb.AppendLine($"[FAIL] Ca 8: Part1={part1} (granted={res1.starsGranted}, dia={res1.diamondsGranted}, actUnlocked={mgr.IsActUnlocked(1)}), Part2={part2} (granted={res2.starsGranted}, dia={res2.diamondsGranted})"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 9: Thưởng đúng: màn 8 = 52/68/84; màn 10 3 sao = 240 Sao + 10 Kim Cương; màn 15 3 sao = 330 Sao + 15 Kim Cương.
            // -------------------------------------------------------------
            {
                bool m8 = LevelRewards.StarReward(8, 1) == 52 &&
                          LevelRewards.StarReward(8, 2) == 68 &&
                          LevelRewards.StarReward(8, 3) == 84 &&
                          LevelRewards.DiamondReward(8, 3) == 0;

                int m10Stars = LevelRewards.StarReward(10, 1) + LevelRewards.StarReward(10, 2) + LevelRewards.StarReward(10, 3);
                int m10Dia = LevelRewards.DiamondReward(10, 3);
                bool m10 = (m10Stars == 240) && (m10Dia == 10);

                int m15Stars = LevelRewards.StarReward(15, 1) + LevelRewards.StarReward(15, 2) + LevelRewards.StarReward(15, 3);
                int m15Dia = LevelRewards.DiamondReward(15, 3);
                bool m15 = (m15Stars == 330) && (m15Dia == 15);

                if (m8 && m10 && m15) { passedCount++; sb.AppendLine("[PASS] Ca 9: Thưởng chuẩn -> Màn 8 (52/68/84), Màn 10 (240 Sao + 10 KC), Màn 15 (330 Sao + 15 KC)."); }
                else { sb.AppendLine($"[FAIL] Ca 9: m8={m8}, m10={m10} (s={m10Stars}, d={m10Dia}), m15={m15} (s={m15Stars}, d={m15Dia})"); }
            }

            // -------------------------------------------------------------
            // Ca 10: Hồi 3 KHÔNG mở khi Boss màn 10 chưa >= 1 sao, mở ngay khi có.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                for (int i = 1; i <= 9; i++) mgr.SetStarsDirect(i, 1);

                bool before = !mgr.IsActUnlocked(2);
                mgr.ReportLevelResult(10, won: true, hpRemaining01: 0.40f);
                bool after = mgr.IsActUnlocked(2);

                if (before && after) { passedCount++; sb.AppendLine("[PASS] Ca 10: Hồi 3 chỉ mở khi Boss màn 10 đạt >= 1 sao."); }
                else { sb.AppendLine($"[FAIL] Ca 10: Before={before}, After={after}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 11: Qua toàn bộ 15 màn 3 sao: tổng thưởng đúng 3060 Sao + 30 Kim Cương; GetTotalStars = 45; mọi Hồi Completed; không còn màn Current.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                for (int i = 1; i <= 15; i++)
                {
                    mgr.ReportLevelResult(i, won: true, hpRemaining01: 0.90f);
                }

                bool c11 = sink.TotalStars == 3060 &&
                           sink.TotalDiamonds == 30 &&
                           mgr.GetTotalStars() == 45 &&
                           mgr.GetActStatus(0) == ActStatus.Completed &&
                           mgr.GetActStatus(1) == ActStatus.Completed &&
                           mgr.GetActStatus(2) == ActStatus.Completed &&
                           mgr.GetCurrentLevelId() == -1;

                if (c11) { passedCount++; sb.AppendLine("[PASS] Ca 11: Toàn bộ 15 màn 3 sao -> Tổng 3060 Sao, 30 KC, 45 LevelStars, cả 3 Hồi Completed, không còn Current."); }
                else { sb.AppendLine($"[FAIL] Ca 11: stars={sink.TotalStars}, dia={sink.TotalDiamonds}, totalStars={mgr.GetTotalStars()}, current={mgr.GetCurrentLevelId()}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 12: Bền vững: tạo manager mới trên CÙNG storage thì trạng thái giữ nguyên (sao, mốc đã nhận, mở khóa).
            // -------------------------------------------------------------
            {
                var sharedStorage = new InMemoryProgressStorage();
                var sink1 = new FakeRewardSink();
                var mgr1 = CreateTestManager(sharedStorage, sink1);

                mgr1.ReportLevelResult(1, won: true, hpRemaining01: 0.90f); // 3 sao (nhận mốc 1,2,3)
                mgr1.ReportLevelResult(2, won: true, hpRemaining01: 0.50f); // 2 sao (nhận mốc 1,2)

                CleanupManager(mgr1);

                var sink2 = new FakeRewardSink();
                var mgr2 = CreateTestManager(sharedStorage, sink2);

                bool c12 = mgr2.GetStars(1) == 3 &&
                           mgr2.GetStars(2) == 2 &&
                           mgr2.IsMilestoneClaimed(1, 1) &&
                           mgr2.IsMilestoneClaimed(1, 3) &&
                           mgr2.IsMilestoneClaimed(2, 2) &&
                           !mgr2.IsMilestoneClaimed(2, 3) &&
                           mgr2.IsLevelUnlocked(3);

                if (c12) { passedCount++; sb.AppendLine("[PASS] Ca 12: Bền vững -> Manager mới dùng chung storage đọc đúng chính xác sao, mốc đã nhận và mở khóa."); }
                else { sb.AppendLine($"[FAIL] Ca 12: Lvl1Stars={mgr2.GetStars(1)}, Lvl2Stars={mgr2.GetStars(2)}, M2Claimed3={mgr2.IsMilestoneClaimed(2, 3)}, Lvl3Unlocked={mgr2.IsLevelUnlocked(3)}"); }

                CleanupManager(mgr2);
            }

            // -------------------------------------------------------------
            // Ca 13: Rủi ro: EconomyRewardSink lỗi (giả lập rewardFailed): mốc KHÔNG bị đánh dấu đã nhận, lần báo kế tiếp vẫn nhận được.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                sink.IsAvailable = false; // Giả lập EconomyManager null / unavailable

                var mgr = CreateTestManager(storage, sink);

                var resFailed = mgr.ReportLevelResult(1, won: true, hpRemaining01: 0.90f);

                bool step1 = resFailed.rewardFailed &&
                             !mgr.IsMilestoneClaimed(1, 1) &&
                             mgr.GetStars(1) == 3;

                // Phục hồi sink hoạt động lại
                sink.IsAvailable = true;
                var resRecovered = mgr.ReportLevelResult(1, won: true, hpRemaining01: 0.90f);

                bool step2 = !resRecovered.rewardFailed &&
                             resRecovered.starsGranted == 78 &&
                             mgr.IsMilestoneClaimed(1, 1) &&
                             mgr.IsMilestoneClaimed(1, 3);

                if (step1 && step2) { passedCount++; sb.AppendLine("[PASS] Ca 13: Rủi ro EconomyRewardSink lỗi -> rewardFailed=true, mốc chưa đánh dấu nhận; lần sau nhận bù đủ 78 Sao."); }
                else { sb.AppendLine($"[FAIL] Ca 13: Step1={step1} (failed={resFailed.rewardFailed}, m1={mgr.IsMilestoneClaimed(1, 1)}), Step2={step2} (granted={resRecovered.starsGranted})"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 14: LevelRewards.GetTotalFirstClearRewards() trả đúng 3060 và 30.
            // -------------------------------------------------------------
            {
                var (stars, diamonds) = LevelRewards.GetTotalFirstClearRewards(15);
                bool c14 = (stars == 3060) && (diamonds == 30);

                if (c14) { passedCount++; sb.AppendLine("[PASS] Ca 14: LevelRewards.GetTotalFirstClearRewards(15) trả đúng 3060 Sao và 30 Kim Cương."); }
                else { sb.AppendLine($"[FAIL] Ca 14: Trả về stars={stars}, diamonds={diamonds}"); }
            }

            // -------------------------------------------------------------
            // Ca 15: Dữ liệu: LevelDatabase có đúng 3 Hồi, 15 màn, Boss ở màn 5/10/15, hệ số khớp bảng Excel.
            // -------------------------------------------------------------
            {
                bool c15 = db.ActCount == 3 && db.TotalLevels == 15;

                LevelData l1 = db.GetLevel(1);
                LevelData l5 = db.GetLevel(5);
                LevelData l10 = db.GetLevel(10);
                LevelData l15 = db.GetLevel(15);

                bool match1 = l1 != null && !l1.isBoss && Mathf.Approximately(l1.hpMultiplier, 1.00f) && l1.enemyCount == 25;
                bool match5 = l5 != null && l5.isBoss && l5.bossName == "Đèn Tàn Trùm Làng" && Mathf.Approximately(l5.hpMultiplier, 1.32f) && l5.enemyCount == 33;
                bool match10 = l10 != null && l10.isBoss && l10.bossName == "Rồng Đèn Phố Cổ" && Mathf.Approximately(l10.hpMultiplier, 1.72f) && l10.enemyCount == 43;
                bool match15 = l15 != null && l15.isBoss && l15.bossName == "Thỏ Ngọc Hóa Quái" && Mathf.Approximately(l15.hpMultiplier, 2.12f) && l15.enemyCount == 53;

                if (c15 && match1 && match5 && match10 && match15) { passedCount++; sb.AppendLine("[PASS] Ca 15: Dữ liệu LevelDatabase khớp 100% GDD Excel (3 Hồi, 15 màn, Boss 5/10/15)."); }
                else { sb.AppendLine($"[FAIL] Ca 15: c15={c15}, match1={match1}, match5={match5}, match10={match10}, match15={match15}"); }
            }

            // -------------------------------------------------------------
            // Ca 16 (Yêu cầu 5a): Sink lỗi khi thắng màn 5 với 3 sao: kỷ lục lưu 3 sao, không mốc nào bị đánh dấu nhận.
            // Sink sẵn sàng lại rồi gọi ClaimPendingRewards: nhận đủ 150 Sao và 5 Kim Cương, gọi lại lần nữa thì 0.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                sink.isReady = false; // Sink lỗi ban đầu

                var mgr = CreateTestManager(storage, sink);

                // Mở khóa tới màn 5 (màn 1..4 đạt 1 sao và đã nhận mốc 1 để không lẫn vào ClaimPendingRewards của màn 5)
                for (int i = 1; i <= 4; i++)
                {
                    mgr.SetStarsDirect(i, 1);
                    mgr.SetMilestoneClaimedDirect(i, 1, true);
                }

                var resFailed = mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.90f);

                bool step1 = resFailed.rewardFailed &&
                             mgr.GetStars(5) == 3 &&
                             !mgr.IsMilestoneClaimed(5, 1) &&
                             !mgr.IsMilestoneClaimed(5, 2) &&
                             !mgr.IsMilestoneClaimed(5, 3);

                // Sink sẵn sàng trở lại
                sink.isReady = true;
                var claimRes1 = mgr.ClaimPendingRewards();

                bool step2 = !claimRes1.rewardFailed &&
                             claimRes1.totalStarsGranted == 150 &&
                             claimRes1.totalDiamondsGranted == 5 &&
                             claimRes1.totalMilestonesClaimed == 3 &&
                             mgr.IsMilestoneClaimed(5, 1) &&
                             mgr.IsMilestoneClaimed(5, 2) &&
                             mgr.IsMilestoneClaimed(5, 3);

                // Gọi lần hai: phải idempotent, trả về 0
                var claimRes2 = mgr.ClaimPendingRewards();
                bool step3 = claimRes2.IsEmpty &&
                             claimRes2.totalStarsGranted == 0 &&
                             claimRes2.totalDiamondsGranted == 0 &&
                             claimRes2.totalMilestonesClaimed == 0;

                if (step1 && step2 && step3) { passedCount++; sb.AppendLine("[PASS] Ca 16: Sink lỗi khi thắng màn 5 (3 sao) -> Kỷ lục 3 sao, 0 mốc nhận; Sink sẵn sàng -> ClaimPendingRewards nhận đủ 150 Sao 5 KC, lần 2 trả 0."); }
                else { sb.AppendLine($"[FAIL] Ca 16: Step1={step1}, Step2={step2} [failed={claimRes1.rewardFailed}, stars={claimRes1.totalStarsGranted}, dia={claimRes1.totalDiamondsGranted}, msCount={claimRes1.totalMilestonesClaimed}, m1={mgr.IsMilestoneClaimed(5,1)}, m2={mgr.IsMilestoneClaimed(5,2)}, m3={mgr.IsMilestoneClaimed(5,3)}], Step3={step3} [empty={claimRes2.IsEmpty}, s2Stars={claimRes2.totalStarsGranted}, s2Dia={claimRes2.totalDiamondsGranted}, s2Ms={claimRes2.totalMilestonesClaimed}]"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 17 (Yêu cầu 5b): Giống (a) nhưng thay vì ClaimPendingRewards, chơi lại màn 5 với 1 sao:
            // vẫn nhận đủ phần còn thiếu của kỷ lục 3 sao (không chỉ mốc 1).
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                sink.isReady = false; // Ban đầu lỗi

                var mgr = CreateTestManager(storage, sink);

                // Mở khóa tới màn 5
                for (int i = 1; i <= 4; i++) mgr.SetStarsDirect(i, 1);

                // Lần 1: thắng 3 sao khi sink lỗi
                mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.90f);

                // Sink hoạt động trở lại
                sink.isReady = true;

                // Chơi lại chỉ đạt 1 sao (HP 40%)
                var resReplay = mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.40f);

                bool c17 = resReplay.starsThisRun == 1 &&
                           resReplay.newMaxStars == 3 &&
                           resReplay.starsGranted == 150 &&
                           resReplay.diamondsGranted == 5 &&
                           mgr.IsMilestoneClaimed(5, 1) &&
                           mgr.IsMilestoneClaimed(5, 2) &&
                           mgr.IsMilestoneClaimed(5, 3) &&
                           sink.TotalStars == 150 &&
                           sink.TotalDiamonds == 5;

                if (c17) { passedCount++; sb.AppendLine("[PASS] Ca 17: Sau khi sink lỗi kỷ lục 3 sao, chơi lại chỉ đạt 1 sao -> Vẫn nhận đủ toàn bộ phần còn thiếu theo kỷ lục 3 sao (150 Sao, 5 KC)."); }
                else { sb.AppendLine($"[FAIL] Ca 17: StarsGranted={resReplay.starsGranted}, DiaGranted={resReplay.diamondsGranted}, SinkStars={sink.TotalStars}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 18 (Yêu cầu 5c): Sink lỗi giữa chừng không để cấp một nửa (ví dụ Sao được cấp mà Kim Cương không).
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();

                var mgr = CreateTestManager(storage, sink);

                // Mở khóa tới màn 5
                for (int i = 1; i <= 4; i++) mgr.SetStarsDirect(i, 1);

                // Màn 5: mốc 1 và 2 chỉ có Sao (40 và 50). Mốc 3 có cả Sao (60) và Kim Cương (5).
                // Giả lập lỗi giữa chừng: failMidway = true (từ chối khi mốc có cả 2 loại tiền tệ).
                sink.failMidway = true;

                var res = mgr.ReportLevelResult(5, won: true, hpRemaining01: 0.90f);

                // Mốc 1 và 2 thành công (40 + 50 = 90 sao).
                // Mốc 3 (60 sao + 5 kim cương) gặp lỗi giữa chừng -> KHÔNG được cấp một nửa, bitmask mốc 3 không được set!
                bool m1Claimed = mgr.IsMilestoneClaimed(5, 1);
                bool m2Claimed = mgr.IsMilestoneClaimed(5, 2);
                bool m3Claimed = mgr.IsMilestoneClaimed(5, 3);

                // Tổng sao chỉ tăng đúng 90 (không có 60 của mốc 3), kim cương là 0
                bool atomicHold = (sink.TotalStars == 90) && (sink.TotalDiamonds == 0) && !m3Claimed && res.rewardFailed;

                if (m1Claimed && m2Claimed && !m3Claimed && atomicHold) { passedCount++; sb.AppendLine("[PASS] Ca 18: Sink lỗi giữa chừng -> Đảm bảo tính nguyên tử: mốc 3 không cấp một nửa, không ghi nhận đã nhận."); }
                else { sb.AppendLine($"[FAIL] Ca 18: m1={m1Claimed}, m2={m2Claimed}, m3={m3Claimed}, stars={sink.TotalStars}, diamonds={sink.TotalDiamonds}, rewardFailed={res.rewardFailed}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 19 (Yêu cầu 5d): ClaimPendingRewards khi không còn gì thì trả rỗng.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();

                var mgr = CreateTestManager(storage, sink);

                // Chưa chơi màn nào (mọi màn sao = 0)
                var empty1 = mgr.ClaimPendingRewards();
                bool cEmpty1 = empty1.IsEmpty &&
                               empty1.totalStarsGranted == 0 &&
                               empty1.totalDiamondsGranted == 0 &&
                               empty1.claimedDetails.Count == 0 &&
                               !empty1.rewardFailed;

                // Sau khi đã nhận hết mốc của màn 1
                mgr.ReportLevelResult(1, won: true, hpRemaining01: 0.90f);
                var empty2 = mgr.ClaimPendingRewards();
                bool cEmpty2 = empty2.IsEmpty &&
                               empty2.totalStarsGranted == 0 &&
                               empty2.totalDiamondsGranted == 0 &&
                               empty2.claimedDetails.Count == 0 &&
                               !empty2.rewardFailed;

                if (cEmpty1 && cEmpty2) { passedCount++; sb.AppendLine("[PASS] Ca 19: ClaimPendingRewards khi không còn gì cần trả -> Trả về rỗng (IsEmpty = true, 0 Sao, 0 KC)."); }
                else { sb.AppendLine($"[FAIL] Ca 19: cEmpty1={cEmpty1}, cEmpty2={cEmpty2}"); }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 20: Kiểm chứng thưởng màn 2 (Yêu cầu Giai đoạn 4 - A1)
            // Mới tinh, thắng màn 1 (HP 90%, 3 sao) -> +78 Sao (24+26+28).
            // Thắng màn 2 (HP 60%, 2 sao) -> +60 Sao (mốc 1 = 28, mốc 2 = 32).
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                // 1. Mới tinh thắng màn 1 (HP 90% = 3 sao)
                var res1 = mgr.ReportLevelResult(1, won: true, hpRemaining01: 0.90f);
                bool cRes1 = res1.won &&
                             res1.starsThisRun == 3 &&
                             res1.starsGranted == 78 && // 24 + 26 + 28
                             res1.diamondsGranted == 0 &&
                             sink.TotalStars == 78;

                // 2. Thắng tiếp màn 2 (HP 60% = 2 sao)
                var res2 = mgr.ReportLevelResult(2, won: true, hpRemaining01: 0.60f);
                // Mốc 1: 20 + 2*4 = 28, Mốc 2: 20 + 2*6 = 32 -> Tổng = 60 Sao
                bool cRes2 = res2.won &&
                             res2.starsThisRun == 2 &&
                             res2.starsGranted == 60 && // 28 + 32
                             res2.diamondsGranted == 0 &&
                             sink.TotalStars == 138; // 78 + 60

                if (cRes1 && cRes2)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 20: Thưởng màn 2 đúng công thức (+60 Sao: M1=28, M2=32; tổng sau 2 màn: 138 Sao).");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 20: cRes1={cRes1} (stars={res1.starsGranted}, sink={sink.TotalStars}), cRes2={cRes2} (stars={res2.starsGranted})");
                }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 21: Báo thắng màn bị khóa (Yêu cầu Giai đoạn 4 - A2)
            // Khi màn 2 chưa có sao nào (màn 3 đang khóa): gọi ReportLevelResult(3, won: true, 0.9f)
            // -> Bị từ chối (rejected = true), 0 thưởng, 0 đổi tiến độ (GetStars(3) == 0).
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                // Mới tinh: Màn 1 mở, màn 2 khóa, màn 3 khóa.
                // Thắng màn 1 để mở màn 2, nhưng màn 2 chưa chơi (0 sao) -> Màn 3 chắc chắn khóa.
                mgr.ReportLevelResult(1, won: true, hpRemaining01: 0.90f);
                int starsBefore = sink.TotalStars;

                bool isM3Unlocked = mgr.IsLevelUnlocked(3);
                var res3 = mgr.ReportLevelResult(3, won: true, hpRemaining01: 0.90f);

                bool cRejected = !isM3Unlocked &&
                                 res3.rejected &&
                                 res3.starsGranted == 0 &&
                                 res3.diamondsGranted == 0 &&
                                 res3.newMaxStars == 0 &&
                                 sink.TotalStars == starsBefore &&
                                 mgr.GetStars(3) == 0;

                if (cRejected)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 21: Chặn thắng màn đang khóa -> rejected = true, 0 thưởng, tiến độ giữ nguyên.");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 21: isM3Unlocked={isM3Unlocked}, rejected={res3.rejected}, stars={res3.starsGranted}, getStars3={mgr.GetStars(3)}");
                }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 22: Công thức tính phần trăm tổng máu gộp thanh máu và mạng (Ca a)
            // (90, 90, 3) = 1.0; (90, 90, 2) ≈ 0.6667; (45, 90, 1) ≈ 0.1667; (0, 90, 1) = 0;
            // heartsLeft = 0 -> 0; maxHp = 0 -> 0 và không crash; Lân Vàng (150, 150, 3) = 1.0.
            // -------------------------------------------------------------
            {
                float hp1 = LevelRules.CalcHpRemaining01(90f, 90f, 3);
                float hp2 = LevelRules.CalcHpRemaining01(90f, 90f, 2);
                float hp3 = LevelRules.CalcHpRemaining01(45f, 90f, 1);
                float hp4 = LevelRules.CalcHpRemaining01(0f, 90f, 1);
                float hp5 = LevelRules.CalcHpRemaining01(90f, 90f, 0);
                float hp6 = LevelRules.CalcHpRemaining01(50f, 0f, 3);
                float hp7 = LevelRules.CalcHpRemaining01(150f, 150f, 3);

                bool passA = Mathf.Approximately(hp1, 1.0f) &&
                             Mathf.Abs(hp2 - 2f / 3f) < 0.001f &&
                             Mathf.Abs(hp3 - 45f / 270f) < 0.001f &&
                             Mathf.Approximately(hp4, 0f) &&
                             Mathf.Approximately(hp5, 0f) &&
                             Mathf.Approximately(hp6, 0f) &&
                             Mathf.Approximately(hp7, 1.0f);

                if (passA)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 22: Công thức CalcHpRemaining01 chính xác trên mọi biên máu và mạng.");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 22: hp1={hp1}, hp2={hp2}, hp3={hp3}, hp4={hp4}, hp5={hp5}, hp6={hp6}, hp7={hp7}");
                }
            }

            // -------------------------------------------------------------
            // Ca 23: Giới hạn sao khi dùng hồi sinh quảng cáo (Ca b)
            // Thắng, 0.9, usedRevive = false -> 3 sao; thắng, 0.9, usedRevive = true -> 1 sao;
            // Thua kèm usedRevive -> 0 sao.
            // -------------------------------------------------------------
            {
                int s1 = LevelRewards.CalcStars(won: true, hpRemaining01: 0.9f, usedRevive: false);
                int s2 = LevelRewards.CalcStars(won: true, hpRemaining01: 0.9f, usedRevive: true);
                int s3 = LevelRewards.CalcStars(won: false, hpRemaining01: 0.9f, usedRevive: true);
                int s4 = LevelRewards.CalcStars(won: false, hpRemaining01: 0.9f, usedRevive: false);

                bool passB = (s1 == 3) && (s2 == 1) && (s3 == 0) && (s4 == 0);
                if (passB)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 23: CalcStars áp dụng cap 1 sao khi usedRevive = true, thua = 0 sao.");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 23: s1={s1}, s2={s2}, s3={s3}, s4={s4}");
                }
            }

            // -------------------------------------------------------------
            // Ca 24: Kiểm thử các ngưỡng biên trên tổng máu 270 HP (Ca c)
            // 229.5 HP (= 0.85) -> 3 sao; 229 HP -> 2 sao; 135 HP (= 0.50) -> 2 sao; 134 HP -> 1 sao.
            // -------------------------------------------------------------
            {
                float r229_5 = 229.5f / 270f;
                float r229 = 229f / 270f;
                float r135 = 135f / 270f;
                float r134 = 134f / 270f;

                int star229_5 = LevelRewards.CalcStars(won: true, hpRemaining01: r229_5, usedRevive: false);
                int star229 = LevelRewards.CalcStars(won: true, hpRemaining01: r229, usedRevive: false);
                int star135 = LevelRewards.CalcStars(won: true, hpRemaining01: r135, usedRevive: false);
                int star134 = LevelRewards.CalcStars(won: true, hpRemaining01: r134, usedRevive: false);

                bool passC = (star229_5 == 3) && (star229 == 2) && (star135 == 2) && (star134 == 1);
                if (passC)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 24: Ngưỡng biên 270 HP (229.5 -> 3★, 229 -> 2★, 135 -> 2★, 134 -> 1★) chính xác tuyệt đối.");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 24: star229_5={star229_5}, star229={star229}, star135={star135}, star134={star134}");
                }
            }

            // -------------------------------------------------------------
            // Ca 25: ReportLevelResult có usedRevive và nhận bù quà khi thắng lại (Ca d)
            // Lần 1: usedRevive = true trên màn mới -> 1 sao, chỉ nhận mốc 1.
            // Lần 2: thắng lại không hồi sinh với 100% máu -> nhận mốc 2 & 3 bù, kỷ lục lên 3 sao.
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new FakeRewardSink();
                var mgr = CreateTestManager(storage, sink);

                // Lần 1: Thắng màn 1 có hồi sinh
                var res1 = mgr.ReportLevelResult(1, won: true, hpRemaining01: 1.0f, usedRevive: true);

                bool passD1 = res1.won &&
                              res1.cappedByRevive &&
                              res1.starsThisRun == 1 &&
                              res1.newMaxStars == 1 &&
                              res1.newlyClaimed[0] &&
                              !res1.newlyClaimed[1] &&
                              !res1.newlyClaimed[2] &&
                              res1.starsGranted == LevelRewards.StarReward(1, 1) &&
                              sink.TotalStars == LevelRewards.StarReward(1, 1) &&
                              mgr.GetStars(1) == 1 &&
                              mgr.IsMilestoneClaimed(1, 1) &&
                              !mgr.IsMilestoneClaimed(1, 2) &&
                              !mgr.IsMilestoneClaimed(1, 3);

                // Lần 2: Thắng lại màn 1 không hồi sinh với 100% HP
                var res2 = mgr.ReportLevelResult(1, won: true, hpRemaining01: 1.0f, usedRevive: false);

                int expectedCompensation = LevelRewards.StarReward(1, 2) + LevelRewards.StarReward(1, 3);
                int totalExpectedBothRuns = LevelRewards.StarReward(1, 1) + expectedCompensation;

                bool passD2 = res2.won &&
                              !res2.cappedByRevive &&
                              res2.starsThisRun == 3 &&
                              res2.previousMaxStars == 1 &&
                              res2.newMaxStars == 3 &&
                              res2.isNewRecord &&
                              !res2.newlyClaimed[0] &&
                              res2.newlyClaimed[1] &&
                              res2.newlyClaimed[2] &&
                              res2.starsGranted == expectedCompensation &&
                              sink.TotalStars == totalExpectedBothRuns &&
                              mgr.GetStars(1) == 3 &&
                              mgr.IsMilestoneClaimed(1, 1) &&
                              mgr.IsMilestoneClaimed(1, 2) &&
                              mgr.IsMilestoneClaimed(1, 3);

                if (passD1 && passD2)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 25: Thắng với hồi sinh đạt 1★ nhận mốc 1; thắng lại không hồi sinh nhận bù mốc 2 & 3 đủ 78 Sao.");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 25: passD1={passD1} (starsThisRun={res1.starsThisRun}, capped={res1.cappedByRevive}, sink={sink.TotalStars}), passD2={passD2} (res2Stars={res2.starsGranted})");
                }

                CleanupManager(mgr);
            }

            // -------------------------------------------------------------
            // Ca 26: Kiểm tra 15 LevelData theo luật thắng & tính không ghi đè (Ca e)
            // Màn 5/10/15 là BossKill; màn 1, 3 là KillCount (25, 29);
            // Màn 2, 4, 7 là SurviveTime (51, 57, 66s); SetupLevelData không phá giá trị đã sửa tay.
            // -------------------------------------------------------------
            {
                LevelData l1 = db.GetLevel(1);
                LevelData l2 = db.GetLevel(2);
                LevelData l3 = db.GetLevel(3);
                LevelData l4 = db.GetLevel(4);
                LevelData l5 = db.GetLevel(5);
                LevelData l7 = db.GetLevel(7);
                LevelData l10 = db.GetLevel(10);
                LevelData l15 = db.GetLevel(15);

                bool cBoss = (l5.winCondition == WinCondition.BossKill) &&
                             (l10.winCondition == WinCondition.BossKill) &&
                             (l15.winCondition == WinCondition.BossKill);

                bool cKill = (l1.winCondition == WinCondition.KillCount && l1.targetKillCount == 25) &&
                             (l3.winCondition == WinCondition.KillCount && l3.targetKillCount == 29);

                bool cSurvive = (l2.winCondition == WinCondition.SurviveTime && Mathf.Approximately(l2.surviveSeconds, 51f)) &&
                                (l4.winCondition == WinCondition.SurviveTime && Mathf.Approximately(l4.surviveSeconds, 57f)) &&
                                (l7.winCondition == WinCondition.SurviveTime && Mathf.Approximately(l7.surviveSeconds, 66f));

                // Kiểm tra tính không ghi đè (Idempotence): sửa tạm một trường, gọi SetupAll(), giá trị không bị mất
                float origSurvive2 = l2.surviveSeconds;
                l2.surviveSeconds = 999f;
                EditorUtility.SetDirty(l2);

                SetupLevelData.SetupAll();

                LevelData l2After = db.GetLevel(2);
                bool cNotOverwritten = Mathf.Approximately(l2After.surviveSeconds, 999f);

                // Khôi phục lại giá trị chuẩn
                l2.surviveSeconds = origSurvive2;
                EditorUtility.SetDirty(l2);
                AssetDatabase.SaveAssets();

                bool passE = cBoss && cKill && cSurvive && cNotOverwritten;
                if (passE)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 26: 15 LevelData khởi tạo đúng WinCondition (BossKill, KillCount, SurviveTime) và Setup không ghi đè giá trị sửa tay.");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 26: cBoss={cBoss}, cKill={cKill}, cSurvive={cSurvive}, cNotOverwritten={cNotOverwritten}");
                }
            }

            // -------------------------------------------------------------
            // Ca 27: Kiểm tra tính toán chỉ số LanStats.Compute thống nhất cho Hangar và Gameplay (5A.6)
            // Với mọi Lân (1..4) và mọi cấp (0..10):
            // HP = baseHp + 10 * cấp
            // Damage = baseDamage + 2 * cấp
            // FireRate = baseFireRate + 0.2 * cấp
            // -------------------------------------------------------------
            {
                bool pass27 = true;
                string failDetail = "";
                string[] lanIds = new string[] { "1", "2", "3", "4" };

                // Lưu lại trạng thái PlayerPrefs trước test
                System.Collections.Generic.Dictionary<string, int> savedPrefs = new System.Collections.Generic.Dictionary<string, int>();
                foreach (var id in lanIds)
                {
                    savedPrefs[$"UpgradeLevel_{id}_HP"] = PlayerPrefs.GetInt($"UpgradeLevel_{id}_HP", 1);
                    savedPrefs[$"UpgradeLevel_{id}_Damage"] = PlayerPrefs.GetInt($"UpgradeLevel_{id}_Damage", 1);
                    savedPrefs[$"UpgradeLevel_{id}_FireRate"] = PlayerPrefs.GetInt($"UpgradeLevel_{id}_FireRate", 1);
                }

                try
                {
                    foreach (var id in lanIds)
                    {
                        var lan = LanCatalog.GetLan(id);
                        float baseHp = lan != null ? lan.maxHp : 100f;
                        float baseDmg = lan != null ? lan.baseDamage : 10f;
                        float baseFr = lan != null ? lan.fireRate : 2.5f;

                        for (int cap = 0; cap <= 10; cap++)
                        {
                            // Hangar quy ước cấp 1..11 tương ứng 0..10 lần nâng
                            PlayerPrefs.SetInt($"UpgradeLevel_{id}_HP", cap + 1);
                            PlayerPrefs.SetInt($"UpgradeLevel_{id}_Damage", cap + 1);
                            PlayerPrefs.SetInt($"UpgradeLevel_{id}_FireRate", cap + 1);

                            var stats = LanStats.Compute(id, lan);

                            float expHp = baseHp + 10f * cap;
                            float expDmg = baseDmg + 2f * cap;
                            float expFr = baseFr + 0.2f * cap;

                            if (!Mathf.Approximately(stats.hp, expHp) ||
                                !Mathf.Approximately(stats.damage, expDmg) ||
                                !Mathf.Approximately(stats.fireRate, expFr))
                            {
                                pass27 = false;
                                failDetail = $"Lân {id} cấp {cap}: HP={stats.hp} (kỳ vọng {expHp}), DMG={stats.damage} (kỳ vọng {expDmg}), FR={stats.fireRate} (kỳ vọng {expFr})";
                                break;
                            }
                        }
                        if (!pass27) break;
                    }
                }
                finally
                {
                    // Khôi phục PlayerPrefs
                    foreach (var kvp in savedPrefs)
                    {
                        PlayerPrefs.SetInt(kvp.Key, kvp.Value);
                    }
                    PlayerPrefs.Save();
                }

                if (pass27)
                {
                    passedCount++;
                    sb.AppendLine("[PASS] Ca 27: LanStats.Compute thống nhất cho cả 4 Lân ở mọi cấp 0..10 (HP=base+10*cấp, DMG=base+2*cấp, FR=base+0.2*cấp).");
                }
                else
                {
                    sb.AppendLine($"[FAIL] Ca 27: Lệch chỉ số: {failDetail}");
                }
            }

            sb.AppendLine("-------------------------------------------------------------");
            sb.AppendLine($"TỔNG KẾT: {passedCount}/{totalTests} ca PASS.");

            if (passedCount == totalTests)
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
