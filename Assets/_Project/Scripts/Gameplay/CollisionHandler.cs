using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// CollisionHandler provides atomic collision processing between Projectiles,
    /// Player, and Enemy targets to ensure 100% accurate hit detection with zero double-hits.
    /// </summary>
    public static class CollisionHandler
    {
        /// <summary>
        /// Processes collision between a Player Bullet and an IDamageable target (Enemy, Boss, etc.).
        /// Returns true if hit was registered and applied.
        /// </summary>
        public static bool ProcessPlayerBulletHit(Bullet bullet, IDamageable target)
        {
            if (bullet == null || target == null) return false;
            if (!bullet.gameObject.activeSelf || target.IsDead) return false;

            target.TakeDamage(bullet.Damage);
            bullet.Recycle();
            return true;
        }

        /// <summary>
        /// Processes collision between an Enemy / Enemy Bullet and the Player.
        /// Respects player invincibility frames and active shield.
        /// </summary>
        public static bool ProcessDamageToPlayer(PlayerController player, float damageAmount)
        {
            if (player == null || player.IsDead || player.IsInvincible) return false;

            player.TakeDamage(damageAmount);
            return true;
        }
    }
}
