using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DemHoiDenLong.Meta;

namespace DemHoiDenLong.UI
{
    public class WorldMapUI : MonoBehaviour
    {
        [System.Serializable]
        public class LevelButton
        {
            public int levelIndex;
            public Button button;
            public GameObject lockIcon;
            public GameObject completeIcon;
        }

        [Header("Level Configuration")]
        [SerializeField] private List<LevelButton> levelButtons;

        private void Start()
        {
            RefreshUI();
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnDataLoaded += RefreshUI;
                SaveManager.Instance.OnDataSaved += RefreshUI;
            }
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnDataLoaded -= RefreshUI;
                SaveManager.Instance.OnDataSaved -= RefreshUI;
            }
        }

        public void RefreshUI()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
            {
                Debug.LogWarning("WorldMapUI: SaveManager or CurrentData is null.");
                return;
            }
            
            int highestUnlocked = SaveManager.Instance.CurrentData.HighestUnlockedLevel;

            foreach (var lb in levelButtons)
            {
                bool isUnlocked = lb.levelIndex <= highestUnlocked;
                bool isCompleted = lb.levelIndex < highestUnlocked;

                lb.button.interactable = isUnlocked;
                
                if (lb.lockIcon != null) 
                    lb.lockIcon.SetActive(!isUnlocked);
                    
                if (lb.completeIcon != null) 
                    lb.completeIcon.SetActive(isCompleted);
                
                lb.button.onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    int lvl = lb.levelIndex;
                    lb.button.onClick.AddListener(() => OnLevelSelected(lvl));
                }
            }
        }

        private void OnLevelSelected(int levelIndex)
        {
            Debug.Log($"WorldMapUI: Selected Level {levelIndex}. Proceeding to Gameplay Scene...");
            // Load gameplay scene here
            // UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
        }
    }
}
