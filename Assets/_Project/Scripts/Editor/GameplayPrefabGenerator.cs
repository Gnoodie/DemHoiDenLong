using UnityEngine;
using UnityEditor;
using System.IO;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public class GameplayPrefabGenerator : EditorWindow
    {
        [MenuItem("Tools/Dem Hoi Den Long/Generate Bullet & Enemy Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<GameplayPrefabGenerator>("Generate Gameplay Prefabs");
        }

        private void OnGUI()
        {
            GUILayout.Label("Tạo Bullet & Enemy Prefabs (Đầy Đủ 100% Art)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ này sinh toàn bộ Prefabs sử dụng đúng hình ảnh thiết kế:\n\n" +
                "1. ĐẠN LÂN (Player Bullets):\n" +
                "   - PF_Bullet_Kim (Kim Lân - bullet_player1.png)\n" +
                "   - PF_Bullet_Hoa (Hỏa Lân - bullet_player2.png)\n" +
                "   - PF_Bullet_Thuy (Thủy Lân - bullet_player3.png)\n" +
                "   - PF_Bullet_Vang (Lân Vàng - bullet_player4.png)\n\n" +
                "2. ĐẠN QUÁI & BOSS (Enemy Bullets):\n" +
                "   - PF_EnemyBullet_Normal (Đèn ma/giấy - bullet_enemy1.png)\n" +
                "   - PF_EnemyBullet_Special (Đèn gai/cá chép - bullet_enemy2.png)\n" +
                "   - PF_EnemyBullet_Boss (Boss 3 Act - bullet_enemy3.png)\n\n" +
                "3. KẺ ĐỊCH (4 Loại Quái Đèn Tàn):\n" +
                "   - PF_Enemy_DenGiay (Đèn Giấy - den_giay.png)\n" +
                "   - PF_Enemy_DenGai (Đèn Gai - den_gai.png)\n" +
                "   - PF_Enemy_DenCa (Đèn Cá Chép - den_ca.png)\n" +
                "   - PF_Enemy_DenQuy (Đèn Quỷ - den_quy.png)",
                MessageType.Info);

            if (GUILayout.Button("Tạo Tất Cả Bullet & Enemy Prefabs Ngay", GUILayout.Height(40)))
            {
                GenerateGameplayPrefabs();
            }
        }

        public static void GenerateGameplayPrefabs()
        {
            string bulletsFolder = "Assets/_Project/Prefabs/Bullets";
            string enemiesFolder = "Assets/_Project/Prefabs/Enemies";
            string bulletsArtFolder = "Assets/_Project/Art/Bullets";
            string enemiesArtFolder = "Assets/_Project/Art/Enemies";

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");

            if (!AssetDatabase.IsValidFolder(bulletsFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Bullets");

            if (!AssetDatabase.IsValidFolder(enemiesFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Enemies");

            // Ensure sprite import settings
            EnsureSpriteImportSettings(bulletsArtFolder);
            EnsureSpriteImportSettings(enemiesArtFolder);

            // 1. Tạo đạn 4 Lân
            CreateBulletPrefab(bulletsFolder, "PF_Bullet_Kim", $"{bulletsArtFolder}/bullet_player1.png", true);
            CreateBulletPrefab(bulletsFolder, "PF_Bullet_Hoa", $"{bulletsArtFolder}/bullet_player2.png", true);
            CreateBulletPrefab(bulletsFolder, "PF_Bullet_Thuy", $"{bulletsArtFolder}/bullet_player3.png", true);
            CreateBulletPrefab(bulletsFolder, "PF_Bullet_Vang", $"{bulletsArtFolder}/bullet_player4.png", true);

            // 2. Tạo đạn Quái & Boss
            CreateBulletPrefab(bulletsFolder, "PF_EnemyBullet_Normal", $"{bulletsArtFolder}/bullet_enemy1.png", false);
            CreateBulletPrefab(bulletsFolder, "PF_EnemyBullet_Special", $"{bulletsArtFolder}/bullet_enemy2.png", false);
            CreateBulletPrefab(bulletsFolder, "PF_EnemyBullet_Boss", $"{bulletsArtFolder}/bullet_enemy3.png", false);

            // 3. Tạo 4 Loại Quái Đèn Tàn với đúng hình ảnh
            CreateEnemyPrefab<PaperLanternEnemy>(enemiesFolder, "PF_Enemy_DenGiay", $"{enemiesArtFolder}/den_giay.png");
            CreateEnemyPrefab<PaperLanternEnemy>(enemiesFolder, "PF_Enemy_DenGai", $"{enemiesArtFolder}/den_gai.png");
            CreateEnemyPrefab<PaperLanternEnemy>(enemiesFolder, "PF_Enemy_DenCa", $"{enemiesArtFolder}/den_ca.png");
            CreateEnemyPrefab<PaperLanternEnemy>(enemiesFolder, "PF_Enemy_DenQuy", $"{enemiesArtFolder}/den_quy.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Hoàn tất",
                "✅ Đã tạo thành công bộ Prefab 7 loại đạn & 4 loại quái chuẩn 100% Art!\n\n" +
                $"Đạn: {bulletsFolder}/\nQuái: {enemiesFolder}/", "OK");
        }

        private static void CreateBulletPrefab(string folder, string prefabName, string spritePath, bool isPlayer)
        {
            GameObject root = new GameObject(prefabName);
            root.transform.localScale = new Vector3(0.08f, 0.08f, 1f);

            Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            CapsuleCollider2D col = root.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.3f, 0.6f);
            col.direction = CapsuleDirection2D.Vertical;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            if (bulletSprite != null)
            {
                sr.sprite = bulletSprite;
            }
            sr.color = Color.white;

            Bullet bulletComp = root.AddComponent<Bullet>();

            SerializedObject so = new SerializedObject(bulletComp);
            so.FindProperty("isPlayerBullet").boolValue = isPlayer;
            so.ApplyModifiedProperties();

            string savePath = $"{folder}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, savePath);
            DestroyImmediate(root);
        }

        private static void CreateEnemyPrefab<T>(string folder, string prefabName, string spritePath) where T : EnemyBase
        {
            GameObject root = new GameObject(prefabName);
            root.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            CapsuleCollider2D col = root.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 1.0f);
            col.direction = CapsuleDirection2D.Vertical;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();

            Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (enemySprite != null)
            {
                sr.sprite = enemySprite;
            }
            sr.color = Color.white;

            root.AddComponent<T>();

            string savePath = $"{folder}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, savePath);
            DestroyImmediate(root);
        }

        private static void EnsureSpriteImportSettings(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;
            string[] files = Directory.GetFiles(folderPath, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
