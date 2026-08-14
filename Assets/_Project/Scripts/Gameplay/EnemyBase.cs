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
        [Header("Enemy Data Configuration")]
        [SerializeField] protected EnemyData enemyData;

        [Header("Runtime Stats")]
        [SerializeField] protected float currentHp;
        [SerializeField] protected float maxHp = 10f;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected int starReward = 1;

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
        public bool IsDead => currentHp <= 0;

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
            }
            else
            {
                maxHp *= hpMult;
                moveSpeed *= speedMult;
                damage *= dmgMult;
            }

            currentHp = maxHp;
            gameObject.SetActive(true);
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            aliveTimer += Time.deltaTime;
            Move();
            CheckScreenBounds();
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
                    float newY = transform.position.y - (moveSpeed * Time.deltaTime);
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
            if (IsDead) return;

            currentHp = Mathf.Max(0f, currentHp - amount);
            if (IsDead)
            {
                OnDeath();
            }
        }

        public virtual void Heal(float amount)
        {
            if (IsDead) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
        }

        protected virtual void OnDeath()
        {
            // Award stars/currency to player
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
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                Despawn();
            }
        }
    }
}
