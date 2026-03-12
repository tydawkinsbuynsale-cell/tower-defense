using UnityEngine;

namespace RobotTD.Map
{
    /// <summary>
    /// Mobile-friendly camera controller with touch pan and pinch zoom.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Bounds")]
        [SerializeField] private Vector2 xBounds = new Vector2(-15, 15);
        [SerializeField] private Vector2 zBounds = new Vector2(-10, 25);

        [Header("Zoom")]
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float touchZoomSpeed = 0.01f;

        [Header("Pan")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float touchPanSensitivity = 0.01f;
        [SerializeField] private float panSmoothTime = 0.1f;

        [Header("Settings")]
        [SerializeField] private bool invertDrag = true;
        [SerializeField] private float edgePanThreshold = 30f; // Pixels from screen edge
        [SerializeField] private float edgePanSpeed = 10f;
        [SerializeField] private bool enableEdgePan = false; // Usually disabled on mobile

        [Header("Camera Shake")]
        [SerializeField] private float shakeDuration = 0.1f;
        [SerializeField] private float shakeMagnitude = 0.2f;

        // Internal state
        private Camera cam;
        private Vector3 targetPosition;
        private Vector3 velocity = Vector3.zero;
        private float targetZoom;
        private bool isDragging;
        private Vector3 dragStartWorld;

        // Touch state
        private int activeTouchId = -1;
        private float initialPinchDistance;
        private float initialZoom;

        // Shake state
        private float currentShakeDuration;
        private Vector3 shakeOffset;

        // Blocking
        private bool inputBlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }

            targetPosition = transform.position;
            targetZoom = cam.orthographicSize;
        }

        private void Update()
        {
            if (inputBlocked || Core.GameManager.Instance?.IsPaused == true)
            {
                return;
            }

            // Handle input based on platform
            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
                HandleKeyboardInput();
                
                if (enableEdgePan)
                {
                    HandleEdgePan();
                }
            }

            // Apply smooth movement
            ApplyCameraMovement();

            // Apply camera shake
            UpdateShake();
        }

        #region Touch Input

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                // Single touch - pan
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        isDragging = true;
                        activeTouchId = touch.fingerId;
                        dragStartWorld = GetWorldPoint(touch.position);
                        break;

                    case TouchPhase.Moved:
                        if (isDragging && touch.fingerId == activeTouchId)
                        {
                            Vector3 currentWorld = GetWorldPoint(touch.position);
                            Vector3 delta = dragStartWorld - currentWorld;

                            if (invertDrag)
                            {
                                delta = -delta;
                            }

                            targetPosition += delta;
                            dragStartWorld = GetWorldPoint(touch.position);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == activeTouchId)
                        {
                            isDragging = false;
                            activeTouchId = -1;
                        }
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                // Two touch - pinch zoom
                isDragging = false;
                activeTouchId = -1;

                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    initialZoom = targetZoom;
                }
                else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                    float delta = initialPinchDistance - currentDistance;

                    targetZoom = initialZoom + delta * touchZoomSpeed;
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                }
            }
        }

        private Vector3 GetWorldPoint(Vector2 screenPoint)
        {
            Ray ray = cam.ScreenPointToRay(screenPoint);
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            
            if (ground.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        #endregion

        #region Mouse Input

        private void HandleMouseInput()
        {
            // Scroll wheel zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSpeed;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }

            // Middle mouse or right mouse drag
            if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                dragStartWorld = GetWorldPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 currentWorld = GetWorldPoint(Input.mousePosition);
                Vector3 delta = dragStartWorld - currentWorld;

                if (!invertDrag)
                {
                    delta = -delta;
                }

                targetPosition += delta;
                dragStartWorld = GetWorldPoint(Input.mousePosition);
            }
        }

        private void HandleKeyboardInput()
        {
            Vector3 move = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                move.z += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                move.z -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                move.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                move.x += 1;

            if (move.magnitude > 0.1f)
            {
                targetPosition += move.normalized * panSpeed * Time.deltaTime;
            }

            // Keyboard zoom
            if (Input.GetKey(KeyCode.Q))
            {
                targetZoom += zoomSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.E))
            {
                targetZoom -= zoomSpeed * Time.deltaTime;
            }

            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        private void HandleEdgePan()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 move = Vector3.zero;

            if (mousePos.x < edgePanThreshold)
                move.x -= 1;
            if (mousePos.x > Screen.width - edgePanThreshold)
                move.x += 1;
            if (mousePos.y < edgePanThreshold)
                move.z -= 1;
            if (mousePos.y > Screen.height - edgePanThreshold)
                move.z += 1;

            if (move.magnitude > 0.1f)
            {
                targetPosition += move.normalized * edgePanSpeed * Time.deltaTime;
            }
        }

        #endregion

        #region Camera Movement

        private void ApplyCameraMovement()
        {
            // Clamp target position to bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, xBounds.x, xBounds.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, zBounds.x, zBounds.y);

            // Smooth position
            Vector3 newPos = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                panSmoothTime
            );

            // Maintain Y position
            newPos.y = transform.position.y;
            transform.position = newPos + shakeOffset;

            // Smooth zoom
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetZoom,
                Time.deltaTime * 10f
            );
        }

        #endregion

        #region Camera Shake

        private void UpdateShake()
        {
            if (currentShakeDuration > 0)
            {
                float progress = 1 - (currentShakeDuration / shakeDuration);
                float magnitude = shakeMagnitude * (1 - progress);

                shakeOffset = Random.insideUnitSphere * magnitude;
                shakeOffset.y = 0;

                currentShakeDuration -= Time.deltaTime;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
        }

        /// <summary>
        /// Trigger camera shake (e.g., on big explosion)
        /// </summary>
        public void Shake(float duration = -1, float magnitude = -1)
        {
            currentShakeDuration = duration > 0 ? duration : shakeDuration;
            shakeMagnitude = magnitude > 0 ? magnitude : this.shakeMagnitude;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Block camera input (during tower placement, UI interaction, etc.)
        /// </summary>
        public void BlockInput(bool block)
        {
            inputBlocked = block;
        }

        /// <summary>
        /// Instantly move camera to position
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            targetPosition = position;
            transform.position = new Vector3(position.x, transform.position.y, position.z);
        }

        /// <summary>
        /// Smoothly pan camera to position
        /// </summary>
        public void PanTo(Vector3 position)
        {
            targetPosition = position;
        }

        /// <summary>
        /// Set zoom level
        /// </summary>
        public void SetZoom(float zoom)
        {
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        /// <summary>
        /// Focus on a specific point with optional zoom
        /// </summary>
        public void FocusOn(Vector3 position, float zoom = -1)
        {
            PanTo(position);
            if (zoom > 0)
            {
                SetZoom(zoom);
            }
        }

        /// <summary>
        /// Reset camera to starting position
        /// </summary>
        public void ResetCamera()
        {
            if (MapManager.Instance?.SpawnPoint != null)
            {
                FocusOn(MapManager.Instance.SpawnPoint.position, (minZoom + maxZoom) * 0.5f);
            }
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            // Draw camera bounds
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (xBounds.x + xBounds.y) / 2f,
                transform.position.y,
                (zBounds.x + zBounds.y) / 2f
            );
            Vector3 size = new Vector3(
                xBounds.y - xBounds.x,
                0.1f,
                zBounds.y - zBounds.x
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}
