using System;
using UnityEngine;

namespace DemHoiDenLong.UI
{
    public enum ButtonStyleType
    {
        GoldPrimary,    // Nút chính vàng (Chơi, Mở khóa, Tiếp tục, Màn tiếp)
        NavySecondary,  // Nút phụ xanh tím than (Truyền thuyết, Chơi lại, Chọn màn, Đóng)
        CoralExit       // Nút thoát / bỏ cuộc san hô đỏ (Thoát game, Về chọn màn, Bỏ cuộc)
    }

    [System.Serializable]
    public class ButtonStyleData
    {
        public ButtonStyleType styleType;
        public Sprite backgroundSprite;
        public Color textColor = Color.white;
        public Color iconColor = Color.white;
    }

    [CreateAssetMenu(fileName = "UiButtonStyleSet", menuName = "DemHoiDenLong/UI/Button Style Set", order = 10)]
    public class UiButtonStyleSet : ScriptableObject
    {
        // ==========================================
        // BẢNG MÀU TẬP TRUNG (COLORS DEFINED IN ONE PLACE)
        // ==========================================
        // GoldPrimary: Nâu đậm hoàng kim tương phản cao trên nền vàng
        public static readonly Color ColorGoldText = new Color(0x3A / 255f, 0x22 / 255f, 0x00 / 255f, 1f); // #3A2200
        public static readonly Color ColorGoldIcon = new Color(0x3A / 255f, 0x22 / 255f, 0x00 / 255f, 1f); // #3A2200 (icon nâu)

        // NavySecondary: Kem sáng trang nhã trên nền xanh navy đậm
        public static readonly Color ColorNavyText = new Color(0xF4 / 255f, 0xF1 / 255f, 0xE6 / 255f, 1f); // #F4F1E6
        public static readonly Color ColorNavyIcon = new Color(0xF4 / 255f, 0xF1 / 255f, 0xE6 / 255f, 1f); // #F4F1E6

        // CoralExit: Trắng tinh khiết tương phản trên nền san hô đỏ
        public static readonly Color ColorCoralText = Color.white; // #FFFFFF
        public static readonly Color ColorCoralIcon = Color.white; // #FFFFFF

        // Trạng thái không bấm được (Disabled): Màu nhân #8E98BC, alpha 0.6
        public static readonly Color ColorDisabledTint = new Color(0x8E / 255f, 0x98 / 255f, 0xBC / 255f, 0.6f); // #8E98BC @ 0.6 alpha

        [Header("Button Style Presets")]
        public ButtonStyleData goldPrimary = new ButtonStyleData
        {
            styleType = ButtonStyleType.GoldPrimary,
            textColor = ColorGoldText,
            iconColor = ColorGoldIcon
        };

        public ButtonStyleData navySecondary = new ButtonStyleData
        {
            styleType = ButtonStyleType.NavySecondary,
            textColor = ColorNavyText,
            iconColor = ColorNavyIcon
        };

        public ButtonStyleData coralExit = new ButtonStyleData
        {
            styleType = ButtonStyleType.CoralExit,
            textColor = ColorCoralText,
            iconColor = ColorCoralIcon
        };

        public ButtonStyleData GetStyle(ButtonStyleType type)
        {
            switch (type)
            {
                case ButtonStyleType.GoldPrimary:
                    return goldPrimary;
                case ButtonStyleType.NavySecondary:
                    return navySecondary;
                case ButtonStyleType.CoralExit:
                    return coralExit;
                default:
                    return goldPrimary;
            }
        }
    }
}
