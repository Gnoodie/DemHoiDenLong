using System;
using UnityEngine;

namespace DemHoiDenLong.Data
{
    [Serializable]
    public class WaveData
    {
        [Tooltip("Time (in seconds) since level start to trigger this wave")]
        public float spawnTime;
        
        [Tooltip("Number of enemies to spawn in this wave")]
        public int enemyCount;
        
        [Tooltip("Delay between each enemy spawn")]
        public float spawnInterval;
        
        [Tooltip("The enemy data definition to spawn")]
        public EnemyData enemyType;
    }
}
