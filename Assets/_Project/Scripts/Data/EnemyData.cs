using UnityEngine;

namespace DemHoiDenLong.Data
{
    public enum MovementPattern
    {
        StraightDown,
        SinWave,
        Zigzag
    }

    [CreateAssetMenu(fileName = "SO_EnemyData", menuName = "DemHoiDenLong/Enemy Data", order = 2)]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyId = "lantern_paper";
        public string enemyName = "Đèn lồng giấy (thường)";
        public Sprite sprite;
        public GameObject prefab;

        [Header("Base Stats")]
        public float maxHp = 10f;
        public float moveSpeed = 60f; // px/s or world units/s
        public float damage = 10f;

        [Header("Rewards & Drops")]
        public int starReward = 1;
        public float dropChance = 0.5f; // % chance to drop powerup

        [Header("Movement & Behavior")]
        public MovementPattern movementPattern = MovementPattern.StraightDown;
        public float sinFrequency = 2.0f; // Oscillation speed for SinWave
        public float sinAmplitude = 1.5f; // Oscillation width for SinWave
    }
}
