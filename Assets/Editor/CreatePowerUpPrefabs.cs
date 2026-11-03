using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.EditorScripts
{
    public class CreatePowerUpPrefabs : MonoBehaviour
    {
        [MenuItem("DemHoiDenLong/Create PowerUp Prefabs")]
        public static void CreatePrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Items"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Items");
            }

            CreatePrefab("Star", ItemType.Star, 10f, null, 0.5f);
            
            // Load sprites
            Sprite sprHeal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/PowerUps/Power_ups1.png");
            Sprite sprShield = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/PowerUps/Power_ups2.png");
            Sprite sprMagnet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/PowerUps/Power_ups3.png");
            Sprite sprStun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/PowerUps/Power_ups4.png");

            CreatePrefab("Heal", ItemType.Heal, 20f, sprHeal, 1.5f);
            CreatePrefab("Shield", ItemType.Shield, 1f, sprShield, 1.5f);
            CreatePrefab("Magnet", ItemType.Magnet, 10f, sprMagnet, 1.5f);
            CreatePrefab("Stun", ItemType.Stun, 3f, sprStun, 1.5f);
            
            // Auto update PowerUpManager if it exists in the active scene
            PowerUpManager mgr = Object.FindAnyObjectByType<PowerUpManager>();
            if (mgr != null)
            {
                SerializedObject so = new SerializedObject(mgr);
                
                so.FindProperty("starPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Star.prefab");
                
                SerializedProperty arr = so.FindProperty("powerUpPrefabs");
                arr.ClearArray();
                arr.InsertArrayElementAtIndex(0);
                arr.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Heal.prefab");
                arr.InsertArrayElementAtIndex(1);
                arr.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Shield.prefab");
                arr.InsertArrayElementAtIndex(2);
                arr.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Magnet.prefab");
                arr.InsertArrayElementAtIndex(3);
                arr.GetArrayElementAtIndex(3).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Stun.prefab");
                
                so.ApplyModifiedProperties();
                Debug.Log("Updated PowerUpManager in scene.");
            }
            else
            {
                // Create a prefab for PowerUpManager
                GameObject mgrObj = new GameObject("PowerUpManager");
                PowerUpManager puMgr = mgrObj.AddComponent<PowerUpManager>();
                
                SerializedObject so = new SerializedObject(puMgr);
                so.FindProperty("starPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Star.prefab");
                SerializedProperty arr = so.FindProperty("powerUpPrefabs");
                arr.ClearArray();
                arr.InsertArrayElementAtIndex(0);
                arr.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Heal.prefab");
                arr.InsertArrayElementAtIndex(1);
                arr.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Shield.prefab");
                arr.InsertArrayElementAtIndex(2);
                arr.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Magnet.prefab");
                arr.InsertArrayElementAtIndex(3);
                arr.GetArrayElementAtIndex(3).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Items/PF_Item_Stun.prefab");
                so.ApplyModifiedProperties();
                
                PrefabUtility.SaveAsPrefabAsset(mgrObj, "Assets/_Project/Prefabs/Items/PF_PowerUpManager.prefab");
                DestroyImmediate(mgrObj);
                Debug.Log("Created PF_PowerUpManager prefab.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("PowerUp prefabs created successfully.");
        }

        private static void CreatePrefab(string name, ItemType type, float value, Sprite sprite, float scale)
        {
            string path = $"Assets/_Project/Prefabs/Items/PF_Item_{name}.prefab";
            GameObject obj = new GameObject($"PF_Item_{name}");
            
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sr.sprite = sprite;
            }
            else if (type == ItemType.Star)
            {
                // Assign a generic sprite programmatically? Unity's default UI sprite
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                sr.color = Color.yellow;
            }
            
            obj.transform.localScale = new Vector3(scale, scale, 1f);

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            CollectibleItem item = obj.AddComponent<CollectibleItem>();
            item.Initialize(type, value); // Will be serialized via fields
            
            SerializedObject so = new SerializedObject(item);
            so.FindProperty("itemType").enumValueIndex = (int)type;
            so.FindProperty("value").floatValue = value;
            so.FindProperty("fallSpeed").floatValue = 3f;
            so.FindProperty("magnetSpeed").floatValue = 15f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);
        }
    }
}
