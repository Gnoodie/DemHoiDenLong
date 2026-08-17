using System;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public class BossController : MonoBehaviour, IDamageable
    {
        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        [Header("Boss Stats")]
        [SerializeField] private float maxHp = 1000f;
        [SerializeField] private float currentHp;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float baseDamage = 20f;
        
        [Header("Attack Pattern")]
        [SerializeField] private float fireRate = 1.0f;
        [SerializeField] private int bulletCount = 8;
        [SerializeField] private float bulletSpeed = 5f;
        [SerializeField] private Sprite bulletSprite;
        
        private float hpMultiplier = 1.0f;
        private float damageMultiplier = 1.0f;
        
        private bool isDead = false;
        private bool isEntering = true;
        
        private Vector3 targetPosition;
        private float attackTimer = 0f;
        private Vector2 minScreenBounds;
        private Vector2 maxScreenBounds;

        public float CurrentHp => currentHp;
        public float MaxHp => maxHp * hpMultiplier;
        public bool IsDead => isDead;
        public bool IsEntering => isEntering;

        private void Start()
        {
            if (Camera.main != null)
            {
                Vector3 lowerLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
                Vector3 upperRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
                minScreenBounds = lowerLeft;
                maxScreenBounds = upperRight;
                
                targetPosition = new Vector3(0, maxScreenBounds.y - 2f, 0); // Upper middle of screen
            }
            else
            {
                targetPosition = new Vector3(0, 3f, 0);
            }
        }

        public void InitializeStats(float hpMult, float dmgMult)
        {
            hpMultiplier = hpMult;
            damageMultiplier = dmgMult;
            currentHp = MaxHp;
            isDead = false;
            isEntering = true;
            OnHealthChanged?.Invoke(currentHp, MaxHp);
        }

        private void Update()
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

        private void HandleAttackPattern()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / fireRate)
            {
                attackTimer = 0f;
                FireCircularPattern();
            }
        }

        private void FireCircularPattern()
        {
            if (BulletPool.Instance == null) return;

            float angleStep = 360f / bulletCount;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down; // down is default
                BulletPool.Instance.SpawnBullet(transform.position, direction, bulletSpeed, baseDamage * damageMultiplier, false, bulletSprite);
            }
        }

        public void TakeDamage(float amount)
        {
            if (isDead) return;

            currentHp = Mathf.Max(0, currentHp - amount);
            OnHealthChanged?.Invoke(currentHp, MaxHp);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            currentHp = Mathf.Min(MaxHp, currentHp + amount);
            OnHealthChanged?.Invoke(currentHp, MaxHp);
        }

        private void Die()
        {
            isDead = true;
            OnDeath?.Invoke();
            
            // Clear bullets
            if (BulletPool.Instance != null)
            {
                BulletPool.Instance.ClearAllActiveBullets();
            }
            
            gameObject.SetActive(false);
        }
    }
}
