using UnityEngine;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Đảm bảo LevelSession luôn được khởi tạo khi chạy trực tiếp Scene Gameplay trong Editor hoặc Dev Build.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class LevelSessionBootstrap : MonoBehaviour
    {
        [Header("Editor Direct Play Debug Configuration")]
        [SerializeField] private int debugLevelId = 1;
        [SerializeField] private string debugLanId = "1";

        private void Awake()
        {
            // Nếu đã được đặt từ LevelSelect (LevelSelectContext.SelectedLevelId)
            int targetLevelId = LevelSelectContext.SelectedLevelId;
            if (targetLevelId <= 0)
            {
                targetLevelId = debugLevelId;
                Debug.LogWarning($"[LevelSessionBootstrap] LevelSession chưa được khởi tạo từ LevelSelect! Chạy chế độ thử nghiệm trực tiếp: Màn {debugLevelId}, Lân ID {debugLanId}.");
                LevelSelectContext.SelectedLevelId = debugLevelId;
                LevelSelectContext.LastPlayedLevelId = debugLevelId;
            }

            string actualLanId = LevelSelectContext.SelectedLanId.ToString();
            if (string.IsNullOrEmpty(actualLanId) || actualLanId == "0")
            {
                actualLanId = PlayerPrefs.GetString("SelectedLan", debugLanId);
            }
            if (string.IsNullOrEmpty(actualLanId)) actualLanId = debugLanId;

            if (LevelSession.level == null || LevelSession.level.levelId != targetLevelId || LevelSession.lan == null || LevelSession.lan.lanId != actualLanId)
            {
                LevelSession.Initialize(targetLevelId, actualLanId);
            }
        }
    }
}
