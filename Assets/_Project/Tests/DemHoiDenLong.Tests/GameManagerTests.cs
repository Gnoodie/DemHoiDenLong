using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Gameplay;
using System.Reflection;

namespace DemHoiDenLong.Tests
{
    [TestFixture]
    public class GameManagerTests
    {
        private GameObject gameManagerObject;
        private GameManager gameManager;
        private GameObject playerObject;
        private PlayerController player;

        [SetUp]
        public void SetUp()
        {
            GameManager.ResetInstanceForTesting();
            
            playerObject = new GameObject("TestPlayer");
            playerObject.AddComponent<BoxCollider2D>();
            player = playerObject.AddComponent<PlayerController>();
            player.InitializeStats();

            gameManagerObject = new GameObject("GameManager");
            gameManager = gameManagerObject.AddComponent<GameManager>();

            // Assign player field via reflection because it's private SerializedField
            FieldInfo playerField = typeof(GameManager).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance);
            playerField?.SetValue(gameManager, player);

            // Call Start manually via reflection to wire up the events (since Edit Mode doesn't do it)
            MethodInfo startMethod = typeof(GameManager).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(gameManager, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (gameManagerObject != null) Object.DestroyImmediate(gameManagerObject);
            if (playerObject != null) Object.DestroyImmediate(playerObject);
            GameManager.ResetInstanceForTesting();
        }

        [Test]
        public void GameManager_StartsWithPlayingState()
        {
            Assert.AreEqual(GameState.Playing, gameManager.CurrentState);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void GameManager_PlayerDeath_TransitionsToGameOver()
        {
            // Simulate death
            player.LivesCount = 0;
            player.TakeDamage(1000f);

            Assert.AreEqual(GameState.GameOver, gameManager.CurrentState);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void GameManager_TriggerVictory_TransitionsToVictory()
        {
            gameManager.TriggerVictory();

            Assert.AreEqual(GameState.Victory, gameManager.CurrentState);
            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void GameManager_TogglePause_TogglesBetweenPlayingAndPaused()
        {
            gameManager.TogglePause();
            Assert.AreEqual(GameState.Paused, gameManager.CurrentState);
            Assert.AreEqual(0f, Time.timeScale);

            gameManager.TogglePause();
            Assert.AreEqual(GameState.Playing, gameManager.CurrentState);
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
