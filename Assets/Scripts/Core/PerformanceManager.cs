using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Core
{
    /// <summary>
    /// Mobile performance manager.
    /// Handles frame rate targeting, battery saving, adaptive quality,
    /// quality presets, and memory optimization.
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        // ── Quality Presets ──────────────────────────────────────────────────

        public enum QualityPreset
        {
            Low,      // 30 FPS, reduced effects, optimized for battery
            Medium,   // 60 FPS, balanced quality and performance
            High,     // 60 FPS, all effects enabled
            Custom    // User-defined settings
        }

        [Header("Quality Settings")]
        [SerializeField] private QualityPreset currentPreset = QualityPreset.Medium;
        [SerializeField] private bool autoDetectQualityOnStart = true;

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
        [SerializeField] private float memoryWarningThresholdMB = 100f; // Low memory warning

        [Header("Effects")]
        [SerializeField] private bool enableShadows = true;
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private bool enablePostProcessing = false; // Heavy on mobile

        // Running stats
        private float frameTimeSampleSum;
        private int frameTimeSampleCount;
        private float adaptiveTimer;
        private bool isBatterySaving;
        
        // Performance metrics
        private Queue<float> fpsHistory = new Queue<float>(300); // Last 5 seconds at 60fps
        private float lastFPSUpdate;
        private float currentFPS;
        
        // Public properties
        public QualityPreset CurrentPreset => currentPreset;
        public float CurrentFPS => currentFPS;
        public float AverageFrameTimeMs => GetAverageFrameTimeMs();
        public bool IsInBatterySaveMode => isBatterySaving;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Register for low memory warnings
            Application.lowMemory += OnLowMemory;
        }

        private void Start()
        {
            // Auto-detect quality based on device capabilities
            if (autoDetectQualityOnStart)
            {
                currentPreset = DetectOptimalQuality();
            // Track FPS
            UpdateFPSTracking();

            // Adaptive quality evaluation
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

        private void UpdateFPSTracking()
        {
            // Update FPS counter
            if (Time.unscaledTime - lastFPSUpdate >= 0.5f)
            {
                currentFPS = 1f / Time.unscaledDeltaTime;
                lastFPSUpdate = Time.unscaledTime;

                // Store in history
                fpsHis& Quality Detection ──────────────────────────────────────

        private QualityPreset DetectOptimalQuality()
        {
            // Detect device capabilities
            int processorCount = SystemInfo.processorCount;
            int systemMemoryMB = SystemInfo.systemMemorySize;
            int graphicsMemoryMB = SystemInfo.graphicsMemorySize;

            Debug.Log($"[PerformanceManager] Device: {SystemInfo.deviceModel}");
            Debug.Log($"[PerformanceManager] CPU Cores: {processorCount}, RAM: {systemMemoryMB}MB, VRAM: {graphicsMemoryMB}MB");

            // Decision logic for mobile devices
            if (processorCount >= 6 && systemMemoryMB >= 4096)
            {
                return QualityPreset.High;
            }
            else if (processorCount >= 4 && systemMemoryMB >= 2048)
            {
                return QualityPreset.Medium;
            }
            else
            {
                return QualityPreset.Low;
            }
        }

        private void ApplyStartupSettings()
        {
            // Apply quality preset
            ApplyQualityPreset(currentPreset);

            // Keep screen on while playing
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Lock to landscape for tower defense gameplay
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            Debug.Log($"[PerformanceManager] Initialized with {currentPreset} quality preset");
        }

        // ── Quality Presets ──────────────────────────────────────────────────

        public void ApplyQualityPreset(QualityPreset preset)
        {
            currentPreset = preset;

            switch (preset)
            {
                case QualityPreset.Low:
                    ApplyLowQualitySettings();
                    break;
                case QualityPreset.Medium:
                    ApplyMediumQualitySettings();
                    break;
                case QualityPreset.High:
                    ApplyHighQualitySettings();
                    break;
                case QualityPreset.Custom:
                    // Keep current settings
                    break;
            }

            // Save to player prefs
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.graphicsQuality = (int)preset;
                SaveManager.Instance.Save();
            }

            Debug.Log($"[PerformanceManager] Applied {preset} quality preset");
        }

        private void ApplyLowQualitySettings()
        {
            // Frame rate: 30 FPS for battery saving
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
            targetFPS = 30;

            // Unity Quality Level: Lowest
            QualitySettings.SetQualityLevel(0, true);

            // Shadows: Off
            QualitySettings.shadows = ShadowQuality.Disable;
            enableShadows = false;

            // Particles: Reduced
            QuMemory Management ─────────────────────────────────────────────────

        private void OnLowMemory()
        {
            Debug.LogWarning("[PerformanceManager] Low memory warning received!");

            // Aggressive cleanup
            Resources.UnloadUnusedAssets();
            System.GC.Collect();

            // If not already on low quality, suggest downgrade
            if (currentPreset != QualityPreset.Low)
            {
                Debug.LogWarning("[PerformanceManager] Consider reducing quality settings");
            }
        }

        public void ForceGarbageCollection()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.Log("[PerformanceManager] Forced garbage collection");
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Set target frame rate (30, 60, or -1 for uncapped)</summary>
        public void SetTargetFrameRate(int fps)
        {
            targetFPS = fps;
            Application.targetFrameRate = fps;
            Debug.Log($"[PerformanceManager] Target FPS set to {fps}");
        }

        /// <summary>Toggle VSync on/off</summary>
        public void SetVSync(bool enabled)
        {
            vSyncEnabled = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            Debug.Log($"[PerformanceManager] VSync: {enabled}");
        }

        /// <summary>Enable/disable shadows</summary>
        public void SetShadows(bool enabled)
        {
            enableShadows = enabled;
            QualitySettings.shadows = enabled ? ShadowQuality.All : ShadowQuality.Disable;
        }

        /// <summary>Enable/disable particles</summary>
        public void SetParticles(bool enabled)
        {
            enableParticles = enabled;
            // Toggle all particle systems in scene
            var particles = FindObjectsOfType<ParticleSystem>();
            foreach (var p in particles)
            {
                if (enabled && !p.isPlaying) p.Play();
                else if (!enabled && p.isPlaying) p.Stop();
            }
        }

        /// <summary>Enable/disable post-processing effects</summary>
        public void SetPostProcessing(bool enabled)
        {
            enablePostProcessing = enabled;
            // Note: Actual post-processing volume toggle would be implemented
            // when post-processing is added to the scene
        }

        /// <summary>Get average frame time in milliseconds</summary>
        public float GetAverageFrameTimeMs()
        {
            if (frameTimeSampleCount == 0) return 0f;
            return frameTimeSampleSum / frameTimeSampleCount;
        }

        /// <summary>Get average FPS from recent history</summary>
        public float GetAverageFPS()
        {
            if (fpsHistory.Count == 0) return 0f;

            float sum = 0f;
            foreach (float fps in fpsHistory)
                sum += fps;

            return sum / fpsHistory.Count;
        }

        /// <summary>Get minimum FPS from recent history (for performance metrics)</summary>
        public float GetMinimumFPS()
        {
            if (fpsHistory.Count == 0) return 0f;

            float min = float.MaxValue;
            foreach (float fps in fpsHistory)
            {
                if (fps < min) min = fps;
            }

            return min;
        }

        /// <summary>Get maximum FPS from recent history</summary>
        public float GetMaximumFPS()
        {
            if (fpsHistory.Count == 0) return 0f;

            float max = 0f;
            foreach (float fps in fpsHistory)
            {
                if (fps > max) max = fps;
            }

            return max;
        }

        /// <summary>Check if device is currently in battery save mode</summary>
        public bool IsBatterySaveActive()
        {
            return isBatterySaving;
        }

        /// <summary>Manually trigger battery save mode</summary>
        public void SetBatterySaveMode(bool enabled)
        {
            isBatterySaving = enabled;
            Application.targetFrameRate = enabled ? batteryFPS : targetFPS;
            Debug.Log($"[PerformanceManager] Battery save mode: {enabled}");
        }

        /// <summary>Get detailed performance report</summary>
        public string GetPerformanceReport()
        {
            return $"Quality: {currentPreset}\n" +
                   $"FPS: {currentFPS:F1} (Avg: {GetAverageFPS():F1}, Min: {GetMinimumFPS():F1}, Max: {GetMaximumFPS():F1})\n" +
                   $"Frame Time: {GetAverageFrameTimeMs():F2}ms\n" +
                   $"Battery Save: {isBatterySaving}\n" +
                   $"Memory: {(System.GC.GetTotalMemory(false) / 1048576f):F1} MB\n" +
                   $"Shadows: {enableShadows}, Particles: {enableParticles}, Post: {enablePostProcessing}"
            QualitySettings.pixelLightCount = 1;
        }

        private void ApplyMediumQualitySettings()
        {
            // Frame rate: 60 FPS
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            targetFPS = 60;

            // Unity Quality Level: Medium
            QualitySettings.SetQualityLevel(2, true);

            // Shadows: Low quality, short distance
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = 50f;
            enableShadows = true;

            // Particles: Standard budget
            QualitySettings.particleRaycastBudget = 256;
            enableParticles = true;

            // Textures: Full resolution
            QualitySettings.globalTextureMipmapLimit = 0;

            // Post-processing: Off (still heavy for mobile)
            enablePostProcessing = false;

            // LOD: Balanced
            QualitySettings.lodBias = 1.0f;
            QualitySettings.maximumLODLevel = 0;

            // Skinned meshes: Normal quality
            QualitySettings.skinWeights = SkinWeights.TwoBones;

            // Anti-aliasing: 2x MSAA
            QualitySettings.antiAliasing = 2;

            // Pixel lights: Moderate
            QualitySettings.pixelLightCount = 2;
        }

        private void ApplyHighQualitySettings()
        {
            // Frame rate: 60 FPS
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            targetFPS = 60;

            // Unity Quality Level: High
            QualitySettings.SetQualityLevel(4, true);

            // Shadows: Full quality
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 100f;
            QualitySettings.shadowResolution = ShadowResolution.High;
            enableShadows = true;

            // Particles: High budget
            QualitySettings.particleRaycastBudget = 1024;
            enableParticles = true;

            // Textures: Full resolution
            QualitySettings.globalTextureMipmapLimit = 0;

            // Post-processing: Enabled
            enablePostProcessing = true;

            // LOD: High quality
            QualitySettings.lodBias = 1.5f;
            QualitySettings.maximumLODLevel = 0;

            // Skinned meshes: Four bones
            QualitySettings.skinWeights = SkinWeights.FourBones;

            // Anti-aliasing: 4x MSAA
            QualitySettings.antiAliasing = 4;

            // Pixel lights: More lights
            QualitySettings.pixelLightCount = 4;
        }
    }
}
