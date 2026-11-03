using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Mảnh vỡ đèn gai - nổ ra 8 mảnh tỏa đều 360 độ khi đèn gai bị tiêu diệt.
    /// HP = 5, Damage = 5, tốc độ = 3.5 đơn vị/giây, kéo theo vệt ngắn màu cam đỏ.
    /// Không tính kills, không rơi sao, không nổ tiếp, tự hủy khi ra ngoài màn hình.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SpikedLanternDebris : EnemyBase
    {
        private Vector2 flyDirection = Vector2.down;
        private TrailRenderer trail;

        public override EnemyKind Kind => EnemyKind.SpikeDebris;

        protected override void Awake()
        {
            base.Awake();
            maxHp = 5f;
            currentHp = 5f;
            damage = 5f;
            moveSpeed = 4.8f;
            starReward = 0;
            canShoot = false;

            trail = GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
                trail.time = 0.25f;
                trail.startWidth = 0.18f;
                trail.endWidth = 0f;
                trail.material = new Material(Shader.Find("Sprites/Default"));
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.35f, 0f), 0f), new GradientColorKey(new Color(1f, 0.6f, 0f), 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                trail.colorGradient = grad;
            }
        }

        public void InitializeDebris(Vector3 spawnPos, Vector2 dir, Sprite spr, float scale = 0.6f)
        {
            InitializeDebris(spawnPos, dir, spr, Vector2.zero, Vector2.zero, scale);
        }

        public void InitializeDebris(Vector3 spawnPos, Vector2 dir, Sprite spr, Vector2 screenMin, Vector2 screenMax, float scale = 0.6f)
        {
            transform.position = spawnPos;
            flyDirection = dir.normalized;
            transform.localScale = new Vector3(scale, scale, 1f);

            minScreenBounds = screenMin;
            maxScreenBounds = screenMax;
            if (minScreenBounds == Vector2.zero && maxScreenBounds == Vector2.zero)
            {
                if (Camera.main != null)
                {
                    minScreenBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
                    maxScreenBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                }
                else
                {
                    minScreenBounds = new Vector2(-10f, -10f);
                    maxScreenBounds = new Vector2(10f, 10f);
                }
            }

            // Xoay hướng mũi gai theo chiều bay ra ngoài
            if (flyDirection.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (spr != null) spriteRenderer.sprite = spr;
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 15; // Nổi bật trên nền, mây và đạn thường
            }

            if (trail != null)
            {
                trail.Clear();
            }

            maxHp = 5f;
            currentHp = 5f;
            damage = 5f;
            moveSpeed = 4.8f;
            starReward = 0;
            canShoot = false;
            isDeadProcessed = false;

            if (enemyCollider != null)
            {
                enemyCollider.enabled = true;
            }

            gameObject.SetActive(true);
        }

        protected override void Move()
        {
            transform.position += (Vector3)flyDirection * (moveSpeed * Time.deltaTime);
        }

        protected override void CheckScreenBounds()
        {
            float margin = 2.0f;
            if (transform.position.y < minScreenBounds.y - margin ||
                transform.position.y > maxScreenBounds.y + margin ||
                transform.position.x < minScreenBounds.x - margin ||
                transform.position.x > maxScreenBounds.x + margin)
            {
                Despawn();
            }
        }

        public override void Die(KillCause cause)
        {
            if (isDeadProcessed) return;
            isDeadProcessed = true;
            currentHp = 0f;

            // Đăng ký kill (ShouldCountKill trả false cho SpikeDebris -> không tăng bộ đếm)
            LevelSession.RegisterKill(EnemyKind.SpikeDebris, cause);

            // Không nổ mảnh tiếp, không rơi sao
            if (cause != KillCause.CollisionWithPlayer && cause != KillCause.ScreenExit)
            {
                Audio.AudioManager.Instance?.PlayEnemyHit();
            }

            Despawn();
        }

        public override void Despawn()
        {
            SpikedLanternEnemy.ReturnToPool(this);
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (IsDead) return;

            PlayerController p = other.GetComponentInParent<PlayerController>();
            if (p != null)
            {
                p.TakeDamage(damage);
                Die(KillCause.CollisionWithPlayer);
            }
        }
    }
}
