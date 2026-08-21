using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    public enum FireMode
    {
        SingleStream,
        DoubleStream,
        TripleStream
    }

    /// <summary>
    /// PlayerController handles player movement via touch-drag, screen bounds clamping,
    /// health management, lives/respawn, invincibility i-frames, and automatic projectile firing using BulletPool.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Character Data")]
        [SerializeField] private LanData lanData;

        [Header("Movement Settings")]
        [Tooltip("Relative touch offset (Y-axis) so finger does not cover the character sprite.")]
        [SerializeField] private float fingerVerticalOffset = 0.8f;

        [Tooltip("Smooth time for movement interpolation (0 = instant 1:1, higher = smoother). Set to 0.01f for ultra-responsive 60fps feel.")]
        [Range(0f, 0.1f)]
        [SerializeField] private float smoothTime = 0.01f;

        [Tooltip("Enable relative drag delta mode. If true, player moves by finger delta; if false, player follows finger position + offset.")]
        [SerializeField] private bool useRelativeDeltaDrag = true;

        [Header("Screen Boundary Settings")]
        [Tooltip("Camera used for world bounds calculation. Fallback to Camera.main if null.")]
        [SerializeField] private Camera mainCamera;

        [Tooltip("Manual padding offset from screen edges (X: Left/Right, Y: Top/Bottom). Auto-calculated from SpriteRenderer if 0.")]
        [SerializeField] private Vector2 padding = Vector2.zero;

        [Header("Auto-Firing Settings")]
        [SerializeField] private bool isAutoFiring = true;
        [SerializeField] private FireMode currentFireMode = FireMode.SingleStream;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 12f;
        [SerializeField] private Sprite bulletSprite;

        [Header("Health & Lives System")]
        [SerializeField] private int livesCount = 3;
        [SerializeField] private float currentHp;
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float moveSpeed = 300f;
        [SerializeField] private float currentDamage = 5f;
        [SerializeField] private float fireRate = 5f; // Bullets per second
        [SerializeField] private int shieldCharges = 0;

        [Header("Invincibility i-Frames")]
        [SerializeField] private bool isInvincible = false;
        [SerializeField] private float defaultInvincibilityDuration = 1.5f;

        // Events
        public event System.Action<int, float, float> OnPlayerHealthChanged; // lives, currentHp, maxHp
        public event System.Action OnPlayerDeath;

        // Private variables
        private Vector3 targetWorldPosition;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 lastTouchPosition;
        private bool isDragging = false;
        private float fireTimer = 0f;
        private float invincibilityTimer = 0f;
        private float blinkTimer = 0f;

        // Screen bounds caching
        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;
        private SpriteRenderer spriteRenderer;
        private Collider2D playerCollider;

        public int LivesCount { get => livesCount; set => livesCount = value; }
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float CurrentDamage => currentDamage;
        public bool IsDead => currentHp <= 0 && livesCount <= 0;
        public bool IsInvincible => isInvincible;
        public int ShieldCharges { get => shieldCharges; set => shieldCharges = value; }
        public bool IsAutoFiring { get => isAutoFiring; set => isAutoFiring = value; }
        public FireMode CurrentFireMode { get => currentFireMode; set => currentFireMode = value; }

        // Public getters for testing and UI integration
        public Vector2 MinScreenBounds => minScreenBounds;
        public Vector2 MaxScreenBounds => maxScreenBounds;
        public Camera MainCamera { get => mainCamera; set => mainCamera = value; }
        public Vector3 TargetWorldPosition { get => targetWorldPosition; set => targetWorldPosition = value; }
        public bool IsDragging => isDragging;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            playerCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            InitializeStats();
            CalculateScreenBounds();
            targetWorldPosition = transform.position;
        }

        private void Update()
        {
            HandleInput();
            UpdatePosition();
            HandleInvincibility();

            if (isAutoFiring && !IsDead)
            {
                HandleAutoFiring();
            }
        }

        private void LateUpdate()
        {
            CalculateScreenBounds();
            ClampPosition();
        }

        /// <summary>
        /// Initializes player stats from LanData ScriptableObject if assigned.
        /// </summary>
        public void InitializeStats()
        {
            if (lanData != null)
            {
                maxHp = lanData.maxHp;
                moveSpeed = lanData.moveSpeed;
                currentDamage = lanData.baseDamage;
                fireRate = lanData.fireRate;
            }
            currentHp = maxHp;
            isInvincible = false;
            invincibilityTimer = 0f;
            
            OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
        }

        /// <summary>
        /// Triggers invincibility i-frames with visual sprite blinking.
        /// </summary>
        public void TriggerInvincibility(float duration)
        {
            isInvincible = true;
            invincibilityTimer = duration;
            blinkTimer = 0f;
        }

        private void HandleInvincibility()
        {
            if (!isInvincible) return;

            invincibilityTimer -= Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (spriteRenderer != null)
            {
                // Visual sprite alpha flicker during i-frames
                float alpha = (Mathf.FloorToInt(blinkTimer * 10f) % 2 == 0) ? 0.3f : 1.0f;
                Color col = spriteRenderer.color;
                col.a = alpha;
                spriteRenderer.color = col;
            }

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                invincibilityTimer = 0f;

                if (spriteRenderer != null)
                {
                    Color col = spriteRenderer.color;
                    col.a = 1.0f;
                    spriteRenderer.color = col;
                }
            }
        }

        /// <summary>
        /// Applies damage to player, respecting invincibility and active shield.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (IsDead || isInvincible) return;

            // Check Mooncake Shield
            if (shieldCharges > 0)
            {
                shieldCharges--;
                TriggerInvincibility(0.5f); // Short grace period after shield break
                if (VFX.CameraShake.Instance != null) VFX.CameraShake.Instance.Shake(0.1f, 0.1f);
                return;
            }

            currentHp = Mathf.Max(0f, currentHp - amount);
            OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);

            if (VFX.CameraShake.Instance != null) VFX.CameraShake.Instance.Shake(0.2f, 0.2f);

            if (currentHp <= 0f)
            {
                if (livesCount > 1)
                {
                    Respawn();
                }
                else
                {
                    livesCount = 0;
                    OnDeath();
                }
            }
            else
            {
                TriggerInvincibility(defaultInvincibilityDuration);
            }
        }

        /// <summary>
        /// Respawns player at bottom center with restored HP and invincibility i-frames.
        /// </summary>
        public void Respawn()
        {
            livesCount = Mathf.Max(0, livesCount - 1);
            currentHp = maxHp;

            Vector3 respawnPos = new Vector3(0f, minScreenBounds.y + 1.5f, transform.position.z);
            ForceUpdatePosition(respawnPos);

            TriggerInvincibility(2.0f);
            gameObject.SetActive(true);
            OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
        }

        public void AddLife()
        {
            livesCount++;
            OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
        }

        public void AddShieldCharges(int count)
        {
            shieldCharges += count;
        }

        private void OnDeath()
        {
            if (VFX.VFXManager.Instance != null)
            {
                VFX.VFXManager.Instance.PlayExplosion(transform.position);
            }
            if (VFX.CameraShake.Instance != null)
            {
                VFX.CameraShake.Instance.Shake(0.5f, 0.5f); // Big shake
            }
            
            gameObject.SetActive(false);
            OnPlayerDeath?.Invoke();
        }

        /// <summary>
        /// Handles continuous automatic firing using BulletPool.
        /// </summary>
        private void HandleAutoFiring()
        {
            if (fireRate <= 0f) return;

            fireTimer += Time.deltaTime;
            float fireInterval = 1f / fireRate;

            if (fireTimer >= fireInterval)
            {
                fireTimer -= fireInterval;
                FireProjectiles();
            }
        }

        /// <summary>
        /// Fires projectiles according to active FireMode (Single, Double, Triple stream).
        /// </summary>
        public void FireProjectiles()
        {
            if (BulletPool.Instance == null) return;

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + new Vector3(0, 0.5f, 0);

            switch (currentFireMode)
            {
                case FireMode.SingleStream:
                    BulletPool.Instance.SpawnBullet(origin, Vector2.up, bulletSpeed, currentDamage, true, bulletSprite);
                    break;

                case FireMode.DoubleStream:
                    Vector3 leftOrigin = origin + new Vector3(-0.25f, 0, 0);
                    Vector3 rightOrigin = origin + new Vector3(0.25f, 0, 0);
                    BulletPool.Instance.SpawnBullet(leftOrigin, Vector2.up, bulletSpeed, currentDamage, true, bulletSprite);
                    BulletPool.Instance.SpawnBullet(rightOrigin, Vector2.up, bulletSpeed, currentDamage, true, bulletSprite);
                    break;

                case FireMode.TripleStream:
                    BulletPool.Instance.SpawnBullet(origin, Vector2.up, bulletSpeed, currentDamage, true, bulletSprite);
                    Vector2 leftDir = Quaternion.Euler(0, 0, 15f) * Vector2.up;
                    Vector2 rightDir = Quaternion.Euler(0, 0, -15f) * Vector2.up;
                    BulletPool.Instance.SpawnBullet(origin, leftDir, bulletSpeed, currentDamage, true, bulletSprite);
                    BulletPool.Instance.SpawnBullet(origin, rightDir, bulletSpeed, currentDamage, true, bulletSprite);
                    break;
            }
        }

        public void CalculateScreenBounds()
        {
            if (mainCamera == null) return;

            Vector3 lowerLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 upperRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            Vector2 objectExtent = Vector2.zero;

            if (padding.x > 0 || padding.y > 0)
            {
                objectExtent = padding;
            }
            else if (spriteRenderer != null)
            {
                objectExtent = spriteRenderer.bounds.extents;
            }
            else if (playerCollider != null)
            {
                objectExtent = playerCollider.bounds.extents;
            }

            minScreenBounds = new Vector2(lowerLeft.x + objectExtent.x, lowerLeft.y + objectExtent.y);
            maxScreenBounds = new Vector2(upperRight.x - objectExtent.x, upperRight.y - objectExtent.y);
        }

        public void SimulateDragDelta(Vector3 delta)
        {
            targetWorldPosition += delta;
            UpdatePosition();
            ClampPosition();
        }

        public void ForceUpdatePosition(Vector3 newTargetPos)
        {
            targetWorldPosition = newTargetPos;
            targetWorldPosition.x = Mathf.Clamp(targetWorldPosition.x, minScreenBounds.x, maxScreenBounds.x);
            targetWorldPosition.y = Mathf.Clamp(targetWorldPosition.y, minScreenBounds.y, maxScreenBounds.y);
            transform.position = targetWorldPosition;
        }

        private void HandleInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    lastTouchPosition = GetWorldTouchPosition(touch.position);

                    if (!useRelativeDeltaDrag)
                    {
                        targetWorldPosition = lastTouchPosition + new Vector3(0, fingerVerticalOffset, 0);
                    }
                }
                else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isDragging)
                {
                    Vector3 currentTouchWorldPos = GetWorldTouchPosition(touch.position);

                    if (useRelativeDeltaDrag)
                    {
                        Vector3 delta = currentTouchWorldPos - lastTouchPosition;
                        targetWorldPosition += delta;
                    }
                    else
                    {
                        targetWorldPosition = currentTouchWorldPos + new Vector3(0, fingerVerticalOffset, 0);
                    }

                    lastTouchPosition = currentTouchWorldPos;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastTouchPosition = GetWorldTouchPosition(Input.mousePosition);

                if (!useRelativeDeltaDrag)
                {
                    targetWorldPosition = lastTouchPosition + new Vector3(0, fingerVerticalOffset, 0);
                }
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 currentMouseWorldPos = GetWorldTouchPosition(Input.mousePosition);

                if (useRelativeDeltaDrag)
                {
                    Vector3 delta = currentMouseWorldPos - lastTouchPosition;
                    targetWorldPosition += delta;
                }
                else
                {
                    targetWorldPosition = currentMouseWorldPos + new Vector3(0, fingerVerticalOffset, 0);
                }

                lastTouchPosition = currentMouseWorldPos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }

        private Vector3 GetWorldTouchPosition(Vector3 screenPos)
        {
            if (mainCamera == null) return transform.position;
            screenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        public void UpdatePosition()
        {
            targetWorldPosition.x = Mathf.Clamp(targetWorldPosition.x, minScreenBounds.x, maxScreenBounds.x);
            targetWorldPosition.y = Mathf.Clamp(targetWorldPosition.y, minScreenBounds.y, maxScreenBounds.y);
            targetWorldPosition.z = transform.position.z;

            if (smoothTime > 0f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetWorldPosition, ref currentVelocity, smoothTime);
            }
            else
            {
                transform.position = targetWorldPosition;
            }
        }

        public void ClampPosition()
        {
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, minScreenBounds.x, maxScreenBounds.x);
            clampedPos.y = Mathf.Clamp(clampedPos.y, minScreenBounds.y, maxScreenBounds.y);
            transform.position = clampedPos;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            CalculateScreenBounds();

            Gizmos.color = Color.green;
            Vector3 center = new Vector3((minScreenBounds.x + maxScreenBounds.x) / 2f, (minScreenBounds.y + maxScreenBounds.y) / 2f, transform.position.z);
            Vector3 size = new Vector3(maxScreenBounds.x - minScreenBounds.x, maxScreenBounds.y - minScreenBounds.y, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
