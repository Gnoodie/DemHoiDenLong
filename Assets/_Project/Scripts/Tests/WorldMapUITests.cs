using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DemHoiDenLong.UI;
using DemHoiDenLong.Meta;
using UnityEngine.UI;

namespace DemHoiDenLong.Tests
{
    public class WorldMapUITests
    {
        private GameObject mapUIObject;
        private WorldMapUI worldMapUI;
        private GameObject saveManagerObj;
        
        [SetUp]
        public void Setup()
        {
            // Setup SaveManager Mock
            saveManagerObj = new GameObject("SaveManager");
            var saveManager = saveManagerObj.AddComponent<SaveManager>();
            
            // Setup WorldMapUI
            mapUIObject = new GameObject("WorldMapUI");
            worldMapUI = mapUIObject.AddComponent<WorldMapUI>();

            // Mock Data
            var levelButtons = new List<WorldMapUI.LevelButton>();
            
            for (int i = 1; i <= 3; i++)
            {
                var btnObj = new GameObject($"Level{i}_Btn");
                var lockObj = new GameObject($"LockIcon");
                var completeObj = new GameObject($"CompleteIcon");
                
                var btn = btnObj.AddComponent<Button>();
                
                levelButtons.Add(new WorldMapUI.LevelButton() {
                    levelIndex = i,
                    button = btn,
                    lockIcon = lockObj,
                    completeIcon = completeObj
                });
            }

            // Using reflection to set private field for testing
            var fieldInfo = typeof(WorldMapUI).GetField("levelButtons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue(worldMapUI, levelButtons);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(mapUIObject);
            Object.DestroyImmediate(saveManagerObj);
        }

        [Test]
        public void RefreshUI_UnlocksLevelsCorrectly_BasedOnHighestUnlocked()
        {
            // Arrange
            SaveManager.Instance.DeleteSaveData(); // Ensure fresh data
            SaveManager.Instance.CurrentData.HighestUnlockedLevel = 2; // Level 1 and 2 unlocked
            
            // Act
            worldMapUI.RefreshUI();

            // Assert
            var buttons = (List<WorldMapUI.LevelButton>)typeof(WorldMapUI).GetField("levelButtons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(worldMapUI);

            // Level 1: Unlocked and Completed (Index < 2)
            Assert.IsTrue(buttons[0].button.interactable);
            Assert.IsFalse(buttons[0].lockIcon.activeSelf);
            Assert.IsTrue(buttons[0].completeIcon.activeSelf);

            // Level 2: Unlocked but Not Completed (Index == 2)
            Assert.IsTrue(buttons[1].button.interactable);
            Assert.IsFalse(buttons[1].lockIcon.activeSelf);
            Assert.IsFalse(buttons[1].completeIcon.activeSelf);

            // Level 3: Locked (Index > 2)
            Assert.IsFalse(buttons[2].button.interactable);
            Assert.IsTrue(buttons[2].lockIcon.activeSelf);
            Assert.IsFalse(buttons[2].completeIcon.activeSelf);
        }
    }
}
