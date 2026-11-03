using System;
using System.Collections;
using UnityEngine;

namespace DemHoiDenLong.Monetization
{
    public class MonetizationManager : MonoBehaviour
    {
        public static MonetizationManager Instance { get; private set; }

        // --- IAP Product IDs ---
        public const string IAP_DIAMONDS_SMALL = "com.yourstudio.demhoidenlong.diamonds_small";
        public const string IAP_DIAMONDS_MEDIUM = "com.yourstudio.demhoidenlong.diamonds_medium";
        public const string IAP_REMOVE_ADS = "com.yourstudio.demhoidenlong.remove_ads";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region REWARDED ADS
        
        /// <summary>
        /// Hiển thị quảng cáo video phần thưởng (Rewarded Ad).
        /// </summary>
        /// <param name="onAdCompleted">Trả về true nếu người chơi xem hết, false nếu skip hoặc lỗi.</param>
        public void ShowRewardedAd(Action<bool> onAdCompleted)
        {
            Debug.Log("[Monetization] Yêu cầu xem quảng cáo Rewarded Ad...");

            // Giả lập xem quảng cáo mất 2 giây trong Editor
            StartCoroutine(SimulateAdViewing(onAdCompleted));
        }

        private IEnumerator SimulateAdViewing(Action<bool> onAdCompleted)
        {
            Time.timeScale = 0f; // Tạm dừng game khi xem ads
            
            // TODO: Ở đây sau này sẽ gọi code thật của Unity Ads hoặc Google AdMob
            // Ví dụ: Advertisement.Show("Rewarded_Android", this);
            
            Debug.Log("[Monetization] Đang phát quảng cáo giả lập... (Đợi 2 giây thực tế)");
            
            // Chờ 2 giây thực (không bị ảnh hưởng bởi timeScale)
            yield return new WaitForSecondsRealtime(2f);
            
            Debug.Log("[Monetization] Đã xem xong quảng cáo!");
            Time.timeScale = 1f; // Tiếp tục game
            
            // Trả về true (thành công)
            onAdCompleted?.Invoke(true);
        }

        #endregion

        #region IN-APP PURCHASES (IAP)

        /// <summary>
        /// Mua một gói vật phẩm bằng tiền thật.
        /// </summary>
        public void PurchaseProduct(string productId, Action<bool> onPurchaseCompleted)
        {
            Debug.Log($"[Monetization] Yêu cầu mua gói IAP: {productId}...");

            // TODO: Sau này sẽ gọi hàm BuyProductID của Unity IAP
            // m_StoreController.InitiatePurchase(productId);

            // Giả lập xử lý thanh toán thành công ngay lập tức
            bool success = true;
            
            if (success)
            {
                Debug.Log($"[Monetization] Thanh toán thành công gói {productId}!");
                GrantIAPReward(productId);
            }
            else
            {
                Debug.Log($"[Monetization] Thanh toán thất bại hoặc bị hủy.");
            }

            onPurchaseCompleted?.Invoke(success);
        }

        private void GrantIAPReward(string productId)
        {
            // Tặng đồ cho người chơi dựa theo ID gói
            switch (productId)
            {
                case IAP_DIAMONDS_SMALL:
                    DemHoiDenLong.Meta.CurrencyManager.Instance?.AddDiamonds(100);
                    break;
                case IAP_DIAMONDS_MEDIUM:
                    DemHoiDenLong.Meta.CurrencyManager.Instance?.AddDiamonds(500);
                    break;
                case IAP_REMOVE_ADS:
                    PlayerPrefs.SetInt("HasRemovedAds", 1);
                    PlayerPrefs.Save();
                    break;
            }
        }

        #endregion
    }
}
