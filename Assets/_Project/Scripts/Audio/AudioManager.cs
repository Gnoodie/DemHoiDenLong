using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DemHoiDenLong.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindAnyObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        var prefab = Resources.Load<GameObject>("AudioManager") 
                                  ?? Resources.Load<GameObject>("Core/AudioManager");
                        if (prefab != null)
                        {
                            var go = Object.Instantiate(prefab);
                            go.name = "AudioManager";
                            instance = go.GetComponent<AudioManager>();
                        }
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        private const string BGM_VOL_KEY = "BGMVolume";
        private const string SFX_VOL_KEY = "SFXVolume";

        public float MasterBGMVolume { get; private set; } = 1f;
        public float MasterSFXVolume { get; private set; } = 1f;

        [Header("BGM Clips")]
        public AudioClip bgmMainMenu;
        public AudioClip bgmGameplay;
        public AudioClip bgmBossFight;
        public AudioClip jingleVictory;
        public AudioClip jingleDefeat;

        [Header("Player SFX")]
        public AudioClip sfxPlayerShoot;
        public AudioClip sfxPlayerHit;
        public AudioClip sfxPlayerDeath; // Optional fallback
        public AudioClip sfxShieldBreak;

        [Header("Skill SFX")]
        public AudioClip sfxSkillKim;
        public AudioClip sfxSkillHoa;
        public AudioClip sfxSkillThuy;
        public AudioClip sfxSkillVang;

        [Header("Enemy/Boss SFX")]
        public AudioClip sfxEnemyHit;
        public AudioClip sfxEnemyDeath;
        public AudioClip sfxBossAppear;
        public AudioClip sfxBossShoot;
        public AudioClip sfxBossDeath;

        [Header("Item/UI SFX")]
        public AudioClip sfxItemDrop;
        public AudioClip sfxPickupCoin;
        public AudioClip sfxPickupHeal;
        public AudioClip sfxPickupPowerup;
        public AudioClip sfxUiClick;
        public AudioClip sfxUiError;
        public AudioClip sfxWarningBoss;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private int sfxSourceCount = 10;
        private List<AudioSource> sfxSources = new List<AudioSource>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes

            LoadSettings();
            InitializeAudioSources();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void LoadSettings()
        {
            MasterBGMVolume = PlayerPrefs.GetFloat(BGM_VOL_KEY, 1f);
            MasterSFXVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        }

        public void SetBGMVolume(float volume)
        {
            MasterBGMVolume = Mathf.Clamp01(volume);
            if (bgmSource != null) bgmSource.volume = MasterBGMVolume;
            PlayerPrefs.SetFloat(BGM_VOL_KEY, MasterBGMVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            MasterSFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SFX_VOL_KEY, MasterSFXVolume);
            PlayerPrefs.Save();
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            for (int i = 0; i < sfxSourceCount; i++)
            {
                AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSources.Add(sfxSource);
            }
        }

        private Coroutine bgmFadeRoutine;

        public void PlayBGM(AudioClip clip, bool fade = true, float fadeDuration = 0.8f)
        {
            if (clip == null) return;
            if (bgmSource == null) InitializeAudioSources();

            // Nếu đang phát đúng bài này
            if (bgmSource.clip == clip)
            {
                if (!bgmSource.isPlaying)
                {
                    bgmSource.volume = MasterBGMVolume;
                    bgmSource.UnPause();
                    if (!bgmSource.isPlaying) bgmSource.Play();
                }
                return;
            }

            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }

            Debug.Log($"[AUDIO] BGM Switch -> '{clip.name}' (Fade={fade})");

            if (fade && bgmSource.isPlaying)
            {
                bgmFadeRoutine = StartCoroutine(FadeBGM(clip, fadeDuration));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.volume = MasterBGMVolume;
                bgmSource.Play();
            }
        }

        public AudioClip CurrentBGM => bgmSource != null ? bgmSource.clip : null;
        public bool IsBGMPlaying => bgmSource != null && bgmSource.isPlaying;

        public void PauseBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Pause();
            }
        }

        public void ResumeBGM()
        {
            if (bgmSource != null && !bgmSource.isPlaying && bgmSource.clip != null)
            {
                bgmSource.UnPause();
            }
        }

        public void StopBGM()
        {
            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            }
        }

        private IEnumerator FadeBGM(AudioClip newClip, float duration)
        {
            float halfDur = Mathf.Max(0.01f, duration * 0.5f);
            float startVolume = bgmSource.volume;
            float targetVolume = MasterBGMVolume;

            // Fade Out
            if (bgmSource.isPlaying && startVolume > 0f)
            {
                float t = 0f;
                while (t < halfDur)
                {
                    t += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDur);
                    yield return null;
                }
                bgmSource.Stop();
            }

            // Swap and Fade In
            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float inTimer = 0f;
            while (inTimer < halfDur)
            {
                inTimer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVolume, inTimer / halfDur);
                yield return null;
            }
            bgmSource.volume = targetVolume;
            bgmFadeRoutine = null;
        }

        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null) return;

            // Find an available SFX source
            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.volume = volumeMultiplier * MasterSFXVolume;
                source.PlayOneShot(clip);
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // If all are playing, find the one with the lowest playback priority (e.g. longest playing)
            // For simplicity, just grab the first one to recycle.
            if (sfxSources.Count > 0) return sfxSources[0];
            return null;
        }

        // Convenience methods
        private float lastShootSfxTime = 0f;
        public void PlayPlayerShoot()
        {
            if (Time.unscaledTime - lastShootSfxTime >= 0.08f)
            {
                lastShootSfxTime = Time.unscaledTime;
                PlaySFX(sfxPlayerShoot, 0.45f);
            }
        }
        public void PlayPlayerHit() { PlaySFX(sfxPlayerHit); }
        public void PlayShieldBreak() { PlaySFX(sfxShieldBreak); }
        
        public void PlaySkill(string lanId)
        {
            string id = string.IsNullOrEmpty(lanId) ? "" : lanId.ToLower();
            if (id.Contains("lan1") || id.Contains("kim") || id == "1") PlaySFX(sfxSkillKim);
            else if (id.Contains("lan2") || id.Contains("hoa") || id.Contains("hoả") || id == "2") PlaySFX(sfxSkillHoa);
            else if (id.Contains("lan3") || id.Contains("thuy") || id.Contains("thuỷ") || id == "3") PlaySFX(sfxSkillThuy);
            else if (id.Contains("lan4") || id.Contains("vang") || id.Contains("vàng") || id == "4") PlaySFX(sfxSkillVang);
            else PlaySFX(sfxSkillKim); // Fallback
        }

        public void PlayEnemyHit() { PlaySFX(sfxEnemyHit, 0.8f); }
        public void PlayEnemyDeath() { PlaySFX(sfxEnemyDeath); }
        
        public void PlayBossAppear() { PlaySFX(sfxBossAppear); }
        public void PlayBossShoot() { PlaySFX(sfxBossShoot, 0.7f); }
        public void PlayBossDeath() { PlaySFX(sfxBossDeath); }

        public void PlayItemDrop() { PlaySFX(sfxItemDrop, 0.6f); }
        public void PlayPickupCoin() { PlaySFX(sfxPickupCoin); }
        public void PlayPickupHeal() { PlaySFX(sfxPickupHeal); }
        public void PlayPickupPowerup() { PlaySFX(sfxPickupPowerup); }

        public void PlayUIClick() { PlaySFX(sfxUiClick); }
        public void PlayUIError() { PlaySFX(sfxUiError); }
        public void PlayWarningBoss() { PlaySFX(sfxWarningBoss); }

#if UNITY_EDITOR
        private void Reset()
        {
            AutoAssignClipsInEditor();
        }

        public void AutoAssignClipsInEditor()
        {
            string audioPath = "Assets/_Project/Audio/";
            if (bgmMainMenu == null) bgmMainMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_main_menu.wav") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_main_menu.mp3");
            if (bgmGameplay == null) bgmGameplay = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_gameplay.wav") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_gameplay.mp3");
            if (bgmBossFight == null) bgmBossFight = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_boss_fight.mp3") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_boss_fight.wav");
            if (jingleVictory == null) jingleVictory = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "jingle_victory.wav");
            if (jingleDefeat == null) jingleDefeat = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "jingle_defeat.wav");

            if (sfxPlayerShoot == null) sfxPlayerShoot = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_player_shot.wav");
            if (sfxPlayerHit == null) sfxPlayerHit = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_player_hit.wav");
            if (sfxPlayerDeath == null) sfxPlayerDeath = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_player_death.wav") ?? sfxPlayerHit;
            if (sfxShieldBreak == null) sfxShieldBreak = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_shield_break.wav");

            if (sfxSkillKim == null) sfxSkillKim = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_kim.wav");
            if (sfxSkillHoa == null) sfxSkillHoa = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_hoa.mp3") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_hoa.wav");
            if (sfxSkillThuy == null) sfxSkillThuy = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_thuy.wav");
            if (sfxSkillVang == null) sfxSkillVang = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_vang.wav");

            if (sfxEnemyHit == null) sfxEnemyHit = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_enemy_hit.wav");
            if (sfxEnemyDeath == null) sfxEnemyDeath = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_enemy_death.wav");
            if (sfxBossAppear == null) sfxBossAppear = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_boss_appear.wav");
            if (sfxBossShoot == null) sfxBossShoot = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_boss_shoot.wav");
            if (sfxBossDeath == null) sfxBossDeath = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_boss_death.wav");

            if (sfxItemDrop == null) sfxItemDrop = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_item_drop.ogg") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_item_drop.wav");
            if (sfxPickupCoin == null) sfxPickupCoin = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_pickup_coin.wav");
            if (sfxPickupHeal == null) sfxPickupHeal = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_pickup_heal.wav");
            if (sfxPickupPowerup == null) sfxPickupPowerup = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_pickup_powerup.wav");

            if (sfxUiClick == null) sfxUiClick = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_ui_click.wav");
            if (sfxUiError == null) sfxUiError = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_ui_error.ogg") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_ui_error.wav");
            if (sfxWarningBoss == null) sfxWarningBoss = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_warning_boss.wav");
        }
#endif
    }
}
