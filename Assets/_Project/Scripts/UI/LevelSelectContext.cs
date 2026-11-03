namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Lưu trữ context và trạng thái khi chuyển giữa màn Chọn Hồi, Chọn Màn và Gameplay.
    /// </summary>
    public static class LevelSelectContext
    {
        public static int SelectedActIndex = 2; // Mặc định Hồi 2 (1: Hồi 1, 2: Hồi 2, 3: Hồi 3)
        public static int SelectedLevelId = 8;  // Màn được chọn (VD: màn 8)
        public static int SelectedLanId = 1;    // ID Lân đang chọn (1: Kim Lân, 2: Hỏa Lân...)

        // Giai đoạn 4: Điều hướng từ Gameplay hoặc bên ngoài
        public static int OpenActOnStart = -1;  // Nếu >= 0 thì vào thẳng LevelDetailView của Hồi đó, rồi reset về -1
        public static int LastPlayedLevelId = -1; // ID màn vừa chơi gần nhất

        public static void ResetToDefault()
        {
            SelectedActIndex = 2;
            SelectedLevelId = 8;
            SelectedLanId = 1;
            OpenActOnStart = -1;
            LastPlayedLevelId = -1;
        }
    }
}
