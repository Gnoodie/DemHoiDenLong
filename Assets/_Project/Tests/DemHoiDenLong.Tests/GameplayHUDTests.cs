using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.UI;
using DemHoiDenLong.Gameplay;
using System.Reflection;
using UnityEngine.UI;
using TMPro;

namespace DemHoiDenLong.Tests
{
    [TestFixture]
    public class GameplayHUDTests
    {
        private GameObject hudObject;
        private GameplayHUD hud;
        private GameObject playerObject;
        private PlayerController player;
        private GameObject gameManagerObject;
        private GameManager gameManager;
        
        private GameObject gameOverPanel;
        private GameObject victoryPanel;

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
            typeof(GameManager).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(gameManager, player);

            hudObject = new GameObject("HUD");
            hud = hudObject.AddComponent<GameplayHUD>();

            // Setup UI Elements
            Slider slider = new GameObject("Slider").AddComponent<Slider>();
            TextMeshProUGUI hpText = new GameObject("HpText").AddComponent<TextMeshProUGUI>();
            TextMeshProUGUI livesText = new GameObject("LivesText").AddComponent<TextMeshProUGUI>();
            gameOverPanel = new GameObject("GameOverPanel");
            victoryPanel = new GameObject("VictoryPanel");

            // Assign via reflection
            typeof(GameplayHUD).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, player);
            typeof(GameplayHUD).GetField("gameManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, gameManager);
            typeof(GameplayHUD).GetField("hpSlider", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, slider);
            typeof(GameplayHUD).GetField("hpText", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, hpText);
            typeof(GameplayHUD).GetField("livesText", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, livesText);
            typeof(GameplayHUD).GetField("gameOverPanel", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, gameOverPanel);
            typeof(GameplayHUD).GetField("victoryPanel", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(hud, victoryPanel);

            // Call Start manually
            typeof(GameManager).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(gameManager, null);
            typeof(GameplayHUD).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(hud, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (hudObject != null) Object.DestroyImmediate(hudObject);
            if (playerObject != null) Object.DestroyImmediate(playerObject);
            if (gameManagerObject != null) Object.DestroyImmediate(gameManagerObject);
            
            if (gameOverPanel != null) Object.DestroyImmediate(gameOverPanel);
            if (victoryPanel != null) Object.DestroyImmediate(victoryPanel);
            
            GameManager.ResetInstanceForTesting();
        }

        [Test]
        public void HUD_UpdatesWhenPlayerTakesDamage()
        {
            player.TakeDamage(10f); // default max is 100, so hp becomes 90

            Slider hpSlider = (Slider)typeof(GameplayHUD).GetField("hpSlider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hud);
            TextMeshProUGUI hpText = (TextMeshProUGUI)typeof(GameplayHUD).GetField("hpText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hud);

            Assert.AreEqual(90f, hpSlider.value);
            Assert.AreEqual("90 / 100", hpText.text);
        }

        [Test]
        public void HUD_ShowsGameOverPanel_WhenGameManagerTransitionsToGameOver()
        {
            gameManager.ChangeState(GameState.GameOver);
            Assert.IsTrue(gameOverPanel.activeSelf);
        }
        
        [Test]
        public void HUD_ShowsVictoryPanel_WhenGameManagerTransitionsToVictory()
        {
            gameManager.ChangeState(GameState.Victory);
            Assert.IsTrue(victoryPanel.activeSelf);
        }
    }
}
