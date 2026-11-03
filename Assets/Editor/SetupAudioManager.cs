using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Audio;

public class SetupAudioManager
{
    [MenuItem("Tools/DemHoiDenLong/Setup Audio Manager")]
    public static void Setup()
    {
        // 1. Create or find AudioManager object
        AudioManager audioManager = Object.FindAnyObjectByType<AudioManager>();
        GameObject audioObj;
        if (audioManager == null)
        {
            audioObj = new GameObject("AudioManager");
            audioManager = audioObj.AddComponent<AudioManager>();
        }
        else
        {
            audioObj = audioManager.gameObject;
        }

        // 2. Load Clips
        string audioPath = "Assets/_Project/Audio/";
        
        audioManager.bgmMainMenu = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_main_menu.wav") 
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_main_menu.mp3");
        audioManager.bgmGameplay = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_gameplay.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_gameplay.mp3");
        audioManager.bgmBossFight = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_boss_fight.mp3")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "bgm_boss_fight.wav");
        audioManager.jingleVictory = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "jingle_victory.wav");
        audioManager.jingleDefeat = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "jingle_defeat.wav");

        audioManager.sfxPlayerShoot = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_player_shot.wav");
        audioManager.sfxPlayerHit = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_player_hit.wav");
        audioManager.sfxPlayerDeath = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_player_death.wav") // Fallback if missing
            ?? audioManager.sfxPlayerHit; 
        audioManager.sfxShieldBreak = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_shield_break.wav");

        audioManager.sfxSkillKim = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_kim.wav");
        audioManager.sfxSkillHoa = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_hoa.mp3")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_hoa.wav");
        audioManager.sfxSkillThuy = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_thuy.wav");
        audioManager.sfxSkillVang = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_skill_vang.wav");

        audioManager.sfxEnemyHit = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_enemy_hit.wav");
        audioManager.sfxEnemyDeath = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_enemy_death.wav");
        audioManager.sfxBossAppear = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_boss_appear.wav");
        audioManager.sfxBossShoot = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_boss_shoot.wav");
        audioManager.sfxBossDeath = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_boss_death.wav");

        audioManager.sfxItemDrop = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_item_drop.ogg")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_item_drop.wav");
        audioManager.sfxPickupCoin = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_pickup_coin.wav");
        audioManager.sfxPickupHeal = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_pickup_heal.wav");
        audioManager.sfxPickupPowerup = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_pickup_powerup.wav");

        audioManager.sfxUiClick = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_ui_click.wav");
        audioManager.sfxUiError = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_ui_error.ogg")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_ui_error.wav");
        audioManager.sfxWarningBoss = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath + "sfx_warning_boss.wav");

        // Force save scene object
        EditorUtility.SetDirty(audioManager);

        // Make it a Prefab
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Core"))
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Core");

        string prefabPath = "Assets/_Project/Prefabs/Core/AudioManager.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(audioObj, prefabPath, InteractionMode.UserAction);

        Debug.Log("AudioManager setup successfully! All clips assigned.");
        Selection.activeGameObject = audioObj;
    }
}
