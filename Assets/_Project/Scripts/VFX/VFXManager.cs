using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DemHoiDenLong.VFX
{
    /// <summary>
    /// Quản lý 2 loại VFX dùng frame-by-frame animation từ sprite tĩnh:
    ///   - Hit   : 4 frame, kích hoạt khi đạn chạm địch
    ///   - Explosion : 6 frame, kích hoạt khi địch chết
    /// Object Pooling ngăn GC spike; pool tự mở rộng khi cần.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager _instance;
        public static VFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Tự tìm trong scene trước
                    _instance = FindAnyObjectByType<VFXManager>();

                    // Nếu không có, tự tạo GameObject mới
                    if (_instance == null)
                    {
                        var go = new GameObject("VFXManager [Auto]");
                        _instance = go.AddComponent<VFXManager>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // ── Hit VFX ──────────────────────────────────────────────
        [Header("Hit VFX (4 frames — khi đạn trúng địch)")]
        [Tooltip("Kéo 4 sprite hit vào đây theo thứ tự frame 0→3")]
        [SerializeField] private Sprite[] hitFrames;
        [SerializeField] private int hitPoolSize = 12;

        // ── Explosion VFX ─────────────────────────────────────────
        [Header("Explosion VFX (6 frames — khi địch chết)")]
        [Tooltip("Kéo 6 sprite explosion vào đây theo thứ tự frame 0→5")]
        [SerializeField] private Sprite[] explosionFrames;
        [SerializeField] private int explosionPoolSize = 8;

        // ── Animation Settings ────────────────────────────────────
        [Header("Animation Settings")]
        [Tooltip("Số frame mỗi giây (FPS) của animation VFX")]
        [SerializeField] private float fps = 24f;

        // ── Internal ──────────────────────────────────────────────
        private Queue<SpriteRenderer> hitPool       = new Queue<SpriteRenderer>();
        private Queue<SpriteRenderer> explosionPool = new Queue<SpriteRenderer>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                
                // Auto-load sprites from Resources if not assigned
                if (hitFrames == null || hitFrames.Length == 0)
                {
                    hitFrames = Resources.LoadAll<Sprite>("VFX/Hit");
                }
                if (explosionFrames == null || explosionFrames.Length == 0)
                {
                    explosionFrames = Resources.LoadAll<Sprite>("VFX/Explosion");
                }

                BuildPool(ref hitPool,       "HitVFX",       hitPoolSize);
                BuildPool(ref explosionPool, "ExplosionVFX", explosionPoolSize);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Phát hiệu ứng va chạm tại vị trí chỉ định.</summary>
        public void PlayHit(Vector3 position)
        {
            if (hitFrames == null || hitFrames.Length == 0) return;
            SpriteRenderer sr = GetFromPool(ref hitPool, "HitVFX");
            sr.transform.position = position;
            sr.gameObject.SetActive(true);
            StartCoroutine(PlayFrames(sr, hitFrames, hitPool));
        }

        /// <summary>Phát hiệu ứng nổ tại vị trí chỉ định.</summary>
        public void PlayExplosion(Vector3 position)
        {
            if (explosionFrames == null || explosionFrames.Length == 0) return;
            SpriteRenderer sr = GetFromPool(ref explosionPool, "ExplosionVFX");
            sr.transform.position = position;
            sr.gameObject.SetActive(true);
            StartCoroutine(PlayFrames(sr, explosionFrames, explosionPool));
        }

        // ── Private helpers ───────────────────────────────────────

        private void BuildPool(ref Queue<SpriteRenderer> pool, string label, int size)
        {
            for (int i = 0; i < size; i++)
                pool.Enqueue(CreateVFXObject(label));
        }

        private SpriteRenderer CreateVFXObject(string label)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform);
            go.SetActive(false);
            return go.AddComponent<SpriteRenderer>();
        }

        private SpriteRenderer GetFromPool(ref Queue<SpriteRenderer> pool, string label)
        {
            // ponytail: pool mở rộng thay vì block — đơn giản hơn size cố định
            if (pool.Count == 0)
                pool.Enqueue(CreateVFXObject(label));

            return pool.Dequeue();
        }

        private IEnumerator PlayFrames(SpriteRenderer sr, Sprite[] frames, Queue<SpriteRenderer> returnPool)
        {
            float frameDuration = 1f / Mathf.Max(fps, 1f);

            foreach (Sprite frame in frames)
            {
                if (sr == null) yield break;
                sr.sprite = frame;
                yield return new WaitForSeconds(frameDuration);
            }

            if (sr != null)
            {
                sr.gameObject.SetActive(false);
                returnPool.Enqueue(sr);
            }
        }

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        public static void ResetInstanceForTesting()
        {
            _instance = null;
        }
#endif
    }
}
