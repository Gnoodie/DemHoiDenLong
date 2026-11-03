using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// EnemySpawner manages spawning waves of enemies at top of screen,
    /// applying level difficulty scaling multipliers (HP, Speed, Damage), and recycling inactive enemy objects.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("Spawner Configuration")]
        [SerializeField] private GameObject defaultEnemyPrefab;
        [SerializeField] private EnemyData defaultEnemyData;

        [Header("Act Lantern Prefabs (Giai đoạn 5A.1)")]
        [SerializeField] private GameObject prefabPaper;
        [SerializeField] private GameObject prefabSpike;
        [SerializeField] private GameObject prefabCarp;
        [SerializeField] private GameObject prefabGhost;
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float spawnYOffset = 1.0f;
        [SerializeField] private int initialPoolSize = 30;
        [SerializeField] private bool autoStart = true;

        [Header("Level Difficulty Multipliers")]
        [SerializeField] private float hpMultiplier = 1.0f;
        [SerializeField] private float speedMultiplier = 1.0f;
        [SerializeField] private float damageMultiplier = 1.0f;

        [Header("Camera & Bounds")]
        [SerializeField] private Camera mainCamera;

        private Queue<EnemyBase> availableEnemies;
        private List<EnemyBase> activeEnemies;
        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;
        private float spawnTimer = 0f;
        private bool isSpawning = false;

        public bool IsSpawning => isSpawning;
        public int ActiveEnemyCount => activeEnemies != null ? activeEnemies.Count : 0;
        public int AvailableEnemyCount => availableEnemies != null ? availableEnemies.Count : 0;
        public List<EnemyBase> GetActiveEnemies() => activeEnemies;
        public Vector2 GetMinScreenBounds() => minScreenBounds;
        public Vector2 GetMaxScreenBounds() => maxScreenBounds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePool();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            CalculateScreenBounds();
            if (autoStart && WaveManager.Instance == null)
            {
                StartSpawning();
            }
        }

        private void Update()
        {
            if (!isSpawning) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnEnemy();
            }
        }

        public void InitializePool()
        {
            if (availableEnemies != null && availableEnemies.Count > 0) return;

            availableEnemies = new Queue<EnemyBase>(initialPoolSize);
            activeEnemies = new List<EnemyBase>(initialPoolSize);

            if (defaultEnemyPrefab == null)
            {
                defaultEnemyPrefab = CreateDefaultPaperLanternPrefab();
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                EnemyBase enemy = CreateNewEnemy();
                availableEnemies.Enqueue(enemy);
            }
        }

        private GameObject CreateDefaultPaperLanternPrefab()
        {
            GameObject obj = new GameObject("DefaultPaperLanternPrefab");
            obj.SetActive(false);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.color = Color.red;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            obj.AddComponent<PaperLanternEnemy>();
            obj.transform.SetParent(transform);
            return obj;
        }

        private EnemyBase CreateNewEnemy()
        {
            GameObject enemyObj = Instantiate(defaultEnemyPrefab, transform);
            enemyObj.name = "Enemy_Pooled";
            enemyObj.SetActive(false);

            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy == null)
            {
                enemy = enemyObj.AddComponent<PaperLanternEnemy>();
            }
            return enemy;
        }

        public void CalculateScreenBounds()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null)
            {
                minScreenBounds = new Vector2(-5f, -5f);
                maxScreenBounds = new Vector2(5f, 5f);
                return;
            }

            Vector3 lowerLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 upperRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            minScreenBounds = lowerLeft;
            maxScreenBounds = upperRight;
        }

        /// <summary>
        /// Starts automatic periodic enemy spawning.
        /// </summary>
        public void StartSpawning()
        {
            isSpawning = true;
            spawnTimer = 0f;
        }

        /// <summary>
        /// Stops enemy spawning.
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Sets level scaling difficulty parameters.
        /// </summary>
        public void SetDifficultyMultipliers(float hpm, float speedm, float dmgm)
        {
            hpMultiplier = hpm;
            speedMultiplier = speedm;
            damageMultiplier = dmgm;
        }

        private void EnsurePrefabsLoaded()
        {
            if (prefabPaper == null) prefabPaper = defaultEnemyPrefab;
#if UNITY_EDITOR
            if (prefabPaper == null) prefabPaper = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGiay.prefab");
            if (prefabSpike == null) prefabSpike = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenGai.prefab");
            if (prefabCarp == null) prefabCarp = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenCa.prefab");
            if (prefabGhost == null) prefabGhost = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_DenQuy.prefab");
#endif
            if (prefabPaper == null) prefabPaper = Resources.Load<GameObject>("Prefabs/Enemies/PF_Enemy_DenGiay");
            if (prefabSpike == null) prefabSpike = Resources.Load<GameObject>("Prefabs/Enemies/PF_Enemy_DenGai");
            if (prefabCarp == null) prefabCarp = Resources.Load<GameObject>("Prefabs/Enemies/PF_Enemy_DenCa");
            if (prefabGhost == null) prefabGhost = Resources.Load<GameObject>("Prefabs/Enemies/PF_Enemy_DenQuy");
        }

        public GameObject GetWeightedRandomEnemy(out string typeId, out int baseStarVal)
        {
            EnsurePrefabsLoaded();

            int actIdx = 0;
            int levelId = 1;
            if (LevelSession.level != null)
            {
                actIdx = LevelSession.level.actIndex;
                levelId = LevelSession.level.levelId;
            }
            else if (WaveManager.Instance != null && WaveManager.Instance.CurrentLevelData != null)
            {
                actIdx = WaveManager.Instance.CurrentLevelData.actIndex;
                levelId = WaveManager.Instance.CurrentLevelData.levelId;
            }

            var candidates = new List<(string id, float weight, GameObject prefab, int stars)>();

            if (actIdx == 0) // Hồi 1: Giấy (0.5), Gai (0.2)
            {
                candidates.Add(("den_giay", GameplayBalance.SpawnWeightPaper, prefabPaper != null ? prefabPaper : defaultEnemyPrefab, GameplayBalance.StarValuePaper));
                candidates.Add(("den_gai", GameplayBalance.SpawnWeightSpike, prefabSpike != null ? prefabSpike : defaultEnemyPrefab, GameplayBalance.StarValueSpike));
            }
            else if (actIdx == 1) // Hồi 2: Gai (0.2), Cá chép (0.15), Ma (0.1)
            {
                candidates.Add(("den_gai", GameplayBalance.SpawnWeightSpike, prefabSpike != null ? prefabSpike : defaultEnemyPrefab, GameplayBalance.StarValueSpike));
                candidates.Add(("den_ca", GameplayBalance.SpawnWeightCarp, prefabCarp != null ? prefabCarp : defaultEnemyPrefab, GameplayBalance.StarValueCarp));
                candidates.Add(("den_quy", GameplayBalance.SpawnWeightGhost, prefabGhost != null ? prefabGhost : defaultEnemyPrefab, GameplayBalance.StarValueGhost));
            }
            else // Hồi 3: Ma (0.1)
            {
                candidates.Add(("den_quy", GameplayBalance.SpawnWeightGhost, prefabGhost != null ? prefabGhost : defaultEnemyPrefab, GameplayBalance.StarValueGhost));
            }

            float totalWeight = 0f;
            foreach (var c in candidates) totalWeight += c.weight;

            float roll = Random.Range(0f, totalWeight);
            float accum = 0f;

            foreach (var c in candidates)
            {
                accum += c.weight;
                if (roll <= accum)
                {
                    typeId = c.id;
                    baseStarVal = c.stars;
                    return c.prefab;
                }
            }

            var fallback = candidates[0];
            typeId = fallback.id;
            baseStarVal = fallback.stars;
            return fallback.prefab;
        }

        /// <summary>
        /// Spawns a single enemy at a random X coordinate above top screen boundary.
        /// </summary>
        public EnemyBase SpawnEnemy()
        {
            CalculateScreenBounds();

            float paddingX = 0.5f;
            float randomX = Random.Range(minScreenBounds.x + paddingX, maxScreenBounds.x - paddingX);
            float spawnY = maxScreenBounds.y + spawnYOffset;
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            string typeId;
            int baseStars;
            GameObject prefab = GetWeightedRandomEnemy(out typeId, out baseStars);

            EnemyBase enemy = null;
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                enemy = obj.GetComponent<EnemyBase>();
            }

            if (enemy == null)
            {
                if (availableEnemies.Count > 0)
                {
                    enemy = availableEnemies.Dequeue();
                }
                else
                {
                    enemy = CreateNewEnemy();
                }
            }

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                enemy.Initialize(spawnPos, minScreenBounds, maxScreenBounds, hpMultiplier, speedMultiplier, damageMultiplier);
                enemy.SetStarReward(Mathf.RoundToInt(baseStars * GameplayBalance.StarValueMultiplier));
                LevelSession.RecordEnemySpawned(typeId);
            }

            return enemy;
        }

        /// <summary>
        /// Spawns a specific enemy prefab at the given position (bypasses the default pool)
        /// </summary>
        public EnemyBase SpawnSpecificEnemy(GameObject prefab, Vector3 position, float customHpMult = 1f)
        {
            if (prefab == null) return null;
            CalculateScreenBounds();
            
            GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
            EnemyBase enemy = obj.GetComponent<EnemyBase>();
            
            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                enemy.Initialize(position, minScreenBounds, maxScreenBounds, hpMultiplier * customHpMult, speedMultiplier, damageMultiplier);
                string id = prefab.name.ToLower();
                int stars = GameplayBalance.GetEnemyStarValue(id);
                enemy.SetStarReward(Mathf.RoundToInt(stars * GameplayBalance.StarValueMultiplier));
                LevelSession.RecordEnemySpawned(id);
            }
            return enemy;
        }

        /// <summary>
        /// Recycles an enemy instance back to the available pool.
        /// </summary>
        public void RecycleEnemy(EnemyBase enemy)
        {
            if (enemy == null) return;

            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
            }

            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(transform);

            if (!availableEnemies.Contains(enemy))
            {
                availableEnemies.Enqueue(enemy);
            }
        }

                /// <summary>
        /// Cleans up all active enemies when the level is won: fades them out over 0.3s without kill/star rewards.
        /// </summary>
        public void CleanupEnemiesOnVictory()
        {
            StopSpawning();
            if (activeEnemies != null)
            {
                for (int i = activeEnemies.Count - 1; i >= 0; i--)
                {
                    var enemy = activeEnemies[i];
                    if (enemy != null && enemy.gameObject.activeInHierarchy)
                    {
                        enemy.FadeAndDespawnOnVictory();
                    }
                }
            }
        }

        public void ClearAllActiveEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                RecycleEnemy(activeEnemies[i]);
            }
        }

        public void KillAllActiveEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null && activeEnemies[i].gameObject.activeInHierarchy)
                {
                    activeEnemies[i].TakeDamage(9999f);
                }
            }
        }

        public void StunAllEnemies(float duration)
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    enemy.ApplyStun(duration);
                }
            }
        }

        public void ApplyTemporarySpeedBuff(float multiplier, float duration)
        {
            StartCoroutine(TemporarySpeedBuffRoutine(multiplier, duration));
        }

        private System.Collections.IEnumerator TemporarySpeedBuffRoutine(float multiplier, float duration)
        {
            float originalSpeed = speedMultiplier;
            speedMultiplier *= multiplier;
            
            // Also update currently active enemies
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    enemy.Initialize(enemy.transform.position, minScreenBounds, maxScreenBounds, hpMultiplier, speedMultiplier, damageMultiplier);
                }
            }

            yield return new WaitForSeconds(duration);

            speedMultiplier = originalSpeed;
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    enemy.Initialize(enemy.transform.position, minScreenBounds, maxScreenBounds, hpMultiplier, speedMultiplier, damageMultiplier);
                }
            }
        }

        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
    }
}
