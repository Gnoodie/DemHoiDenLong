using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    public class LanSelectionItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Button selectButton;

        private LanConfig currentConfig;
        private HangarUI hangarUI;

        public string LanId => currentConfig != null ? currentConfig.LanId : string.Empty;

        public void Initialize(LanConfig config, bool isUnlocked, HangarUI parentUI)
        {
            currentConfig = config;
            hangarUI = parentUI;

            if (iconImage != null) iconImage.sprite = config.LanIcon;
            if (nameText != null) nameText.text = config.LanName;
            
            if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClicked);
        }

        public void UpdateUnlockState(bool isUnlocked)
        {
            if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);
        }

        private void OnClicked()
        {
            if (hangarUI != null && currentConfig != null)
            {
                hangarUI.SelectLan(currentConfig);
            }
        }
    }
}
