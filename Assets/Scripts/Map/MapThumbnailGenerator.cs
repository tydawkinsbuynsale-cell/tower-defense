using UnityEngine;
using System.IO;
using RobotTD.Core;

namespace RobotTD.Map
{
    /// <summary>
    /// Generates thumbnail images for custom maps.
    /// Captures a top-down orthographic view of the map grid and saves as PNG.
    /// </summary>
    public class MapThumbnailGenerator : MonoBehaviour
    {
        public static MapThumbnailGenerator Instance { get; private set; }

        [Header("Thumbnail Settings")]
        [SerializeField] private int thumbnailWidth = 256;
        [SerializeField] private int thumbnailHeight = 256;
        [SerializeField] private LayerMask captureLayerMask = -1; // All layers by default
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        [Header("Camera Settings")]
        [SerializeField] private float cameraHeight = 50f;
        [SerializeField] private float orthographicSize = 15f;
        [SerializeField] private bool autoCalculateSize = true;

        private Camera thumbnailCamera;
        private RenderTexture renderTexture;

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

        private void OnDestroy()
        {
            CleanupResources();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Thumbnail Generation ──────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Generates a thumbnail for the given custom map.
        /// Returns the file path of the saved thumbnail, or null if generation failed.
        /// </summary>
        public string GenerateThumbnail(CustomMapData mapData, Transform gridParent = null)
        {
            if (mapData == null)
            {
                Debug.LogError("[MapThumbnailGenerator] Cannot generate thumbnail for null map data!");
                return null;
            }

            try
            {
                // Setup camera
                SetupThumbnailCamera(mapData, gridParent);

                // Capture the thumbnail
                Texture2D thumbnail = CaptureMapView();

                if (thumbnail == null)
                {
                    Debug.LogError("[MapThumbnailGenerator] Failed to capture map view!");
                    return null;
                }

                // Save thumbnail to file
                string thumbnailPath = SaveThumbnail(thumbnail, mapData.mapId);

                // Cleanup
                Destroy(thumbnail);

                return thumbnailPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapThumbnailGenerator] Error generating thumbnail: {ex.Message}");
                return null;
            }
            finally
            {
                CleanupCamera();
            }
        }

        /// <summary>
        /// Generates a thumbnail from the current scene view.
        /// Useful for generating thumbnails in the map editor.
        /// </summary>
        public string GenerateThumbnailFromScene(string mapId, Vector3 centerPoint, float mapWidth, float mapHeight)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                Debug.LogError("[MapThumbnailGenerator] Map ID is required!");
                return null;
            }

            try
            {
                // Setup camera at center point
                SetupThumbnailCameraAtPosition(centerPoint, mapWidth, mapHeight);

                // Capture the thumbnail
                Texture2D thumbnail = CaptureMapView();

                if (thumbnail == null)
                {
                    Debug.LogError("[MapThumbnailGenerator] Failed to capture scene view!");
                    return null;
                }

                // Save thumbnail to file
                string thumbnailPath = SaveThumbnail(thumbnail, mapId);

                // Cleanup
                Destroy(thumbnail);

                return thumbnailPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapThumbnailGenerator] Error generating thumbnail from scene: {ex.Message}");
                return null;
            }
            finally
            {
                CleanupCamera();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Camera Setup ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void SetupThumbnailCamera(CustomMapData mapData, Transform gridParent)
        {
            // Calculate map center and size
            float mapWidth = mapData.gridWidth;
            float mapHeight = mapData.gridHeight;
            Vector3 centerPoint = new Vector3(mapWidth / 2f, 0, mapHeight / 2f);

            SetupThumbnailCameraAtPosition(centerPoint, mapWidth, mapHeight);
        }

        private void SetupThumbnailCameraAtPosition(Vector3 centerPoint, float mapWidth, float mapHeight)
        {
            // Create camera GameObject
            GameObject cameraObj = new GameObject("ThumbnailCamera");
            cameraObj.transform.position = centerPoint + Vector3.up * cameraHeight;
            cameraObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Look straight down

            // Add and configure camera component
            thumbnailCamera = cameraObj.AddComponent<Camera>();
            thumbnailCamera.orthographic = true;
            thumbnailCamera.clearFlags = CameraClearFlags.SolidColor;
            thumbnailCamera.backgroundColor = backgroundColor;
            thumbnailCamera.cullingMask = captureLayerMask;
            thumbnailCamera.enabled = false; // Manual rendering

            // Calculate orthographic size to fit the entire map
            if (autoCalculateSize)
            {
                float maxDimension = Mathf.Max(mapWidth, mapHeight);
                thumbnailCamera.orthographicSize = maxDimension / 2f + 1f; // Add 1 unit padding
            }
            else
            {
                thumbnailCamera.orthographicSize = orthographicSize;
            }

            // Create render texture
            renderTexture = new RenderTexture(thumbnailWidth, thumbnailHeight, 24);
            renderTexture.antiAliasing = 4; // 4x MSAA for better quality
            thumbnailCamera.targetTexture = renderTexture;

            Debug.Log($"[MapThumbnailGenerator] Camera setup at {centerPoint} with ortho size {thumbnailCamera.orthographicSize}");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Capture & Save ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private Texture2D CaptureMapView()
        {
            if (thumbnailCamera == null || renderTexture == null)
            {
                Debug.LogError("[MapThumbnailGenerator] Camera or render texture not initialized!");
                return null;
            }

            // Render the camera view
            thumbnailCamera.Render();

            // Read pixels from render texture
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(thumbnailWidth, thumbnailHeight, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, thumbnailWidth, thumbnailHeight), 0, 0);
            texture.Apply();

            RenderTexture.active = null;

            return texture;
        }

        private string SaveThumbnail(Texture2D thumbnail, string mapId)
        {
            if (thumbnail == null)
            {
                Debug.LogError("[MapThumbnailGenerator] Cannot save null thumbnail!");
                return null;
            }

            // Encode to PNG
            byte[] bytes = thumbnail.EncodeToPNG();

            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("[MapThumbnailGenerator] Failed to encode thumbnail to PNG!");
                return null;
            }

            // Determine save path
            string thumbnailsPath = Path.Combine(Application.persistentDataPath, "Thumbnails");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(thumbnailsPath))
            {
                Directory.CreateDirectory(thumbnailsPath);
            }

            string fileName = $"{mapId}.png";
            string filePath = Path.Combine(thumbnailsPath, fileName);

            // Save to file
            File.WriteAllBytes(filePath, bytes);

            Debug.Log($"[MapThumbnailGenerator] Thumbnail saved to: {filePath}");

            return filePath;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Cleanup ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void CleanupCamera()
        {
            if (thumbnailCamera != null)
            {
                Destroy(thumbnailCamera.gameObject);
                thumbnailCamera = null;
            }
        }

        private void CleanupResources()
        {
            CleanupCamera();

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Utility Methods ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Deletes the thumbnail file for the given map ID.
        /// </summary>
        public bool DeleteThumbnail(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
                return false;

            try
            {
                string thumbnailsPath = Path.Combine(Application.persistentDataPath, "Thumbnails");
                string filePath = Path.Combine(thumbnailsPath, $"{mapId}.png");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[MapThumbnailGenerator] Deleted thumbnail: {filePath}");
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapThumbnailGenerator] Error deleting thumbnail: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a thumbnail exists for the given map ID.
        /// </summary>
        public bool ThumbnailExists(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
                return false;

            string thumbnailsPath = Path.Combine(Application.persistentDataPath, "Thumbnails");
            string filePath = Path.Combine(thumbnailsPath, $"{mapId}.png");

            return File.Exists(filePath);
        }

        /// <summary>
        /// Gets the file path for a map's thumbnail.
        /// </summary>
        public string GetThumbnailPath(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
                return null;

            string thumbnailsPath = Path.Combine(Application.persistentDataPath, "Thumbnails");
            string filePath = Path.Combine(thumbnailsPath, $"{mapId}.png");

            return File.Exists(filePath) ? filePath : null;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Configuration ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════────────════════════

        public void SetThumbnailSize(int width, int height)
        {
            thumbnailWidth = Mathf.Clamp(width, 64, 1024);
            thumbnailHeight = Mathf.Clamp(height, 64, 1024);
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
        }

        public void SetCaptureLayerMask(LayerMask mask)
        {
            captureLayerMask = mask;
        }
    }
}
