using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// PlayerController handles player movement via touch-drag, screen bounds clamping,
    /// health management, and stat initialization for the "Đêm Hội Đèn Lồng" game.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField] private LanData lanData;

        [Header("Movement Settings")]
        [Tooltip("Relative touch offset (Y-axis) so finger does not cover the character sprite.")]
        [SerializeField] private float fingerVerticalOffset = 0.8f;

        [Tooltip("Smooth time for movement interpolation (0 = instant 1:1, higher = smoother). Set to 0.02f for ultra-responsive 60fps feel.")]
        [Range(0f, 0.1f)]
        [SerializeField] private float smoothTime = 0.01f;

        [Tooltip("Enable relative drag delta mode. If true, player moves by finger delta; if false, player follows finger position + offset.")]
        [SerializeField] private bool useRelativeDeltaDrag = true;

        [Header("Screen Boundary Settings")]
        [Tooltip("Camera used for world bounds calculation. Fallback to Camera.main if null.")]
        [SerializeField] private Camera mainCamera;

        [Tooltip("Manual padding offset from screen edges (X: Left/Right, Y: Top/Bottom). Auto-calculated from SpriteRenderer if 0.")]
        [SerializeField] private Vector2 padding = Vector2.zero;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHp;
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float moveSpeed = 300f;
        [SerializeField] private float currentDamage = 5f;
        [SerializeField] private float fireRate = 5f;

        // Private movement variables
        private Vector3 targetWorldPosition;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 lastTouchPosition;
        private bool isDragging = false;

        // Screen bounds caching
        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;
        private SpriteRenderer spriteRenderer;
        private Collider2D playerCollider;

        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public bool IsDead => currentHp <= 0;

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
        }

        private void LateUpdate()
        {
            // Recalculate bounds in case screen orientation or camera size changes
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
        }

        /// <summary>
        /// Calculates world space boundaries based on Camera orthographic view and Sprite/Collider size.
        /// </summary>
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

        /// <summary>
        /// Simulates touch/mouse drag movement delta (useful for unit testing and automated AI player input).
        /// </summary>
        public void SimulateDragDelta(Vector3 delta)
        {
            targetWorldPosition += delta;
            UpdatePosition();
            ClampPosition();
        }

        /// <summary>
        /// Directly forces updating and clamping transform position to target position (instant move).
        /// </summary>
        public void ForceUpdatePosition(Vector3 newTargetPos)
        {
            targetWorldPosition = newTargetPos;
            targetWorldPosition.x = Mathf.Clamp(targetWorldPosition.x, minScreenBounds.x, maxScreenBounds.x);
            targetWorldPosition.y = Mathf.Clamp(targetWorldPosition.y, minScreenBounds.y, maxScreenBounds.y);
            transform.position = targetWorldPosition;
        }

        /// <summary>
        /// Handles touch and mouse input across mobile and Unity Editor platforms.
        /// </summary>
        private void HandleInput()
        {
            // Mobile Touch Input
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
            // PC / Unity Editor Mouse Input Fallback
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

        /// <summary>
        /// Converts screen pixel position into 2D world coordinates.
        /// </summary>
        private Vector3 GetWorldTouchPosition(Vector3 screenPos)
        {
            if (mainCamera == null) return transform.position;
            screenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        /// <summary>
        /// Smoothly updates transform position towards target location.
        /// </summary>
        public void UpdatePosition()
        {
            // Clamp target position first to avoid target drifting off screen
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

        /// <summary>
        /// Enforces strict screen bounds clamping on character transform.
        /// </summary>
        public void ClampPosition()
        {
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, minScreenBounds.x, maxScreenBounds.x);
            clampedPos.y = Mathf.Clamp(clampedPos.y, minScreenBounds.y, maxScreenBounds.y);
            transform.position = clampedPos;
        }

        /// <summary>
        /// Applies damage to player.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            currentHp = Mathf.Max(0f, currentHp - amount);
            if (IsDead)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// Heals player.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
        }

        private void OnDeath()
        {
            // Trigger death sequence / event notification
            gameObject.SetActive(false);
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
