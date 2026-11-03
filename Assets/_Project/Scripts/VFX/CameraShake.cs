using System.Collections;
using UnityEngine;

namespace DemHoiDenLong.VFX
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private const string SHAKE_KEY = "ScreenShakeEnabled";

        public static bool IsShakeEnabled
        {
            get => PlayerPrefs.GetInt(SHAKE_KEY, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SHAKE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void SetShakeEnabled(bool enabled)
        {
            IsShakeEnabled = enabled;
        }

        private Vector3 originalPos;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                originalPos = transform.localPosition;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Shake(float duration, float magnitude)
        {
            if (!IsShakeEnabled) return;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalPos;
            }
            shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
                float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(x, y, originalPos.z);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localPosition = originalPos;
        }

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
#endif
    }
}
