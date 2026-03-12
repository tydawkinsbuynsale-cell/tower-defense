using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace RobotTD.Map
{
    /// <summary>
    /// Unified input handler for touch and mouse input.
    /// Distinguishes between taps, drags, and UI interactions.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Tap Detection")]
        [SerializeField] private float tapMaxDuration = 0.3f;
        [SerializeField] private float tapMaxMovement = 10f; // Pixels

        [Header("Long Press")]
        [SerializeField] private float longPressDuration = 0.5f;

        // Events
        public event Action<Vector3> OnTap; // World position
        public event Action<Vector3> OnLongPress; // World position
        public event Action<Vector3> OnDragStart; // World position
        public event Action<Vector3> OnDrag; // World position
        public event Action<Vector3> OnDragEnd; // World position
        public event Action<Vector2> OnPinchZoom; // Center screen position, delta

        // State
        private bool isPointerDown;
        private float pointerDownTime;
        private Vector2 pointerDownScreenPos;
        private Vector3 pointerDownWorldPos;
        private bool isDragging;
        private bool longPressTriggered;

        // Touch pinch
        private float lastPinchDistance;

        private Camera mainCam;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCam = Camera.main;
        }

        private void Update()
        {
            if (Core.GameManager.Instance?.IsPaused == true)
            {
                return;
            }

            // Handle touch or mouse
            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        #region Touch Input

        private void HandleTouchInput()
        {
            // Two-finger pinch zoom
            if (Input.touchCount == 2)
            {
                isPointerDown = false;
                isDragging = false;

                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                float currentDist = Vector2.Distance(t0.position, t1.position);

                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    lastPinchDistance = currentDist;
                }
                else
                {
                    float delta = currentDist - lastPinchDistance;
                    Vector2 center = (t0.position + t1.position) / 2f;
                    OnPinchZoom?.Invoke(center);
                    lastPinchDistance = currentDist;
                }

                return;
            }

            // Single touch
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandlePointerDown(touch.position);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        HandlePointerMove(touch.position);
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        HandlePointerUp(touch.position);
                        break;
                }
            }
        }

        #endregion

        #region Mouse Input

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandlePointerDown(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isPointerDown)
            {
                HandlePointerMove(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandlePointerUp(Input.mousePosition);
            }
        }

        #endregion

        #region Unified Pointer Handling

        private void HandlePointerDown(Vector2 screenPos)
        {
            // Check if over UI
            if (IsOverUI(screenPos))
            {
                return;
            }

            isPointerDown = true;
            pointerDownTime = Time.time;
            pointerDownScreenPos = screenPos;
            pointerDownWorldPos = ScreenToWorld(screenPos);
            isDragging = false;
            longPressTriggered = false;
        }

        private void HandlePointerMove(Vector2 screenPos)
        {
            if (!isPointerDown) return;

            float movement = Vector2.Distance(screenPos, pointerDownScreenPos);
            float duration = Time.time - pointerDownTime;

            // Check for long press
            if (!longPressTriggered && !isDragging && duration >= longPressDuration && movement < tapMaxMovement)
            {
                longPressTriggered = true;
                OnLongPress?.Invoke(pointerDownWorldPos);
                return;
            }

            // Check for drag start
            if (!isDragging && movement > tapMaxMovement)
            {
                isDragging = true;
                OnDragStart?.Invoke(pointerDownWorldPos);
            }

            // Continue drag
            if (isDragging)
            {
                OnDrag?.Invoke(ScreenToWorld(screenPos));
            }
        }

        private void HandlePointerUp(Vector2 screenPos)
        {
            if (!isPointerDown)
            {
                return;
            }

            float movement = Vector2.Distance(screenPos, pointerDownScreenPos);
            float duration = Time.time - pointerDownTime;

            // End drag
            if (isDragging)
            {
                OnDragEnd?.Invoke(ScreenToWorld(screenPos));
            }
            // Check for tap
            else if (!longPressTriggered && duration < tapMaxDuration && movement < tapMaxMovement)
            {
                // Make sure not over UI for tap
                if (!IsOverUI(screenPos))
                {
                    OnTap?.Invoke(pointerDownWorldPos);
                }
            }

            isPointerDown = false;
            isDragging = false;
            longPressTriggered = false;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Check if screen position is over UI element
        /// </summary>
        public bool IsOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        /// <summary>
        /// Check if currently over UI
        /// </summary>
        public bool IsOverUI()
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                return IsOverUI(Input.GetTouch(0).position);
            }
            return IsOverUI(Input.mousePosition);
        }

        /// <summary>
        /// Convert screen position to world position on ground plane
        /// </summary>
        public Vector3 ScreenToWorld(Vector2 screenPos)
        {
            if (mainCam == null) mainCam = Camera.main;

            Ray ray = mainCam.ScreenPointToRay(screenPos);
            Plane ground = new Plane(Vector3.up, Vector3.zero);

            if (ground.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Get current pointer world position
        /// </summary>
        public Vector3 GetPointerWorldPosition()
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                return ScreenToWorld(Input.GetTouch(0).position);
            }
            return ScreenToWorld(Input.mousePosition);
        }

        /// <summary>
        /// Raycast from screen position
        /// </summary>
        public bool Raycast(Vector2 screenPos, out RaycastHit hit, float maxDistance = 100f, LayerMask? layerMask = null)
        {
            if (mainCam == null) mainCam = Camera.main;

            Ray ray = mainCam.ScreenPointToRay(screenPos);

            if (layerMask.HasValue)
            {
                return Physics.Raycast(ray, out hit, maxDistance, layerMask.Value);
            }

            return Physics.Raycast(ray, out hit, maxDistance);
        }

        /// <summary>
        /// Raycast from current pointer position
        /// </summary>
        public bool RaycastPointer(out RaycastHit hit, float maxDistance = 100f, LayerMask? layerMask = null)
        {
            Vector2 screenPos = Input.touchSupported && Input.touchCount > 0 
                ? (Vector2)Input.GetTouch(0).position 
                : (Vector2)Input.mousePosition;

            return Raycast(screenPos, out hit, maxDistance, layerMask);
        }

        #endregion
    }
}
