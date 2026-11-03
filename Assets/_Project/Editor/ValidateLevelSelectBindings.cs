using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.UI;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public static class ValidateLevelSelectBindings
    {
        private const string SCENE_PATH = "Assets/Scenes/LevelSelect.unity";

        [MenuItem("DemHoiDenLong/LevelSelect/Validate Bindings")]
        public static bool ValidateAll()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            bool needReopen = activeScene.path != SCENE_PATH;
            if (needReopen)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    Debug.LogWarning("[Validate] Người dùng đã hủy chuyển Scene.");
                    return false;
                }
                EditorSceneManager.OpenScene(SCENE_PATH);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== KIỂM TRA LIÊN KẾT SCENE LEVELSELECT ===");
            int errors = 0;

            // 1. LevelSelectFlow
            var flow = Object.FindAnyObjectByType<LevelSelectFlow>(FindObjectsInactive.Include);
            if (flow == null)
            {
                sb.AppendLine("[FAIL] Không tìm thấy LevelSelectFlow trong scene!");
                errors++;
            }
            else
            {
                sb.AppendLine("[PASS] Tìm thấy LevelSelectFlow.");
                if (flow.actSelectView == null) { sb.AppendLine("[FAIL] LevelSelectFlow.actSelectView là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelSelectFlow.actSelectView hợp lệ.");

                if (flow.levelDetailView == null) { sb.AppendLine("[FAIL] LevelSelectFlow.levelDetailView là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelSelectFlow.levelDetailView hợp lệ.");
            }

            // 2. ActSelectView & Cards
            var actSelectView = Object.FindAnyObjectByType<ActSelectView>(FindObjectsInactive.Include);
            if (actSelectView == null && flow != null && flow.actSelectView != null)
            {
                actSelectView = flow.actSelectView.GetComponent<ActSelectView>();
                if (actSelectView == null)
                {
                    actSelectView = flow.actSelectView.AddComponent<ActSelectView>();
                    EditorUtility.SetDirty(flow.actSelectView);
                }
            }

            if (actSelectView == null)
            {
                sb.AppendLine("[FAIL] Không tìm thấy ActSelectView!");
                errors++;
            }
            else
            {
                actSelectView.AutoBindReferences();
                sb.AppendLine("[PASS] ActSelectView component hợp lệ.");
                if (actSelectView.contentContainer == null) { sb.AppendLine("[FAIL] ActSelectView.contentContainer là null!"); errors++; }
                else sb.AppendLine($"[PASS] contentContainer có {actSelectView.contentContainer.childCount} thẻ con.");
            }

            // 3. LevelDetailView
            var detailView = Object.FindAnyObjectByType<LevelDetailView>(FindObjectsInactive.Include);
            if (detailView == null)
            {
                sb.AppendLine("[FAIL] Không tìm thấy LevelDetailView trong scene!");
                errors++;
            }
            else
            {
                sb.AppendLine("[PASS] Tìm thấy LevelDetailView.");
                if (detailView.detailModal == null) { sb.AppendLine("[FAIL] LevelDetailView.detailModal là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelDetailView.detailModal hợp lệ.");

                if (detailView.nodesContainer == null) { sb.AppendLine("[FAIL] LevelDetailView.nodesContainer là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelDetailView.nodesContainer hợp lệ.");

                if (detailView.pathDrawer == null) { sb.AppendLine("[FAIL] LevelDetailView.pathDrawer là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelDetailView.pathDrawer hợp lệ.");

                if (detailView.backButton == null) { sb.AppendLine("[FAIL] LevelDetailView.backButton là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelDetailView.backButton hợp lệ.");

                if (detailView.toastRoot == null) { sb.AppendLine("[FAIL] LevelDetailView.toastRoot là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelDetailView.toastRoot hợp lệ.");
            }

            // 4. LevelDetailModal
            var modal = Object.FindAnyObjectByType<LevelDetailModal>(FindObjectsInactive.Include);
            if (modal == null)
            {
                sb.AppendLine("[FAIL] Không tìm thấy LevelDetailModal trong scene!");
                errors++;
            }
            else
            {
                sb.AppendLine("[PASS] Tìm thấy LevelDetailModal.");
                if (modal.playButton == null) { sb.AppendLine("[FAIL] LevelDetailModal.playButton là null!"); errors++; }
                else sb.AppendLine("[PASS] LevelDetailModal.playButton hợp lệ.");

                if (modal.rewardRows == null || modal.rewardRows.Length < 3)
                {
                    sb.AppendLine("[FAIL] LevelDetailModal không đủ 3 RewardRowUI!");
                    errors++;
                }
                else
                {
                    sb.AppendLine($"[PASS] LevelDetailModal có {modal.rewardRows.Length} hàng quà.");
                }

                if (modal.starSprite == null) { sb.AppendLine("[FAIL] modal.starSprite là null!"); errors++; }
                if (modal.lockSprite == null) { sb.AppendLine("[FAIL] modal.lockSprite là null!"); errors++; }
                if (modal.checkSprite == null) { sb.AppendLine("[FAIL] modal.checkSprite là null!"); errors++; }
                if (modal.playSprite == null) { sb.AppendLine("[FAIL] modal.playSprite là null!"); errors++; }
            }

            // 5. Currency & TotalStars Pills
            var totalStarsPill = GameObject.Find("TotalStarsPill");
            if (totalStarsPill == null && flow != null && flow.actSelectView != null)
            {
                totalStarsPill = flow.actSelectView.transform.Find("SafeAreaRoot/HeaderBar/TotalStarsPill")?.gameObject;
            }
            if (totalStarsPill == null) { sb.AppendLine("[FAIL] Không tìm thấy TotalStarsPill!"); errors++; }
            else sb.AppendLine("[PASS] TotalStarsPill hợp lệ.");

            // 6. LanCatalog check
            var lans = LanCatalog.GetAllLans();
            if (lans == null || lans.Count == 0)
            {
                sb.AppendLine("[FAIL] LanCatalog không tìm thấy Lân nào!");
                errors++;
            }
            else
            {
                sb.AppendLine($"[PASS] LanCatalog tìm thấy {lans.Count} Lân hợp lệ (Selected: {LanCatalog.GetSelectedLan()?.lanName ?? "Null"}, Playable: {LanCatalog.GetPlayableLan()?.lanName ?? "Null"}).");
            }

            // 7. LevelDatabase check & battleBackground check
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");
            if (db == null || db.ActCount != 3 || db.TotalLevels != 15)
            {
                sb.AppendLine("[FAIL] LevelDatabase không đúng cấu hình 3 Hồi 15 màn!");
                errors++;
            }
            else
            {
                sb.AppendLine("[PASS] LevelDatabase có đủ 3 Hồi 15 màn.");

                for (int a = 0; a < db.ActCount; a++)
                {
                    var act = db.GetAct(a);
                    if (act != null)
                    {
                        if (act.battleBackground == null)
                        {
                            sb.AppendLine($"[WARN] Hồi {a + 1} ({act.title}): Chưa gán battleBackground (cần gán tay).");
                            Debug.LogWarning($"[ValidateLevelSelectBindings] Hồi {a + 1} ({act.title}): Chưa gán battleBackground (cần gán tay trong Inspector)!");
                        }
                        if (act.bossSprite == null)
                        {
                            sb.AppendLine($"[FAIL] Hồi {a + 1} ({act.title}): Chưa gán bossSprite!");
                            errors++;
                        }
                        else
                        {
                            sb.AppendLine($"[PASS] Hồi {a + 1} ({act.title}): bossSprite = {act.bossSprite.name}.");
                        }
                    }
                }

                var actA = db.GetAct(0);
                var actB = db.GetAct(1);
                var actC = db.GetAct(2);
                if (actA != null && actB != null && actC != null && actA.bossSprite != null && actB.bossSprite != null && actC.bossSprite != null)
                {
                    if (actA.bossSprite == actB.bossSprite || actB.bossSprite == actC.bossSprite || actA.bossSprite == actC.bossSprite)
                    {
                        sb.AppendLine("[FAIL] Các sprite boss giữa các Hồi bị trùng lặp!");
                        errors++;
                    }
                    else
                    {
                        sb.AppendLine("[PASS] Sprite boss của 3 Hồi hoàn toàn khác nhau và hợp lệ.");
                    }
                }
            }

            sb.AppendLine("-------------------------------------------------------------");
            if (errors == 0)
            {
                sb.AppendLine("[PASS] Validate Bindings: Tất cả liên kết trong Scene LevelSelect đều hợp lệ!");
                Debug.Log(sb.ToString());
                return true;
            }
            else
            {
                sb.AppendLine($"[FAIL] Validate Bindings: Có {errors} lỗi liên kết cần khắc phục!");
                Debug.LogError(sb.ToString());
                return false;
            }
        }
    }
}
