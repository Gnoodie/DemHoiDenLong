using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace DemHoiDenLong.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Gameplay.PlayerController player;
        [SerializeField] private Gameplay.GameManager gameManager;

        [Header("Player Stats UI")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI livesText;

        [Header("State Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject pausePanel;

        private void Start()
        {
            if (player != null)
            {
                player.OnPlayerHealthChanged += UpdatePlayerStats;
                UpdatePlayerStats(player.LivesCount, player.CurrentHp, player.MaxHp);
            }

            if (gameManager == null)
            {
                gameManager = Gameplay.GameManager.Instance;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
                HandleGameStateChanged(gameManager.CurrentState);
            }
            else
            {
                HideAllPanels();
            }
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnPlayerHealthChanged -= UpdatePlayerStats;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void UpdatePlayerStats(int lives, float currentHp, float maxHp)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHp;
                hpSlider.value = currentHp;
            }

            if (hpText != null)
            {
                hpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
            }

            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }

        private void HandleGameStateChanged(Gameplay.GameState state)
        {
            HideAllPanels();

            switch (state)
            {
                case Gameplay.GameState.GameOver:
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    break;
                case Gameplay.GameState.Victory:
                    if (victoryPanel != null) victoryPanel.SetActive(true);
                    break;
                case Gameplay.GameState.Paused:
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        // Button UI callbacks
        public void OnRestartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartLevel();
            }
        }

        public void OnResumeButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.TogglePause();
            }
        }

        public void OnMainMenuButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.ReturnToMainMenu();
            }
        }
    }
}
