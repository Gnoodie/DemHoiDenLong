using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.UI
{
    /// <summary>
    /// Màn hình Lore — hiển thị cốt truyện chung (Prologue/Epilogue)
    /// và lore riêng của từng Lân qua hệ thống tab.
    /// Gán NarrativeData (SO) + danh sách LanConfig vào Inspector.
    /// </summary>
    public class LoreUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private NarrativeData narrativeData;
        [SerializeField] private List<LanConfig> lanConfigs;

        // --- Panels ---
        [Header("Panels (chọn một trong ba hiển thị)")]
        [SerializeField] private GameObject prologuePanel;
        [SerializeField] private GameObject lanDetailPanel;
        [SerializeField] private GameObject epiloguePanel;

        // --- Prologue ---
        [Header("Prologue UI")]
        [SerializeField] private TextMeshProUGUI prologueTitleText;
        [SerializeField] private TextMeshProUGUI prologueBodyText;

        // --- Epilogue ---
        [Header("Epilogue UI")]
        [SerializeField] private TextMeshProUGUI epilogueTitleText;
        [SerializeField] private TextMeshProUGUI epilogueBodyText;

        // --- Character Detail ---
        [Header("Character Detail UI")]
        [SerializeField] private Image lanIconImage;
        [SerializeField] private TextMeshProUGUI lanNameText;
        [SerializeField] private TextMeshProUGUI lanNicknameText;
        [SerializeField] private TextMeshProUGUI lanChapterText;
        [SerializeField] private TextMeshProUGUI lanStoryTitleText;
        [SerializeField] private TextMeshProUGUI lanStoryBodyText;
        [SerializeField] private TextMeshProUGUI lanRoleText;
        [SerializeField] private TextMeshProUGUI lanTokenText;
        [SerializeField] private TextMeshProUGUI lanSkillNameText;
        [SerializeField] private TextMeshProUGUI lanSkillDescText;

        // --- Tab Buttons ---
        [Header("Tab Buttons (Prologue | Lan0 | Lan1 | ... | Epilogue)")]
        [SerializeField] private Button prologueTabBtn;
        [SerializeField] private List<Button> lanTabButtons;  // 1 button per Lan, same order as lanConfigs
        [SerializeField] private Button epilogueTabBtn;

        private void Start()
        {
            // Guard
            if (narrativeData == null)
            {
                Debug.LogError("LoreUI: NarrativeData chưa được gán!");
                return;
            }

            // Wire tab buttons
            prologueTabBtn.onClick.AddListener(ShowPrologue);
            epilogueTabBtn.onClick.AddListener(ShowEpilogue);

            for (int i = 0; i < lanTabButtons.Count && i < lanConfigs.Count; i++)
            {
                int index = i; // closure capture
                lanTabButtons[i].onClick.AddListener(() => ShowLanDetail(index));
            }

            // Default: mở Prologue khi vào màn hình
            ShowPrologue();
        }

        // -----------------------------------------------------------------------
        // Public methods (có thể gọi từ nút bấm trực tiếp trong Inspector)
        // -----------------------------------------------------------------------

        public void ShowPrologue()
        {
            SetActivePanel(prologuePanel);
            if (prologueTitleText) prologueTitleText.text = narrativeData.prologueTitle;
            if (prologueBodyText)  prologueBodyText.text  = narrativeData.prologueBody;
        }

        public void ShowEpilogue()
        {
            SetActivePanel(epiloguePanel);
            if (epilogueTitleText) epilogueTitleText.text = narrativeData.epilogueTitle;
            if (epilogueBodyText)  epilogueBodyText.text  = narrativeData.epilogueBody;
        }

        public void ShowLanDetail(int index)
        {
            if (index < 0 || index >= lanConfigs.Count) return;
            LanConfig lan = lanConfigs[index];

            SetActivePanel(lanDetailPanel);

            if (lanIconImage)       lanIconImage.sprite    = lan.LanIcon;
            if (lanNameText)        lanNameText.text       = lan.LanName;
            if (lanNicknameText)    lanNicknameText.text   = $"〔{lan.lanNickname}〕";
            if (lanChapterText)     lanChapterText.text    = lan.storyChapter;
            if (lanStoryTitleText)  lanStoryTitleText.text = lan.storyTitle;
            if (lanStoryBodyText)   lanStoryBodyText.text  = lan.storyDescription;
            if (lanRoleText)        lanRoleText.text       = $"✦ Vai trò: {lan.lanRole}";
            if (lanTokenText)       lanTokenText.text      = lan.storyToken;
            if (lanSkillNameText)   lanSkillNameText.text  = $"Kỹ năng: {lan.SkillName}";
            if (lanSkillDescText)   lanSkillDescText.text  = lan.SkillDescription;
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private void SetActivePanel(GameObject target)
        {
            prologuePanel.SetActive(prologuePanel == target);
            lanDetailPanel.SetActive(lanDetailPanel == target);
            epiloguePanel.SetActive(epiloguePanel == target);
        }
    }
}
