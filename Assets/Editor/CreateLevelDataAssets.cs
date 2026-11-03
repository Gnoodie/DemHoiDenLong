using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using System.Collections.Generic;

public class CreateLevelDataAssets : MonoBehaviour
{
    [MenuItem("DemHoiDenLong/Create Level Data Assets")]
    public static void CreateLevelAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Levels"))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Data", "Levels");
        }

        GameObject boss1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Bosses/PF_BossAct1.prefab");
        GameObject boss2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Bosses/PF_BossAct2.prefab");
        GameObject boss3 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Bosses/PF_BossAct3.prefab");

        // Load enemy assets
        EnemyData enemyPaper = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/SO_DenGiay.asset");
        EnemyData enemyFish = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/SO_DenCa.asset");
        EnemyData enemySpike = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/SO_DenGai.asset");
        EnemyData enemyDemon = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/SO_DenQuy.asset");

        if (enemyPaper == null) Debug.LogWarning("Missing SO_DenGiay.asset!");

        // Create 20 levels
        for (int i = 1; i <= 20; i++)
        {
            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
            
            // Configure level difficulty
            levelData.baseHpMultiplier = 1.0f + (i - 1) * 0.15f;
            levelData.baseSpeedMultiplier = 1.0f + (i - 1) * 0.1f;
            levelData.baseDamageMultiplier = 1.0f + (i - 1) * 0.12f;
            levelData.bossSpawnDelay = 3.0f;
            
            // Assign bosses to specific levels
            if (i == 5 && boss1 != null) levelData.bossPrefab = boss1.GetComponent<DemHoiDenLong.Gameplay.BossController>();
            else if (i == 10 && boss2 != null) levelData.bossPrefab = boss2.GetComponent<DemHoiDenLong.Gameplay.BossController>();
            else if (i == 15 && boss3 != null) levelData.bossPrefab = boss3.GetComponent<DemHoiDenLong.Gameplay.BossController>();
            // Maybe a final boss at level 20? Let's leave it null for now or reuse boss3 if needed.

            // Initialize waves list
            levelData.waves = new List<WaveData>();
            
            // Calculate how many waves in this level
            // Boss levels (5, 10, 15, 20) have fewer waves (2 waves) to focus on the boss
            bool isBossLevel = (i % 5 == 0);
            int numWaves = isBossLevel ? 2 : (3 + (i / 3));

            float currentSpawnTime = 3.0f; // first wave starts at 3 seconds

            for (int w = 0; w < numWaves; w++)
            {
                WaveData wave = new WaveData();
                wave.spawnTime = currentSpawnTime;
                
                // Increase enemy count gradually
                wave.enemyCount = 3 + i + (w * 2);
                
                // Spawn interval decreases as level increases (faster spawning)
                wave.spawnInterval = Mathf.Max(0.5f, 2.0f - (i * 0.05f));

                // Determine which enemy to spawn based on level progression
                EnemyData chosenEnemy = enemyPaper;
                int rnd = Random.Range(0, 100);

                if (i <= 4)
                {
                    // Level 1-4: Mostly Paper, some Fish
                    chosenEnemy = (rnd < 20) ? enemyFish : enemyPaper;
                }
                else if (i <= 8)
                {
                    // Level 5-8: Paper, Fish, Spike
                    if (rnd < 40) chosenEnemy = enemyPaper;
                    else if (rnd < 70) chosenEnemy = enemyFish;
                    else chosenEnemy = enemySpike;
                }
                else
                {
                    // Level 9-20: All types (Paper, Fish, Spike, Demon)
                    if (rnd < 30) chosenEnemy = enemyPaper;
                    else if (rnd < 55) chosenEnemy = enemyFish;
                    else if (rnd < 80) chosenEnemy = enemySpike;
                    else chosenEnemy = enemyDemon;
                }

                // Fallback if missing
                if (chosenEnemy == null) chosenEnemy = enemyPaper;
                wave.enemyType = chosenEnemy;

                levelData.waves.Add(wave);

                // Advance spawn time for the next wave
                float waveDuration = wave.enemyCount * wave.spawnInterval;
                currentSpawnTime += waveDuration + 4.0f; // 4 seconds rest between waves
            }

            // If it's a boss level, set the boss spawn delay to right after the last wave
            levelData.bossSpawnDelay = currentSpawnTime + 3.0f;

            string assetPath = $"Assets/_Project/Data/Levels/SO_Level_{i:00}.asset";
            
            // Delete existing asset if it exists so we can overwrite it
            if (AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(levelData, assetPath);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Created 20 LevelData assets. Bosses automatically assigned to Level 5, 10, 15.");
    }
}
