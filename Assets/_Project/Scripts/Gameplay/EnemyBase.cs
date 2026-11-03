using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// EnemyBase serves as the base class for all enemy types (paper lantern, spiked lantern, bosses).
    /// Handles movement patterns, health tracking, level difficulty scaling, and collision damage.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        public static event System.Action<EnemyBase> OnEnemyDied;
        [Header("Enemy Data Configuration")]
        [SerializeField] protected EnemyData enemyData;

        [Header("Runtime Stats")]
        [SerializeField] protected float currentHp;
        [SerializeField] protected float maxHp = 10f;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected int starReward = 1;

        [Header("Shooting Stats")]
        [SerializeField] protected bool canShoot = false;
        [SerializeField] protected float fireRate = 2f;
        [SerializeField] protected float bulletSpeed = 300f;
        [SerializeField] protected Sprite bulletSprite;
        [SerializeField] protected float bulletScale = 1f;
        protected float shootTimer = 0f;

        [Header("Movement Control")]
        [SerializeField] protected MovementPattern movementPattern = MovementPattern.StraightDown;
        [SerializeField] protected float sinFrequency = 2f;
        [SerializeField] protected float sinAmplitude = 1.5f;

        protected Vector3 spawnPosition;
        protected float aliveTimer = 0f;
        protected SpriteRenderer spriteRenderer;
        protected Collider2D enemyCollider;
        protected Vector2 minScreenBounds;
        protected Vector2 maxScreenBounds;

        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public float Damage => damage;
        public int StarReward => starReward;
        public void SetStarReward(int reward) { starReward = reward; }
        public bool IsDead => currentHp <= 0 || isDeadProcessed;

        protected bool isDeadProcessed = false;
        protected bool isStunned = false;
        protected float stunTimer = 0f;
        private static int killTraceCount = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            OnEnemyDied = null;
            killTraceCount = 0;
        }

        public virtual EnemyKind Kind
        {
            get
            {
                if (this is PaperLanternEnemy) return EnemyKind.PaperLantern;
                if (this is SpikedLanternEnemy) return EnemyKind.SpikedLantern;
                string n = gameObject.name.ToLower();
                if (n.Contains("ca") || n.Contains("carp")) return EnemyKind.CarpLantern;
                if (n.Contains("ma") || n.Contains("quy") || n.Contains("ghost")) return EnemyKind.GhostLantern;
                return EnemyKind.PaperLantern;
            }
        }

        protected virtual void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            enemyCollider = GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                enemyCollider.isTrigger = true;
            }
        }

        /// <summary>
        /// Parameterless initialization for tests and pool resets.
        /// </summary>
        public virtual void Initialize()
        {
            Initialize(Vector3.zero, new Vector2(-10f, -10f), new Vector2(10f, 10f), 1f, 1f, 1f);
        }

        /// <summary>
        /// Initializes enemy instance with position, level difficulty scaling multipliers, and screen bounds.
        /// </summary>
        public virtual void Initialize(Vector3 spawnPos, Vector2 screenMin, Vector2 screenMax, float hpMult = 1.0f, float speedMult = 1.0f, float dmgMult = 1.0f)
        {
            spawnPosition = spawnPos;
            transform.position = spawnPos;
            minScreenBounds = screenMin;
            maxScreenBounds = screenMax;
            aliveTimer = 0f;

            if (enemyData != null)
            {
                maxHp = enemyData.maxHp * hpMult;
                moveSpeed = enemyData.moveSpeed * speedMult;
                damage = enemyData.damage * dmgMult;
                starReward = enemyData.starReward;
                movementPattern = enemyData.movementPattern;
                sinFrequency = enemyData.sinFrequency;
                sinAmplitude = enemyData.sinAmplitude;

                if (spriteRenderer != null && enemyData.sprite != null)
                {
                    spriteRenderer.sprite = enemyData.sprite;
                }

                canShoot = enemyData.canShoot;
                fireRate = enemyData.fireRate;
                bulletSpeed = enemyData.bulletSpeed;
                bulletSprite = enemyData.bulletSprite;
                bulletScale = enemyData.bulletScale;
            }
            else
            {
                maxHp *= hpMult;
                moveSpeed *= speedMult;
                damage *= dmgMult;
            }

            currentHp = maxHp;

            // Tinh chỉnh Đèn Quỷ (Ghost Lantern): Tăng tốc độ bay, bắn nhắm dồn dập và tốc độ đạn
            bool isGhost = (enemyData != null && (enemyData.enemyId.Contains("quy") || enemyData.enemyName.Contains("quỷ") || enemyData.name.Contains("Quy"))) 
                           || gameObject.name.ToLower().Contains("quy") 
                           || gameObject.name.ToLower().Contains("ghost");
            if (isGhost)
            {
                moveSpeed = 1.35f * speedMult; // Di chuyển chậm vừa phải để có thời gian bắn nhiều đợt
                canShoot = true;
                fireRate = 1.4f;               // 1.4s bắn 1 phát (bắn được 4-5 lần trong vòng đời)
                bulletSpeed = 6.0f;             // Đạn bay nhanh, dứt khoát
            }

            // Tinh chỉnh Đèn Giấy (Paper Lantern): Tăng lượng máu cơ bản để chịu 2-3 viên đạn
            bool isPaper = (enemyData != null && (enemyData.enemyId.Contains("giay") || enemyData.enemyName.Contains("giấy")))
                           || gameObject.name.ToLower().Contains("giay");
            if (isPaper)
            {
                maxHp = Mathf.Max(maxHp, 25f * hpMult);
                currentHp = maxHp;
            }

            shootTimer = fireRate / 2f; // Shoot halfway into the first cycle so they don't all fire at exactly 0.0s
            isStunned = false;
            stunTimer = 0f;
            isDeadProcessed = false;
            gameObject.SetActive(true);
        }

        public void ApplyStun(float duration)
        {
            isStunned = true;
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            if (isStunned)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0f)
                {
                    isStunned = false;
                }
                else
                {
                    return; // Skip Move and Shoot
                }
            }

            aliveTimer += Time.deltaTime;
            Move();
            CheckScreenBounds();
            CheckShooting();
        }

        /// <summary>
        /// Moves enemy according to its movement pattern (StraightDown, SinWave, etc.).
        /// </summary>
        protected virtual void Move()
        {
            switch (movementPattern)
            {
                case MovementPattern.StraightDown:
                    transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
                    break;

                case MovementPattern.SinWave:
                    // Đèn Cá Chép (Carp Lantern): Quẫy đuôi lướt nhanh bất ngờ khi đi qua giữa màn hình (aliveTimer 1.2s - 2.2s)
                    float carpDashMult = 1.0f;
                    bool isCarp = (enemyData != null && (enemyData.enemyId.Contains("ca") || enemyData.enemyName.Contains("cá"))) 
                                  || gameObject.name.ToLower().Contains("ca");
                    if (isCarp && aliveTimer >= 1.2f && aliveTimer <= 2.2f)
                    {
                        carpDashMult = 1.7f;
                    }

                    float newY = transform.position.y - (moveSpeed * carpDashMult * Time.deltaTime);
                    float newX = spawnPosition.x + Mathf.Sin(aliveTimer * sinFrequency) * sinAmplitude;
                    transform.position = new Vector3(newX, newY, transform.position.z);
                    break;

                case MovementPattern.Zigzag:
                    float zigzagX = spawnPosition.x + Mathf.PingPong(aliveTimer * moveSpeed, sinAmplitude * 2f) - sinAmplitude;
                    float zigzagY = transform.position.y - (moveSpeed * Time.deltaTime);
                    transform.position = new Vector3(zigzagX, zigzagY, transform.position.z);
                    break;
            }
        }

        /// <summary>
        /// Handles weapon cooldowns and triggers firing if the enemy is allowed to shoot.
        /// </summary>
        protected virtual void CheckShooting()
        {
            if (!canShoot) return;

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                shootTimer = fireRate;
                FireBullet();
            }
        }

        /// <summary>
        /// Spawns a bullet heading straight down. Can be overridden by Bosses for complex patterns.
        /// </summary>
        protected virtual void FireBullet()
        {
            if (BulletPool.Instance == null) return;

            Vector2 direction = Vector2.down;

            // Đèn Quỷ: Bắn nhắm thẳng vào vị trí Lân của người chơi
            bool isGhost = (enemyData != null && (enemyData.enemyId.Contains("quy") || enemyData.enemyName.Contains("quỷ") || enemyData.name.Contains("Quy"))) 
                           || gameObject.name.ToLower().Contains("quy") 
                           || gameObject.name.ToLower().Contains("ghost");
            if (isGhost)
            {
                var player = Object.FindAnyObjectByType<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                }
            }

            BulletPool.Instance.SpawnBullet(
                transform.position,
                direction,
                bulletSpeed,
                damage,
                false,
                bulletSprite,
                bulletScale
            );
        }

        /// <summary>
        /// Recycles or deactivates enemy when moving below bottom screen boundary.
        /// </summary>
        protected virtual void CheckScreenBounds()
        {
            float margin = 1.0f;
            if (transform.position.y < minScreenBounds.y - margin)
            {
                Despawn();
            }
        }

        public virtual void TakeDamage(float amount)
        {
            TakeDamage(amount, KillCause.PlayerBullet);
        }

        public virtual void TakeDamage(float amount, KillCause cause)
        {
            if (IsDead) return;

            currentHp = Mathf.Max(0f, currentHp - amount);
            
            if (gameObject.activeInHierarchy)
            {
                Audio.AudioManager.Instance?.PlayEnemyHit();
                StartCoroutine(HitFlash());
            }

            if (IsDead)
            {
                Die(cause);
            }
        }

        /// <summary>
        /// Đường chết DUY NHẤT của EnemyBase: mọi nguyên nhân đều đi qua hàm này.
        /// Gọi RegisterKill(Kind, cause) vào nguồn sự thật LevelSession.
        /// </summary>
        public virtual void Die(KillCause cause)
        {
            if (isDeadProcessed) return;
            isDeadProcessed = true;
            currentHp = 0f;

            bool willCount = LevelSession.ShouldCountKill(Kind, cause);
            if (killTraceCount < 5)
            {
                killTraceCount++;
                Debug.Log($"[KILLTRACE] EnemyBase.Die: enemy={gameObject.name}, type={GetType().Name}, kind={Kind}, cause={cause}, willCount={willCount}");
            }

            LevelSession.RegisterKill(Kind, cause);
            OnEnemyDied?.Invoke(this);

            // Địch chết do tông người chơi: KHÔNG rơi sao và KHÔNG hiệu ứng chết nổ
            if (cause != KillCause.CollisionWithPlayer && cause != KillCause.ScreenExit)
            {
                Audio.AudioManager.Instance?.PlayEnemyDeath();

                if (VFX.VFXManager.Instance != null)
                {
                    VFX.VFXManager.Instance.PlayExplosion(transform.position);
                }
                if (VFX.CameraShake.Instance != null)
                {
                    VFX.CameraShake.Instance.Shake(0.1f, 0.2f);
                }

                // Award stars/currency and drop powerups
                if (PowerUpManager.Instance != null)
                {
                    PowerUpManager.Instance.TryDropItems(transform.position, starReward);
                }
            }

            Despawn();
        }

        protected virtual System.Collections.IEnumerator HitFlash()
        {
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = originalColor;
            }
        }

        private Coroutine slowCoroutine;

        public virtual void ApplySlow(float slowPercent, float duration)
        {
            if (slowCoroutine != null) StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(Routine_ApplySlow(slowPercent, duration));
        }

        protected virtual System.Collections.IEnumerator Routine_ApplySlow(float slowPercent, float duration)
        {
            float originalSpeed = moveSpeed;
            moveSpeed *= Mathf.Clamp01(1f - slowPercent);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.7f, 0.9f, 1f, 1f);
            }

            yield return new WaitForSeconds(duration);

            moveSpeed = originalSpeed;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
            slowCoroutine = null;
        }

        public virtual void Heal(float amount)
        {
            if (IsDead) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
        }

        protected virtual void OnDeath()
        {
            Die(KillCause.PlayerBullet);
        }

                /// <summary>
        /// Smoothly fades out and scales down remaining enemies on victory without counting kills or dropping stars.
        /// </summary>
        public void FadeAndDespawnOnVictory()
        {
            if (isDeadProcessed) return;
            isDeadProcessed = true;

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(VictoryDespawnRoutine());
            }
            else
            {
                Despawn();
            }
        }

        private System.Collections.IEnumerator VictoryDespawnRoutine()
        {
            if (enemyCollider != null) enemyCollider.enabled = false;
            Vector3 origScale = transform.localScale;
            Color origColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(origScale, Vector3.zero, t);
                if (spriteRenderer != null)
                {
                    Color c = origColor;
                    c.a = Mathf.Lerp(origColor.a, 0f, t);
                    spriteRenderer.color = c;
                }
                yield return null;
            }

            transform.localScale = origScale;
            if (spriteRenderer != null) spriteRenderer.color = origColor;
            if (enemyCollider != null) enemyCollider.enabled = true;
            Despawn();
        }

        public virtual void Despawn()
        {
            gameObject.SetActive(false);
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.RecycleEnemy(this);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // Collision with Player
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Kim Lan dang luot bat tu: dich cham vao khong gay sat thuong va khong chet
                if (player.IsKimDashing) return;

                bool hitProcessed = CollisionHandler.ProcessDamageToPlayer(player, damage);
                if (hitProcessed)
                {
                    // Địch tông vào người chơi rồi chết: KHÔNG đếm và KHÔNG rơi sao
                    Die(KillCause.CollisionWithPlayer);
                }
            }
        }
    }
}
