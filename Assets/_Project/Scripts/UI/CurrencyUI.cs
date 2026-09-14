using UnityEngine;
using TMPro;
using DemHoiDenLong.Meta;

namespace DemHoiDenLong.UI
{
    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI starLanternText;
        [SerializeField] private TextMeshProUGUI diamondText;

        private void Start()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnStarLanternsChanged += UpdateStarLanterns;
                CurrencyManager.Instance.OnDiamondsChanged += UpdateDiamonds;
                
                // Fetch initial values
                CurrencyManager.Instance.BroadcastCurrentCurrencies();
            }
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnStarLanternsChanged -= UpdateStarLanterns;
                CurrencyManager.Instance.OnDiamondsChanged -= UpdateDiamonds;
            }
        }

        private void UpdateStarLanterns(int amount)
        {
            if (starLanternText != null)
            {
                starLanternText.text = amount.ToString("N0");
            }
        }

        private void UpdateDiamonds(int amount)
        {
            if (diamondText != null)
            {
                diamondText.text = amount.ToString("N0");
            }
        }
    }
}
