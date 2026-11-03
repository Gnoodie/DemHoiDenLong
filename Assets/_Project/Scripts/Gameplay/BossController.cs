using System;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public class BossController : MonoBehaviour, IDamageable
    {
        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        [Header("Boss Stats")]
        [SerializeField] protected float maxHp = 1000f;
        [SerializeField] protected float currentHp;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float baseDamage = 20f;
        
        [Header("Attack Pattern")]
        [SerializeField] protected float fireRate = 1.0f;
        [SerializeField] protected int bulletCount = 8;
        [SerializeField] protected float bulletSpeed = 5f;
        [SerializeField] protected Sprite bulletSprite;
        
        protected float hpMultiplier = 1.0f;
        protected float damageMultiplier = 1.0f;
        
        protected int currentPhase = 1;
        
        protected bool isDead = false;
        protected bool isEntering = true;
        
        protected Vector3 targetPosition;
        protected float attackTimer = 0f;
        protected Vector2 minScreenBounds;
        protected Vector2 maxScreenBounds;

        protected bool isInvulnerable = false;
        protected bool isPhaseTransitioning = false;
        public string BossDisplayName { get; set; }
        public bool IsInvulnerable => isInvulnerable;
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp * hpMultiplier;
        public bool IsDead => isDead;
        public bool IsEntering => isEntering;
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

        private Coroutine slowRoutine;
        private float originalMoveSpeed = -1f;

        public void ApplySlow(float slowPercent, float duration = 3.0f)
        {
            if (originalMoveSpeed < 0f) originalMoveSpeed = moveSpeed;
            if (slowRoutine != null) StopCoroutine(slowRoutine);
            slowRoutine = StartCoroutine(Routine_SlowBoss(slowPercent, duration));
        }

        public void RemoveSlow()
        {
            if (slowRoutine != null)
            {
                StopCoroutine(slowRoutine);
                slowRoutine = null;
            }
            if (originalMoveSpeed >= 0f)
            {
                moveSpeed = originalMoveSpeed;
                originalMoveSpeed = -1f;
            }
        }

        private System.Collections.IEnumerator Routine_SlowBoss(float slowPercent, float duration)
        {
            moveSpeed = originalMoveSpeed * Mathf.Clamp01(1f - slowPercent);
            yield return new WaitForSeconds(duration);
            moveSpeed = originalMoveSpeed;
            originalMoveSpeed = -1f;
            slowRoutine = null;
        }
        
        protected SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            if (Camera.main != null)
            {
                Vector3 lowerLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
                Vector3 upperRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                minScreenBounds = lowerLeft;
                maxScreenBounds = upperRight;
                
                targetPosition = new Vector3(0, maxScreenBounds.y - 2f, transform.position.z); // Upper middle of screen
            }
            else
            {
                targetPosition = new Vector3(0, 3f, transform.position.z);
            }
        }

        public void InitializeStats(float hpMult, float dmgMult)
        {
            hpMultiplier = hpMult;
            damageMultiplier = dmgMult;
            currentHp = MaxHp;
            isDead = false;
            isEntering = true;
            currentPhase = 1;
            OnHealthChanged?.Invoke(currentHp, MaxHp);
            
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlayBossAppear();
                Audio.AudioManager.Instance.PlayBGM(Audio.AudioManager.Instance.bgmBossFight);
            }
        }

        /// <summary>
        /// Nạp chỉ số Boss trực tiếp từ LevelData theo GDD (màn 5: 500, màn 10: 800, màn 15: 1200).
        /// Không nhân hệ số độ khó của màn.
        /// </summary>
        public void InitializeFromData(float hp, float speed, float damage)
        {
            if (hp > 0f) maxHp = hp;
            if (speed > 0f) moveSpeed = speed;
            if (damage > 0f) baseDamage = damage;

            hpMultiplier = 1.0f;
            damageMultiplier = 1.0f;
            currentHp = maxHp;
            isDead = false;
            isEntering = true;
            currentPhase = 1;
            OnHealthChanged?.Invoke(currentHp, maxHp);

            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlayBossAppear();
                Audio.AudioManager.Instance.PlayBGM(Audio.AudioManager.Instance.bgmBossFight);
            }
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (isEntering)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    isEntering = false;
                }
            }
            else
            {
                HandleAttackPattern();
            }
        }

        protected virtual void HandleAttackPattern()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / fireRate)
            {
                attackTimer = 0f;
                FireCircularPattern();
            }
        }

        protected void FireCircularPattern()
        {
            if (BulletPool.Instance == null) return;

            Audio.AudioManager.Instance?.PlayBossShoot();

            float angleStep = 360f / bulletCount;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down; // down is default
                BulletPool.Instance.SpawnBullet(transform.position, direction, bulletSpeed, baseDamage * damageMultiplier, false, bulletSprite);
            }
        }

        public virtual void TakeDamage(float amount)
        {
            if (isDead || isInvulnerable || isPhaseTransitioning) return;

            // Damage Cap: Tối đa 15% MaxHp cho 1 phát trúng để chặn hoàn toàn sốc sát thương bốc hơi
            float maxDamageAllowed = MaxHp * 0.15f;
            if (amount > maxDamageAllowed)
            {
                amount = maxDamageAllowed;
            }

            // Phase Gate: Chặn không cho sát thương tụt xuyên qua ngưỡng Phase tiếp theo
            float minHpAllowed = 0f;
            if (currentPhase == 1) minHpAllowed = MaxHp * 0.66f;
            else if (currentPhase == 2) minHpAllowed = MaxHp * 0.33f;

            currentHp -= amount;

            // Kẹp máu tại ngưỡng chuyển Phase và kích hoạt chuyển Phase an toàn
            if (currentPhase == 1 && currentHp <= minHpAllowed)
            {
                currentHp = minHpAllowed;
                OnHealthChanged?.Invoke(currentHp, MaxHp);
                currentPhase = 2;
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(HitFlash());
                }
                OnPhaseChanged(2);
                return;
            }
            else if (currentPhase == 2 && currentHp <= minHpAllowed)
            {
                currentHp = minHpAllowed;
                OnHealthChanged?.Invoke(currentHp, MaxHp);
                currentPhase = 3;
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(HitFlash());
                }
                OnPhaseChanged(3);
                return;
            }

            currentHp = Mathf.Max(0, currentHp);
            OnHealthChanged?.Invoke(currentHp, MaxHp);

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HitFlash());
            }

            if (currentHp <= 0)
            {
                Die();
            }
        }

        private void CheckPhaseChange()
        {
            // Được quản lý trực tiếp bằng Phase Gate trong TakeDamage
        }

        protected virtual void OnPhaseChanged(int newPhase)
        {
            // Override in derived classes
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            currentHp = Mathf.Min(MaxHp, currentHp + amount);
            OnHealthChanged?.Invoke(currentHp, MaxHp);
        }

        private System.Collections.IEnumerator HitFlash()
        {
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            isDead = true;
            Audio.AudioManager.Instance?.PlayBossDeath();

            // Cấp sao Boss theo Hồi (màn 5 = 50, màn 10 = 80, màn 15 = 150) cộng thẳng vào runStars
            int levelId = LevelSession.level != null ? LevelSession.level.levelId : 5;
            int bossStars = GameplayBalance.GetBossStarReward(levelId);
            LevelSession.runStars += bossStars;
            LevelSession.RecordStarDropped(bossStars);
            LevelSession.RecordStarCollected(bossStars);
            PlayerController.NotifyRunStarsChanged(LevelSession.runStars);
            Debug.Log($"[BossController] Trùm bị tiêu diệt! Thưởng +{bossStars} Sao trực tiếp vào runStars (Tổng: {LevelSession.runStars})");

            OnDeath?.Invoke();
            
            // Nếu không có WaveManager điều phối luồng thì fallback gọi TriggerVictory
            if (WaveManager.Instance == null && GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory("BossKill");
            }

            if (VFX.VFXManager.Instance != null)
            {
                VFX.VFXManager.Instance.PlayExplosion(transform.position);
            }
            if (VFX.CameraShake.Instance != null)
            {
                VFX.CameraShake.Instance.Shake(0.5f, 0.5f); // Bigger shake for boss
            }
            
            // Clear bullets
            if (BulletPool.Instance != null)
            {
                BulletPool.Instance.ClearAllActiveBullets();
            }
            
            gameObject.SetActive(false);
        }
    }
}
