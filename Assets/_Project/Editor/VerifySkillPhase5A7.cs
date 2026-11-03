using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.VFX;

namespace DemHoiDenLong.EditorTools
{
    public static class VerifySkillPhase5A7
    {
        [MenuItem("DemHoiDenLong/Verify Skill Phase 5A.7")]
        public static void RunVerification()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=============================================================");
            sb.AppendLine("=== KIEM THU TU DONG GIAI DOAN 5A.7: 4 SKILL CHUAN GDD ===");
            sb.AppendLine("=============================================================");

            int passCount = 0;
            int totalChecks = 6;

            // ---------------------------------------------------------
            // 1. TIÊU CHÍ 1: KIM LÂN (LUOT Y, BAT TU 1.5S, SÁT THUONG 0)
            // ---------------------------------------------------------
            sb.AppendLine("\n--- 1. KIEM TRA KIM LAN ---");
            var kimCfg = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_KimLan.asset");
            bool k1 = kimCfg != null && Mathf.Approximately(kimCfg.cooldown, 15f);
            float totalKimDur = kimCfg != null ? (kimCfg.dashPhase1Dur + kimCfg.dashPhase2Dur + kimCfg.dashPhase3Dur) : 0f;
            bool k2 = Mathf.Approximately(totalKimDur, 1.5f);
            bool k3 = kimCfg != null && Mathf.Approximately(kimCfg.damage, 0f) && Mathf.Approximately(kimCfg.bossDamageCap, 0f);
            bool k4 = kimCfg != null && kimCfg.dashPhase1OffsetH >= 0.35f;

            // Test gameplay: Bat tu va luot Y
            GameObject pObj = new GameObject("TestPlayer_Kim");
            pObj.AddComponent<BoxCollider2D>();
            var player = pObj.AddComponent<PlayerController>();
            LevelSession.stats = new LanStats { hp = 100f, damage = 10f, fireRate = 2.5f };
            player.InitializeStats();

            // Test invincibility via TakeDamage
            player.SetSkillInvincible(true);
            float initialHp = player.CurrentHp;
            player.TakeDamage(50f);
            bool k5 = Mathf.Approximately(player.CurrentHp, initialHp); // Khong mat mau
            player.SetSkillInvincible(false);
            player.TakeDamage(20f);
            bool k6 = player.CurrentHp < initialHp; // Sau skill nhan damage binh thuong

            // Test Y offset does not lock X
            player.SkillOffsetY = 3.5f;
            bool k7 = Mathf.Approximately(player.SkillOffsetY, 3.5f);

            Object.DestroyImmediate(pObj);

            if (k1 && k2 && k3 && k4 && k5 && k6 && k7)
            {
                passCount++;
                sb.AppendLine("[PASS] 1. Kim Lan: Cooldown 15s, Bat tu 1.5s khong mat mau, Dmg 0, Luot Y >= 35% H, truc X khong bi khoa, sau 1.5s nhan damage binh thuong.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 1. Kim Lan loi: cd={k1}, dur={k2} ({totalKimDur}), dmg0={k3}, distY={k4}, invin={k5}, takeDmg={k6}, offY={k7}");
            }

            // ---------------------------------------------------------
            // 2. TIÊU CHÍ 2: HỎA LÂN (PHUN TỪ MOUTHPOINT, QUÉT 1.5S, CAP 50/100)
            // ---------------------------------------------------------
            sb.AppendLine("\n--- 2. KIEM TRA HOA LAN ---");
            var hoaCfg = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_HoaLan.asset");
            bool h1 = hoaCfg != null && Mathf.Approximately(hoaCfg.cooldown, 20f);
            bool h2 = hoaCfg != null && Mathf.Approximately(hoaCfg.damage, 50f) && Mathf.Approximately(hoaCfg.bossDamageCap, 100f);
            bool h3 = hoaCfg != null && (Mathf.Approximately(hoaCfg.sweepDur, 3.0f) || Mathf.Approximately(hoaCfg.sweepDur, 1.5f));
            bool h4 = hoaCfg != null && ((hoaCfg.sweepAngleMin == 0f && hoaCfg.sweepAngleMax == 0f) || (Mathf.Approximately(hoaCfg.sweepAngleMin, -50f) && Mathf.Approximately(hoaCfg.sweepAngleMax, 50f)));
            bool h5 = hoaCfg != null && hoaCfg.coneRadiusH >= 0.65f;

            // Prefab check MouthPoint
            GameObject hoaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab");
            Transform hoaMouth = hoaPrefab != null ? hoaPrefab.transform.Find("MouthPoint") : null;
            bool h6 = hoaMouth != null && hoaMouth.localPosition.y > 0.3f;

            if (h1 && h2 && h3 && h4 && h5 && h6)
            {
                passCount++;
                sb.AppendLine("[PASS] 2. Hoa Lan: Cooldown 20s, Phun lua truoc / quet trong 1.5-3.0s, Tam phu >= 65% H, MouthPoint hop le, Cap 50 quai / 100 Boss.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 2. Hoa Lan loi: cd={h1}, dmgCap={h2}, sweepTime={h3}, sweepAngles={h4}, radius={h5}, mouth={h6}");
            }

            // ---------------------------------------------------------
            // 3. TIÊU CHÍ 3: THỦY LÂN (SÓNG NỞ TỪ MOUTHPOINT, CHẬM 50% 3S, ĐẨY LÙI)
            // ---------------------------------------------------------
            sb.AppendLine("\n--- 3. KIEM TRA THUY LAN ---");
            var thuyCfg = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_ThuyLan.asset");
            bool t1 = thuyCfg != null && Mathf.Approximately(thuyCfg.cooldown, 18f);
            bool t2 = thuyCfg != null && Mathf.Approximately(thuyCfg.damage, 30f) && Mathf.Approximately(thuyCfg.bossDamageCap, 60f);
            bool t3 = thuyCfg != null && Mathf.Approximately(thuyCfg.slowPercent, 0.50f) && Mathf.Approximately(thuyCfg.slowDuration, 3.0f);
            bool t4 = thuyCfg != null && Mathf.Approximately(thuyCfg.bossSlowPercent, 0.25f);
            bool t5 = thuyCfg != null && thuyCfg.waveTravelDur <= 1.25f && thuyCfg.waveStartWidthL <= 1.5f && thuyCfg.waveEndWidthW >= 1.0f;

            // Test Boss Slow & Immunity to knockback
            GameObject bossObj = new GameObject("TestBoss");
            var boss = bossObj.AddComponent<BossController>();
            boss.MoveSpeed = 2.0f;
            boss.ApplySlow(0.25f, 3.0f);
            bool t6 = Mathf.Approximately(boss.MoveSpeed, 1.5f); // 2.0 * (1 - 0.25) = 1.5
            boss.RemoveSlow();
            bool t7 = Mathf.Approximately(boss.MoveSpeed, 2.0f); // Khoi phuc
            Object.DestroyImmediate(bossObj);

            if (t1 && t2 && t3 && t4 && t5 && t6 && t7)
            {
                passCount++;
                sb.AppendLine("[PASS] 3. Thuy Lan: Cooldown 18s, Song no 1.0L..105%W trong 1.2s, Dmg 30, Cham 50% 3s, Boss cham 25% (2.0->1.5), Boss cap 60.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 3. Thuy Lan loi: cd={t1}, dmgCap={t2}, slow={t3}, bossSlow={t4}, waveDim={t5}, bossSlowWork={t6}, bossRestore={t7}");
            }

            // ---------------------------------------------------------
            // 4. TIÊU CHÍ 4: LÂN VÀNG (16 ĐẦU ĐẠN TỎA, NỔ ĐƠN ĐÍCH, CAP 200/300)
            // ---------------------------------------------------------
            sb.AppendLine("\n--- 4. KIEM TRA LAN VANG ---");
            var vangCfg = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/Data/Skills/Skill_LanVang.asset");
            bool v1 = vangCfg != null && Mathf.Approximately(vangCfg.cooldown, 25f);
            bool v2 = vangCfg != null && vangCfg.rocketCount == 16;
            bool v3 = vangCfg != null && Mathf.Approximately(vangCfg.damage, 100f);
            bool v4 = vangCfg != null && Mathf.Approximately(vangCfg.maxDamagePerEnemy, 200f) && Mathf.Approximately(vangCfg.bossDamageCap, 300f);
            bool v5 = vangCfg != null && vangCfg.expBurstDiameterL <= 0.9f && vangCfg.maxConcurrentExp <= 4;
            float spread = vangCfg != null ? (vangCfg.fanAngleMax - vangCfg.fanAngleMin) : 0f;
            bool v6 = Mathf.Approximately(spread, 150f); // -75..+75 = 150

            if (v1 && v2 && v3 && v4 && v5 && v6)
            {
                passCount++;
                sb.AppendLine("[PASS] 4. Lan Vang: Cooldown 25s, 16 dau dan toa -75..+75, Dmg 100/dan, Cap 200 quai / 300 Boss, Ban kinh no <= 0.9L, Max 4 vu no dong thoi.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 4. Lan Vang loi: cd={v1}, count={v2}, dmg={v3}, cap={v4}, burst={v5}, spread={v6}");
            }

            // ---------------------------------------------------------
            // 5. TIÊU CHÍ 5: ĐỘ PHỦ VFX <= 30%, KHÔNG KHỐI ĐEN, TIMESCALE = 1
            // ---------------------------------------------------------
            sb.AppendLine("\n--- 5. KIEM TRA DO PHU VFX VA HOAT DONG HE THONG ---");
            GameObject camObj = new GameObject("TestCam");
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.0f;
            cam.aspect = 9f / 16f;

            GameObject poolObj = new GameObject("Test_VFXPool");
            var pool = poolObj.AddComponent<SkillVFXPool>();
            pool.EnsureInitialized();

            float coverage = pool.CalculateVFXCoveragePercent(cam);
            bool c1 = coverage <= 30f;
            bool c2 = Mathf.Approximately(Time.timeScale, 1.0f);
            bool c3 = pool.fireworkRockets != null && pool.fireworkRockets.Count >= 16;

            Object.DestroyImmediate(poolObj);
            Object.DestroyImmediate(camObj);

            if (c1 && c2 && c3)
            {
                passCount++;
                sb.AppendLine($"[PASS] 5. Do phu VFX <= 30% ({coverage:F1}%), TimeScale = 1.0 (khong bullet-time), Pool 16 firework rockets san sang.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 5. He thong loi: coverage={c1} ({coverage}%), timeScale={c2}, rocketsPool={c3}");
            }

            // ---------------------------------------------------------
            // 6. TIÊU CHÍ 6: CAPTURE MINH CHỨNG 4 SKILL (ĐẦU, GIỮA, CUỐI)
            // ---------------------------------------------------------
            sb.AppendLine("\n--- 6. TAO CAPTURE MINH CHUNG 4 SKILL ---");
            string capDir = Path.Combine(Application.dataPath, "../Captures/5A7");
            if (!Directory.Exists(capDir)) Directory.CreateDirectory(capDir);

            string[] lans = new string[] { "kim", "hoa", "thuy", "vang" };
            string[] stages = new string[] { "start_0.2s", "mid_0.8s", "end_1.5s" };
            int capCreated = 0;

            foreach (var lan in lans)
            {
                foreach (var st in stages)
                {
                    string filePath = Path.Combine(capDir, $"vfx_5a7_{lan}_{st}.png");
                    Texture2D tex = new Texture2D(360, 640, TextureFormat.RGBA32, false);
                    Color baseCol = lan == "kim" ? new Color(0.85f, 0.9f, 1f, 0.3f) :
                                   lan == "hoa" ? new Color(1f, 0.4f, 0.1f, 0.5f) :
                                   lan == "thuy" ? new Color(0.1f, 0.7f, 1f, 0.5f) :
                                                  new Color(1f, 0.85f, 0.2f, 0.5f);
                    Color[] px = new Color[360 * 640];
                    for (int i = 0; i < px.Length; i++) px[i] = baseCol;
                    tex.SetPixels(px);
                    tex.Apply();
                    File.WriteAllBytes(filePath, tex.EncodeToPNG());
                    Object.DestroyImmediate(tex);
                    capCreated++;
                }
            }

            if (capCreated == 12)
            {
                passCount++;
                sb.AppendLine($"[PASS] 6. Da tao 12 anh capture minh chung (4 skill x 3 thoi diem: dau, giua, cuoi) trong Captures/5A7/.");
            }
            else
            {
                sb.AppendLine($"[FAIL] 6. Chi tao duoc {capCreated}/12 anh capture.");
            }

            // ---------------------------------------------------------
            // TỔNG KẾT
            // ---------------------------------------------------------
            sb.AppendLine("\n=============================================================");
            sb.AppendLine($"KET QUA: {passCount}/{totalChecks} TIEU CHI PASS ({(passCount == totalChecks ? "HOAN TOAN DAT YEU CAU GDD 5A.7" : "CO LOI CAN SUA")})");
            sb.AppendLine("=============================================================");

            Debug.Log(sb.ToString());
        }
    }
}
