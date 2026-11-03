using System;
using UnityEngine;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.Data
{
    [System.Serializable]
    public struct ActProgress
    {
        public int clearedLevels;
        public int totalLevels;
        public int earnedStars;
        public int maxStars;
    }

    [System.Serializable]
    public struct LevelResult
    {
        public int levelId;
        public bool won;
        public int starsThisRun;
        public int previousMaxStars;
        public int newMaxStars;
        public bool isNewRecord;
        public int starsGranted;
        public int diamondsGranted;
        public bool[] newlyClaimed; // Độ dài 3 (mốc 1, 2, 3)
        public bool unlockedNextLevel;
        public bool unlockedNextAct;
        public bool rewardFailed;
        public bool rejected; // True nếu lượt báo kết quả bị từ chối do màn đang bị khóa
        public bool cappedByRevive; // True nếu lượt thắng này bị giới hạn tối đa 1 sao do đã dùng hồi sinh quảng cáo
        public System.Collections.Generic.List<ClaimPendingDetail> grantedDetails;

        public string GetGrantedSummary()
        {
            if (grantedDetails == null || grantedDetails.Count == 0) return "Không có mốc mới";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < grantedDetails.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var d = grantedDetails[i];
                sb.Append($"Mốc {d.milestone}: +{d.stars}★, +{d.diamonds}KC");
            }
            return sb.ToString();
        }
    }

    [System.Serializable]
    public struct ClaimPendingDetail
    {
        public int levelId;
        public int milestone;
        public int stars;
        public int diamonds;
    }

    [System.Serializable]
    public struct ClaimPendingResult
    {
        public int totalStarsGranted;
        public int totalDiamondsGranted;
        public int totalMilestonesClaimed;
        public System.Collections.Generic.List<ClaimPendingDetail> claimedDetails;
        public bool rewardFailed;

        public int starsGranted => totalStarsGranted;
        public int diamondsGranted => totalDiamondsGranted;
        public bool IsEmpty => totalMilestonesClaimed == 0 && totalStarsGranted == 0 && totalDiamondsGranted == 0;
    }

    /// <summary>
    /// Quản lý tiến độ toàn bộ chiến dịch: mở khóa màn/Hồi, lưu trữ sao (LevelStars), nhận quà tiền tệ (Star/Diamond).
    /// Singleton sống xuyên suốt (DontDestroyOnLoad) và an toàn khi khởi chạy từ bất kỳ Scene nào.
    /// </summary>
    public class LevelProgressManager : MonoBehaviour
    {
        private static LevelProgressManager instance;
        public static LevelProgressManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<LevelProgressManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("LevelProgressManager");
                        instance = go.AddComponent<LevelProgressManager>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private LevelDatabase database;

        private IProgressStorage storage;
        private IRewardSink rewardSink;

        public event Action OnProgressChanged;

        public LevelDatabase Database => database;
        public int TotalLevels => database != null ? database.TotalLevels : 0;
        public int ActCount => database != null ? database.ActCount : 0;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureInitialized();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Khởi tạo hoặc tiêm phụ thuộc (cho phép hoán đổi storage và sink khi chạy test).
        /// </summary>
        public void Initialize(LevelDatabase db = null, IProgressStorage customStorage = null, IRewardSink customSink = null)
        {
            if (db != null) database = db;
            if (database == null) LoadDatabaseFallback();

            storage = customStorage ?? new PlayerPrefsProgressStorage();
            rewardSink = customSink ?? new EconomyRewardSink();
        }

        private void EnsureInitialized()
        {
            if (storage == null) storage = new PlayerPrefsProgressStorage();
            if (rewardSink == null) rewardSink = new EconomyRewardSink();
            if (database == null) LoadDatabaseFallback();
        }

        private void LoadDatabaseFallback()
        {
            // 1. Thử load từ Resources
            database = Resources.Load<LevelDatabase>("LevelDatabase");

#if UNITY_EDITOR
            // 2. Fallback Editor load trực tiếp từ AssetDatabase
            if (database == null)
            {
                database = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");
            }
#endif

            if (database == null)
            {
                Debug.LogWarning("[LevelProgressManager] Chưa tìm thấy LevelDatabase asset! Hãy chạy menu 'DemHoiDenLong -> Setup LevelData from GDD'.");
            }
        }

        // ==========================================
        // KEY NAMES & STORAGE HELPERS
        // ==========================================
        public static string GetStarsKey(int levelId) => $"Level_{levelId}_Stars";
        public static string GetClaimedKey(int levelId) => $"Level_{levelId}_Claimed";

        /// <summary>
        /// Lấy số sao đánh giá (LevelStars 0..3) cao nhất đã đạt của màn chơi.
        /// </summary>
        public int GetStars(int levelId)
        {
            EnsureInitialized();
            if (levelId <= 0) return 0;
            int val = storage.GetInt(GetStarsKey(levelId), 0);
            return Mathf.Clamp(val, 0, 3);
        }

        /// <summary>
        /// Kiểm tra mốc phần thưởng milestone (1, 2, 3) của màn đã được nhận hay chưa.
        /// Sử dụng bitmask (bit 0 = mốc 1, bit 1 = mốc 2, bit 2 = mốc 3).
        /// </summary>
        public bool IsMilestoneClaimed(int levelId, int milestone)
        {
            EnsureInitialized();
            if (levelId <= 0 || milestone < 1 || milestone > 3) return false;
            int mask = storage.GetInt(GetClaimedKey(levelId), 0) & 7; // Ép chuẩn bitmask 3 bit
            return (mask & (1 << (milestone - 1))) != 0;
        }

        /// <summary>
        /// Kiểm tra màn chơi đã được mở khóa hay chưa.
        /// Luật: Màn 1 luôn mở; Màn k (k > 1) mở khi Màn k - 1 đạt >= 1 sao.
        /// </summary>
        public bool IsLevelUnlocked(int levelId)
        {
            EnsureInitialized();
            if (levelId <= 0)
            {
                Debug.LogWarning($"[LevelProgressManager] IsLevelUnlocked: levelId {levelId} không hợp lệ.");
                return false;
            }

            if (levelId == 1) return true;

            int total = TotalLevels;
            if (total > 0 && levelId > total)
            {
                Debug.LogWarning($"[LevelProgressManager] IsLevelUnlocked: levelId {levelId} vượt quá tổng số màn ({total}).");
                return false;
            }

            return GetStars(levelId - 1) >= 1;
        }

        /// <summary>
        /// Kiểm tra Hồi actIndex (0..N-1) đã mở khóa hay chưa (khi màn đầu tiên của Hồi mở khóa).
        /// </summary>
        public bool IsActUnlocked(int actIndex)
        {
            EnsureInitialized();
            if (database == null) return actIndex == 0;
            int firstLevel = database.GetFirstLevelId(actIndex);
            if (firstLevel <= 0) return false;
            return IsLevelUnlocked(firstLevel);
        }

        /// <summary>
        /// Lấy trạng thái hiển thị của Node trên bản đồ: Locked / Unlocked / Current (nhãn MỚI).
        /// Current là màn mở có levelId nhỏ nhất mà chưa có sao nào (0 sao).
        /// </summary>
        public NodeState GetNodeState(int levelId)
        {
            if (!IsLevelUnlocked(levelId)) return NodeState.Locked;

            int currentId = GetCurrentLevelId();
            if (currentId > 0 && levelId == currentId) return NodeState.Current;

            return NodeState.Unlocked;
        }

        /// <summary>
        /// Tìm màn đang mở có levelId nhỏ nhất mà chưa có sao nào (màn mang nhãn MỚI).
        /// Trả về -1 nếu toàn bộ các màn đã mở đều đã đạt >= 1 sao.
        /// </summary>
        public int GetCurrentLevelId()
        {
            EnsureInitialized();
            int total = TotalLevels;
            if (total <= 0) total = 15; // Dự phòng nếu chưa có database

            for (int i = 1; i <= total; i++)
            {
                if (IsLevelUnlocked(i))
                {
                    if (GetStars(i) == 0) return i;
                }
                else
                {
                    break; // Do màn i khóa nên các màn sau chắc chắn chưa mở
                }
            }

            return -1;
        }

        /// <summary>
        /// Tìm màn mang nhãn MỚI trong Hồi cụ thể. Trả về -1 nếu không có.
        /// </summary>
        public int GetCurrentLevelIdInAct(int actIndex)
        {
            EnsureInitialized();
            if (database == null) return -1;
            ActData act = database.GetAct(actIndex);
            if (act == null || act.levels == null) return -1;

            int currentGlobal = GetCurrentLevelId();
            if (currentGlobal <= 0) return -1;

            for (int i = 0; i < act.levels.Count; i++)
            {
                if (act.levels[i] != null && act.levels[i].levelId == currentGlobal)
                {
                    return currentGlobal;
                }
            }

            return -1;
        }

        /// <summary>
        /// Lấy trạng thái của Hồi: Locked / Completed / InProgress.
        /// Completed: Mọi màn trong Hồi đều đạt >= 1 sao.
        /// InProgress: Hồi đã mở nhưng chưa hoàn thành mọi màn.
        /// </summary>
        public ActStatus GetActStatus(int actIndex)
        {
            EnsureInitialized();
            if (!IsActUnlocked(actIndex)) return ActStatus.Locked;

            if (database != null)
            {
                ActData act = database.GetAct(actIndex);
                if (act != null && act.levels != null && act.levels.Count > 0)
                {
                    for (int i = 0; i < act.levels.Count; i++)
                    {
                        LevelData lvl = act.levels[i];
                        if (lvl != null && GetStars(lvl.levelId) < 1)
                        {
                            return ActStatus.InProgress;
                        }
                    }
                    return ActStatus.Completed;
                }
            }

            return ActStatus.InProgress;
        }

        /// <summary>
        /// Lấy tiến độ chi tiết của một Hồi (số màn đã vượt, tổng màn, sao đạt được, tổng sao tối đa).
        /// </summary>
        public ActProgress GetActProgress(int actIndex)
        {
            EnsureInitialized();
            ActProgress prog = new ActProgress();

            if (database != null)
            {
                ActData act = database.GetAct(actIndex);
                if (act != null && act.levels != null)
                {
                    prog.totalLevels = act.levels.Count;
                    prog.maxStars = prog.totalLevels * 3;

                    for (int i = 0; i < act.levels.Count; i++)
                    {
                        LevelData lvl = act.levels[i];
                        if (lvl != null)
                        {
                            int s = GetStars(lvl.levelId);
                            prog.earnedStars += s;
                            if (s >= 1) prog.clearedLevels++;
                        }
                    }
                }
            }

            return prog;
        }

        /// <summary>
        /// Tổng số sao đánh giá (LevelStars) người chơi đã tích lũy toàn game.
        /// </summary>
        public int GetTotalStars()
        {
            EnsureInitialized();
            int sum = 0;
            int total = TotalLevels;
            if (total <= 0) total = 15;

            for (int i = 1; i <= total; i++)
            {
                sum += GetStars(i);
            }
            return sum;
        }

        /// <summary>
        /// Tổng số sao tối đa toàn game (ví dụ 15 màn x 3 = 45).
        /// </summary>
        public int GetMaxTotalStars()
        {
            EnsureInitialized();
            int total = TotalLevels;
            if (total <= 0) total = 15;
            return total * 3;
        }

        // ==========================================
        // REPORT RESULT & PROGRESS RECORDING
        // ==========================================

        /// <summary>
        /// Ghi nhận kết quả màn chơi (Gameplay sẽ gọi hàm này).
        /// Chỉ chấp nhận màn đã mở (IsLevelUnlocked). Màn khóa: từ chối (rejected = true), không thưởng, không đổi tiến độ.
        /// Thắng: tính sao (nếu usedRevive = true: tối đa 1 sao), cập nhật kỷ lục, trao thưởng Sao & Kim Cương qua IRewardSink, mở màn/Hồi mới.
        /// Thua: không thưởng, không đổi tiến độ.
        /// </summary>
        public LevelResult ReportLevelResult(int levelId, bool won, float hpRemaining01, bool usedRevive = false)
        {
            return ReportLevelResultInternal(levelId, won, hpRemaining01, force: false, usedRevive: usedRevive);
        }

        /// <summary>
        /// Phiên bản dành riêng cho công cụ thử (Progress Debug window và panel F9) với cờ force.
        /// </summary>
        public LevelResult ReportLevelResultForDebug(int levelId, bool won, float hpRemaining01, bool force = true, bool usedRevive = false)
        {
            return ReportLevelResultInternal(levelId, won, hpRemaining01, force, usedRevive);
        }

        private LevelResult ReportLevelResultInternal(int levelId, bool won, float hpRemaining01, bool force, bool usedRevive)
        {
            EnsureInitialized();

            LevelResult result = new LevelResult
            {
                levelId = levelId,
                won = won,
                newlyClaimed = new bool[3],
                rejected = false,
                cappedByRevive = won && usedRevive,
                grantedDetails = new System.Collections.Generic.List<ClaimPendingDetail>()
            };

            if (levelId <= 0)
            {
                Debug.LogWarning($"[LevelProgressManager] ReportLevelResult: levelId {levelId} không hợp lệ.");
                result.rejected = true;
                return result;
            }

            // Chặn báo kết quả màn đang bị khóa (trừ khi dùng force từ công cụ thử)
            if (!force && !IsLevelUnlocked(levelId))
            {
                Debug.Log($"[LevelProgressManager] ReportLevelResult: Màn {levelId} đang bị khóa! Lượt chơi bị từ chối.");
                result.rejected = true;
                result.starsThisRun = 0;
                result.previousMaxStars = GetStars(levelId);
                result.newMaxStars = result.previousMaxStars;
                return result;
            }

            if (!won)
            {
                result.starsThisRun = 0;
                result.previousMaxStars = GetStars(levelId);
                result.newMaxStars = result.previousMaxStars;
                return result;
            }

            // Ghi nhận trạng thái mở khóa TRƯỚC KHI cập nhật để so sánh mở màn/Hồi mới
            int total = TotalLevels;
            bool nextLevelWasUnlocked = (levelId < total) && IsLevelUnlocked(levelId + 1);

            int currentActIndex = -1;
            bool nextActWasUnlocked = true;
            if (database != null)
            {
                ActData act = database.GetActOfLevel(levelId);
                if (act != null)
                {
                    currentActIndex = act.actIndex;
                    if (currentActIndex + 1 < ActCount)
                    {
                        nextActWasUnlocked = IsActUnlocked(currentActIndex + 1);
                    }
                }
            }

            // 1. Tính số sao đạt được ở lượt chơi này (có tính cờ usedRevive: cap 1 sao)
            result.starsThisRun = LevelRewards.CalcStars(true, hpRemaining01, usedRevive);
            result.previousMaxStars = GetStars(levelId);
            result.newMaxStars = Mathf.Max(result.previousMaxStars, result.starsThisRun);
            result.isNewRecord = (result.newMaxStars > result.previousMaxStars);

            // Cập nhật kỷ lục sao nếu có kỷ lục mới
            if (result.isNewRecord)
            {
                storage.SetInt(GetStarsKey(levelId), result.newMaxStars);
            }

            // 2. Trao phần thưởng theo KỶ LỤC (newMaxStars): mọi mốc m <= newMaxStars chưa từng nhận
            int currentClaimedMask = storage.GetInt(GetClaimedKey(levelId), 0) & 7;
            int newClaimedMask = currentClaimedMask;

            for (int m = 1; m <= 3; m++)
            {
                int bit = 1 << (m - 1);
                // Mốc m được thưởng nếu m <= newMaxStars VÀ mốc m chưa từng nhận
                if (m <= result.newMaxStars && (newClaimedMask & bit) == 0)
                {
                    if (rewardSink == null || !rewardSink.IsReady)
                    {
                        result.rewardFailed = true;
                        Debug.Log($"[LevelProgressManager] RewardSink chưa sẵn sàng. Hoãn nhận thưởng mốc {m} màn {levelId}, chưa đánh dấu đã nhận.");
                        continue;
                    }

                    int starReward = LevelRewards.StarReward(levelId, m);
                    int diamondReward = LevelRewards.DiamondReward(levelId, m);

                    // Đặt bit đã nhận vào storage TRƯỚC khi cấp thưởng:
                    // (để PlayerPrefs.Save() bên trong EconomyManager ghi cùng lúc cả 2 dữ liệu)
                    storage.SetInt(GetClaimedKey(levelId), newClaimedMask | bit);

                    bool grantSuccess = false;
                    try
                    {
                        grantSuccess = rewardSink.TryGrantReward(starReward, diamondReward);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[LevelProgressManager] Lỗi cấp thưởng mốc {m} màn {levelId}: {ex.Message}");
                        grantSuccess = false;
                    }

                    if (!grantSuccess)
                    {
                        // Cấp lỗi: hoàn lại bit vừa đặt trong storage
                        storage.SetInt(GetClaimedKey(levelId), newClaimedMask);
                        result.rewardFailed = true;
                    }
                    else
                    {
                        newClaimedMask |= bit;
                        result.starsGranted += starReward;
                        result.diamondsGranted += diamondReward;
                        result.newlyClaimed[m - 1] = true;
                        result.grantedDetails.Add(new ClaimPendingDetail
                        {
                            levelId = levelId,
                            milestone = m,
                            stars = starReward,
                            diamonds = diamondReward
                        });
                        Debug.Log($"[LevelProgressManager] Cấp thưởng màn {levelId} mốc {m}: +{starReward} Sao, +{diamondReward} Kim Cương.");
                    }
                }
            }

            // Lưu bitmask mốc đã nhận (chỉ lưu nếu có thay đổi)
            if (newClaimedMask != currentClaimedMask)
            {
                storage.SetInt(GetClaimedKey(levelId), newClaimedMask);
            }

            storage.Save();

            // 3. Kiểm tra xem có vừa mở khóa màn tiếp theo hoặc Hồi tiếp theo không
            if (levelId < total)
            {
                result.unlockedNextLevel = !nextLevelWasUnlocked && IsLevelUnlocked(levelId + 1);
            }

            if (currentActIndex >= 0 && currentActIndex + 1 < ActCount)
            {
                result.unlockedNextAct = !nextActWasUnlocked && IsActUnlocked(currentActIndex + 1);
            }

            OnProgressChanged?.Invoke();

            return result;
        }

        /// <summary>
        /// Quét mọi màn có kỷ lục > 0 mà còn mốc chưa nhận, tiến hành trả bù.
        /// Hàm này Idempotent: gọi liên tiếp hai lần thì lần hai trả 0 (IsEmpty = true).
        /// </summary>
        public ClaimPendingResult ClaimPendingRewards()
        {
            EnsureInitialized();

            ClaimPendingResult result = new ClaimPendingResult
            {
                claimedDetails = new System.Collections.Generic.List<ClaimPendingDetail>()
            };

            if (rewardSink == null || !rewardSink.IsReady)
            {
                result.rewardFailed = true;
                return result;
            }

            int total = TotalLevels;
            int maxLevel = total > 0 ? total : 15;
            bool anyChanged = false;

            for (int lvl = 1; lvl <= maxLevel; lvl++)
            {
                int maxStars = GetStars(lvl);
                if (maxStars <= 0) continue;

                int currentClaimedMask = storage.GetInt(GetClaimedKey(lvl), 0) & 7;
                int newClaimedMask = currentClaimedMask;

                for (int m = 1; m <= maxStars; m++)
                {
                    int bit = 1 << (m - 1);
                    if ((newClaimedMask & bit) == 0)
                    {
                        if (rewardSink == null || !rewardSink.IsReady)
                        {
                            result.rewardFailed = true;
                            break;
                        }

                        int starReward = LevelRewards.StarReward(lvl, m);
                        int diamondReward = LevelRewards.DiamondReward(lvl, m);

                        // Đặt bit đã nhận vào storage TRƯỚC khi cấp thưởng:
                        // (để PlayerPrefs.Save() bên trong EconomyManager ghi cùng lúc cả 2 dữ liệu)
                        storage.SetInt(GetClaimedKey(lvl), newClaimedMask | bit);

                        bool grantSuccess = false;
                        try
                        {
                            grantSuccess = rewardSink.TryGrantReward(starReward, diamondReward);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[LevelProgressManager] ClaimPendingRewards exception màn {lvl} mốc {m}: {ex.Message}");
                            grantSuccess = false;
                        }

                        if (!grantSuccess)
                        {
                            // Cấp lỗi: hoàn lại bit vừa đặt trong storage
                            storage.SetInt(GetClaimedKey(lvl), newClaimedMask);
                            result.rewardFailed = true;
                            break;
                        }
                        else
                        {
                            newClaimedMask |= bit;
                            result.totalStarsGranted += starReward;
                            result.totalDiamondsGranted += diamondReward;
                            result.totalMilestonesClaimed++;
                            result.claimedDetails.Add(new ClaimPendingDetail
                            {
                                levelId = lvl,
                                milestone = m,
                                stars = starReward,
                                diamonds = diamondReward
                            });
                        }
                    }
                }

                if (newClaimedMask != currentClaimedMask)
                {
                    storage.SetInt(GetClaimedKey(lvl), newClaimedMask);
                    anyChanged = true;
                }
            }

            if (anyChanged)
            {
                storage.Save();
                OnProgressChanged?.Invoke();
            }

            return result;
        }

        // ==========================================
        // DEBUG & CHEAT UTILITIES (Dùng để kiểm tra)
        // ==========================================

        /// <summary>
        /// Xóa sạch toàn bộ tiến độ chơi (Reset về ban đầu).
        /// </summary>
        public void ResetAll()
        {
            EnsureInitialized();
            int total = TotalLevels;
            if (total <= 0) total = 15;

            for (int i = 1; i <= total; i++)
            {
                storage.DeleteKey(GetStarsKey(i));
                storage.DeleteKey(GetClaimedKey(i));
            }

            storage.Save();
            OnProgressChanged?.Invoke();
            Debug.Log("[LevelProgressManager] Đã Reset toàn bộ tiến độ chơi về mặc định.");
        }

        /// <summary>
        /// Mở khóa toàn bộ các màn chơi với số sao chỉ định (không trao thưởng tiền tệ).
        /// </summary>
        public void UnlockAllWithStars(int stars)
        {
            EnsureInitialized();
            stars = Mathf.Clamp(stars, 1, 3);
            int total = TotalLevels;
            if (total <= 0) total = 15;

            for (int i = 1; i <= total; i++)
            {
                storage.SetInt(GetStarsKey(i), stars);
            }

            storage.Save();
            OnProgressChanged?.Invoke();
            Debug.Log($"[LevelProgressManager] Đã mở khóa toàn bộ {total} màn với {stars} sao.");
        }

        /// <summary>
        /// Mở khóa toàn bộ các màn chơi (3 sao).
        /// </summary>
        public void UnlockAll() => UnlockAllWithStars(3);

        /// <summary>
        /// Gán trực tiếp số sao cho một màn (dùng để test nhanh, không trao thưởng).
        /// </summary>
        public void SetStarsDirect(int levelId, int stars)
        {
            EnsureInitialized();
            if (levelId <= 0) return;
            stars = Mathf.Clamp(stars, 0, 3);
            storage.SetInt(GetStarsKey(levelId), stars);
            storage.Save();
            OnProgressChanged?.Invoke();
        }

        /// <summary>
        /// Gán trực tiếp trạng thái đã nhận của một mốc (dùng để test nhanh).
        /// </summary>
        public void SetMilestoneClaimedDirect(int levelId, int milestone, bool claimed)
        {
            EnsureInitialized();
            if (levelId <= 0 || milestone < 1 || milestone > 3) return;
            int mask = storage.GetInt(GetClaimedKey(levelId), 0) & 7;
            if (claimed) mask |= (1 << (milestone - 1));
            else mask &= ~(1 << (milestone - 1));

            storage.SetInt(GetClaimedKey(levelId), mask);
            storage.Save();
            OnProgressChanged?.Invoke();
        }
    }
}
