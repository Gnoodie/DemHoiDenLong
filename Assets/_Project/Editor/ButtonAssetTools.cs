using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DemHoiDenLong.Editor
{
    public static class ButtonAssetTools
    {
        private static readonly HashSet<string> warnedMissingFiles = new HashSet<string>();

        [MenuItem("DemHoiDenLong/UI/Import Button Assets")]
        public static void ImportButtonAssets()
        {
            Debug.Log("=== [BẮT ĐẦU IMPORT BUTTON ASSETS] ===");

            string[] buttonFiles = new[]
            {
                "btn_gold_play.png",
                "btn_navy_lore.png",
                "btn_coral_exit.png",
                "icon_play.png",
                "icon_book.png",
                "icon_exit.png",
                "btn_close.png"
            };

            string[] storyFiles = new[]
            {
                "story_card.png",
                "story_ribbon.png",
                "story_medallion.png",
                "story_divider.png"
            };

            int successCount = 0;

            foreach (var file in buttonFiles)
            {
                string path = ResolveAssetPath("Button", "Buttons", file);
                if (string.IsNullOrEmpty(path))
                {
                    WarnMissingOnce(file);
                    continue;
                }

                ConfigureTexture(path, isRectButton: file.StartsWith("btn_") && !file.Contains("close"), isStoryCard: false);
                successCount++;
            }

            foreach (var file in storyFiles)
            {
                string path = ResolveAssetPath("Story", "Story", file);
                if (string.IsNullOrEmpty(path))
                {
                    WarnMissingOnce(file);
                    continue;
                }

                ConfigureTexture(path, isRectButton: false, isStoryCard: file.Contains("card"));
                successCount++;
            }

            // Tạo hoặc cập nhật UiButtonStyleSet asset
            CreateOrUpdateStyleSetAsset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"=== [HOÀN TẤT IMPORT BUTTON ASSETS] Thành công: {successCount} assets ===");
        }

        private static string ResolveAssetPath(string primaryDir, string secondaryDir, string filename)
        {
            string p1 = $"Assets/_Project/Art/UI/{primaryDir}/{filename}";
            if (File.Exists(p1)) return p1;

            string p2 = $"Assets/_Project/Art/UI/{secondaryDir}/{filename}";
            if (File.Exists(p2)) return p2;

            return null;
        }

        private static void WarnMissingOnce(string filename)
        {
            if (!warnedMissingFiles.Contains(filename))
            {
                warnedMissingFiles.Add(filename);
                Debug.LogWarning($"[UI Asset Warning] Không tìm thấy tài nguyên: '{filename}'. Giữ nguyên giao diện hiện có ở vị trí tương ứng.");
            }
        }

        private static void ConfigureTexture(string path, bool isRectButton, bool isStoryCard)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            TextureImporterSettings texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            if (texSettings.spriteMeshType != SpriteMeshType.FullRect)
            {
                texSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(texSettings);
                changed = true;
            }

            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            // Đọc kích thước ảnh gốc từ file
            int width = 0, height = 0;
            GetImageDimensions(path, out width, out height);

            if (isRectButton && width > 0 && height > 0)
            {
                // 3 nút chữ nhật: 9-slice border (trái/phải = 18% chiều rộng, trên/dưới = 28% chiều cao)
                // Vector4: (x: left, y: bottom, z: right, w: top)
                int borderLR = Mathf.RoundToInt(width * 0.18f);
                int borderBT = Mathf.RoundToInt(height * 0.28f);
                Vector4 border = new Vector4(borderLR, borderBT, borderLR, borderBT);

                if (importer.spriteBorder != border)
                {
                    importer.spriteBorder = border;
                    changed = true;
                }
            }
            else if (isStoryCard && width > 0 && height > 0)
            {
                // story_card: border 15% mọi cạnh
                int borderLR = Mathf.RoundToInt(width * 0.15f);
                int borderBT = Mathf.RoundToInt(height * 0.15f);
                Vector4 border = new Vector4(borderLR, borderBT, borderLR, borderBT);

                if (importer.spriteBorder != border)
                {
                    importer.spriteBorder = border;
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                Debug.Log($"[Import Asset] Đã cấu hình chuẩn: {path} (Border: {importer.spriteBorder})");
            }
        }

        private static void GetImageDimensions(string assetPath, out int width, out int height)
        {
            width = 0;
            height = 0;
            try
            {
                byte[] bytes = File.ReadAllBytes(assetPath);
                // Đọc kích thước PNG từ chunk IHDR (byte 16-23)
                if (bytes.Length >= 24 && bytes[1] == 'P' && bytes[2] == 'N' && bytes[3] == 'G')
                {
                    width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
                    height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Không thể đọc dimensions của {assetPath}: {ex.Message}");
            }
        }

        public static void CreateOrUpdateStyleSetAsset()
        {
            string dir = "Assets/_Project/Data/UI";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string assetPath = $"{dir}/UiButtonStyleSet.asset";
            var styleSet = AssetDatabase.LoadAssetAtPath<DemHoiDenLong.UI.UiButtonStyleSet>(assetPath);
            if (styleSet == null)
            {
                styleSet = ScriptableObject.CreateInstance<DemHoiDenLong.UI.UiButtonStyleSet>();
                AssetDatabase.CreateAsset(styleSet, assetPath);
            }

            // Gán sprite
            Sprite goldSpr = LoadSprite("btn_gold_play");
            Sprite navySpr = LoadSprite("btn_navy_lore");
            Sprite coralSpr = LoadSprite("btn_coral_exit");

            styleSet.goldPrimary.backgroundSprite = goldSpr;
            styleSet.navySecondary.backgroundSprite = navySpr;
            styleSet.coralExit.backgroundSprite = coralSpr;

            EditorUtility.SetDirty(styleSet);
        }

        public static Sprite LoadSprite(string name)
        {
            string[] paths = new[]
            {
                $"Assets/_Project/Art/UI/Button/{name}.png",
                $"Assets/_Project/Art/UI/Buttons/{name}.png",
                $"Assets/_Project/Art/UI/Story/{name}.png"
            };

            foreach (var p in paths)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) return s;
            }
            return null;
        }

        // ==========================================
        // TOOL: REMOVE WHITE BACKGROUND
        // ==========================================
        [MenuItem("DemHoiDenLong/UI/Remove White Background")]
        public static void RemoveWhiteBackground()
        {
            Debug.Log("=== [KIỂM TRA VÀ KHỬ NỀN TRẮNG (REMOVE WHITE BACKGROUND)] ===");

            List<string> candidatePaths = new List<string>();
            string[] dirs = new[] { "Assets/_Project/Art/UI/Button", "Assets/_Project/Art/UI/Story" };

            foreach (var d in dirs)
            {
                if (Directory.Exists(d))
                {
                    var files = Directory.GetFiles(d, "*.png");
                    foreach (var f in files)
                    {
                        if (!f.EndsWith("_clean.png"))
                        {
                            candidatePaths.Add(f.Replace('\\', '/'));
                        }
                    }
                }
            }

            List<string> processedFiles = new List<string>();

            foreach (var path in candidatePaths)
            {
                if (CheckAndCleanWhiteBackground(path, threshold: 0.15f))
                {
                    processedFiles.Add(path);
                }
            }

            if (processedFiles.Count == 0)
            {
                Debug.Log($"[RemoveWhiteBackground] Đã quét {candidatePaths.Count} file PNG. Tất cả các file đều đã có nền trong suốt (alpha = 0 tại 4 góc), KHÔNG có file nào nền trắng đặc cần xử lý.");
            }
            else
            {
                Debug.Log($"[RemoveWhiteBackground] Đã xử lý khử nền trắng cho {processedFiles.Count} file: {string.Join(", ", processedFiles)}");
                AssetDatabase.Refresh();
            }
        }

        private static bool CheckAndCleanWhiteBackground(string path, float threshold)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);

                int w = tex.width;
                int h = tex.height;

                Color c00 = tex.GetPixel(0, 0);
                Color c01 = tex.GetPixel(w - 1, 0);
                Color c10 = tex.GetPixel(0, h - 1);
                Color c11 = tex.GetPixel(w - 1, h - 1);

                // Nếu 4 góc đã có alpha = 0, ảnh đã là nền trong suốt
                if (c00.a < 0.05f && c01.a < 0.05f && c10.a < 0.05f && c11.a < 0.05f)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                    return false;
                }

                // Kiểm tra xem 4 góc có phải là màu trắng không (R, G, B > 0.9 và A > 0.9)
                bool isWhiteBg = (IsNearWhite(c00) && IsNearWhite(c01) && IsNearWhite(c10) && IsNearWhite(c11));
                if (!isWhiteBg)
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                    return false;
                }

                Debug.Log($"[RemoveWhiteBackground] Phát hiện nền trắng đặc tại: {path}. Đang thực hiện flood-fill và làm mịn viền...");

                Color[] pixels = tex.GetPixels();
                bool[,] visited = new bool[w, h];
                Queue<Vector2Int> q = new Queue<Vector2Int>();

                void TryEnqueue(int x, int y)
                {
                    if (x < 0 || x >= w || y < 0 || y >= h) return;
                    if (visited[x, y]) return;
                    Color c = pixels[y * w + x];
                    if (IsNearWhite(c, threshold))
                    {
                        visited[x, y] = true;
                        q.Enqueue(new Vector2Int(x, y));
                    }
                }

                TryEnqueue(0, 0);
                TryEnqueue(w - 1, 0);
                TryEnqueue(0, h - 1);
                TryEnqueue(w - 1, h - 1);

                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    pixels[p.y * w + p.x] = new Color(0, 0, 0, 0);

                    TryEnqueue(p.x + 1, p.y);
                    TryEnqueue(p.x - 1, p.y);
                    TryEnqueue(p.x, p.y + 1);
                    TryEnqueue(p.x, p.y - 1);
                }

                tex.SetPixels(pixels);
                tex.Apply();

                string cleanPath = path.Substring(0, path.Length - 4) + "_clean.png";
                byte[] cleanBytes = tex.EncodeToPNG();
                File.WriteAllBytes(cleanPath, cleanBytes);

                UnityEngine.Object.DestroyImmediate(tex);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi xử lý file {path}: {ex.Message}");
                return false;
            }
        }

        private static bool IsNearWhite(Color c, float threshold = 0.15f)
        {
            if (c.a < 0.1f) return false;
            return (1f - c.r <= threshold) && (1f - c.g <= threshold) && (1f - c.b <= threshold);
        }
    }
}
