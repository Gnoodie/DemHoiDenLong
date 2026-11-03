using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public class BossAct1 : BossController
    {
        [Header("Shield Barrier Settings")]
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] private float shieldSpacing = 1.35f;
        [SerializeField] private float phase3FireRateMultiplier = 1.8f;
        
        [Header("Movement Settings")]
        [SerializeField] private float horizontalMoveSpeed = 1.2f;
        [SerializeField] private float moveRange = 2f;
        
        private List<BossShield> activeShields = new List<BossShield>();
        private bool hasShieldBarrier = false;
        private float moveDirection = 1f;
        private float baseY;
        private bool alternatePattern = false;

        protected override void Start()
        {
            base.Start();
            baseY = targetPosition.y;
            bulletSpeed = 5.5f;
            fireRate = 1.0f;
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
                HandleHorizontalMovement();
                HandleAttackPattern();
            }
        }

        private void HandleHorizontalMovement()
        {
            float newX = transform.position.x + moveDirection * horizontalMoveSpeed * Time.deltaTime;
            
            if (Mathf.Abs(newX) > moveRange)
            {
                moveDirection *= -1;
                newX = Mathf.Clamp(newX, -moveRange, moveRange);
            }
            
            transform.position = new Vector3(newX, baseY, 0);
        }

        protected override void HandleAttackPattern()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / fireRate)
            {
                attackTimer = 0f;
                FireAlternatingPattern();
            }
        }

        private void FireAlternatingPattern()
        {
            if (BulletPool.Instance == null) return;
            Audio.AudioManager.Instance?.PlayBossShoot();

            alternatePattern = !alternatePattern;
            float startAngleOffset = alternatePattern ? 22.5f : 0f;

            // Bắn tỏa tròn 8 hướng với góc lệch xoay xen kẽ
            float angleStep = 360f / bulletCount;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep + startAngleOffset;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
                BulletPool.Instance.SpawnBullet(transform.position, direction, bulletSpeed, baseDamage * damageMultiplier, false, bulletSprite);
            }

            // Ở loạt xoay lệch, kèm thêm 1 viên đạn nhắm thẳng vào người chơi
            if (alternatePattern)
            {
                PlayerController player = FindAnyObjectByType<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    Vector2 aimedDir = (player.transform.position - transform.position).normalized;
                    BulletPool.Instance.SpawnBullet(transform.position, aimedDir, bulletSpeed * 1.15f, baseDamage * damageMultiplier, false, bulletSprite);
                }
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
                Debug.Log("Boss Act 1 - Phase 2: Triệu 2 tay khiên khép góc bảo vệ!");
                SpawnShieldArms();
            }
            else if (newPhase == 3)
            {
                Debug.Log("Boss Act 1 - Phase 3: Bỏ khiên, cuồng nộ tăng tốc bắn và lướt nhanh!");
                DestroyAllShields();
                horizontalMoveSpeed *= 1.5f;
                fireRate *= phase3FireRateMultiplier;
                bulletSpeed = 6.2f;
            }
        }

        private void SpawnShieldArms()
        {
            if (shieldPrefab == null)
            {
                Debug.LogWarning("Shield Prefab is not assigned!");
                return;
            }

            hasShieldBarrier = true;
            
            // 2 tay khiên khép lại phía trước mặt Boss bảo vệ trực diện
            Vector3 leftPos = transform.position + new Vector3(-shieldSpacing, -0.4f, 0);
            Vector3 rightPos = transform.position + new Vector3(shieldSpacing, -0.4f, 0);

            SpawnShield(leftPos);
            SpawnShield(rightPos);
        }

        private void SpawnShield(Vector3 position)
        {
            GameObject shieldObj = Instantiate(shieldPrefab, position, Quaternion.identity, transform);
            BossShield shield = shieldObj.GetComponent<BossShield>();
            
            if (shield != null)
            {
                shield.OnShieldDestroyed += HandleShieldDestroyed;
                activeShields.Add(shield);
            }
        }

        private void HandleShieldDestroyed(BossShield shield)
        {
            if (activeShields.Contains(shield))
            {
                activeShields.Remove(shield);
            }

            if (activeShields.Count == 0)
            {
                hasShieldBarrier = false;
                Debug.Log("Tất cả khiên đã bị phá! Boss có thể bị sát thương trở lại!");
            }
        }

        private void DestroyAllShields()
        {
            foreach (var shield in activeShields)
            {
                if (shield != null)
                {
                    Destroy(shield.gameObject);
                }
            }
            activeShields.Clear();
            hasShieldBarrier = false;
        }

        public override void TakeDamage(float amount)
        {
            if (isInvulnerable || isPhaseTransitioning)
            {
                return;
            }

            if (hasShieldBarrier && activeShields.Count > 0)
            {
                if (gameObject.activeInHierarchy && !isInvulnerable)
                {
                    StartCoroutine(HitFlash());
                }
                return;
            }

            base.TakeDamage(amount);
        }

        protected IEnumerator HitFlash()
        {
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = originalColor;
            }
        }
    }
}
