using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace DemHoiDenLong.EditorTools
{
    public static class GenerateVFXSprites
    {
        private const string VfxDir = "Assets/_Project/Art/VFX";

        [MenuItem("DemHoiDenLong/Generate VFX Sprites")]
        public static void GenerateAllSprites()
        {
            if (!Directory.Exists(VfxDir))
            {
                Directory.CreateDirectory(VfxDir);
            }

            Debug.Log("[GenerateVFXSprites] Bắt đầu sinh 8 sprite VFX bằng code...");

            CreateSoftCircle();
            CreateSpark4();
            CreateStreak();
            CreateRing();
            CreateFlame();
            CreateWaveBand();
            CreateBurst();
            CreateDroplet();
            CreateAdditiveMaterial();

            AssetDatabase.Refresh();
            Debug.Log("[GenerateVFXSprites] Đã sinh thành công toàn bộ 8 sprite VFX và Material Additive!");
        }

        private static void SaveTexture(Texture2D tex, string fileName, bool wrapRepeat = false)
        {
            string fullPath = Path.Combine(VfxDir, fileName);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = wrapRepeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.spritePixelsPerUnit = 100f;
                importer.SaveAndReimport();
            }
        }

        // 1. vfx_soft_circle (64x64)
        private static void CreateSoftCircle()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            float maxR = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float t = Mathf.Clamp01(dist / maxR);
                    float alpha = Mathf.SmoothStep(1f, 0f, t);
                    alpha = Mathf.Pow(alpha, 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_soft_circle.png");
        }

        // 2. vfx_spark4 (128x128) - Tia sáng 4 cánh
        private static void CreateSpark4()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) / center;
                    float dy = Mathf.Abs(y - center) / center;

                    // Tia nhọn dọc và ngang giao thoa
                    float hSpike = Mathf.Pow(Mathf.Clamp01(1f - dx), 4f) * Mathf.Pow(Mathf.Clamp01(1f - dy * 4f), 2f);
                    float vSpike = Mathf.Pow(Mathf.Clamp01(1f - dy), 4f) * Mathf.Pow(Mathf.Clamp01(1f - dx * 4f), 2f);
                    float core = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) * 2.5f), 2f);

                    float alpha = Mathf.Clamp01(hSpike + vSpike + core);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_spark4.png");
        }

        // 3. vfx_streak (32x128) - Vệt dọc mờ 2 đầu
        private static void CreateStreak()
        {
            int w = 32;
            int h = 128;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float centerX = (w - 1) * 0.5f;
            float centerY = (h - 1) * 0.5f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Abs(x - centerX) / centerX;
                    float dy = Mathf.Abs(y - centerY) / centerY;

                    float alphaX = Mathf.SmoothStep(1f, 0f, dx);
                    float alphaY = Mathf.SmoothStep(1f, 0f, dy);
                    float alpha = Mathf.Clamp01(alphaX * alphaY);

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_streak.png");
        }

        // 4. vfx_ring (128x128) - Vòng tròn mảnh
        private static void CreateRing()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            float radius = center * 0.82f;
            float thickness = center * 0.08f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float delta = Mathf.Abs(dist - radius);
                    float alpha = Mathf.SmoothStep(1f, 0f, delta / thickness);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_ring.png");
        }

        // 5. vfx_flame (128x128) - Giọt lửa mềm
        private static void CreateFlame()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float centerX = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                float normY = (float)y / (size - 1); // 0 (đáy) đến 1 (đỉnh nhọn)
                // Chiều rộng lửa nở rộng ở 1/3 dưới (y ~ 0.3) và thuôn nhọn ở đỉnh (y -> 1)
                float widthAtY = Mathf.Sin(normY * Mathf.PI * 0.85f);
                if (normY > 0.85f) widthAtY *= Mathf.Clamp01((1f - normY) / 0.15f);
                float maxW = centerX * 0.8f * Mathf.Max(0.01f, widthAtY);

                for (int x = 0; x < size; x++)
                {
                    float distFromCenter = Mathf.Abs(x - centerX);
                    float alpha = 0f;
                    if (distFromCenter <= maxW)
                    {
                        float t = distFromCenter / maxW;
                        alpha = Mathf.SmoothStep(1f, 0f, t);
                        // Đáy mờ nhẹ, đỉnh thuôn mờ
                        alpha *= Mathf.Sin(Mathf.Clamp01(normY / 0.85f) * Mathf.PI * 0.5f);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_flame.png");
        }

        // 6. vfx_wave_band (512x128) - Dải sóng nước lặp ngang có bọt trắng
        private static void CreateWaveBand()
        {
            int w = 512;
            int h = 128;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            for (int y = 0; y < h; y++)
            {
                float normY = (float)y / (h - 1);
                for (int x = 0; x < w; x++)
                {
                    float normX = (float)x / w;
                    // Sóng sin chu kỳ 2 sóng trên 512px (seamless 2*PI*2)
                    float waveOffset = Mathf.Sin(normX * Mathf.PI * 4f) * 0.12f + Mathf.Cos(normX * Mathf.PI * 8f) * 0.05f;
                    float crestY = 0.75f + waveOffset;

                    float alpha = 0f;
                    float brightness = 1f;

                    if (normY <= crestY)
                    {
                        // Bên trong dải sóng
                        float depth = (crestY - normY) / crestY;
                        alpha = Mathf.SmoothStep(0.2f, 0.85f, 1f - depth); // Mép trên đậm hơn

                        // Gờ bọt trắng ở sát đỉnh sóng (khoảng 0.08)
                        if (normY >= crestY - 0.08f)
                        {
                            brightness = 1.0f;
                            alpha = 1.0f;
                        }
                    }
                    else
                    {
                        // Mờ dần ngoài đỉnh sóng
                        float over = (normY - crestY) / 0.1f;
                        alpha = Mathf.SmoothStep(1f, 0f, over);
                    }

                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_wave_band.png", wrapRepeat: true);
        }

        // 7. vfx_burst (128x128) - Hoa pháo hoa 8 tia
        private static void CreateBurst()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 dir = new Vector2(x - center, y - center);
                    float dist = dir.magnitude;
                    float normDist = dist / center;

                    if (normDist >= 1f || dist < 0.5f)
                    {
                        tex.SetPixel(x, y, dist < 1f ? Color.white : Color.clear);
                        continue;
                    }

                    float angle = Mathf.Atan2(dir.y, dir.x); // -PI đến PI
                    // 8 tia = chu kỳ 8
                    float rayFactor = Mathf.Cos(angle * 8f); // 1 tại tâm tia, -1 giữa các tia
                    rayFactor = Mathf.Pow(Mathf.Clamp01((rayFactor + 1f) * 0.5f), 6f); // Rất mảnh

                    float fade = Mathf.SmoothStep(1f, 0f, normDist);
                    float core = Mathf.SmoothStep(1f, 0f, normDist * 3.5f);
                    float alpha = Mathf.Clamp01(rayFactor * fade + core);

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_burst.png");
        }

        // 8. vfx_droplet (64x64) - Giọt nước
        private static void CreateDroplet()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float centerX = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                float normY = (float)y / (size - 1); // 0 (đáy tròn) đến 1 (đỉnh nhọn)
                float maxW = 0f;
                if (normY < 0.4f)
                {
                    // Nửa dưới hình cầu
                    float t = normY / 0.4f;
                    maxW = centerX * 0.7f * Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow(1f - t, 2f)));
                }
                else
                {
                    // Nửa trên vuốt nhọn dần lên đỉnh
                    float t = (normY - 0.4f) / 0.6f;
                    maxW = centerX * 0.7f * (1f - t);
                }

                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - centerX);
                    float alpha = 0f;
                    if (dx <= maxW && maxW > 0.01f)
                    {
                        alpha = Mathf.SmoothStep(1f, 0f, dx / maxW);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            SaveTexture(tex, "vfx_droplet.png");
        }

        // Tạo Material Additive cho hạt VFX
        private static void CreateAdditiveMaterial()
        {
            string matPath = Path.Combine(VfxDir, "Mat_VFX_Additive.mat");
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Mobile/Particles/Additive");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            // Thiết lập chế độ Additive
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1); // Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 1);     // Additive
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_BLENDMODE_ADD");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            AssetDatabase.CreateAsset(mat, matPath);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GenerateVFXSprites] Đã tạo Material Additive tại {matPath} với shader '{shader.name}'");
        }
    }
}
