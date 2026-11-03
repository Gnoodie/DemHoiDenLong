using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public static class GameplayScaleTool
    {
        private const string ConfigPath = "Assets/_Project/Data/GameplayScaleConfig.asset";
        private const float CameraRefWidth = 5.625f; // OrthoSize 5.0 * 2 * (9/16)

        public struct ScaleEntry
        {
            public string name;
            public string prefabPath;
            public float targetPercent;
            public float colliderRatio;
        }

        private static List<ScaleEntry> GetScaleEntries(GameplayScaleConfig config)
        {
            return new List<ScaleEntry>
            {
                new ScaleEntry { name = "PF_Enemy_DenGiay", prefabPath = "Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGiay.prefab", targetPercent = config.denGiayWidthPercent, colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_Enemy_DenGai",  prefabPath = "Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGai.prefab",  targetPercent = config.denGaiWidthPercent,  colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_Enemy_DenCa",   prefabPath = "Assets/_Project/Prefabs/Enemies/PF_Enemy_DenCa.prefab",   targetPercent = config.denCaWidthPercent,   colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_Enemy_DenQuy",  prefabPath = "Assets/_Project/Prefabs/Enemies/PF_Enemy_DenQuy.prefab",  targetPercent = config.denQuyWidthPercent,  colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_SpikeDebris",   prefabPath = "Assets/_Project/Prefabs/Enemies/PF_SpikeDebris.prefab",   targetPercent = config.manhGaiWidthPercent, colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_LanKim",        prefabPath = "Assets/_Project/Prefabs/Lans/PF_LanKim.prefab",        targetPercent = config.lanWidthPercent,     colliderRatio = config.lanColliderRatio },
                new ScaleEntry { name = "PF_LanHoa",        prefabPath = "Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab",        targetPercent = config.lanWidthPercent,     colliderRatio = config.lanColliderRatio },
                new ScaleEntry { name = "PF_LanThuy",       prefabPath = "Assets/_Project/Prefabs/Lans/PF_LanThuy.prefab",       targetPercent = config.lanWidthPercent,     colliderRatio = config.lanColliderRatio },
                new ScaleEntry { name = "PF_LanVang",       prefabPath = "Assets/_Project/Prefabs/Lans/PF_LanVang.prefab",       targetPercent = config.lanWidthPercent,     colliderRatio = config.lanColliderRatio },
                new ScaleEntry { name = "PF_BossAct1",      prefabPath = "Assets/_Project/Prefabs/Bosses/PF_BossAct1.prefab",      targetPercent = config.bossWidthPercent,    colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_BossAct2",      prefabPath = "Assets/_Project/Prefabs/Bosses/PF_BossAct2.prefab",      targetPercent = config.bossWidthPercent,    colliderRatio = config.enemyColliderRatio },
                new ScaleEntry { name = "PF_BossAct3",      prefabPath = "Assets/_Project/Prefabs/Bosses/PF_BossAct3.prefab",      targetPercent = config.bossWidthPercent,    colliderRatio = config.enemyColliderRatio }
            };
        }

        public static GameplayScaleConfig GetOrCreateConfig()
        {
            GameplayScaleConfig config = AssetDatabase.LoadAssetAtPath<GameplayScaleConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameplayScaleConfig>();
                config.referenceOrthoSize = 5f;
                config.referenceAspectWidth = 9f;
                config.referenceAspectHeight = 16f;
                config.denGiayWidthPercent = 12f;
                config.denGaiWidthPercent = 13f;
                config.denCaWidthPercent = 12f;
                config.denQuyWidthPercent = 14f;
                config.manhGaiWidthPercent = 4f;
                config.lanWidthPercent = 10f;
                config.bossWidthPercent = 28f;
                config.enemyColliderRatio = 0.8f;
                config.lanColliderRatio = 0.5f;

                System.IO.Directory.CreateDirectory("Assets/_Project/Data");
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[GameplayScale] Created {ConfigPath}");
            }
            return config;
        }

        [MenuItem("DemHoiDenLong/Gameplay Scale Report")]
        public static void GenerateScaleReport()
        {
            GameplayScaleConfig config = GetOrCreateConfig();
            var entries = GetScaleEntries(config);

            Debug.Log("======================== [GAMEPLAY SCALE REPORT - HIỆN TẠI] ========================");
            Debug.Log(string.Format("{0,-20} | {1,-10} | {2,-8} | {3,-10} | {4,-12} | {5,-12}", 
                "Prefab Name", "Scale", "Width(u)", "% Screen", "Target %", "Collider Size"));
            Debug.Log(new string('-', 85));

            foreach (var e in entries)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(e.prefabPath);
                if (go == null)
                {
                    Debug.LogWarning($"[GameplayScale] Prefab not found: {e.prefabPath}");
                    continue;
                }

                SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
                Collider2D col = go.GetComponentInChildren<Collider2D>();

                float curScaleX = go.transform.localScale.x;
                float visualWidth = 0f;
                if (sr != null && sr.sprite != null)
                {
                    Rect alphaRect = GetAlphaBoundingBox(sr.sprite, 0.1f);
                    visualWidth = (alphaRect.width / sr.sprite.pixelsPerUnit) * curScaleX;
                }

                float curPercent = (visualWidth / CameraRefWidth) * 100f;
                string colStr = "None";
                if (col is CapsuleCollider2D cap) colStr = $"{cap.size.x:F2}x{cap.size.y:F2}";
                else if (col is BoxCollider2D box) colStr = $"{box.size.x:F2}x{box.size.y:F2}";
                else if (col is CircleCollider2D cir) colStr = $"R={cir.radius:F2}";

                Debug.Log(string.Format("{0,-20} | {1,-10:F3} | {2,-8:F3} | {3,-9:F1}% | {4,-11:F1}% | {5,-12}",
                    e.name, curScaleX, visualWidth, curPercent, e.targetPercent, colStr));
            }
            Debug.Log("====================================================================================");
        }

        [MenuItem("DemHoiDenLong/Apply Scale Config")]
        public static void ApplyScaleConfig()
        {
            GameplayScaleConfig config = GetOrCreateConfig();
            EnsureSpikeDebrisPrefab();
            var entries = GetScaleEntries(config);

            Debug.Log("======================== [GAMEPLAY SCALE - BẮT ĐẦU CHUẨN HÓA] ========================");

            foreach (var e in entries)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(e.prefabPath);
                if (go == null)
                {
                    Debug.LogWarning($"[GameplayScale] Missing prefab: {e.prefabPath}");
                    continue;
                }

                SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr == null || sr.sprite == null) continue;

                Rect alphaRect = GetAlphaBoundingBox(sr.sprite, 0.1f);
                float ppu = sr.sprite.pixelsPerUnit;
                float spriteLocalVisualWidth = alphaRect.width / ppu;
                float spriteLocalVisualHeight = alphaRect.height / ppu;

                float targetWorldWidth = (e.targetPercent / 100f) * CameraRefWidth;
                float targetScale = targetWorldWidth / spriteLocalVisualWidth;

                // Cập nhật localScale
                go.transform.localScale = new Vector3(targetScale, targetScale, 1f);

                // Cập nhật Collider2D theo tỷ lệ
                Collider2D col = go.GetComponentInChildren<Collider2D>();
                if (col != null)
                {
                    if (col is CapsuleCollider2D cap)
                    {
                        cap.size = new Vector2(spriteLocalVisualWidth * e.colliderRatio, spriteLocalVisualHeight * e.colliderRatio);
                    }
                    else if (col is BoxCollider2D box)
                    {
                        box.size = new Vector2(spriteLocalVisualWidth * e.colliderRatio, spriteLocalVisualHeight * e.colliderRatio);
                    }
                    else if (col is CircleCollider2D cir)
                    {
                        cir.radius = (spriteLocalVisualWidth * 0.5f) * e.colliderRatio;
                    }
                    EditorUtility.SetDirty(col);
                }

                EditorUtility.SetDirty(go);
                PrefabUtility.SavePrefabAsset(go);
                Debug.Log($"[GameplayScale] Applied to {e.name}: scale={targetScale:F3}, targetWidth={targetWorldWidth:F3}u ({e.targetPercent}%), colliderRatio={e.colliderRatio}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("======================== [GAMEPLAY SCALE - HOÀN TẤT CHUẨN HÓA] ========================");
            GenerateScaleReport();
        }

        private static void EnsureSpikeDebrisPrefab()
        {
            string path = "Assets/_Project/Prefabs/Enemies/PF_SpikeDebris.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            GameObject go = new GameObject("PF_SpikeDebris", typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(DemHoiDenLong.Gameplay.SpikedLanternDebris));
            BoxCollider2D box = go.GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.4f, 0.4f);

            GameObject gai = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGai.prefab");
            if (gai != null)
            {
                SpriteRenderer srGai = gai.GetComponentInChildren<SpriteRenderer>();
                if (srGai != null)
                {
                    go.GetComponent<SpriteRenderer>().sprite = srGai.sprite;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[GameplayScale] Created {path}");
        }

        public static Rect GetAlphaBoundingBox(Sprite spr, float minAlpha = 0.1f)
        {
            if (spr == null || spr.texture == null) return new Rect(0, 0, 100, 100);

            string assetPath = AssetDatabase.GetAssetPath(spr.texture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            bool wasReadable = importer != null && importer.isReadable;
            if (importer != null && !wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Rect r = spr.rect;
            int xMin = Mathf.Clamp((int)r.x, 0, spr.texture.width - 1);
            int yMin = Mathf.Clamp((int)r.y, 0, spr.texture.height - 1);
            int width = Mathf.Clamp((int)r.width, 1, spr.texture.width - xMin);
            int height = Mathf.Clamp((int)r.height, 1, spr.texture.height - yMin);

            Color[] pixels = spr.texture.GetPixels(xMin, yMin, width, height);

            int bMinX = width, bMaxX = 0;
            int bMinY = height, bMaxY = 0;
            bool found = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[y * width + x].a > minAlpha)
                    {
                        found = true;
                        if (x < bMinX) bMinX = x;
                        if (x > bMaxX) bMaxX = x;
                        if (y < bMinY) bMinY = y;
                        if (y > bMaxY) bMaxY = y;
                    }
                }
            }

            if (!wasReadable && importer != null)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            if (!found) return new Rect(0, 0, width, height);
            return new Rect(bMinX, bMinY, Mathf.Max(1, bMaxX - bMinX + 1), Mathf.Max(1, bMaxY - bMinY + 1));
        }
    }
}
