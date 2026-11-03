using System;
using System.Collections;
using UnityEngine;

namespace DemHoiDenLong.VFX
{
    /// <summary>
    /// VfxLayer: Đơn vị chuyển động thị giác độc lập trong một hiệu ứng kỹ năng.
    /// Điều khiển scale, rotation, alpha, vị trí và màu sắc theo AnimationCurve và nhịp timeline.
    /// </summary>
    public class VfxLayer : MonoBehaviour
    {
        [Header("Thời gian")]
        public float startTime = 0f;
        public float duration = 1.0f;

        [Header("Animation Curves")]
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        public AnimationCurve alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        public AnimationCurve posXCurve = AnimationCurve.Constant(0f, 1f, 0f);
        public AnimationCurve posYCurve = AnimationCurve.Constant(0f, 1f, 0f);

        [Header("Màu sắc & Chất liệu")]
        public Gradient colorGradient;
        public bool useGradient = false;
        public Color tintColor = Color.white;
        public Vector2 scrollSpeed = Vector2.zero;

        [Header("Rendering")]
        public int sortingOrder = 18;

        private SpriteRenderer spriteRenderer;
        private ParticleSystem particleSys;
        private MaterialPropertyBlock propBlock;
        private Coroutine animCoroutine;
        private Vector3 initialLocalPos;
        private Vector3 baseScale = Vector3.one;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            particleSys = GetComponent<ParticleSystem>();
            propBlock = new MaterialPropertyBlock();
            initialLocalPos = transform.localPosition;
            baseScale = transform.localScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }

        public void Play(float customDuration = -1f)
        {
            float d = customDuration > 0f ? customDuration : duration;
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateRoutine(d));
        }

        public void Stop()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
                animCoroutine = null;
            }

            if (particleSys != null)
            {
                particleSys.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            gameObject.SetActive(false);
        }

        public void ResetLayer()
        {
            Stop();
            transform.localPosition = initialLocalPos;
            transform.localScale = baseScale;
            transform.localRotation = Quaternion.identity;

            if (spriteRenderer != null)
            {
                Color c = tintColor;
                c.a = 0f;
                spriteRenderer.color = c;
            }
        }

        private IEnumerator AnimateRoutine(float activeDuration)
        {
            gameObject.SetActive(false);

            if (startTime > 0f)
            {
                yield return new WaitForSeconds(startTime);
            }

            gameObject.SetActive(true);

            if (particleSys != null)
            {
                particleSys.Play();
            }

            float elapsed = 0f;
            while (elapsed < activeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / activeDuration);

                // 1. Scale
                float s = scaleCurve.Evaluate(t);
                transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z);

                // 2. Rotation
                float rot = rotationCurve.Evaluate(t);
                transform.localRotation = Quaternion.Euler(0f, 0f, rot);

                // 3. Position offset
                float offX = posXCurve.Evaluate(t);
                float offY = posYCurve.Evaluate(t);
                transform.localPosition = initialLocalPos + new Vector3(offX, offY, 0f);

                // 4. Color & Alpha
                if (spriteRenderer != null)
                {
                    Color baseC = useGradient && colorGradient != null ? colorGradient.Evaluate(t) : tintColor;
                    baseC.a *= alphaCurve.Evaluate(t);
                    spriteRenderer.color = baseC;
                }

                yield return null;
            }

            if (particleSys != null)
            {
                particleSys.Stop();
            }

            gameObject.SetActive(false);
            animCoroutine = null;
        }
    }
}
