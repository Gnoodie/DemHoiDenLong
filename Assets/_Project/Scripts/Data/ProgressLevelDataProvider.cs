using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Triển khai ILevelDataProvider kết nối Tầng Dữ liệu thật (LevelDatabase) và Bộ quản lý tiến độ (LevelProgressManager).
    /// Chuẩn bị sẵn cho Giai đoạn 4 để hoán đổi thay thế MockLevelDataProvider trên giao diện.
    /// </summary>
    public class ProgressLevelDataProvider : MonoBehaviour, ILevelDataProvider
    {
        [Header("Data References")]
        [SerializeField] private LevelDatabase database;
        [SerializeField] private LevelProgressManager progressManager;

        [Header("Sprites Cache")]
        [SerializeField] private Sprite nodeBaseSprite;
        [SerializeField] private Sprite bossAct1Sprite;
        [SerializeField] private Sprite bossAct2Sprite;
        [SerializeField] private Sprite bossAct3Sprite;

        // Tọa độ chuẩn các node theo thiết kế map 3 Hồi
        private static readonly Vector2[][] DefaultNodeAnchors = new Vector2[][]
        {
            // Hồi 1 (Act 0)
            new Vector2[]
            {
                new Vector2(0.32f, 0.16f),
                new Vector2(0.68f, 0.32f),
                new Vector2(0.36f, 0.48f),
                new Vector2(0.65f, 0.62f),
                new Vector2(0.48f, 0.78f)
            },
            // Hồi 2 (Act 1)
            new Vector2[]
            {
                new Vector2(0.42f, 0.15f),
                new Vector2(0.64f, 0.30f),
                new Vector2(0.38f, 0.46f),
                new Vector2(0.66f, 0.63f),
                new Vector2(0.50f, 0.80f)
            },
            // Hồi 3 (Act 2)
            new Vector2[]
            {
                new Vector2(0.50f, 0.14f),
                new Vector2(0.68f, 0.28f),
                new Vector2(0.34f, 0.44f),
                new Vector2(0.64f, 0.60f),
                new Vector2(0.50f, 0.78f)
            }
        };

        private void Awake()
        {
            EnsureDependencies();
        }

        private void OnEnable()
        {
            EnsureDependencies();
            if (progressManager != null)
            {
                progressManager.OnProgressChanged += HandleProgressChanged;
            }
        }

        private void OnDisable()
        {
            if (progressManager != null)
            {
                progressManager.OnProgressChanged -= HandleProgressChanged;
            }
        }

        private void HandleProgressChanged()
        {
            OnDataChanged?.Invoke();
        }

        public void EnsureDependencies()
        {
            if (progressManager == null)
            {
                progressManager = LevelProgressManager.Instance;
            }

            if (database == null && progressManager != null)
            {
                database = progressManager.Database;
            }

            if (database == null)
            {
                database = Resources.Load<LevelDatabase>("LevelDatabase");
#if UNITY_EDITOR
                if (database == null)
                {
                    database = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");
                }
#endif
            }

#if UNITY_EDITOR
            if (nodeBaseSprite == null)
                nodeBaseSprite = LoadFirstSprite("Assets/_Project/Art/LevelSelect/node_base.png");
            if (bossAct1Sprite == null)
                bossAct1Sprite = LoadFirstSprite("Assets/_Project/Art/LevelSelect/node_boss_act1.png");
            if (bossAct2Sprite == null)
                bossAct2Sprite = LoadFirstSprite("Assets/_Project/Art/LevelSelect/node_boss_act2.png");
            if (bossAct3Sprite == null)
                bossAct3Sprite = LoadFirstSprite("Assets/_Project/Art/LevelSelect/node_boss_act3.png");
#endif
        }

#if UNITY_EDITOR
        private static Sprite LoadFirstSprite(string path)
        {
            var spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets != null)
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is Sprite s) return s;
                }
            }
            return null;
        }
#endif

        /// <summary>
        /// Chuyển đổi linh hoạt actIndex (hỗ trợ cả 0..2 và 1..3 từ UI caller).
        /// </summary>
        private int ToZeroBasedActIndex(int actIndex)
        {
            EnsureDependencies();
            int actCount = database != null ? database.ActCount : 3;
            if (actIndex >= 1 && actIndex <= actCount) return actIndex - 1;
            if (actIndex >= 0 && actIndex < actCount) return actIndex;
            return 0;
        }

        public List<LevelNodeData> GetLevelsForAct(int actIndex)
        {
            EnsureDependencies();
            List<LevelNodeData> list = new List<LevelNodeData>();
            int aIdx = ToZeroBasedActIndex(actIndex);

            if (database == null) return list;
            ActData act = database.GetAct(aIdx);
            if (act == null || act.levels == null) return list;

            var manager = progressManager ?? LevelProgressManager.Instance;
            Vector2[] anchors = (aIdx >= 0 && aIdx < DefaultNodeAnchors.Length) ? DefaultNodeAnchors[aIdx] : null;

            for (int i = 0; i < act.levels.Count; i++)
            {
                LevelData lvl = act.levels[i];
                if (lvl == null) continue;

                int lvlId = lvl.levelId;
                Vector2 pos = (anchors != null && i < anchors.Length) ? anchors[i] : new Vector2(0.5f, 0.15f + i * 0.15f);

                int starsRecord = manager.GetStars(lvlId);
                bool[] claimedArr = new bool[]
                {
                    manager.IsMilestoneClaimed(lvlId, 1),
                    manager.IsMilestoneClaimed(lvlId, 2),
                    manager.IsMilestoneClaimed(lvlId, 3)
                };

                MilestoneState[] mStates = new MilestoneState[3];
                for (int m = 1; m <= 3; m++)
                {
                    if (claimedArr[m - 1])
                    {
                        mStates[m - 1] = MilestoneState.Claimed;
                    }
                    else if (m <= starsRecord)
                    {
                        mStates[m - 1] = MilestoneState.Pending;
                    }
                    else
                    {
                        mStates[m - 1] = MilestoneState.Unachieved;
                    }
                }

                LevelNodeData node = new LevelNodeData
                {
                    levelId = lvlId,
                    levelName = lvl.isBoss ? $"{lvl.displayName} · {lvl.bossName}".ToUpper() : lvl.displayName,
                    isBoss = lvl.isBoss,
                    state = manager.GetNodeState(lvlId),
                    stars = starsRecord,
                    claimed = claimedArr,
                    anchorPos = pos,
                    actIndex = aIdx,
                    indexInAct = lvl.indexInAct,
                    displayName = lvl.displayName,
                    bossName = lvl.bossName,
                    maxStars = starsRecord,
                    milestoneStates = mStates,
                    winCondition = lvl.winCondition,
                    targetKillCount = lvl.winCondition == WinCondition.KillCount ? (lvl.targetKillCount > 0 ? lvl.targetKillCount : lvl.enemyCount) : lvl.targetKillCount,
                    surviveSeconds = lvl.surviveSeconds
                };

                list.Add(node);
            }

            return list;
        }

        public int GetActStars(int actIndex)
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetActProgress(ToZeroBasedActIndex(actIndex)).earnedStars;
        }

        public int GetActMaxStars(int actIndex)
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetActProgress(ToZeroBasedActIndex(actIndex)).maxStars;
        }

        public string GetActTitle(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return $"HỒI {actIndex}";
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            return act != null ? act.title : $"HỒI {actIndex}";
        }

        public string GetActSubtitle(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return "BẢN ĐỒ VƯỢT ẢI";
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            return act != null ? act.subtitle : "BẢN ĐỒ VƯỢT ẢI";
        }

        public Color GetActAccentColor(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return Color.white;
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            return act != null ? act.accentColor : Color.white;
        }

        public Sprite GetActBackground(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return null;
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            return act != null ? act.mapBackground : null;
        }

        public string GetBossName(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return "TRÙM CUỐI";
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            if (act != null && act.levels != null)
            {
                for (int i = act.levels.Count - 1; i >= 0; i--)
                {
                    if (act.levels[i] != null && act.levels[i].isBoss)
                    {
                        return act.levels[i].bossName;
                    }
                }
            }
            return "TRÙM CUỐI";
        }

        public Sprite GetBossSprite(int actIndex)
        {
            EnsureDependencies();
            int a = ToZeroBasedActIndex(actIndex);
            if (database != null)
            {
                ActData act = database.GetAct(a);
                if (act != null && act.bossSprite != null) return act.bossSprite;
            }
            switch (a)
            {
                case 0: return bossAct1Sprite;
                case 1: return bossAct2Sprite;
                case 2: return bossAct3Sprite;
                default: return bossAct1Sprite;
            }
        }

        public Sprite GetNodeBaseSprite() => nodeBaseSprite;

        // Các phương thức mở rộng
        public ActStatus GetActStatus(int actIndex)
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetActStatus(ToZeroBasedActIndex(actIndex));
        }

        public string GetUnlockHint(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return string.Empty;
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            return act != null ? act.unlockHint : string.Empty;
        }

        public int GetTotalStars()
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetTotalStars();
        }

        public int GetMaxTotalStars()
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetMaxTotalStars();
        }

        // Mở rộng cho Giai đoạn 4
        public int GetActCount()
        {
            EnsureDependencies();
            return database != null ? database.ActCount : 3;
        }

        public Sprite GetActBanner(int actIndex)
        {
            EnsureDependencies();
            if (database == null) return null;
            ActData act = database.GetAct(ToZeroBasedActIndex(actIndex));
            return act != null ? act.banner : null;
        }

        public int GetActClearedLevels(int actIndex)
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetActProgress(ToZeroBasedActIndex(actIndex)).clearedLevels;
        }

        public int GetActLevelCount(int actIndex)
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetActProgress(ToZeroBasedActIndex(actIndex)).totalLevels;
        }

        public int GetCurrentLevelId()
        {
            EnsureDependencies();
            var manager = progressManager ?? LevelProgressManager.Instance;
            return manager.GetCurrentLevelId();
        }

        public event System.Action OnDataChanged;
    }
}
