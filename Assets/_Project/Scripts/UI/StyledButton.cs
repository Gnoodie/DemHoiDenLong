using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DemHoiDenLong.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class StyledButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Style Configuration")]
        [SerializeField] private ButtonStyleType styleType = ButtonStyleType.GoldPrimary;
        [SerializeField] private UiButtonStyleSet styleSet;

        [Header("Element References")]
        [SerializeField] private Image bgImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image iconImage;

        [Header("Runtime Button Reference")]
        [SerializeField] private Button button;

        private Coroutine scaleCoroutine;
        private Vector3 initialScale = Vector3.one;
        private bool lastInteractableState = true;
        private static float lastGlobalClickTime = -1f;

        public ButtonStyleType StyleType
        {
            get => styleType;
            set
            {
                styleType = value;
                ApplyStyle();
            }
        }

        public Image BackgroundImage => bgImage;
        public TMP_Text LabelText => labelText;
        public Image IconImage => iconImage;

        private void Reset()
        {
            AutoFindReferences();
            ApplyStyle();
        }

        private void OnValidate()
        {
            AutoFindReferences();
            ApplyStyle();
        }

        private void Awake()
        {
            initialScale = transform.localScale;
            AutoFindReferences();
            ApplyStyle();
        }

        private void OnEnable()
        {
            transform.localScale = initialScale;
            ApplyStyle();
        }

        private void Update()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null && button.interactable != lastInteractableState)
            {
                lastInteractableState = button.interactable;
                UpdateInteractableVisuals();
            }
        }

        public void AutoFindReferences()
        {
            if (button == null) button = GetComponent<Button>();
            if (bgImage == null) bgImage = GetComponent<Image>();

            if (labelText == null)
            {
                labelText = GetComponentInChildren<TMP_Text>(true);
            }

            if (iconImage == null)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img != bgImage && img.gameObject.name.ToLower().Contains("icon"))
                    {
                        iconImage = img;
                        break;
                    }
                }
            }
        }

        public void ApplyStyle()
        {
            // 1. Tự tải StyleSet nếu chưa được gán
            if (styleSet == null)
            {
#if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UiButtonStyleSet");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    styleSet = UnityEditor.AssetDatabase.LoadAssetAtPath<UiButtonStyleSet>(path);
                }
#endif
            }

            ButtonStyleData data = styleSet != null ? styleSet.GetStyle(styleType) : null;
            Sprite bgSprite = data?.backgroundSprite;
            Color txtColor = data != null ? data.textColor : UiButtonStyleSet.ColorGoldText;
            Color icColor = data != null ? data.iconColor : UiButtonStyleSet.ColorGoldIcon;

            // Fallback nạp sprite nếu data chưa có sprite
            if (bgSprite == null)
            {
                bgSprite = LoadFallbackSprite(styleType);
            }

            // 2. Cấu hình Background Image (Type Sliced, điều chỉnh pixelsPerUnitMultiplier)
            if (bgImage != null)
            {
                if (bgSprite != null) bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
                UpdatePixelsPerUnitMultiplier();
            }

            // 3. Cấu hình Nhãn TextMeshPro (1 dòng, không wrap, AutoSize 28-48 Bold, Overflow Ellipsis)
            if (labelText != null)
            {
                labelText.textWrappingMode = TextWrappingModes.NoWrap;
                labelText.enableWordWrapping = false;
                labelText.enableAutoSizing = true;
                labelText.fontSizeMin = 28f;
                labelText.fontSizeMax = 48f;
                labelText.fontStyle = FontStyles.Bold;
                labelText.overflowMode = TextOverflowModes.Ellipsis;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.color = txtColor;
                labelText.raycastTarget = false;
            }

            // 4. Cấu hình Icon
            if (iconImage != null)
            {
                iconImage.color = icColor;
                iconImage.raycastTarget = false;

                // Thêm/cấu hình LayoutElement để icon không bị HorizontalLayoutGroup co về 0
                LayoutElement le = iconImage.GetComponent<LayoutElement>();
                if (le == null) le = iconImage.gameObject.AddComponent<LayoutElement>();
                RectTransform iRt = iconImage.rectTransform;

                // Tránh lấy kích thước mặc định 100x100 của GameObject mới tạo trong Unity UI
                float icW = 34f;
                if (le.preferredWidth > 10f && Mathf.Abs(le.preferredWidth - 100f) > 0.01f)
                {
                    icW = le.preferredWidth;
                }
                else if (iRt.sizeDelta.x > 10f && Mathf.Abs(iRt.sizeDelta.x - 100f) > 0.01f)
                {
                    icW = iRt.sizeDelta.x;
                }

                float icH = 34f;
                if (le.preferredHeight > 10f && Mathf.Abs(le.preferredHeight - 100f) > 0.01f)
                {
                    icH = le.preferredHeight;
                }
                else if (iRt.sizeDelta.y > 10f && Mathf.Abs(iRt.sizeDelta.y - 100f) > 0.01f)
                {
                    icH = iRt.sizeDelta.y;
                }

                le.preferredWidth = icW;
                le.preferredHeight = icH;
                le.minWidth = icW;
                le.minHeight = icH;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;
                if (Mathf.Abs(iRt.sizeDelta.x - icW) > 0.01f || Mathf.Abs(iRt.sizeDelta.y - icH) > 0.01f)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            if (iRt != null) iRt.sizeDelta = new Vector2(icW, icH);
                        };
                    }
                    else
#endif
                    {
                        iRt.sizeDelta = new Vector2(icW, icH);
                    }
                }
            }

            // 5. Cấu hình Layout: Horizontal Layout Group căn giữa (Middle Center, Spacing 14, Control Child Size bật)
            EnsureLayoutGroup();

            // 6. Cập nhật trạng thái interactable
            UpdateInteractableVisuals();
        }

        private void EnsureLayoutGroup()
        {
            // Tìm container chứa icon và label để gắn HorizontalLayoutGroup
            Transform layoutTarget = transform;
            if (labelText != null && labelText.transform.parent != transform)
            {
                // Nếu icon và text nằm trong một container con (như Content)
                layoutTarget = labelText.transform.parent;
            }

            var hlg = layoutTarget.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = layoutTarget.gameObject.AddComponent<HorizontalLayoutGroup>();

            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 14f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Nếu layout group nằm trực tiếp trên nút chính, đảm bảo padding tránh hoa văn mây ở 2 đầu nút
            if (layoutTarget == transform)
            {
                hlg.padding = new RectOffset(45, 45, 8, 8);
            }
        }

        public void UpdatePixelsPerUnitMultiplier()
        {
            if (bgImage != null && bgImage.sprite != null)
            {
                float borderLeft = bgImage.sprite.border.x;
                RectTransform rt = transform as RectTransform;
                float h = rt != null ? rt.rect.height : 0f;
                if (h > 0f && borderLeft > 0f)
                {
                    // Hoa văn đầu nút trên màn hình = 0.5 * chiều cao nút
                    // borderLeft / pixelsPerUnitMultiplier = 0.5 * h => pixelsPerUnitMultiplier = (2 * borderLeft) / h
                    bgImage.pixelsPerUnitMultiplier = (borderLeft * 2f) / h;
                }
                else
                {
                    bgImage.pixelsPerUnitMultiplier = 2.5f;
                }
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null) button.interactable = interactable;
            UpdateInteractableVisuals();
        }

        private void UpdateInteractableVisuals()
        {
            bool interactable = (button == null || button.interactable);

            if (button != null)
            {
                // Tắt transition ColorTint/SpriteSwap mặc định của Button để StyledButton tự điều khiển
                button.transition = Selectable.Transition.None;
            }

            if (!interactable)
            {
                // Trạng thái không bấm được: màu nhân #8E98BC, alpha 0.6
                if (bgImage != null)
                {
                    bgImage.color = UiButtonStyleSet.ColorDisabledTint;
                }
                if (labelText != null)
                {
                    Color c = labelText.color;
                    c.a = 0.6f;
                    labelText.color = c;
                }
                if (iconImage != null)
                {
                    Color c = iconImage.color;
                    c.a = 0.6f;
                    iconImage.color = c;
                }
            }
            else
            {
                // Khôi phục màu chuẩn
                if (bgImage != null)
                {
                    bgImage.color = Color.white;
                }
                ButtonStyleData data = styleSet != null ? styleSet.GetStyle(styleType) : null;
                if (labelText != null)
                {
                    labelText.color = data != null ? data.textColor : UiButtonStyleSet.ColorGoldText;
                }
                if (iconImage != null)
                {
                    iconImage.color = data != null ? data.iconColor : UiButtonStyleSet.ColorGoldIcon;
                }
            }
        }

        // ==========================================
        // HIỆU ỨNG ẤN (PRESS EFFECT: SCALE 0.96 TRONG 0.08S)
        // ==========================================
        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            AnimateScale(0.96f, 0.08f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            AnimateScale(1.0f, 0.08f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            PlayClickSound();
        }

        private void PlayClickSound()
        {
            // Debounce click sound trong 0.06s tránh trùng lặp âm thanh
            if (Time.unscaledTime - lastGlobalClickTime > 0.06f)
            {
                lastGlobalClickTime = Time.unscaledTime;
                if (DemHoiDenLong.Audio.AudioManager.Instance != null)
                {
                    DemHoiDenLong.Audio.AudioManager.Instance.PlayUIClick();
                }
            }
        }

        private void AnimateScale(float targetScaleFactor, float duration)
        {
            if (!gameObject.activeInHierarchy)
            {
                transform.localScale = initialScale * targetScaleFactor;
                return;
            }

            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleRoutine(transform.localScale, initialScale * targetScaleFactor, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }
            transform.localScale = to;
            scaleCoroutine = null;
        }

        private Sprite LoadFallbackSprite(ButtonStyleType type)
        {
#if UNITY_EDITOR
            string filename = "";
            switch (type)
            {
                case ButtonStyleType.GoldPrimary:
                    filename = "btn_gold_play";
                    break;
                case ButtonStyleType.NavySecondary:
                    filename = "btn_navy_lore";
                    break;
                case ButtonStyleType.CoralExit:
                    filename = "btn_coral_exit";
                    break;
            }

            string[] paths = new[]
            {
                $"Assets/_Project/Art/UI/Button/{filename}.png",
                $"Assets/_Project/Art/UI/Buttons/{filename}.png"
            };

            foreach (var p in paths)
            {
                var s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) return s;
            }
#endif
            return null;
        }
    }
}
