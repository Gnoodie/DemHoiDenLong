using UnityEngine;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Lưu trữ kích thước hiển thị thật của 4 Lân (L) và cung cấp các hàm quy đổi
    /// kích thước phần trăm màn hình (% W, % H) sang World Units ở runtime theo Camera hiện tại.
    /// </summary>
    public static class LanDimensions
    {
        // Kích thước tham chiếu theo 9:16 (orthoSize = 5.0f -> H = 10.0f, W = 5.625f)
        public const float RefOrthoSize = 5.0f;
        public const float RefScreenHeight = 10.0f;
        public const float RefScreenWidth = 5.625f;

        // Bề rộng chuẩn của 4 Lân (10% W = 0.5625 world units)
        public const float DefaultLanWidthWorld = 0.5625f;
        public const float DefaultLanWidthPercent = 10.0f;

        // Bề rộng đo thật của từng Lân
        public static float LanKimWidth = 0.5625f;
        public static float LanHoaWidth = 0.5625f;
        public static float LanThuyWidth = 0.5625f;
        public static float LanVangWidth = 0.5625f;

        public static float GetLanWidth(Camera cam = null)
        {
            float w = GetScreenWidthWorld(cam);
            return (DefaultLanWidthPercent / 100f) * w;
        }

        public static float GetLanWidth(string lanId)
        {
            switch (lanId)
            {
                case "1": return LanKimWidth;
                case "2": return LanHoaWidth;
                case "3": return LanThuyWidth;
                case "4": return LanVangWidth;
                default: return DefaultLanWidthWorld;
            }
        }

        public static float GetLanWidth(int lanId)
        {
            return GetLanWidth(lanId.ToString());
        }

        public static float GetScreenHeightWorld(Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return RefScreenHeight;
            return 2f * cam.orthographicSize;
        }

        public static float GetScreenWidthWorld(Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return RefScreenWidth;
            return 2f * cam.orthographicSize * cam.aspect;
        }

        public static float ConvertPercentHToWorld(float percentH, Camera cam = null)
        {
            return (percentH / 100f) * GetScreenHeightWorld(cam);
        }

        public static float ConvertPercentWToWorld(float percentW, Camera cam = null)
        {
            return (percentW / 100f) * GetScreenWidthWorld(cam);
        }

        public static float ConvertRatioHToWorld(float ratioH, Camera cam = null)
        {
            return ratioH * GetScreenHeightWorld(cam);
        }

        public static float ConvertRatioWToWorld(float ratioW, Camera cam = null)
        {
            return ratioW * GetScreenWidthWorld(cam);
        }
    }
}
