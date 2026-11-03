using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Vẽ đường nét đứt cong hình chữ S giữa các node liên tiếp bằng Bezier bậc ba.
    /// Tự động cập nhật khi kéo thả node trong Scene view hoặc khi đổi trạng thái.
    /// </summary>
    [ExecuteAlways]
    public class PathDrawer : MonoBehaviour
    {
        [Header("Assets & Settings")]
        public Sprite dashSprite;
        public Vector2 dashSize = new Vector2(34f, 11f);
        public float dashSpacing = 50f;

        [Header("Colors")]
        public Color unlockedColor = new Color(1f, 0.824f, 0.478f, 1f);     // #FFD27A (1.0 alpha)
        public Color lockedColor = new Color(0.957f, 0.945f, 0.902f, 0.55f); // #F4F1E6 (0.55 alpha)
        public Color shadowColor = new Color(0.043f, 0.082f, 0.2f, 0.5f);   // #0B1533 (0.5 alpha)

        [Header("References")]
        public List<Transform> nodeTransforms = new List<Transform>();
        public List<NodeState> nodeStates = new List<NodeState>();

        private readonly List<GameObject> dashPool = new List<GameObject>();
        private RectTransform rectTransform;

        private Vector3[] lastNodePositions;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
            RebuildPath();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (HasNodePositionsChanged())
                {
                    RebuildPath();
                }
            }
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && gameObject != null)
                {
                    RebuildPath();
                }
            };
        }
#endif

        private bool HasNodePositionsChanged()
        {
            if (nodeTransforms == null || nodeTransforms.Count < 2) return false;

            if (lastNodePositions == null || lastNodePositions.Length != nodeTransforms.Count)
            {
                lastNodePositions = new Vector3[nodeTransforms.Count];
                for (int i = 0; i < nodeTransforms.Count; i++)
                {
                    if (nodeTransforms[i] != null)
                        lastNodePositions[i] = nodeTransforms[i].position;
                }
                return true;
            }

            bool changed = false;
            for (int i = 0; i < nodeTransforms.Count; i++)
            {
                if (nodeTransforms[i] == null) continue;
                if ((nodeTransforms[i].position - lastNodePositions[i]).sqrMagnitude > 0.001f)
                {
                    lastNodePositions[i] = nodeTransforms[i].position;
                    changed = true;
                }
            }
            return changed;
        }

        public void SetNodes(List<Transform> nodes, List<NodeState> states)
        {
            nodeTransforms = nodes != null ? new List<Transform>(nodes) : new List<Transform>();
            nodeStates = states != null ? new List<NodeState>(states) : new List<NodeState>();
            RebuildPath();
        }

        [ContextMenu("Rebuild Path")]
        public void RebuildPath()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null) return;
            }

            // Tìm node nếu danh sách trống
            if (nodeTransforms == null || nodeTransforms.Count < 2)
            {
                AutoFindNodes();
                if (nodeTransforms == null || nodeTransforms.Count < 2)
                {
                    DeactivateAllDashes();
                    return;
                }
            }

            CollectExistingPool();

            int poolIndex = 0;

            // Nối từng cặp node liên tiếp bằng đường cong S Bezier bậc 3
            for (int i = 0; i < nodeTransforms.Count - 1; i++)
            {
                Transform startNode = nodeTransforms[i];
                Transform endNode = nodeTransforms[i + 1];
                if (startNode == null || endNode == null) continue;

                // Tọa độ tâm trong không gian của PathRoot
                Vector3 startLocal = rectTransform.InverseTransformPoint(startNode.position);
                Vector3 endLocal = rectTransform.InverseTransformPoint(endNode.position);

                // Bezier bậc ba: 2 điểm điều khiển lệch trục dọc 40%
                float dy = endLocal.y - startLocal.y;
                Vector3 p0 = startLocal;
                Vector3 p1 = new Vector3(startLocal.x, startLocal.y + 0.4f * dy, 0f);
                Vector3 p2 = new Vector3(endLocal.x, endLocal.y - 0.4f * dy, 0f);
                Vector3 p3 = endLocal;

                // Màu sắc của đoạn: nếu node đích đã mở hoặc hiện tại thì màu ấm, ngược lại màu tối
                bool isUnlockedSegment = false;
                if (nodeStates != null && i + 1 < nodeStates.Count)
                {
                    isUnlockedSegment = (nodeStates[i + 1] != NodeState.Locked);
                }
                Color segmentColor = isUnlockedSegment ? unlockedColor : lockedColor;

                // Đo độ dài xấp xỉ đường cong
                float totalLength = ApproximateBezierLength(p0, p1, p2, p3, 24);
                if (totalLength <= 0.01f) continue;

                // Rải các vạch dash cách nhau dashSpacing
                // Chừa lề ở 2 đầu để không đè vào tâm node
                float margin = 40f;
                float startDist = margin;
                float endDist = totalLength - margin;

                if (endDist <= startDist)
                {
                    startDist = 0f;
                    endDist = totalLength;
                }

                float currentDist = startDist;
                while (currentDist <= endDist)
                {
                    float t = currentDist / totalLength;
                    Vector3 pos = EvaluateCubicBezier(p0, p1, p2, p3, t);
                    Vector3 tangent = EvaluateCubicBezierTangent(p0, p1, p2, p3, t);

                    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

                    GameObject dashObj = GetOrCreateDash(poolIndex++);
                    dashObj.SetActive(true);

                    RectTransform rt = dashObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(pos.x, pos.y);
                    rt.localRotation = Quaternion.Euler(0f, 0f, angle);
                    rt.sizeDelta = dashSize;

                    // Lớp bóng tối phía dưới (#0B1533 alpha 0.5, lệch 2px xuống dưới trên màn hình)
                    Transform shadowTrans = dashObj.transform.Find("Shadow");
                    if (shadowTrans != null)
                    {
                        RectTransform sRt = shadowTrans.GetComponent<RectTransform>();
                        sRt.sizeDelta = dashSize;
                        Vector2 shadowOffsetLocal = Quaternion.Euler(0f, 0f, -angle) * new Vector3(0f, -2f, 0f);
                        sRt.anchoredPosition = shadowOffsetLocal;

                        Image sImg = shadowTrans.GetComponent<Image>();
                        if (sImg != null)
                        {
                            if (dashSprite != null) sImg.sprite = dashSprite;
                            sImg.color = shadowColor;
                            sImg.raycastTarget = false;
                        }
                    }

                    // Lớp nét đứt chính
                    Transform mainTrans = dashObj.transform.Find("Main");
                    if (mainTrans != null)
                    {
                        RectTransform mRt = mainTrans.GetComponent<RectTransform>();
                        mRt.sizeDelta = dashSize;
                        mRt.anchoredPosition = Vector2.zero;

                        Image mImg = mainTrans.GetComponent<Image>();
                        if (mImg != null)
                        {
                            if (dashSprite != null) mImg.sprite = dashSprite;
                            mImg.color = segmentColor;
                            mImg.raycastTarget = false;
                        }
                    }

                    currentDist += dashSpacing;
                }
            }

            // Tắt các dash thừa trong pool
            for (int i = poolIndex; i < dashPool.Count; i++)
            {
                if (dashPool[i] != null)
                    dashPool[i].SetActive(false);
            }
        }

        private void AutoFindNodes()
        {
            if (transform.parent == null) return;
            nodeTransforms.Clear();

            // Tìm các Node_1, Node_2, Node_3, Node_4, Node_5 hoặc Boss trong NodesContainer
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                Transform child = transform.parent.GetChild(i);
                if (child == transform) continue; // Bỏ qua PathRoot
                if (child.name.StartsWith("Node_") || child.name.Contains("Boss") || child.name.Contains("Node"))
                {
                    nodeTransforms.Add(child);
                }
            }
        }

        private void CollectExistingPool()
        {
            dashPool.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Dash_"))
                {
                    dashPool.Add(child.gameObject);
                }
            }
        }

        private void DeactivateAllDashes()
        {
            CollectExistingPool();
            foreach (var dash in dashPool)
            {
                if (dash != null) dash.SetActive(false);
            }
        }

        private GameObject GetOrCreateDash(int index)
        {
            GameObject dashObj;
            if (index < dashPool.Count && dashPool[index] != null)
            {
                dashObj = dashPool[index];
            }
            else
            {
                dashObj = new GameObject($"Dash_{index}", typeof(RectTransform));
                dashObj.transform.SetParent(transform, false);
                dashPool.Add(dashObj);
            }

            // Tắt Image trên gốc nếu có từ trước
            Image rootImg = dashObj.GetComponent<Image>();
            if (rootImg != null) rootImg.enabled = false;

            // 1. Tạo hoặc lấy con Shadow (sibling 0: render trước)
            Transform shadowTrans = dashObj.transform.Find("Shadow");
            if (shadowTrans == null)
            {
                GameObject sObj = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                sObj.transform.SetParent(dashObj.transform, false);
                sObj.transform.SetSiblingIndex(0);
            }

            // 2. Tạo hoặc lấy con Main (sibling 1: render sau, đè lên shadow)
            Transform mainTrans = dashObj.transform.Find("Main");
            if (mainTrans == null)
            {
                GameObject mObj = new GameObject("Main", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                mObj.transform.SetParent(dashObj.transform, false);
                mObj.transform.SetSiblingIndex(1);
            }

            return dashObj;
        }

        private static Vector3 EvaluateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3f * uu * t * p1;
            p += 3f * u * tt * p2;
            p += ttt * p3;
            return p;
        }

        private static Vector3 EvaluateCubicBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            return 3f * u * u * (p1 - p0) + 6f * u * t * (p2 - p1) + 3f * t * t * (p3 - p2);
        }

        private static float ApproximateBezierLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps)
        {
            float length = 0f;
            Vector3 prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector3 curr = EvaluateCubicBezier(p0, p1, p2, p3, t);
                length += Vector3.Distance(prev, curr);
                prev = curr;
            }
            return length;
        }
    }
}
