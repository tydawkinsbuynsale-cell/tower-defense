using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RobotTD.Analytics;

namespace RobotTD.Accessibility
{
    /// <summary>
    /// Comprehensive accessibility system for inclusive gameplay.
    /// Features colorblind modes, screen reader support, remappable controls, text scaling, and one-handed mode.
    /// Ensures the game is playable by users with various accessibility needs.
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableAccessibility = true;
        [SerializeField] private bool verboseLogging = true;

        [Header("Colorblind Settings")]
        [SerializeField] private ColorblindMode defaultColorblindMode = ColorblindMode.None;
        [SerializeField] private float colorblindIntensity = 1.0f;

        [Header("Text Settings")]
        [SerializeField] private float minTextScale = 0.8f;
        [SerializeField] private float maxTextScale = 2.0f;
        [SerializeField] private float defaultTextScale = 1.0f;

        [Header("Screen Reader Settings")]
        [SerializeField] private bool enableScreenReader = false;
        [SerializeField] private float screenReaderSpeed = 1.0f;
        [SerializeField] private bool announceUIChanges = true;

        [Header("Control Settings")]
        [SerializeField] private bool enableRemappableControls = true;
        [SerializeField] private bool enableOneHandedMode = false;

        [Header("UI Settings")]
        [SerializeField] private bool enableHighContrast = false;
        [SerializeField] private bool reduceMotion = false;
        [SerializeField] private bool simplifyUI = false;

        // State
        private bool isInitialized = false;
        private AccessibilitySettings currentSettings;
        private Dictionary<string, KeyCode> controlMappings = new Dictionary<string, KeyCode>();
        private Dictionary<string, Vector2> touchZones = new Dictionary<string, Vector2>();
        private List<string> screenReaderQueue = new List<string>();
        private bool isReadingScreen = false;

        // Events
        public event Action<ColorblindMode> OnColorblindModeChanged;
        public event Action<float> OnTextScaleChanged;
        public event Action<bool> OnScreenReaderToggled;
        public event Action<bool> OnHighContrastToggled;
        public event Action<bool> OnOneHandedModeToggled;
        public event Action<string, KeyCode> OnControlRemapped;
        public event Action<string> OnScreenReaderAnnouncement; // For UI integration

        // ══════════════════════════════════════════════════════════════════════
        // ── Unity Lifecycle ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

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
            if (!enableAccessibility)
            {
                LogDebug("Accessibility system disabled");
                return;
            }

            StartCoroutine(InitializeAccessibilitySystem());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeAccessibilitySystem()
        {
            LogDebug("Initializing accessibility system...");

            yield return new WaitForSeconds(0.3f);

            // Load saved settings
            LoadAccessibilitySettings();

            if (currentSettings == null)
            {
                currentSettings = new AccessibilitySettings
                {
                    colorblindMode = defaultColorblindMode,
                    colorblindIntensity = colorblindIntensity,
                    textScale = defaultTextScale,
                    screenReaderEnabled = enableScreenReader,
                    screenReaderSpeed = screenReaderSpeed,
                    announceUIChanges = announceUIChanges,
                    highContrastEnabled = enableHighContrast,
                    reduceMotionEnabled = reduceMotion,
                    simplifyUIEnabled = simplifyUI,
                    oneHandedModeEnabled = enableOneHandedMode
                };
            }

            // Initialize default control mappings
            InitializeDefaultControlMappings();

            // Initialize default touch zones
            InitializeDefaultTouchZones();

            // Apply loaded settings
            ApplyAccessibilitySettings();

            isInitialized = true;
            LogDebug("Accessibility system initialized");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("accessibility_initialized", new Dictionary<string, object>
                {
                    { "colorblind_mode", currentSettings.colorblindMode.ToString() },
                    { "screen_reader", currentSettings.screenReaderEnabled },
                    { "high_contrast", currentSettings.highContrastEnabled },
                    { "text_scale", currentSettings.textScale },
                    { "one_handed_mode", currentSettings.oneHandedModeEnabled }
                });
            }
        }

        private void InitializeDefaultControlMappings()
        {
            controlMappings = new Dictionary<string, KeyCode>
            {
                { "pause", KeyCode.Escape },
                { "place_tower", KeyCode.Mouse0 },
                { "cancel", KeyCode.Mouse1 },
                { "upgrade", KeyCode.U },
                { "sell", KeyCode.S },
                { "start_wave", KeyCode.Space },
                { "fast_forward", KeyCode.F },
                { "camera_up", KeyCode.W },
                { "camera_down", KeyCode.S },
                { "camera_left", KeyCode.A },
                { "camera_right", KeyCode.D },
                { "zoom_in", KeyCode.E },
                { "zoom_out", KeyCode.Q }
            };

            // Load custom mappings if they exist
            LoadControlMappings();
            LogDebug($"Initialized {controlMappings.Count} control mappings");
        }

        private void InitializeDefaultTouchZones()
        {
            // Touch zones as normalized screen positions (0-1)
            touchZones = new Dictionary<string, Vector2>
            {
                { "tower_menu", new Vector2(0.85f, 0.5f) },    // Right center
                { "wave_start", new Vector2(0.5f, 0.1f) },     // Bottom center
                { "pause", new Vector2(0.05f, 0.95f) },        // Top left
                { "fast_forward", new Vector2(0.95f, 0.95f) }, // Top right
                { "upgrade", new Vector2(0.85f, 0.3f) },       // Right lower
                { "sell", new Vector2(0.85f, 0.2f) }           // Right bottom
            };

            // Load custom touch zones if they exist
            LoadTouchZones();
            LogDebug($"Initialized {touchZones.Count} touch zones");
        }

        private void ApplyAccessibilitySettings()
        {
            // Apply colorblind mode
            if (currentSettings.colorblindMode != ColorblindMode.None)
            {
                ApplyColorblindMode(currentSettings.colorblindMode);
            }

            // Apply text scale
            ApplyTextScale(currentSettings.textScale);

            // Apply screen reader
            if (currentSettings.screenReaderEnabled)
            {
                EnableScreenReader(true);
            }

            // Apply high contrast
            if (currentSettings.highContrastEnabled)
            {
                EnableHighContrast(true);
            }

            // Apply one-handed mode
            if (currentSettings.oneHandedModeEnabled)
            {
                EnableOneHandedMode(true);
            }

            LogDebug("Applied accessibility settings");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Colorblind Support ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sets the colorblind mode.
        /// </summary>
        public void SetColorblindMode(ColorblindMode mode)
        {
            if (!isInitialized)
                return;

            currentSettings.colorblindMode = mode;
            ApplyColorblindMode(mode);
            SaveAccessibilitySettings();

            OnColorblindModeChanged?.Invoke(mode);
            LogDebug($"Colorblind mode set to: {mode}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("colorblind_mode_changed", new Dictionary<string, object>
                {
                    { "mode", mode.ToString() },
                    { "intensity", currentSettings.colorblindIntensity }
                });
            }

            AnnounceToScreenReader($"Colorblind mode changed to {mode}");
        }

        private void ApplyColorblindMode(ColorblindMode mode)
        {
            // In a real implementation, this would apply shader effects
            // For now, we'll set a global shader property that materials can use
            switch (mode)
            {
                case ColorblindMode.Protanopia:
                    Shader.SetGlobalFloat("_ColorblindMode", 1f);
                    Shader.SetGlobalColor("_ColorblindFilter", new Color(0.567f, 0.433f, 0f, currentSettings.colorblindIntensity));
                    break;
                case ColorblindMode.Deuteranopia:
                    Shader.SetGlobalFloat("_ColorblindMode", 2f);
                    Shader.SetGlobalColor("_ColorblindFilter", new Color(0.625f, 0.375f, 0f, currentSettings.colorblindIntensity));
                    break;
                case ColorblindMode.Tritanopia:
                    Shader.SetGlobalFloat("_ColorblindMode", 3f);
                    Shader.SetGlobalColor("_ColorblindFilter", new Color(0f, 0.45f, 0.55f, currentSettings.colorblindIntensity));
                    break;
                default:
                    Shader.SetGlobalFloat("_ColorblindMode", 0f);
                    break;
            }

            LogDebug($"Applied colorblind mode: {mode}");
        }

        /// <summary>
        /// Gets current colorblind mode.
        /// </summary>
        public ColorblindMode GetColorblindMode()
        {
            return currentSettings?.colorblindMode ?? ColorblindMode.None;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Text Scaling ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sets the text scale for all UI text.
        /// </summary>
        public void SetTextScale(float scale)
        {
            if (!isInitialized)
                return;

            scale = Mathf.Clamp(scale, minTextScale, maxTextScale);
            currentSettings.textScale = scale;
            ApplyTextScale(scale);
            SaveAccessibilitySettings();

            OnTextScaleChanged?.Invoke(scale);
            LogDebug($"Text scale set to: {scale:F2}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("text_scale_changed", new Dictionary<string, object>
                {
                    { "scale", scale }
                });
            }

            AnnounceToScreenReader($"Text size changed to {Mathf.RoundToInt(scale * 100)} percent");
        }

        private void ApplyTextScale(float scale)
        {
            // Set global text scale that UI scripts can reference
            Shader.SetGlobalFloat("_TextScale", scale);

            // In a real implementation, this would update all TMPro components
            // We'd iterate through all text components and apply the scale
            LogDebug($"Applied text scale: {scale:F2}");
        }

        /// <summary>
        /// Gets current text scale.
        /// </summary>
        public float GetTextScale()
        {
            return currentSettings?.textScale ?? 1.0f;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Screen Reader Support ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enables or disables screen reader functionality.
        /// </summary>
        public void EnableScreenReader(bool enabled)
        {
            if (!isInitialized)
                return;

            currentSettings.screenReaderEnabled = enabled;
            SaveAccessibilitySettings();

            OnScreenReaderToggled?.Invoke(enabled);
            LogDebug($"Screen reader {(enabled ? "enabled" : "disabled")}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("screen_reader_toggled", new Dictionary<string, object>
                {
                    { "enabled", enabled }
                });
            }

            if (enabled)
            {
                AnnounceToScreenReader("Screen reader enabled");
            }
        }

        /// <summary>
        /// Announces text to screen reader.
        /// </summary>
        public void AnnounceToScreenReader(string text)
        {
            if (!currentSettings.screenReaderEnabled || string.IsNullOrEmpty(text))
                return;

            screenReaderQueue.Add(text);

            if (!isReadingScreen)
            {
                StartCoroutine(ProcessScreenReaderQueue());
            }

            OnScreenReaderAnnouncement?.Invoke(text);
            LogDebug($"Screen reader: {text}");
        }

        private IEnumerator ProcessScreenReaderQueue()
        {
            isReadingScreen = true;

            while (screenReaderQueue.Count > 0)
            {
                string announcement = screenReaderQueue[0];
                screenReaderQueue.RemoveAt(0);

                // In a real implementation, this would interface with platform-specific TTS
                // For now, we'll just log and wait based on text length
                float duration = announcement.Length * 0.05f / currentSettings.screenReaderSpeed;
                yield return new WaitForSeconds(duration);
            }

            isReadingScreen = false;
        }

        /// <summary>
        /// Gets screen reader enabled state.
        /// </summary>
        public bool IsScreenReaderEnabled()
        {
            return currentSettings?.screenReaderEnabled ?? false;
        }

        /// <summary>
        /// Sets screen reader speed.
        /// </summary>
        public void SetScreenReaderSpeed(float speed)
        {
            currentSettings.screenReaderSpeed = Mathf.Clamp(speed, 0.5f, 2.0f);
            SaveAccessibilitySettings();
            LogDebug($"Screen reader speed set to: {speed:F2}");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Control Remapping ─────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Remaps a control action to a new key.
        /// </summary>
        public void RemapControl(string action, KeyCode newKey)
        {
            if (!enableRemappableControls || !controlMappings.ContainsKey(action))
            {
                LogDebug($"Cannot remap action: {action}");
                return;
            }

            KeyCode oldKey = controlMappings[action];
            controlMappings[action] = newKey;
            SaveControlMappings();

            OnControlRemapped?.Invoke(action, newKey);
            LogDebug($"Remapped {action}: {oldKey} -> {newKey}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("control_remapped", new Dictionary<string, object>
                {
                    { "action", action },
                    { "old_key", oldKey.ToString() },
                    { "new_key", newKey.ToString() }
                });
            }

            AnnounceToScreenReader($"Remapped {action} to {newKey}");
        }

        /// <summary>
        /// Gets the current key mapping for an action.
        /// </summary>
        public KeyCode GetKeyForAction(string action)
        {
            return controlMappings.ContainsKey(action) ? controlMappings[action] : KeyCode.None;
        }

        /// <summary>
        /// Resets all control mappings to defaults.
        /// </summary>
        public void ResetControlMappings()
        {
            InitializeDefaultControlMappings();
            SaveControlMappings();
            LogDebug("Reset control mappings to defaults");

            AnnounceToScreenReader("Controls reset to defaults");
        }

        /// <summary>
        /// Gets all control mappings.
        /// </summary>
        public Dictionary<string, KeyCode> GetAllControlMappings()
        {
            return new Dictionary<string, KeyCode>(controlMappings);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Touch Zone Customization ──────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sets the position of a touch zone.
        /// </summary>
        public void SetTouchZonePosition(string zoneName, Vector2 normalizedPosition)
        {
            if (!touchZones.ContainsKey(zoneName))
            {
                LogDebug($"Touch zone not found: {zoneName}");
                return;
            }

            // Clamp to screen bounds
            normalizedPosition.x = Mathf.Clamp01(normalizedPosition.x);
            normalizedPosition.y = Mathf.Clamp01(normalizedPosition.y);

            touchZones[zoneName] = normalizedPosition;
            SaveTouchZones();

            LogDebug($"Touch zone {zoneName} moved to: {normalizedPosition}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("touch_zone_moved", new Dictionary<string, object>
                {
                    { "zone_name", zoneName },
                    { "x", normalizedPosition.x },
                    { "y", normalizedPosition.y }
                });
            }
        }

        /// <summary>
        /// Gets the position of a touch zone.
        /// </summary>
        public Vector2 GetTouchZonePosition(string zoneName)
        {
            return touchZones.ContainsKey(zoneName) ? touchZones[zoneName] : Vector2.zero;
        }

        /// <summary>
        /// Resets all touch zones to defaults.
        /// </summary>
        public void ResetTouchZones()
        {
            InitializeDefaultTouchZones();
            SaveTouchZones();
            LogDebug("Reset touch zones to defaults");

            AnnounceToScreenReader("Touch zones reset to defaults");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── High Contrast Mode ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enables or disables high contrast mode.
        /// </summary>
        public void EnableHighContrast(bool enabled)
        {
            if (!isInitialized)
                return;

            currentSettings.highContrastEnabled = enabled;
            SaveAccessibilitySettings();

            // Apply high contrast shader settings
            Shader.SetGlobalFloat("_HighContrast", enabled ? 1f : 0f);

            OnHighContrastToggled?.Invoke(enabled);
            LogDebug($"High contrast {(enabled ? "enabled" : "disabled")}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("high_contrast_toggled", new Dictionary<string, object>
                {
                    { "enabled", enabled }
                });
            }

            AnnounceToScreenReader($"High contrast mode {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Gets high contrast enabled state.
        /// </summary>
        public bool IsHighContrastEnabled()
        {
            return currentSettings?.highContrastEnabled ?? false;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── One-Handed Mode ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enables or disables one-handed mode for mobile devices.
        /// </summary>
        public void EnableOneHandedMode(bool enabled)
        {
            if (!isInitialized)
                return;

            currentSettings.oneHandedModeEnabled = enabled;
            SaveAccessibilitySettings();

            // In one-handed mode, reposition touch zones to the lower half of the screen
            if (enabled)
            {
                AdjustTouchZonesForOneHandedMode();
            }
            else
            {
                ResetTouchZones();
            }

            OnOneHandedModeToggled?.Invoke(enabled);
            LogDebug($"One-handed mode {(enabled ? "enabled" : "disabled")}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("one_handed_mode_toggled", new Dictionary<string, object>
                {
                    { "enabled", enabled }
                });
            }

            AnnounceToScreenReader($"One-handed mode {(enabled ? "enabled" : "disabled")}");
        }

        private void AdjustTouchZonesForOneHandedMode()
        {
            // Move all UI elements to the bottom half and right side of the screen
            touchZones["tower_menu"] = new Vector2(0.85f, 0.3f);
            touchZones["wave_start"] = new Vector2(0.7f, 0.1f);
            touchZones["pause"] = new Vector2(0.1f, 0.5f);
            touchZones["fast_forward"] = new Vector2(0.9f, 0.5f);
            touchZones["upgrade"] = new Vector2(0.85f, 0.2f);
            touchZones["sell"] = new Vector2(0.7f, 0.05f);

            SaveTouchZones();
            LogDebug("Adjusted touch zones for one-handed mode");
        }

        /// <summary>
        /// Gets one-handed mode enabled state.
        /// </summary>
        public bool IsOneHandedModeEnabled()
        {
            return currentSettings?.oneHandedModeEnabled ?? false;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Additional Accessibility Options ──────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enables or disables reduced motion.
        /// </summary>
        public void SetReduceMotion(bool enabled)
        {
            currentSettings.reduceMotionEnabled = enabled;
            SaveAccessibilitySettings();

            Shader.SetGlobalFloat("_ReduceMotion", enabled ? 1f : 0f);
            LogDebug($"Reduce motion {(enabled ? "enabled" : "disabled")}");

            AnnounceToScreenReader($"Reduced motion {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Enables or disables simplified UI.
        /// </summary>
        public void SetSimplifyUI(bool enabled)
        {
            currentSettings.simplifyUIEnabled = enabled;
            SaveAccessibilitySettings();

            LogDebug($"Simplify UI {(enabled ? "enabled" : "disabled")}");
            AnnounceToScreenReader($"Simplified user interface {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Gets current accessibility settings.
        /// </summary>
        public AccessibilitySettings GetCurrentSettings()
        {
            return currentSettings;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadAccessibilitySettings()
        {
            string json = PlayerPrefs.GetString("AccessibilitySettings", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    currentSettings = JsonUtility.FromJson<AccessibilitySettings>(json);
                    LogDebug("Loaded accessibility settings");
                }
                catch
                {
                    LogDebug("Failed to load accessibility settings");
                }
            }
        }

        private void SaveAccessibilitySettings()
        {
            if (currentSettings == null)
                return;

            string json = JsonUtility.ToJson(currentSettings);
            PlayerPrefs.SetString("AccessibilitySettings", json);
            PlayerPrefs.Save();
        }

        private void LoadControlMappings()
        {
            string json = PlayerPrefs.GetString("ControlMappings", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    ControlMappingsData data = JsonUtility.FromJson<ControlMappingsData>(json);
                    for (int i = 0; i < data.actions.Count; i++)
                    {
                        controlMappings[data.actions[i]] = (KeyCode)data.keyCodes[i];
                    }
                    LogDebug("Loaded control mappings");
                }
                catch
                {
                    LogDebug("Failed to load control mappings");
                }
            }
        }

        private void SaveControlMappings()
        {
            ControlMappingsData data = new ControlMappingsData
            {
                actions = new List<string>(controlMappings.Keys),
                keyCodes = new List<int>()
            };

            foreach (var kvp in controlMappings)
            {
                data.keyCodes.Add((int)kvp.Value);
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("ControlMappings", json);
            PlayerPrefs.Save();
        }

        private void LoadTouchZones()
        {
            string json = PlayerPrefs.GetString("TouchZones", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    TouchZonesData data = JsonUtility.FromJson<TouchZonesData>(json);
                    for (int i = 0; i < data.zoneNames.Count; i++)
                    {
                        touchZones[data.zoneNames[i]] = data.positions[i];
                    }
                    LogDebug("Loaded touch zones");
                }
                catch
                {
                    LogDebug("Failed to load touch zones");
                }
            }
        }

        private void SaveTouchZones()
        {
            TouchZonesData data = new TouchZonesData
            {
                zoneNames = new List<string>(touchZones.Keys),
                positions = new List<Vector2>(touchZones.Values)
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("TouchZones", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[AccessibilityManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Colorblind mode options.
    /// </summary>
    public enum ColorblindMode
    {
        None,           // No color adjustment
        Protanopia,     // Red-green colorblindness (red-weak)
        Deuteranopia,   // Red-green colorblindness (green-weak)
        Tritanopia      // Blue-yellow colorblindness
    }

    /// <summary>
    /// Complete accessibility settings.
    /// </summary>
    [Serializable]
    public class AccessibilitySettings
    {
        public ColorblindMode colorblindMode;
        public float colorblindIntensity;
        public float textScale;
        public bool screenReaderEnabled;
        public float screenReaderSpeed;
        public bool announceUIChanges;
        public bool highContrastEnabled;
        public bool reduceMotionEnabled;
        public bool simplifyUIEnabled;
        public bool oneHandedModeEnabled;
    }

    /// <summary>
    /// Serialization helper for control mappings.
    /// </summary>
    [Serializable]
    public class ControlMappingsData
    {
        public List<string> actions;
        public List<int> keyCodes; // KeyCode as int for serialization
    }

    /// <summary>
    /// Serialization helper for touch zones.
    /// </summary>
    [Serializable]
    public class TouchZonesData
    {
        public List<string> zoneNames;
        public List<Vector2> positions;
    }
}
