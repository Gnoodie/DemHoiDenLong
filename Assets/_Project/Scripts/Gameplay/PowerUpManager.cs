using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    [System.Serializable]
    public class PowerUpDropData
    {
        public string name;
        public GameObject prefab;
        [Tooltip("Higher weight = higher chance to drop")]
        public float weight = 10f;
    }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Drop Rates")]
        [Tooltip("Percentage chance to drop a power-up (0 to 100)")]
        [SerializeField] private float powerUpDropChance = 5f;
        [Tooltip("Percentage chance to drop a star (0 to 100)")]
        [SerializeField] private float starDropChance = 60f;

        [Header("Prefabs")]
        [SerializeField] private GameObject starPrefab;
        [Tooltip("Configure power-ups and their drop weights here.")]
        [SerializeField] private PowerUpDropData[] powerUpDrops;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void TryDropItems(Vector3 position, int starValue)
        {
            bool itemDropped = false;
            // Try drop Star (Theo GameplayBalance: StarDropChance = 1.0f tức 100%, sheet "Địch - Đèn lồng")
            float dropChancePercent = GameplayBalance.StarDropChance >= 0f ? GameplayBalance.StarDropChance * 100f : starDropChance;
            if (starPrefab != null && Random.Range(0f, 100f) <= dropChancePercent)
            {
                int finalStarVal = Mathf.Max(1, Mathf.RoundToInt(starValue * GameplayBalance.StarValueMultiplier));
                GameObject starObj = Instantiate(starPrefab, position, Quaternion.identity);
                CollectibleItem item = starObj.GetComponent<CollectibleItem>();
                if (item != null)
                {
                    item.Initialize(ItemType.Star, finalStarVal);
                    LevelSession.RecordStarDropped(finalStarVal);
                    itemDropped = true;
                }
            }

            // Try drop PowerUp
            if (powerUpDrops != null && powerUpDrops.Length > 0)
            {
                if (Random.Range(0f, 100f) <= powerUpDropChance)
                {
                    GameObject puPrefab = GetRandomWeightedPowerUp();
                    
                    if (puPrefab != null)
                    {
                        // Add a small offset so they don't overlap exactly if both drop
                        Vector3 dropPos = position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.2f), 0);
                        Instantiate(puPrefab, dropPos, Quaternion.identity);
                        itemDropped = true;
                    }
                }
            }

            if (itemDropped)
            {
                Audio.AudioManager.Instance?.PlayItemDrop();
            }
        }

        private GameObject GetRandomWeightedPowerUp()
        {
            float totalWeight = 0f;
            foreach (var drop in powerUpDrops)
            {
                totalWeight += drop.weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentSum = 0f;

            foreach (var drop in powerUpDrops)
            {
                currentSum += drop.weight;
                if (randomValue <= currentSum)
                {
                    return drop.prefab;
                }
            }

            return null;
        }

        public void ForceDropRandomPowerUp(Vector3 position)
        {
            if (powerUpDrops != null && powerUpDrops.Length > 0)
            {
                GameObject puPrefab = GetRandomWeightedPowerUp();
                if (puPrefab != null)
                {
                    Instantiate(puPrefab, position, Quaternion.identity);
                }
            }
        }

        public void ForceDropStar(Vector3 position, int starValue)
        {
            if (starPrefab != null)
            {
                int finalStarVal = Mathf.Max(1, Mathf.RoundToInt(starValue * GameplayBalance.StarValueMultiplier));
                GameObject starObj = Instantiate(starPrefab, position, Quaternion.identity);
                CollectibleItem item = starObj.GetComponent<CollectibleItem>();
                if (item != null)
                {
                    item.Initialize(ItemType.Star, finalStarVal);
                    LevelSession.RecordStarDropped(finalStarVal);
                }
            }
        }
    }
}
