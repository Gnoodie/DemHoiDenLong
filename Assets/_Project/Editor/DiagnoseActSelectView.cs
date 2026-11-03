using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class DiagnoseActSelectView
    {
        [MenuItem("DemHoiDenLong/Diagnose ActSelectView")]
        [InitializeOnLoadMethod]
        public static void RunDiagnosis()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                       BÁO CÁO CHẨN ĐOÁN ACTSELECTVIEW                          ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Thời gian: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} | IsPlaying: {Application.isPlaying}");

            // 1. Tìm Canvas và ActSelectView
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                sb.AppendLine("LỖI: Không tìm thấy Canvas trong Scene!");
                WriteAndLog(sb.ToString());
                return;
            }

            Transform actSelectView = canvas.transform.Find("ActSelectView");
            if (actSelectView == null)
            {
                sb.AppendLine("LỖI: Không tìm thấy ActSelectView dưới Canvas!");
                WriteAndLog(sb.ToString());
                return;
            }

            Transform safeArea = actSelectView.Find("SafeAreaRoot");
            Transform actListScroll = safeArea != null ? safeArea.Find("ActListScroll") : null;
            Transform viewport = actListScroll != null ? actListScroll.Find("Viewport") : null;
            Transform content = viewport != null ? viewport.Find("Content") : null;

            sb.AppendLine("\n--- 1. ĐO ĐẠC CÁC KHUNG CHÍNH (ActListScroll, Viewport, Content) ---");
            LogRectInfo(sb, "ActListScroll", actListScroll as RectTransform);
            LogRectInfo(sb, "Viewport", viewport as RectTransform);
            LogRectInfo(sb, "Content", content as RectTransform);

            Rect viewportWorldRect = GetWorldRect(viewport as RectTransform);
            sb.AppendLine($"Viewport World Rect: {viewportWorldRect}");

            // 2. Đo đạc từng ActCard
            sb.AppendLine("\n--- 2. ĐO ĐẠC TỪNG ACTCARD (ActCard_1, ActCard_2, ActCard_3) ---");
            string[] cardNames = { "ActCard_1", "ActCard_2", "ActCard_3" };
            foreach (var name in cardNames)
            {
                Transform card = content != null ? content.Find(name) : null;
                if (card == null)
                {
                    sb.AppendLine($"\n[!] KHÔNG TÌM THẤY '{name}' trong Content!");
                    continue;
                }

                RectTransform rt = card as RectTransform;
                sb.AppendLine($"\n>>> {name} <<<");
                sb.AppendLine($" - activeInHierarchy: {card.gameObject.activeInHierarchy} (self: {card.gameObject.activeSelf})");
                sb.AppendLine($" - rect: {rt.rect}");
                sb.AppendLine($" - sizeDelta: {rt.sizeDelta}");
                sb.AppendLine($" - anchoredPosition: {rt.anchoredPosition}");
                sb.AppendLine($" - lossyScale: {rt.lossyScale}");

                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                sb.AppendLine($" - World Corners: BottomLeft={corners[0]}, TopLeft={corners[1]}, TopRight={corners[2]}, BottomRight={corners[3]}");

                Rect cardWorldRect = GetWorldRect(rt);
                bool intersects = viewportWorldRect.Overlaps(cardWorldRect);
                sb.AppendLine($" - World Rect: {cardWorldRect}");
                sb.AppendLine($" - GIAO VỚI VIEWPORT (World Overlaps): {intersects}");

                // CanvasGroup trên chính card
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    sb.AppendLine($" - CanvasGroup trên card: alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}, ignoreParentGroups={cg.ignoreParentGroups}");
                }
                else
                {
                    sb.AppendLine(" - CanvasGroup trên card: (None)");
                }

                // Mask trên card
                Mask cardMask = card.GetComponent<Mask>();
                if (cardMask != null)
                {
                    sb.AppendLine($" - Mask trên card: enabled={cardMask.enabled}, showMaskGraphic={cardMask.showMaskGraphic}");
                }
                RectMask2D cardRMask = card.GetComponent<RectMask2D>();
                if (cardRMask != null)
                {
                    sb.AppendLine($" - RectMask2D trên card: enabled={cardRMask.enabled}");
                }

                // Image trên card
                Image cardImg = card.GetComponent<Image>();
                if (cardImg != null)
                {
                    sb.AppendLine($" - Image trên card: enabled={cardImg.enabled}, sprite={(cardImg.sprite != null ? cardImg.sprite.name : "null")}, color={cardImg.color} (alpha={cardImg.color.a}), type={cardImg.type}");
                }

                // Chuỗi cha từ ActCard lên Canvas
                sb.AppendLine(" - Chuỗi cha lên Canvas:");
                Transform curr = card.parent;
                while (curr != null)
                {
                    string indent = "     ^ ";
                    StringBuilder pInfo = new StringBuilder();
                    pInfo.Append($"{indent}[{curr.name}]");

                    Mask m = curr.GetComponent<Mask>();
                    if (m != null) pInfo.Append($" | Mask(enabled={m.enabled}, showGraphic={m.showMaskGraphic})");

                    RectMask2D rm = curr.GetComponent<RectMask2D>();
                    if (rm != null) pInfo.Append($" | RectMask2D(enabled={rm.enabled})");

                    CanvasGroup pcg = curr.GetComponent<CanvasGroup>();
                    if (pcg != null) pInfo.Append($" | CanvasGroup(alpha={pcg.alpha})");

                    Image pimg = curr.GetComponent<Image>();
                    if (pimg != null) pInfo.Append($" | Image(enabled={pimg.enabled}, colorAlpha={pimg.color.a}, cullTransparentMesh={(pimg.canvasRenderer != null && pimg.canvasRenderer.cullTransparentMesh)})");

                    sb.AppendLine(pInfo.ToString());
                    if (curr == canvas.transform) break;
                    curr = curr.parent;
                }
            }

            // 3. Danh sách Graphic nằm SAU ActListScroll trong thứ tự vẽ
            sb.AppendLine("\n--- 3. GRAPHIC NẰM SAU ACTLISTSCROLL TRONG THỨ TỰ VẼ (Có khả năng đè lên danh sách) ---");
            if (safeArea != null)
            {
                int scrollSiblingIndex = actListScroll != null ? actListScroll.GetSiblingIndex() : -1;
                sb.AppendLine($"ActListScroll Sibling Index: {scrollSiblingIndex}");

                Graphic[] allGraphics = safeArea.GetComponentsInChildren<Graphic>(true);
                List<string> occluders = new List<string>();

                foreach (var g in allGraphics)
                {
                    // Kiểm tra xem g có nằm sau actListScroll không
                    Transform t = g.transform;
                    bool isAfter = false;
                    while (t != null && t.parent != safeArea)
                    {
                        t = t.parent;
                    }
                    if (t != null && t.GetSiblingIndex() > scrollSiblingIndex)
                    {
                        isAfter = true;
                    }

                    if (isAfter)
                    {
                        Rect gWorldRect = GetWorldRect(g.rectTransform);
                        bool overlaps = viewportWorldRect.Overlaps(gWorldRect);
                        if (overlaps)
                        {
                            occluders.Add($" - [{GetHierarchyPath(g.transform)}] Image/Text: enabled={g.enabled}, color={g.color} (alpha={g.color.a}), raycastTarget={g.raycastTarget}, worldRect={gWorldRect}");
                        }
                    }
                }

                if (occluders.Count == 0)
                {
                    sb.AppendLine(" Không có Graphic nào nằm sau ActListScroll đè lên vùng Viewport.");
                }
                else
                {
                    foreach (var occ in occluders) sb.AppendLine(occ);
                }
            }

            // 4. Kiểm tra toàn bộ Missing Script trong ActSelectView
            sb.AppendLine("\n--- 4. DANH SÁCH MỌI COMPONENT MISSING SCRIPT TRONG ACTSELECTVIEW ---");
            Component[] allComps = actSelectView.GetComponentsInChildren<Component>(true);
            int missingCount = 0;
            Dictionary<string, int> missingByObject = new Dictionary<string, int>();

            foreach (var comp in allComps)
            {
                if (comp == null)
                {
                    missingCount++;
                }
            }

            // Quét từng GameObject để lấy tên chính xác
            Transform[] allTransforms = actSelectView.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                int miss = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (miss > 0)
                {
                    sb.AppendLine($" [MISSING] {GetHierarchyPath(t)}: {miss} missing script(s)");
                }
            }

            if (missingCount == 0)
            {
                sb.AppendLine(" TUYỆT VỜI: Không có component nào bị Missing Script!");
            }
            else
            {
                sb.AppendLine($" Tổng cộng phát hiện: {missingCount} missing component(s).");
            }

            sb.AppendLine("\n================================================================================");
            sb.AppendLine("                       KẾT THÚC BÁO CÁO CHẨN ĐOÁN                               ");
            sb.AppendLine("================================================================================");

            WriteAndLog(sb.ToString());
        }

        private static void LogRectInfo(StringBuilder sb, string label, RectTransform rt)
        {
            if (rt == null)
            {
                sb.AppendLine($" - {label}: NULL");
                return;
            }
            sb.AppendLine($" - {label}: rect={rt.rect}, sizeDelta={rt.sizeDelta}, anchoredPos={rt.anchoredPosition}, anchorMin={rt.anchorMin}, anchorMax={rt.anchorMax}, pivot={rt.pivot}");
            Mask m = rt.GetComponent<Mask>();
            if (m != null) sb.AppendLine($"   + Mask: enabled={m.enabled}, showGraphic={m.showMaskGraphic}");
            RectMask2D rm = rt.GetComponent<RectMask2D>();
            if (rm != null) sb.AppendLine($"   + RectMask2D: enabled={rm.enabled}");
            Image img = rt.GetComponent<Image>();
            if (img != null) sb.AppendLine($"   + Image: enabled={img.enabled}, color={img.color} (alpha={img.color.a}), cullTransparentMesh={(img.canvasRenderer != null && img.canvasRenderer.cullTransparentMesh)}");
            ScrollRect sr = rt.GetComponent<ScrollRect>();
            if (sr != null)
            {
                sb.AppendLine($"   + ScrollRect: content={(sr.content != null ? sr.content.name : "null")}, viewport={(sr.viewport != null ? sr.viewport.name : "null")}, vertical={sr.vertical}, horizontal={sr.horizontal}, movementType={sr.movementType}");
            }
            VerticalLayoutGroup vlg = rt.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                sb.AppendLine($"   + VerticalLayoutGroup: spacing={vlg.spacing}, padding=[L:{vlg.padding.left}, R:{vlg.padding.right}, T:{vlg.padding.top}, B:{vlg.padding.bottom}], controlWidth={vlg.childControlWidth}, controlHeight={vlg.childControlHeight}, forceExpandWidth={vlg.childForceExpandWidth}, forceExpandHeight={vlg.childForceExpandHeight}");
            }
            ContentSizeFitter csf = rt.GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                sb.AppendLine($"   + ContentSizeFitter: horizontalFit={csf.horizontalFit}, verticalFit={csf.verticalFit}");
            }
        }

        private static Rect GetWorldRect(RectTransform rt)
        {
            if (rt == null) return Rect.zero;
            Vector3[] c = new Vector3[4];
            rt.GetWorldCorners(c);
            float width = Vector3.Distance(c[0], c[3]);
            float height = Vector3.Distance(c[0], c[1]);
            return new Rect(c[0].x, c[0].y, width, height);
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "";
            string path = t.name;
            Transform curr = t.parent;
            while (curr != null)
            {
                path = curr.name + "/" + path;
                curr = curr.parent;
            }
            return path;
        }

        private static void WriteAndLog(string report)
        {
            Debug.Log(report);
            string outPath = Path.Combine(Application.dataPath, "../diagnose_report.txt");
            try
            {
                File.WriteAllText(outPath, report, Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Không thể ghi file diagnose_report.txt: " + ex.Message);
            }
        }
    }
}
