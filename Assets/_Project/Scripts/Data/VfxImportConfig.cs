using System;
using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Data
{
    [Serializable]
    public class VfxFileEntry
    {
        public string sourceFile;
        public string outputFile;
        public bool isGrayscale;
        public bool isAlphaFromLuminance;
        public int maxDimension = 1024;
        public bool isRing;
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public string materialType = "VFX_Additive";
        public string description;
    }

    [CreateAssetMenu(fileName = "VfxImportConfig", menuName = "DemHoiDenLong/VFX Import Config")]
    public class VfxImportConfig : ScriptableObject
    {
        public List<VfxFileEntry> entries = new List<VfxFileEntry>();

        public void PopulateDefaults()
        {
            entries = new List<VfxFileEntry>
            {
                new VfxFileEntry { sourceFile = "vfx_ring.png", outputFile = "vfx_ring_gray.png", isGrayscale = true, isAlphaFromLuminance = false, maxDimension = 512, isRing = true, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Vòng năng lượng dồn lực & kết chiêu" },
                new VfxFileEntry { sourceFile = "vfx_spark.png", outputFile = "vfx_spark_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 512, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Tia sáng va chạm & tàn lửa" },
                new VfxFileEntry { sourceFile = "vfx_silver_streaks.png", outputFile = "vfx_silver_streaks_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 1536, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Vệt gió bạc 2 mép màn hình (Kim)" },
                new VfxFileEntry { sourceFile = "vfx_shield_aura.png", outputFile = "vfx_shield_aura_gray.png", isGrayscale = true, isAlphaFromLuminance = false, maxDimension = 1024, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Hào quang khiên bất tử (Kim)" },
                new VfxFileEntry { sourceFile = "vfx_fire_swirl.png", outputFile = "vfx_fire_swirl_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 1024, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Xoáy lửa dồn lực & bắt lửa (Hỏa)" },
                new VfxFileEntry { sourceFile = "vfx_fire_wave.png", outputFile = "vfx_fire_wave_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 1536, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_ScrollMask", description = "Dải sóng lửa quét ngang (Hỏa)" },
                new VfxFileEntry { sourceFile = "vfx_firework_burst.png", outputFile = "vfx_firework_burst_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 1024, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Vụ nổ pháo hoa tỏa tia (Vàng)" },
                new VfxFileEntry { sourceFile = "vfx_firework_star.png", outputFile = "vfx_firework_star_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 1024, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Vụ nổ pháo hoa ngôi sao (Vàng)" },
                new VfxFileEntry { sourceFile = "vfx_glitter.png", outputFile = "vfx_glitter_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 1024, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Bụi kim tuyến bay lên & rơi chậm (Vàng)" },
                new VfxFileEntry { sourceFile = "vfx_water_wave.png", outputFile = "vfx_water_wave_alpha.png", isGrayscale = false, isAlphaFromLuminance = true, maxDimension = 1536, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_ScrollMask", description = "Sóng nước ngọn lớn ở giữa (Thủy)" },
                new VfxFileEntry { sourceFile = "vfx_splash.png", outputFile = "vfx_splash_alpha.png", isGrayscale = false, isAlphaFromLuminance = true, maxDimension = 1024, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_AlphaBlend", description = "Bọt nước bắn tung tóe & tan (Thủy)" },
                new VfxFileEntry { sourceFile = "vfx_smoke.png", outputFile = "vfx_smoke_alpha.png", isGrayscale = false, isAlphaFromLuminance = true, maxDimension = 512, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_AlphaBlend", description = "Khói tàn dư (Tùy chọn)" },
                new VfxFileEntry { sourceFile = "vfx_rocket_trail.png", outputFile = "vfx_rocket_trail_proc.png", isGrayscale = false, isAlphaFromLuminance = false, maxDimension = 512, isRing = false, pivot = new Vector2(0.5f, 0.5f), materialType = "VFX_Additive", description = "Vệt khói đuôi pháo (Tùy chọn)" }
            };
        }
    }
}
