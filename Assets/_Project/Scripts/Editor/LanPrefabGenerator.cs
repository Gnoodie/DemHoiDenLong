using UnityEngine;
using UnityEditor;
using System.IO;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.EditorTools
{
    public class LanPrefabGenerator : EditorWindow
    {
        [MenuItem("Tools/Dem Hoi Den Long/Generate Lan Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<LanPrefabGenerator>("Generate Lan Prefabs");
        }

        private void OnGUI()
        {
            GUILayout.Label("Tạo 4 Prefab Lân Tự Động", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ này sẽ tự động:\n" +
                "1. Chuyển đổi các file ảnh PNG Lân sang định dạng Sprite (2D and UI).\n" +
                "2. Tạo 4 Prefab Lân (PF_LanKim, PF_LanHoa, PF_LanThuy, PF_LanVang) với đầy đủ SpriteRenderer, Collider2D, Rigidbody2D và PlayerController.\n" +
                "3. Gán vị trí nòng bắn FirePoint chuẩn cho mỗi Lân.\n" +
                "4. Lưu các Prefab vào thư mục Assets/_Project/Prefabs/Lans/",
                MessageType.Info);

            if (GUILayout.Button("Tạo 4 Prefab Lân Ngay", GUILayout.Height(40)))
            {
                GenerateAllLanPrefabs();
            }
        }

        public static void GenerateAllLanPrefabs()
        {
            string outputFolder = "Assets/_Project/Prefabs/Lans";
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            if (!AssetDatabase.IsValidFolder(outputFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Lans");

            LanInfo[] lans = new LanInfo[]
            {
                new LanInfo { id = "1", prefabName = "PF_LanKim", artFolderPath = "Assets/_Project/Art/Characters/Lans/LanKim", mainSpriteName = "topview_lankim.png" },
                new LanInfo { id = "2", prefabName = "PF_LanHoa", artFolderPath = "Assets/_Project/Art/Characters/Lans/LanHoa", mainSpriteName = "topview_lanhoa.png" },
                new LanInfo { id = "3", prefabName = "PF_LanThuy", artFolderPath = "Assets/_Project/Art/Characters/Lans/LanThuy", mainSpriteName = "topview_lanthuy.png" },
                new LanInfo { id = "4", prefabName = "PF_LanVang", artFolderPath = "Assets/_Project/Art/Characters/Lans/LanVang", mainSpriteName = "topview_lanvang.png" }
            };

            int count = 0;

            foreach (var lan in lans)
            {
                // Ensure sprite import settings for all PNGs in folder
                EnsureSpriteImportSettings(lan.artFolderPath);

                string mainSpritePath = $"{lan.artFolderPath}/{lan.mainSpriteName}";
                Sprite mainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mainSpritePath);
                if (mainSprite == null)
                {
                    Debug.LogWarning($"[LanPrefabGenerator] Không tìm thấy sprite tại: {mainSpritePath}");
                    continue;
                }

                // Create Root GameObject
                GameObject root = new GameObject(lan.prefabName);
                root.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

                // Add Rigidbody2D (Kinematic for 2D Shmup)
                Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.useFullKinematicContacts = true;

                // Add CapsuleCollider2D
                CapsuleCollider2D col = root.AddComponent<CapsuleCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 1.0f);
                col.direction = CapsuleDirection2D.Vertical;

                // Create Visual Child
                GameObject visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = mainSprite;

                // Create FirePoint Child (position above character nose)
                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(root.transform, false);
                firePoint.transform.localPosition = new Vector3(0f, 0.7f, 0f);

                // Add PlayerController
                PlayerController pc = root.AddComponent<PlayerController>();

                // Auto-assign to LanData asset if exists
                string dataPath = $"Assets/_Project/Data/Lans/{lan.id}.asset";
                DemHoiDenLong.Data.LanData lanData = AssetDatabase.LoadAssetAtPath<DemHoiDenLong.Data.LanData>(dataPath);

                // Assign Serialized Properties
                SerializedObject so = new SerializedObject(pc);
                so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
                if (lanData != null)
                {
                    so.FindProperty("lanData").objectReferenceValue = lanData;
                }
                so.FindProperty("lanConfig").objectReferenceValue = null;
                so.ApplyModifiedProperties();

                // Save Prefab
                string savePath = $"{outputFolder}/{lan.prefabName}.prefab";
                GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, savePath);

                if (lanData != null)
                {
                    lanData.prefab = prefabAsset;
                    EditorUtility.SetDirty(lanData);
                }

                // Destroy temp scene object
                DestroyImmediate(root);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Hoàn tất", $"✅ Đã tạo thành công {count}/4 Prefab Lân tại:\n{outputFolder}", "OK");
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

        private struct LanInfo
        {
            public string id;
            public string prefabName;
            public string artFolderPath;
            public string mainSpriteName;
        }
    }
}
