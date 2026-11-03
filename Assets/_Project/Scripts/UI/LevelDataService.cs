using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Nguồn cung cấp ILevelDataProvider duy nhất cho toàn bộ UI (ActSelectView, LevelDetailView, Popup).
    /// Mặc định dùng ProgressLevelDataProvider (dữ liệu thật).
    /// </summary>
    public static class LevelDataService
    {
        private static ILevelDataProvider customProvider;

        public static void SetCustomProvider(ILevelDataProvider provider)
        {
            customProvider = provider;
        }

        public static ILevelDataProvider Provider
        {
            get
            {
                if (customProvider != null) return customProvider;

                var existing = Object.FindAnyObjectByType<ProgressLevelDataProvider>();
                if (existing != null)
                {
                    customProvider = existing;
                    return customProvider;
                }

                // Tự động gắn ProgressLevelDataProvider nếu chưa có
                GameObject go = GameObject.Find("LevelProgressManager");
                if (go == null) go = new GameObject("LevelDataProvider");
                var p = go.GetComponent<ProgressLevelDataProvider>();
                if (p == null) p = go.AddComponent<ProgressLevelDataProvider>();
                customProvider = p;
                return customProvider;
            }
        }
    }
}
