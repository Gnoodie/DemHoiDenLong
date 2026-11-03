using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public class LevelProgressDebugWindow : EditorWindow
    {
        [MenuItem("DemHoiDenLong/Progress Debug")]
        public static void OpenWindow()
        {
            var win = GetWindow<LevelProgressDebugWindow>("Progress Debug");
            win.minSize = new Vector2(650, 750);
            win.Show();
        }

        private Vector2 scrollPos;
        private bool useInMemoryStorage = false;
        private InMemoryProgressStorage inMemoryStorage;
        private int selectedLevelId = 1;
        private float simulateHpPercent = 90f;
        private bool simulateUsedRevive = false;
        private string lastResultInfo = "Chưa có lượt mô phỏng nào.";

        private void OnEnable()
        {
            if (inMemoryStorage == null) inMemoryStorage = new InMemoryProgressStorage();
        }

        private LevelProgressManager GetActiveManager()
        {
            var mgr = LevelProgressManager.Instance;
            if (useInMemoryStorage)
            {
                mgr.Initialize(null, inMemoryStorage, null);
            }
            return mgr;
        }

        private void OnGUI()
        {
            var mgr = GetActiveManager();
            if (mgr == null)
            {
                EditorGUILayout.HelpBox("Không thể khởi tạo LevelProgressManager!", MessageType.Error);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 1. Storage Toggle
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("CẤU HÌNH BỘ NHỚ LƯU TIẾN ĐỘ", EditorStyles.boldLabel);
            bool prevMode = useInMemoryStorage;
            useInMemoryStorage = EditorGUILayout.ToggleLeft(
                useInMemoryStorage ? "Đang dùng: BỘ NHỚ TẠM (In-Memory, không ảnh hưởng PlayerPrefs thật)"
                                   : "Đang dùng: PLAYERPREFS THẬT (Dữ liệu chơi thật của game)",
                useInMemoryStorage);

            if (prevMode != useInMemoryStorage)
            {
                if (useInMemoryStorage) mgr.Initialize(null, inMemoryStorage, null);
                else mgr.Initialize(null, new PlayerPrefsProgressStorage(), null);
                lastResultInfo = "Đã chuyển đổi nguồn lưu trữ.";
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 2. Summary Acts & Total
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("TIẾN ĐỘ TỔNG QUAN CHIẾN DỊCH", EditorStyles.boldLabel);
            int totalStars = mgr.GetTotalStars();
            int maxTotalStars = mgr.GetMaxTotalStars();
            int currentLevel = mgr.GetCurrentLevelId();

            EditorGUILayout.LabelField($"★ Tổng số sao toàn game: {totalStars} / {maxTotalStars}   |   Màn MỚI (Current): {(currentLevel > 0 ? "MÀN " + currentLevel : "Đã xong hết")}");

            EditorGUILayout.Space(4);
            for (int a = 0; a < 3; a++)
            {
                var prog = mgr.GetActProgress(a);
                ActStatus status = mgr.GetActStatus(a);
                string title = mgr.Database != null ? mgr.Database.GetAct(a)?.title ?? $"HỒI {a + 1}" : $"HỒI {a + 1}";
                EditorGUILayout.LabelField($"• {title}: Màn {prog.clearedLevels}/{prog.totalLevels}  |  Sao {prog.earnedStars}/{prog.maxStars}  |  Trạng thái: {status}");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 3. Table of 15 Levels
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("DANH SÁCH 15 MÀN CHƠI", EditorStyles.boldLabel);

            // Header row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", EditorStyles.miniBoldLabel, GUILayout.Width(35));
            EditorGUILayout.LabelField("Tên màn", EditorStyles.miniBoldLabel, GUILayout.Width(130));
            EditorGUILayout.LabelField("Trạng thái", EditorStyles.miniBoldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Sao", EditorStyles.miniBoldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Mốc 1 (Sao)", EditorStyles.miniBoldLabel, GUILayout.Width(85));
            EditorGUILayout.LabelField("Mốc 2 (Sao)", EditorStyles.miniBoldLabel, GUILayout.Width(85));
            EditorGUILayout.LabelField("Mốc 3 (Sao+KC)", EditorStyles.miniBoldLabel, GUILayout.Width(95));
            EditorGUILayout.EndHorizontal();

            int total = mgr.TotalLevels > 0 ? mgr.TotalLevels : 15;
            for (int i = 1; i <= total; i++)
            {
                LevelData data = mgr.Database != null ? mgr.Database.GetLevel(i) : null;
                string name = data != null ? data.displayName : $"Màn {i}";
                if (data != null && data.isBoss) name += " (BOSS)";

                NodeState state = mgr.GetNodeState(i);
                int stars = mgr.GetStars(i);
                bool c1 = mgr.IsMilestoneClaimed(i, 1);
                bool c2 = mgr.IsMilestoneClaimed(i, 2);
                bool c3 = mgr.IsMilestoneClaimed(i, 3);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i}", GUILayout.Width(35));
                EditorGUILayout.LabelField(name, GUILayout.Width(130));

                // Color code state
                Color prevColor = GUI.color;
                if (state == NodeState.Locked) GUI.color = Color.gray;
                else if (state == NodeState.Current) GUI.color = Color.yellow;
                else GUI.color = Color.green;
                EditorGUILayout.LabelField($"{state}", GUILayout.Width(80));
                GUI.color = prevColor;

                EditorGUILayout.LabelField($"{stars}★", GUILayout.Width(50));
                EditorGUILayout.LabelField(c1 ? "✓ Đã nhận" : "— Chưa", GUILayout.Width(85));
                EditorGUILayout.LabelField(c2 ? "✓ Đã nhận" : "— Chưa", GUILayout.Width(85));
                EditorGUILayout.LabelField(c3 ? "✓ Đã nhận" : "— Chưa", GUILayout.Width(95));

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 4. Controls & Simulation
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("CÔNG CỤ MÔ PHỎNG KẾT QUẢ GAMEPLAY", EditorStyles.boldLabel);

            selectedLevelId = EditorGUILayout.IntSlider("Chọn màn (1..15)", selectedLevelId, 1, total);
            simulateHpPercent = EditorGUILayout.Slider("HP còn lại khi xong (%)", simulateHpPercent, 0f, 100f);
            simulateUsedRevive = EditorGUILayout.Toggle("Đã dùng hồi sinh", simulateUsedRevive);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Simulate Win (Thắng)", GUILayout.Height(30)))
            {
                var res = mgr.ReportLevelResultForDebug(selectedLevelId, won: true, simulateHpPercent / 100f, force: true, usedRevive: simulateUsedRevive);
                lastResultInfo = FormatResult(res);
                Repaint();
            }
            if (GUILayout.Button("Simulate Loss (Thua)", GUILayout.Height(30)))
            {
                var res = mgr.ReportLevelResultForDebug(selectedLevelId, won: false, 0f, force: true, usedRevive: simulateUsedRevive);
                lastResultInfo = FormatResult(res);
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset All Tiến độ"))
            {
                if (EditorUtility.DisplayDialog("Xác nhận Reset", "Bạn có chắc chắn muốn xóa toàn bộ tiến độ chơi?", "Đồng ý", "Hủy"))
                {
                    mgr.ResetAll();
                    lastResultInfo = "Đã Reset toàn bộ tiến độ.";
                }
            }
            if (GUILayout.Button("Unlock All (1★)"))
            {
                mgr.UnlockAllWithStars(1);
                lastResultInfo = "Đã mở khóa 15 màn với 1 sao.";
            }
            if (GUILayout.Button("Unlock All (3★)"))
            {
                mgr.UnlockAllWithStars(3);
                lastResultInfo = "Đã mở khóa 15 màn với 3 sao.";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+1000 Sao tiền tệ"))
            {
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddStar(1000);
                    lastResultInfo = "Đã cộng 1000 Sao tiền tệ qua EconomyManager.";
                }
                else
                {
                    lastResultInfo = "EconomyManager.Instance chưa có (chỉ chạy trong Play mode có EconomyManager).";
                }
            }
            if (GUILayout.Button("+100 Kim Cương"))
            {
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddDiamond(100);
                    lastResultInfo = "Đã cộng 100 Kim Cương qua EconomyManager.";
                }
                else
                {
                    lastResultInfo = "EconomyManager.Instance chưa có (chỉ chạy trong Play mode có EconomyManager).";
                }
            }
            if (GUILayout.Button("Trả bù quà còn nợ (ClaimPendingRewards)"))
            {
                var claimRes = mgr.ClaimPendingRewards();
                lastResultInfo = $"ClaimPendingRewards: Nhận bù {claimRes.totalStarsGranted} Sao, {claimRes.totalDiamondsGranted} Kim Cương, {claimRes.totalMilestonesClaimed} mốc (IsEmpty: {claimRes.IsEmpty}, Lỗi: {claimRes.rewardFailed}).";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 5. Result box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("KẾT QUẢ LƯỢT MÔ PHỎNG VỪA RỒI", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(lastResultInfo, GUILayout.MinHeight(80));
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private string FormatResult(LevelResult res)
        {
            return $"Màn: {res.levelId} | Thắng: {res.won} | Bị từ chối: {res.rejected} | Hồi sinh (Cap 1★): {res.cappedByRevive} | Sao đạt: {res.starsThisRun}★ (Cũ: {res.previousMaxStars}★ -> Mới: {res.newMaxStars}★ | Kỷ lục mới: {res.isNewRecord})\n" +
                   $"Thưởng Sao: +{res.starsGranted} | Thưởng Kim Cương: +{res.diamondsGranted}\n" +
                   $"Chi tiết mốc đã cấp: {res.GetGrantedSummary()}\n" +
                   $"Mốc mới nhận: [M1: {res.newlyClaimed[0]}, M2: {res.newlyClaimed[1]}, M3: {res.newlyClaimed[2]}]\n" +
                   $"Mở màn kế: {res.unlockedNextLevel} | Mở Hồi kế: {res.unlockedNextAct} | Lỗi thưởng: {res.rewardFailed}";
        }
    }
}
