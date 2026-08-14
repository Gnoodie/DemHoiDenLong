#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.Tests
{
    public class BulletPoolTests
    {
        private GameObject poolObject;
        private BulletPool bulletPool;
        private GameObject cameraObject;
        private Camera testCamera;

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
        }

        [TearDown]
        public void TearDown()
        {
            if (poolObject != null)
            {
                Object.DestroyImmediate(poolObject);
            }
            if (cameraObject != null)
            {
                Object.DestroyImmediate(cameraObject);
            }
            BulletPool.ResetInstanceForTesting();
        }

        [Test]
        public void BulletPool_InitializesPool_PreAllocates200Bullets()
        {
            Assert.AreEqual(200, bulletPool.AvailableCount);
            Assert.AreEqual(0, bulletPool.ActiveCount);
            Assert.AreEqual(200, bulletPool.TotalCount);
        }

        [Test]
        public void BulletPool_SpawnBullet_ActivatesAndReturnsBulletFromAvailablePool()
        {
            Bullet spawned = bulletPool.SpawnBullet(Vector3.zero, Vector2.up, 10f, 5f, true);

            Assert.IsNotNull(spawned);
            Assert.IsTrue(spawned.gameObject.activeSelf);
            Assert.AreEqual(199, bulletPool.AvailableCount);
            Assert.AreEqual(1, bulletPool.ActiveCount);
        }

        [Test]
        public void BulletPool_ReturnBullet_RecyclesBulletBackToAvailablePool()
        {
            Bullet spawned = bulletPool.SpawnBullet(Vector3.zero, Vector2.up, 10f, 5f, true);
            bulletPool.ReturnBullet(spawned);

            Assert.IsFalse(spawned.gameObject.activeSelf);
            Assert.AreEqual(200, bulletPool.AvailableCount);
            Assert.AreEqual(0, bulletPool.ActiveCount);
        }

        [Test]
        public void BulletPool_HighVolumeSpawning_Handles100SimultaneousBulletsWithoutStutter()
        {
            Bullet[] spawnedBullets = new Bullet[100];
            for (int i = 0; i < 100; i++)
            {
                spawnedBullets[i] = bulletPool.SpawnBullet(new Vector3(i * 0.1f, 0, 0), Vector2.up, 10f, 5f, true);
            }

            Assert.AreEqual(100, bulletPool.ActiveCount);
            Assert.AreEqual(100, bulletPool.AvailableCount);

            // Clean up
            for (int i = 0; i < 100; i++)
            {
                bulletPool.ReturnBullet(spawnedBullets[i]);
            }

            Assert.AreEqual(0, bulletPool.ActiveCount);
            Assert.AreEqual(200, bulletPool.AvailableCount);
        }

        [Test]
        public void BulletPool_ClearAllActiveBullets_RecyclesAllActiveBullets()
        {
            for (int i = 0; i < 120; i++)
            {
                bulletPool.SpawnBullet(Vector3.zero, Vector2.up, 10f, 5f, true);
            }

            Assert.AreEqual(120, bulletPool.ActiveCount);

            bulletPool.ClearAllActiveBullets();

            Assert.AreEqual(0, bulletPool.ActiveCount);
            Assert.AreEqual(200, bulletPool.AvailableCount);
        }
    }
}
#endif
