using UnityEngine;
using UnityEngine.UI;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.UI
{
    public class BossHUD : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private GameObject hudPanel;
        
        private BossController activeBoss;

        private void Start()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnBossSpawned += HandleBossSpawned;
                WaveManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            }
        }

        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnBossSpawned -= HandleBossSpawned;
                WaveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            }
            
            if (activeBoss != null)
            {
                activeBoss.OnHealthChanged -= UpdateHPBar;
                activeBoss.OnDeath -= HandleBossDeath;
            }
        }

        private void HandleBossSpawned(BossController boss)
        {
            activeBoss = boss;
            activeBoss.OnHealthChanged += UpdateHPBar;
            activeBoss.OnDeath += HandleBossDeath;

            if (hudPanel != null)
            {
                hudPanel.SetActive(true);
            }

            UpdateHPBar(activeBoss.CurrentHp, activeBoss.MaxHp);
        }

        private void UpdateHPBar(float currentHp, float maxHp)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHp;
                hpSlider.value = currentHp;
            }
        }

        private void HandleBossDeath()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
            
            if (activeBoss != null)
            {
                activeBoss.OnHealthChanged -= UpdateHPBar;
                activeBoss.OnDeath -= HandleBossDeath;
                activeBoss = null;
            }
        }

        private void HandleLevelCompleted()
        {
            if (hudPanel != null)
            {
                hudPanel.SetActive(false);
            }
        }
    }
}
