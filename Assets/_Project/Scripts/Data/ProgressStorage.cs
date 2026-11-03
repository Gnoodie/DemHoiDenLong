using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Giao diện trừu tượng hóa việc lưu trữ tiến độ (PlayerPrefs hoặc In-Memory test).
    /// </summary>
    public interface IProgressStorage
    {
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
        void Save();
        void DeleteKey(string key);
    }

    /// <summary>
    /// Lưu trữ tiến độ thật bằng PlayerPrefs của Unity.
    /// </summary>
    public class PlayerPrefsProgressStorage : IProgressStorage
    {
        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public void Save() => PlayerPrefs.Save();
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
    }

    /// <summary>
    /// Lưu trữ tiến độ tạm thời trong bộ nhớ (dùng cho Self Test và Debug giả lập không ảnh hưởng save thật).
    /// </summary>
    public class InMemoryProgressStorage : IProgressStorage
    {
        private readonly Dictionary<string, int> storage = new Dictionary<string, int>();

        public int GetInt(string key, int defaultValue)
        {
            return storage.TryGetValue(key, out int v) ? v : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            storage[key] = value;
        }

        public void Save()
        {
            // In-memory không cần ghi đĩa
        }

        public void DeleteKey(string key)
        {
            storage.Remove(key);
        }

        public void Clear()
        {
            storage.Clear();
        }

        public int KeyCount => storage.Count;
    }

    /// <summary>
    /// Giao diện tiếp nhận phần thưởng tiền tệ (Sao, Kim Cương).
    /// </summary>
    public interface IRewardSink
    {
        bool IsReady { get; }
        bool IsAvailable { get; }
        void AddStar(int amount);
        void AddDiamond(int amount);
        bool TryGrantReward(int starAmount, int diamondAmount);
    }

    /// <summary>
    /// RewardSink thật gọi vào EconomyManager của project.
    /// Nếu EconomyManager chưa tồn tại (ví dụ chạy thẳng từ LevelSelect), IsReady/IsAvailable trả về false.
    /// </summary>
    public class EconomyRewardSink : IRewardSink
    {
        public bool IsReady => EconomyManager.Instance != null;
        public bool IsAvailable => IsReady;

        public void AddStar(int amount)
        {
            if (amount <= 0) return;
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddStar(amount);
            }
            else
            {
                Debug.LogError("[EconomyRewardSink] EconomyManager.Instance chưa tồn tại! Không thể cộng Sao.");
            }
        }

        public void AddDiamond(int amount)
        {
            if (amount <= 0) return;
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddDiamond(amount);
            }
            else
            {
                Debug.LogError("[EconomyRewardSink] EconomyManager.Instance chưa tồn tại! Không thể cộng Kim Cương.");
            }
        }

        /// <summary>
        /// Cấp thưởng nguyên tử cho một mốc (cả sao lẫn kim cương hoặc không gì cả).
        /// </summary>
        public bool TryGrantReward(int starAmount, int diamondAmount)
        {
            if (!IsReady) return false;

            try
            {
                if (starAmount > 0 && diamondAmount > 0)
                {
                    EconomyManager.Instance.AddStar(starAmount);
                    try
                    {
                        EconomyManager.Instance.AddDiamond(diamondAmount);
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        // Hoàn lại Sao nếu AddDiamond lỗi để đảm bảo không cấp một nửa
                        EconomyManager.Instance.SpendStar(starAmount);
                        Debug.LogError($"[EconomyRewardSink] Lỗi khi cấp Kim Cương: {ex.Message}. Đã hoàn lại {starAmount} Sao.");
                        return false;
                    }
                }
                else if (starAmount > 0)
                {
                    EconomyManager.Instance.AddStar(starAmount);
                    return true;
                }
                else if (diamondAmount > 0)
                {
                    EconomyManager.Instance.AddDiamond(diamondAmount);
                    return true;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EconomyRewardSink] Lỗi cấp thưởng: {ex.Message}");
                return false;
            }
        }
    }
}
