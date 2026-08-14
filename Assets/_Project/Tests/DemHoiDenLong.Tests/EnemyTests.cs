#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Tests
{
    public class EnemyTests
    {
        private GameObject cameraObject;
        private Camera testCamera;
        private GameObject spawnerObject;
        private EnemySpawner enemySpawner;
        private GameObject enemyObject;
        private PaperLanternEnemy enemy;

        [SetUp]
        public void SetUp()
        {
            EnemySpawner.ResetInstanceForTesting();

            cameraObject = new GameObject("TestCamera");
            testCamera = cameraObject.AddComponent<Camera>();
            testCamera.orthographic = true;
            testCamera.orthographicSize = 5f;

            spawnerObject = new GameObject("EnemySpawner");
            enemySpawner = spawnerObject.AddComponent<EnemySpawner>();
            enemySpawner.InitializePool();

            enemyObject = new GameObject("TestEnemy");
            enemyObject.AddComponent<CircleCollider2D>();
            enemy = enemyObject.AddComponent<PaperLanternEnemy>();
        }

        [TearDown]
        public void TearDown()
        {
            if (enemyObject != null) Object.DestroyImmediate(enemyObject);
            if (spawnerObject != null) Object.DestroyImmediate(spawnerObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            EnemySpawner.ResetInstanceForTesting();
        }

        [Test]
        public void EnemyBase_InitializesStatsAndHealth_Correctly()
        {
            Vector2 minBounds = new Vector2(-5f, -5f);
            Vector2 maxBounds = new Vector2(5f, 5f);

            enemy.Initialize(new Vector3(0, 6f, 0), minBounds, maxBounds, 2.0f, 1.5f, 1.0f);

            Assert.AreEqual(20f, enemy.MaxHp);
            Assert.AreEqual(20f, enemy.CurrentHp);
            Assert.IsFalse(enemy.IsDead);
        }

        [Test]
        public void EnemyBase_TakeDamage_ReducesHpAndDespawnsOnDeath()
        {
            Vector2 minBounds = new Vector2(-5f, -5f);
            Vector2 maxBounds = new Vector2(5f, 5f);
            enemy.Initialize(Vector3.zero, minBounds, maxBounds);

            enemy.TakeDamage(5f);
            Assert.AreEqual(5f, enemy.CurrentHp);

            enemy.TakeDamage(10f);
            Assert.AreEqual(0f, enemy.CurrentHp);
            Assert.IsTrue(enemy.IsDead);
            Assert.IsFalse(enemyObject.activeSelf);
        }

        [Test]
        public void EnemySpawner_SpawnEnemy_RandomizesPositionAboveScreenAndAppliesMultipliers()
        {
            enemySpawner.SetDifficultyMultipliers(1.5f, 1.2f, 1.1f);
            EnemyBase spawned = enemySpawner.SpawnEnemy();

            Assert.IsNotNull(spawned);
            Assert.IsTrue(spawned.gameObject.activeSelf);

            // Spawns above maxScreenBounds.y (+5f)
            Assert.GreaterOrEqual(spawned.transform.position.y, 5f);
            Assert.AreEqual(15f, spawned.MaxHp); // Base 10 * 1.5

            enemySpawner.RecycleEnemy(spawned);
            Assert.IsFalse(spawned.gameObject.activeSelf);
        }
    }
}
#endif
