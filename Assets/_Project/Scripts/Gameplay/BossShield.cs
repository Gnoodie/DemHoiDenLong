using System;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public class BossShield : MonoBehaviour, IDamageable
    {
        public event Action<BossShield> OnShieldDestroyed;
        
        [SerializeField] private float maxHp = 50f;
        private float currentHp;
        
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public bool IsDead => currentHp <= 0;
        
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            currentHp = maxHp;
        }

        public void TakeDamage(float amount)
        {
            currentHp -= amount;
            
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HitFlash());
            }

            if (currentHp <= 0)
            {
                OnShieldDestroyed?.Invoke(this);
                if (VFX.VFXManager.Instance != null)
                {
                    VFX.VFXManager.Instance.PlayExplosion(transform.position);
                }
                Destroy(gameObject);
            }
        }

        public void Heal(float amount)
        {
            // Shields usually don't heal
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
    }
}
