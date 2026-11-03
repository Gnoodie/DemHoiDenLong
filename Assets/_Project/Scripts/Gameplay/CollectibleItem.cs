using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public enum ItemType
    {
        Star,
        Heal,
        Shield,
        Magnet,
        Stun,
        CoinX2
    }

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CollectibleItem : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private float fallSpeed = 3f;
        [SerializeField] private float magnetSpeed = 15f;
        [SerializeField] private float value = 1f; // Heal amount or Star value

        private bool isMagnetized = false;
        private Transform playerTransform;

        public ItemType Type => itemType;
        public float Value => value;

        public void Initialize(ItemType type, float amount)
        {
            this.itemType = type;
            this.value = amount;
            isMagnetized = false;
            
            // Randomize fall speed slightly for variation
            fallSpeed = Random.Range(2f, 4f);
        }

        private void Update()
        {
            if (isMagnetized && playerTransform != null)
            {
                // Fly towards player
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
            }
            else
            {
                // Fall down
                transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

                // Magnet check: If player has magnet active, pull it in
                if (PlayerController.Instance != null && PlayerController.Instance.IsMagnetActive)
                {
                    isMagnetized = true;
                    playerTransform = PlayerController.Instance.transform;
                }
            }

            // Despawn if it falls out of screen (assuming y < -10 is out of bounds)
            if (transform.position.y < -15f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.CollectItem(this);
                
                // Play sound or particle effect if any
                if (VFX.VFXManager.Instance != null && itemType != ItemType.Star)
                {
                    // Generic small poof or sparkle for powerups
                    VFX.VFXManager.Instance.PlayExplosion(transform.position); 
                }

                Destroy(gameObject);
            }
        }
    }
}
