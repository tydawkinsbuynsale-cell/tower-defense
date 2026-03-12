using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RobotTowerDefense.UI
{
    /// <summary>
    /// iOS Safe Area handler for notch support (iPhone X and newer).
    /// Automatically adjusts UI elements to respect the safe area on iOS devices.
    /// Handles notch, home indicator, and status bar areas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class iOSSafeAreaHandler : MonoBehaviour
    {
        #region Configuration

        [Header("Safe Area Settings")]
        [SerializeField] private bool applyToThisPanel = true;
        [SerializeField] private bool respectTop = true;
        [SerializeField] private bool respectBottom = true;
        [SerializeField] private bool respectLeft = true;
        [SerializeField] private bool respectRight = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private Color debugColor = Color.green;

        #endregion

        #region Private Fields

        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.Unknown;

        // Canvas tracking
        private Canvas canvas;
        private CanvasScaler canvasScaler;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasScaler = GetComponentInParent<CanvasScaler>();

            if (canvas == null)
            {
                Debug.LogWarning($"iOSSafeAreaHandler on {gameObject.name}: No Canvas found in parents! Safe area may not work correctly.");
            }
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            // Check if safe area changed (orientation change, device rotation, etc.)
            if (HasSafeAreaChanged())
            {
                ApplySafeArea();
            }
        }

        #endregion

        #region Safe Area Application

        /// <summary>
        /// Apply the safe area to this RectTransform.
        /// </summary>
        public void ApplySafeArea()
        {
            if (rectTransform == null || !applyToThisPanel)
                return;

            Rect safeArea = Screen.safeArea;

            // Convert safe area to anchors
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Normalize to 0-1 range
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Apply respects (allow selective ignoring of certain edges)
            Vector2 currentAnchorMin = rectTransform.anchorMin;
            Vector2 currentAnchorMax = rectTransform.anchorMax;

            if (respectLeft)
                currentAnchorMin.x = anchorMin.x;

            if (respectBottom)
                currentAnchorMin.y = anchorMin.y;

            if (respectRight)
                currentAnchorMax.x = anchorMax.x;

            if (respectTop)
                currentAnchorMax.y = anchorMax.y;

            rectTransform.anchorMin = currentAnchorMin;
            rectTransform.anchorMax = currentAnchorMax;

            // Update tracking
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;

            if (showDebugInfo)
            {
                Debug.Log($"✅ Safe Area Applied to {gameObject.name}:\n" +
                    $"Screen: {Screen.width}x{Screen.height}\n" +
                    $"Safe Area: {safeArea}\n" +
                    $"Anchors: Min({currentAnchorMin}), Max({currentAnchorMax})");
            }
        }

        /// <summary>
        /// Check if safe area or screen orientation changed.
        /// </summary>
        private bool HasSafeAreaChanged()
        {
            return Screen.safeArea != lastSafeArea ||
                   Screen.width != lastScreenSize.x ||
                   Screen.height != lastScreenSize.y ||
                   Screen.orientation != lastOrientation;
        }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Set which edges respect the safe area.
        /// </summary>
        public void SetRespectEdges(bool top, bool bottom, bool left, bool right)
        {
            respectTop = top;
            respectBottom = bottom;
            respectLeft = left;
            respectRight = right;
            ApplySafeArea();
        }

        /// <summary>
        /// Enable or disable safe area application.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            applyToThisPanel = enabled;
            if (enabled)
                ApplySafeArea();
            else
                ResetAnchors();
        }

        /// <summary>
        /// Reset anchors to full screen.
        /// </summary>
        private void ResetAnchors()
        {
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
            }
        }

        #endregion

        #region Debug Visualization

        private void OnGUI()
        {
            if (!showDebugInfo)
                return;

            // Draw safe area rectangle
            Rect safeArea = Screen.safeArea;

            // Convert safe area to GUI coordinates (GUI is top-left origin, Screen is bottom-left)
            Rect guiSafeArea = new Rect(
                safeArea.x,
                Screen.height - safeArea.y - safeArea.height,
                safeArea.width,
                safeArea.height
            );

            // Draw border around safe area
            DrawRectBorder(guiSafeArea, debugColor, 3f);

            // Draw info text
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;

            string info = $"Safe Area Debug\n" +
                $"Screen: {Screen.width}x{Screen.height}\n" +
                $"Safe Area: {safeArea.x}, {safeArea.y}, {safeArea.width}, {safeArea.height}\n" +
                $"Orientation: {Screen.orientation}\n" +
                $"Respect: T:{respectTop} B:{respectBottom} L:{respectLeft} R:{respectRight}";

            GUI.Label(new Rect(10, 10, 500, 200), info, style);
        }

        private void DrawRectBorder(Rect rect, Color color, float thickness)
        {
            Color oldColor = GUI.color;
            GUI.color = color;

            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), Texture2D.whiteTexture);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);

            GUI.color = oldColor;
        }

        #endregion

        #region Static Utility Methods

        /// <summary>
        /// Get the current safe area as a percentage of screen size.
        /// </summary>
        public static Rect GetSafeAreaNormalized()
        {
            Rect safeArea = Screen.safeArea;
            return new Rect(
                safeArea.x / Screen.width,
                safeArea.y / Screen.height,
                safeArea.width / Screen.width,
                safeArea.height / Screen.height
            );
        }

        /// <summary>
        /// Get the safe area insets (distance from screen edges).
        /// </summary>
        public static RectOffset GetSafeAreaInsets()
        {
            Rect safeArea = Screen.safeArea;
            return new RectOffset(
                (int)safeArea.x,                          // Left
                (int)(Screen.width - safeArea.xMax),      // Right
                (int)(Screen.height - safeArea.yMax),     // Top (inverted Y)
                (int)safeArea.y                           // Bottom
            );
        }

        /// <summary>
        /// Check if the device has a notch or safe area insets.
        /// </summary>
        public static bool HasNotch()
        {
            Rect safeArea = Screen.safeArea;
            return safeArea.x > 0 || safeArea.y > 0 ||
                   safeArea.width < Screen.width ||
                   safeArea.height < Screen.height;
        }

        /// <summary>
        /// Get device model information (iOS only).
        /// </summary>
        public static string GetDeviceModel()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.iOS.Device.generation.ToString();
#else
            return "Not iOS Device";
#endif
        }

        /// <summary>
        /// Apply safe area to all UI canvases in the scene.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ApplySafeAreaToAllCanvases()
        {
            // Find all canvases
            Canvas[] canvases = FindObjectsOfType<Canvas>();

            foreach (Canvas canvas in canvases)
            {
                // Check if canvas root already has safe area handler
                iOSSafeAreaHandler safeAreaHandler = canvas.GetComponentInChildren<iOSSafeAreaHandler>();

                if (safeAreaHandler == null)
                {
                    // Add to root RectTransform
                    RectTransform rootRect = canvas.GetComponent<RectTransform>();
                    if (rootRect != null)
                    {
                        safeAreaHandler = rootRect.gameObject.AddComponent<iOSSafeAreaHandler>();
                        Debug.Log($"✅ Added iOSSafeAreaHandler to canvas: {canvas.name}");
                    }
                }
            }
        }

        #endregion

        #region Context Menu

#if UNITY_EDITOR
        [ContextMenu("Apply Safe Area Now")]
        private void ApplySafeAreaContext()
        {
            ApplySafeArea();
            Debug.Log($"✅ Safe area applied to {gameObject.name}");
        }

        [ContextMenu("Reset Anchors")]
        private void ResetAnchorsContext()
        {
            ResetAnchors();
            Debug.Log($"✅ Anchors reset on {gameObject.name}");
        }

        [ContextMenu("Toggle Debug Info")]
        private void ToggleDebugInfo()
        {
            showDebugInfo = !showDebugInfo;
            Debug.Log($"Debug info {(showDebugInfo ? "enabled" : "disabled")}");
        }

        [ContextMenu("Print Safe Area Info")]
        private void PrintSafeAreaInfo()
        {
            Rect safeArea = Screen.safeArea;
            RectOffset insets = GetSafeAreaInsets();

            Debug.Log($"📱 Safe Area Information:\n" +
                $"Screen Size: {Screen.width}x{Screen.height}\n" +
                $"Safe Area: {safeArea}\n" +
                $"Insets - Left: {insets.left}, Right: {insets.right}, Top: {insets.top}, Bottom: {insets.bottom}\n" +
                $"Has Notch: {HasNotch()}\n" +
                $"Orientation: {Screen.orientation}\n" +
                $"Device Model: {GetDeviceModel()}");
        }

        [ContextMenu("Simulate iPhone X Notch")]
        private void SimulateNotch()
        {
            // Simulate iPhone X safe area in editor
            rectTransform.anchorMin = new Vector2(0f, 0.05f);  // Bottom inset (home indicator)
            rectTransform.anchorMax = new Vector2(1f, 0.95f);  // Top inset (notch)
            Debug.Log("📱 Simulated iPhone X notch (5% top/bottom insets)");
        }
#endif

        #endregion
    }

    #region Auto-Apply Component

    /// <summary>
    /// Automatically adds iOSSafeAreaHandler to Canvas objects.
    /// Attach this to GameObjects you want to auto-configure for safe area.
    /// </summary>
    public class AutoApplySafeArea : MonoBehaviour
    {
        [SerializeField] private bool applyOnStart = true;

        private void Start()
        {
            if (applyOnStart)
            {
                ApplySafeAreaToCanvas();
            }
        }

        private void ApplySafeAreaToCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning($"AutoApplySafeArea on {gameObject.name}: No Canvas component found!");
                return;
            }

            // Check if already has handler
            iOSSafeAreaHandler existingHandler = GetComponent<iOSSafeAreaHandler>();
            if (existingHandler == null)
            {
                gameObject.AddComponent<iOSSafeAreaHandler>();
                Debug.Log($"✅ Auto-applied safe area handler to {gameObject.name}");
            }
        }
    }

    #endregion
}
