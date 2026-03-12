using UnityEngine;
using System.Collections.Generic;

namespace RobotTowerDefense.Input
{
    /// <summary>
    /// iOS-specific input handler with multi-touch gesture support.
    /// Handles iOS-specific touch behaviors, gestures, and haptic feedback.
    /// Complements the existing InputManager with iOS optimizations.
    /// </summary>
    public class iOSInputHandler : MonoBehaviour
    {
        #region Singleton

        private static iOSInputHandler instance;
        public static iOSInputHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("iOSInputHandler");
                    instance = go.AddComponent<iOSInputHandler>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Gesture Detection")]
        [SerializeField] private float swipeThreshold = 50f; // Minimum distance for swipe
        [SerializeField] private float tapMaxDuration = 0.3f; // Maximum duration for tap
        [SerializeField] private float doubleTapMaxInterval = 0.3f; // Max time between taps
        [SerializeField] private float longPressMinDuration = 0.5f; // Minimum hold time for long press

        [Header("Pinch Zoom")]
        [SerializeField] private float pinchZoomSpeed = 0.01f;
        [SerializeField] private float minPinchDistance = 10f; // Minimum distance between fingers

        [Header("Haptics")]
        [SerializeField] private bool enableHaptics = true;

        #endregion

        #region Events

        public delegate void SwipeHandler(Vector2 direction);
        public delegate void PinchHandler(float delta);
        public delegate void TapHandler(Vector2 position);
        public delegate void DoubleTapHandler(Vector2 position);
        public delegate void LongPressHandler(Vector2 position);

        public static event SwipeHandler OnSwipe;
        public static event PinchHandler OnPinch;
        public static event TapHandler OnTap;
        public static event DoubleTapHandler OnDoubleTap;
        public static event LongPressHandler OnLongPress;

        #endregion

        #region Touch Tracking

        private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
        private float lastTapTime = 0f;
        private Vector2 lastTapPosition;
        private bool waitingForDoubleTap = false;

        private class TouchData
        {
            public Vector2 startPosition;
            public float startTime;
            public bool longPressTriggered;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

#if !UNITY_IOS
            Debug.LogWarning("iOSInputHandler is designed for iOS platform. It will work but may not be optimal.");
#endif
        }

        private void Start()
        {
            // Configure iOS-specific input settings
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = false; // Don't hide home indicator
            Input.multiTouchEnabled = true; // Enable multi-touch
#endif

            Debug.Log("✅ iOSInputHandler initialized");
        }

        private void Update()
        {
#if UNITY_IOS || UNITY_EDITOR
            ProcessTouches();
#endif
        }

        #endregion

        #region Touch Processing

        private void ProcessTouches()
        {
            // Process all active touches
            if (UnityEngine.Input.touchCount == 0)
            {
                // Clear finished touches
                activeTouches.Clear();
                return;
            }

            // Single touch gestures
            if (UnityEngine.Input.touchCount == 1)
            {
                ProcessSingleTouch(UnityEngine.Input.GetTouch(0));
            }
            // Multi-touch gestures (pinch zoom)
            else if (UnityEngine.Input.touchCount == 2)
            {
                ProcessPinchGesture(UnityEngine.Input.GetTouch(0), UnityEngine.Input.GetTouch(1));
            }
        }

        private void ProcessSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;

                case TouchPhase.Moved:
                    HandleTouchMoved(touch);
                    break;

                case TouchPhase.Ended:
                    HandleTouchEnded(touch);
                    break;

                case TouchPhase.Canceled:
                    HandleTouchCanceled(touch);
                    break;
            }
        }

        private void HandleTouchBegan(Touch touch)
        {
            activeTouches[touch.fingerId] = new TouchData
            {
                startPosition = touch.position,
                startTime = Time.time,
                longPressTriggered = false
            };
        }

        private void HandleTouchMoved(Touch touch)
        {
            if (!activeTouches.ContainsKey(touch.fingerId))
                return;

            TouchData touchData = activeTouches[touch.fingerId];

            // Check for long press (if not moving too much)
            float distance = Vector2.Distance(touch.position, touchData.startPosition);
            if (distance > swipeThreshold * 0.2f)
            {
                touchData.longPressTriggered = true; // Moving, not a long press
            }

            // Detect swipe during movement
            if (distance > swipeThreshold)
            {
                Vector2 swipeDirection = (touch.position - touchData.startPosition).normalized;
                OnSwipe?.Invoke(swipeDirection);
                TriggerHapticFeedback(HapticFeedbackType.Light);
                touchData.longPressTriggered = true; // Swiped, not a long press
            }
        }

        private void HandleTouchEnded(Touch touch)
        {
            if (!activeTouches.ContainsKey(touch.fingerId))
                return;

            TouchData touchData = activeTouches[touch.fingerId];
            float touchDuration = Time.time - touchData.startTime;
            float distance = Vector2.Distance(touch.position, touchData.startPosition);

            // Long press detection
            if (!touchData.longPressTriggered && touchDuration >= longPressMinDuration && distance < swipeThreshold * 0.2f)
            {
                OnLongPress?.Invoke(touch.position);
                TriggerHapticFeedback(HapticFeedbackType.Medium);
            }
            // Tap detection (short duration, minimal movement)
            else if (touchDuration < tapMaxDuration && distance < swipeThreshold * 0.2f)
            {
                // Check for double tap
                if (waitingForDoubleTap && Time.time - lastTapTime < doubleTapMaxInterval)
                {
                    float doubleTapDistance = Vector2.Distance(touch.position, lastTapPosition);
                    if (doubleTapDistance < swipeThreshold * 0.5f)
                    {
                        OnDoubleTap?.Invoke(touch.position);
                        TriggerHapticFeedback(HapticFeedbackType.Medium);
                        waitingForDoubleTap = false;
                    }
                    else
                    {
                        // Too far apart, treat as separate taps
                        OnTap?.Invoke(touch.position);
                        TriggerHapticFeedback(HapticFeedbackType.Light);
                        lastTapTime = Time.time;
                        lastTapPosition = touch.position;
                    }
                }
                else
                {
                    // First tap, wait for potential double tap
                    OnTap?.Invoke(touch.position);
                    TriggerHapticFeedback(HapticFeedbackType.Light);
                    waitingForDoubleTap = true;
                    lastTapTime = Time.time;
                    lastTapPosition = touch.position;
                }
            }
            // Swipe detection (moved far enough)
            else if (distance > swipeThreshold)
            {
                Vector2 swipeDirection = (touch.position - touchData.startPosition).normalized;
                OnSwipe?.Invoke(swipeDirection);
                TriggerHapticFeedback(HapticFeedbackType.Light);
            }

            activeTouches.Remove(touch.fingerId);
        }

        private void HandleTouchCanceled(Touch touch)
        {
            activeTouches.Remove(touch.fingerId);
        }

        #endregion

        #region Pinch Gesture

        private float previousPinchDistance = 0f;

        private void ProcessPinchGesture(Touch touch0, Touch touch1)
        {
            // Both touches must be moving
            if (touch0.phase != TouchPhase.Moved && touch1.phase != TouchPhase.Moved)
            {
                previousPinchDistance = 0f;
                return;
            }

            // Calculate current distance
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            // Initialize previous distance on first frame
            if (previousPinchDistance == 0f || touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                previousPinchDistance = currentDistance;
                return;
            }

            // Ignore tiny movements
            if (currentDistance < minPinchDistance)
                return;

            // Calculate pinch delta
            float pinchDelta = (currentDistance - previousPinchDistance) * pinchZoomSpeed;
            previousPinchDistance = currentDistance;

            if (Mathf.Abs(pinchDelta) > 0.001f)
            {
                OnPinch?.Invoke(pinchDelta);
            }
        }

        #endregion

        #region Haptic Feedback

        public enum HapticFeedbackType
        {
            Light,
            Medium,
            Heavy,
            Success,
            Warning,
            Error
        }

        public void TriggerHapticFeedback(HapticFeedbackType type)
        {
            if (!enableHaptics)
                return;

#if UNITY_IOS && !UNITY_EDITOR
            switch (type)
            {
                case HapticFeedbackType.Light:
                    Handheld.Vibrate(); // Light vibration
                    break;

                case HapticFeedbackType.Medium:
                    Handheld.Vibrate(); // Medium vibration
                    break;

                case HapticFeedbackType.Heavy:
                    Handheld.Vibrate(); // Heavy vibration
                    break;

                case HapticFeedbackType.Success:
                    Handheld.Vibrate(); // Success pattern
                    break;

                case HapticFeedbackType.Warning:
                    Handheld.Vibrate(); // Warning pattern
                    break;

                case HapticFeedbackType.Error:
                    Handheld.Vibrate(); // Error pattern
                    break;
            }
#endif
        }

        /// <summary>
        /// Play haptic feedback for tower placement.
        /// </summary>
        public void PlayPlacementHaptic(bool success)
        {
            if (success)
                TriggerHapticFeedback(HapticFeedbackType.Success);
            else
                TriggerHapticFeedback(HapticFeedbackType.Error);
        }

        /// <summary>
        /// Play haptic feedback for button press.
        /// </summary>
        public void PlayButtonHaptic()
        {
            TriggerHapticFeedback(HapticFeedbackType.Light);
        }

        /// <summary>
        /// Play haptic feedback for wave complete.
        /// </summary>
        public void PlayWaveCompleteHaptic()
        {
            TriggerHapticFeedback(HapticFeedbackType.Success);
        }

        /// <summary>
        /// Play haptic feedback for game over.
        /// </summary>
        public void PlayGameOverHaptic()
        {
            TriggerHapticFeedback(HapticFeedbackType.Heavy);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the dominant swipe direction from a vector.
        /// </summary>
        public static SwipeDirection GetSwipeDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (angle < -45f && angle >= -135f)
                return SwipeDirection.Down;
            else if (angle >= -45f && angle < 45f)
                return SwipeDirection.Right;
            else if (angle >= 45f && angle < 135f)
                return SwipeDirection.Up;
            else
                return SwipeDirection.Left;
        }

        /// <summary>
        /// Check if a position is within safe touch area (not near edges/notch).
        /// </summary>
        public bool IsSafeTouchPosition(Vector2 screenPosition)
        {
            // Use safe area bounds (handled by iOSSafeAreaHandler)
            Rect safeArea = Screen.safeArea;
            return safeArea.Contains(screenPosition);
        }

        /// <summary>
        /// Convert screen position to world position for tower placement.
        /// </summary>
        public Vector3 ScreenToWorldPosition(Vector2 screenPosition, Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;

            return camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enable or disable haptic feedback.
        /// </summary>
        public void SetHapticsEnabled(bool enabled)
        {
            enableHaptics = enabled;
            PlayerPrefs.SetInt("iOS_HapticsEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Get current haptics enabled state.
        /// </summary>
        public bool GetHapticsEnabled()
        {
            return enableHaptics;
        }

        /// <summary>
        /// Get number of active touches.
        /// </summary>
        public int GetTouchCount()
        {
#if UNITY_IOS || UNITY_EDITOR
            return UnityEngine.Input.touchCount;
#else
            return 0;
#endif
        }

        #endregion

        #region Context Menu Testing

#if UNITY_EDITOR
        [ContextMenu("Test Light Haptic")]
        private void TestLightHaptic()
        {
            TriggerHapticFeedback(HapticFeedbackType.Light);
            Debug.Log("Triggered Light Haptic");
        }

        [ContextMenu("Test Success Haptic")]
        private void TestSuccessHaptic()
        {
            TriggerHapticFeedback(HapticFeedbackType.Success);
            Debug.Log("Triggered Success Haptic");
        }

        [ContextMenu("Test Error Haptic")]
        private void TestErrorHaptic()
        {
            TriggerHapticFeedback(HapticFeedbackType.Error);
            Debug.Log("Triggered Error Haptic");
        }

        [ContextMenu("Simulate Swipe Right")]
        private void SimulateSwipeRight()
        {
            OnSwipe?.Invoke(Vector2.right);
            Debug.Log("Simulated Swipe Right");
        }

        [ContextMenu("Simulate Double Tap")]
        private void SimulateDoubleTap()
        {
            OnDoubleTap?.Invoke(new Vector2(Screen.width / 2, Screen.height / 2));
            Debug.Log("Simulated Double Tap");
        }
#endif

        #endregion
    }

    #region Enums

    public enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    #endregion
}
