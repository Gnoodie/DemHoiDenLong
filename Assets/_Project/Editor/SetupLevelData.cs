using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public static class SetupLevelData
    {
        private const string LevelsFolder = "Assets/_Project/Data/Levels";
        private const string ActsFolder = "Assets/_Project/Data/Acts";
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string DatabasePath = "Assets/_Project/Data/LevelDatabase.asset";
        private const string ResourcesDatabasePath = "Assets/_Project/Resources/LevelDatabase.asset";

        private struct LevelDef
        {
            public int levelId;
            public int actIndex;
            public int indexInAct;
            public float hpMultiplier;
            public float speedMultiplier;
            public float damageMultiplier;
            public int enemyCount;
            public bool isBoss;
            public string bossName;
            public string bossEnemyId;
        }

        private static readonly LevelDef[] LevelDefs = new LevelDef[]
        {
            new LevelDef { levelId = 1,  actIndex = 0, indexInAct = 1, hpMultiplier = 1.00f, speedMultiplier = 1.00f, damageMultiplier = 1.00f, enemyCount = 25, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 2,  actIndex = 0, indexInAct = 2, hpMultiplier = 1.08f, speedMultiplier = 1.03f, damageMultiplier = 1.05f, enemyCount = 27, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 3,  actIndex = 0, indexInAct = 3, hpMultiplier = 1.16f, speedMultiplier = 1.06f, damageMultiplier = 1.10f, enemyCount = 29, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 4,  actIndex = 0, indexInAct = 4, hpMultiplier = 1.24f, speedMultiplier = 1.09f, damageMultiplier = 1.15f, enemyCount = 31, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 5,  actIndex = 0, indexInAct = 5, hpMultiplier = 1.32f, speedMultiplier = 1.12f, damageMultiplier = 1.20f, enemyCount = 33, isBoss = true,  bossName = "Đèn Tàn Trùm Làng", bossEnemyId = "boss_den_long_khong_lo" },

            new LevelDef { levelId = 6,  actIndex = 1, indexInAct = 1, hpMultiplier = 1.40f, speedMultiplier = 1.15f, damageMultiplier = 1.25f, enemyCount = 35, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 7,  actIndex = 1, indexInAct = 2, hpMultiplier = 1.48f, speedMultiplier = 1.18f, damageMultiplier = 1.30f, enemyCount = 37, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 8,  actIndex = 1, indexInAct = 3, hpMultiplier = 1.56f, speedMultiplier = 1.21f, damageMultiplier = 1.35f, enemyCount = 39, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 9,  actIndex = 1, indexInAct = 4, hpMultiplier = 1.64f, speedMultiplier = 1.24f, damageMultiplier = 1.40f, enemyCount = 41, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 10, actIndex = 1, indexInAct = 5, hpMultiplier = 1.72f, speedMultiplier = 1.27f, damageMultiplier = 1.45f, enemyCount = 43, isBoss = true,  bossName = "Rồng Đèn Phố Cổ", bossEnemyId = "boss_rong_den_pho_co" },

            new LevelDef { levelId = 11, actIndex = 2, indexInAct = 1, hpMultiplier = 1.80f, speedMultiplier = 1.30f, damageMultiplier = 1.50f, enemyCount = 45, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 12, actIndex = 2, indexInAct = 2, hpMultiplier = 1.88f, speedMultiplier = 1.33f, damageMultiplier = 1.55f, enemyCount = 47, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 13, actIndex = 2, indexInAct = 3, hpMultiplier = 1.96f, speedMultiplier = 1.36f, damageMultiplier = 1.60f, enemyCount = 49, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 14, actIndex = 2, indexInAct = 4, hpMultiplier = 2.04f, speedMultiplier = 1.39f, damageMultiplier = 1.65f, enemyCount = 51, isBoss = false, bossName = "", bossEnemyId = "" },
            new LevelDef { levelId = 15, actIndex = 2, indexInAct = 5, hpMultiplier = 2.12f, speedMultiplier = 1.42f, damageMultiplier = 1.70f, enemyCount = 53, isBoss = true,  bossName = "Thỏ Ngọc Hóa Quái", bossEnemyId = "boss_tho_ngoc_hoa_quai" }
        };

        [MenuItem("DemHoiDenLong/Setup LevelData from GDD")]
        public static void SetupAll()
        {
            EnsureFolders();

            // 1. Tạo hoặc cập nhật 15 LevelData assets
            Dictionary<int, LevelData> levelDict = new Dictionary<int, LevelData>();
            for (int i = 0; i < LevelDefs.Length; i++)
            {
                LevelDef def = LevelDefs[i];
                string path = $"{LevelsFolder}/Level_{def.levelId:00}.asset";

                LevelData lvl = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (lvl == null)
                {
                    lvl = ScriptableObject.CreateInstance<LevelData>();
                    AssetDatabase.CreateAsset(lvl, path);
                }

                lvl.levelId = def.levelId;
                lvl.actIndex = def.actIndex;
                lvl.indexInAct = def.indexInAct;
                lvl.displayName = $"MÀN {def.actIndex + 1}-{def.indexInAct}";
                lvl.isBoss = def.isBoss;
                lvl.bossName = def.bossName;
                lvl.bossEnemyId = def.bossEnemyId;
                lvl.hpMultiplier = def.hpMultiplier;
                lvl.speedMultiplier = def.speedMultiplier;
                lvl.damageMultiplier = def.damageMultiplier;
                lvl.enemyCount = def.enemyCount;

                // Đồng bộ sang các trường legacy để WaveManager tương thích
                lvl.baseHpMultiplier = def.hpMultiplier;
                lvl.baseSpeedMultiplier = def.speedMultiplier;
                lvl.baseDamageMultiplier = def.damageMultiplier;

                // Khởi tạo luật thắng Phase 3.5 (chỉ gán khi chưa khởi tạo, không ghi đè giá trị đã chỉnh tay)
                if (!lvl.winRuleInitialized)
                {
                    lvl.enemySpawnBudget = def.enemyCount;
                    if (def.isBoss)
                    {
                        lvl.winCondition = WinCondition.BossKill;
                        lvl.surviveSeconds = 0f;
                        lvl.targetKillCount = 0;
                    }
                    else if (def.indexInAct == 1 || def.indexInAct == 3)
                    {
                        lvl.winCondition = WinCondition.KillCount;
                        lvl.targetKillCount = def.enemyCount;
                        lvl.surviveSeconds = 0f;
                    }
                    else // def.indexInAct == 2 || def.indexInAct == 4
                    {
                        lvl.winCondition = WinCondition.SurviveTime;
                        // đề xuất khởi điểm, cần playtest
                        lvl.surviveSeconds = 45f + 3f * def.levelId;
                        lvl.targetKillCount = 0;
                    }
                    lvl.winRuleInitialized = true;
                }

                EditorUtility.SetDirty(lvl);
                levelDict[def.levelId] = lvl;
            }

            // 2. Tạo hoặc cập nhật 3 ActData assets
            ActData act1 = SetupAct(0, "HỒI 1 - LÀNG QUÊ THANH BÌNH", "KHỞI ĐẦU ĐÊM RẰM", "", new Color(0.498f, 0.89f, 0.753f, 1f), // #7FE3C0
                new string[] { "den_giay", "den_gai" },
                "Assets/_Project/Art/Backgrounds/bg_village_map.png",
                new string[] { "Assets/_Project/Art/Backgrounds/bg_village_map_ngang.png", "Assets/_Project/Art/Backgrounds/back_act1.png" },
                levelDict, 1, 5, "Assets/_Project/Art/LevelSelect/node_boss_act1.png");

            ActData act2 = SetupAct(1, "HỒI 2 - PHỐ CỔ ĐÈN LỒNG", "TÂM ĐIỂM LỄ HỘI", "Đánh bại trùm Hồi 1 để mở khóa", new Color(1f, 0.706f, 0.42f, 1f), // #FFB46B
                new string[] { "den_gai", "den_ca_chep", "den_ma" },
                "Assets/_Project/Art/Backgrounds/bg_oldtown_map.png",
                new string[] { "Assets/_Project/Art/Backgrounds/bg_oldtown_map_ngang.png", "Assets/_Project/Art/Backgrounds/back_act2.png" },
                levelDict, 6, 10, "Assets/_Project/Art/LevelSelect/node_boss_act2.png");

            ActData act3 = SetupAct(2, "HỒI 3 - CUNG TRĂNG HUYỀN ẢO", "BÍ ẨN CUNG TRĂNG", "Đánh bại trùm Hồi 2 để mở khóa", new Color(0.663f, 0.608f, 1f, 1f), // #A99BFF
                new string[] { "den_ma" },
                "Assets/_Project/Art/Backgrounds/bg_moon_map.png",
                new string[] { "Assets/_Project/Art/Backgrounds/bg_moon_map_ngang.png", "Assets/_Project/Art/Backgrounds/back_act3.png" },
                levelDict, 11, 15, "Assets/_Project/Art/LevelSelect/node_boss_act3.png");

            // 3. Tạo hoặc cập nhật LevelDatabase asset (bảo tồn các Hồi bổ sung ngoài 3 Hồi đã biết)
            LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<LevelDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }

            if (db.acts == null) db.acts = new List<ActData>();
            while (db.acts.Count < 3) db.acts.Add(null);
            db.acts[0] = act1;
            db.acts[1] = act2;
            db.acts[2] = act3;
            // Giữ nguyên các Hồi từ index 3 trở đi (ví dụ Act 4) nếu người dùng đã tạo trước đó
            EditorUtility.SetDirty(db);

            // 4. Đồng bộ thêm 1 bản trong Resources để Resources.Load hoạt động độc lập không cần Editor
            LevelDatabase resDb = AssetDatabase.LoadAssetAtPath<LevelDatabase>(ResourcesDatabasePath);
            if (resDb == null)
            {
                resDb = ScriptableObject.CreateInstance<LevelDatabase>();
                AssetDatabase.CreateAsset(resDb, ResourcesDatabasePath);
            }
            if (resDb.acts == null) resDb.acts = new List<ActData>();
            while (resDb.acts.Count < 3) resDb.acts.Add(null);
            resDb.acts[0] = act1;
            resDb.acts[1] = act2;
            resDb.acts[2] = act3;
            EditorUtility.SetDirty(resDb);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SetupLevelData] === THÀNH CÔNG: Đã khởi tạo {LevelDefs.Length} LevelData, 3 ActData, và LevelDatabase ===");
        }

        private static ActData SetupAct(int actIndex, string title, string subtitle, string unlockHint, Color accent, string[] enemyTypeIds, string mapBgPath, string[] bannerCandidates, Dictionary<int, LevelData> levels, int startLevelId, int endLevelId, string bossSpritePath = null)
        {
            string path = $"{ActsFolder}/Act_{actIndex + 1}.asset";
            ActData act = AssetDatabase.LoadAssetAtPath<ActData>(path);
            if (act == null)
            {
                act = ScriptableObject.CreateInstance<ActData>();
                AssetDatabase.CreateAsset(act, path);
            }

            act.actIndex = actIndex;
            act.title = title;
            act.subtitle = subtitle;
            act.unlockHint = unlockHint;
            act.accentColor = accent;
            act.enemyTypeIds = enemyTypeIds;

            if (!string.IsNullOrEmpty(bossSpritePath))
            {
                Sprite bSpr = LoadFirstSprite(bossSpritePath);
                if (bSpr != null) act.bossSprite = bSpr;
            }

            // Giữ sprite nếu đã gán tay; chỉ gán tự động nếu đang trống
            if (act.mapBackground == null && !string.IsNullOrEmpty(mapBgPath))
            {
                act.mapBackground = AssetDatabase.LoadAssetAtPath<Sprite>(mapBgPath);
                if (act.mapBackground == null)
                {
                    Debug.LogWarning($"[SetupLevelData] Không tìm thấy mapBackground tại {mapBgPath}");
                }
            }

            if (act.banner == null && bannerCandidates != null)
            {
                for (int i = 0; i < bannerCandidates.Length; i++)
                {
                    Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(bannerCandidates[i]);
                    if (s != null)
                    {
                        act.banner = s;
                        break;
                    }
                }
                if (act.banner == null)
                {
                    Debug.LogWarning($"[SetupLevelData] Không tìm thấy banner cho Hồi {actIndex + 1}");
                }
            }

            // Gán danh sách màn chơi (bảo tồn các màn bổ sung nếu có)
            if (act.levels == null) act.levels = new List<LevelData>();
            for (int id = startLevelId; id <= endLevelId; id++)
            {
                if (levels.TryGetValue(id, out LevelData lvl))
                {
                    int indexInList = id - startLevelId;
                    while (act.levels.Count <= indexInList) act.levels.Add(null);
                    act.levels[indexInList] = lvl;
                }
            }

            EditorUtility.SetDirty(act);
            return act;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder(LevelsFolder);
            EnsureFolder(ActsFolder);
            EnsureFolder(ResourcesFolder);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = System.IO.Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EnsureFolder(parent);
                }
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static Sprite LoadFirstSprite(string path)
        {
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets != null)
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is Sprite s) return s;
                }
            }
            return null;
        }
    }
}
