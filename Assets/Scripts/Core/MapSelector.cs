using UnityEngine;

namespace RobotTD.Core
{
    /// <summary>
    /// Singleton service for managing the currently selected map.
    /// Persists the selected map ID across scene loads.
    /// Used by MapSelectUI → GameManager → MapManager flow.
    /// </summary>
    public class MapSelector : MonoBehaviour
    {
        public static MapSelector Instance { get; private set; }

        [Header("Current Selection")]
        [SerializeField] private string selectedMapId;
        private Map.MapData selectedMapData;

        [Header("Registry")]
        [SerializeField] private Map.MapRegistry mapRegistry;

        public string SelectedMapId => selectedMapId;
        public Map.MapData SelectedMapData => selectedMapData;

        private void Awake()
        {
            // Singleton with DontDestroyOnLoad
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Set the map to load for the next gameplay session
        /// </summary>
        public void SelectMap(string mapId)
        {
            selectedMapId = mapId;

            if (mapRegistry != null)
            {
                selectedMapData = mapRegistry.GetMap(mapId);
                if (selectedMapData != null)
                {
                    Debug.Log($"[MapSelector] Selected map: {selectedMapData.mapName} ({mapId})");
                }
                else
                {
                    Debug.LogWarning($"[MapSelector] Map not found in registry: {mapId}");
                }
            }
            else
            {
                Debug.LogWarning("[MapSelector] MapRegistry not assigned!");
            }
        }

        /// <summary>
        /// Select map by MapData directly
        /// </summary>
        public void SelectMap(Map.MapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogWarning("[MapSelector] Attempted to select null MapData");
                return;
            }

            selectedMapId = mapData.name;
            selectedMapData = mapData;
            Debug.Log($"[MapSelector] Selected map: {mapData.mapName} ({mapData.name})");
        }

        /// <summary>
        /// Get the currently selected map data
        /// </summary>
        public Map.MapData GetSelectedMap()
        {
            // If map data not loaded yet, try to load from registry
            if (selectedMapData == null && !string.IsNullOrEmpty(selectedMapId))
            {
                if (mapRegistry != null)
                {
                    selectedMapData = mapRegistry.GetMap(selectedMapId);
                }
            }

            return selectedMapData;
        }

        /// <summary>
        /// Clear selection (for cleanup)
        /// </summary>
        public void ClearSelection()
        {
            selectedMapId = string.Empty;
            selectedMapData = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Set map registry reference (for testing in editor)
        /// </summary>
        public void SetMapRegistry(Map.MapRegistry registry)
        {
            mapRegistry = registry;
        }
#endif
    }
}
