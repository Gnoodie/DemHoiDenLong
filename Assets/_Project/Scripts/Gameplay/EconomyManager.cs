using UnityEngine;
using System;

namespace DemHoiDenLong.Gameplay
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        private const string STAR_KEY = "Player_Stars";
        private const string DIAMOND_KEY = "Player_Diamonds";

        public int StarCount { get; private set; }
        public int DiamondCount { get; private set; }

        public event Action<int> OnStarChanged;
        public event Action<int> OnDiamondChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadEconomy();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadEconomy()
        {
            StarCount = PlayerPrefs.GetInt(STAR_KEY, 1000); // Mặc định 1000 Sao cho người mới
            DiamondCount = PlayerPrefs.GetInt(DIAMOND_KEY, 500); // Mặc định 500 Kim Cương
        }

        public void AddStar(int amount)
        {
            if (amount <= 0) return;
            StarCount += amount;
            PlayerPrefs.SetInt(STAR_KEY, StarCount);
            PlayerPrefs.Save();
            OnStarChanged?.Invoke(StarCount);
        }

        public bool SpendStar(int amount)
        {
            if (amount <= 0) return true;
            if (StarCount >= amount)
            {
                StarCount -= amount;
                PlayerPrefs.SetInt(STAR_KEY, StarCount);
                PlayerPrefs.Save();
                OnStarChanged?.Invoke(StarCount);
                return true;
            }
            return false;
        }

        public void AddDiamond(int amount)
        {
            if (amount <= 0) return;
            DiamondCount += amount;
            PlayerPrefs.SetInt(DIAMOND_KEY, DiamondCount);
            PlayerPrefs.Save();
            OnDiamondChanged?.Invoke(DiamondCount);
        }

        public bool SpendDiamond(int amount)
        {
            if (amount <= 0) return true;
            if (DiamondCount >= amount)
            {
                DiamondCount -= amount;
                PlayerPrefs.SetInt(DIAMOND_KEY, DiamondCount);
                PlayerPrefs.Save();
                OnDiamondChanged?.Invoke(DiamondCount);
                return true;
            }
            return false;
        }

        // Helper for debugging
        [ContextMenu("Add 1000 Stars")]
        public void DebugAddStars()
        {
            AddStar(1000);
        }

        [ContextMenu("Add 1000 Diamonds")]
        public void DebugAddDiamonds()
        {
            AddDiamond(1000);
        }
    }
}
