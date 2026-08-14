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
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float spawnYOffset = 1.0f;
        [SerializeField] private int initialPoolSize = 30;

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

            EnemyBase enemy = null;
            if (availableEnemies.Count > 0)
            {
                enemy = availableEnemies.Dequeue();
            }
            else
            {
                enemy = CreateNewEnemy();
            }

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                enemy.Initialize(spawnPos, minScreenBounds, maxScreenBounds, hpMultiplier, speedMultiplier, damageMultiplier);
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

        public void ClearAllActiveEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                RecycleEnemy(activeEnemies[i]);
            }
        }

        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
    }
}
