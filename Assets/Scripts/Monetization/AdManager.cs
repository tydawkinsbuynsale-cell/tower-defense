using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace RobotTowerDefense.Monetization
{
    /// <summary>
    /// Manages ad monetization with Unity Ads integration.
    /// Supports interstitial ads, rewarded ads, and banner ads with frequency controls.
    /// Respects "Remove Ads" IAP purchase.
    /// </summary>
#if UNITY_ADS
    public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#else
    public class AdManager : MonoBehaviour
#endif
    {
        #region Singleton
        public static AdManager Instance { get; private set; }
        #endregion

        #region Configuration
        [Header("Unity Ads Setup")]
        [SerializeField] private string androidGameId = "5678901";
        [SerializeField] private string iosGameId = "5678900";
        [SerializeField] private bool testMode = true;
        [SerializeField] private bool enableAds = true;

        [Header("Ad Placement IDs")]
        [SerializeField] private string interstitialPlacementId = "Interstitial_Android";
        [SerializeField] private string rewardedPlacementId = "Rewarded_Android";
        [SerializeField] private string bannerPlacementId = "Banner_Android";

        [Header("Frequency Controls")]
        [SerializeField] private float interstitialCooldown = 180f; // 3 minutes
        [SerializeField] private int interstitialGameplayCount = 3; // Show after 3 games
        [SerializeField] private float rewardedCooldown = 60f; // 1 minute

        [Header("Banner Settings")]
        [SerializeField] private bool showBannerOnStart = false;
        [SerializeField] private BannerPosition bannerPosition = BannerPosition.BottomCenter;

        [Header("Editor Simulation")]
        [SerializeField] private bool simulateAdsInEditor = true;
        [SerializeField] private float simulatedAdDuration = 2f;
        #endregion

        #region State
        private bool isInitialized = false;
        private string currentAdPlacement = null;
        private Action<bool> currentRewardCallback = null;

        // Frequency tracking
        private float lastInterstitialTime = -999f;
        private float lastRewardedTime = -999f;
        private int gameplayCountSinceAd = 0;

        // Ad loading state
        private bool isLoadingInterstitial = false;
        private bool isLoadingRewarded = false;
        private bool interstitialLoaded = false;
        private bool rewardedLoaded = false;
        #endregion

        #region Events
        public event Action OnAdsInitialized;
        public event Action<string> OnAdShown; // placementId
        public event Action<string> OnAdClosed; // placementId
        public event Action<string, string> OnAdFailed; // placementId, error
        public event Action<string> OnRewardEarned; // rewardType
        #endregion

        #region Unity Lifecycle
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

        private void Start()
        {
            if (enableAds)
            {
                InitializeAds();
            }
            else
            {
                Debug.Log("[AdManager] Ads disabled in configuration");
            }
        }
        #endregion

        #region Initialization
        private void InitializeAds()
        {
            // Check if ads have been removed via IAP
            if (IAPManager.Instance != null && IAPManager.Instance.AreAdsRemoved())
            {
                Debug.Log("[AdManager] Ads removed via IAP - skipping initialization");
                return;
            }

#if UNITY_ADS
            string gameId = GetGameId();

            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("[AdManager] Game ID not configured");
                return;
            }

            Debug.Log($"[AdManager] Initializing Unity Ads - Game ID: {gameId}, Test Mode: {testMode}");
            Advertisement.Initialize(gameId, testMode, this);
#else
            Debug.LogWarning("[AdManager] Unity Ads SDK not installed. Install via Package Manager.");
            SimulateInitialization();
#endif
        }

        private string GetGameId()
        {
#if UNITY_ANDROID
            return androidGameId;
#elif UNITY_IOS
            return iosGameId;
#else
            return androidGameId; // Default for editor
#endif
        }

        private void SimulateInitialization()
        {
            if (Application.isEditor && simulateAdsInEditor)
            {
                Debug.Log("[AdManager] [SIMULATION] Ads initialized (editor mode)");
                isInitialized = true;
                OnAdsInitialized?.Invoke();

                Analytics.AnalyticsManager.Instance?.TrackEvent("ads_initialized", new Dictionary<string, object>
                {
                    { "simulation", true }
                });
            }
        }
        #endregion

        #region IUnityAdsInitializationListener
#if UNITY_ADS
        public void OnInitializationComplete()
        {
            Debug.Log("[AdManager] Unity Ads initialization complete");
            isInitialized = true;

            // Load ads after initialization
            LoadInterstitialAd();
            LoadRewardedAd();

            if (showBannerOnStart)
            {
                ShowBanner();
            }

            OnAdsInitialized?.Invoke();

            Analytics.AnalyticsManager.Instance?.TrackEvent("ads_initialized", new Dictionary<string, object>
            {
                { "test_mode", testMode }
            });
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[AdManager] Unity Ads initialization failed: {error} - {message}");

            Analytics.AnalyticsManager.Instance?.TrackEvent("ads_init_failed", new Dictionary<string, object>
            {
                { "error", error.ToString() },
                { "message", message }
            });
        }
#endif
        #endregion

        #region Interstitial Ads
        public void ShowInterstitialAd(Action onComplete = null)
        {
            // Check if ads removed
            if (IAPManager.Instance != null && IAPManager.Instance.AreAdsRemoved())
            {
                Debug.Log("[AdManager] Ads removed - skipping interstitial");
                onComplete?.Invoke();
                return;
            }

            // Check cooldown
            if (Time.time - lastInterstitialTime < interstitialCooldown)
            {
                float remaining = interstitialCooldown - (Time.time - lastInterstitialTime);
                Debug.Log($"[AdManager] Interstitial on cooldown ({remaining:F0}s remaining)");
                onComplete?.Invoke();
                return;
            }

            // Check gameplay count
            if (gameplayCountSinceAd < interstitialGameplayCount)
            {
                Debug.Log($"[AdManager] Interstitial frequency not met ({gameplayCountSinceAd}/{interstitialGameplayCount} games)");
                onComplete?.Invoke();
                return;
            }

#if UNITY_ADS
            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] Ads not initialized yet");
                onComplete?.Invoke();
                return;
            }

            if (!interstitialLoaded)
            {
                Debug.Log("[AdManager] Interstitial not loaded - loading now");
                LoadInterstitialAd();
                onComplete?.Invoke();
                return;
            }

            Debug.Log("[AdManager] Showing interstitial ad");
            currentAdPlacement = interstitialPlacementId;
            Advertisement.Show(interstitialPlacementId, this);
            
            // Track attempt
            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_shown", new Dictionary<string, object>
            {
                { "type", "interstitial" },
                { "placement", interstitialPlacementId }
            });
#else
            SimulateInterstitialAd(onComplete);
#endif
        }

        private void LoadInterstitialAd()
        {
            if (isLoadingInterstitial || interstitialLoaded) return;

#if UNITY_ADS
            if (!isInitialized) return;

            Debug.Log("[AdManager] Loading interstitial ad");
            isLoadingInterstitial = true;
            Advertisement.Load(interstitialPlacementId, this);
#endif
        }

        private void SimulateInterstitialAd(Action onComplete)
        {
            if (Application.isEditor && simulateAdsInEditor)
            {
                Debug.Log("[AdManager] [SIMULATION] Showing interstitial ad (editor mode)");
                StartCoroutine(SimulateAdRoutine("interstitial", onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Call this after each game session to track frequency.
        /// </summary>
        public void OnGameplaySessionComplete()
        {
            gameplayCountSinceAd++;
            Debug.Log($"[AdManager] Gameplay session complete - Count: {gameplayCountSinceAd}");
        }
        #endregion

        #region Rewarded Ads
        public void ShowRewardedAd(string rewardType, Action<bool> onComplete)
        {
            // Rewarded ads shown even if ads removed (optional bonus)
            // But check cooldown
            if (Time.time - lastRewardedTime < rewardedCooldown)
            {
                float remaining = rewardedCooldown - (Time.time - lastRewardedTime);
                Debug.Log($"[AdManager] Rewarded ad on cooldown ({remaining:F0}s remaining)");
                onComplete?.Invoke(false);
                return;
            }

#if UNITY_ADS
            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] Ads not initialized yet");
                onComplete?.Invoke(false);
                return;
            }

            if (!rewardedLoaded)
            {
                Debug.Log("[AdManager] Rewarded ad not loaded - loading now");
                LoadRewardedAd();
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"[AdManager] Showing rewarded ad - Reward: {rewardType}");
            currentAdPlacement = rewardedPlacementId;
            currentRewardCallback = onComplete;
            Advertisement.Show(rewardedPlacementId, this);

            // Track attempt
            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_shown", new Dictionary<string, object>
            {
                { "type", "rewarded" },
                { "placement", rewardedPlacementId },
                { "reward_type", rewardType }
            });
#else
            SimulateRewardedAd(rewardType, onComplete);
#endif
        }

        private void LoadRewardedAd()
        {
            if (isLoadingRewarded || rewardedLoaded) return;

#if UNITY_ADS
            if (!isInitialized) return;

            Debug.Log("[AdManager] Loading rewarded ad");
            isLoadingRewarded = true;
            Advertisement.Load(rewardedPlacementId, this);
#endif
        }

        private void SimulateRewardedAd(string rewardType, Action<bool> onComplete)
        {
            if (Application.isEditor && simulateAdsInEditor)
            {
                Debug.Log($"[AdManager] [SIMULATION] Showing rewarded ad (editor mode) - Reward: {rewardType}");
                StartCoroutine(SimulateRewardedAdRoutine(rewardType, onComplete));
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }
        #endregion

        #region Banner Ads
        public void ShowBanner()
        {
            // Check if ads removed
            if (IAPManager.Instance != null && IAPManager.Instance.AreAdsRemoved())
            {
                Debug.Log("[AdManager] Ads removed - skipping banner");
                return;
            }

#if UNITY_ADS
            if (!isInitialized)
            {
                Debug.LogWarning("[AdManager] Ads not initialized yet");
                return;
            }

            Debug.Log("[AdManager] Showing banner ad");
            Advertisement.Banner.SetPosition(ConvertBannerPosition(bannerPosition));
            Advertisement.Banner.Show(bannerPlacementId);

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_shown", new Dictionary<string, object>
            {
                { "type", "banner" },
                { "placement", bannerPlacementId },
                { "position", bannerPosition.ToString() }
            });
#else
            if (Application.isEditor && simulateAdsInEditor)
            {
                Debug.Log($"[AdManager] [SIMULATION] Showing banner ad at {bannerPosition}");
            }
#endif
        }

        public void HideBanner()
        {
#if UNITY_ADS
            Debug.Log("[AdManager] Hiding banner ad");
            Advertisement.Banner.Hide();
#else
            if (Application.isEditor && simulateAdsInEditor)
            {
                Debug.Log("[AdManager] [SIMULATION] Hiding banner ad");
            }
#endif
        }

#if UNITY_ADS
        private BannerPosition ConvertBannerPosition(BannerPosition position)
        {
            return position;
        }
#endif
        #endregion

        #region IUnityAdsLoadListener
#if UNITY_ADS
        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log($"[AdManager] Ad loaded: {placementId}");

            if (placementId == interstitialPlacementId)
            {
                interstitialLoaded = true;
                isLoadingInterstitial = false;
            }
            else if (placementId == rewardedPlacementId)
            {
                rewardedLoaded = true;
                isLoadingRewarded = false;
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogError($"[AdManager] Failed to load ad: {placementId} - {error} - {message}");

            if (placementId == interstitialPlacementId)
            {
                isLoadingInterstitial = false;
                interstitialLoaded = false;
            }
            else if (placementId == rewardedPlacementId)
            {
                isLoadingRewarded = false;
                rewardedLoaded = false;
            }

            OnAdFailed?.Invoke(placementId, $"{error} - {message}");

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_load_failed", new Dictionary<string, object>
            {
                { "placement", placementId },
                { "error", error.ToString() },
                { "message", message }
            });

            // Retry load after delay
            StartCoroutine(RetryLoadAd(placementId, 5f));
        }

        private IEnumerator RetryLoadAd(string placementId, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (placementId == interstitialPlacementId)
            {
                LoadInterstitialAd();
            }
            else if (placementId == rewardedPlacementId)
            {
                LoadRewardedAd();
            }
        }
#endif
        #endregion

        #region IUnityAdsShowListener
#if UNITY_ADS
        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log($"[AdManager] Ad started: {placementId}");
            OnAdShown?.Invoke(placementId);

            // Pause game audio/gameplay
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log($"[AdManager] Ad clicked: {placementId}");

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_clicked", new Dictionary<string, object>
            {
                { "placement", placementId }
            });
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log($"[AdManager] Ad completed: {placementId} - {showCompletionState}");

            // Resume game
            Time.timeScale = 1f;
            AudioListener.pause = false;

            bool isRewarded = placementId == rewardedPlacementId;
            bool wasCompleted = showCompletionState == UnityAdsShowCompletionState.COMPLETED;

            if (isRewarded)
            {
                if (wasCompleted)
                {
                    // Grant reward
                    Debug.Log("[AdManager] Rewarded ad completed - granting reward");
                    currentRewardCallback?.Invoke(true);
                    OnRewardEarned?.Invoke("standard");

                    Analytics.AnalyticsManager.Instance?.TrackEvent("ad_reward_earned", new Dictionary<string, object>
                    {
                        { "placement", placementId },
                        { "reward_type", "standard" }
                    });

                    lastRewardedTime = Time.time;
                }
                else
                {
                    // Ad skipped/failed - no reward
                    Debug.Log("[AdManager] Rewarded ad not completed - no reward");
                    currentRewardCallback?.Invoke(false);
                }

                currentRewardCallback = null;

                // Reload rewarded ad
                rewardedLoaded = false;
                LoadRewardedAd();
            }
            else
            {
                // Interstitial completed
                lastInterstitialTime = Time.time;
                gameplayCountSinceAd = 0;

                // Reload interstitial ad
                interstitialLoaded = false;
                LoadInterstitialAd();
            }

            OnAdClosed?.Invoke(placementId);
            currentAdPlacement = null;

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_completed", new Dictionary<string, object>
            {
                { "placement", placementId },
                { "completion_state", showCompletionState.ToString() }
            });
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"[AdManager] Failed to show ad: {placementId} - {error} - {message}");

            // Resume game
            Time.timeScale = 1f;
            AudioListener.pause = false;

            // If rewarded ad failed, notify callback
            if (placementId == rewardedPlacementId && currentRewardCallback != null)
            {
                currentRewardCallback?.Invoke(false);
                currentRewardCallback = null;
            }

            OnAdFailed?.Invoke(placementId, $"{error} - {message}");

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_show_failed", new Dictionary<string, object>
            {
                { "placement", placementId },
                { "error", error.ToString() },
                { "message", message }
            });

            currentAdPlacement = null;

            // Reload ad
            if (placementId == interstitialPlacementId)
            {
                interstitialLoaded = false;
                LoadInterstitialAd();
            }
            else if (placementId == rewardedPlacementId)
            {
                rewardedLoaded = false;
                LoadRewardedAd();
            }
        }
#endif
        #endregion

        #region Simulation Coroutines
        private IEnumerator SimulateAdRoutine(string adType, Action onComplete)
        {
            Debug.Log($"[AdManager] [SIMULATION] Playing {adType} ad for {simulatedAdDuration}s");
            
            OnAdShown?.Invoke(adType);

            // Simulate ad duration
            yield return new WaitForSecondsRealtime(simulatedAdDuration);

            Debug.Log($"[AdManager] [SIMULATION] {adType} ad complete");
            
            OnAdClosed?.Invoke(adType);

            if (adType == "interstitial")
            {
                lastInterstitialTime = Time.time;
                gameplayCountSinceAd = 0;
            }

            onComplete?.Invoke();

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_completed", new Dictionary<string, object>
            {
                { "type", adType },
                { "simulation", true }
            });
        }

        private IEnumerator SimulateRewardedAdRoutine(string rewardType, Action<bool> onComplete)
        {
            Debug.Log($"[AdManager] [SIMULATION] Playing rewarded ad for {simulatedAdDuration}s - Reward: {rewardType}");
            
            OnAdShown?.Invoke("rewarded");

            // Simulate ad duration
            yield return new WaitForSecondsRealtime(simulatedAdDuration);

            Debug.Log($"[AdManager] [SIMULATION] Rewarded ad complete - granting reward");
            
            OnRewardEarned?.Invoke(rewardType);
            OnAdClosed?.Invoke("rewarded");

            lastRewardedTime = Time.time;
            onComplete?.Invoke(true);

            Analytics.AnalyticsManager.Instance?.TrackEvent("ad_reward_earned", new Dictionary<string, object>
            {
                { "reward_type", rewardType },
                { "simulation", true }
            });
        }
        #endregion

        #region Public Query API
        public bool IsInitialized => isInitialized;

        public bool CanShowInterstitial()
        {
            if (IAPManager.Instance != null && IAPManager.Instance.AreAdsRemoved())
                return false;

            if (!isInitialized)
                return false;

            if (Time.time - lastInterstitialTime < interstitialCooldown)
                return false;

            if (gameplayCountSinceAd < interstitialGameplayCount)
                return false;

            return true;
        }

        public bool CanShowRewarded()
        {
            if (!isInitialized)
                return false;

            if (Time.time - lastRewardedTime < rewardedCooldown)
                return false;

            return true;
        }

        public float GetInterstitialCooldownRemaining()
        {
            return Mathf.Max(0f, interstitialCooldown - (Time.time - lastInterstitialTime));
        }

        public float GetRewardedCooldownRemaining()
        {
            return Mathf.Max(0f, rewardedCooldown - (Time.time - lastRewardedTime));
        }

        public int GetGameplayCountUntilAd()
        {
            return Mathf.Max(0, interstitialGameplayCount - gameplayCountSinceAd);
        }
        #endregion

        #region Context Menu Testing
        [ContextMenu("Show Interstitial Ad")]
        private void TestShowInterstitial()
        {
            ShowInterstitialAd(() =>
            {
                Debug.Log("[AdManager] [TEST] Interstitial ad complete");
            });
        }

        [ContextMenu("Show Rewarded Ad")]
        private void TestShowRewarded()
        {
            ShowRewardedAd("test_reward", (success) =>
            {
                Debug.Log($"[AdManager] [TEST] Rewarded ad complete - Success: {success}");
            });
        }

        [ContextMenu("Show Banner")]
        private void TestShowBanner()
        {
            ShowBanner();
        }

        [ContextMenu("Hide Banner")]
        private void TestHideBanner()
        {
            HideBanner();
        }

        [ContextMenu("Print Ad Status")]
        private void TestPrintStatus()
        {
            Debug.Log($"[AdManager] Status:\n" +
                      $"  Initialized: {isInitialized}\n" +
                      $"  Ads Removed: {(IAPManager.Instance != null ? IAPManager.Instance.AreAdsRemoved() : false)}\n" +
                      $"  Can Show Interstitial: {CanShowInterstitial()}\n" +
                      $"  Can Show Rewarded: {CanShowRewarded()}\n" +
                      $"  Interstitial Cooldown: {GetInterstitialCooldownRemaining():F0}s\n" +
                      $"  Rewarded Cooldown: {GetRewardedCooldownRemaining():F0}s\n" +
                      $"  Gameplay Count: {gameplayCountSinceAd}/{interstitialGameplayCount}");
        }
        #endregion
    }

    #region Enums
    public enum BannerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Center
    }
    #endregion
}
