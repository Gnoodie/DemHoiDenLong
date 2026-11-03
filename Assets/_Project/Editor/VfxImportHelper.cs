using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public static class VfxImportHelper
    {
        private const string VfxGeneratedDir = "Assets/_Project/Art/VFX/Generated";
        private const string VfxMaterialsDir = "Assets/_Project/Art/VFX/Materials";
        private const string VfxResourcesDir = "Assets/_Project/Resources/VFX";
        private const string ConfigPath = "Assets/_Project/Data/VFX/VfxImportConfig.asset";

        private static bool loggedSmokeWarning = false;
        private static bool loggedRocketWarning = false;

        [MenuItem("DemHoiDenLong/VFX Import Helper")]
        public static void RunImport()
        {
            EnsureDirectories();
            VfxImportConfig config = GetOrCreateConfig();

            // 1. Setup Materials
            SetupMaterials();

            // 2. Check Optional Sprites
            CheckOptionalSprites();

            // 3. Configure TextureImporters for processed textures
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================================");
            sb.AppendLine("=== DEM HOI DEN LONG - VFX IMPORT HELPER (GIAI ĐOẠN 5A.5) ===");
            sb.AppendLine("=========================================================================================");
            sb.AppendLine(string.Format("{0,-28} | {1,-9} | {2,-12} | {3,-15} | {4,-10}", "File", "Pivot", "Vật liệu", "Bản xám", "Trạng thái"));
            sb.AppendLine(new string('-', 95));

            int processedCount = 0;

            foreach (var entry in config.entries)
            {
                string path = $"{VfxGeneratedDir}/{entry.outputFile}";
                if (!File.Exists(path))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
                    if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
                    if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; changed = true; }
                    if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
                    if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }
                    if (importer.alphaIsTransparency != entry.isAlphaFromLuminance) { importer.alphaIsTransparency = entry.isAlphaFromLuminance; changed = true; }

                    TextureImporterSettings tis = new TextureImporterSettings();
                    importer.ReadTextureSettings(tis);
                    tis.spriteAlignment = (int)SpriteAlignment.Custom;
                    tis.spritePivot = entry.pivot;
                    importer.SetTextureSettings(tis);
                    changed = true;

                    if (changed)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                    }
                }

                processedCount++;
                string grayStatus = entry.isGrayscale ? "Có (_gray)" : "Không";
                sb.AppendLine(string.Format("{0,-28} | ({1:F2},{2:F2})    | {3,-15} | {4,-10} | OK", entry.outputFile, entry.pivot.x, entry.pivot.y, entry.materialType, grayStatus));
            }

            sb.AppendLine(new string('-', 95));
            sb.AppendLine($"Đã thiết lập TextureImporter & Material cho {processedCount} sprites VFX.");
            sb.AppendLine("Pipeline: Universal Render Pipeline 2D (Renderer2D). Shader: Universal Render Pipeline/Particles/Unlit & Custom/VFX_ScrollMask.");
            sb.AppendLine("=========================================================================================");

            Debug.Log(sb.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/_Project/Data/VFX")) Directory.CreateDirectory("Assets/_Project/Data/VFX");
            if (!Directory.Exists(VfxMaterialsDir)) Directory.CreateDirectory(VfxMaterialsDir);
            if (!Directory.Exists(VfxResourcesDir)) Directory.CreateDirectory(VfxResourcesDir);
        }

        private static VfxImportConfig GetOrCreateConfig()
        {
            VfxImportConfig cfg = AssetDatabase.LoadAssetAtPath<VfxImportConfig>(ConfigPath);
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<VfxImportConfig>();
                cfg.PopulateDefaults();
                AssetDatabase.CreateAsset(cfg, ConfigPath);
                AssetDatabase.SaveAssets();
            }
            return cfg;
        }

        private static void CheckOptionalSprites()
        {
            string smokePath = $"{VfxGeneratedDir}/vfx_smoke.png";
            if (!File.Exists(smokePath) && !loggedSmokeWarning)
            {
                Debug.LogWarning("[VFX] Không tìm thấy sprite tùy chọn 'vfx_smoke.png'. Sử dụng fallback hạt vfx_spark/vfx_soft_circle.");
                loggedSmokeWarning = true;
            }

            string rocketPath = $"{VfxGeneratedDir}/vfx_rocket_trail.png";
            if (!File.Exists(rocketPath) && !loggedRocketWarning)
            {
                Debug.LogWarning("[VFX] Không tìm thấy sprite tùy chọn 'vfx_rocket_trail.png'. Sử dụng fallback TrailRenderer vfx_spark.");
                loggedRocketWarning = true;
            }
        }

        private static void SetupMaterials()
        {
            Shader urpParticle = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (urpParticle == null) urpParticle = Shader.Find("Particles/Standard Unlit");
            if (urpParticle == null) urpParticle = Shader.Find("Sprites/Default");

            Shader scrollMaskShader = Shader.Find("Custom/VFX_ScrollMask");
            if (scrollMaskShader == null) scrollMaskShader = urpParticle;

            // 1. Mat_VFX_Additive
            CreateOrUpdateMaterial($"{VfxMaterialsDir}/Mat_VFX_Additive.mat", $"{VfxResourcesDir}/Mat_VFX_Additive.mat", urpParticle, mat =>
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 1); // Additive
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
                mat.SetFloat("_ZWrite", 0f);
            });

            // 2. Mat_VFX_AlphaBlend
            CreateOrUpdateMaterial($"{VfxMaterialsDir}/Mat_VFX_AlphaBlend.mat", $"{VfxResourcesDir}/Mat_VFX_AlphaBlend.mat", urpParticle, mat =>
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0); // Alpha
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
            });

            // 3. Mat_VFX_ScrollMask_Fire (Additive)
            CreateOrUpdateMaterial($"{VfxMaterialsDir}/Mat_VFX_ScrollMask_Fire.mat", $"{VfxResourcesDir}/Mat_VFX_ScrollMask_Fire.mat", scrollMaskShader, mat =>
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.SetVector("_ScrollSpeed", new Vector4(0f, 0.8f, 0f, 0f));
                mat.SetVector("_Feather", new Vector4(0.05f, 0.05f, 0.08f, 0.08f));
                mat.SetColor("_Color", Color.white);
            });

            // 4. Mat_VFX_ScrollMask_Water (AlphaBlend)
            CreateOrUpdateMaterial($"{VfxMaterialsDir}/Mat_VFX_ScrollMask_Water.mat", $"{VfxResourcesDir}/Mat_VFX_ScrollMask_Water.mat", scrollMaskShader, mat =>
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.SetVector("_ScrollSpeed", new Vector4(0f, 0.6f, 0f, 0f));
                mat.SetVector("_Feather", new Vector4(0.04f, 0.04f, 0.04f, 0.04f));
                mat.SetColor("_Color", Color.white);
            });
        }

        private static void CreateOrUpdateMaterial(string assetPath, string resPath, Shader shader, System.Action<Material> config)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                mat = new Material(shader);
                config(mat);
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            else
            {
                mat.shader = shader;
                config(mat);
                EditorUtility.SetDirty(mat);
            }

            // Copy to Resources for runtime loading
            Material resMat = AssetDatabase.LoadAssetAtPath<Material>(resPath);
            if (resMat == null)
            {
                AssetDatabase.CopyAsset(assetPath, resPath);
            }
            else
            {
                resMat.CopyPropertiesFromMaterial(mat);
                EditorUtility.SetDirty(resMat);
            }
        }
    }
}
