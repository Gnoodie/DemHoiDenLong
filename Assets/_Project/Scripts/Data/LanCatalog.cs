using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Helper cung cấp danh mục Lân cho toàn game (đọc cùng thư mục với HangarUI, không sửa HangarUI).
    /// </summary>
    public static class LanCatalog
    {
        private static List<LanData> cachedLans;

        public static List<LanData> GetAllLans()
        {
            if (cachedLans == null || cachedLans.Count == 0)
            {
                cachedLans = new List<LanData>();

#if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LanData");
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    LanData lan = UnityEditor.AssetDatabase.LoadAssetAtPath<LanData>(path);
                    if (lan != null && !cachedLans.Contains(lan))
                    {
                        cachedLans.Add(lan);
                    }
                }
#endif
                if (cachedLans.Count == 0)
                {
                    // Fallback load từ Resources nếu có
                    var loaded = Resources.LoadAll<LanData>("Lans");
                    if (loaded != null && loaded.Length > 0)
                    {
                        cachedLans.AddRange(loaded);
                    }
                }

                // Sắp xếp theo lanId ("1", "2", "3", "4")
                cachedLans.Sort((a, b) => string.Compare(a.lanId, b.lanId, System.StringComparison.Ordinal));
            }

            return cachedLans;
        }

        public static LanData GetLan(string lanId)
        {
            var list = GetAllLans();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].lanId == lanId)
                    return list[i];
            }
            return list.Count > 0 ? list[0] : null;
        }

        public static LanData GetSelectedLan()
        {
            string selectedId = PlayerPrefs.GetString("SelectedLan", "1");
            LanData lan = GetLan(selectedId);
            if (lan == null)
            {
                var list = GetAllLans();
                if (list.Count > 0) lan = list[0];
            }
            return lan;
        }

        /// <summary>
        /// Kiểm tra xem Lân đã được mở khóa hay chưa (theo isUnlockedByDefault hoặc PlayerPrefs "Unlocked_LanId_{id}").
        /// </summary>
        public static bool IsLanUnlocked(LanData lan)
        {
            if (lan == null) return false;
            if (lan.isUnlockedByDefault) return true;
            return PlayerPrefs.GetInt("Unlocked_LanId_" + lan.lanId, 0) == 1;
        }

        /// <summary>
        /// Lấy Lân hợp lệ để chơi: đọc SelectedLan; nếu Lân đó chưa mở khóa thì dùng Lân đã mở khóa gần nhất, cuối cùng là Kim Lân (luôn mở).
        /// </summary>
        public static LanData GetPlayableLan()
        {
            var list = GetAllLans();
            if (list == null || list.Count == 0) return null;

            string selectedId = PlayerPrefs.GetString("SelectedLan", "1");
            LanData selectedLan = GetLan(selectedId);
            if (selectedLan != null && IsLanUnlocked(selectedLan))
            {
                return selectedLan;
            }

            // Nếu Lân được chọn chưa mở khóa, fallback về Lân đã mở khóa gần nhất
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] != null && IsLanUnlocked(list[i]))
                {
                    return list[i];
                }
            }

            // Cuối cùng fallback về Kim Lân (id "1", luôn mở mặc định)
            LanData kimLan = GetLan("1");
            return kimLan ?? list[0];
        }
    }
}
