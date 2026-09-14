using System.Collections;
using NUnit.Framework;
using UnityEngine;
using DemHoiDenLong.Meta;
using DemHoiDenLong.Data;
using System.Reflection;

namespace DemHoiDenLong.Tests
{
    public class CurrencyManagerTests
    {
        private GameObject saveManagerGO;
        private SaveManager saveManager;
        
        private GameObject currencyManagerGO;
        private CurrencyManager currencyManager;

        [SetUp]
        public void Setup()
        {
            // Clear static instances to ensure clean state before test
            var smInstance = typeof(SaveManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (smInstance != null) smInstance.SetValue(null, null);
            
            var cmInstance = typeof(CurrencyManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (cmInstance != null) cmInstance.SetValue(null, null);

            // Setup SaveManager
            saveManagerGO = new GameObject("SaveManager");
            saveManager = saveManagerGO.AddComponent<SaveManager>();
            typeof(SaveManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(saveManager, null);
            saveManager.LoadData(); // Force load
            saveManager.CurrentData.StarLanterns = 100; // Initialize with 100
            saveManager.CurrentData.Diamonds = 10;      // Initialize with 10
            
            // Setup CurrencyManager
            currencyManagerGO = new GameObject("CurrencyManager");
            currencyManager = currencyManagerGO.AddComponent<CurrencyManager>();
            typeof(CurrencyManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(currencyManager, null);
        }

        [TearDown]
        public void Teardown()
        {
            if (saveManager != null)
            {
                saveManager.DeleteSaveData();
            }
            
            Object.DestroyImmediate(currencyManagerGO);
            Object.DestroyImmediate(saveManagerGO);

            // Clear static instances
            var smInstance = typeof(SaveManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (smInstance != null) smInstance.SetValue(null, null);
            
            var cmInstance = typeof(CurrencyManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (cmInstance != null) cmInstance.SetValue(null, null);
        }

        [Test]
        public void CurrencyManager_ReadsInitialValues_Correctly()
        {
            Assert.AreEqual(100, currencyManager.StarLanterns);
            Assert.AreEqual(10, currencyManager.Diamonds);
        }

        [Test]
        public void CurrencyManager_AddStarLanterns_UpdatesValueAndFiresEvent()
        {
            int eventValue = -1;
            currencyManager.OnStarLanternsChanged += (val) => eventValue = val;

            currencyManager.AddStarLanterns(50);

            Assert.AreEqual(150, currencyManager.StarLanterns);
            Assert.AreEqual(150, eventValue);
        }

        [Test]
        public void CurrencyManager_SpendStarLanterns_FailsIfInsufficient()
        {
            int eventValue = -1;
            currencyManager.OnStarLanternsChanged += (val) => eventValue = val;

            bool success = currencyManager.SpendStarLanterns(150);

            Assert.IsFalse(success);
            Assert.AreEqual(100, currencyManager.StarLanterns); // Unchanged
            Assert.AreEqual(-1, eventValue); // Event not fired
        }

        [Test]
        public void CurrencyManager_SpendStarLanterns_SucceedsIfSufficient()
        {
            int eventValue = -1;
            currencyManager.OnStarLanternsChanged += (val) => eventValue = val;

            bool success = currencyManager.SpendStarLanterns(40);

            Assert.IsTrue(success);
            Assert.AreEqual(60, currencyManager.StarLanterns); 
            Assert.AreEqual(60, eventValue);
        }
    }
}
