using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Audio;

namespace DemHoiDenLong.UI
{
    [RequireComponent(typeof(Button))]
    public class OpenSettingsButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            NormalizeButtonVisuals();
            RemoveOldGearButtonsInCanvas();
        }

        private void Start()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OpenSettings);
            NormalizeButtonVisuals();
            RemoveOldGearButtonsInCanvas();
        }

        public void NormalizeButtonVisuals()
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-28f, -28f);
                rt.sizeDelta = new Vector2(64f, 64f);
                rt.localScale = Vector3.one;
            }

            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.preserveAspect = true;
                img.color = Color.white;
                img.raycastTarget = true;
                if (img.sprite == null || img.sprite.name.ToLower().Contains("gear"))
                {
                    img.sprite = Resources.Load<Sprite>("UI/setting_button") 
                              ?? Resources.Load<Sprite>("setting_button");
                }
            }

            // Remove legacy text if any
            Transform textTr = transform.Find("Text");
            if (textTr != null)
            {
                textTr.gameObject.SetActive(false);
            }
        }

        private void RemoveOldGearButtonsInCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == null || img.gameObject == gameObject) continue;
                if (img.sprite != null && (img.sprite.name.ToLower() == "gear" || img.sprite.name.ToLower().Contains("gear")))
                {
                    GameObject target = img.gameObject;
                    if (target.transform.parent != null && (target.name == "Icon" || target.transform.parent.GetComponent<Button>() != null))
                    {
                        target = target.transform.parent.gameObject;
                    }
                    if (target != gameObject)
                    {
                        Debug.Log($"[OpenSettingsButton] Auto-removing old gear button: '{target.name}'");
                        Destroy(target);
                    }
                }
            }
        }

        public void OpenSettings()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }

            if (SettingsUI.Instance != null)
            {
                SettingsUI.Instance.gameObject.SetActive(true);
                SettingsUI.Instance.transform.SetAsLastSibling();
            }
            else
            {
                // Fallback 1: search for inactive SettingsUI in scene
                SettingsUI[] allSettings = Resources.FindObjectsOfTypeAll<SettingsUI>();
                bool found = false;
                foreach (var s in allSettings)
                {
                    if (s.gameObject.scene.IsValid())
                    {
                        s.gameObject.SetActive(true);
                        s.transform.SetAsLastSibling();
                        found = true;
                        break;
                    }
                }

                // Fallback 2: instantiate from Resources
                if (!found)
                {
                    GameObject prefab = Resources.Load<GameObject>("UI/SettingsUI") 
                                     ?? Resources.Load<GameObject>("SettingsUI");

#if UNITY_EDITOR
                    if (prefab == null)
                    {
                        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/SettingsUI.prefab");
                    }
#endif

                    if (prefab != null)
                    {
                        Canvas canvas = GetComponentInParent<Canvas>();
                        if (canvas == null)
                        {
                            canvas = Object.FindAnyObjectByType<Canvas>();
                        }

                        if (canvas != null)
                        {
                            GameObject uiObj = Instantiate(prefab, canvas.transform);
                            uiObj.name = "SettingsUI";
                            uiObj.SetActive(true);
                            uiObj.transform.SetAsLastSibling();
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    Debug.LogWarning("[OpenSettingsButton] SettingsUI Instance could not be found or loaded!");
                }
            }
        }
    }
}
