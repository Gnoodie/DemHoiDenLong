using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Đèn lồng gai - khi bị tiêu diệt sẽ nổ thành 8 mảnh gai tỏa đều 8 hướng (360 độ).
    /// Mỗi mảnh có HP=5, dmg=5, tốc độ 3.5 đơn vị/giây, kéo vệt cam đỏ.
    /// Không nổ khi dọn màn hoặc rời màn hình.
    /// Dùng pool chuyên dụng, không Instantiate lúc chơi.
    /// </summary>
    public class SpikedLanternEnemy : EnemyBase
    {
        [Header("Spiked Lantern - Fragment Settings")]
        [SerializeField] private Sprite fragmentSprite;
        // fragmentDamage (5f) and fragmentSpeed (3.5f) are defaults in SpikedLanternDebris

        private static Queue<SpikedLanternDebris> debrisPool = new Queue<SpikedLanternDebris>();
        private static Transform debrisPoolRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDebrisPool()
        {
            debrisPool.Clear();
            debrisPoolRoot = null;
        }

        public override void Die(KillCause cause)
        {
            if (isDeadProcessed) return;

            // Nổ 8 mảnh gai khi bị tiêu diệt (không nổ khi dọn màn thắng cuộc hoặc thoát màn hình)
            if (cause != KillCause.ScreenExit && cause != KillCause.Other)
            {
                SpawnFragments();
            }

            base.Die(cause);
        }

        private void SpawnFragments()
        {
            // 8 mảnh gai bay tỏa đều 8 hướng (360 độ: 0, 45, 90, 135, 180, 225, 270, 315 độ)
            float[] angles = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
            Vector3 spawnPos = transform.position;
            Sprite debSpr = fragmentSprite != null ? fragmentSprite : (spriteRenderer != null ? spriteRenderer.sprite : null);

            foreach (float angle in angles)
            {
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
                SpikedLanternDebris deb = GetPooledDebris();
                deb.InitializeDebris(spawnPos, dir, debSpr, minScreenBounds, maxScreenBounds, 0.6f);
            }
        }

        private static SpikedLanternDebris GetPooledDebris()
        {
            while (debrisPool.Count > 0)
            {
                var d = debrisPool.Dequeue();
                if (d != null) return d;
            }

            if (debrisPoolRoot == null)
            {
                GameObject rootObj = new GameObject("DebrisPoolRoot");
                debrisPoolRoot = rootObj.transform;
            }

            GameObject obj = new GameObject("SpikedLanternDebris", typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(SpikedLanternDebris));
            obj.transform.SetParent(debrisPoolRoot);
            BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.4f, 0.4f);

            SpikedLanternDebris deb = obj.GetComponent<SpikedLanternDebris>();
            return deb;
        }

        public static void ReturnToPool(SpikedLanternDebris deb)
        {
            if (deb == null) return;
            deb.gameObject.SetActive(false);
            debrisPool.Enqueue(deb);
        }
    }
}
