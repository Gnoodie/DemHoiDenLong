#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Tests
{
    public class WaveBossTests
    {
        private GameObject poolObject;
        private BulletPool bulletPool;
        private GameObject bossObject;
        private BossController boss;
        private Bullet playerBullet;

        [SetUp]
        public void SetUp()
        {
            BulletPool.ResetInstanceForTesting();

            poolObject = new GameObject("BulletPool");
            bulletPool = poolObject.AddComponent<BulletPool>();
            bulletPool.InitializePool();
            
            // In Edit Mode tests, Awake is not called automatically, so we must set Instance manually
            typeof(BulletPool).GetProperty("Instance").SetValue(null, bulletPool);

            bossObject = new GameObject("TestBoss");
            bossObject.AddComponent<BoxCollider2D>();
            boss = bossObject.AddComponent<BossController>();
            boss.InitializeStats(1f, 1f); // Base 1000 HP

            playerBullet = bulletPool.SpawnBullet(Vector3.zero, Vector2.up, 10f, 50f, true);
        }

        [TearDown]
        public void TearDown()
        {
            if (bossObject != null) Object.DestroyImmediate(bossObject);
            if (poolObject != null) Object.DestroyImmediate(poolObject);
            if (playerBullet != null && playerBullet.gameObject != null) Object.DestroyImmediate(playerBullet.gameObject);
            BulletPool.ResetInstanceForTesting();
        }

        [Test]
        public void BossController_TakeDamage_ReducesHpAndTriggersDeath()
        {
            Assert.AreEqual(1000f, boss.CurrentHp);
            Assert.IsFalse(boss.IsDead);

            boss.TakeDamage(300f);
            Assert.AreEqual(700f, boss.CurrentHp);
            Assert.IsFalse(boss.IsDead);

            boss.TakeDamage(800f);
            Assert.AreEqual(0f, boss.CurrentHp);
            Assert.IsTrue(boss.IsDead);
        }

        [Test]
        public void CollisionHandler_BulletHitsBoss_ReducesBossHpAndRecyclesBullet()
        {
            bool hit = CollisionHandler.ProcessPlayerBulletHit(playerBullet, boss);
            
            Assert.IsTrue(hit);
            Assert.AreEqual(950f, boss.CurrentHp); // 1000 - 50
            Assert.IsFalse(playerBullet.gameObject.activeSelf);
        }

        [Test]
        public void BossController_Death_ClearsEnemyBullets()
        {
            // Spawn some boss bullets
            Bullet bossBullet1 = bulletPool.SpawnBullet(Vector3.zero, Vector2.down, 10f, 10f, false);
            Bullet bossBullet2 = bulletPool.SpawnBullet(Vector3.zero, Vector2.down, 10f, 10f, false);
            
            Assert.IsTrue(bossBullet1.gameObject.activeSelf);
            Assert.IsTrue(bossBullet2.gameObject.activeSelf);

            // Kill boss
            boss.TakeDamage(2000f);

            // Verify boss bullets are cleared by the BulletPool.ClearAllActiveBullets call on Boss death
            Assert.IsFalse(bossBullet1.gameObject.activeSelf);
            Assert.IsFalse(bossBullet2.gameObject.activeSelf);
        }
    }
}
#endif
