using System.Collections.Generic;
using UnityEngine;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.Data
{
    [CreateAssetMenu(fileName = "SO_LevelData", menuName = "DemHoiDenLong/Level Data", order = 3)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Waves Timeline")]
        public List<WaveData> waves;

        [Header("Level Difficulty Scaling")]
        public float baseHpMultiplier = 1.0f;
        public float baseSpeedMultiplier = 1.0f;
        public float baseDamageMultiplier = 1.0f;

        [Header("Boss Fight")]
        public BossController bossPrefab;
        [Tooltip("Time to wait after the last wave completes before spawning the boss")]
        public float bossSpawnDelay = 3.0f;
    }
}
