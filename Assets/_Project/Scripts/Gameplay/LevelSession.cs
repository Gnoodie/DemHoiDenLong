using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Chỉ số thời gian chạy của Lân, tính từ chỉ số cơ bản + cấp nâng cấp.
    /// Nguồn: Sheet "Nâng cấp & Kinh tế" của Trung_Thu_Shooter_Balance.xlsx.
    /// </summary>
    public class LanStats
    {
        public float hp;
        public float damage;
        public float fireRate;

        public static LanStats Compute(string lanId, LanData lan = null)
        {
            if (lan == null) lan = LanCatalog.GetLan(lanId);

            int rawHp = PlayerPrefs.GetInt($"UpgradeLevel_{lanId}_HP", 1);
            int rawDmg = PlayerPrefs.GetInt($"UpgradeLevel_{lanId}_Damage", 1);
            int rawFr = PlayerPrefs.GetInt($"UpgradeLevel_{lanId}_FireRate", 1);

            int capHp = Mathf.Clamp(rawHp > 0 ? rawHp - 1 : 0, 0, GameplayBalance.LanMaxUpgradeLevel);
            int capDmg = Mathf.Clamp(rawDmg > 0 ? rawDmg - 1 : 0, 0, GameplayBalance.LanMaxUpgradeLevel);
            int capFr = Mathf.Clamp(rawFr > 0 ? rawFr - 1 : 0, 0, GameplayBalance.LanMaxUpgradeLevel);

            float baseHp = lan != null ? lan.maxHp : 100f;
            float baseDmg = lan != null ? lan.baseDamage : 10f;
            float baseFr = lan != null ? lan.fireRate : 2.5f;

            return new LanStats
            {
                hp = baseHp + GameplayBalance.LanHpPerLevel * capHp,
                damage = baseDmg + GameplayBalance.LanDamagePerLevel * capDmg,
                fireRate = baseFr + GameplayBalance.LanFireRatePerLevel * capFr
            };
        }

        public static LanStats Calculate(string lanId, LanData lan)
        {
            return Compute(lanId, lan);
        }
    }

    public enum EnemyKind
    {
        PaperLantern,
        SpikedLantern,
        CarpLantern,
        GhostLantern,
        Boss,
        SpikeDebris,
        Unknown
    }

    public enum KillCause
    {
        PlayerBullet,
        PlayerSkill,
        ShieldReflect,
        CollisionWithPlayer,
        ScreenExit,
        DebugF9,
        Other
    }

    /// <summary>
    /// Giữ trạng thái phiên chơi hiện tại trong Gameplay, kết nối từ LevelSelect sang Gameplay.
    /// </summary>
    public static class LevelSession
    {
        public static LevelData level;
        public static LanData lan;
        public static LanStats stats;

        public static int runStars;
        public static bool usedRevive;
        public static bool ended;
        public static bool resultReported;

        // Telemetry & Run tracking (Giai đoạn 5A.1)
        public static int currentKills;
        public static float elapsedTime;
        public static int Kills => currentKills;
        public static int killsCount => currentKills;
        public static float elapsedSeconds => elapsedTime;
        public static int countPaper;
        public static int countSpike;
        public static int countCarp;
        public static int countGhost;
        public static int starsDropped;
        public static int starsCollected;

        public static event System.Action<int, int> OnKillsChanged;
        private static int killTraceCount = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitSubsystem()
        {
            ResetAllStatic();
        }

        public static void ResetAllStatic()
        {
            Reset();
            level = null;
            lan = null;
            stats = null;
            OnKillsChanged = null;
        }

        /// <summary>
        /// Quy ước đếm hạ địch (Single Source of Truth):
        /// - CHỈ đếm địch chết do SÁT THƯƠNG của người chơi (PlayerBullet, PlayerSkill, ShieldReflect, DebugF9).
        /// - KHÔNG đếm: địch rời màn hình (ScreenExit), mảnh vỡ đèn gai, Boss (Boss dùng BossKill riêng).
        /// - Địch tông vào người chơi rồi chết (CollisionWithPlayer): KHÔNG đếm và KHÔNG rơi sao.
        /// - Mỗi địch chỉ đếm tối đa 1 lần.
        /// </summary>
        public static bool ShouldCountKill(EnemyKind kind, KillCause cause)
        {
            if (kind == EnemyKind.Boss || kind == EnemyKind.SpikeDebris) return false;
            switch (cause)
            {
                case KillCause.PlayerBullet:
                case KillCause.PlayerSkill:
                case KillCause.ShieldReflect:
                case KillCause.DebugF9:
                    return true;
                default:
                    return false;
            }
        }

        public static bool RegisterKill(EnemyKind kind, KillCause cause)
        {
            bool willCount = ShouldCountKill(kind, cause);
            int prev = currentKills;

            if (willCount)
            {
                currentKills++;
                int target = level != null ? level.targetKillCount : 0;
                OnKillsChanged?.Invoke(currentKills, target);

                if (killTraceCount < 5)
                {
                    killTraceCount++;
                    Debug.Log($"[KILLTRACE] Counter: before={prev}, after={currentKills}, target={target}, cause={cause}");
                }

                // Kiểm tra thắng KillCount ngay lập tức khi đủ
                if (level != null && level.winCondition == WinCondition.KillCount)
                {
                    if (currentKills >= target)
                    {
                        if (WaveManager.Instance != null && !WaveManager.Instance.HasTriggeredEnd)
                        {
                            WaveManager.Instance.TriggerLevelCompleted("KillCount");
                        }
                        else if (GameManager.Instance != null)
                        {
                            GameManager.Instance.TriggerVictory("KillCount");
                        }
                    }
                }
            }
            return willCount;
        }

        public static void SetKillsForDebug(int kills)
        {
            currentKills = Mathf.Max(0, kills);
            int target = level != null ? level.targetKillCount : 0;
            OnKillsChanged?.Invoke(currentKills, target);

            if (level != null && level.winCondition == WinCondition.KillCount && currentKills >= target)
            {
                if (WaveManager.Instance != null && !WaveManager.Instance.HasTriggeredEnd)
                {
                    WaveManager.Instance.TriggerLevelCompleted("KillCount");
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory("KillCount");
                }
            }
        }

        public static void Initialize(int levelId, string lanId = null)
        {
            Reset();

            // 1. Nạp LevelData
            LevelDatabase db = Resources.Load<LevelDatabase>("LevelDatabase");
#if UNITY_EDITOR
            if (db == null)
            {
                db = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");
            }
#endif
            if (db != null)
            {
                level = db.GetLevel(levelId);
            }

#if UNITY_EDITOR
            if (level == null)
            {
                string path = $"Assets/_Project/Data/Levels/Level_{levelId:D2}.asset";
                level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level == null)
                {
                    path = $"Assets/_Project/Data/Levels/SO_Level_{levelId:D2}.asset";
                    level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(path);
                }
            }
#endif

            // 2. Nạp LanData
            if (!string.IsNullOrEmpty(lanId))
            {
                lan = LanCatalog.GetLan(lanId);
                if (lan == null || !LanCatalog.IsLanUnlocked(lan))
                {
                    lan = LanCatalog.GetPlayableLan();
                }
            }
            else
            {
                lan = LanCatalog.GetPlayableLan();
            }

            string actualLanId = lan != null ? lan.lanId : "1";
            stats = LanStats.Compute(actualLanId, lan);
            int capHp = Mathf.Clamp(PlayerPrefs.GetInt($"UpgradeLevel_{actualLanId}_HP", 1) - 1, 0, 10);
            int capDmg = Mathf.Clamp(PlayerPrefs.GetInt($"UpgradeLevel_{actualLanId}_Damage", 1) - 1, 0, 10);
            int capFr = Mathf.Clamp(PlayerPrefs.GetInt($"UpgradeLevel_{actualLanId}_FireRate", 1) - 1, 0, 10);
            Debug.Log($"[STATS] Gameplay lan={actualLanId} level={capHp} hp={stats.hp} dmg={stats.damage} fr={stats.fireRate}");

            Debug.Log($"[LevelSession] Khởi tạo phiên chơi: Màn={levelId} ({level?.displayName}), Lân={lan?.lanName} (id={actualLanId}), HP={stats.hp}, DMG={stats.damage}, FireRate={stats.fireRate}");
        }

        public static void Reset()
        {
            runStars = 0;
            usedRevive = false;
            ended = false;
            resultReported = false;

            currentKills = 0;
            elapsedTime = 0f;
            countPaper = 0;
            countSpike = 0;
            countCarp = 0;
            countGhost = 0;
            starsDropped = 0;
            starsCollected = 0;
            killTraceCount = 0;
        }

        public static void RecordEnemySpawned(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) { countPaper++; return; }
            string lower = enemyId.ToLower();
            if (lower.Contains("gai") || lower.Contains("spike")) countSpike++;
            else if (lower.Contains("ca") || lower.Contains("carp")) countCarp++;
            else if (lower.Contains("ma") || lower.Contains("quy") || lower.Contains("ghost")) countGhost++;
            else countPaper++;
        }

        public static void RecordStarDropped(int amount)
        {
            if (amount > 0) starsDropped += amount;
        }

        public static void RecordStarCollected(int amount)
        {
            if (amount > 0) starsCollected += amount;
        }

        public static void LogRunEnd(bool won, string reason, int levelId, PlayerController player, int starsCount)
        {
            int hearts = player != null ? player.LivesCount : 0;
            float curHp = player != null ? player.CurrentHp : 0f;
            float maxHp = player != null ? player.MaxHp : 100f;
            float hp01 = player != null ? LevelRules.CalcHpRemaining01(curHp, maxHp, hearts, LevelRules.MaxHearts) : 0f;

            string killsPart = "";
            if (reason == "KillCount")
            {
                int target = level != null ? level.targetKillCount : 25;
                killsPart = $"kills={currentKills}/{target} ";
            }

            string timePart = "";
            if (reason == "SurviveTime")
            {
                int targetSec = level != null ? Mathf.RoundToInt(level.surviveSeconds) : 51;
                timePart = $"time={Mathf.RoundToInt(elapsedTime)}/{targetSec} ";
            }
            else
            {
                timePart = $"time={elapsedTime:F1}s ";
            }

            string resultStr = won ? "WIN" : "LOSE";
            Debug.Log($"[RUN END] level={levelId} result={resultStr} reason={reason} {killsPart}{timePart}hearts={hearts} hp={Mathf.RoundToInt(curHp)}/{Mathf.RoundToInt(maxHp)} hp01={hp01:F2} stars={starsCount} usedRevive={usedRevive} runStars={runStars}");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogTelemetry(levelId);
#endif
        }

        public static void LogTelemetry(int levelId)
        {
            int totalEnemies = countPaper + countSpike + countCarp + countGhost;
            float ratio = starsDropped > 0 ? (float)starsCollected / starsDropped : 0f;
            Debug.Log($"[TELEMETRY] level={levelId} enemies=[Giấy:{countPaper}, Gai:{countSpike}, Cá:{countCarp}, Ma:{countGhost}, Tổng:{totalEnemies}] starsDropped={starsDropped} starsCollected={starsCollected} pickupRatio={ratio:P1}");
        }

        /// <summary>
        /// Cộng Sao vào ví người chơi an toàn. Nếu EconomyManager chưa sẵn sàng, lưu tạm vào PlayerPrefs không để mất.
        /// </summary>
        public static void AwardStars(int amount)
        {
            if (amount <= 0) return;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddStar(amount);
            }
            else
            {
                int currentStars = PlayerPrefs.GetInt("Player_Stars", 1000);
                PlayerPrefs.SetInt("Player_Stars", currentStars + amount);
                int pending = PlayerPrefs.GetInt("PendingStars", 0);
                PlayerPrefs.SetInt("PendingStars", pending + amount);
                PlayerPrefs.Save();
                Debug.Log($"[LevelSession] EconomyManager null: Đệm +{amount} Sao vào PlayerPrefs (tổng Player_Stars: {currentStars + amount})");
            }
        }
    }
}
