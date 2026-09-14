using System;
using System.Collections.Generic;

namespace DemHoiDenLong.Data
{
    [Serializable]
    public class PlayerData
    {
        public int StarLanterns;
        public int Diamonds;
        
        public int HighestUnlockedLevel;
        
        // Sử dụng List thay vì Dictionary để tương thích với JsonUtility của Unity
        public List<LanSaveData> UnlockedLans = new List<LanSaveData>();
        
        // Khởi tạo giá trị mặc định cho người chơi mới
        public PlayerData()
        {
            StarLanterns = 0;
            Diamonds = 0;
            HighestUnlockedLevel = 1;
            
            // Lân mặc định khi vào game
            UnlockedLans.Add(new LanSaveData("Lan_Default", 1, 1, 1));
        }
    }

    [Serializable]
    public class LanSaveData
    {
        public string LanId;
        public int DamageLevel;
        public int HpLevel;
        public int FireRateLevel;

        public LanSaveData(string lanId, int dmgLvl, int hpLvl, int fireRateLvl)
        {
            LanId = lanId;
            DamageLevel = dmgLvl;
            HpLevel = hpLvl;
            FireRateLevel = fireRateLvl;
        }
    }
}
