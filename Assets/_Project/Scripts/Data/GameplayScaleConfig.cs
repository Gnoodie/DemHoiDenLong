using UnityEngine;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// Cấu hình kích thước hiển thị và collider cho các đối tượng trong Gameplay (Giai đoạn 5A.3).
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayScaleConfig", menuName = "DemHoiDenLong/Gameplay Scale Config")]
    public class GameplayScaleConfig : ScriptableObject
    {
        [Header("Camera Reference")]
        [Tooltip("Orthographic size of camera in 9:16 reference")]
        public float referenceOrthoSize = 5f;
        public float referenceAspectWidth = 9f;
        public float referenceAspectHeight = 16f;

        public float ReferenceCameraWidth => (referenceOrthoSize * 2f) * (referenceAspectWidth / referenceAspectHeight); // 5.625 world units

        [Header("Target Visual Width (% of 9:16 screen width)")]
        [Range(1f, 100f)] public float denGiayWidthPercent = 12f;
        [Range(1f, 100f)] public float denGaiWidthPercent = 13f;
        [Range(1f, 100f)] public float denCaWidthPercent = 12f;
        [Range(1f, 100f)] public float denQuyWidthPercent = 14f;
        [Range(1f, 100f)] public float manhGaiWidthPercent = 4f;
        [Range(1f, 100f)] public float lanWidthPercent = 10f; // Thân và đầu Lân (không tính đuôi)
        [Range(1f, 100f)] public float bossWidthPercent = 75f; // Boss cả 3 Act (uy mãnh, đồ sộ chiếm 75% màn hình)

        [Header("Collider Ratios")]
        [Range(0.1f, 1f)] public float enemyColliderRatio = 0.8f; // 0.8 sprite cho địch
        [Range(0.1f, 1f)] public float lanColliderRatio = 0.5f;   // 0.5 cho Lân (chỉ vùng đầu và thân)
    }
}
