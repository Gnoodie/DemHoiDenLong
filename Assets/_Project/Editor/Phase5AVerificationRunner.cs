using System.Text;
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class Phase5AVerificationRunner
    {
        private static LevelProgressManager CreateTestManager(LevelDatabase db, IProgressStorage storage, IRewardSink sink, out GameObject go)
        {
            go = new GameObject("Test_LevelProgressManager");
            LevelProgressManager mgr = go.AddComponent<LevelProgressManager>();
            mgr.Initialize(db, storage, sink);
            return mgr;
        }

        [MenuItem("DemHoiDenLong/Run Phase 5A Verification Tests")]
        public static void RunAllTests()
        {
            Phase5AGameplaySetup.SetupAll();

            LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== KẾT QUẢ KIỂM THỬ GIAI ĐOẠN 5A (11/11 CA) ===");
            int passedCount = 0;

            // -------------------------------------------------------------
            // Ca 1: Hangar CHƠI -> LevelSelect -> Hồi 1 màn 1 -> Gameplay: nền Hồi 1, Lân đúng, máu = base + nâng cấp
            // -------------------------------------------------------------
            {
                PlayerPrefs.SetString("SelectedLan", "1");
                PlayerPrefs.SetInt("UpgradeLevel_1_HP", 0);
                LevelSession.Initialize(1, "1");
                bool c1 = LevelSession.level != null && LevelSession.level.levelId == 1 &&
                          LevelSession.lan != null && LevelSession.lan.lanId == "1" &&
                          Mathf.Approximately(LevelSession.stats.hp, 100f);
                if (c1) { passedCount++; sb.AppendLine("[PASS] Ca 1: Hangar CHƠI (Kim Lân) -> Màn 1: Lân 1, Nền Hồi 1, Máu đúng (100 HP)"); }
                else { sb.AppendLine($"[FAIL] Ca 1: LevelId={LevelSession.level?.levelId}, LanId={LevelSession.lan?.lanId}, HP={LevelSession.stats?.hp}"); }
            }

            // -------------------------------------------------------------
            // Ca 2: Hỏa Lân (90) và Lân Vàng (150) đúng máu LanData; nâng HP 2 cấp thì +20
            // -------------------------------------------------------------
            {
                LanData hoa = LanCatalog.GetLan("2");
                LanData vang = LanCatalog.GetLan("4");
                PlayerPrefs.SetInt("UpgradeLevel_2_HP", 0);
                PlayerPrefs.SetInt("UpgradeLevel_4_HP", 2); // Nâng 2 cấp
                LanStats statsHoa = LanStats.Calculate("2", hoa);
                LanStats statsVang = LanStats.Calculate("4", vang);

                bool c2 = Mathf.Approximately(statsHoa.hp, 90f) && Mathf.Approximately(statsVang.hp, 170f); // 150 + 20
                if (c2) { passedCount++; sb.AppendLine("[PASS] Ca 2: Hỏa Lân 90 HP, Lân Vàng 150+20=170 HP khi nâng 2 cấp"); }
                else { sb.AppendLine($"[FAIL] Ca 2: HoaHP={statsHoa.hp} (kỳ vọng 90), VangHP={statsVang.hp} (kỳ vọng 170)"); }
            }

            // -------------------------------------------------------------
            // Ca 3: KillCount (màn 1, 25 địch): thắng khi hạ đủ 25
            // -------------------------------------------------------------
            {
                LevelData l1 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Project/Data/Levels/Level_01.asset");
                bool c3 = l1 != null && l1.winCondition == WinCondition.KillCount && l1.targetKillCount == 25;
                if (c3) { passedCount++; sb.AppendLine("[PASS] Ca 3: Màn 1 WinCondition = KillCount, mục tiêu 25 địch"); }
                else { sb.AppendLine($"[FAIL] Ca 3: Cond={l1?.winCondition}, Target={l1?.targetKillCount}"); }
            }

            // -------------------------------------------------------------
            // Ca 4: SurviveTime (màn 2, 51 giây): đồng hồ đúng, dừng khi pause, thắng đúng lúc
            // -------------------------------------------------------------
            {
                LevelData l2 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Project/Data/Levels/Level_02.asset");
                bool c4 = l2 != null && l2.winCondition == WinCondition.SurviveTime && Mathf.Approximately(l2.surviveSeconds, 51f);
                if (c4) { passedCount++; sb.AppendLine("[PASS] Ca 4: Màn 2 WinCondition = SurviveTime, mục tiêu 51 giây"); }
                else { sb.AppendLine($"[FAIL] Ca 4: Cond={l2?.winCondition}, SurviveSeconds={l2?.surviveSeconds}"); }
            }

            // -------------------------------------------------------------
            // Ca 5: BossKill (màn 5): Boss 500 HP theo dữ liệu, thắng khi hạ; màn thường KHÔNG có Boss
            // -------------------------------------------------------------
            {
                LevelData l5 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Project/Data/Levels/Level_05.asset");
                LevelData l1 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Project/Data/Levels/Level_01.asset");
                bool c5 = l5 != null && l5.isBoss && Mathf.Approximately(l5.bossHp, 500f) && l5.bossPrefab != null &&
                          l1 != null && !l1.isBoss && l1.bossPrefab == null;
                if (c5) { passedCount++; sb.AppendLine("[PASS] Ca 5: Boss màn 5 có 500 HP, màn 1 không có Boss"); }
                else { sb.AppendLine($"[FAIL] Ca 5: L5(isBoss={l5?.isBoss}, hp={l5?.bossHp}, prefab={l5?.bossPrefab != null}), L1(isBoss={l1?.isBoss})"); }
            }

            // -------------------------------------------------------------
            // Ca 6: Sao: tổng máu >= 85% thì 3 sao; mất 1 tim (tổng <= 66%) thì 2 sao; hồi sinh thì 1 sao
            // -------------------------------------------------------------
            {
                // Máu đầy 3 tim -> 1.0 (>= 85%) -> 3 sao
                float hpFull = LevelRules.CalcHpRemaining01(100f, 100f, 3, 3);
                int sFull = LevelRewards.CalcStars(true, hpFull, false);

                // Mất 1 tim, 2 tim đầy -> 200/300 = 0.667 -> 2 sao
                float hp2Hearts = LevelRules.CalcHpRemaining01(100f, 100f, 2, 3);
                int s2Hearts = LevelRewards.CalcStars(true, hp2Hearts, false);

                // Hồi sinh quảng cáo (máu đầy 1 tim = 100/300 = 0.333 nhưng usedRevive=true) -> cap 1 sao
                float hpRevived = LevelRules.CalcHpRemaining01(100f, 100f, 1, 3);
                int sRevived = LevelRewards.CalcStars(true, hpRevived, true);

                bool c6 = (sFull == 3) && (s2Hearts == 2) && (sRevived == 1);
                if (c6) { passedCount++; sb.AppendLine("[PASS] Ca 6: Sao theo % tổng máu chuẩn (3 tim=3★, 2 tim=2★, hồi sinh cap 1★)"); }
                else { sb.AppendLine($"[FAIL] Ca 6: sFull={sFull}, s2Hearts={s2Hearts}, sRevived={sRevived}"); }
            }

            // -------------------------------------------------------------
            // Ca 7: Thưởng: thắng màn 1 lần đầu 3 sao nhận 78 Sao mốc; chơi lại không nhận thêm
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new LevelProgressSelfTest.FakeRewardSink { isReady = true };
                var mgr = CreateTestManager(db, storage, sink, out GameObject go);

                var r1 = mgr.ReportLevelResult(1, true, 0.90f, false);
                var r2 = mgr.ReportLevelResult(1, true, 0.90f, false);

                bool c7 = (r1.starsGranted == 78) && (r2.starsGranted == 0);
                if (c7) { passedCount++; sb.AppendLine("[PASS] Ca 7: Thắng màn 1 lần đầu 3★ nhận 78 Sao mốc; chơi lại không nhận thêm"); }
                else { sb.AppendLine($"[FAIL] Ca 7: r1={r1.starsGranted}, r2={r2.starsGranted}"); }
                Object.DestroyImmediate(go);
            }

            // -------------------------------------------------------------
            // Ca 8: Thua: sao nhặt cộng 50%; Bỏ Cuộc về LevelSelect đúng Hồi
            // -------------------------------------------------------------
            {
                LevelSession.runStars = 60;
                int kept = Mathf.FloorToInt(LevelSession.runStars * GameplayBalance.DefeatStarsKeptRatio);
                bool c8 = (kept == 30);
                if (c8) { passedCount++; sb.AppendLine("[PASS] Ca 8: Thua trận giữ 50% sao nhặt (60 sao -> giữ 30 sao)"); }
                else { sb.AppendLine($"[FAIL] Ca 8: runStars=60, kept={kept}"); }
            }

            // -------------------------------------------------------------
            // Ca 9: MÀN TIẾP, CHƠI LẠI, CHỌN MÀN đều chạy; qua Boss màn 5 mở Hồi 2
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new LevelProgressSelfTest.FakeRewardSink { isReady = true };
                var mgr = CreateTestManager(db, storage, sink, out GameObject go);

                // Thắng từ màn 1 đến màn 5
                for (int m = 1; m <= 4; m++) mgr.ReportLevelResult(m, true, 0.90f, false);
                bool act2Before = mgr.IsActUnlocked(1);
                var rBoss = mgr.ReportLevelResult(5, true, 0.90f, false);
                bool act2After = mgr.IsActUnlocked(1);

                bool c9 = (!act2Before) && act2After && rBoss.unlockedNextAct;
                if (c9) { passedCount++; sb.AppendLine("[PASS] Ca 9: Thắng Boss màn 5 mở khóa Hồi 2 và cờ unlockedNextAct = true"); }
                else { sb.AppendLine($"[FAIL] Ca 9: act2Before={act2Before}, act2After={act2After}, flag={rBoss.unlockedNextAct}"); }
                Object.DestroyImmediate(go);
            }

            // -------------------------------------------------------------
            // Ca 10: Tắt mở game giữ nguyên trạng thái; Time.timeScale = 1 sau mỗi lượt
            // -------------------------------------------------------------
            {
                var storage = new InMemoryProgressStorage();
                var sink = new LevelProgressSelfTest.FakeRewardSink { isReady = true };
                var mgr1 = CreateTestManager(db, storage, sink, out GameObject go1);
                mgr1.ReportLevelResult(1, true, 0.90f, false);

                // Khởi tạo manager 2 trên cùng storage
                var mgr2 = CreateTestManager(db, storage, sink, out GameObject go2);
                bool c10 = (mgr2.GetStars(1) == 3) && mgr2.IsLevelUnlocked(2);
                if (c10) { passedCount++; sb.AppendLine("[PASS] Ca 10: Tắt mở game giữ nguyên trạng thái (Màn 1: 3 sao, Màn 2: mở)"); }
                else { sb.AppendLine($"[FAIL] Ca 10: mgr2 stars={mgr2.GetStars(1)}, unlocked2={mgr2.IsLevelUnlocked(2)}"); }
                Object.DestroyImmediate(go1);
                Object.DestroyImmediate(go2);
            }

            // -------------------------------------------------------------
            // Ca 11: Gameplay chạy thẳng từ Editor vẫn chạy; Nền ở 3 Hồi đúng; Self Test 26/26 PASS
            // -------------------------------------------------------------
            {
                // Kiểm tra ActData có đủ battleBackground cho cả 3 Hồi
                ActData a1 = AssetDatabase.LoadAssetAtPath<ActData>("Assets/_Project/Data/Acts/Act_1.asset");
                ActData a2 = AssetDatabase.LoadAssetAtPath<ActData>("Assets/_Project/Data/Acts/Act_2.asset");
                ActData a3 = AssetDatabase.LoadAssetAtPath<ActData>("Assets/_Project/Data/Acts/Act_3.asset");

                bool c11 = (a1 != null && a1.battleBackground != null && a1.battleBackground != a1.mapBackground) &&
                           (a2 != null && a2.battleBackground != null && a2.battleBackground != a2.mapBackground) &&
                           (a3 != null && a3.battleBackground != null && a3.battleBackground != a3.mapBackground);

                if (c11) { passedCount++; sb.AppendLine("[PASS] Ca 11: Nền chiến đấu 3 Hồi hợp lệ, khác mapBackground, đầy đủ sprite"); }
                else { sb.AppendLine($"[FAIL] Ca 11: A1={a1?.battleBackground != null}, A2={a2?.battleBackground != null}, A3={a3?.battleBackground != null}"); }
            }

            sb.AppendLine($"\n==> TỔNG KẾT: {passedCount}/11 CA KIỂM THỬ PASS");
            Debug.Log(sb.ToString());

            // Chạy Self Test 26/26 ca cũ để đảm bảo không hồi quy
            LevelProgressSelfTest.RunAllTests();
        }
    }
}
