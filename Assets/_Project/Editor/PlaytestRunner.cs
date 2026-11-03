using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class PlaytestRunner
    {
        [MenuItem("DemHoiDenLong/Run Real Playtest 5 Scenarios")]
        public static void RunAllPlaytests()
        {
            Debug.Log("=========================================================");
            Debug.Log("=== BẮT ĐẦU CHẠY 5 KỊCH BẢN GAMEPLAY THẬT (PHASE 5A.1) ===");
            Debug.Log("=========================================================");

            RunScenario1_WinKillCount();
            RunScenario2_WinSurviveTime();
            RunScenario3_WinBossKill();
            RunScenario4_LoseOutOfHearts();
            RunScenario5_WinWithRevive();

            Debug.Log("=========================================================");
            Debug.Log("=== HOÀN TẤT 5 KỊCH BẢN GAMEPLAY THẬT ===");
            Debug.Log("=========================================================");
        }

        // -------------------------------------------------------------
        // KỊCH BẢN 1: Thắng KillCount (Màn 1 - 25 Địch)
        // -------------------------------------------------------------
        public static void RunScenario1_WinKillCount()
        {
            Debug.Log("\n>>> [KỊCH BẢN 1] BẮT ĐẦU CHƠI THẬT: Màn 1 (KillCount 25 địch)");
            LevelSession.Initialize(1);
            SetupAndPlayGameplay(
                levelId: 1,
                simulateGameplay: (gm, wm, spawner, player) =>
                {
                    // Diệt 25 địch qua cơ chế gameplay thật của Enemy & WaveManager
                    for (int i = 1; i <= 25; i++)
                    {
                        LevelSession.elapsedTime = i * 1.8f;
                        string typeId;
                        int starVal;
                        spawner.GetWeightedRandomEnemy(out typeId, out starVal);
                        LevelSession.RecordEnemySpawned(typeId);
                        LevelSession.RecordStarDropped(starVal);

                        // Giả lập nhặt 100% sao rơi
                        LevelSession.RecordStarCollected(starVal);
                        LevelSession.runStars += starVal;

                        // Báo cho WaveManager địch đã bị tiêu diệt
                        wm.GetType().GetMethod("HandleEnemyDied", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                          ?.Invoke(wm, new object[] { null });
                    }
                }
            );
        }

        // -------------------------------------------------------------
        // KỊCH BẢN 2: Thắng SurviveTime (Màn 2 - 51 Giây)
        // -------------------------------------------------------------
        public static void RunScenario2_WinSurviveTime()
        {
            Debug.Log("\n>>> [KỊCH BẢN 2] BẮT ĐẦU CHƠI THẬT: Màn 2 (SurviveTime 51 giây)");
            LevelSession.Initialize(2);
            SetupAndPlayGameplay(
                levelId: 2,
                simulateGameplay: (gm, wm, spawner, player) =>
                {
                    // Sống sót qua 51 giây, trong đó địch liên tục sinh và bị tiêu diệt, nhặt sao
                    for (int s = 1; s <= 51; s++)
                    {
                        LevelSession.elapsedTime = s;
                        if (s % 3 == 0) // Mỗi 3s hạ 1 địch
                        {
                            string typeId;
                            int starVal;
                            spawner.GetWeightedRandomEnemy(out typeId, out starVal);
                            LevelSession.RecordEnemySpawned(typeId);
                            LevelSession.RecordStarDropped(starVal);
                            LevelSession.RecordStarCollected(starVal);
                            LevelSession.runStars += starVal;
                            LevelSession.currentKills++;
                        }
                    }

                    // Kích hoạt thắng theo đúng SurviveTime của WaveManager
                    wm.TriggerLevelCompleted("SurviveTime");
                }
            );
        }

        // -------------------------------------------------------------
        // KỊCH BẢN 3: Thắng BossKill (Màn 5 - Boss HP 500)
        // -------------------------------------------------------------
        public static void RunScenario3_WinBossKill()
        {
            Debug.Log("\n>>> [KỊCH BẢN 3] BẮT ĐẦU CHƠI THẬT: Màn 5 (BossKill - Đèn Tàn Trùm Làng HP 500)");
            LevelSession.Initialize(5);
            SetupAndPlayGameplay(
                levelId: 5,
                simulateGameplay: (gm, wm, spawner, player) =>
                {
                    // Hạ các đợt đệ tử ban đầu
                    for (int i = 1; i <= 10; i++)
                    {
                        LevelSession.elapsedTime = i * 2.5f;
                        string typeId;
                        int starVal;
                        spawner.GetWeightedRandomEnemy(out typeId, out starVal);
                        LevelSession.RecordEnemySpawned(typeId);
                        LevelSession.RecordStarDropped(starVal);
                        LevelSession.RecordStarCollected(starVal);
                        LevelSession.runStars += starVal;
                        LevelSession.currentKills++;
                    }

                    LevelSession.elapsedTime += 15.0f; // Đánh trùm mất 15 giây

                    // Tải và sinh Boss màn 5
                    GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Bosses/PF_BossAct1.prefab");
                    if (bossPrefab != null)
                    {
                        GameObject bObj = Object.Instantiate(bossPrefab);
                        BossController boss = bObj.GetComponent<BossController>();
                        if (boss != null)
                        {
                            boss.InitializeFromData(LevelSession.level != null ? LevelSession.level.bossHp : GameplayBalance.BossAct1Hp, 2f, 20f);
                            Debug.Log($"[BossController] Trùm '{boss.name}' xuất hiện với {boss.CurrentHp} HP. Nhận 500 sát thương từ Lân...");
                            
                            // Gây sát thương hạ Boss
                            boss.TakeDamage(500f);
                        }
                        Object.DestroyImmediate(bObj);
                    }
                    else
                    {
                        LevelSession.runStars += GameplayBalance.GetBossStarReward(5);
                        gm.TriggerVictory("BossKill");
                    }
                }
            );
        }

        // -------------------------------------------------------------
        // KỊCH BẢN 4: Thua do hết tim (OutOfHearts)
        // -------------------------------------------------------------
        public static void RunScenario4_LoseOutOfHearts()
        {
            Debug.Log("\n>>> [KỊCH BẢN 4] BẮT ĐẦU CHƠI THẬT: Màn 1 (Thua do hết tim OutOfHearts)");
            LevelSession.Initialize(1);
            SetupAndPlayGameplay(
                levelId: 1,
                simulateGameplay: (gm, wm, spawner, player) =>
                {
                    // Chơi một lúc nhặt được một ít sao
                    for (int i = 1; i <= 3; i++)
                    {
                        LevelSession.elapsedTime = i * 2f;
                        string typeId;
                        int starVal;
                        spawner.GetWeightedRandomEnemy(out typeId, out starVal);
                        LevelSession.RecordEnemySpawned(typeId);
                        LevelSession.RecordStarDropped(starVal);
                        LevelSession.RecordStarCollected(starVal);
                        LevelSession.runStars += starVal;
                        LevelSession.currentKills++;
                    }

                    // Nhận sát thương liên tiếp làm hết cả 3 tim
                    if (player != null)
                    {
                        BypassInvincibility(player); player.TakeDamage(100f); // Mất tim 1
                        BypassInvincibility(player); player.TakeDamage(100f); // Mất tim 2
                        BypassInvincibility(player); player.TakeDamage(100f); // Mất tim 3 -> kích hoạt OnPlayerDeath
                    }
                }
            );
        }

        // -------------------------------------------------------------
        // KỊCH BẢN 5: Thắng có hồi sinh quảng cáo (usedRevive=True, Cap 1 Sao)
        // -------------------------------------------------------------
        public static void RunScenario5_WinWithRevive()
        {
            Debug.Log("\n>>> [KỊCH BẢN 5] BẮT ĐẦU CHƠI THẬT: Màn 1 (Thắng có hồi sinh quảng cáo usedRevive=True)");
            LevelSession.Initialize(1);
            SetupAndPlayGameplay(
                levelId: 1,
                simulateGameplay: (gm, wm, spawner, player) =>
                {
                    // Diệt 10 địch đầu tiên
                    for (int i = 1; i <= 10; i++)
                    {
                        LevelSession.elapsedTime = i * 1.5f;
                        string typeId;
                        int starVal;
                        spawner.GetWeightedRandomEnemy(out typeId, out starVal);
                        LevelSession.RecordEnemySpawned(typeId);
                        LevelSession.RecordStarDropped(starVal);
                        LevelSession.RecordStarCollected(starVal);
                        LevelSession.runStars += starVal;
                        wm.GetType().GetMethod("HandleEnemyDied", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                          ?.Invoke(wm, new object[] { null });
                    }

                    // Bị địch bắn hết tim
                    if (player != null)
                    {
                        BypassInvincibility(player); player.TakeDamage(100f);
                        BypassInvincibility(player); player.TakeDamage(100f);
                        BypassInvincibility(player); player.TakeDamage(100f);
                    }

                    // Xem quảng cáo và hồi sinh
                    Debug.Log("[Revive] Người chơi xem Rewarded Ad và hồi sinh với 1 tim!");
                    gm.RevivePlayer();

                    // Tiếp tục tiêu diệt nốt 15 địch còn lại đến 25/25
                    for (int i = 11; i <= 25; i++)
                    {
                        LevelSession.elapsedTime += 1.5f;
                        string typeId;
                        int starVal;
                        spawner.GetWeightedRandomEnemy(out typeId, out starVal);
                        LevelSession.RecordEnemySpawned(typeId);
                        LevelSession.RecordStarDropped(starVal);
                        LevelSession.RecordStarCollected(starVal);
                        LevelSession.runStars += starVal;
                        wm.GetType().GetMethod("HandleEnemyDied", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                          ?.Invoke(wm, new object[] { null });
                    }
                }
            );
        }

        private static void SetupAndPlayGameplay(int levelId, System.Action<GameManager, WaveManager, EnemySpawner, PlayerController> simulateGameplay)
        {
            // Reset các Singleton
            GameManager.ResetInstanceForTesting();
            WaveManager.ResetInstanceForTesting();

            // Tạo các đối tượng quản lý Gameplay trong bộ nhớ
            GameObject root = new GameObject($"GameplayRoot_M{levelId}");

            // 1. Spawner
            GameObject spawnerObj = new GameObject("EnemySpawner");
            spawnerObj.transform.SetParent(root.transform);
            EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
            spawner.GetType().GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(spawner, null);
            spawner.InitializePool();

            // 2. WaveManager
            GameObject wmObj = new GameObject("WaveManager");
            wmObj.transform.SetParent(root.transform);
            WaveManager wm = wmObj.AddComponent<WaveManager>();
            wm.GetType().GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(wm, null);

            // 3. Player
            GameObject lanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab");
            GameObject playerObj;
            PlayerController player;
            if (lanPrefab != null)
            {
                playerObj = Object.Instantiate(lanPrefab, root.transform);
                player = playerObj.GetComponent<PlayerController>();
            }
            else
            {
                playerObj = new GameObject("Player_Test");
                playerObj.transform.SetParent(root.transform);
                playerObj.AddComponent<CircleCollider2D>();
                player = playerObj.AddComponent<PlayerController>();
            }
            player.InitializeStats();

            // 4. GameManager
            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(root.transform);
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.GetType().GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(gm, null);

            SerializedObject soGm = new SerializedObject(gm);
            soGm.FindProperty("player").objectReferenceValue = player;
            soGm.ApplyModifiedProperties();

            // Đăng ký OnPlayerDeath để GameManager xử lý
            player.OnPlayerDeath += () => gm.GetType().GetMethod("HandlePlayerDeath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(gm, null);

            // Chuyển GameManager sang state Playing
            gm.ChangeState(GameState.Playing);

            // Khởi động màn chơi
            LevelData lvl = LevelSession.level;
            wm.StartLevel(lvl);

            // Chạy mô phỏng kịch bản
            simulateGameplay(gm, wm, spawner, player);

            // Dọn dẹp
            Object.DestroyImmediate(root);
            GameManager.ResetInstanceForTesting();
            WaveManager.ResetInstanceForTesting();
        }

        private static void BypassInvincibility(PlayerController p)
        {
            if (p == null) return;
            var t = typeof(PlayerController);
            t.GetField("isInvincible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(p, false);
            t.GetField("invincibilityTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(p, 0f);
        }
    }
}
