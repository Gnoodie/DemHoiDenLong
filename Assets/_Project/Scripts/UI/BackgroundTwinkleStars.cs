using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundTwinkleStars : MonoBehaviour
{
    private class StarData
    {
        public Image image;
        public float baseAlpha;
        public float speed;
        public float phase;
    }

    private List<StarData> stars = new List<StarData>();

    private void Awake()
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject == this.gameObject) continue;
            img.raycastTarget = false;
            float baseA = img.color.a;
            if (baseA <= 0.01f) baseA = Random.Range(0.2f, 0.5f);

            stars.Add(new StarData
            {
                image = img,
                baseAlpha = baseA,
                speed = Random.Range(1.0f, 2.2f),
                phase = Random.Range(0f, Mathf.PI * 2f)
            });
        }
    }

    private void Start()
    {
        StartCoroutine(TwinkleRoutine());
    }

    private IEnumerator TwinkleRoutine()
    {
        // Nhap nhay nhe nhang, khong can chay moi frame neu khong can (co the chay moi frame hoac cach frame)
        while (true)
        {
            float time = Time.time;
            for (int i = 0; i < stars.Count; i++)
            {
                var s = stars[i];
                if (s.image != null)
                {
                    float factor = 0.65f + 0.35f * Mathf.Sin(time * s.speed + s.phase);
                    Color c = s.image.color;
                    c.a = s.baseAlpha * factor;
                    s.image.color = c;
                }
            }
            yield return null;
        }
    }
}
