using System;
using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }
        
        public event Action OnLevelCompleted;
        public event Action<BossController> OnBossSpawned;

        [SerializeField] private LevelData currentLevelData;
        
        private float levelTimer = 0f;
        private int currentWaveIndex = 0;
        private bool isLevelActive = false;
        private bool isSpawningWave = false;
        private int enemiesSpawnedInCurrentWave = 0;
        private float nextSpawnTime = 0f;
        
        private bool bossSpawned = false;
        private BossController activeBoss;

        public bool IsLevelActive => isLevelActive;
        public BossController ActiveBoss => activeBoss;
        public LevelData CurrentLevelData => currentLevelData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void StartLevel(LevelData levelData)
        {
            currentLevelData = levelData;
            levelTimer = 0f;
            currentWaveIndex = 0;
            isLevelActive = true;
            bossSpawned = false;
            isSpawningWave = false;

            if (EnemySpawner.Instance != null && levelData != null)
            {
                EnemySpawner.Instance.SetDifficultyMultipliers(
                    levelData.baseHpMultiplier,
                    levelData.baseSpeedMultiplier,
                    levelData.baseDamageMultiplier
                );
            }
        }

        private void Update()
        {
            if (!isLevelActive || currentLevelData == null || bossSpawned) return;

            levelTimer += Time.deltaTime;

            if (!isSpawningWave)
            {
                if (currentWaveIndex < currentLevelData.waves.Count)
                {
                    WaveData nextWave = currentLevelData.waves[currentWaveIndex];
                    if (levelTimer >= nextWave.spawnTime)
                    {
                        StartWave();
                    }
                }
                else
                {
                    // Check if boss should spawn
                    if (EnemySpawner.Instance != null && EnemySpawner.Instance.ActiveEnemyCount == 0)
                    {
                        SpawnBoss();
                    }
                    else if (EnemySpawner.Instance == null)
                    {
                        SpawnBoss();
                    }
                }
            }
            else
            {
                ProcessWaveSpawning();
            }
        }

        private void StartWave()
        {
            isSpawningWave = true;
            enemiesSpawnedInCurrentWave = 0;
            nextSpawnTime = Time.time;
        }

        private void ProcessWaveSpawning()
        {
            WaveData currentWave = currentLevelData.waves[currentWaveIndex];

            if (Time.time >= nextSpawnTime)
            {
                if (EnemySpawner.Instance != null)
                {
                    EnemySpawner.Instance.SpawnEnemy();
                }

                enemiesSpawnedInCurrentWave++;
                nextSpawnTime = Time.time + currentWave.spawnInterval;

                if (enemiesSpawnedInCurrentWave >= currentWave.enemyCount)
                {
                    isSpawningWave = false;
                    currentWaveIndex++;
                }
            }
        }

        private void SpawnBoss()
        {
            if (currentLevelData.bossPrefab == null) 
            {
                isLevelActive = false;
                OnLevelCompleted?.Invoke();
                return;
            }
            
            bossSpawned = true;
            
            float spawnY = 7f;
            if (Camera.main != null)
            {
                Vector3 upperRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                spawnY = upperRight.y + 2f;
            }

            Vector3 spawnPos = new Vector3(0, spawnY, 0); 
            activeBoss = Instantiate(currentLevelData.bossPrefab, spawnPos, Quaternion.identity);
            
            activeBoss.InitializeStats(
                currentLevelData.baseHpMultiplier,
                currentLevelData.baseDamageMultiplier
            );
            
            activeBoss.OnDeath += HandleBossDeath;
            
            OnBossSpawned?.Invoke(activeBoss);
        }

        private void HandleBossDeath()
        {
            if (activeBoss != null)
            {
                activeBoss.OnDeath -= HandleBossDeath;
            }
            isLevelActive = false;
            OnLevelCompleted?.Invoke();
        }

        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
    }
}
