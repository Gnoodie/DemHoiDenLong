using System.Collections;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public class BossAct2 : BossController
    {
        [Header("Dragon Segments")]
        [SerializeField] private int segmentCount = 3;
        [SerializeField] private float segmentSpacing = 0.5f;
        
        [Header("Movement Settings")]
        [SerializeField] private float sinFrequency = 1.5f;
        [SerializeField] private float sinAmplitude = 1.0f;
        [SerializeField] private float phase3FrequencyMultiplier = 1.4f;
        
        [Header("Fire Breath Attack")]
        [SerializeField] private GameObject fireBreathPrefab;
        [SerializeField] private float fireBreathWarningDuration = 1.0f;
        [SerializeField] private float fireBreathCooldown = 4.5f;
        [SerializeField] private Color fireBreathWarningColor = Color.red;
        
        private GameObject[] bodySegments;
        private bool[] segmentActiveShooters;
        private float[] segmentFireTimers;
        private float baseY;
        private float breathCooldownTimer = 0f;
        private bool isBreathingFire = false;

        protected override void Start()
        {
            base.Start();

            // Chuẩn hóa kích thước Rồng Đèn vừa vặn màn hình dọc (~52% chiều rộng màn hình)
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                float screenWidth = Camera.main != null ? Camera.main.orthographicSize * 2f * Camera.main.aspect : 5.625f;
                float targetWidth = screenWidth * 0.52f;
                float currentWidth = spriteRenderer.bounds.size.x;
                if (currentWidth > 0.01f)
                {
                    float factor = targetWidth / currentWidth;
                    transform.localScale *= factor;
                }
            }

            float topY = Camera.main != null ? Camera.main.orthographicSize : 5.0f;
            baseY = topY - 2.5f;
            targetPosition = new Vector3(0, baseY, transform.position.z);
            bulletSpeed = 6.0f;
            fireRate = 1.1f;

            CreateBodySegments();
        }

        private void CreateBodySegments()
        {
            bodySegments = new GameObject[segmentCount];
            segmentActiveShooters = new bool[segmentCount];
            segmentFireTimers = new float[segmentCount];

            Vector2[] segmentOffsets = new Vector2[]
            {
                new Vector2(0.45f, -0.35f),
                new Vector2(-0.35f, -0.7f),
                new Vector2(0.25f, -1.05f)
            };

            for (int i = 0; i < segmentCount; i++)
            {
                GameObject segment = new GameObject($"DragonSegment_{i}");
                segment.transform.SetParent(transform);
                Vector2 offset = i < segmentOffsets.Length ? segmentOffsets[i] : new Vector2(0f, -0.4f * (i + 1));
                segment.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
                
                bodySegments[i] = segment;
                segmentActiveShooters[i] = false;
                segmentFireTimers[i] = i * 0.35f;
            }
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
                HandleSinWaveMovement();
                HandleAttackPattern();
                
                if (currentPhase >= 2)
                {
                    HandleSegmentShooting();
                }

                if (currentPhase >= 3 && !isBreathingFire)
                {
                    breathCooldownTimer += Time.deltaTime;
                    if (breathCooldownTimer >= fireBreathCooldown)
                    {
                        breathCooldownTimer = 0f;
                        StartCoroutine(FireBreathAttack());
                    }
                }
            }
        }

        private void HandleSinWaveMovement()
        {
            float x = Mathf.Sin(Time.time * sinFrequency) * sinAmplitude;
            transform.position = new Vector3(x, baseY, 0);
        }

        protected override void HandleAttackPattern()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / fireRate)
            {
                attackTimer = 0f;
                StartCoroutine(Routine_BurstAimFire(3));
            }
        }

        private IEnumerator Routine_BurstAimFire(int burstCount)
        {
            for (int i = 0; i < burstCount; i++)
            {
                if (isDead || BulletPool.Instance == null) yield break;

                PlayerController player = FindAnyObjectByType<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    Vector2 direction = (player.transform.position - transform.position).normalized;
                    BulletPool.Instance.SpawnBullet(transform.position, direction, bulletSpeed, baseDamage * damageMultiplier, false, bulletSprite);
                }

                yield return new WaitForSeconds(0.12f);
            }
        }

        private void HandleSegmentShooting()
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (!segmentActiveShooters[i]) continue;

                segmentFireTimers[i] += Time.deltaTime;
                if (segmentFireTimers[i] >= 1f / (fireRate * 0.7f))
                {
                    segmentFireTimers[i] = 0f;
                    FireSegmentBullet(i);
                }
            }
        }

        private void FireSegmentBullet(int segmentIndex)
        {
            if (BulletPool.Instance == null || segmentIndex >= bodySegments.Length) return;
            if (bodySegments[segmentIndex] == null) return;

            Vector3 segmentPos = bodySegments[segmentIndex].transform.position;
            
            // Các đốt thân bắn đan chéo chữ X tạo lưới lửa
            float angleOffset = (segmentIndex % 2 == 0) ? -18f : 18f;
            if (segmentIndex == 2) angleOffset = 0f;

            Vector2 direction = Quaternion.Euler(0, 0, angleOffset) * Vector2.down;
            BulletPool.Instance.SpawnBullet(segmentPos, direction, bulletSpeed * 0.85f, baseDamage * damageMultiplier * 0.8f, false, bulletSprite);
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
                Debug.Log("Boss Act 2 - Phase 2: Kích hoạt các đốt thân bắn chéo!");
                ActivateAllSegments();
                bulletSpeed = 6.5f;
            }
            else if (newPhase == 3)
            {
                Debug.Log("Boss Act 2 - Phase 3: Cuồng nộ tăng tốc lượn + Phun lửa định kỳ!");
                sinFrequency *= phase3FrequencyMultiplier;
                fireRate *= 1.35f;
                bulletSpeed = 7.0f;
                breathCooldownTimer = fireBreathCooldown; // Kích hoạt ngay đợt phun lửa đầu tiên!
            }
        }

        private void ActivateAllSegments()
        {
            for (int i = 0; i < segmentCount; i++)
            {
                segmentActiveShooters[i] = true;
            }
        }

        private IEnumerator FireBreathAttack()
        {
            isBreathingFire = true;
            
            // Cảnh báo: Ánh sáng đỏ rực
            Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float warningElapsed = 0f;
            
            while (warningElapsed < fireBreathWarningDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(originalColor, fireBreathWarningColor, Mathf.PingPong(warningElapsed * 4f, 1f));
                }
                yield return null;
                warningElapsed += Time.deltaTime;
            }
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // Quét 11 tia lửa hình quạt phủ rộng
            Debug.Log("Boss Act 2: Fire Breath Sweep!");
            if (BulletPool.Instance != null)
            {
                for (int i = -5; i <= 5; i++)
                {
                    Vector2 direction = new Vector2(i * 0.18f, -1f).normalized;
                    BulletPool.Instance.SpawnBullet(transform.position, direction, bulletSpeed * 1.35f, baseDamage * damageMultiplier * 1.5f, false, bulletSprite);
                }
            }

            yield return new WaitForSeconds(0.5f);
            isBreathingFire = false;
        }

        public override void TakeDamage(float amount)
        {
            if (isInvulnerable || isPhaseTransitioning)
            {
                return;
            }

            base.TakeDamage(amount);
        }
    }
}
