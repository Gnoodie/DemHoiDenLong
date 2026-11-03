using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.VFX
{
    /// <summary>
    /// Quản lý Object Pool cho hiệu ứng của cả 4 Skill (Giai đoạn 5A.5).
    /// Sử dụng sprite vẽ tay từ Assets/_Project/Art/VFX/Generated/ với fallback sprite sinh bằng code.
    /// Khởi tạo sẵn 1 lần, tuyệt đối không Instantiate/Destroy khi chơi, tối đa ~120 hạt/skill.
    /// </summary>
    public class SkillVFXPool : MonoBehaviour
    {
        public static SkillVFXPool Instance { get; private set; }

        // Materials
        public Material additiveMaterial;
        public Material alphaBlendMaterial;
        public Material scrollMaskFireMaterial;
        public Material scrollMaskWaterMaterial;

        // Sprites Vẽ Tay (Generated)
        public Sprite sprRingGray;
        public Sprite sprSpark;
        public Sprite sprSilverStreaks;
        public Sprite sprShieldAuraGray;
        public Sprite sprFireSwirl;
        public Sprite sprFireWave;
        public Sprite sprFireworkBurst;
        public Sprite sprFireworkStar;
        public Sprite sprGlitter;
        public Sprite sprWaterWave;
        public Sprite sprSplash;
        public Sprite sprSmoke;
        public Sprite sprRocketTrail;

        // Sprites Fallback (Sinh bằng code)
        private Sprite fbSoftCircle;
        private Sprite fbSpark4;
        private Sprite fbStreak;
        private Sprite fbRing;
        private Sprite fbFlame;
        private Sprite fbWaveBand;
        private Sprite fbBurst;
        private Sprite fbDroplet;

        // -------------------------------------------------------------
        // Kim Lân VFX (Bóng mờ, khiên pulse, vạch gió 2 mép)
        // -------------------------------------------------------------
        public GameObject kimChargeRing;
        public SpriteRenderer kimChargeRingSR;
        public GameObject kimShieldRing;
        public SpriteRenderer kimShieldRingSR;
        public GameObject kimWindLeft;
        public SpriteRenderer kimWindLeftSR;
        public GameObject kimWindRight;
        public SpriteRenderer kimWindRightSR;
        public GameObject kimEndRing;
        public SpriteRenderer kimEndRingSR;
        public GameObject kimSilverStreak;
        public SpriteRenderer kimSilverStreakSR;
        public ParticleSystem kimSparkPS;
        private List<SpriteRenderer> ghostTrailPool = new List<SpriteRenderer>();

        // -------------------------------------------------------------
        // Hỏa Lân VFX (Xoáy miệng, dải lửa cuộn, tàn lửa bay)
        // -------------------------------------------------------------
        public GameObject hoaMouthSwirl;
        public SpriteRenderer hoaMouthSwirlSR;
        public GameObject hoaFlameBand;
        public SpriteRenderer hoaFlameBandSR;
        public GameObject hoaFlameCoreBand;
        public SpriteRenderer hoaFlameCoreBandSR;
        public ParticleSystem hoaFlameStreamPS;
        public ParticleSystem hoaSparksPS;
        private List<GameObject> enemyBurnPool = new List<GameObject>();

        // -------------------------------------------------------------
        // Thủy Lân VFX (Vòng sóng, ngọn sóng lớn dâng, bọt nước)
        // -------------------------------------------------------------
        public GameObject thuyChargeRing;
        public SpriteRenderer thuyChargeRingSR;
        public GameObject thuyWaveBand;
        public SpriteRenderer thuyWaveBandSR;
        public GameObject thuyWaveFront;
        public SpriteRenderer thuyWaveFrontSR;
        public ParticleSystem thuySplashPS;
        public GameObject thuyRippleRing;
        public SpriteRenderer thuyRippleRingSR;
        public ParticleSystem thuySplashDropPS;
        private List<GameObject> enemySlowBubblePool = new List<GameObject>();

        // -------------------------------------------------------------
        // Lân Vàng VFX (Kim tuyến, 12 pháo hoa tỏa quạt, nổ star/burst)
        // -------------------------------------------------------------
        public ParticleSystem vangChargeGlitter;
        public ParticleSystem vangFallingGlitter;

        [System.Serializable]
        public class FireworkRocket
        {
            public GameObject go;
            public SpriteRenderer sr;
            public TrailRenderer tr;
        }
        public List<FireworkRocket> fireworkRockets = new List<FireworkRocket>();

        [System.Serializable]
        public class FireworkExpItem
        {
            public GameObject go;
            public SpriteRenderer sr;
            public GameObject ring;
            public SpriteRenderer ringSR;
        }
        public List<FireworkExpItem> fireworkExplosions = new List<FireworkExpItem>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }
            Instance = this;
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (vangChargeGlitter != null) return;

            LoadMaterials();
            LoadVFXSprites();

            BuildKimLanVFX();
            BuildHoaLanVFX();
            BuildThuyLanVFX();
            BuildLanVangVFX();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // =============================================================
        // NẠP VẬT LIỆU & SPRITES (HỖ TRỢ FALLBACK NẾU THIẾU)
        // =============================================================
        private void LoadMaterials()
        {
            additiveMaterial = Resources.Load<Material>("VFX/Mat_VFX_Additive");
            alphaBlendMaterial = Resources.Load<Material>("VFX/Mat_VFX_AlphaBlend");
            scrollMaskFireMaterial = Resources.Load<Material>("VFX/Mat_VFX_ScrollMask_Fire");
            scrollMaskWaterMaterial = Resources.Load<Material>("VFX/Mat_VFX_ScrollMask_Water");

            // Fallback shader nếu chưa import material
            Shader urpParticle = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (urpParticle == null) urpParticle = Shader.Find("Particles/Standard Unlit");
            if (urpParticle == null) urpParticle = Shader.Find("Sprites/Default");

            if (additiveMaterial == null)
            {
                additiveMaterial = new Material(urpParticle);
                if (additiveMaterial.HasProperty("_Surface")) additiveMaterial.SetFloat("_Surface", 1);
                if (additiveMaterial.HasProperty("_Blend")) additiveMaterial.SetFloat("_Blend", 1);
                additiveMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                additiveMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                additiveMaterial.SetInt("_ZWrite", 0);
            }

            if (alphaBlendMaterial == null)
            {
                alphaBlendMaterial = new Material(urpParticle);
                if (alphaBlendMaterial.HasProperty("_Surface")) alphaBlendMaterial.SetFloat("_Surface", 1);
                if (alphaBlendMaterial.HasProperty("_Blend")) alphaBlendMaterial.SetFloat("_Blend", 0);
                alphaBlendMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                alphaBlendMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                alphaBlendMaterial.SetInt("_ZWrite", 0);
            }

            Shader scrollShader = Shader.Find("Custom/VFX_ScrollMask");
            if (scrollShader == null) scrollShader = urpParticle;

            if (scrollMaskFireMaterial == null)
            {
                scrollMaskFireMaterial = new Material(scrollShader);
                scrollMaskFireMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                scrollMaskFireMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                scrollMaskFireMaterial.SetVector("_ScrollSpeed", new Vector4(0f, 0.8f, 0f, 0f));
                scrollMaskFireMaterial.SetVector("_Feather", new Vector4(0.05f, 0.05f, 0.08f, 0.08f));
            }

            if (scrollMaskWaterMaterial == null)
            {
                scrollMaskWaterMaterial = new Material(scrollShader);
                scrollMaskWaterMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                scrollMaskWaterMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                scrollMaskWaterMaterial.SetVector("_ScrollSpeed", new Vector4(0f, 0.6f, 0f, 0f));
                scrollMaskWaterMaterial.SetVector("_Feather", new Vector4(0.04f, 0.04f, 0.04f, 0.04f));
            }
        }

        private void LoadVFXSprites()
        {
            // 1. Fallback sprites (sinh bằng code)
            fbSoftCircle = LoadSprite("VFX/vfx_soft_circle", "Assets/_Project/Art/VFX/vfx_soft_circle.png");
            fbSpark4     = LoadSprite("VFX/vfx_spark4", "Assets/_Project/Art/VFX/vfx_spark4.png");
            fbStreak     = LoadSprite("VFX/vfx_streak", "Assets/_Project/Art/VFX/vfx_streak.png");
            fbRing       = LoadSprite("VFX/vfx_ring", "Assets/_Project/Art/VFX/vfx_ring.png");
            fbFlame      = LoadSprite("VFX/vfx_flame", "Assets/_Project/Art/VFX/vfx_flame.png");
            fbWaveBand   = LoadSprite("VFX/vfx_wave_band", "Assets/_Project/Art/VFX/vfx_wave_band.png");
            fbBurst      = LoadSprite("VFX/vfx_burst", "Assets/_Project/Art/VFX/vfx_burst.png");
            fbDroplet    = LoadSprite("VFX/vfx_droplet", "Assets/_Project/Art/VFX/vfx_droplet.png");

            // 2. Sprite vẽ tay đã xử lý với alpha trong suốt (Giai đoạn 5A.5 & 5A.6)
            sprRingGray       = LoadHandDrawn("vfx_ring_alpha", LoadHandDrawn("vfx_ring_gray", fbRing));
            sprSpark          = LoadHandDrawn("vfx_spark_alpha", LoadHandDrawn("vfx_spark_proc", fbSpark4));
            sprSilverStreaks  = LoadHandDrawn("vfx_silver_streaks_alpha", LoadHandDrawn("vfx_silver_streaks_proc", fbStreak));
            sprShieldAuraGray = LoadHandDrawn("vfx_shield_aura_alpha", LoadHandDrawn("vfx_shield_aura_gray", fbRing));
            sprFireSwirl      = LoadHandDrawn("vfx_fire_swirl_alpha", LoadHandDrawn("vfx_fire_swirl_proc", fbFlame));
            sprFireWave       = LoadHandDrawn("vfx_fire_wave_alpha", LoadHandDrawn("vfx_fire_wave_proc", fbWaveBand));
            sprFireworkBurst  = LoadHandDrawn("vfx_firework_burst_alpha", LoadHandDrawn("vfx_firework_burst_proc", fbBurst));
            sprFireworkStar   = LoadHandDrawn("vfx_firework_star_alpha", LoadHandDrawn("vfx_firework_star_proc", fbBurst));
            sprGlitter        = LoadHandDrawn("vfx_glitter_alpha", LoadHandDrawn("vfx_glitter_proc", fbSoftCircle));
            sprWaterWave      = LoadHandDrawn("vfx_water_wave_alpha", fbWaveBand);
            sprSplash         = LoadHandDrawn("vfx_splash_alpha", fbDroplet);

            // Optional
            sprSmoke          = LoadHandDrawn("vfx_smoke_alpha", fbSoftCircle, isOptional: true);
            sprRocketTrail    = LoadHandDrawn("vfx_rocket_trail_proc", fbStreak, isOptional: true);
        }

        private Sprite LoadHandDrawn(string baseName, Sprite fallback, bool isOptional = false)
        {
            Sprite s = Resources.Load<Sprite>($"VFX/Generated/{baseName}");
#if UNITY_EDITOR
            if (s == null)
            {
                s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/VFX/Generated/{baseName}.png");
                if (s == null)
                {
                    s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Resources/VFX/Generated/{baseName}.png");
                }
            }
#endif
            if (s == null)
            {
                if (!isOptional)
                {
                    Debug.LogWarning($"[VFX] Không tìm thấy sprite vẽ tay '{baseName}'. Sử dụng fallback sprite hệ thống.");
                }
                return fallback;
            }
            return s;
        }

        private Sprite LoadSprite(string resPath, string editorPath)
        {
            Sprite s = Resources.Load<Sprite>(resPath);
#if UNITY_EDITOR
            if (s == null)
            {
                s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            }
#endif
            return s;
        }

        private void SetupSpriteRenderer(GameObject go, Sprite spr, int sortingOrder, Material mat, Color col)
        {
            var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = sortingOrder;
            sr.color = col;
            sr.material = mat != null ? mat : additiveMaterial;
        }

        private void SetupSpriteParticle(ParticleSystem ps, Sprite spr, int maxParticles, Material mat)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.None;
            main.maxParticles = maxParticles;

            var psr = ps.GetComponent<ParticleSystemRenderer>();
            psr.material = mat != null ? mat : additiveMaterial;
            psr.sortingOrder = 18;

            if (spr != null)
            {
                var tsa = ps.textureSheetAnimation;
                tsa.enabled = true;
                tsa.mode = ParticleSystemAnimationMode.Sprites;
                if (tsa.spriteCount == 0) tsa.AddSprite(spr);
                else tsa.SetSprite(0, spr);
            }
        }

        // =============================================================
        // 1. KIM LÂN VFX (Bóng mờ, khiên pulse, vạch gió 2 mép)
        // =============================================================
        private void BuildKimLanVFX()
        {
            // A. Vòng dồn lực Nhịp 1
            kimChargeRing = new GameObject("VFX_KimChargeRing");
            kimChargeRing.transform.SetParent(transform);
            kimChargeRingSR = kimChargeRing.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(kimChargeRing, sprRingGray, 19, additiveMaterial, new Color(0.85f, 0.89f, 0.94f, 0.8f));
            kimChargeRing.SetActive(false);

            // B. Khiên bạc bám Lân suốt 1.5s (vfx_shield_aura_gray)
            kimShieldRing = new GameObject("VFX_KimShieldRing");
            kimShieldRing.transform.SetParent(transform);
            kimShieldRingSR = kimShieldRing.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(kimShieldRing, sprShieldAuraGray, 19, additiveMaterial, new Color(0.85f, 0.89f, 0.94f, 0.85f));
            kimShieldRing.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            kimShieldRing.SetActive(false);

            // C. 2 Dải vệt gió bạc ở 2 mép màn hình (vfx_silver_streaks_proc)
            kimWindLeft = new GameObject("VFX_KimWind_Left");
            kimWindLeft.transform.SetParent(transform);
            kimWindLeft.transform.position = new Vector3(-2.6f, 0f, 0f);
            kimWindLeftSR = kimWindLeft.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(kimWindLeft, sprSilverStreaks, 17, additiveMaterial, new Color(0.85f, 0.89f, 0.94f, 0.65f));
            kimWindLeft.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
            kimWindLeft.SetActive(false);

            kimWindRight = new GameObject("VFX_KimWind_Right");
            kimWindRight.transform.SetParent(transform);
            kimWindRight.transform.position = new Vector3(2.6f, 0f, 0f);
            kimWindRightSR = kimWindRight.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(kimWindRight, sprSilverStreaks, 17, additiveMaterial, new Color(0.85f, 0.89f, 0.94f, 0.65f));
            kimWindRight.transform.localScale = new Vector3(-0.9f, 1.2f, 1f); // Lật ngang
            kimWindRight.SetActive(false);

            // C2. Vệt gió bạc phía SAU Lân (GDD 5A.7: rộng 0.6L, cao 18% H)
            kimSilverStreak = new GameObject("VFX_KimSilverStreak");
            kimSilverStreak.transform.SetParent(transform);
            kimSilverStreakSR = kimSilverStreak.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(kimSilverStreak, sprSilverStreaks, 17, additiveMaterial, new Color(0.85f, 0.89f, 0.94f, 0.65f));
            kimSilverStreak.SetActive(false);

            // D. Vòng tan Nhịp 4
            kimEndRing = new GameObject("VFX_KimEndRing");
            kimEndRing.transform.SetParent(transform);
            kimEndRingSR = kimEndRing.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(kimEndRing, sprRingGray, 19, additiveMaterial, new Color(0.85f, 0.89f, 0.94f, 0.8f));
            kimEndRing.SetActive(false);

            // E. Spark particle va chạm
            GameObject spkGO = new GameObject("VFX_KimSparkPS");
            spkGO.transform.SetParent(transform);
            kimSparkPS = spkGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(kimSparkPS, sprSpark, 20, additiveMaterial);
            var mainSpk = kimSparkPS.main;
            mainSpk.duration = 0.5f;
            mainSpk.startLifetime = 0.25f;
            mainSpk.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            mainSpk.startSize = 0.35f;
            mainSpk.simulationSpace = ParticleSystemSimulationSpace.World;
            var emSpk = kimSparkPS.emission;
            emSpk.rateOverTime = 0f;
            emSpk.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });
            spkGO.SetActive(false);

            // F. Ghost Trail Pool (8 bóng mờ)
            for (int i = 0; i < 8; i++)
            {
                GameObject gObj = new GameObject($"VFX_KimGhost_{i}");
                gObj.transform.SetParent(transform);
                var sr = gObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 9;
                sr.material = additiveMaterial;
                gObj.SetActive(false);
                ghostTrailPool.Add(sr);
            }
        }

        public SpriteRenderer GetGhostFromPool()
        {
            foreach (var g in ghostTrailPool)
            {
                if (!g.gameObject.activeInHierarchy) return g;
            }
            return ghostTrailPool[0];
        }

        // =============================================================
        // 2. HỎA LÂN VFX (Xoáy miệng, dải lửa cuộn, tàn lửa bay)
        // =============================================================
        private void BuildHoaLanVFX()
        {
            // A. Xoáy lửa nhỏ trước miệng Nhịp 1
            hoaMouthSwirl = new GameObject("VFX_HoaMouthSwirl");
            hoaMouthSwirl.transform.SetParent(transform);
            hoaMouthSwirlSR = hoaMouthSwirl.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(hoaMouthSwirl, sprFireSwirl, 20, additiveMaterial, Color.white);
            hoaMouthSwirl.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            hoaMouthSwirl.SetActive(false);

            // B. Dải sóng lửa chính quét toàn màn (vfx_fire_wave_proc)
            hoaFlameBand = new GameObject("VFX_HoaFlameBand");
            hoaFlameBand.transform.SetParent(transform);
            hoaFlameBandSR = hoaFlameBand.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(hoaFlameBand, sprFireWave, 18, scrollMaskFireMaterial, Color.white);
            // Camera width ~6 units -> scale x = 6.0f
            hoaFlameBand.transform.localScale = new Vector3(6.0f, 1.8f, 1f);
            hoaFlameBand.SetActive(false);

            // C. Lớp lõi lửa sáng (thân nhỏ hơn, lệch pha)
            hoaFlameCoreBand = new GameObject("VFX_HoaFlameCoreBand");
            hoaFlameCoreBand.transform.SetParent(transform);
            hoaFlameCoreBandSR = hoaFlameCoreBand.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(hoaFlameCoreBand, sprFireWave, 19, scrollMaskFireMaterial, new Color(1f, 0.95f, 0.7f, 0.85f));
            hoaFlameCoreBand.transform.localScale = new Vector3(5.2f, 1.4f, 1f);
            hoaFlameCoreBand.SetActive(false);

            // C2. Tia lửa phun về phía trước (Ngọn lửa lớn, kéo dài 3 giây, 140 hạt/s, World simulation)
            GameObject streamGO = new GameObject("VFX_HoaFlameStreamPS");
            streamGO.transform.SetParent(transform);
            hoaFlameStreamPS = streamGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(hoaFlameStreamPS, sprFireSwirl, 200, additiveMaterial);
            var mainStream = hoaFlameStreamPS.main;
            mainStream.duration = 3.0f;
            mainStream.startLifetime = 0.40f;
            mainStream.startSpeed = 22f; // Phóng nhanh tạo cảm giác phun lửa uy lực
            mainStream.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.4f); // Ngọn lửa lớn hơn
            mainStream.simulationSpace = ParticleSystemSimulationSpace.World;
            var emStream = hoaFlameStreamPS.emission;
            emStream.rateOverTime = 140f;
            emStream.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

            // Shape = Cone góc 18 độ (nở rộng hơn), quay -90 độ trục X để bắn lên +Y trong 2D
            var shapeStream = hoaFlameStreamPS.shape;
            shapeStream.enabled = true;
            shapeStream.shapeType = ParticleSystemShapeType.Cone;
            shapeStream.angle = 18f;
            shapeStream.radius = 0.12f;
            shapeStream.rotation = new Vector3(-90f, 0f, 0f);

            var szStream = hoaFlameStreamPS.sizeOverLifetime;
            szStream.enabled = true;
            AnimationCurve szCurve = new AnimationCurve();
            szCurve.AddKey(0f, 0.6f); // Nhỏ ở miệng Lân
            szCurve.AddKey(0.4f, 1.4f);
            szCurve.AddKey(1f, 3.0f); // Nở to thành nón lửa cuồn cuộn
            szStream.size = new ParticleSystem.MinMaxCurve(1f, szCurve);

            var colStream = hoaFlameStreamPS.colorOverLifetime;
            colStream.enabled = true;
            Gradient gradStream = new Gradient();
            gradStream.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 1f, 0.9f), 0f), new GradientColorKey(new Color(1f, 0.55f, 0.12f), 0.45f), new GradientColorKey(new Color(0.85f, 0.15f, 0.05f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.75f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            colStream.color = gradStream;
            streamGO.SetActive(false);

            // D. Tàn lửa phóng nhanh theo tia lửa (vfx_spark)
            GameObject spkGO = new GameObject("VFX_HoaSparksPS");
            spkGO.transform.SetParent(transform);
            hoaSparksPS = spkGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(hoaSparksPS, sprSpark, 80, additiveMaterial);
            var mainSpk = hoaSparksPS.main;
            mainSpk.duration = 3.0f;
            mainSpk.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.42f);
            mainSpk.startSpeed = new ParticleSystem.MinMaxCurve(20f, 28f); // Tàn lửa xé gió
            mainSpk.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.65f); // Tàn lửa lớn hơn
            mainSpk.simulationSpace = ParticleSystemSimulationSpace.World;
            var emSpk = hoaSparksPS.emission;
            emSpk.rateOverTime = 60f;
            emSpk.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
            var shapeSpk = hoaSparksPS.shape;
            shapeSpk.enabled = true;
            shapeSpk.shapeType = ParticleSystemShapeType.Cone;
            shapeSpk.angle = 16f;
            shapeSpk.radius = 0.10f;
            shapeSpk.rotation = new Vector3(-90f, 0f, 0f);
            var colSpk = hoaSparksPS.colorOverLifetime;
            colSpk.enabled = true;
            Gradient gradSpk = new Gradient();
            gradSpk.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.5f, 0.1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colSpk.color = gradSpk;
            spkGO.SetActive(false);

            // E. Enemy burn pool (6 vfx_fire_swirl nhỏ bám địch)
            for (int i = 0; i < 6; i++)
            {
                GameObject bObj = new GameObject($"VFX_HoaBurnItem_{i}");
                bObj.transform.SetParent(transform);
                var sr = bObj.AddComponent<SpriteRenderer>();
                SetupSpriteRenderer(bObj, sprFireSwirl, 19, additiveMaterial, Color.white);
                bObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                bObj.SetActive(false);
                enemyBurnPool.Add(bObj);
            }
        }

        public GameObject GetBurnFromPool()
        {
            foreach (var b in enemyBurnPool)
            {
                if (!b.activeInHierarchy) return b;
            }
            return enemyBurnPool[0];
        }

        // =============================================================
        // 3. THỦY LÂN VFX (Vòng sóng, ngọn sóng lớn dâng, bọt nước)
        // =============================================================
        private void BuildThuyLanVFX()
        {
            // A. Vòng sóng dồn lực Nhịp 1 (vfx_ring_gray tint #7FE8F5)
            thuyChargeRing = new GameObject("VFX_ThuyChargeRing");
            thuyChargeRing.transform.SetParent(transform);
            thuyChargeRingSR = thuyChargeRing.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(thuyChargeRing, sprRingGray, 19, additiveMaterial, new Color(0.50f, 0.91f, 0.96f, 0.85f));
            thuyChargeRing.SetActive(false);

            // B. Ngọn sóng lớn ở giữa dâng từ đáy lên đỉnh (vfx_water_wave_alpha)
            thuyWaveBand = new GameObject("VFX_ThuyWaveBand");
            thuyWaveBand.transform.SetParent(transform);
            thuyWaveBandSR = thuyWaveBand.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(thuyWaveBand, sprWaterWave, 18, scrollMaskWaterMaterial, Color.white);
            thuyWaveBand.transform.localScale = new Vector3(6.0f, 2.2f, 1f);
            thuyWaveBand.SetActive(false);

            // B2. Mũi sóng nước 5A.7 lướt từ MouthPoint (vfx_water_wave_alpha, tỉ lệ Y = 0.55)
            thuyWaveFront = new GameObject("VFX_ThuyWaveFront");
            thuyWaveFront.transform.SetParent(transform);
            thuyWaveFrontSR = thuyWaveFront.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(thuyWaveFront, sprWaterWave, 18, additiveMaterial != null ? additiveMaterial : alphaBlendMaterial, new Color(0.85f, 0.96f, 1f, 0.9f));
            thuyWaveFront.SetActive(false);

            // C. Bọt nước bắn trước mũi sóng (vfx_splash_alpha)
            GameObject splGO = new GameObject("VFX_ThuySplashPS");
            splGO.transform.SetParent(transform);
            thuySplashPS = splGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(thuySplashPS, sprSplash, 40, alphaBlendMaterial);
            var mainSpl = thuySplashPS.main;
            mainSpl.duration = 1.0f;
            mainSpl.startLifetime = 0.4f;
            mainSpl.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            mainSpl.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            mainSpl.simulationSpace = ParticleSystemSimulationSpace.World;
            var emSpl = thuySplashPS.emission;
            emSpl.rateOverTime = 30f;
            var shapeSpl = thuySplashPS.shape;
            shapeSpl.shapeType = ParticleSystemShapeType.Rectangle;
            shapeSpl.scale = new Vector3(5.5f, 0.4f, 1f);
            splGO.SetActive(false);

            // D. Ripple ring
            thuyRippleRing = new GameObject("VFX_ThuyRippleRing");
            thuyRippleRing.transform.SetParent(transform);
            thuyRippleRingSR = thuyRippleRing.AddComponent<SpriteRenderer>();
            SetupSpriteRenderer(thuyRippleRing, sprRingGray, 17, additiveMaterial, new Color(0.50f, 0.91f, 0.96f, 0.5f));
            thuyRippleRing.SetActive(false);

            // E. Tàn dư bọt nước rơi xuống Nhịp 4
            GameObject dropGO = new GameObject("VFX_ThuySplashDropPS");
            dropGO.transform.SetParent(transform);
            thuySplashDropPS = dropGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(thuySplashDropPS, sprSplash, 30, alphaBlendMaterial);
            var mainDrop = thuySplashDropPS.main;
            mainDrop.duration = 0.5f;
            mainDrop.startLifetime = 0.4f;
            mainDrop.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            mainDrop.gravityModifier = 2.0f; // Rơi xuống
            mainDrop.startSize = 0.4f;
            mainDrop.simulationSpace = ParticleSystemSimulationSpace.World;
            var emDrop = thuySplashDropPS.emission;
            emDrop.rateOverTime = 0f;
            emDrop.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });
            dropGO.SetActive(false);

            // F. Enemy slow bubbles pool (6 vfx_splash nhỏ bám quanh địch 3s)
            for (int i = 0; i < 6; i++)
            {
                GameObject bub = new GameObject($"VFX_ThuySlowBubble_{i}");
                bub.transform.SetParent(transform);
                var sr = bub.AddComponent<SpriteRenderer>();
                SetupSpriteRenderer(bub, sprSplash, 19, alphaBlendMaterial, new Color(0.7f, 0.95f, 1f, 0.8f));
                bub.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                bub.SetActive(false);
                enemySlowBubblePool.Add(bub);
            }
        }

        public GameObject GetSlowBubbleFromPool()
        {
            foreach (var b in enemySlowBubblePool)
            {
                if (!b.activeInHierarchy) return b;
            }
            return enemySlowBubblePool[0];
        }

        // =============================================================
        // 4. LÂN VÀNG VFX (12 pháo hoa tỏa quạt, nổ star/burst)
        // =============================================================
        private void BuildLanVangVFX()
        {
            // A. Kim tuyến bay lên Nhịp 1
            GameObject gltGO = new GameObject("VFX_VangChargeGlitter");
            gltGO.transform.SetParent(transform);
            vangChargeGlitter = gltGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(vangChargeGlitter, sprGlitter, 30, additiveMaterial);
            var mainGlt = vangChargeGlitter.main;
            mainGlt.loop = false;
            mainGlt.duration = 0.4f;
            mainGlt.startLifetime = 0.35f;
            mainGlt.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
            mainGlt.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            mainGlt.simulationSpace = ParticleSystemSimulationSpace.World;
            var emGlt = vangChargeGlitter.emission;
            emGlt.rateOverTime = 0f;
            emGlt.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });
            gltGO.SetActive(false);

            // B. 12 Quả pháo bay tỏa quạt
            for (int i = 0; i < 16; i++)
            {
                GameObject rk = new GameObject($"VFX_FireworkRocket_{i}");
                rk.transform.SetParent(transform);

                var sr = rk.AddComponent<SpriteRenderer>();
                SetupSpriteRenderer(rk, sprSpark, 18, additiveMaterial, Color.yellow);
                rk.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

                var tr = rk.AddComponent<TrailRenderer>();
                tr.time = 0.2f;
                tr.startWidth = 0.25f;
                tr.endWidth = 0f;
                tr.material = additiveMaterial;
                tr.startColor = new Color(1f, 0.84f, 0f, 0.95f);
                tr.endColor = new Color(1f, 0.3f, 0.1f, 0f);

                rk.SetActive(false);
                fireworkRockets.Add(new FireworkRocket { go = rk, sr = sr, tr = tr });
            }

            // C. 12 Vụ nổ kết hợp Star & Burst vẽ tay (không tint, giữ màu đa sắc)
            for (int i = 0; i < 12; i++)
            {
                GameObject expGO = new GameObject($"VFX_FireworkExp_{i}");
                expGO.transform.SetParent(transform);

                // 4 quả star, 8 quả burst
                Sprite chosenSpr = (i % 3 == 0) ? sprFireworkStar : sprFireworkBurst;

                var sr = expGO.AddComponent<SpriteRenderer>();
                SetupSpriteRenderer(expGO, chosenSpr, 20, additiveMaterial, Color.white);
                expGO.transform.localScale = Vector3.zero;

                // Vòng sóng vàng kèm theo
                GameObject ringGO = new GameObject($"VFX_ExpRing_{i}");
                ringGO.transform.SetParent(expGO.transform);
                var ringSR = ringGO.AddComponent<SpriteRenderer>();
                SetupSpriteRenderer(ringGO, sprRingGray, 19, additiveMaterial, new Color(1f, 0.84f, 0f, 0.9f));
                ringGO.transform.localScale = Vector3.one;

                expGO.SetActive(false);
                fireworkExplosions.Add(new FireworkExpItem { go = expGO, sr = sr, ring = ringGO, ringSR = ringSR });
            }

            // D. Bụi kim tuyến rơi chậm Nhịp 4
            GameObject fallGO = new GameObject("VFX_VangFallingGlitter");
            fallGO.transform.SetParent(transform);
            vangFallingGlitter = fallGO.AddComponent<ParticleSystem>();
            SetupSpriteParticle(vangFallingGlitter, sprGlitter, 40, additiveMaterial);
            var mainFall = vangFallingGlitter.main;
            mainFall.loop = false;
            mainFall.duration = 0.8f;
            mainFall.startLifetime = 1.2f;
            mainFall.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            mainFall.gravityModifier = 0.8f;
            mainFall.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            mainFall.simulationSpace = ParticleSystemSimulationSpace.World;
            var emFall = vangFallingGlitter.emission;
            emFall.rateOverTime = 0f;
            emFall.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 35) });
            fallGO.SetActive(false);
        }

        // =============================================================
        // ĐO ĐẠC & TELEMETRY CHO PHIÊN BẢN 5A.6
        // =============================================================
        public int CountActiveVFXObjects()
        {
            int count = 0;
            // Kim
            if (kimChargeRing != null && kimChargeRing.activeInHierarchy) count++;
            if (kimShieldRing != null && kimShieldRing.activeInHierarchy) count++;
            if (kimWindLeft != null && kimWindLeft.activeInHierarchy) count++;
            if (kimWindRight != null && kimWindRight.activeInHierarchy) count++;
            if (kimEndRing != null && kimEndRing.activeInHierarchy) count++;
            if (kimSparkPS != null && kimSparkPS.gameObject.activeInHierarchy && kimSparkPS.IsAlive()) count++;
            foreach (var g in ghostTrailPool) if (g != null && g.gameObject.activeInHierarchy) count++;

            // Hỏa
            if (hoaMouthSwirl != null && hoaMouthSwirl.activeInHierarchy) count++;
            if (hoaFlameBand != null && hoaFlameBand.activeInHierarchy) count++;
            if (hoaFlameCoreBand != null && hoaFlameCoreBand.activeInHierarchy) count++;
            if (hoaSparksPS != null && hoaSparksPS.gameObject.activeInHierarchy && hoaSparksPS.IsAlive()) count++;
            foreach (var b in enemyBurnPool) if (b != null && b.activeInHierarchy) count++;

            // Thủy
            if (thuyChargeRing != null && thuyChargeRing.activeInHierarchy) count++;
            if (thuyWaveBand != null && thuyWaveBand.activeInHierarchy) count++;
            if (thuySplashPS != null && thuySplashPS.gameObject.activeInHierarchy && thuySplashPS.IsAlive()) count++;
            if (thuyRippleRing != null && thuyRippleRing.activeInHierarchy) count++;
            if (thuySplashDropPS != null && thuySplashDropPS.gameObject.activeInHierarchy && thuySplashDropPS.IsAlive()) count++;
            foreach (var s in enemySlowBubblePool) if (s != null && s.activeInHierarchy) count++;

            // Vàng
            if (vangChargeGlitter != null && vangChargeGlitter.gameObject.activeInHierarchy && vangChargeGlitter.IsAlive()) count++;
            if (vangFallingGlitter != null && vangFallingGlitter.gameObject.activeInHierarchy && vangFallingGlitter.IsAlive()) count++;
            foreach (var rk in fireworkRockets) if (rk != null && rk.go != null && rk.go.activeInHierarchy) count++;
            foreach (var exp in fireworkExplosions) if (exp != null && exp.go != null && exp.go.activeInHierarchy) count++;

            return count;
        }

        public static void LogVfxRendererMat(string objName, Component comp)
        {
            if (comp == null) return;
            Renderer rnd = comp.GetComponent<Renderer>();
            if (rnd == null) return;
            string matName = rnd.sharedMaterial != null ? rnd.sharedMaterial.name : "null";
            string shaderName = (rnd.sharedMaterial != null && rnd.sharedMaterial.shader != null) ? rnd.sharedMaterial.shader.name : "null";
            string blendMode = "Unknown";
            if (rnd.sharedMaterial != null)
            {
                int src = rnd.sharedMaterial.HasProperty("_SrcBlend") ? rnd.sharedMaterial.GetInt("_SrcBlend") : -1;
                int dst = rnd.sharedMaterial.HasProperty("_DstBlend") ? rnd.sharedMaterial.GetInt("_DstBlend") : -1;
                blendMode = $"SrcBlend={src}, DstBlend={dst}";
            }
            string sprName = "none";
            if (comp is SpriteRenderer sr && sr.sprite != null) sprName = sr.sprite.name;
            Debug.Log($"[VFXMAT] obj={objName} type={rnd.GetType().Name} mat={matName} shader={shaderName} blend=({blendMode}) sprite={sprName}");
        }

        public void LogAllSkillRenderers(string skillPrefix)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r.gameObject.name.Contains(skillPrefix) || r.transform.parent?.name.Contains(skillPrefix) == true)
                {
                    LogVfxRendererMat(r.gameObject.name, r);
                }
            }
        }

        // =============================================================
        // DỌN SẠCH KHI TRẬN KẾT THÚC / LEVEL UNLOAD
        // =============================================================
        public float CalculateVFXCoveragePercent(Camera cam)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return 0f;

            float screenH = 2f * cam.orthographicSize;
            float screenW = screenH * cam.aspect;
            float screenArea = screenW * screenH;
            if (screenArea <= 0.001f) return 0f;

            float totalArea = 0f;
            var renderers = GetComponentsInChildren<Renderer>(false);
            foreach (var r in renderers)
            {
                if (r == null || !r.gameObject.activeInHierarchy || !r.enabled) continue;
                Bounds b = r.bounds;
                float area = b.size.x * b.size.y;
                totalArea += area;
            }

            return Mathf.Clamp((totalArea / screenArea) * 100f, 0f, 100f);
        }

        public void CleanAllVFX()
        {
            // Kim
            if (kimChargeRing != null) kimChargeRing.SetActive(false);
            if (kimShieldRing != null) kimShieldRing.SetActive(false);
            if (kimWindLeft != null) kimWindLeft.SetActive(false);
            if (kimWindRight != null) kimWindRight.SetActive(false);
            if (kimEndRing != null) kimEndRing.SetActive(false);
            if (kimSilverStreak != null) kimSilverStreak.SetActive(false);
            if (kimSparkPS != null) { kimSparkPS.Stop(); kimSparkPS.gameObject.SetActive(false); }
            foreach (var g in ghostTrailPool) if (g != null) g.gameObject.SetActive(false);

            // Hỏa
            if (hoaMouthSwirl != null) hoaMouthSwirl.SetActive(false);
            if (hoaFlameBand != null) hoaFlameBand.SetActive(false);
            if (hoaFlameCoreBand != null) hoaFlameCoreBand.SetActive(false);
            if (hoaFlameStreamPS != null) { hoaFlameStreamPS.Stop(); hoaFlameStreamPS.gameObject.SetActive(false); }
            if (hoaSparksPS != null) { hoaSparksPS.Stop(); hoaSparksPS.gameObject.SetActive(false); }
            foreach (var b in enemyBurnPool) if (b != null) b.SetActive(false);

            // Thủy
            if (thuyChargeRing != null) thuyChargeRing.SetActive(false);
            if (thuyWaveBand != null) thuyWaveBand.SetActive(false);
            if (thuyWaveFront != null) thuyWaveFront.SetActive(false);
            if (thuySplashPS != null) { thuySplashPS.Stop(); thuySplashPS.gameObject.SetActive(false); }
            if (thuyRippleRing != null) thuyRippleRing.SetActive(false);
            if (thuySplashDropPS != null) { thuySplashDropPS.Stop(); thuySplashDropPS.gameObject.SetActive(false); }
            foreach (var s in enemySlowBubblePool) if (s != null) s.SetActive(false);

            // Vàng
            if (vangChargeGlitter != null) { vangChargeGlitter.Stop(); vangChargeGlitter.gameObject.SetActive(false); }
            if (vangFallingGlitter != null) { vangFallingGlitter.Stop(); vangFallingGlitter.gameObject.SetActive(false); }
            foreach (var rk in fireworkRockets) if (rk != null && rk.go != null) rk.go.SetActive(false);
            foreach (var exp in fireworkExplosions) if (exp != null && exp.go != null) exp.go.SetActive(false);
        }
    }
}
