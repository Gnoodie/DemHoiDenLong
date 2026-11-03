using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.EditorTools
{
    /// <summary>
    /// Công cụ đo kích thước hiển thị thật của đầu và thân 4 Lân (L, % W)
    /// và tự động cấu hình các điểm neo MouthPoint, CenterPoint cho 4 Prefabs Lân.
    /// </summary>
    public static class MeasureLanTool
    {
        public struct LanMeasureInfo
        {
            public string lanId;
            public string name;
            public string prefabPath;
            public float widthWorld;
            public float widthPercentW;
            public float headWidthWorld;
            public float bodyWidthWorld;
            public float heightWorld;
            public Vector3 mouthLocalPos;
            public Vector3 centerLocalPos;
        }

        private const float CameraRefWidth = 5.625f; // Ortho 5.0 * 2 * (9/16)
        private const float CameraRefHeight = 10.0f;

        private static readonly Dictionary<string, string> LanPrefabPaths = new Dictionary<string, string>
        {
            { "1", "Assets/_Project/Prefabs/Lans/PF_LanKim.prefab" },
            { "2", "Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab" },
            { "3", "Assets/_Project/Prefabs/Lans/PF_LanThuy.prefab" },
            { "4", "Assets/_Project/Prefabs/Lans/PF_LanVang.prefab" }
        };

        [MenuItem("DemHoiDenLong/Measure Lân (F9 Tool)")]
        public static List<LanMeasureInfo> MeasureAndSetupAllLans()
        {
            List<LanMeasureInfo> results = new List<LanMeasureInfo>();

            Debug.Log("==========================================================================================");
            Debug.Log("=== [MEASURE LÂN - GIAI ĐOẠN 5A.7] ĐO BỀ RỘNG HIỂN THỊ THẬT VÀ THIẾT LẬP ĐIỂM NEO ===");
            Debug.Log("==========================================================================================");
            Debug.Log(string.Format("{0,-12} | {1,-10} | {2,-8} | {3,-10} | {4,-10} | {5,-12} | {6,-16}",
                "Lân Name", "Scale", "L (u)", "L (% W)", "Head (u)", "Body (u)", "MouthPoint (Local)"));
            Debug.Log(new string('-', 95));

            foreach (var kvp in LanPrefabPaths)
            {
                string lanId = kvp.Key;
                string path = kvp.Value;

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                if (prefabRoot == null)
                {
                    Debug.LogError($"[MeasureLan] Không thể tải prefab: {path}");
                    continue;
                }

                try
                {
                    var pc = prefabRoot.GetComponent<PlayerController>();
                    var sr = prefabRoot.GetComponentInChildren<SpriteRenderer>();

                    float rootScale = prefabRoot.transform.localScale.x;
                    float visualWidth = 0.5625f; // default 10%
                    float headWidth = 0.5625f;
                    float bodyWidth = 0.5625f;
                    float visualHeight = 0.8f;
                    float mouthYLocal = 0.65f;

                    if (sr != null && sr.sprite != null)
                    {
                        Rect alphaRect = GameplayScaleTool.GetAlphaBoundingBox(sr.sprite, 0.1f);
                        float ppu = sr.sprite.pixelsPerUnit;
                        float spriteW = alphaRect.width / ppu;
                        float spriteH = alphaRect.height / ppu;

                        visualWidth = spriteW * rootScale;
                        visualHeight = spriteH * rootScale;

                        // Đo phần đầu (nửa trên) và thân (nửa dưới)
                        MeasureHeadAndBody(sr.sprite, alphaRect, ppu, rootScale, out headWidth, out bodyWidth, out float topAlphaLocalY);

                        // Vị trí miệng: gần đỉnh đầu phía trước
                        mouthYLocal = topAlphaLocalY;
                    }

                    float percentW = (visualWidth / CameraRefWidth) * 100f;

                    // 1. Đảm bảo CenterPoint
                    Transform centerTrans = prefabRoot.transform.Find("CenterPoint");
                    if (centerTrans == null)
                    {
                        GameObject centerObj = new GameObject("CenterPoint");
                        centerObj.transform.SetParent(prefabRoot.transform, false);
                        centerTrans = centerObj.transform;
                    }
                    centerTrans.localPosition = Vector3.zero;

                    // 2. Đảm bảo MouthPoint
                    Transform mouthTrans = prefabRoot.transform.Find("MouthPoint");
                    if (mouthTrans == null)
                    {
                        GameObject mouthObj = new GameObject("MouthPoint");
                        mouthObj.transform.SetParent(prefabRoot.transform, false);
                        mouthTrans = mouthObj.transform;
                    }
                    mouthTrans.localPosition = new Vector3(0f, mouthYLocal, 0f);

                    // 3. Serialize references trên PlayerController
                    if (pc != null)
                    {
                        var so = new SerializedObject(pc);
                        var mouthProp = so.FindProperty("mouthPoint");
                        var centerProp = so.FindProperty("centerPoint");
                        if (mouthProp != null) mouthProp.objectReferenceValue = mouthTrans;
                        if (centerProp != null) centerProp.objectReferenceValue = centerTrans;
                        so.ApplyModifiedProperties();
                    }

                    // Lưu prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);

                    LanMeasureInfo info = new LanMeasureInfo
                    {
                        lanId = lanId,
                        name = prefabRoot.name,
                        prefabPath = path,
                        widthWorld = visualWidth,
                        widthPercentW = percentW,
                        headWidthWorld = headWidth,
                        bodyWidthWorld = bodyWidth,
                        heightWorld = visualHeight,
                        mouthLocalPos = mouthTrans.localPosition,
                        centerLocalPos = centerTrans.localPosition
                    };
                    results.Add(info);

                    Debug.Log(string.Format("{0,-12} | {1,-10:F3} | {2,-8:F3} | {3,-9:F1}% | {4,-10:F3} | {5,-12:F3} | {6,-16}",
                        info.name, rootScale, info.widthWorld, info.widthPercentW, info.headWidthWorld, info.bodyWidthWorld,
                        $"({info.mouthLocalPos.x:F2}, {info.mouthLocalPos.y:F2}, {info.mouthLocalPos.z:F2})"));
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }

            Debug.Log(new string('=', 95));
            return results;
        }

        public static void MeasureAndSetupBatchmode()
        {
            try
            {
                MeasureAndSetupAllLans();
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MeasureLan Error]: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static void MeasureHeadAndBody(Sprite sprite, Rect fullRect, float ppu, float rootScale,
            out float headWidth, out float bodyWidth, out float topAlphaLocalY)
        {
            headWidth = fullRect.width / ppu * rootScale;
            bodyWidth = headWidth;

            Texture2D tex = sprite.texture;
            if (!tex.isReadable)
            {
                // Fallback nếu không read được
                topAlphaLocalY = 0.65f;
                return;
            }

            Rect textureRect = sprite.textureRect;
            int xMin = (int)fullRect.xMin;
            int xMax = (int)fullRect.xMax;
            int yMin = (int)fullRect.yMin;
            int yMax = (int)fullRect.yMax;
            int midY = (yMin + yMax) / 2;

            int headMinX = xMax, headMaxX = xMin;
            int bodyMinX = xMax, bodyMaxX = xMin;
            int highestY = yMin;

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    Color pixel = tex.GetPixel((int)textureRect.x + x, (int)textureRect.y + y);
                    if (pixel.a > 0.1f)
                    {
                        if (y > highestY) highestY = y;

                        if (y >= midY)
                        {
                            if (x < headMinX) headMinX = x;
                            if (x > headMaxX) headMaxX = x;
                        }
                        else
                        {
                            if (x < bodyMinX) bodyMinX = x;
                            if (x > bodyMaxX) bodyMaxX = x;
                        }
                    }
                }
            }

            float pivotPixelY = sprite.pivot.y;
            topAlphaLocalY = (highestY - pivotPixelY) / ppu;

            if (headMaxX >= headMinX)
                headWidth = ((headMaxX - headMinX + 1) / ppu) * rootScale;
            if (bodyMaxX >= bodyMinX)
                bodyWidth = ((bodyMaxX - bodyMinX + 1) / ppu) * rootScale;
        }

        private static Rect GetAlphaBoundingBox(Sprite sprite, float alphaThreshold)
        {
            Texture2D tex = sprite.texture;
            if (!tex.isReadable)
            {
                return sprite.rect;
            }

            Rect tr = sprite.textureRect;
            int minX = (int)tr.width;
            int maxX = 0;
            int minY = (int)tr.height;
            int maxY = 0;

            for (int y = 0; y < (int)tr.height; y++)
            {
                for (int x = 0; x < (int)tr.width; x++)
                {
                    Color c = tex.GetPixel((int)tr.x + x, (int)tr.y + y);
                    if (c.a > alphaThreshold)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (minX > maxX || minY > maxY)
            {
                return new Rect(0, 0, tr.width, tr.height);
            }

            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }
}
