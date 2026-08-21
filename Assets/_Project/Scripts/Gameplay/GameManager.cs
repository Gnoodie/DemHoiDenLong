using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemHoiDenLong.Gameplay
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnGameStateChanged;

        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private PlayerController player;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (player != null)
            {
                player.OnPlayerDeath += HandlePlayerDeath;
            }

            // By default, if the game starts in a gameplay scene, switch to Playing
            ChangeState(GameState.Playing);
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnPlayerDeath -= HandlePlayerDeath;
            }
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);

            switch (currentState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f; // Pause everything
                    break;
                case GameState.Victory:
                    Time.timeScale = 0.5f; // Slow motion on victory for cool effect
                    break;
            }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void TriggerVictory()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Victory);
            }
        }

        private void HandlePlayerDeath()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.GameOver);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            // Assuming "MainMenu" scene is at build index 0 or named "MainMenu"
            // SceneManager.LoadScene("MainMenu");
        }
        
#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
#endif
    }
}
