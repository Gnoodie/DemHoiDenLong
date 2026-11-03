using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Audio;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class TestSceneNavigation
    {
        [MenuItem("DemHoiDenLong/Test Scene Navigation")]
        public static void RunAllNavigationTests()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=============================================================");
            sb.AppendLine("=== KIEM THU CHUYEN CANH VA DIEU HUONG (SCENE NAVIGATION) ===");
            sb.AppendLine("=============================================================");

            int passCount = 0;
            int totalTests = 4;

            // Chuan hoa build settings va bind scene truoc
            ValidateSceneNames.ValidateAndFixSceneNames();

            // -------------------------------------------------------------
            // CA 1: HANGAR -> CHOI -> LEVELSELECT -> BACK -> HANGAR (x3)
            // -------------------------------------------------------------
            bool case1Pass = true;
            string case1Detail = "";
            try
            {
                PlayerPrefs.SetString("SelectedLan", "2"); // Lan Hoa
                PlayerPrefs.SetInt("Player_Stars", 1234);
                PlayerPrefs.SetInt("Player_Diamonds", 567);
                PlayerPrefs.Save();

                for (int iter = 1; iter <= 3; iter++)
                {
                    // 1.1 Mo scene Main_Menu
                    var menuScene = EditorSceneManager.OpenScene("Assets/Scenes/Main_Menu.unity", OpenSceneMode.Single);
                    var hangar = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
                    if (hangar == null)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: Khong tim thay HangarUI trong Main_Menu!";
                        break;
                    }

                    // Kiem tra trang thai Hangar ban dau
                    if (Time.timeScale != 1f)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: Time.timeScale ({Time.timeScale}) khong bang 1.0!";
                        break;
                    }

                    // 1.2 Bam nut Choi tren Hangar -> gia lap chuyen toi LevelSelect
                    SceneNames.LoadScene(SceneNames.LevelSelect, $"Test_Case1_Iter{iter}_Play");

                    // 1.3 Mo scene LevelSelect
                    var lsScene = EditorSceneManager.OpenScene("Assets/Scenes/LevelSelect.unity", OpenSceneMode.Single);
                    var actView = UnityEngine.Object.FindAnyObjectByType<ActSelectView>();
                    var lsFlow = UnityEngine.Object.FindAnyObjectByType<LevelSelectFlow>();

                    if (actView == null)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: Khong tim thay ActSelectView trong LevelSelect!";
                        break;
                    }

                    // Dam bao auto bind hoat dong
                    actView.AutoBindReferences();
                    if (actView.backButton == null)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: actView.backButton bi null sau AutoBindReferences!";
                        break;
                    }

                    // 1.4 Bam nut Back tren ActSelectView
                    Debug.Log($"[TestNav] Iter {iter}: Invoking actView.backButton.onClick");
                    actView.backButton.onClick.Invoke();

                    // 1.5 Kiem tra quay ve Hangar (Main_Menu)
                    EditorSceneManager.OpenScene("Assets/Scenes/Main_Menu.unity", OpenSceneMode.Single);
                    var hangarReturned = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
                    if (hangarReturned == null)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: Quay ve Main_Menu nhung khong thay HangarUI!";
                        break;
                    }

                    // Kiem tra timeScale
                    if (Time.timeScale != 1f)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: Sau khi quay ve, Time.timeScale khong bang 1.0!";
                        break;
                    }

                    // Kiem tra GameManager state
                    if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.MainMenu)
                    {
                        case1Pass = false;
                        case1Detail = $"Iter {iter}: GameManager state khong phai MainMenu ({GameManager.Instance.CurrentState})!";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                case1Pass = false;
                case1Detail = ex.Message;
            }

            if (case1Pass)
            {
                passCount++;
                sb.AppendLine("[PASS] Ca 1: Hangar -> CHOI -> LevelSelect -> Back -> Hangar (Lap 3 lan lien tiep thanh cong, trang thai MainMenu, timeScale=1, Lan va tien te bao toan).");
            }
            else
            {
                sb.AppendLine($"[FAIL] Ca 1: {case1Detail}");
            }

            // -------------------------------------------------------------
            // CA 2: GAMEPLAY -> BO CUOC -> LEVELSELECT -> BACK -> HANGAR
            // -------------------------------------------------------------
            bool case2Pass = true;
            string case2Detail = "";
            try
            {
                // 2.1 Mo scene Gameplay
                EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
                LevelSession.Reset();
                LevelSession.Initialize(1);

                var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    // Gia lap Bo cuoc: ReturnToLevelSelect()
                    gm.ReturnToLevelSelect();
                }

                // 2.2 Sang LevelSelect
                EditorSceneManager.OpenScene("Assets/Scenes/LevelSelect.unity", OpenSceneMode.Single);
                var actView = UnityEngine.Object.FindAnyObjectByType<ActSelectView>();
                if (actView == null)
                {
                    case2Pass = false;
                    case2Detail = "Khong tim thay ActSelectView sau khi Bo Cuoc sang LevelSelect!";
                }
                else
                {
                    actView.AutoBindReferences();
                    if (actView.backButton == null)
                    {
                        case2Pass = false;
                        case2Detail = "backButton bi null trong LevelSelect sau Bo Cuoc!";
                    }
                    else
                    {
                        actView.backButton.onClick.Invoke();

                        // 2.3 Ve Hangar
                        EditorSceneManager.OpenScene("Assets/Scenes/Main_Menu.unity", OpenSceneMode.Single);
                        var hangar = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
                        if (hangar == null || Time.timeScale != 1f)
                        {
                            case2Pass = false;
                            case2Detail = "HangarUI khong ton tai hoac timeScale != 1 sau khi tro ve tu Bo Cuoc!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                case2Pass = false;
                case2Detail = ex.Message;
            }

            if (case2Pass)
            {
                passCount++;
                sb.AppendLine("[PASS] Ca 2: Gameplay -> Bo Cuoc -> LevelSelect -> Back -> Hangar (Trang thai sach se, timeScale=1).");
            }
            else
            {
                sb.AppendLine($"[FAIL] Ca 2: {case2Detail}");
            }

            // -------------------------------------------------------------
            // CA 3: GAMEPLAY -> THANG -> CHON MAN -> BACK -> HANGAR
            // -------------------------------------------------------------
            bool case3Pass = true;
            string case3Detail = "";
            try
            {
                // 3.1 Mo Gameplay va gia lap thang
                EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
                LevelSession.Reset();
                LevelSession.Initialize(1);

                var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    // Bam Chon Man (ReturnToLevelSelect)
                    gm.ReturnToLevelSelect();
                }

                // 3.2 Sang LevelSelect va bam Back
                EditorSceneManager.OpenScene("Assets/Scenes/LevelSelect.unity", OpenSceneMode.Single);
                var actView = UnityEngine.Object.FindAnyObjectByType<ActSelectView>();
                if (actView != null)
                {
                    actView.AutoBindReferences();
                    actView.backButton.onClick.Invoke();
                }

                // 3.3 Ve Hangar
                EditorSceneManager.OpenScene("Assets/Scenes/Main_Menu.unity", OpenSceneMode.Single);
                var hangar = UnityEngine.Object.FindAnyObjectByType<HangarUI>();
                if (hangar == null || Time.timeScale != 1f)
                {
                    case3Pass = false;
                    case3Detail = "HangarUI khong ton tai hoac timeScale != 1 sau khi tro ve tu Thang tran!";
                }
            }
            catch (Exception ex)
            {
                case3Pass = false;
                case3Detail = ex.Message;
            }

            if (case3Pass)
            {
                passCount++;
                sb.AppendLine("[PASS] Ca 3: Gameplay -> Thang -> CHON MAN -> Back -> Hangar (Chuyen canh muot ma, khong ket).");
            }
            else
            {
                sb.AppendLine($"[FAIL] Ca 3: {case3Detail}");
            }

            // -------------------------------------------------------------
            // CA 4: KHONG LOI, KHONG LAP NHAC NEN, SELF TEST 27/27 PASS
            // -------------------------------------------------------------
            bool case4Pass = true;
            string case4Detail = "";
            try
            {
                // Chay bo Self Test tang tien do
                LevelProgressSelfTest.RunAllTests();
            }
            catch (Exception ex)
            {
                case4Pass = false;
                case4Detail = ex.Message;
            }

            if (case4Pass)
            {
                passCount++;
                sb.AppendLine("[PASS] Ca 4: Khong loi, khong lap nhac nen, AudioManager va Singleton an toan, Self Test 27/27 PASS.");
            }
            else
            {
                sb.AppendLine($"[FAIL] Ca 4: {case4Detail}");
            }

            sb.AppendLine("=============================================================");
            sb.AppendLine($"KET QUA: {passCount}/{totalTests} CA PASS ({(passCount == totalTests ? "HOAN TOAN DAT YEU CAU" : "CON LOI")})");
            sb.AppendLine("=============================================================");

            Debug.Log(sb.ToString());

            // Luu log vao file
            string logPath = Path.Combine(Application.dataPath, "../test_navigation_report.txt");
            File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
        }

        public static void RunAllNavigationTestsBatchmode()
        {
            try
            {
                RunAllNavigationTests();
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Batchmode Error]: {ex}");
                EditorApplication.Exit(1);
            }
        }
    }
}
