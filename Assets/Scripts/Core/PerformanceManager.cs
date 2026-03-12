using UnityEngine;
using System.Collections;

namespace RobotTD.Core
{
    /// <summary>
    /// Mobile performance manager.
    /// Handles frame rate targeting, battery saving, adaptive quality,
    /// and warm-up of object pools on scene load.
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [Header("Frame Rate")]
        [SerializeField] private int targetFPS = 60;
        [SerializeField] private int batteryFPS = 30;          // Used when battery < threshold
        [SerializeField, Range(0, 100)] private int batteryThreshold = 20;
        [SerializeField] private bool vSyncEnabled = false;

        [Header("Adaptive Quality")]
        [SerializeField] private bool enableAdaptiveQuality = true;
        [SerializeField] private float adaptiveCheckInterval = 5f; // seconds
        [SerializeField] private float targetFrameTimeMs = 16.7f;  // 60 fps target
        [SerializeField] private float upgradeThresholdMs = 14f;   // If faster, try higher quality
        [SerializeField] private float downgradeThresholdMs = 22f; // If slower, drop quality

        [Header("Memory")]
        [SerializeField] private bool gcOnSceneTransition = true;
        [SerializeField] private int textureQuality = 0; // 0 = full, 1 = half...

        // Running stats
        private float frameTimeSampleSum;
        private int frameTimeSampleCount;
        private float adaptiveTimer;
        private bool isBatterySaving;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyStartupSettings();
        }

        private void Start()
        {
            StartCoroutine(BatteryMonitor());
        }

        private void Update()
        {
            if (!enableAdaptiveQuality) return;

            frameTimeSampleSum   += Time.unscaledDeltaTime * 1000f;
            frameTimeSampleCount += 1;

            adaptiveTimer += Time.unscaledDeltaTime;
            if (adaptiveTimer >= adaptiveCheckInterval)
            {
                adaptiveTimer = 0f;
                EvaluateAdaptiveQuality();
                frameTimeSampleSum   = 0f;
                frameTimeSampleCount = 0;
            }
        }

        // ── Startup ──────────────────────────────────────────────────────────

        private void ApplyStartupSettings()
        {
            // Frame rate
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
            Application.targetFrameRate = targetFPS;

            // Texture quality from save
            int savedQuality = SaveManager.Instance?.Data.graphicsQuality ?? 2;
            QualitySettings.SetQualityLevel(savedQuality, true);
            QualitySettings.globalTextureMipmapLimit = textureQuality;

            // Keep screen on while playing
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Lock to portrait or landscape based on game design
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            Debug.Log($"[PerformanceManager] Target FPS: {targetFPS}, Quality: {savedQuality}");
        }

        // ── Adaptive quality ─────────────────────────────────────────────────

        private void EvaluateAdaptiveQuality()
        {
            if (frameTimeSampleCount == 0) return;

            float avgFrameTime = frameTimeSampleSum / frameTimeSampleCount;
            int currentQuality = QualitySettings.GetQualityLevel();

            if (avgFrameTime > downgradeThresholdMs && currentQuality > 0)
            {
                int newQuality = currentQuality - 1;
                QualitySettings.SetQualityLevel(newQuality, true);
                Debug.Log($"[PerformanceManager] Downgraded quality to {newQuality} (avg {avgFrameTime:F1}ms)");
            }
            else if (avgFrameTime < upgradeThresholdMs && currentQuality < QualitySettings.names.Length - 1)
            {
                int newQuality = currentQuality + 1;
                QualitySettings.SetQualityLevel(newQuality, true);
                Debug.Log($"[PerformanceManager] Upgraded quality to {newQuality} (avg {avgFrameTime:F1}ms)");
            }
        }

        // ── Battery monitoring ────────────────────────────────────────────────

        private IEnumerator BatteryMonitor()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(30f);

                bool shouldSave = SystemInfo.batteryLevel >= 0 &&
                                  SystemInfo.batteryLevel * 100f <= batteryThreshold &&
                                  SystemInfo.batteryStatus != BatteryStatus.Charging;

                if (shouldSave != isBatterySaving)
                {
                    isBatterySaving = shouldSave;
                    Application.targetFrameRate = isBatterySaving ? batteryFPS : targetFPS;
                    Debug.Log($"[PerformanceManager] Battery save mode: {isBatterySaving}");
                }
            }
        }

        // ── Scene cleanup ─────────────────────────────────────────────────────

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                                   UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (gcOnSceneTransition)
            {
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Force a specific quality level and persist it.</summary>
        public void SetQuality(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
            if (SaveManager.Instance != null)
                SaveManager.Instance.Data.graphicsQuality = level;
        }

        public float GetAverageFrameTimeMs()
        {
            if (frameTimeSampleCount == 0) return 0f;
            return frameTimeSampleSum / frameTimeSampleCount;
        }

        public float GetAverageFPS()
        {
            float avgMs = GetAverageFrameTimeMs();
            return avgMs > 0 ? 1000f / avgMs : 0f;
        }
    }
}
