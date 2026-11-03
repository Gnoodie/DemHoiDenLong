#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.VFX;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Panel F9 hỗ trợ kiểm thử trực tiếp trong Gameplay (Chỉ chạy trong Editor và Development Build).
    /// Tích hợp tab VFX LAB (Giai đoạn 5A.5) để kiểm thử chuyển động, nhịp, hiệu năng hạt và phản hồi của 4 Lân.
    /// </summary>
    public class GameplayDebugPanel : MonoBehaviour
    {
        private bool showPanel = false;
        private Rect windowRect = new Rect(20, 60, 420, 580);
        private int selectedTab = 0;
        private readonly string[] tabHeaders = new string[] { "Gameplay Debug", "VFX LAB (5A.7)" };
        private Vector2 scrollPos = Vector2.zero;

        private float instantFPS = 60f;
        private float fpsTimer = 0f;
        private int frameCounter = 0;
        private string measureLanSummary = "";

        private void Update()
        {
            if (IsF9Pressed())
            {
                showPanel = !showPanel;
            }

            frameCounter++;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 0.25f)
            {
                instantFPS = frameCounter / fpsTimer;
                frameCounter = 0;
                fpsTimer = 0f;
            }
        }

        private bool IsF9Pressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.f9Key.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                return true;
            }
#endif
            return false;
        }

        private void OnGUI()
        {
            if (!showPanel) return;

            GUI.skin.window.fontSize = 13;
            windowRect = GUI.Window(8888, windowRect, DrawWindow, "[F9] GAMEPLAY & VFX LAB PANEL");
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, 420, 25));

            GUILayout.Space(5);
            selectedTab = GUILayout.Toolbar(selectedTab, tabHeaders, GUILayout.Height(28));
            GUILayout.Space(5);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            if (selectedTab == 0)
            {
                DrawGameplayTab();
            }
            else
            {
                DrawVfxLabTab();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5);
            if (GUILayout.Button("Đóng panel (F9)", GUILayout.Height(24)))
            {
                showPanel = false;
            }
        }

        private void DrawGameplayTab()
        {
            LevelData lvl = LevelSession.level;
            LanData lan = LevelSession.lan;
            LanStats stats = LevelSession.stats;
            PlayerController player = GameManager.Instance != null ? GameManager.Instance.Player : null;

            int levelId = lvl != null ? lvl.levelId : 1;
            string lvlName = lvl != null ? lvl.displayName : $"Màn {levelId}";
            string condStr = lvl != null ? lvl.winCondition.ToString() : "Unknown";

            GUILayout.Label($"<b>Màn:</b> {lvlName} (ID: {levelId}) | <b>Luật:</b> {condStr}");
            GUILayout.Label($"<b>Lân:</b> {lan?.lanName} (ID: {lan?.lanId})");

            if (stats != null)
            {
                GUILayout.Label($"<b>Chỉ số:</b> HP={stats.hp:F0} | DMG={stats.damage:F1} | FireRate={stats.fireRate:F1}");
            }

            if (player != null)
            {
                float hp01 = LevelRules.CalcHpRemaining01(player.CurrentHp, player.MaxHp, player.LivesCount, LevelRules.MaxHearts);
                GUILayout.Label($"<b>Máu:</b> {player.CurrentHp:F0}/{player.MaxHp:F0} | <b>Tim:</b> {player.LivesCount}/{LevelRules.MaxHearts} | <b>Tổng%:</b> {hp01:P1}");
            }

            GUILayout.Label($"<b>Đã hồi sinh:</b> {LevelSession.usedRevive} | <b>Sao nhặt:</b> {LevelSession.runStars}");

            int targetKills = lvl != null ? lvl.targetKillCount : 25;
            if (lvl != null && lvl.winCondition == WinCondition.KillCount)
            {
                GUILayout.Label($"<b>Tiến độ hạ:</b> {LevelSession.Kills} / {targetKills} (Nguồn: LevelSession.Kills)");
            }
            else if (lvl != null && lvl.winCondition == WinCondition.SurviveTime)
            {
                float lvlTimer = WaveManager.Instance != null ? WaveManager.Instance.LevelTimer : LevelSession.elapsedTime;
                GUILayout.Label($"<b>Thời gian:</b> {lvlTimer:F1}s / {lvl.surviveSeconds:F1}s");
            }
            else if (lvl != null && lvl.winCondition == WinCondition.BossKill)
            {
                BossController boss = WaveManager.Instance != null ? WaveManager.Instance.ActiveBoss : null;
                if (boss != null)
                {
                    GUILayout.Label($"<b>Máu Trùm:</b> {boss.CurrentHp:F0} / {boss.MaxHp:F0}");
                }
                else
                {
                    GUILayout.Label("<b>Trùm:</b> Chưa xuất hiện");
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- THAO TÁC NHANH ---</b>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Giả lập hạ 1 địch", GUILayout.Height(30)))
            {
                LevelSession.RegisterKill(EnemyKind.PaperLantern, KillCause.DebugF9);
            }
            if (GUILayout.Button("Đặt kills = target - 1", GUILayout.Height(30)))
            {
                int tgt = lvl != null ? lvl.targetKillCount : 25;
                LevelSession.SetKillsForDebug(Mathf.Max(0, tgt - 1));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Thắng ngay", GUILayout.Height(30)))
            {
                GameManager.Instance?.TriggerVictory();
            }
            if (GUILayout.Button("Thua ngay", GUILayout.Height(30)))
            {
                if (player != null)
                {
                    player.TakeDamage(99999f);
                }
                else
                {
                    GameManager.Instance?.ChangeState(GameState.GameOver);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Trừ 20% tổng máu", GUILayout.Height(30)))
            {
                if (player != null)
                {
                    float dmg = 0.20f * (player.MaxHp * LevelRules.MaxHearts);
                    player.TakeDamage(dmg);
                }
            }
            if (GUILayout.Button($"Hồi sinh: {(LevelSession.usedRevive ? "BẬT" : "TẮT")}", GUILayout.Height(30)))
            {
                LevelSession.usedRevive = !LevelSession.usedRevive;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawVfxLabTab()
        {
            PlayerController player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            PlayerSkillController skillCtrl = player != null ? player.GetComponent<PlayerSkillController>() : null;

            GUILayout.Label("<b>--- 1. CHỌN LÂN KIỂM THỬ ---</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kim (Lướt)", GUILayout.Height(28)))
            {
                skillCtrl?.SwitchLanForDebug("1");
            }
            if (GUILayout.Button("Hỏa (Lửa)", GUILayout.Height(28)))
            {
                skillCtrl?.SwitchLanForDebug("2");
            }
            if (GUILayout.Button("Thủy (Sóng)", GUILayout.Height(28)))
            {
                skillCtrl?.SwitchLanForDebug("3");
            }
            if (GUILayout.Button("Vàng (Pháo)", GUILayout.Height(28)))
            {
                skillCtrl?.SwitchLanForDebug("4");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kích hoạt Skill ngay", GUILayout.Height(30)))
            {
                if (skillCtrl != null)
                {
                    skillCtrl.ResetCooldownForDebug();
                    skillCtrl.ActivateSkill();
                }
            }
            PlayerSkillController.AutoLoopSkill = GUILayout.Toggle(PlayerSkillController.AutoLoopSkill, "Phát lặp skill", GUILayout.Height(30));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Capture VFX (0.2s, 0.6s, 1.2s, 3.0s)", GUILayout.Height(30)))
            {
                if (skillCtrl != null)
                {
                    StartCoroutine(Routine_CaptureVFXSequence(skillCtrl));
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- 2. THAM SỐ HIỆU ỨNG (SCALE & INTENSITY) ---</b>");
            GUILayout.Label($"Cường độ (Alpha/Shake/Juice): {PlayerSkillController.VfxIntensity:F2}x");
            PlayerSkillController.VfxIntensity = GUILayout.HorizontalSlider(PlayerSkillController.VfxIntensity, 0.5f, 2.0f);

            GUILayout.Label($"Kích thước hiệu ứng (Scale): {PlayerSkillController.VfxScale:F2}x");
            PlayerSkillController.VfxScale = GUILayout.HorizontalSlider(PlayerSkillController.VfxScale, 0.5f, 1.5f);

            GUILayout.Space(10);
            GUILayout.Label("<b>--- 3. TOGGLE CÁC LỚP VISUAL ---</b>");
            GUILayout.BeginHorizontal();
            PlayerSkillController.EnableAuraLayer = GUILayout.Toggle(PlayerSkillController.EnableAuraLayer, "Aura / Ring");
            PlayerSkillController.EnableWaveLayer = GUILayout.Toggle(PlayerSkillController.EnableWaveLayer, "Waves / Streaks");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            PlayerSkillController.EnableParticleLayer = GUILayout.Toggle(PlayerSkillController.EnableParticleLayer, "Particles");
            PlayerSkillController.EnableJuiceLayer = GUILayout.Toggle(PlayerSkillController.EnableJuiceLayer, "Screen Juice");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Measure Lan", GUILayout.Height(30)))
            {
                Camera cam = Camera.main;
                float screenW = cam != null ? cam.orthographicSize * 2f * cam.aspect : 5.625f;
                float L = LanDimensions.GetLanWidth(cam);
                float percentW = (L / screenW) * 100f;
                measureLanSummary = $"Current: L={L:F3}u ({percentW:F1}% W)\n" +
                                    $"Mouth={player?.MouthPosition}, Center={player?.CenterPosition}\n" +
                                    "Lan 1: L=0.563u (10.0% W) Mouth=(0, 0.65, 0)\n" +
                                    "Lan 2: L=0.563u (10.0% W) Mouth=(0, 0.65, 0)\n" +
                                    "Lan 3: L=0.563u (10.0% W) Mouth=(0, 0.65, 0)\n" +
                                    "Lan 4: L=0.563u (10.0% W) Mouth=(0, 0.65, 0)";
                Debug.Log($"[MEASURE] {measureLanSummary}");
#if UNITY_EDITOR
                System.Type toolType = System.Type.GetType("DemHoiDenLong.EditorTools.MeasureLanTool, Assembly-CSharp-Editor");
                if (toolType != null)
                {
                    var m = toolType.GetMethod("MeasureAndSetupAllLans", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    m?.Invoke(null, null);
                }
#endif
            }
            PlayerSkillController.ShowSkillGizmos = GUILayout.Toggle(PlayerSkillController.ShowSkillGizmos, "Hien vung skill", GUILayout.Height(30));
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(measureLanSummary))
            {
                GUILayout.TextArea(measureLanSummary);
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- 4. THỜI GIAN & CHẤT LƯỢNG (TIMESCALE / QUALITY) ---</b>");
            GUILayout.BeginHorizontal();
            bool isPaused = Time.timeScale <= 0f;
            if (GUILayout.Button(isPaused ? "Resume (Tiếp tục)" : "Pause (Tạm dừng)", GUILayout.Height(28)))
            {
                Time.timeScale = isPaused ? 1.0f : 0.0f;
            }
            if (GUILayout.Button("Step 1 Frame", GUILayout.Height(28)))
            {
                StartCoroutine(Routine_StepOneFrame());
            }
            GUILayout.EndHorizontal();

            bool isHighQuality = JuiceFx.QualityLevel == 1;
            bool newQuality = GUILayout.Toggle(isHighQuality, $"Chất lượng VFX: {(isHighQuality ? "Cao (Đầy đủ hiệu ứng)" : "Thấp (Tối ưu)")}");
            if (newQuality != isHighQuality)
            {
                JuiceFx.QualityLevel = newQuality ? 1 : 0;
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>--- 5. ĐỒNG HỒ TELEMETRY & HIỆU NĂNG ---</b>");
            int liveParticles = CountActiveParticles();
            int visibleSprites = CountVisibleSprites();
            float avgFps = PlayerSkillController.LastSkillFPS;

            GUIStyle fpsStyle = new GUIStyle(GUI.skin.label);
            if (instantFPS < 55f)
            {
                fpsStyle.normal.textColor = Color.red;
                GUILayout.Label($"<b>CẢNH BÁO: FPS TỨC THỜI DƯỚI 55! ({instantFPS:F1} FPS)</b>", fpsStyle);
            }
            else
            {
                fpsStyle.normal.textColor = Color.green;
                GUILayout.Label($"FPS Tức thời: {instantFPS:F1}", fpsStyle);
            }

            GUILayout.Label($"FPS Trung bình khi thi triển Skill: {avgFps:F1}");
            GUILayout.Label($"Hạt Particle đang hoạt động: {liveParticles}");
            GUILayout.Label($"Sprites đang hiển thị: {visibleSprites}");
        }

        private IEnumerator Routine_StepOneFrame()
        {
            Time.timeScale = 1.0f;
            yield return null;
            Time.timeScale = 0.0f;
        }

        private int CountActiveParticles()
        {
            ParticleSystem[] pss = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var ps in pss)
            {
                if (ps != null && ps.gameObject.activeInHierarchy)
                {
                    count += ps.particleCount;
                }
            }
            return count;
        }

        private int CountVisibleSprites()
        {
            SpriteRenderer[] srs = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var sr in srs)
            {
                if (sr != null && sr.gameObject.activeInHierarchy && sr.isVisible)
                {
                    count++;
                }
            }
            return count;
        }
    

private System.Collections.IEnumerator Routine_CaptureVFXSequence(PlayerSkillController skillCtrl)
        {
            if (skillCtrl == null) yield break;
            string lanId = skillCtrl.CurrentConfig != null ? skillCtrl.CurrentConfig.lanId : "1";
            string lanName = (lanId == "4" || lanId.Contains("vang")) ? "vang" : ((lanId == "2" || lanId.Contains("hoa")) ? "hoa" : ((lanId == "3" || lanId.Contains("thuy")) ? "thuy" : "kim"));

            skillCtrl.ResetCooldownForDebug();
            skillCtrl.ActivateSkill();

            float[] captureTimes = new float[] { 0.2f, 0.6f, 1.2f, 3.0f };
            float elapsed = 0f;

            string dir = System.IO.Path.Combine(Application.dataPath, "../Captures");
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            for (int i = 0; i < captureTimes.Length; i++)
            {
                float targetT = captureTimes[i];
                while (elapsed < targetT)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                yield return new WaitForEndOfFrame();
                Texture2D screenTex = ScreenCapture.CaptureScreenshotAsTexture();
                if (screenTex != null)
                {
                    string filePath = System.IO.Path.Combine(dir, $"vfx_5a6_{lanName}_{targetT:F1}s.png");
                    byte[] pngBytes = screenTex.EncodeToPNG();
                    System.IO.File.WriteAllBytes(filePath, pngBytes);

                    int maxDarkBlock = MeasureMaxDarkBlock(screenTex);
                    string status = maxDarkBlock < 40 ? "PASS" : "FAIL";
                    Debug.Log($"[VFXCAPTURE] Skill={lanName} Time={targetT:F1}s File={filePath} MaxDarkBlock={maxDarkBlock}x{maxDarkBlock} -> {status}");
                    Destroy(screenTex);
                }
            }
        }

        public static int MeasureMaxDarkBlock(Texture2D tex)
        {
            if (tex == null) return 0;
            Color32[] pixels = tex.GetPixels32();
            int w = tex.width;
            int h = tex.height;

            int maxSquare = 0;
            int step = 4;
            for (int y = 0; y < h - 40; y += step)
            {
                for (int x = 0; x < w - 40; x += step)
                {
                    int s = 0;
                    bool allDark = true;
                    while (allDark && (x + s < w) && (y + s < h))
                    {
                        for (int k = 0; k <= s; k++)
                        {
                            Color32 c1 = pixels[(y + s) * w + (x + k)];
                            Color32 c2 = pixels[(y + k) * w + (x + s)];
                            if (!(c1.r < 15 && c1.g < 15 && c1.b < 15 && c1.a > 240) ||
                                !(c2.r < 15 && c2.g < 15 && c2.b < 15 && c2.a > 240))
                            {
                                allDark = false;
                                break;
                            }
                        }
                        if (allDark) s++;
                    }
                    if (s > maxSquare) maxSquare = s;
                }
            }
            return maxSquare;
    }
    }
}
#endif
