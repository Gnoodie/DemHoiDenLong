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
        public event Action<int> OnWaveCompleted;
        public event Action<BossController> OnBossSpawned;
        public event Action<int, int> OnKillsChanged;
        public event Action<float, float> OnSurviveTimerChanged;

        [SerializeField] private LevelData currentLevelData;
        
        private float levelTimer = 0f;
        private int currentKills = 0;
        private int currentWaveIndex = 0;
        private bool isLevelActive = false;
        private bool isSpawningWave = false;
        private int enemiesSpawnedInCurrentWave = 0;
        private float nextSpawnTime = 0f;
        
        private bool bossSpawned = false;
        private bool hasTriggeredEnd = false;
        private BossController activeBoss;

        public bool IsLevelActive => isLevelActive;
        public bool HasTriggeredEnd => hasTriggeredEnd;
        public BossController ActiveBoss => activeBoss;
        public LevelData CurrentLevelData => currentLevelData;
        public int CurrentKills => currentKills;
        public float LevelTimer => levelTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EnemyBase.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            EnemyBase.OnEnemyDied -= HandleEnemyDied;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (activeBoss != null)
            {
                activeBoss.OnDeath -= HandleBossDeath;
            }
        }

        private void Start()
        {
            // Level starts when GameManager enters Playing state
        }

        public void StartLevel(LevelData levelData)
        {
            if (LevelSession.level != null)
            {
                levelData = LevelSession.level;
            }

            currentLevelData = levelData;
            levelTimer = 0f;
            currentKills = 0;
            currentWaveIndex = 0;
            isLevelActive = true;
            bossSpawned = false;
            isSpawningWave = false;
            hasTriggeredEnd = false;

            if (EnemySpawner.Instance != null && levelData != null)
            {
                EnemySpawner.Instance.SetDifficultyMultipliers(
                    levelData.hpMultiplier > 0f ? levelData.hpMultiplier : levelData.baseHpMultiplier,
                    levelData.speedMultiplier > 0f ? levelData.speedMultiplier : levelData.baseSpeedMultiplier,
                    levelData.damageMultiplier > 0f ? levelData.damageMultiplier : levelData.baseDamageMultiplier
                );
                EnemySpawner.Instance.StopSpawning();
            }

            int targetKills = currentLevelData != null ? currentLevelData.targetKillCount : 0;
            OnKillsChanged?.Invoke(currentKills, targetKills);
            float targetSurvive = currentLevelData != null ? currentLevelData.surviveSeconds : 0f;
            OnSurviveTimerChanged?.Invoke(levelTimer, targetSurvive);

            Debug.Log($"[WaveManager] Bắt đầu màn {levelData?.levelId}: Điều kiện={levelData?.winCondition}, TargetKills={targetKills}, Survive={targetSurvive}s, IsBoss={levelData?.isBoss}");
        }

        private void HandleEnemyDied(EnemyBase enemy)
        {
            if (!isLevelActive || hasTriggeredEnd) return;

            currentKills = LevelSession.Kills;
            int targetKills = currentLevelData != null ? currentLevelData.targetKillCount : 0;
            OnKillsChanged?.Invoke(currentKills, targetKills);

            // Log mỗi khi đạt mốc 5 địch
            if (currentKills > 0 && currentKills % 5 == 0 && currentLevelData != null && currentLevelData.winCondition == WinCondition.KillCount)
            {
                Debug.Log($"[RUN] KillCount {currentKills}/{targetKills}");
            }

            // Kiểm tra điều kiện thắng KillCount
            if (currentLevelData != null && currentLevelData.winCondition == WinCondition.KillCount)
            {
                if (currentKills >= targetKills)
                {
                    Debug.Log($"[WaveManager] Đạt mục tiêu hạ {currentKills}/{targetKills} địch -> Thắng màn!");
                    TriggerLevelCompleted("KillCount");
                }
            }
        }

        private void Update()
        {
            if (!isLevelActive || currentLevelData == null || hasTriggeredEnd) return;

            levelTimer += Time.deltaTime;
            LevelSession.elapsedTime = levelTimer;

            // Kiểm tra điều kiện thắng SurviveTime
            if (currentLevelData.winCondition == WinCondition.SurviveTime)
            {
                float targetSurvive = currentLevelData.surviveSeconds;
                OnSurviveTimerChanged?.Invoke(levelTimer, targetSurvive);
                if (levelTimer >= targetSurvive)
                {
                    Debug.Log($"[WaveManager] Sống sót đủ {levelTimer:F1}/{targetSurvive:F1}s -> Thắng màn!");
                    TriggerLevelCompleted();
                    return;
                }
            }

            if (bossSpawned) return;

            if (!isSpawningWave)
            {
                if (currentLevelData.waves != null && currentWaveIndex < currentLevelData.waves.Count)
                {
                    WaveData nextWave = currentLevelData.waves[currentWaveIndex];
                    if (levelTimer >= nextWave.spawnTime)
                    {
                        StartWave();
                    }
                }
                else
                {
                    // Đã hết danh sách wave trong LevelData
                    if (currentLevelData.winCondition == WinCondition.BossKill)
                    {
                        if (currentLevelData.isBoss)
                        {
                            if (EnemySpawner.Instance == null || EnemySpawner.Instance.ActiveEnemyCount == 0)
                            {
                                SpawnBoss();
                            }
                        }
                        else
                        {
                            // Màn thường không có Boss -> hoàn thành khi sạch địch
                            if (EnemySpawner.Instance == null || EnemySpawner.Instance.ActiveEnemyCount == 0)
                            {
                                TriggerLevelCompleted();
                            }
                        }
                    }
                    else
                    {
                        // Điều kiện KillCount hoặc SurviveTime: nếu hết wave mà chưa đủ mục tiêu,
                        // EnemySpawner tiếp tục sinh địch định kỳ để người chơi hoàn thành mục tiêu.
                        if (EnemySpawner.Instance != null && !EnemySpawner.Instance.IsSpawning)
                        {
                            EnemySpawner.Instance.StartSpawning();
                        }
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
            if (currentLevelData.waves == null || currentWaveIndex >= currentLevelData.waves.Count)
            {
                isSpawningWave = false;
                return;
            }

            WaveData currentWave = currentLevelData.waves[currentWaveIndex];

            if (Time.time >= nextSpawnTime)
            {
                if (EnemySpawner.Instance != null)
                {
                    if (currentWave.enemyType != null && currentWave.enemyType.prefab != null)
                    {
                        EnemySpawner.Instance.CalculateScreenBounds();
                        Vector2 maxBounds = EnemySpawner.Instance.GetMaxScreenBounds();
                        Vector2 minBounds = EnemySpawner.Instance.GetMinScreenBounds();
                        float randomX = UnityEngine.Random.Range(minBounds.x + 0.5f, maxBounds.x - 0.5f);
                        float spawnY = maxBounds.y + 1.0f;
                        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

                        EnemySpawner.Instance.SpawnSpecificEnemy(currentWave.enemyType.prefab, spawnPos, 1.0f);
                    }
                    else
                    {
                        EnemySpawner.Instance.SpawnEnemy();
                    }
                }

                enemiesSpawnedInCurrentWave++;
                nextSpawnTime = Time.time + currentWave.spawnInterval;

                if (enemiesSpawnedInCurrentWave >= currentWave.enemyCount)
                {
                    isSpawningWave = false;
                    currentWaveIndex++;
                    OnWaveCompleted?.Invoke(currentWaveIndex);
                }
            }
        }

        private void SpawnBoss()
        {
            if (!currentLevelData.isBoss) 
            {
                TriggerLevelCompleted();
                return;
            }

            string bossName = currentLevelData.levelId <= 5 ? "PF_BossAct1" : (currentLevelData.levelId <= 10 ? "PF_BossAct2" : "PF_BossAct3");
            string displayName = currentLevelData.bossName;
            float bHp = currentLevelData.bossHp > 0f ? currentLevelData.bossHp : (currentLevelData.levelId <= 5 ? GameplayBalance.BossAct1Hp : (currentLevelData.levelId <= 10 ? GameplayBalance.BossAct2Hp : GameplayBalance.BossAct3Hp));
            float bSpeed = currentLevelData.bossSpeed > 0f ? currentLevelData.bossSpeed : (currentLevelData.levelId <= 5 ? GameplayBalance.BossAct1Speed : (currentLevelData.levelId <= 10 ? GameplayBalance.BossAct2Speed : GameplayBalance.BossAct3Speed));
            float bDmg = currentLevelData.bossDamage > 0f ? currentLevelData.bossDamage : (currentLevelData.levelId <= 5 ? GameplayBalance.BossAct1Damage : (currentLevelData.levelId <= 10 ? GameplayBalance.BossAct2Damage : GameplayBalance.BossAct3Damage));

            BossController prefabToSpawn = Resources.Load<BossController>($"Prefabs/Bosses/{bossName}");
#if UNITY_EDITOR
            if (prefabToSpawn == null)
            {
                prefabToSpawn = UnityEditor.AssetDatabase.LoadAssetAtPath<BossController>($"Assets/_Project/Prefabs/Bosses/{bossName}.prefab");
            }
#endif
            if (prefabToSpawn == null)
            {
                Debug.LogError($"[WaveManager] Không tìm thấy bossPrefab {bossName}!");
                TriggerLevelCompleted();
                return;
            }
            
            bossSpawned = true;
            Audio.AudioManager.Instance?.PlayWarningBoss();
            Audio.AudioManager.Instance?.PlayBGM(Audio.AudioManager.Instance.bgmBossFight);
            
            float spawnY = 7f;
            if (Camera.main != null)
            {
                Vector3 upperRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                spawnY = upperRight.y + 2f;
            }

            Vector3 spawnPos = new Vector3(0, spawnY, 0); 
            activeBoss = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            activeBoss.BossDisplayName = displayName;
            activeBoss.gameObject.name = displayName;
            
            activeBoss.InitializeFromData(bHp, bSpeed, bDmg);
            activeBoss.OnDeath += HandleBossDeath;
            
            OnBossSpawned?.Invoke(activeBoss);
            Debug.Log($"[WaveManager] Đã sinh trùm {displayName}: HP={bHp}, Speed={bSpeed}, DMG={bDmg}");
        }

        private void HandleBossDeath()
        {
            if (activeBoss != null)
            {
                activeBoss.OnDeath -= HandleBossDeath;
            }

            Debug.Log("[WaveManager] Trùm đã bị tiêu diệt -> Thắng màn!");
            TriggerLevelCompleted("BossKill");
        }

        public void StopLevel()
        {
            isLevelActive = false;
        }

        public void TriggerLevelCompleted(string winReason = "")
        {
            if (hasTriggeredEnd) return;
            hasTriggeredEnd = true;
            isLevelActive = false;

            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.StopSpawning();
            }

            OnLevelCompleted?.Invoke();
            GameManager.Instance?.TriggerVictory(winReason);
        }

        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
    }
}
