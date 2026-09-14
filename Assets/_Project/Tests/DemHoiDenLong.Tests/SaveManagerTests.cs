using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DemHoiDenLong.Meta;
using DemHoiDenLong.Data;
using System.IO;

namespace DemHoiDenLong.Tests
{
    public class SaveManagerTests
    {
        private GameObject saveManagerGO;
        private SaveManager saveManager;

        [SetUp]
        public void Setup()
        {
            saveManagerGO = new GameObject("SaveManager");
            saveManager = saveManagerGO.AddComponent<SaveManager>();
            
            // In some Unity EditMode configurations, Awake is not called synchronously on AddComponent.
            // We explicitly call LoadData to ensure initialization.
            saveManager.LoadData();
        }

        [TearDown]
        public void Teardown()
        {
            if (saveManager != null)
            {
                saveManager.DeleteSaveData();
            }
            Object.DestroyImmediate(saveManagerGO);

            // Force clear the singleton instance via reflection to ensure test isolation
            var instanceField = typeof(SaveManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceField != null)
            {
                instanceField.SetValue(null, null);
            }
        }

        [Test]
        public void SaveManager_CreatesNewData_IfNoFileExists()
        {
            Assert.IsNotNull(saveManager.CurrentData);
            Assert.AreEqual(0, saveManager.CurrentData.StarLanterns);
            Assert.AreEqual(1, saveManager.CurrentData.HighestUnlockedLevel);
            Assert.AreEqual(1, saveManager.CurrentData.UnlockedLans.Count);
        }

        [Test]
        public void SaveManager_SavesAndLoadsData_Correctly()
        {
            // Modify current data
            saveManager.CurrentData.StarLanterns = 500;
            saveManager.CurrentData.Diamonds = 50;
            saveManager.CurrentData.HighestUnlockedLevel = 5;
            saveManager.CurrentData.UnlockedLans.Add(new LanSaveData("Lan_Premium", 2, 2, 2));

            // Save data
            saveManager.SaveData();

            // Simulate app restart by destroying and recreating SaveManager
            Object.DestroyImmediate(saveManagerGO);

            saveManagerGO = new GameObject("SaveManager2");
            saveManager = saveManagerGO.AddComponent<SaveManager>(); 
            saveManager.LoadData();

            // Assert data was loaded correctly
            Assert.AreEqual(500, saveManager.CurrentData.StarLanterns);
            Assert.AreEqual(50, saveManager.CurrentData.Diamonds);
            Assert.AreEqual(5, saveManager.CurrentData.HighestUnlockedLevel);
            Assert.AreEqual(2, saveManager.CurrentData.UnlockedLans.Count);
            
            var premiumLan = saveManager.CurrentData.UnlockedLans[1];
            Assert.AreEqual("Lan_Premium", premiumLan.LanId);
            Assert.AreEqual(2, premiumLan.DamageLevel);
        }
    }
}
