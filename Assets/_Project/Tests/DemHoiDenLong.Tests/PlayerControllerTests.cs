#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Tests
{
    public class PlayerControllerTests
    {
        private GameObject playerObject;
        private PlayerController playerController;
        private GameObject cameraObject;
        private Camera testCamera;

        [SetUp]
        public void SetUp()
        {
            // Setup Camera for screen bounds testing
            cameraObject = new GameObject("TestCamera");
            testCamera = cameraObject.AddComponent<Camera>();
            testCamera.orthographic = true;
            testCamera.orthographicSize = 5f; // Screen height 10 units (-5 to +5)
            testCamera.aspect = 1f; // 1:1 Aspect ratio, Screen width 10 units (-5 to +5)
            cameraObject.transform.position = new Vector3(0, 0, -10f);

            // Setup Player
            playerObject = new GameObject("TestPlayer");
            playerObject.AddComponent<BoxCollider2D>();
            playerController = playerObject.AddComponent<PlayerController>();
            playerController.MainCamera = testCamera;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(cameraObject);
        }

        #region Stats & Health Tests

        [Test]
        public void PlayerController_InitializesStats_DefaultValues()
        {
            playerController.InitializeStats();
            Assert.AreEqual(100f, playerController.MaxHp);
            Assert.AreEqual(100f, playerController.CurrentHp);
            Assert.IsFalse(playerController.IsDead);
        }

        [Test]
        public void PlayerController_TakeDamage_ReducesHp()
        {
            playerController.InitializeStats();
            playerController.TakeDamage(30f);

            Assert.AreEqual(70f, playerController.CurrentHp);
            Assert.IsFalse(playerController.IsDead);
        }

        [Test]
        public void PlayerController_TakeFatalDamage_TriggersDeath()
        {
            playerController.InitializeStats();
            playerController.LivesCount = 1; // Explicitly set to 1 to trigger death instead of respawn
            playerController.TakeDamage(150f);

            Assert.AreEqual(0f, playerController.CurrentHp);
            Assert.IsTrue(playerController.IsDead);
        }

        [Test]
        public void PlayerController_Heal_IncreasesHpCappedAtMax()
        {
            playerController.InitializeStats();
            playerController.TakeDamage(50f);
            playerController.Heal(30f);

            Assert.AreEqual(80f, playerController.CurrentHp);

            playerController.Heal(50f);
            Assert.AreEqual(100f, playerController.CurrentHp);
        }

        #endregion

        #region Touch-Drag & Screen Bounds Tests

        [Test]
        public void PlayerController_CalculateScreenBounds_SetsCorrectOrthographicBounds()
        {
            playerController.CalculateScreenBounds();

            // Orthographic size = 5, aspect = 1.0 => X: [-5, 5], Y: [-5, 5]
            Assert.AreEqual(-5f, playerController.MinScreenBounds.x, 0.01f);
            Assert.AreEqual(5f, playerController.MaxScreenBounds.x, 0.01f);
            Assert.AreEqual(-5f, playerController.MinScreenBounds.y, 0.01f);
            Assert.AreEqual(5f, playerController.MaxScreenBounds.y, 0.01f);
        }

        [Test]
        public void PlayerController_SimulateDragDelta_MovesPlayerTargetWithinBounds()
        {
            playerController.CalculateScreenBounds();
            playerController.ForceUpdatePosition(Vector3.zero);

            // Drag by (+2, +3)
            playerController.SimulateDragDelta(new Vector3(2f, 3f, 0f));

            Assert.AreEqual(2f, playerController.TargetWorldPosition.x, 0.01f);
            Assert.AreEqual(3f, playerController.TargetWorldPosition.y, 0.01f);
        }

        [Test]
        public void PlayerController_PositionClamping_PreventsExceedingLeftAndRightScreenBounds()
        {
            playerController.CalculateScreenBounds();

            // Try moving far to the left (-100)
            playerController.ForceUpdatePosition(new Vector3(-100f, 0f, 0f));
            Assert.AreEqual(playerController.MinScreenBounds.x, playerController.transform.position.x, 0.01f);

            // Try moving far to the right (+100)
            playerController.ForceUpdatePosition(new Vector3(100f, 0f, 0f));
            Assert.AreEqual(playerController.MaxScreenBounds.x, playerController.transform.position.x, 0.01f);
        }

        [Test]
        public void PlayerController_PositionClamping_PreventsExceedingTopAndBottomScreenBounds()
        {
            playerController.CalculateScreenBounds();

            // Try moving far down (-100)
            playerController.ForceUpdatePosition(new Vector3(0f, -100f, 0f));
            Assert.AreEqual(playerController.MinScreenBounds.y, playerController.transform.position.y, 0.01f);

            // Try moving far up (+100)
            playerController.ForceUpdatePosition(new Vector3(0f, 100f, 0f));
            Assert.AreEqual(playerController.MaxScreenBounds.y, playerController.transform.position.y, 0.01f);
        }

        #endregion
    }
}
#endif
