using UnityEngine;
using UnityEngine.InputSystem;
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
        public static PlayerController Instance { get; private set; }

        [Header("Character Data")]
        [SerializeField] private LanData lanData;
        [SerializeField] private LanConfig lanConfig; // Actual runtime data asset (1.asset, 2.asset...)

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

        [Header("Tilt Effect")]
        [Tooltip("Sprite khi Lân nghiêng trái (vuốt sang trái).")]
        [SerializeField] private Sprite tiltLeftSprite;
        [Tooltip("Sprite khi Lân nghiêng phải (vuốt sang phải).")]
        [SerializeField] private Sprite tiltRightSprite;
        [Tooltip("Ngưỡng tốc độ (world units/s) để kích hoạt nghiêng. Giảm nếu không thấy hiệu ứng.")]
        [SerializeField] private float tiltSpeedThreshold = 0.5f;
        private Sprite defaultSprite;

        [Header("Skill Anchors (5A.7)")]
        [Tooltip("Phần miệng và phía trước khuôn mặt Lân - tia phun/sóng/pháo hoa phát từ đây")]
        [SerializeField] private Transform mouthPoint;
        [Tooltip("Tâm của thân Lân - khiên và hiệu ứng thân gắn vào đây")]
        [SerializeField] private Transform centerPoint;

        public Transform MouthPoint => mouthPoint != null ? mouthPoint : (firePoint != null ? firePoint : transform);
        public Transform CenterPoint => centerPoint != null ? centerPoint : transform;
        public Vector3 MouthPosition => MouthPoint != null ? MouthPoint.position : transform.position;
        public Vector3 CenterPosition => CenterPoint != null ? CenterPoint.position : transform.position;

        // Skill Offset Y & Invincibility (5A.7 Kim Lan lướt)
        private float skillOffsetY = 0f;
        public float SkillOffsetY { get => skillOffsetY; set => skillOffsetY = value; }
        private bool isSkillInvincible = false;
        public bool IsSkillInvincible => isSkillInvincible;
        public void SetSkillInvincible(bool active) => isSkillInvincible = active;

        // Events
        public event System.Action<int, float, float> OnPlayerHealthChanged; // lives, currentHp, maxHp
        public event System.Action<float> OnSkillCooldownChanged;
        public event System.Action OnPlayerDeath;
        public static event System.Action<int> OnRunStarsChanged;

        public static void NotifyRunStarsChanged(int stars)
        {
            OnRunStarsChanged?.Invoke(stars);
        }

        // Private variables
        private Vector3 targetWorldPosition;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 lastTouchPosition;
        private bool isDragging = false;
        private float fireTimer = 0f;
        private float invincibilityTimer = 0f;
        private float blinkTimer = 0f;
        private float lastTapTime = 0f;
        private const float DOUBLE_TAP_THRESHOLD = 0.3f;
        private PlayerSkillController skillController;
        private Vector3 previousPosition;
        private float smoothedVelocityX = 0f;

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
        public bool CanShoot { get; set; } = true;
        public FireMode CurrentFireMode { get => currentFireMode; set => currentFireMode = value; }

        // Public getters for testing and UI integration
        public Vector2 MinScreenBounds => minScreenBounds;
        public Vector2 MaxScreenBounds => maxScreenBounds;
        public Camera MainCamera { get => mainCamera; set => mainCamera = value; }
        public Vector3 TargetWorldPosition { get => targetWorldPosition; set => targetWorldPosition = value; }
        public bool IsDragging => isDragging;
        public float SkillSpeedMultiplier { get; set; } = 1.0f;
        public bool IsKimDashing { get; set; } = false;
#pragma warning disable 0414
        private int activeDragTouchId = -1;
#pragma warning restore 0414
        public bool IsMagnetActive { get; private set; }
        public bool IsCoinX2Active { get; private set; }
        public LanConfig RuntimeLanConfig => lanConfig;
        public LanData RuntimeLanData => lanData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                defaultSprite = spriteRenderer.sprite;
            }
            playerCollider = GetComponent<Collider2D>();
            
            skillController = GetComponent<PlayerSkillController>();
            if (skillController == null)
            {
                skillController = gameObject.AddComponent<PlayerSkillController>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void ClearInstance()
        {
            Instance = null;
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // Lan & Stats duoc nap tu LevelSession (giai doan 5A)
            if (LevelSession.lan != null)
            {
                lanData = LevelSession.lan;
            }

            InitializeStats();
            CalculateScreenBounds();
            targetWorldPosition = transform.position;
            previousPosition = transform.position;
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.CurrentState != GameState.Playing || GameManager.Instance.CurrentRunState == RunState.Won || GameManager.Instance.CurrentRunState == RunState.Finalized)) return;

            HandleInput();
            UpdatePosition();
            HandleInvincibility();

            if (isAutoFiring && !IsDead)
            {
                HandleAutoFiring();
            }

            if (skillController != null && skillController.IsSkillReady())
            {
                OnSkillCooldownChanged?.Invoke(0f);
            }
            else if (skillController != null)
            {
                OnSkillCooldownChanged?.Invoke(skillController.GetCooldownFillAmount());
            }
        }

        private void LateUpdate()
        {
            CalculateScreenBounds();
            ClampPosition();
        }

        /// <summary>
        /// Initializes player stats from LanConfig (runtime asset) or LanData ScriptableObject.
        /// LanConfig is the actual source used by 1.asset–4.asset; LanData is fallback.
        /// </summary>
        public void InitializeStats()
        {
            livesCount = DemHoiDenLong.Data.LevelRules.MaxHearts;

            if (LevelSession.stats != null)
            {
                maxHp = LevelSession.stats.hp;
                currentDamage = LevelSession.stats.damage;
                fireRate = LevelSession.stats.fireRate;

                if (LevelSession.lan != null)
                {
                    moveSpeed = LevelSession.lan.moveSpeed;
                }
                string lanId = LevelSession.lan != null ? LevelSession.lan.lanId : "1";
                int capHp = Mathf.Clamp(PlayerPrefs.GetInt($"UpgradeLevel_{lanId}_HP", 1) - 1, 0, 10);
                Debug.Log($"[STATS] Gameplay lan={lanId} level={capHp} hp={maxHp} dmg={currentDamage} fr={fireRate}");
                Debug.Log($"[PlayerController] Loaded stats from LevelSession: HP={maxHp}, Speed={moveSpeed}, Damage={currentDamage}, FireRate={fireRate}, Lives={livesCount}");
            }
            else if (lanConfig != null)
            {
                maxHp        = lanConfig.BaseHp;
                moveSpeed    = lanConfig.BaseSpeed;
                currentDamage = lanConfig.BaseDamage;
                fireRate     = lanConfig.BaseFireRate;

                Debug.Log($"[PlayerController] Loaded stats from LanConfig: HP={maxHp}, Speed={moveSpeed}, Damage={currentDamage}, FireRate={fireRate}");
            }
            else if (lanData != null)
            {
                maxHp        = lanData.maxHp;
                moveSpeed    = lanData.moveSpeed;
                currentDamage = lanData.baseDamage;
                fireRate     = lanData.fireRate;
                
                Debug.Log($"[PlayerController] Loaded stats from LanData: HP={maxHp}, Speed={moveSpeed}, Damage={currentDamage}, FireRate={fireRate}");
            }
            else
            {
                Debug.LogWarning($"[PlayerController] No LanConfig or LanData assigned! Using default values: HP={maxHp}");
            }

            currentHp = maxHp; // Always reset current HP to max on initialization
            isInvincible = false;
            invincibilityTimer = 0f;

            OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
        }

        public void SetRuntimeLanDataForDebug(LanData newLan)
        {
            lanData = newLan;
            lanConfig = null;
            if (spriteRenderer != null && newLan != null && newLan.sprite != null)
            {
                spriteRenderer.sprite = newLan.sprite;
            }
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
            if (IsDead || isInvincible || isSkillInvincible) return;

            // Check Mooncake Shield
            if (shieldCharges > 0)
            {
                shieldCharges--;
                TriggerInvincibility(0.5f); // Short grace period after shield break
                if (VFX.CameraShake.Instance != null) VFX.CameraShake.Instance.Shake(0.1f, 0.1f);
                Audio.AudioManager.Instance?.PlayShieldBreak();
                return;
            }

            currentHp = Mathf.Max(0f, currentHp - amount);
            Audio.AudioManager.Instance?.PlayPlayerHit();

            if (VFX.CameraShake.Instance != null) VFX.CameraShake.Instance.Shake(0.2f, 0.2f);

            if (currentHp <= 0f)
            {
                if (livesCount > 1)
                {
                    // Trừ mạng trong Respawn() và sẽ gọi OnPlayerHealthChanged ở đó
                    Respawn();
                }
                else
                {
                    livesCount = 0;
                    OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
                    OnDeath();
                }
            }
            else
            {
                OnPlayerHealthChanged?.Invoke(livesCount, currentHp, maxHp);
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

        /// <summary>
        /// Hoi sinh nguoi choi (quang cao): cap dung so tim (mac dinh 1), day thanh mau va 2 giay i-frames.
        /// </summary>
        public void Revive(int lives = 1)
        {
            livesCount = Mathf.Max(1, lives);
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

        public void CollectItem(CollectibleItem item)
        {
            switch (item.Type)
            {
                case ItemType.Star:
                    int starAmount = Mathf.RoundToInt(IsCoinX2Active ? item.Value * 2f : item.Value);
                    LevelSession.runStars += starAmount;
                    LevelSession.RecordStarCollected(starAmount);
                    OnRunStarsChanged?.Invoke(LevelSession.runStars);
                    Audio.AudioManager.Instance?.PlayPickupCoin();
                    break;
                case ItemType.Heal:
                    Heal(item.Value);
                    Audio.AudioManager.Instance?.PlayPickupHeal();
                    break;
                case ItemType.Shield:
                    AddShieldCharges((int)item.Value);
                    Audio.AudioManager.Instance?.PlayPickupPowerup();
                    break;
                case ItemType.Magnet:
                    StartCoroutine(MagnetRoutine());
                    Audio.AudioManager.Instance?.PlayPickupPowerup();
                    break;
                case ItemType.Stun:
                    if (EnemySpawner.Instance != null)
                    {
                        EnemySpawner.Instance.StunAllEnemies(3.0f); // Stun for 3 seconds
                    }
                    if (VFX.CameraShake.Instance != null)
                    {
                        VFX.CameraShake.Instance.Shake(0.3f, 0.4f);
                    }
                    Audio.AudioManager.Instance?.PlayPickupPowerup();
                    break;
                case ItemType.CoinX2:
                    StartCoroutine(CoinX2Routine());
                    Audio.AudioManager.Instance?.PlayPickupPowerup();
                    break;
            }
        }

        private System.Collections.IEnumerator MagnetRoutine()
        {
            IsMagnetActive = true;
            yield return new WaitForSeconds(10f); // Magnet lasts 10 seconds
            IsMagnetActive = false;
        }

        private System.Collections.IEnumerator CoinX2Routine()
        {
            IsCoinX2Active = true;
            yield return new WaitForSeconds(10f); // CoinX2 lasts 10 seconds
            IsCoinX2Active = false;
        }

        /// <summary>
        /// Handles continuous automatic firing using BulletPool.
        /// </summary>
        private void HandleAutoFiring()
        {
            if (!CanShoot || fireRate <= 0f) return;

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

            Audio.AudioManager.Instance?.PlayPlayerShoot();

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

        public void TriggerSkillFromButton()
        {
            ActivateSkill();
        }

        private void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ActivateSkill();
            }
#else
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ActivateSkill();
            }
#endif
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Pointer.current != null)
            {
                var pointer = UnityEngine.InputSystem.Pointer.current;
                bool isPressing = pointer.press.isPressed;
                bool wasPressedThisFrame = pointer.press.wasPressedThisFrame;
                bool wasReleasedThisFrame = pointer.press.wasReleasedThisFrame;
                Vector3 pointerPos = pointer.position.ReadValue();

                if (wasPressedThisFrame)
                {
                    // Block input if clicking on UI
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        isDragging = false;
                        return;
                    }

                    float timeSinceLastTap = Time.time - lastTapTime;
                    if (timeSinceLastTap <= DOUBLE_TAP_THRESHOLD)
                    {
                        ActivateSkill();
                    }
                    lastTapTime = Time.time;

                    isDragging = true;
                    lastTouchPosition = GetWorldTouchPosition(pointerPos);
                    
                    if (!useRelativeDeltaDrag)
                    {
                        targetWorldPosition = lastTouchPosition + new Vector3(0, fingerVerticalOffset, 0);
                    }
                }
                else if (isPressing && isDragging)
                {
                    Vector3 currentTouchWorldPos = GetWorldTouchPosition(pointerPos);

                    if (useRelativeDeltaDrag)
                    {
                        Vector3 delta = (currentTouchWorldPos - lastTouchPosition) * SkillSpeedMultiplier;
                        targetWorldPosition += delta;
                    }
                    else
                    {
                        targetWorldPosition = currentTouchWorldPos + new Vector3(0, fingerVerticalOffset, 0);
                    }

                    lastTouchPosition = currentTouchWorldPos;
                }
                else if (wasReleasedThisFrame)
                {
                    isDragging = false;
                }
            }
#else
            if (Input.touchCount > 0)
            {
                // Multi-touch isolation: finger tapping UI does not pull the ship
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    bool overUI = UnityEngine.EventSystems.EventSystem.current != null && 
                                  UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);

                    if (touch.phase == TouchPhase.Began)
                    {
                        if (overUI) continue;
                        if (activeDragTouchId == -1)
                        {
                            activeDragTouchId = touch.fingerId;
                            isDragging = true;
                            lastTouchPosition = GetWorldTouchPosition(touch.position);
                            if (!useRelativeDeltaDrag)
                            {
                                targetWorldPosition = lastTouchPosition + new Vector3(0, fingerVerticalOffset, 0);
                            }
                        }
                    }
                    else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && touch.fingerId == activeDragTouchId)
                    {
                        Vector3 currentTouchWorldPos = GetWorldTouchPosition(touch.position);
                        if (useRelativeDeltaDrag)
                        {
                            Vector3 delta = (currentTouchWorldPos - lastTouchPosition) * SkillSpeedMultiplier;
                            targetWorldPosition += delta;
                        }
                        else
                        {
                            targetWorldPosition = currentTouchWorldPos + new Vector3(0, fingerVerticalOffset, 0);
                        }
                        lastTouchPosition = currentTouchWorldPos;
                    }
                    else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && touch.fingerId == activeDragTouchId)
                    {
                        activeDragTouchId = -1;
                        isDragging = false;
                    }
                }
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Block input if clicking on UI
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    isDragging = false;
                    return;
                }

                float timeSinceLastTap = Time.time - lastTapTime;
                if (timeSinceLastTap <= DOUBLE_TAP_THRESHOLD)
                {
                    ActivateSkill();
                }
                lastTapTime = Time.time;

                isDragging = true;
                lastTouchPosition = GetWorldTouchPosition(Input.mousePosition);
                
                if (!useRelativeDeltaDrag)
                {
                    targetWorldPosition = lastTouchPosition + new Vector3(0, fingerVerticalOffset, 0);
                }
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 currentTouchWorldPos = GetWorldTouchPosition(Input.mousePosition);

                if (useRelativeDeltaDrag)
                {
                    Vector3 delta = (currentTouchWorldPos - lastTouchPosition) * SkillSpeedMultiplier;
                    targetWorldPosition += delta;
                }
                else
                {
                    targetWorldPosition = currentTouchWorldPos + new Vector3(0, fingerVerticalOffset, 0);
                }

                lastTouchPosition = currentTouchWorldPos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
#endif
        }

        private void ActivateSkill()
        {
            if (skillController != null)
            {
                if (lanConfig != null)
                {
                    Debug.Log("[PlayerController] Double tap detected! Activating skill (LanConfig)...");
                    skillController.ActivateSkill(lanConfig);
                }
                else if (lanData != null)
                {
                    Debug.Log("[PlayerController] Double tap detected! Activating skill (LanData)...");
                    skillController.ActivateSkill(lanData);
                }
                else
                {
                    Debug.LogWarning("[PlayerController] Cannot activate skill! Both LanConfig and LanData are null.");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerController] SkillController is null!");
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
            Vector3 finalTargetPos = targetWorldPosition;
            finalTargetPos.y += skillOffsetY;

            finalTargetPos.x = Mathf.Clamp(finalTargetPos.x, minScreenBounds.x, maxScreenBounds.x);
            finalTargetPos.y = Mathf.Clamp(finalTargetPos.y, minScreenBounds.y, maxScreenBounds.y);
            finalTargetPos.z = transform.position.z;

            if (smoothTime > 0f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, finalTargetPos, ref currentVelocity, smoothTime);
            }
            else
            {
                transform.position = finalTargetPos;
            }

            // Tính velocity thực tế từ độ dịch chuyển vị trí, không dùng currentVelocity của SmoothDamp
            float rawVelocityX = (transform.position.x - previousPosition.x) / Mathf.Max(Time.deltaTime, 0.0001f);
            // Làm mượt thêm một lớp để loại bỏ spike 1-frame
            smoothedVelocityX = Mathf.Lerp(smoothedVelocityX, rawVelocityX, 0.5f);
            previousPosition = transform.position;

            HandleTiltEffect(smoothedVelocityX);
        }

        private void HandleTiltEffect(float velocityX)
        {
            if (spriteRenderer == null) return;

            // Use LanConfig/LanData as override if set; otherwise fall back to inspector sprites
            Sprite tLeft = (lanConfig != null && lanConfig.TiltLeftSprite != null) ? lanConfig.TiltLeftSprite
                         : (lanData   != null && lanData.tiltLeftSprite   != null) ? lanData.tiltLeftSprite
                         : tiltLeftSprite;
            Sprite tRight = (lanConfig != null && lanConfig.TiltRightSprite != null) ? lanConfig.TiltRightSprite
                          : (lanData   != null && lanData.tiltRightSprite  != null) ? lanData.tiltRightSprite
                          : tiltRightSprite;

            // Hysteresis: ngưỡng vào cao hơn ngưỡng thoát → tránh dao động quanh mốc duy nhất
            float enterThreshold = tiltSpeedThreshold;
            float exitThreshold = tiltSpeedThreshold * 0.4f;

            Sprite targetSprite = spriteRenderer.sprite;

            if (velocityX > enterThreshold && tRight != null)
            {
                targetSprite = tRight;
            }
            else if (velocityX < -enterThreshold && tLeft != null)
            {
                targetSprite = tLeft;
            }
            else if (Mathf.Abs(velocityX) < exitThreshold)
            {
                targetSprite = defaultSprite;
            }
            // else: giữ nguyên sprite hiện tại (đang trong vùng đệm) — không đổi

            if (targetSprite != null && spriteRenderer.sprite != targetSprite)
            {
                spriteRenderer.sprite = targetSprite;
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
            Vector3 center = new Vector3((minScreenBounds.x + maxScreenBounds.x) / 2, (minScreenBounds.y + maxScreenBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxScreenBounds.x - minScreenBounds.x, maxScreenBounds.y - minScreenBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
#endif

        private void OnDrawGizmos()
        {
            if (mouthPoint != null)
            {
                Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
                Gizmos.DrawWireSphere(mouthPoint.position, 0.1f);
                Gizmos.DrawRay(mouthPoint.position, Vector3.up * 0.4f);
            }
            if (centerPoint != null)
            {
                Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
                Gizmos.DrawWireSphere(centerPoint.position, 0.08f);
            }
        }
    }
}
