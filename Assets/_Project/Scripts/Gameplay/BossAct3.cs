using System.Collections;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public class BossAct3 : BossController
    {
        [Header("Movement Control")]
        [SerializeField] private float moveFrequency = 1.2f;
        [SerializeField] private float moveAmplitudeX = 1.35f;
        [SerializeField] private float moveAmplitudeY = 0.35f;
        private float baseY;
        private float aliveTime = 0f;

        [Header("Phase 2 - Crystal Shards")]
        [SerializeField] private GameObject crystalShardPrefab;
        [SerializeField] private int crystalCount = 6;
        [SerializeField] private float crystalOrbitRadius = 2f;
        [SerializeField] private float crystalRotationSpeed = 80f;
        
        [Header("Phase 3 - Ghost Lantern Rain")]
        [SerializeField] private GameObject ghostLanternPrefab;
        [SerializeField] private int ghostsPerWave = 4;
        [SerializeField] private float waveInterval = 2.0f;
        
        private GameObject[] crystalShards;
        private bool isRainingGhosts = false;
        private float waveTimer = 0f;

        protected override void Start()
        {
            base.Start();
            baseY = targetPosition.y;
            baseDamage = 45f;
            bulletSpeed = 6.5f;
            fireRate = 1.2f;
        }

        protected override void Update()
        {
            if (isDead || isInvulnerable) return;

            if (isEntering)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    isEntering = false;
                    baseY = transform.position.y;
                }
            }
            else
            {
                aliveTime += Time.deltaTime;
                HandleMovement();

                if (currentPhase == 1)
                {
                    HandlePhase1Attack();
                }
                else if (currentPhase == 2)
                {
                    HandlePhase2Crystals();
                }
                else if (currentPhase == 3)
                {
                    HandlePhase1Attack();
                    if (isRainingGhosts)
                    {
                        HandlePhase3GhostRain();
                    }
                }
            }
        }

        private void HandleMovement()
        {
            float freq = currentPhase == 3 ? moveFrequency * 1.35f : moveFrequency;
            float ampX = currentPhase == 3 ? moveAmplitudeX * 1.15f : moveAmplitudeX;

            float x = Mathf.Sin(aliveTime * freq) * ampX;
            float y = baseY + Mathf.Sin(aliveTime * freq * 2f) * moveAmplitudeY;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        private void HandlePhase1Attack()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / fireRate)
            {
                attackTimer = 0f;
                FireAimedBullet();
            }
        }

        private void FireAimedBullet()
        {
            if (BulletPool.Instance == null) return;
            
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player == null) return;

            Vector2 baseDirection = (player.transform.position - transform.position).normalized;

            float[] spreadAngles;
            if (currentPhase == 1)
            {
                spreadAngles = new float[] { -14f, 0f, 14f };
            }
            else if (currentPhase == 2)
            {
                spreadAngles = new float[] { -22f, -11f, 0f, 11f, 22f };
            }
            else
            {
                spreadAngles = new float[] { -28f, -14f, 0f, 14f, 28f };
            }

            foreach (float angle in spreadAngles)
            {
                Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDirection;
                BulletPool.Instance.SpawnBullet(transform.position, dir, bulletSpeed, baseDamage * damageMultiplier, false, bulletSprite);
            }
        }

        private void HandlePhase2Crystals()
        {
            if (crystalShards == null) return;
            
            float angle = Time.time * crystalRotationSpeed;
            for (int i = 0; i < crystalShards.Length; i++)
            {
                if (crystalShards[i] == null) continue;
                
                float currentAngle = angle + (360f / crystalCount) * i;
                float rad = currentAngle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * crystalOrbitRadius;
                crystalShards[i].transform.position = transform.position + offset;
            }
            
            HandlePhase1Attack();
        }

        private void HandlePhase3GhostRain()
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                waveTimer = 0f;
                SpawnGhostWave();
            }
        }

        private void SpawnGhostWave()
        {
            if (ghostLanternPrefab == null || EnemySpawner.Instance == null) return;

            float spawnY = maxScreenBounds.y + 1f;
            float startX = minScreenBounds.x + 0.8f;
            float endX = maxScreenBounds.x - 0.8f;
            float spacing = (endX - startX) / Mathf.Max(1, ghostsPerWave - 1);

            for (int i = 0; i < ghostsPerWave; i++)
            {
                float x = startX + i * spacing;
                Vector3 spawnPos = new Vector3(x, spawnY, 0);
                EnemySpawner.Instance.SpawnSpecificEnemy(ghostLanternPrefab, spawnPos);
            }
        }

        protected override void OnPhaseChanged(int newPhase)
        {
            StartCoroutine(PhaseTransition(newPhase));
        }

        private IEnumerator PhaseTransition(int newPhase)
        {
            isInvulnerable = true;
            isPhaseTransitioning = true;
            BulletPool.Instance?.ClearAllActiveBullets();
            
            // Flash effect
            float flashDuration = 1f;
            float flashInterval = 0.1f;
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            
            isInvulnerable = false;
            isPhaseTransitioning = false;

            if (newPhase == 2)
            {
                Debug.Log("Boss Act 3 - Phase 2: Triệu hồi mảnh pha lê xoay nhanh!");
                crystalRotationSpeed = 100f;
                SpawnCrystalShards();
                bulletSpeed = 7.5f;
                fireRate = 1.3f;
            }
            else if (newPhase == 3)
            {
                Debug.Log("Boss Act 3 - Phase 3: Mưa đèn lồng ma chi viện + Bắn cuồng nộ!");
                DestroyCrystalShards();
                isRainingGhosts = true;
                bulletSpeed = 8.5f;
                fireRate = 1.6f;
                waveTimer = waveInterval; // Spawn ngay lập tức 1 đợt Đèn Quỷ hộ vệ!
                SpawnGhostWave();
            }
        }

        private void SpawnCrystalShards()
        {
            if (crystalShardPrefab == null)
            {
                Debug.LogWarning("Crystal Shard Prefab not assigned!");
                return;
            }

            crystalShards = new GameObject[crystalCount];
            
            for (int i = 0; i < crystalCount; i++)
            {
                float angle = (360f / crystalCount) * i;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * crystalOrbitRadius;
                
                GameObject shard = Instantiate(crystalShardPrefab, transform.position + offset, Quaternion.identity, transform);
                
                BossShield shieldComp = shard.GetComponent<BossShield>();
                if (shieldComp != null)
                {
                    shieldComp.OnShieldDestroyed += OnCrystalDestroyed;
                }
                
                crystalShards[i] = shard;
            }
        }

        private void OnCrystalDestroyed(BossShield shard)
        {
            Vector3 pos = shard.transform.position;
            
            if (BulletPool.Instance != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
                    BulletPool.Instance.SpawnBullet(pos, direction, bulletSpeed * 0.7f, baseDamage * damageMultiplier * 0.5f, false, bulletSprite);
                }
            }
        }

        private void DestroyCrystalShards()
        {
            if (crystalShards == null) return;
            
            foreach (var shard in crystalShards)
            {
                if (shard != null)
                {
                    Destroy(shard);
                }
            }
            crystalShards = null;
        }

        public override void TakeDamage(float amount)
        {
            if (isInvulnerable || isPhaseTransitioning)
            {
                return;
            }

            // Ở Phase 3, nếu còn Đèn Quỷ hộ vệ trên sân thì Boss nhận giảm 40% sát thương
            if (currentPhase == 3 && EnemySpawner.Instance != null && EnemySpawner.Instance.ActiveEnemyCount > 0)
            {
                amount *= 0.6f;
            }

            base.TakeDamage(amount);
        }
    }
}
