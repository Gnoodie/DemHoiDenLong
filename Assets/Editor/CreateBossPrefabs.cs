using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Gameplay;

public class CreateBossPrefabs : MonoBehaviour
{
    [MenuItem("DemHoiDenLong/Create Boss Prefabs")]
    public static void CreatePrefabs()
    {
        string[] bossNames = { "BossAct1", "BossAct2", "BossAct3" };
        string[] spritePaths = { "Assets/_Project/Art/Bosses/boss_act1.png", "Assets/_Project/Art/Bosses/boss_act2.png", "Assets/_Project/Art/Bosses/boss_act3.png" };
        System.Type[] bossTypes = { typeof(BossAct1), typeof(BossAct2), typeof(BossAct3) };
        
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Bosses"))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Bosses");
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject bossObj = new GameObject("PF_Boss_" + bossNames[i]);
            
            // Add Boss Component to parent
            Component bossComp = bossObj.AddComponent(bossTypes[i]);
            
            // Add physics components to PARENT (where Boss script is)
            Rigidbody2D rb = bossObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // Prevent sleeping causing missed collisions
            rb.useFullKinematicContacts = true; // Enable kinematic-kinematic collision detection
            
            CapsuleCollider2D col = bossObj.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1.5f); // Adjust collider size
            
            // Add SpriteRenderer as child
            GameObject spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(bossObj.transform);
            spriteObj.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
            
            Sprite bossSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
            if (bossSprite != null)
            {
                sr.sprite = bossSprite;
            }
            else
            {
                Debug.LogWarning("Could not load sprite: " + spritePaths[i]);
            }
            
            // Adjust scale
            bossObj.transform.localScale = Vector3.one * 0.5f; // Bosses are larger
            
            // Save as Prefab
            string prefabPath = "Assets/_Project/Prefabs/Bosses/PF_" + bossNames[i] + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(bossObj, prefabPath);
            
            // Cleanup
            DestroyImmediate(bossObj);
            
            Debug.Log("Created Boss Prefab: " + prefabPath);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
