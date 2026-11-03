using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "DemHoiDenLong/Level Database", order = 1)]
    public class LevelDatabase : ScriptableObject
    {
        [Header("Acts in Campaign")]
        public List<ActData> acts = new List<ActData>();

        public int ActCount => acts != null ? acts.Count : 0;

        public int TotalLevels
        {
            get
            {
                if (acts == null) return 0;
                int sum = 0;
                for (int i = 0; i < acts.Count; i++)
                {
                    if (acts[i] != null && acts[i].levels != null)
                    {
                        sum += acts[i].levels.Count;
                    }
                }
                return sum;
            }
        }

        /// <summary>
        /// Truy vấn an toàn màn chơi theo levelId toàn game (1..N). Trả null và log warning nếu sai id.
        /// </summary>
        public LevelData GetLevel(int levelId)
        {
            if (levelId <= 0)
            {
                Debug.LogWarning($"[LevelDatabase] GetLevel: levelId không hợp lệ ({levelId}).");
                return null;
            }

            if (acts != null)
            {
                for (int a = 0; a < acts.Count; a++)
                {
                    ActData act = acts[a];
                    if (act == null || act.levels == null) continue;
                    for (int l = 0; l < act.levels.Count; l++)
                    {
                        LevelData lvl = act.levels[l];
                        if (lvl != null && lvl.levelId == levelId)
                        {
                            return lvl;
                        }
                    }
                }
            }

            Debug.LogWarning($"[LevelDatabase] Không tìm thấy LevelData cho levelId {levelId}.");
            return null;
        }

        /// <summary>
        /// Truy vấn an toàn Hồi theo actIndex (0..N-1). Trả null và log warning nếu sai index.
        /// </summary>
        public ActData GetAct(int actIndex)
        {
            if (acts == null || actIndex < 0 || actIndex >= acts.Count)
            {
                Debug.LogWarning($"[LevelDatabase] GetAct: actIndex {actIndex} nằm ngoài phạm vi (0..{ActCount - 1}).");
                return null;
            }
            return acts[actIndex];
        }

        /// <summary>
        /// Tìm Hồi chứa màn chơi có levelId. Trả null nếu không tìm thấy.
        /// </summary>
        public ActData GetActOfLevel(int levelId)
        {
            if (levelId <= 0 || acts == null) return null;

            for (int a = 0; a < acts.Count; a++)
            {
                ActData act = acts[a];
                if (act == null || act.levels == null) continue;
                for (int l = 0; l < act.levels.Count; l++)
                {
                    if (act.levels[l] != null && act.levels[l].levelId == levelId)
                    {
                        return act;
                    }
                }
            }

            Debug.LogWarning($"[LevelDatabase] Không tìm thấy Hồi cho levelId {levelId}.");
            return null;
        }

        /// <summary>
        /// Lấy levelId của màn đầu tiên trong Hồi actIndex (ví dụ Hồi 0 -> 1, Hồi 1 -> 6, Hồi 2 -> 11).
        /// Trả về -1 nếu actIndex sai hoặc Hồi rỗng.
        /// </summary>
        public int GetFirstLevelId(int actIndex)
        {
            ActData act = GetAct(actIndex);
            if (act == null || act.levels == null || act.levels.Count == 0 || act.levels[0] == null)
            {
                Debug.LogWarning($"[LevelDatabase] Không lấy được FirstLevelId cho actIndex {actIndex}.");
                return -1;
            }
            return act.levels[0].levelId;
        }
    }
}
