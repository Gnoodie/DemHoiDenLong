using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DemHoiDenLong.Data;
using DemHoiDenLong.UI;

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

public enum RunState
    {
        Playing,
        AwaitingRevive,
        Won,
        Lost,
        Finalized
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnGameStateChanged;

        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private PlayerController player;

        public GameState CurrentState => currentState;
        public RunState CurrentRunState { get; private set; } = RunState.Playing;
        private float victoryStartTime = 0f;

        public void ChangeRunState(RunState newState)
        {
            if (CurrentRunState == newState) return;
            RunState old = CurrentRunState;
            CurrentRunState = newState;
            Debug.Log($"[RUN] {Time.realtimeSinceStartup:F2}s State transition: {old} -> {newState}");
        }
        public LevelResult? LastLevelResult { get; private set; }
        public PlayerController Player => player;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SetupPlayer();

            // Check if HangarUI is in the scene. If not, we are in Gameplay scene.
            var hangarUI = FindAnyObjectByType<DemHoiDenLong.UI.HangarUI>();
            if (hangarUI != null && hangarUI.gameObject.activeSelf)
            {
                ChangeState(GameState.MainMenu);
            }
            else
            {
                ChangeState(GameState.Playing);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (player != null)
            {
                player.OnPlayerDeath -= HandlePlayerDeath;
            }
            if (CurrentRunState == RunState.AwaitingRevive || CurrentRunState == RunState.Playing)
            {
                FinalizeRun(false, "GiveUp");
            }
        }

        private void SetupPlayer()
        {
            // Nếu có player cũ trong scene, dọn dẹp để sinh đúng Lân được chọn
            var existingPlayers = FindObjectsByType<PlayerController>(FindObjectsInactive.Include);
            foreach (var p in existingPlayers)
            {
                if (p != null)
                {
                    p.OnPlayerDeath -= HandlePlayerDeath;
                    p.gameObject.SetActive(false);
                    DestroyImmediate(p.gameObject);
                }
            }
            PlayerController.ClearInstance();
            player = null;

            LanData chosenLan = LevelSession.lan ?? LanCatalog.GetPlayableLan();
            GameObject prefabToSpawn = null;
            bool isFallback = false;

            if (chosenLan != null && chosenLan.prefab != null)
            {
                prefabToSpawn = chosenLan.prefab;
            }
            else
            {
                isFallback = true;
                prefabToSpawn = Resources.Load<GameObject>("Prefabs/Lans/PF_LanHoa");
#if UNITY_EDITOR
                if (prefabToSpawn == null)
                {
                    prefabToSpawn = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab");
                }
#endif
                string lanName = chosenLan != null ? chosenLan.lanName : "Kim Lân";
                string lanId = chosenLan != null ? chosenLan.lanId : "1";
                Debug.LogWarning($"[GameManager] Lân '{lanName}' (id={lanId}) chưa có prefab trong LanData, dùng fallback PF_LanHoa với sprite đổi theo LanData.");
            }

            if (prefabToSpawn != null)
            {
                Vector3 spawnPos = new Vector3(0f, -3.5f, 0f);
                GameObject playerObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                playerObj.name = "Player_" + (chosenLan != null ? chosenLan.lanId : "1");
                player = playerObj.GetComponent<PlayerController>();

                if (player != null)
                {
                    if (isFallback && chosenLan != null && chosenLan.icon != null)
                    {
                        var sr = playerObj.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) sr.sprite = chosenLan.icon;
                    }
                    player.InitializeStats();
                    player.OnPlayerDeath += HandlePlayerDeath;
                }
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
                    if (Audio.AudioManager.Instance != null)
                    {
                        var boss = WaveManager.Instance != null ? WaveManager.Instance.ActiveBoss : null;
                        AudioClip targetBgm = (boss != null && !boss.IsDead) 
                            ? Audio.AudioManager.Instance.bgmBossFight 
                            : Audio.AudioManager.Instance.bgmGameplay;
                        Audio.AudioManager.Instance.PlayBGM(targetBgm);
                    }
                    
                    if (WaveManager.Instance != null && !WaveManager.Instance.IsLevelActive)
                    {
                        WaveManager.Instance.StartLevel(WaveManager.Instance.CurrentLevelData);
                    }
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    if (Audio.AudioManager.Instance != null)
                        Audio.AudioManager.Instance.PauseBGM();
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f; // Pause everything
                    if (Audio.AudioManager.Instance != null)
                    {
                        Audio.AudioManager.Instance.StopBGM();
                        Audio.AudioManager.Instance.PlaySFX(Audio.AudioManager.Instance.jingleDefeat);
                    }
                    break;
                case GameState.Victory:
                    Time.timeScale = 1f; // Standard timeScale on victory
                    if (Audio.AudioManager.Instance != null)
                    {
                        Audio.AudioManager.Instance.StopBGM();
                        Audio.AudioManager.Instance.PlaySFX(Audio.AudioManager.Instance.jingleVictory);
                    }
                    break;
            }
        }

        private void Update()
        {
            if (CurrentRunState == RunState.Won)
            {
                if (Time.realtimeSinceStartup - victoryStartTime >= 3.0f)
                {
                    if (GameplayHUD.Instance != null && GameplayHUD.Instance.VictoryPanelObj != null && !GameplayHUD.Instance.VictoryPanelObj.activeInHierarchy)
                    {
                        Debug.LogWarning("[VICTORY] Fail-safe 3.0s reached: forcing VictoryPanel active and interactable");
                        GameplayHUD.Instance.VictoryPanelObj.SetActive(true);
                        var cg = GameplayHUD.Instance.VictoryPanelObj.GetComponent<CanvasGroup>();
                        if (cg != null) { cg.interactable = true; cg.blocksRaycasts = true; }
                    }
                    if (CurrentRunState != RunState.Finalized)
                    {
                        Debug.LogWarning("[VICTORY] Fail-safe 3.0s reached: forcing FinalizeRun and VictoryPanel");
                        FinalizeRun(true, "FailSafe");
                        ChangeState(GameState.Victory);
                    }
                }

                bool backPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    backPressed = true;
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    backPressed = true;
                }
#endif
                if (backPressed)
                {
                    ReturnToLevelSelect();
                }
            }
            else if (currentState == GameState.Victory)
            {
                bool backPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    backPressed = true;
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    backPressed = true;
                }
#endif
                if (backPressed)
                {
                    ReturnToLevelSelect();
                }
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

        private Coroutine victoryRoutine;

        public void TriggerVictory(string reason = null)
        {
            if (CurrentRunState != RunState.Playing) return;
            ChangeRunState(RunState.Won);
            victoryStartTime = Time.realtimeSinceStartup;

            if (!Application.isPlaying)
            {
                ExecuteVictoryImmediate(reason);
                return;
            }

            if (victoryRoutine != null) StopCoroutine(victoryRoutine);
            victoryRoutine = StartCoroutine(VictorySequenceRoutine(reason));
        }

        public void ExecuteVictoryImmediate(string reason)
        {
            int levelId = LevelSession.level != null ? LevelSession.level.levelId : LevelSelectContext.SelectedLevelId;
            if (levelId <= 0) levelId = 1;

            if (string.IsNullOrEmpty(reason))
            {
                if (LevelSession.level != null)
                {
                    if (LevelSession.level.winCondition == WinCondition.SurviveTime) reason = "SurviveTime";
                    else if (LevelSession.level.winCondition == WinCondition.BossKill || LevelSession.level.isBoss) reason = "BossKill";
                    else reason = "KillCount";
                }
                else reason = "KillCount";
            }

            WaveManager.Instance?.StopLevel();
            EnemySpawner.Instance?.StopSpawning();

            if (player != null)
            {
                player.TriggerInvincibility(999f);
                player.CanShoot = false;
            }

            BulletPool.Instance?.ClearAllActiveBullets();

            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.StopBGM();
                Audio.AudioManager.Instance.PlaySFX(Audio.AudioManager.Instance.jingleVictory);
            }

            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.CleanupEnemiesOnVictory();
            }
            var remainingEnemies = FindObjectsByType<EnemyBase>();
            foreach (var eb in remainingEnemies)
            {
                if (eb != null && eb.gameObject.activeInHierarchy && !eb.IsDead)
                {
                    eb.FadeAndDespawnOnVictory();
                }
            }

            // Thu thập toàn bộ sao đang rơi trên màn
            var activeItems = FindObjectsByType<CollectibleItem>();
            if (activeItems != null)
            {
                foreach (var item in activeItems)
                {
                    if (item != null && item.gameObject.activeInHierarchy && item.Type == ItemType.Star)
                    {
                        if (player != null) player.CollectItem(item);
                        else
                        {
                            int val = Mathf.RoundToInt(item.Value);
                            LevelSession.runStars += val;
                            LevelSession.RecordStarCollected(val);
                        }
                        Destroy(item.gameObject);
                    }
                }
            }

            FinalizeRun(true, reason);
            Time.timeScale = 1f;
            ChangeState(GameState.Victory);
        }

        private System.Collections.IEnumerator VictorySequenceRoutine(string reason)
        {
            Debug.Log($"[VICTORY] {Time.realtimeSinceStartup:F2}s Step 1: Routine started, reason={reason}");

            int levelId = LevelSession.level != null ? LevelSession.level.levelId : LevelSelectContext.SelectedLevelId;
            if (levelId <= 0) levelId = 1;

            if (string.IsNullOrEmpty(reason))
            {
                if (LevelSession.level != null)
                {
                    if (LevelSession.level.winCondition == WinCondition.SurviveTime) reason = "SurviveTime";
                    else if (LevelSession.level.winCondition == WinCondition.BossKill || LevelSession.level.isBoss) reason = "BossKill";
                    else reason = "KillCount";
                }
                else reason = "KillCount";
            }

            // 1. Ngừng spawn ngay khi thắng
            WaveManager.Instance?.StopLevel();
            EnemySpawner.Instance?.StopSpawning();

            // 2. Người chơi bất tử và ngừng nhận sát thương từ địch/đạn, chặn người chơi bắn
            if (player != null)
            {
                player.TriggerInvincibility(999f);
                player.CanShoot = false;
            }
            Debug.Log($"[VICTORY] {Time.realtimeSinceStartup:F2}s Step 2: Spawners stopped, player locked");

            // 3. Đạn của địch/Boss tan đi
            BulletPool.Instance?.ClearAllActiveBullets();

            // 4. Phát âm thanh chiến thắng
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.StopBGM();
                Audio.AudioManager.Instance.PlaySFX(Audio.AudioManager.Instance.jingleVictory);
            }

            // 5. Mọi địch còn lại biến mất bằng hiệu ứng nhỏ (scale về 0 và alpha về 0 trong khoảng 0.3s, trả pool)
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.CleanupEnemiesOnVictory();
            }
            var remainingEnemies = FindObjectsByType<EnemyBase>();
            foreach (var eb in remainingEnemies)
            {
                if (eb != null && eb.gameObject.activeInHierarchy && !eb.IsDead)
                {
                    eb.FadeAndDespawnOnVictory();
                }
            }
            Debug.Log($"[VICTORY] {Time.realtimeSinceStartup:F2}s Step 3: Enemies and bullets cleared");

            // 6. Sao đang rơi trên màn lúc thắng: hút nhanh về người chơi trong khoảng 0.5 giây và cộng vào runStars
            yield return StartCoroutine(SuckStarsRoutine(0.5f));
            Debug.Log($"[VICTORY] {Time.realtimeSinceStartup:F2}s Step 4: Star suction complete, runStars={LevelSession.runStars}");

            // 7. Chờ thêm 0.3s để tổng thời gian thực đạt khoảng 0.8s (nghe nhạc và thấy hiệu ứng dọn)
            yield return new WaitForSecondsRealtime(0.3f);

            // 8. Chốt kết quả qua FinalizeRun
            FinalizeRun(true, reason);
            Debug.Log($"[VICTORY] {Time.realtimeSinceStartup:F2}s Step 5: FinalizeRun complete, activating VictoryPanel");

            // 9. Hiện VictoryPanel
            Time.timeScale = 1f;
            ChangeState(GameState.Victory);

            GameObject vPanel = GameplayHUD.Instance != null ? GameplayHUD.Instance.VictoryPanelObj : null;
            Debug.Log($"[VICTORY] {Time.realtimeSinceStartup:F2}s Step 6: VictoryPanel active={vPanel != null && vPanel.activeInHierarchy}");
        }

        private System.Collections.IEnumerator SuckStarsRoutine(float duration)
        {
            var activeItems = FindObjectsByType<CollectibleItem>();
            if (activeItems == null || activeItems.Length == 0) yield break;

            var stars = new System.Collections.Generic.List<CollectibleItem>();
            foreach (var item in activeItems)
            {
                if (item != null && item.gameObject.activeInHierarchy && item.Type == ItemType.Star)
                {
                    stars.Add(item);
                }
            }

            if (stars.Count == 0) yield break;

            Vector3 targetPos = player != null ? player.transform.position : Vector3.zero;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (player != null) targetPos = player.transform.position;

                for (int i = stars.Count - 1; i >= 0; i--)
                {
                    var star = stars[i];
                    if (star == null || !star.gameObject.activeInHierarchy)
                    {
                        stars.RemoveAt(i);
                        continue;
                    }

                    star.transform.position = Vector3.MoveTowards(star.transform.position, targetPos, 25f * Time.unscaledDeltaTime);

                    if (Vector3.Distance(star.transform.position, targetPos) < 0.6f)
                    {
                        if (player != null)
                        {
                            player.CollectItem(star);
                        }
                        else
                        {
                            int val = Mathf.RoundToInt(star.Value);
                            LevelSession.runStars += val;
                            LevelSession.RecordStarCollected(val);
                        }
                        Destroy(star.gameObject);
                        stars.RemoveAt(i);
                    }
                }

                yield return null;
            }

            // Thu thập sạch mọi sao còn sót lại
            foreach (var star in stars)
            {
                if (star != null && star.gameObject.activeInHierarchy)
                {
                    if (player != null)
                    {
                        player.CollectItem(star);
                    }
                    else
                    {
                        int val = Mathf.RoundToInt(star.Value);
                        LevelSession.runStars += val;
                        LevelSession.RecordStarCollected(val);
                    }
                    Destroy(star.gameObject);
                }
            }
        }

        private void HandlePlayerDeath()
        {
            if (CurrentRunState != RunState.Playing) return;

            if (!LevelSession.usedRevive)
            {
                ChangeRunState(RunState.AwaitingRevive);
                ChangeState(GameState.GameOver);
            }
            else
            {
                ChangeRunState(RunState.Lost);
                FinalizeRun(false, "OutOfHearts");
                ChangeState(GameState.GameOver);
            }
        }

        /// <summary>
        /// Hàm duy nhất FinalizeRun (idempotent):
        /// - Đặt Finalized và ended/resultReported
        /// - Báo tiến độ ReportLevelResult đúng một lần (có usedRevive)
        /// - Trả sao nhặt đúng một lần (thắng 100%, thua 50% làm tròn xuống)
        /// - In một dòng [RUN END] chuẩn format
        /// </summary>
        public void FinalizeRun(bool won, string reason)
        {
            if (CurrentRunState == RunState.Finalized || LevelSession.resultReported) return;

            ChangeRunState(RunState.Finalized);
            LevelSession.ended = true;
            LevelSession.resultReported = true;

            int levelId = LevelSession.level != null ? LevelSession.level.levelId : LevelSelectContext.SelectedLevelId;
            if (levelId <= 0) levelId = 1;

            float hp01 = 0f;
            int hearts = 0;
            float currentHp = 0f;
            float maxHp = 100f;

            if (player != null)
            {
                maxHp = player.MaxHp;
                if (won)
                {
                    hearts = player.LivesCount;
                    currentHp = player.CurrentHp;
                    hp01 = LevelRules.CalcHpRemaining01(player.CurrentHp, player.MaxHp, player.LivesCount, LevelRules.MaxHearts);
                }
            }

            if (LevelProgressManager.Instance != null)
            {
                LastLevelResult = LevelProgressManager.Instance.ReportLevelResult(levelId, won, hp01, LevelSession.usedRevive);
            }

            if (won)
            {
                // Thắng nhận 100% sao nhặt được trong trận
                LevelSession.AwardStars(LevelSession.runStars);
            }
            else
            {
                // Thua giữ 50% sao nhặt được trong trận (làm tròn xuống)
                int starsKept = Mathf.FloorToInt(LevelSession.runStars * GameplayBalance.DefeatStarsKeptRatio);
                LevelSession.AwardStars(starsKept);
            }

            int starsGot = 0;
            if (won)
            {
                starsGot = LastLevelResult.HasValue ? LastLevelResult.Value.starsThisRun : 1;
            }

            float elapsed = LevelSession.elapsedSeconds;
            int kills = LevelSession.killsCount;
            int targetKills = LevelSession.level != null ? LevelSession.level.targetKillCount : 0;
            float surviveSec = LevelSession.level != null ? LevelSession.level.surviveSeconds : elapsed;

            // Kills format: "kills=21/25" khi target > 0, "kills=21" khi không có mục tiêu, không bao giờ "x/0"
            string killsStr = (targetKills > 0) ? $"{kills}/{targetKills}" : $"{kills}";

            string timeStr = (reason == "SurviveTime") 
                ? $"{Mathf.FloorToInt(elapsed)}/{Mathf.FloorToInt(surviveSec)}" 
                : $"{elapsed:F1}s";

            string resultStr = won ? "WIN" : "LOSE";

            Debug.Log($"[RUN END] level={levelId} result={resultStr} reason={reason} kills={killsStr} time={timeStr} hearts={hearts} hp={Mathf.CeilToInt(currentHp)}/{Mathf.CeilToInt(maxHp)} hp01={hp01:F2} stars={starsGot} usedRevive={LevelSession.usedRevive} runStars={LevelSession.runStars}");
            LevelSession.LogTelemetry(levelId);
        }

        public void RestartLevel()
        {
            if (CurrentRunState == RunState.AwaitingRevive || CurrentRunState == RunState.Playing || CurrentRunState == RunState.Lost)
            {
                FinalizeRun(false, "GiveUp");
            }

            Time.timeScale = 1f;
            LevelSession.Reset();
            DemHoiDenLong.Data.SceneNames.LoadScene(SceneManager.GetActiveScene().name, "GameManager_Restart");
        }

        public void RevivePlayer()
        {
            if (CurrentRunState != RunState.AwaitingRevive || LevelSession.usedRevive) return;

            LevelSession.usedRevive = true;
            if (player != null)
            {
                player.Revive(1); // Cấp đúng 1 tim, đầy máu, i-frames
            }

            BulletPool.Instance?.ClearAllActiveBullets();

            Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;
            var enemies = FindObjectsByType<EnemyBase>();
            foreach (var eb in enemies)
            {
                if (eb != null && eb.gameObject.activeInHierarchy && !eb.IsDead)
                {
                    float dist = Vector2.Distance(eb.transform.position, playerPos);
                    if (dist < 4.0f)
                    {
                        Vector3 pushDir = (eb.transform.position - playerPos).normalized;
                        if (pushDir.y < 0.5f) pushDir.y = 0.8f;
                        eb.transform.position += pushDir * 3.0f;
                        eb.ApplyStun(1.5f);
                    }
                }
            }

            Time.timeScale = 1f;
            ChangeRunState(RunState.Playing);
            ChangeState(GameState.Playing);
        }

        public void ReturnToLevelSelect()
        {
            if (CurrentRunState == RunState.AwaitingRevive || CurrentRunState == RunState.Playing || CurrentRunState == RunState.Lost)
            {
                FinalizeRun(false, "GiveUp");
            }

            Time.timeScale = 1f;
            int actIndex = LevelSession.level != null ? (LevelSession.level.actIndex + 1) : 1;
            LevelSelectContext.OpenActOnStart = actIndex;
            LevelSession.Reset();
            DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.LevelSelect, "GameManager_ReturnToLevelSelect");
        }

        public void NextLevel()
        {
            Time.timeScale = 1f;
            int currentId = LevelSession.level != null ? LevelSession.level.levelId : LevelSelectContext.SelectedLevelId;
            int nextId = currentId + 1;
            LevelSelectContext.SelectedLevelId = nextId;
            LevelSelectContext.LastPlayedLevelId = nextId;
            LevelSession.Reset();
            DemHoiDenLong.Data.SceneNames.LoadScene(DemHoiDenLong.Data.SceneNames.Gameplay, "GameManager_NextLevel");
        }

        public void ReturnToMainMenu()
        {
            ReturnToLevelSelect();
        }
        
#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        public static void ResetInstanceForTesting()
        {
            Instance = null;
        }
#endif
    }
}
