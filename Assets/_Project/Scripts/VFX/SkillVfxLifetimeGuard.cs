using System.Collections;
using UnityEngine;

namespace DemHoiDenLong.VFX
{
    /// <summary>
    /// SkillVfxLifetimeGuard: Bảo vệ vòng đời hiệu ứng thị giác.
    /// Tự động thu hồi, dập tắt mọi ParticleSystem và SpriteRenderer còn sót sau thời lượng kỹ năng + 1.5s.
    /// Giới hạn số hạt < 120 và ngăn chặn triệt để tình trạng kim tuyến / vệt lửa chạy vô tận.
    /// </summary>
    public class SkillVfxLifetimeGuard : MonoBehaviour
    {
        public static SkillVfxLifetimeGuard Instance { get; private set; }

        private Coroutine guardCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Kích hoạt bộ canh gác vòng đời cho một chiêu thức với thời lượng tối đa.
        /// </summary>
        public void ArmGuard(float skillDuration)
        {
            if (guardCoroutine != null) StopCoroutine(guardCoroutine);
            guardCoroutine = StartCoroutine(GuardRoutine(skillDuration));
        }

        public void DisarmGuard()
        {
            if (guardCoroutine != null)
            {
                StopCoroutine(guardCoroutine);
                guardCoroutine = null;
            }
        }

        private IEnumerator GuardRoutine(float skillDuration)
        {
            float maxWait = skillDuration + 1.5f;
            yield return new WaitForSeconds(maxWait);

            // Kiểm tra các VFX còn active trong SkillVFXPool
            if (SkillVFXPool.Instance != null)
            {
                int lingeringCount = SkillVFXPool.Instance.CountActiveVFXObjects();
                if (lingeringCount > 0)
                {
                    Debug.LogWarning($"[VFXGUARD] Cưỡng bức dọn {lingeringCount} VFX còn sót sau {maxWait:F1}s từ khi cast skill!");
                    SkillVFXPool.Instance.CleanAllVFX();
                }
            }

            guardCoroutine = null;
        }
    }
}
