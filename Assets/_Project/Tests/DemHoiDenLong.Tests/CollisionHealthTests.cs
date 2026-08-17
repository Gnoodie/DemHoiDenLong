#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.Tests
{
    public class CollisionHealthTests
    {
        private GameObject cameraObject;
        private Camera testCamera;
        private GameObject playerObject;
        private PlayerController player;
        private GameObject enemyObject;
        private PaperLanternEnemy enemy;
        private GameObject poolObject;
        private BulletPool bulletPool;

        [SetUp]
        public void SetUp()
        {
            BulletPool.ResetInstanceForTesting();

            cameraObject = new GameObject("TestCamera");
            testCamera = cameraObject.AddComponent<Camera>();
            testCamera.orthographic = true;
            testCamera.orthographicSize = 5f;

            poolObject = new GameObject("BulletPool");
            bulletPool = poolObject.AddComponent<BulletPool>();
            bulletPool.InitializePool();

            playerObject = new GameObject("TestPlayer");
            playerObject.AddComponent<BoxCollider2D>();
            player = playerObject.AddComponent<PlayerController>();
            player.MainCamera = testCamera;
            player.InitializeStats();

            enemyObject = new GameObject("TestEnemy");
            enemyObject.AddComponent<CircleCollider2D>();
            enemy = enemyObject.AddComponent<PaperLanternEnemy>();
            enemy.Initialize(Vector3.zero, new Vector2(-5f, -5f), new Vector2(5f, 5f));
        }

        [TearDown]
        public void TearDown()
        {
            if (playerObject != null) Object.DestroyImmediate(playerObject);
            if (enemyObject != null) Object.DestroyImmediate(enemyObject);
            if (poolObject != null) Object.DestroyImmediate(poolObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            BulletPool.ResetInstanceForTesting();
        }

        [Test]
        public void CollisionHandler_AtomicBulletHit_AppliesDamageOnceAndRecyclesBullet()
        {
            Bullet bullet = bulletPool.SpawnBullet(Vector3.zero, Vector2.up, 10f, 4f, true);

            // First hit: registers hit and recycles bullet
            bool firstHit = CollisionHandler.ProcessPlayerBulletHitEnemy(bullet, enemy);
            Assert.IsTrue(firstHit);
            Assert.AreEqual(6f, enemy.CurrentHp); // Base 10 - 4 = 6
            Assert.IsFalse(bullet.gameObject.activeSelf);

            // Second hit in same frame: fails because bullet is already recycled (no double hit)
            bool secondHit = CollisionHandler.ProcessPlayerBulletHitEnemy(bullet, enemy);
            Assert.IsFalse(secondHit);
            Assert.AreEqual(6f, enemy.CurrentHp);
        }

        [Test]
        public void PlayerController_Invincibility_IgnoresDamageDuringIFrames()
        {
            player.TriggerInvincibility(1.5f);
            Assert.IsTrue(player.IsInvincible);

            player.TakeDamage(50f);
            Assert.AreEqual(100f, player.CurrentHp); // 0 damage taken during i-frames
        }

        [Test]
        public void PlayerController_ShieldCharges_AbsorbsHitAndDecrementsCharge()
        {
            player.ShieldCharges = 1;

            player.TakeDamage(40f);
            Assert.AreEqual(0, player.ShieldCharges);
            Assert.AreEqual(100f, player.CurrentHp); // HP protected by shield
        }

        [Test]
        public void PlayerController_Respawn_DecrementsLifeRestoresHpAndTriggersIFrames()
        {
            player.LivesCount = 3;

            // Deal fatal damage
            player.TakeDamage(150f);

            Assert.AreEqual(2, player.LivesCount);
            Assert.AreEqual(100f, player.CurrentHp);
            Assert.IsTrue(player.IsInvincible);
        }
    }
}
#endif
