#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.VFX;

namespace DemHoiDenLong.EditorTools
{
    public static class CreateSkillVFXPrefabs
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/VFX";
        private const string GeneratedDir = "Assets/_Project/Art/VFX/Generated";
        private const string MaterialsDir = "Assets/_Project/Art/VFX/Materials";

        [MenuItem("DemHoiDenLong/Generate Skill VFX Prefabs")]
        public static void GenerateAllPrefabs()
        {
            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
                AssetDatabase.Refresh();
            }

            Material matAdd = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_VFX_Additive.mat");
            Material matAlpha = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_VFX_AlphaBlend.mat");
            Material matFire = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_VFX_ScrollMask_Fire.mat");
            Material matWater = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_VFX_ScrollMask_Water.mat");

            CreateKimLanPrefab(matAdd);
            CreateHoaLanPrefab(matAdd, matFire);
            CreateThuyLanPrefab(matAlpha, matWater);
            CreateLanVangPrefab(matAdd);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateSkillVFXPrefabs] Đã tạo thành công 4 Prefabs VFX tại Assets/_Project/Prefabs/VFX/ !");
        }

        private static Sprite LoadSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedDir}/{fileName}.png");
        }

        private static void CreateKimLanPrefab(Material matAdd)
        {
            GameObject root = new GameObject("PF_VFX_Skill_Kim");

            // 1. ChargeRing
            GameObject chargeRing = new GameObject("ChargeRing");
            chargeRing.transform.SetParent(root.transform, false);
            var srCharge = chargeRing.AddComponent<SpriteRenderer>();
            srCharge.sprite = LoadSprite("vfx_ring_gray");
            srCharge.material = matAdd;
            srCharge.sortingOrder = 18;
            var vlCharge = chargeRing.AddComponent<VfxLayer>();
            vlCharge.duration = 0.15f;
            vlCharge.scaleCurve = AnimationCurve.Linear(0f, 2.0f, 1f, 0.4f);
            vlCharge.alphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            vlCharge.tintColor = new Color32(0xE8, 0xD0, 0x8D, 0xFF);

            // 2. ShieldRing
            GameObject shieldRing = new GameObject("ShieldRing");
            shieldRing.transform.SetParent(root.transform, false);
            var srShield = shieldRing.AddComponent<SpriteRenderer>();
            srShield.sprite = LoadSprite("vfx_shield_aura_gray");
            srShield.material = matAdd;
            srShield.sortingOrder = 18;
            var vlShield = shieldRing.AddComponent<VfxLayer>();
            vlShield.duration = 1.5f;
            vlShield.rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 360f);
            vlShield.tintColor = new Color32(0xE8, 0xD0, 0x8D, 0xFF);

            // 3. Streaks
            GameObject streaks = new GameObject("Streaks");
            streaks.transform.SetParent(root.transform, false);
            var srStreaks = streaks.AddComponent<SpriteRenderer>();
            srStreaks.sprite = LoadSprite("vfx_silver_streaks_proc");
            srStreaks.material = matAdd;
            srStreaks.sortingOrder = 17;
            var vlStreaks = streaks.AddComponent<VfxLayer>();
            vlStreaks.duration = 1.5f;
            vlStreaks.posYCurve = AnimationCurve.Linear(0f, 2f, 1f, -2f);

            // 4. Sparks
            GameObject sparks = new GameObject("Sparks");
            sparks.transform.SetParent(root.transform, false);
            var psSparks = sparks.AddComponent<ParticleSystem>();
            var psR = sparks.GetComponent<ParticleSystemRenderer>();
            psR.material = matAdd;
            psR.sortingOrder = 19;
            var vlSparks = sparks.AddComponent<VfxLayer>();
            vlSparks.duration = 1.5f;

            // 5. EndRing
            GameObject endRing = new GameObject("EndRing");
            endRing.transform.SetParent(root.transform, false);
            var srEnd = endRing.AddComponent<SpriteRenderer>();
            srEnd.sprite = LoadSprite("vfx_ring_gray");
            srEnd.material = matAdd;
            srEnd.sortingOrder = 18;
            var vlEnd = endRing.AddComponent<VfxLayer>();
            vlEnd.duration = 0.3f;
            vlEnd.scaleCurve = AnimationCurve.Linear(0f, 1.2f, 1f, 2.5f);
            vlEnd.alphaCurve = AnimationCurve.Linear(0f, 0.8f, 1f, 0f);
            vlEnd.tintColor = new Color32(0xE8, 0xD0, 0x8D, 0xFF);

            SaveAndDestroy(root, $"{PrefabDir}/PF_VFX_Skill_Kim.prefab");
        }

        private static void CreateHoaLanPrefab(Material matAdd, Material matFire)
        {
            GameObject root = new GameObject("PF_VFX_Skill_Hoa");

            // 1. MouthSwirl
            GameObject mouth = new GameObject("MouthSwirl");
            mouth.transform.SetParent(root.transform, false);
            var srMouth = mouth.AddComponent<SpriteRenderer>();
            srMouth.sprite = LoadSprite("vfx_fire_swirl_proc");
            srMouth.material = matAdd;
            srMouth.sortingOrder = 19;
            var vlMouth = mouth.AddComponent<VfxLayer>();
            vlMouth.duration = 0.2f;
            vlMouth.scaleCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1.0f);
            vlMouth.rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, -360f);
            vlMouth.tintColor = new Color32(0xD4, 0x42, 0x1E, 0xFF);

            // 2. FlameBand
            GameObject flame = new GameObject("FlameBand");
            flame.transform.SetParent(root.transform, false);
            var srFlame = flame.AddComponent<SpriteRenderer>();
            srFlame.sprite = LoadSprite("vfx_fire_wave_proc");
            srFlame.material = matFire != null ? matFire : matAdd;
            srFlame.sortingOrder = 18;
            var vlFlame = flame.AddComponent<VfxLayer>();
            vlFlame.duration = 1.1f;
            vlFlame.posYCurve = AnimationCurve.EaseInOut(0f, -2f, 1f, 6f);
            vlFlame.tintColor = new Color(1f, 0.95f, 0.85f, 0.95f);

            // 3. FlameCoreBand
            GameObject core = new GameObject("FlameCoreBand");
            core.transform.SetParent(root.transform, false);
            var srCore = core.AddComponent<SpriteRenderer>();
            srCore.sprite = LoadSprite("vfx_fire_wave_proc");
            srCore.material = matFire != null ? matFire : matAdd;
            srCore.sortingOrder = 19;
            var vlCore = core.AddComponent<VfxLayer>();
            vlCore.duration = 1.1f;
            vlCore.tintColor = new Color(1f, 0.6f, 0.2f, 0.7f);

            // 4. Sparks
            GameObject sparks = new GameObject("Sparks");
            sparks.transform.SetParent(root.transform, false);
            var ps = sparks.AddComponent<ParticleSystem>();
            var psR = sparks.GetComponent<ParticleSystemRenderer>();
            psR.material = matAdd;
            psR.sortingOrder = 20;
            var vlSparks = sparks.AddComponent<VfxLayer>();
            vlSparks.duration = 1.1f;

            SaveAndDestroy(root, $"{PrefabDir}/PF_VFX_Skill_Hoa.prefab");
        }

        private static void CreateThuyLanPrefab(Material matAlpha, Material matWater)
        {
            GameObject root = new GameObject("PF_VFX_Skill_Thuy");

            // 1. ChargeRing
            GameObject charge = new GameObject("ChargeRing");
            charge.transform.SetParent(root.transform, false);
            var srCharge = charge.AddComponent<SpriteRenderer>();
            srCharge.sprite = LoadSprite("vfx_ring_gray");
            srCharge.material = matAlpha;
            srCharge.sortingOrder = 18;
            var vlCharge = charge.AddComponent<VfxLayer>();
            vlCharge.duration = 0.2f;
            vlCharge.scaleCurve = AnimationCurve.Linear(0f, 0.4f, 1f, 1.6f);
            vlCharge.tintColor = new Color32(0x3A, 0x86, 0xC8, 0xFF);

            // 2. WaveBand
            GameObject wave = new GameObject("WaveBand");
            wave.transform.SetParent(root.transform, false);
            var srWave = wave.AddComponent<SpriteRenderer>();
            srWave.sprite = LoadSprite("vfx_water_wave_alpha");
            srWave.material = matWater != null ? matWater : matAlpha;
            srWave.sortingOrder = 18;
            var vlWave = wave.AddComponent<VfxLayer>();
            vlWave.duration = 1.2f;
            vlWave.posYCurve = AnimationCurve.Linear(0f, -6f, 1f, 6f);

            // 3. Splash
            GameObject splash = new GameObject("Splash");
            splash.transform.SetParent(root.transform, false);
            var ps = splash.AddComponent<ParticleSystem>();
            var psR = splash.GetComponent<ParticleSystemRenderer>();
            psR.material = matAlpha;
            psR.sortingOrder = 19;
            var vlSplash = splash.AddComponent<VfxLayer>();
            vlSplash.duration = 1.2f;

            // 4. Ripples
            GameObject ripple = new GameObject("Ripples");
            ripple.transform.SetParent(root.transform, false);
            var srRip = ripple.AddComponent<SpriteRenderer>();
            srRip.sprite = LoadSprite("vfx_ring_gray");
            srRip.material = matAlpha;
            srRip.sortingOrder = 17;
            var vlRip = ripple.AddComponent<VfxLayer>();
            vlRip.duration = 0.4f;
            vlRip.scaleCurve = AnimationCurve.Linear(0f, 1f, 1f, 3f);
            vlRip.alphaCurve = AnimationCurve.Linear(0f, 0.8f, 1f, 0f);

            SaveAndDestroy(root, $"{PrefabDir}/PF_VFX_Skill_Thuy.prefab");
        }

        private static void CreateLanVangPrefab(Material matAdd)
        {
            GameObject root = new GameObject("PF_VFX_Skill_Vang");

            // 1. ChargeGlitter
            GameObject glitter = new GameObject("ChargeGlitter");
            glitter.transform.SetParent(root.transform, false);
            var psG = glitter.AddComponent<ParticleSystem>();
            var psGR = glitter.GetComponent<ParticleSystemRenderer>();
            psGR.material = matAdd;
            psGR.sortingOrder = 20;
            var vlG = glitter.AddComponent<VfxLayer>();
            vlG.duration = 0.1f;

            // 2. RocketsContainer
            GameObject rockets = new GameObject("RocketsContainer");
            rockets.transform.SetParent(root.transform, false);
            var vlRockets = rockets.AddComponent<VfxLayer>();
            vlRockets.duration = 0.75f;

            // 3. ExplosionsContainer
            GameObject exp = new GameObject("ExplosionsContainer");
            exp.transform.SetParent(root.transform, false);
            var vlExp = exp.AddComponent<VfxLayer>();
            vlExp.duration = 0.4f;

            // 4. FallingGlitter
            GameObject falling = new GameObject("FallingGlitter");
            falling.transform.SetParent(root.transform, false);
            var psF = falling.AddComponent<ParticleSystem>();
            var psFR = falling.GetComponent<ParticleSystemRenderer>();
            psFR.material = matAdd;
            psFR.sortingOrder = 20;
            var vlF = falling.AddComponent<VfxLayer>();
            vlF.duration = 0.6f;

            SaveAndDestroy(root, $"{PrefabDir}/PF_VFX_Skill_Vang.prefab");
        }

        private static void SaveAndDestroy(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }
    }
}
#endif
