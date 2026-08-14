using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// PaperLanternEnemy represents the basic "Đèn lồng giấy (thường)" enemy.
    /// Flies straight down with a slight visual floating bob effect.
    /// </summary>
    public class PaperLanternEnemy : EnemyBase
    {
        [Header("Paper Lantern Settings")]
        [SerializeField] private float floatingBobSpeed = 3f;
        [SerializeField] private float floatingBobAmount = 0.05f;

        protected override void Move()
        {
            base.Move();

            // Add slight visual floating wobble for Mid-Autumn lantern feel
            if (spriteRenderer != null)
            {
                float wobble = Mathf.Sin(aliveTimer * floatingBobSpeed) * floatingBobAmount;
                spriteRenderer.transform.localPosition = new Vector3(wobble, 0, 0);
            }
        }
    }
}
