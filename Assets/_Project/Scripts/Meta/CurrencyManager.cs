using UnityEngine;
using System;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Meta
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        public event Action<int> OnStarLanternsChanged;
        public event Action<int> OnDiamondsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private PlayerData Data 
        {
            get
            {
                if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
                {
                    return SaveManager.Instance.CurrentData;
                }
                return null;
            }
        }

        public int StarLanterns => Data?.StarLanterns ?? 0;
        public int Diamonds => Data?.Diamonds ?? 0;

        // Đèn Ông Sao
        public void AddStarLanterns(int amount)
        {
            if (amount <= 0 || Data == null) return;
            Data.StarLanterns += amount;
            SaveManager.Instance.SaveData();
            OnStarLanternsChanged?.Invoke(Data.StarLanterns);
        }

        public bool HasEnoughStarLanterns(int amount)
        {
            return StarLanterns >= amount;
        }

        public bool SpendStarLanterns(int amount)
        {
            if (amount <= 0 || Data == null) return false;
            
            if (HasEnoughStarLanterns(amount))
            {
                Data.StarLanterns -= amount;
                SaveManager.Instance.SaveData();
                OnStarLanternsChanged?.Invoke(Data.StarLanterns);
                return true;
            }
            return false;
        }

        // Kim Cương
        public void AddDiamonds(int amount)
        {
            if (amount <= 0 || Data == null) return;
            Data.Diamonds += amount;
            SaveManager.Instance.SaveData();
            OnDiamondsChanged?.Invoke(Data.Diamonds);
        }

        public bool HasEnoughDiamonds(int amount)
        {
            return Diamonds >= amount;
        }

        public bool SpendDiamonds(int amount)
        {
            if (amount <= 0 || Data == null) return false;
            
            if (HasEnoughDiamonds(amount))
            {
                Data.Diamonds -= amount;
                SaveManager.Instance.SaveData();
                OnDiamondsChanged?.Invoke(Data.Diamonds);
                return true;
            }
            return false;
        }
        
        // Phương thức hỗ trợ gọi event cho UI khi mới khởi tạo
        public void BroadcastCurrentCurrencies()
        {
            if (Data == null) return;
            OnStarLanternsChanged?.Invoke(Data.StarLanterns);
            OnDiamondsChanged?.Invoke(Data.Diamonds);
        }
    }
}
