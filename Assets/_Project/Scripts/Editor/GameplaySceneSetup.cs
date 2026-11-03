using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DemHoiDenLong.Gameplay;
using System.IO;

namespace DemHoiDenLong.Editor
{
    public class GameplaySceneSetup
    {
        private const string BackgroundSourcePath = "D:/Dev_Game/back_act1.png";
        private const string BackgroundDestDir = "Assets/_Project/Art/Backgrounds";
        private const string BackgroundDestPath = "Assets/_Project/Art/Backgrounds/back_act1.png";
        
        [MenuItem("DemHoiDenLong/Setup Gameplay Scene")]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Gameplay Scene", 
                "This will create a new scene named 'Gameplay' and overwrite any unsaved changes in your current scene. Continue?", 
                "Yes", "No"))
            {
                return;
            }

            // 1. Create a new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "Gameplay";

            // 2. Setup Camera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            ColorUtility.TryParseHtmlString("#0a1029", out Color bgColor);
            cam.backgroundColor = bgColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraObj.transform.position = new Vector3(0, 0, -10f);

            // 3. Setup Background Image
            SetupBackgroundImage(cameraObj.transform);

            // 4. Setup Player
            GameObject playerObj = null;
            PlayerController playerController = null;
            string playerPrefabPath = "Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab";
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            if (playerPrefab != null)
            {
                playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerObj.name = "Player_LanHoa";
                playerObj.transform.position = new Vector3(0, -3.5f, 0); // Position near bottom
                playerController = playerObj.GetComponent<PlayerController>();
                
                if (playerController != null)
                {
                    SerializedObject soPlayer = new SerializedObject(playerController);
                    soPlayer.FindProperty("mainCamera").objectReferenceValue = cam;
                    soPlayer.ApplyModifiedProperties();
                }
            }
            else
            {
                Debug.LogError($"Could not find Player prefab at {playerPrefabPath}");
            }

            // 5. Setup Bullet Pool
            GameObject bulletPoolObj = new GameObject("BulletPool");
            BulletPool bulletPool = bulletPoolObj.AddComponent<BulletPool>();
            string bulletPrefabPath = "Assets/_Project/Prefabs/Bullets/PF_Bullet_Hoa.prefab";
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
            
            SerializedObject soBulletPool = new SerializedObject(bulletPool);
            soBulletPool.FindProperty("mainCamera").objectReferenceValue = cam;
            if (bulletPrefab != null)
            {
                soBulletPool.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
            }
            soBulletPool.ApplyModifiedProperties();

            // 6. Setup Enemy Spawner
            GameObject spawnerObj = new GameObject("EnemySpawner");
            EnemySpawner enemySpawner = spawnerObj.AddComponent<EnemySpawner>();
            string enemyPrefabPath = "Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGiay.prefab";
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
            
            SerializedObject soSpawner = new SerializedObject(enemySpawner);
            soSpawner.FindProperty("mainCamera").objectReferenceValue = cam;
            if (enemyPrefab != null)
            {
                soSpawner.FindProperty("defaultEnemyPrefab").objectReferenceValue = enemyPrefab;
            }
            soSpawner.ApplyModifiedProperties();

            // 7. Setup GameManager
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            
            if (playerController != null)
            {
                SerializedObject soGM = new SerializedObject(gm);
                soGM.FindProperty("player").objectReferenceValue = playerController;
                soGM.ApplyModifiedProperties();
            }

            // 8. Save the Scene
            string sceneDir = "Assets/_Project/Scenes";
            if (!Directory.Exists(sceneDir))
            {
                Directory.CreateDirectory(sceneDir);
            }
            string scenePath = sceneDir + "/Gameplay.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"Successfully generated Gameplay Scene at {scenePath}");
        }

        private static void SetupBackgroundImage(Transform cameraTransform)
        {
            // Copy background image if it exists in D:\Dev_Game
            if (File.Exists(BackgroundSourcePath))
            {
                if (!Directory.Exists(BackgroundDestDir))
                {
                    Directory.CreateDirectory(BackgroundDestDir);
                }

                if (!File.Exists(BackgroundDestPath))
                {
                    File.Copy(BackgroundSourcePath, BackgroundDestPath);
                    AssetDatabase.Refresh();
                }

                TextureImporter importer = AssetImporter.GetAtPath(BackgroundDestPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100f;
                    importer.SaveAndReimport();
                }

                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundDestPath);
                if (bgSprite != null)
                {
                    GameObject bgObj = new GameObject("Background");
                    SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
                    sr.sprite = bgSprite;
                    sr.sortingOrder = -100; // Push background behind everything
                    
                    // Optional: adjust scale to fit screen roughly (can be tweaked later)
                    float camHeight = 5f * 2f; // orthographicSize * 2
                    float camWidth = camHeight * (9f / 16f); // Assume vertical ratio for mobile
                    
                    if (bgSprite.bounds.size.x > 0 && bgSprite.bounds.size.y > 0)
                    {
                        float scaleY = camHeight / bgSprite.bounds.size.y;
                        float scaleX = camWidth / bgSprite.bounds.size.x;
                        float maxScale = Mathf.Max(scaleX, scaleY);
                        bgObj.transform.localScale = new Vector3(maxScale, maxScale, 1f);
                    }
                    
                    // Attach to camera so it moves with it if we ever move the camera
                    bgObj.transform.SetParent(cameraTransform);
                    bgObj.transform.localPosition = new Vector3(0, 0, 10f); // Push forward in front of camera
                }
            }
            else
            {
                Debug.LogWarning($"Background image not found at {BackgroundSourcePath}. Using solid color.");
            }
        }
    }
}
