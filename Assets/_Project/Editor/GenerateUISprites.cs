using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateUISprites
{
    private const string OUTPUT_FOLDER = "Assets/_Project/Sprites/UI";

    [MenuItem("DemHoiDenLong/Generate UI Sprites")]
    public static void GenerateAllSprites()
    {
        string fullFolderPath = Path.Combine(Application.dataPath, "_Project/Sprites/UI");
        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
            AssetDatabase.Refresh();
        }

        Debug.Log("--- Bat dau sinh UI Sprites ---");

        // 1. ui_round_36.png: 128x128, bo goc ban kinh 36, border 40
        CreateAndSaveSprite("ui_round_36.png", 128, 128, new Vector4(40, 40, 40, 40), (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f - 64f, y + 0.5f - 64f);
            Vector2 q = new Vector2(Mathf.Abs(p.x) - 28f, Mathf.Abs(p.y) - 28f);
            float dist = (q.x > 0 && q.y > 0) ? Mathf.Sqrt(q.x * q.x + q.y * q.y) - 36f : Mathf.Max(q.x, q.y) - 36f;
            float alpha = Mathf.Clamp01(0.5f - dist);
            return new Color(1f, 1f, 1f, alpha);
        });

        // 2. ui_round_top_36.png: 128x128, chi bo 2 goc tren, border 40 (L:40, B:0, R:40, T:40)
        CreateAndSaveSprite("ui_round_top_36.png", 128, 128, new Vector4(40, 0, 40, 40), (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f - 64f, y + 0.5f - 64f);
            float dist;
            if (p.y >= 0)
            {
                Vector2 q = new Vector2(Mathf.Abs(p.x) - 28f, p.y - 28f);
                dist = (q.x > 0 && q.y > 0) ? Mathf.Sqrt(q.x * q.x + q.y * q.y) - 36f : Mathf.Max(q.x, q.y) - 36f;
            }
            else
            {
                dist = Mathf.Max(Mathf.Abs(p.x) - 64f, -p.y - 64f);
            }
            float alpha = Mathf.Clamp01(0.5f - dist);
            return new Color(1f, 1f, 1f, alpha);
        });

        // 3. ui_round_bottom_36.png: 128x128, chi bo 2 goc duoi, border 40 (L:40, B:40, R:40, T:0)
        CreateAndSaveSprite("ui_round_bottom_36.png", 128, 128, new Vector4(40, 40, 40, 0), (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f - 64f, y + 0.5f - 64f);
            float dist;
            if (p.y <= 0)
            {
                Vector2 q = new Vector2(Mathf.Abs(p.x) - 28f, -p.y - 28f);
                dist = (q.x > 0 && q.y > 0) ? Mathf.Sqrt(q.x * q.x + q.y * q.y) - 36f : Mathf.Max(q.x, q.y) - 36f;
            }
            else
            {
                dist = Mathf.Max(Mathf.Abs(p.x) - 64f, p.y - 64f);
            }
            float alpha = Mathf.Clamp01(0.5f - dist);
            return new Color(1f, 1f, 1f, alpha);
        });

        // 4. ui_pill.png: 128x64, bo tron hoan toan 2 dau, border 32
        CreateAndSaveSprite("ui_pill.png", 128, 64, new Vector4(32, 32, 32, 32), (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f - 64f, y + 0.5f - 32f);
            float px = Mathf.Max(0f, Mathf.Abs(p.x) - 32f);
            float dist = Mathf.Sqrt(px * px + p.y * p.y) - 32f;
            float alpha = Mathf.Clamp01(0.5f - dist);
            return new Color(1f, 1f, 1f, alpha);
        });

        // 5. ui_circle.png: 128x128, hinh tron dac
        CreateAndSaveSprite("ui_circle.png", 128, 128, Vector4.zero, (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f - 64f, y + 0.5f - 64f);
            float dist = Mathf.Sqrt(p.x * p.x + p.y * p.y) - 64f;
            float alpha = Mathf.Clamp01(0.5f - dist);
            return new Color(1f, 1f, 1f, alpha);
        });

        // 6. ui_gradient_fade.png: 4x256, doc: tren den alpha 0, duoi den alpha 255 (smoothstep)
        // GIU NGUYEN MAU DEN (0, 0, 0) cho ActCard de do bong chan banner ro chu
        CreateAndSaveSprite("ui_gradient_fade.png", 4, 256, Vector4.zero, (x, y) =>
        {
            float t = (float)y / 255f; // 0 o duoi, 1 o tren
            float smooth = t * t * (3f - 2f * t);
            float alpha = 1f - smooth; // duoi = 1, tren = 0
            return new Color(0f, 0f, 0f, alpha);
        });

        // 6b. ui_paper_gradient.png: 4x256, trang tinh de tint mau tuy y (nhu mau giay #E8D9B0)
        CreateAndSaveSprite("ui_paper_gradient.png", 4, 256, Vector4.zero, (x, y) =>
        {
            float t = (float)y / 255f;
            float smooth = t * t * (3f - 2f * t);
            float alpha = 1f - smooth;
            return new Color(1f, 1f, 1f, alpha);
        });

        // 7. ui_glow_ring.png: 128x128, vien bo goc 36 day ~6px, mo mem ra ngoai ~10px, giua trong suot, border 48
        CreateAndSaveSprite("ui_glow_ring.png", 128, 128, new Vector4(48, 48, 48, 48), (x, y) =>
        {
            // Duong tam cua vien bo goc dat tai half = 50, ban kinh r = 36 (tam goc bo = 14)
            Vector2 p = new Vector2(x + 0.5f - 64f, y + 0.5f - 64f);
            Vector2 q = new Vector2(Mathf.Abs(p.x) - 14f, Mathf.Abs(p.y) - 14f);
            float d = (q.x > 0 && q.y > 0) ? Mathf.Sqrt(q.x * q.x + q.y * q.y) - 36f : Mathf.Max(q.x, q.y) - 36f;

            float alpha = 0f;
            if (d > 0f)
            {
                // Mo mem ra ngoai 10px
                if (d < 10f)
                {
                    float t = d / 10f;
                    alpha = Mathf.SmoothStep(1f, 0f, t);
                }
            }
            else
            {
                // Day ~6px (tu 0 den -3 dac, tu -3 den -8 giam ve 0)
                if (d >= -3f)
                {
                    alpha = 1f;
                }
                else if (d >= -8f)
                {
                    float t = (d - (-8f)) / 5f;
                    alpha = Mathf.SmoothStep(0f, 1f, t);
                }
            }

            return new Color(1f, 1f, 1f, alpha);
        });

        // 8. ui_bg_gradient.png: 4x256, chieu doc: tren #0E1A3A, duoi #1B1747
        Color32 colorTop = new Color32(0x0E, 0x1A, 0x3A, 255);
        Color32 colorBottom = new Color32(0x1B, 0x17, 0x47, 255);
        CreateAndSaveSprite("ui_bg_gradient.png", 4, 256, Vector4.zero, (x, y) =>
        {
            float t = (float)y / 255f; // y=0: bottom, y=255: top
            return Color.Lerp(colorBottom, colorTop, t);
        });

        // 9. ui_check.png: 128x128, dau tick trang muot bo tron net day
        CreateAndSaveSprite("ui_check.png", 128, 128, Vector4.zero, (x, y) =>
        {
            Vector2 p = new Vector2(x, y);
            Vector2 a = new Vector2(32f, 66f);
            Vector2 b = new Vector2(54f, 40f);
            Vector2 c = new Vector2(98f, 92f);
            float d1 = DistToSegment(p, a, b);
            float d2 = DistToSegment(p, b, c);
            float dist = Mathf.Min(d1, d2) - 8.5f;
            float alpha = Mathf.Clamp01(0.5f - dist);
            return new Color(1f, 1f, 1f, alpha);
        });

        // 10. ui_play.png: 128x128, tam giac bo goc nhe huong sang phai
        CreateAndSaveSprite("ui_play.png", 128, 128, Vector4.zero, (x, y) =>
        {
            Vector2 p = new Vector2(x, y);
            Vector2 a = new Vector2(46f, 88f);
            Vector2 b = new Vector2(46f, 40f);
            Vector2 c = new Vector2(88f, 64f);
            bool inside = (p.x >= 46f) &&
                          (p.y >= 40f + (24f / 42f) * (p.x - 46f)) &&
                          (p.y <= 64f - (24f / 42f) * (p.x - 88f));
            float distToEdges = Mathf.Min(DistToSegment(p, a, b), Mathf.Min(DistToSegment(p, b, c), DistToSegment(p, c, a)));
            float dist = inside ? 0f : distToEdges;
            float alpha = Mathf.Clamp01(0.5f - (dist - 9f));
            return new Color(1f, 1f, 1f, alpha);
        });

        // 11. ui_lock.png: 128x128, o khoa trang muot anti-aliased
        CreateAndSaveSprite("ui_lock.png", 128, 128, Vector4.zero, (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
            
            // Shackle
            float shackleDist;
            if (p.y >= 78f)
            {
                shackleDist = Mathf.Abs(Vector2.Distance(p, new Vector2(64f, 78f)) - 20f) - 6f;
            }
            else
            {
                float legX = (p.x <= 64f) ? 44f : 84f;
                shackleDist = DistToSegment(p, new Vector2(legX, 78f), new Vector2(legX, 52f)) - 6f;
            }

            // Body: rounded box center (64, 34), half-extents (21, 13), radius 9
            Vector2 b = new Vector2(Mathf.Abs(p.x - 64f) - 21f, Mathf.Abs(p.y - 34f) - 13f);
            float bodyDist = (b.x > 0f && b.y > 0f) ? Mathf.Sqrt(b.x * b.x + b.y * b.y) - 9f : Mathf.Max(b.x, b.y) - 9f;

            float lockDist = Mathf.Min(shackleDist, bodyDist);
            float alpha = Mathf.Clamp01(0.5f - lockDist);

            // Keyhole cutout
            float dCircle = Vector2.Distance(p, new Vector2(64f, 38f)) - 5f;
            float dSlot = DistToSegment(p, new Vector2(64f, 38f), new Vector2(64f, 26f)) - 3f;
            float keyDist = Mathf.Min(dCircle, dSlot);
            float keyCut = Mathf.Clamp01(0.5f - keyDist);

            alpha *= (1f - keyCut);
            return new Color(1f, 1f, 1f, alpha);
        });

        AssetDatabase.Refresh();
        Debug.Log("--- Sinh UI Sprites hoan tat! ---");
    }

    private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 pa = p - a, ba = b - a;
        float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
        return (pa - ba * h).magnitude;
    }

    private static void CreateAndSaveSprite(string fileName, int width, int height, Vector4 border, System.Func<int, int, Color> colorFunc)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, colorFunc(x, y));
            }
        }
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string relativePath = Path.Combine(OUTPUT_FOLDER, fileName).Replace("\\", "/");
        string fullPath = Path.Combine(Application.dataPath, relativePath.Substring("Assets/".Length));
        File.WriteAllBytes(fullPath, pngData);

        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spriteBorder = border;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log($"Generated UI Sprite: {relativePath} (Size: {width}x{height}, Border: {border})");
    }
}
