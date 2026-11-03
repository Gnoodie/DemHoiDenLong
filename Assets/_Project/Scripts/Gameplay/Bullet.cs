using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Bullet handles individual projectile movement, lifetime, damage payload,
    /// and auto-recycling back to BulletPool upon screen exit or collision.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float damage = 5f;
        [SerializeField] private float speed = 1000f; // px/s or world units/s
        [SerializeField] private Vector2 direction = Vector2.up;
        [SerializeField] private bool isPlayerBullet = true;
        [SerializeField] private float maxLifetime = 5f;

        private float lifetimeTimer = 0f;
        private SpriteRenderer spriteRenderer;
        private Collider2D bulletCollider;
        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;
        private Sprite defaultSprite; // Store original sprite to reset pooled bullets
        private Vector3 defaultScale; // Store original scale to reset pooled bullets

        public float Damage => damage;
        public float Speed => speed;
        public Vector2 Direction => direction;
        public bool IsPlayerBullet => isPlayerBullet;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            bulletCollider = GetComponent<Collider2D>();
            if (bulletCollider != null)
            {
                bulletCollider.isTrigger = true;
            }
            
            // Store default sprite and scale to reset pooled bullets
            defaultScale = transform.localScale;
            if (spriteRenderer != null)
            {
                defaultSprite = spriteRenderer.sprite;
            }
        }

        /// <summary>
        /// Initializes projectile parameters when spawned from BulletPool.
        /// </summary>
        public void Initialize(Vector3 spawnPosition, Vector2 moveDirection, float moveSpeed, float bulletDamage, bool fromPlayer, Vector2 screenMin, Vector2 screenMax, Sprite customSprite = null, float scale = 1f)
        {
            transform.position = spawnPosition;
            direction = moveDirection.normalized;
            speed = moveSpeed;
            damage = bulletDamage;
            isPlayerBullet = fromPlayer;
            minScreenBounds = screenMin;
            maxScreenBounds = screenMax;
            lifetimeTimer = 0f;

            // Always reset sprite and scale to prevent pooled-bullet pollution
            transform.localScale = defaultScale * scale;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = customSprite != null ? customSprite : defaultSprite;
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                // Sprite đạn địch (bullet_enemy) được vẽ chúc xuống (-Y), bù 180 độ để đầu đạn hướng đúng chiều bay
                if (!isPlayerBullet)
                {
                    angle += 180f;
                }
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            gameObject.SetActive(true);
        }

        private void Update()
        {
            Move();
            CheckBoundsAndLifetime();
        }

        private void Move()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
        }

        private void CheckBoundsAndLifetime()
        {
            lifetimeTimer += Time.deltaTime;

            // Recycles bullet if it travels beyond screen bounds or exceeds max lifetime
            Vector3 pos = transform.position;

            // Add margin around screen bounds so bullet fully exits before despawning
            float margin = 1.0f;
            if (pos.x < minScreenBounds.x - margin || pos.x > maxScreenBounds.x + margin ||
                pos.y < minScreenBounds.y - margin || pos.y > maxScreenBounds.y + margin ||
                lifetimeTimer >= maxLifetime)
            {
                Recycle();
            }
        }

        public void Recycle()
        {
            gameObject.SetActive(false);
            if (BulletPool.Instance != null)
            {
                BulletPool.Instance.ReturnBullet(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player bullets damage Enemies/Boss; Enemy bullets damage Player
            if (isPlayerBullet)
            {
                var target = other.GetComponentInParent<IDamageable>();
                // Make sure we don't shoot ourselves
                if (target != null && !(target is PlayerController))
                {
                    CollisionHandler.ProcessPlayerBulletHit(this, target);
                }
            }
            else
            {
                var player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    bool hitProcessed = CollisionHandler.ProcessDamageToPlayer(player, damage);
                    if (hitProcessed)
                    {
                        Recycle();
                    }
                }
            }
        }
    }
}
