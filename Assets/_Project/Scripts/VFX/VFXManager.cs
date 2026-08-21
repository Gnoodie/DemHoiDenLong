using UnityEngine;
using System.Collections.Generic;

namespace DemHoiDenLong.VFX
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject explosionPrefab;

        [Header("Settings")]
        [SerializeField] private int poolSize = 10;

        private Queue<GameObject> explosionPool = new Queue<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            if (explosionPrefab == null) return;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(explosionPrefab, transform);
                obj.SetActive(false);
                explosionPool.Enqueue(obj);
            }
        }

        public void PlayExplosion(Vector3 position)
        {
            if (explosionPrefab == null || explosionPool.Count == 0)
            {
                if (explosionPrefab != null)
                {
                    // If pool is empty, expand it by instantiating one
                    GameObject newExplosion = Instantiate(explosionPrefab, transform);
                    explosionPool.Enqueue(newExplosion);
                }
                else return;
            }

            GameObject explosion = explosionPool.Dequeue();
            explosion.transform.position = position;
            explosion.SetActive(true);
            
            // Assume the explosion prefab has a script or ParticleSystem that auto-disables it,
            // or we disable it after a delay
            StartCoroutine(ReturnExplosionToPool(explosion, 1.5f));
        }

        private System.Collections.IEnumerator ReturnExplosionToPool(GameObject explosion, float delay)
        {
            yield return new WaitForSeconds(delay);
            explosion.SetActive(false);
            explosionPool.Enqueue(explosion);
        }

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
#endif
    }
}
