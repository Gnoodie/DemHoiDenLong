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

        private LanData currentConfig;
        private HangarUI hangarUI;

        public string LanId => currentConfig != null ? currentConfig.lanId : string.Empty;

        public void Initialize(LanData config, bool isUnlocked, HangarUI parentUI)
        {
            currentConfig = config;
            hangarUI = parentUI;

            if (iconImage != null) iconImage.sprite = config.icon;
            if (nameText != null) nameText.text = config.lanName;
            
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
            Audio.AudioManager.Instance?.PlayUIClick();
            if (hangarUI != null && currentConfig != null)
            {
                hangarUI.SelectLan(currentConfig);
            }
        }
    }
}
