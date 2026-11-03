using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// Quản lý hình nền chiến đấu theo Hồi (ActData.battleBackground) và lớp tối (ActData.battleDim).
    /// Tự động scale kiểu "cover" phủ kín khung hình camera ở mọi tỉ lệ màn hình (9:16, 9:20, 3:4).
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        public static BackgroundManager Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer dimOverlayRenderer;
        [SerializeField] private Camera targetCamera;

        private float lastAspect = 0f;
        private float lastOrthoSize = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (targetCamera == null) targetCamera = Camera.main;
            if (backgroundRenderer == null) backgroundRenderer = GetComponent<SpriteRenderer>();

            EnsureDimOverlay();
        }

        private void Start()
        {
            ApplyActBackground();
        }

        private void LateUpdate()
        {
            if (targetCamera != null)
            {
                if (Mathf.Abs(targetCamera.aspect - lastAspect) > 0.001f || Mathf.Abs(targetCamera.orthographicSize - lastOrthoSize) > 0.001f)
                {
                    UpdateScale();
                }
            }
        }

        public void EnsureDimOverlay()
        {
            if (dimOverlayRenderer != null) return;

            Transform child = transform.Find("DimOverlay");
            if (child != null)
            {
                dimOverlayRenderer = child.GetComponent<SpriteRenderer>();
            }

            if (dimOverlayRenderer == null)
            {
                GameObject dimObj = new GameObject("DimOverlay");
                dimObj.transform.SetParent(transform, false);
                dimObj.transform.localPosition = new Vector3(0, 0, -0.1f); // Nằm trước nền một chút
                dimOverlayRenderer = dimObj.AddComponent<SpriteRenderer>();
                dimOverlayRenderer.sortingOrder = backgroundRenderer != null ? backgroundRenderer.sortingOrder + 1 : -90;
            }
        }

        public void ApplyActBackground()
        {
            if (targetCamera == null) targetCamera = Camera.main;

            int actIndex = 0;
            if (LevelSession.level != null)
            {
                actIndex = LevelSession.level.actIndex;
            }

            LevelDatabase db = Resources.Load<LevelDatabase>("LevelDatabase");
#if UNITY_EDITOR
            if (db == null)
            {
                db = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Data/LevelDatabase.asset");
            }
#endif
            ActData actData = db != null ? db.GetAct(actIndex) : null;
#if UNITY_EDITOR
            if (actData == null)
            {
                actData = UnityEditor.AssetDatabase.LoadAssetAtPath<ActData>($"Assets/_Project/Data/Acts/Act_{actIndex + 1}.asset");
            }
#endif

            if (actData != null)
            {
                if (actData.battleBackground == null)
                {
                    Debug.LogWarning($"[BackgroundManager] Hồi {actIndex + 1} ({actData.title}) chưa có battleBackground!");
                }
                else
                {
                    if (actData.battleBackground == actData.mapBackground)
                    {
                        Debug.LogWarning($"[BackgroundManager] battleBackground đang trùng với mapBackground ở Hồi {actIndex + 1}!");
                    }

                    if (backgroundRenderer != null)
                    {
                        backgroundRenderer.sprite = actData.battleBackground;
                    }
                }

                float dimAlpha = actData.battleDim;
                if (dimAlpha <= 0f)
                {
                    dimAlpha = (actIndex == 1) ? GameplayBalance.Act2BattleDimAlpha : GameplayBalance.DefaultBattleDimAlpha;
                }
                SetDimAlpha(dimAlpha);
            }
            else
            {
                Debug.LogWarning($"[BackgroundManager] Không tìm thấy ActData cho actIndex {actIndex}");
            }

            UpdateScale();
        }

        public void SetDimAlpha(float alpha)
        {
            EnsureDimOverlay();
            if (dimOverlayRenderer == null) return;

            // Dùng sprite trắng 1x1 hoặc sprite của background
            if (backgroundRenderer != null && backgroundRenderer.sprite != null)
            {
                dimOverlayRenderer.sprite = backgroundRenderer.sprite;
            }
            dimOverlayRenderer.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
        }

        public void UpdateScale()
        {
            if (targetCamera == null || backgroundRenderer == null || backgroundRenderer.sprite == null) return;

            lastAspect = targetCamera.aspect;
            lastOrthoSize = targetCamera.orthographicSize;

            float camHeight = targetCamera.orthographicSize * 2f;
            float camWidth = camHeight * targetCamera.aspect;

            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

            // Tỉ lệ scale kiểu Cover (phủ kín toàn bộ khung nhìn của Camera)
            float scaleX = camWidth / spriteSize.x;
            float scaleY = camHeight / spriteSize.y;
            float coverScale = Mathf.Max(scaleX, scaleY);

            transform.localScale = new Vector3(coverScale, coverScale, 1f);

            // Căn chỉnh vị trí trùng với camera (z giữ nguyên)
            Vector3 camPos = targetCamera.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);

            if (dimOverlayRenderer != null)
            {
                dimOverlayRenderer.transform.localScale = Vector3.one;
            }
        }
    }
}
