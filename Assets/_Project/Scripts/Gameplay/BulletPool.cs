using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// BulletPool implements high-performance Object Pooling for projectiles,
    /// eliminating GC allocations and instantiation stutter during intense shooting scenes.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance { get; private set; }

        [Header("Pool Configuration")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int initialPoolSize = 200;
        [SerializeField] private bool allowAutoExpand = true;

        [Header("Screen Boundary References")]
        [SerializeField] private Camera mainCamera;

        private Queue<Bullet> availableBullets;
        private List<Bullet> activeBullets;
        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;

        public int InitialPoolSize
        {
            get => initialPoolSize;
            set => initialPoolSize = value;
        }

        public int AvailableCount => availableBullets != null ? availableBullets.Count : 0;
        public int ActiveCount => activeBullets != null ? activeBullets.Count : 0;
        public int TotalCount => AvailableCount + ActiveCount;

        public static void ResetInstanceForTesting()
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
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            CalculateScreenBounds();
        }

        /// <summary>
        /// Pre-allocates the pool of bullets on initialization to prevent runtime allocations.
        /// </summary>
        public void InitializePool()
        {
            if (availableBullets != null && availableBullets.Count > 0) return;

            int poolSize = initialPoolSize > 0 ? initialPoolSize : 200;
            initialPoolSize = poolSize;

            availableBullets = new Queue<Bullet>(poolSize);
            activeBullets = new List<Bullet>(poolSize);

            // Create default prefab fallback if not set in Inspector
            if (bulletPrefab == null)
            {
                bulletPrefab = CreateDefaultBulletPrefab();
            }

            for (int i = 0; i < poolSize; i++)
            {
                Bullet bullet = CreateNewBullet();
                if (bullet != null)
                {
                    availableBullets.Enqueue(bullet);
                }
            }
        }

        private GameObject CreateDefaultBulletPrefab()
        {
            GameObject defaultObj = new GameObject("DefaultBulletPrefab");
            defaultObj.SetActive(false);

            SpriteRenderer sr = defaultObj.AddComponent<SpriteRenderer>();
            sr.color = Color.yellow;

            BoxCollider2D col = defaultObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.2f, 0.5f);

            defaultObj.AddComponent<Bullet>();
            defaultObj.transform.SetParent(transform);
            return defaultObj;
        }

        private Bullet CreateNewBullet()
        {
            GameObject bulletObj = Instantiate(bulletPrefab, transform);
            bulletObj.name = "Bullet_Pooled";
            bulletObj.SetActive(false);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet == null)
            {
                bullet = bulletObj.AddComponent<Bullet>();
            }
            return bullet;
        }

        /// <summary>
        /// Recalculates screen boundaries for bullet despawn limits.
        /// </summary>
        public void CalculateScreenBounds()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null)
            {
                minScreenBounds = new Vector2(-10f, -10f);
                maxScreenBounds = new Vector2(10f, 10f);
                return;
            }

            Vector3 lowerLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 upperRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            minScreenBounds = lowerLeft;
            maxScreenBounds = upperRight;
        }

        /// <summary>
        /// Spawns a bullet from the pool at specified position, direction, speed, and damage payload.
        /// </summary>
        public Bullet SpawnBullet(Vector3 spawnPosition, Vector2 direction, float speed, float damage, bool isPlayerBullet, Sprite customSprite = null)
        {
            if (availableBullets == null || activeBullets == null || availableBullets.Count == 0 && activeBullets.Count == 0)
            {
                InitializePool();
            }

            Bullet bullet = null;

            if (availableBullets.Count > 0)
            {
                bullet = availableBullets.Dequeue();
            }
            else if (allowAutoExpand)
            {
                bullet = CreateNewBullet();
            }
            else
            {
                // If pool is exhausted and auto-expand disabled, reuse oldest active bullet
                if (activeBullets.Count > 0)
                {
                    bullet = activeBullets[0];
                    activeBullets.RemoveAt(0);
                }
            }

            if (bullet != null)
            {
                activeBullets.Add(bullet);
                bullet.Initialize(spawnPosition, direction, speed, damage, isPlayerBullet, minScreenBounds, maxScreenBounds, customSprite);
            }

            return bullet;
        }

        /// <summary>
        /// Recycles an active bullet back into the pool.
        /// </summary>
        public void ReturnBullet(Bullet bullet)
        {
            if (bullet == null) return;

            if (activeBullets == null || availableBullets == null)
            {
                InitializePool();
            }

            if (activeBullets.Contains(bullet))
            {
                activeBullets.Remove(bullet);
            }

            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(transform);

            if (!availableBullets.Contains(bullet))
            {
                availableBullets.Enqueue(bullet);
            }
        }

        /// <summary>
        /// Clears all active bullets on screen (useful for bombs, level resets, or victory).
        /// </summary>
        public void ClearAllActiveBullets()
        {
            if (activeBullets == null) return;

            for (int i = activeBullets.Count - 1; i >= 0; i--)
            {
                ReturnBullet(activeBullets[i]);
            }
        }
    }
}
