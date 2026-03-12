using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Analytics;
using RobotTD.Map;

namespace RobotTD.Core
{
    /// <summary>
    /// Manages custom map creation and editing functionality.
    /// Handles tile placement, path drawing, validation, and save/load.
    /// </summary>
    public class MapEditorManager : MonoBehaviour
    {
        public static MapEditorManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private int maxUndoSteps = 50;
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 60f; // seconds

        [Header("Grid Settings")]
        [SerializeField] private int defaultGridWidth = 20;
        [SerializeField] private int defaultGridHeight = 15;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Vector3 gridOffset = Vector3.zero;

        [Header("Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject spawnPointPrefab;
        [SerializeField] private GameObject basePrefab;
        [SerializeField] private GameObject pathMarkerPrefab;

        // ── State ─────────────────────────────────────────────────────────────
        private CustomMapData currentMap;
        private EditorMode currentMode = EditorMode.Tile;
        private TileType selectedTileType = TileType.Buildable;
        private bool isEditing = false;
        private bool hasUnsavedChanges = false;

        // ── Undo System ───────────────────────────────────────────────────────
        private Stack<CustomMapData> undoStack = new Stack<CustomMapData>();
        private Stack<CustomMapData> redoStack = new Stack<CustomMapData>();

        // ── Path Drawing ──────────────────────────────────────────────────────
        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private bool isDrawingPath = false;

        // ── Visual Representation ─────────────────────────────────────────────
        private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();
        private Transform gridParent;

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<CustomMapData> OnMapLoaded;
        public event Action OnMapSaved;
        public event Action<string> OnMapValidated;
        public event Action OnUndoRedoChanged;
        public event Action<EditorMode> OnModeChanged;
        public event Action OnTileChanged;

        private float autoSaveTimer = 0f;

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

            InitializeGridParent();
        }

        private void Update()
        {
            if (!isEditing) return;

            // Auto-save timer
            if (autoSave && hasUnsavedChanges)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    AutoSaveCurrentMap();
                    autoSaveTimer = 0f;
                }
            }

            // Keyboard shortcuts
            HandleKeyboardShortcuts();
        }

        private void InitializeGridParent()
        {
            var gridObj = new GameObject("MapEditorGrid");
            gridParent = gridObj.transform;
            gridParent.SetParent(transform);
            gridParent.localPosition = gridOffset;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Map Creation & Loading ────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Create a new blank map for editing.
        /// </summary>
        public void CreateNewMap(string mapName = "New Map", int width = 0, int height = 0)
        {
            if (width <= 0) width = defaultGridWidth;
            if (height <= 0) height = defaultGridHeight;

            currentMap = new CustomMapData(width, height);
            currentMap.mapName = mapName;
            
            // Set author info if authenticated
            if (Online.AuthenticationManager.Instance != null && Online.AuthenticationManager.Instance.IsAuthenticated)
            {
                currentMap.authorId = Online.AuthenticationManager.Instance.PlayerId;
                currentMap.authorName = Online.AuthenticationManager.Instance.PlayerName;
            }
            else
            {
                currentMap.authorId = SystemInfo.deviceUniqueIdentifier;
                currentMap.authorName = "Local Player";
            }

            ClearUndoRedo();
            SaveStateForUndo();
            hasUnsavedChanges = false;
            isEditing = true;

            GenerateGridVisuals();
            OnMapLoaded?.Invoke(currentMap);

            LogDebug($"Created new map: {mapName} ({width}x{height})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_editor_new_map", new Dictionary<string, object>
                {
                    { "map_name", mapName },
                    { "grid_width", width },
                    { "grid_height", height }
                });
            }
        }

        /// <summary>
        /// Load an existing custom map for editing.
        /// </summary>
        public void LoadMap(CustomMapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogError("Cannot load null map data");
                return;
            }

            currentMap = mapData.Clone(); // Work on a copy
            ClearUndoRedo();
            SaveStateForUndo();
            hasUnsavedChanges = false;
            isEditing = true;

            GenerateGridVisuals();
            OnMapLoaded?.Invoke(currentMap);

            LogDebug($"Loaded map: {currentMap.mapName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_editor_load_map", new Dictionary<string, object>
                {
                    { "map_id", currentMap.mapId },
                    { "map_name", currentMap.mapName }
                });
            }
        }

        /// <summary>
        /// Save the current map.
        /// </summary>
        public bool SaveCurrentMap()
        {
            if (currentMap == null)
            {
                Debug.LogError("No map to save");
                return false;
            }

            // Validate before saving
            var validationResult = ValidateMap(currentMap);
            if (!validationResult.isValid)
            {
                Debug.LogWarning($"Map has validation errors: {string.Join(", ", validationResult.errors)}");
                // Allow saving even with errors, but mark as invalid
                currentMap.isValid = false;
                currentMap.validationErrors = validationResult.errors;
            }
            else
            {
                currentMap.isValid = true;
                currentMap.validationErrors.Clear();
            }

            currentMap.lastModifiedDate = DateTime.UtcNow;

            // Save to PlayerPrefs (local storage)
            string json = JsonUtility.ToJson(currentMap);
            PlayerPrefs.SetString($"CustomMap_{currentMap.mapId}", json);
            PlayerPrefs.Save();

            // Generate thumbnail
            GenerateThumbnail();

            hasUnsavedChanges = false;
            OnMapSaved?.Invoke();

            LogDebug($"Saved map: {currentMap.mapName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_editor_save_map", new Dictionary<string, object>
                {
                    { "map_id", currentMap.mapId },
                    { "map_name", currentMap.mapName },
                    { "is_valid", currentMap.isValid }
                });
            }

            return true;
        }

        private void AutoSaveCurrentMap()
        {
            if (currentMap != null && hasUnsavedChanges)
            {
                SaveCurrentMap();
                LogDebug("Auto-saved map");
            }
        }

        /// <summary>
        /// Close current map and exit editing mode.
        /// </summary>
        public void CloseCurrentMap(bool saveBeforeClose = true)
        {
            if (hasUnsavedChanges && saveBeforeClose)
            {
                SaveCurrentMap();
            }

            isEditing = false;
            ClearGridVisuals();
            ClearUndoRedo();
            currentMap = null;
            hasUnsavedChanges = false;

            LogDebug("Closed current map");
        }

        /// <summary>
        /// Generates a thumbnail for the current map.
        /// Uses MapThumbnailGenerator to capture a top-down view of the grid.
        /// </summary>
        private void GenerateThumbnail()
        {
            if (currentMap == null)
            {
                Debug.LogWarning("[MapEditorManager] Cannot generate thumbnail: no current map!");
                return;
            }

            // Ensure thumbnail generator exists
            if (RobotTD.Map.MapThumbnailGenerator.Instance == null)
            {
                // Create thumbnail generator if it doesn't exist
                GameObject generatorObj = new GameObject("MapThumbnailGenerator");
                generatorObj.AddComponent<RobotTD.Map.MapThumbnailGenerator>();
                LogDebug("Created MapThumbnailGenerator instance");
            }

            // Calculate map center and dimensions
            float mapWidth = currentMap.gridWidth * tileSize;
            float mapHeight = currentMap.gridHeight * tileSize;
            Vector3 centerPoint = gridOffset + new Vector3(mapWidth / 2f, 0, mapHeight / 2f);

            // Generate thumbnail
            string thumbnailPath = RobotTD.Map.MapThumbnailGenerator.Instance.GenerateThumbnailFromScene(
                currentMap.mapId,
                centerPoint,
                mapWidth,
                mapHeight
            );

            if (!string.IsNullOrEmpty(thumbnailPath))
            {
                LogDebug($"Generated thumbnail: {thumbnailPath}");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("map_editor_generate_thumbnail", new Dictionary<string, object>
                    {
                        { "map_id", currentMap.mapId },
                        { "map_name", currentMap.mapName },
                        { "success", true }
                    });
                }
            }
            else
            {
                Debug.LogWarning($"[MapEditorManager] Failed to generate thumbnail for map: {currentMap.mapName}");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("map_editor_generate_thumbnail", new Dictionary<string, object>
                    {
                        { "map_id", currentMap.mapId },
                        { "map_name", currentMap.mapName },
                        { "success", false }
                    });
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Tile Editing ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Set tile at grid position.
        /// </summary>
        public void SetTile(int x, int y, TileType type)
        {
            if (currentMap == null || !isEditing) return;
            if (!currentMap.IsPositionValid(x, y)) return;

            SaveStateForUndo();

            currentMap.SetTile(x, y, type);
            UpdateTileVisual(x, y);

            hasUnsavedChanges = true;
            OnTileChanged?.Invoke();

            LogDebug($"Set tile at ({x}, {y}) to {type}");
        }

        /// <summary>
        /// Paint multiple tiles (for brush tool).
        /// </summary>
        public void PaintTiles(List<Vector2Int> positions, TileType type)
        {
            if (currentMap == null || !isEditing) return;
            if (positions == null || positions.Count == 0) return;

            SaveStateForUndo();

            foreach (var pos in positions)
            {
                if (currentMap.IsPositionValid(pos.x, pos.y))
                {
                    currentMap.SetTile(pos.x, pos.y, type);
                    UpdateTileVisual(pos.x, pos.y);
                }
            }

            hasUnsavedChanges = true;
            OnTileChanged?.Invoke();
        }

        /// <summary>
        /// Flood fill tiles from a starting position.
        /// </summary>
        public void FloodFillTiles(int startX, int startY, TileType targetType, TileType replacementType)
        {
            if (currentMap == null || !isEditing) return;
            if (!currentMap.IsPositionValid(startX, startY)) return;
            if (currentMap.GetTile(startX, startY) != targetType) return;
            if (targetType == replacementType) return;

            SaveStateForUndo();

            // BFS flood fill
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            
            queue.Enqueue(new Vector2Int(startX, startY));
            visited.Add(new Vector2Int(startX, startY));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                currentMap.SetTile(current.x, current.y, replacementType);
                UpdateTileVisual(current.x, current.y);

                // Check neighbors (4-directional)
                Vector2Int[] neighbors = {
                    new Vector2Int(current.x + 1, current.y),
                    new Vector2Int(current.x - 1, current.y),
                    new Vector2Int(current.x, current.y + 1),
                    new Vector2Int(current.x, current.y - 1)
                };

                foreach (var neighbor in neighbors)
                {
                    if (currentMap.IsPositionValid(neighbor.x, neighbor.y) &&
                        !visited.Contains(neighbor) &&
                        currentMap.GetTile(neighbor.x, neighbor.y) == targetType)
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }

            hasUnsavedChanges = true;
            OnTileChanged?.Invoke();

            LogDebug($"Flood filled from ({startX}, {startY})");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Path Drawing ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Start drawing a path from a position.
        /// </summary>
        public void StartDrawingPath(int x, int y)
        {
            if (currentMap == null || !isEditing) return;

            SaveStateForUndo();

            isDrawingPath = true;
            currentPath.Clear();
            currentPath.Add(new Vector2Int(x, y));

            LogDebug($"Started drawing path at ({x}, {y})");
        }

        /// <summary>
        /// Continue drawing path to new position.
        /// </summary>
        public void ContinueDrawingPath(int x, int y)
        {
            if (!isDrawingPath || currentMap == null) return;

            Vector2Int newPos = new Vector2Int(x, y);
            
            // Only add if adjacent to last position (4-directional)
            if (currentPath.Count > 0)
            {
                var lastPos = currentPath[currentPath.Count - 1];
                int dx = Mathf.Abs(newPos.x - lastPos.x);
                int dy = Mathf.Abs(newPos.y - lastPos.y);

                // Must be adjacent (not diagonal)
                if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
                {
                    if (!currentPath.Contains(newPos))
                    {
                        currentPath.Add(newPos);
                        currentMap.SetTile(x, y, TileType.Path);
                        UpdateTileVisual(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Finish drawing the current path.
        /// </summary>
        public void FinishDrawingPath()
        {
            if (!isDrawingPath) return;

            // Add path to map's path list
            currentMap.pathTiles.AddRange(currentPath);

            isDrawingPath= false;
            hasUnsavedChanges = true;

            LogDebug($"Finished drawing path with {currentPath.Count} tiles");

            currentPath.Clear();
        }

        /// <summary>
        /// Cancel path drawing.
        /// </summary>
        public void CancelDrawingPath()
        {
            if (!isDrawingPath) return;

            // Revert tiles
            foreach (var pos in currentPath)
            {
                currentMap.SetTile(pos.x, pos.y, TileType.Buildable);
                UpdateTileVisual(pos.x, pos.y);
            }

            isDrawingPath = false;
            currentPath.Clear();

            LogDebug("Cancelled path drawing");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Spawn Points & Base ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Place a spawn point at position.
        /// </summary>
        public void PlaceSpawnPoint(int x, int y, int waveStart = 1)
        {
            if (currentMap == null || !isEditing) return;
            if (!currentMap.IsPositionValid(x, y)) return;

            SaveStateForUndo();

            var position = new Vector2Int(x, y);
            
            // Remove existing spawn at this position
            currentMap.RemoveSpawnPoint(position);
            
            // Add new spawn point
            currentMap.AddSpawnPoint(position, waveStart);
            currentMap.SetTile(x, y, TileType.SpawnPoint);
            UpdateTileVisual(x, y);

            hasUnsavedChanges = true;

            LogDebug($"Placed spawn point at ({x}, {y})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_editor_place_spawn", new Dictionary<string, object>
                {
                    { "position_x", x },
                    { "position_y", y }
                });
            }
        }

        /// <summary>
        /// Remove spawn point at position.
        /// </summary>
        public void RemoveSpawnPoint(int x, int y)
        {
            if (currentMap == null || !isEditing) return;

            SaveStateForUndo();

            var position = new Vector2Int(x, y);
            currentMap.RemoveSpawnPoint(position);
            currentMap.SetTile(x, y, TileType.Buildable);
            UpdateTileVisual(x, y);

            hasUnsavedChanges = true;

            LogDebug($"Removed spawn point at ({x}, {y})");
        }

        /// <summary>
        /// Place the player base at position.
        /// </summary>
        public void PlaceBase(int x, int y)
        {
            if (currentMap == null || !isEditing) return;
            if (!currentMap.IsPositionValid(x, y)) return;

            SaveStateForUndo();

            // Clear old base position
            if (currentMap.basePosition != Vector2Int.zero)
            {
                currentMap.SetTile(currentMap.basePosition.x, currentMap.basePosition.y, TileType.Buildable);
                UpdateTileVisual(currentMap.basePosition.x, currentMap.basePosition.y);
            }

            // Set new base
            currentMap.SetBasePosition(new Vector2Int(x, y));
            currentMap.SetTile(x, y, TileType.Base);
            UpdateTileVisual(x, y);

            hasUnsavedChanges = true;

            LogDebug($"Placed base at ({x}, {y})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_editor_place_base", new Dictionary<string, object>
                {
                    { "position_x", x },
                    { "position_y", y }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Undo / Redo ───────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void SaveStateForUndo()
        {
            if (currentMap == null) return;

            undoStack.Push(currentMap.Clone());
            
            // Limit stack size
            if (undoStack.Count > maxUndoSteps)
            {
                var tempStack = new Stack<CustomMapData>();
                for (int i = 0; i < maxUndoSteps; i++)
                {
                    tempStack.Push(undoStack.Pop());
                }
                undoStack = tempStack;
            }

            // Clear redo stack when new action is made
            redoStack.Clear();

            OnUndoRedoChanged?.Invoke();
        }

        /// <summary>
        /// Undo last action.
        /// </summary>
        public void Undo()
        {
            if (undoStack.Count == 0)
            {
                LogDebug("Nothing to undo");
                return;
            }

            redoStack.Push(currentMap.Clone());
            currentMap = undoStack.Pop();

            RegenerateGridVisuals();
            hasUnsavedChanges = true;

            OnUndoRedoChanged?.Invoke();

            LogDebug("Undo performed");
        }

        /// <summary>
        /// Redo last undone action.
        /// </summary>
        public void Redo()
        {
            if (redoStack.Count == 0)
            {
                LogDebug("Nothing to redo");
                return;
            }

            undoStack.Push(currentMap.Clone());
            currentMap = redoStack.Pop();

            RegenerateGridVisuals();
            hasUnsavedChanges = true;

            OnUndoRedoChanged?.Invoke();

            LogDebug("Redo performed");
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        private void ClearUndoRedo()
        {
            undoStack.Clear();
            redoStack.Clear();
            OnUndoRedoChanged?.Invoke();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Map Validation ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Validate map for playability.
        /// </summary>
        public MapValidationResult ValidateMap(CustomMapData map)
        {
            var result = new MapValidationResult();

            if (map == null)
            {
                result.AddError("Map is null");
                return result;
            }

            // Check map name
            if (string.IsNullOrEmpty(map.mapName) || map.mapName.Trim().Length < 3)
            {
                result.AddError("Map name must be at least 3 characters");
            }

            // Check spawn points
            if (map.spawnPoints.Count == 0)
            {
                result.AddError("Map must have at least one spawn point");
            }

            // Check base
            if (map.basePosition == Vector2Int.zero)
            {
                result.AddError("Map must have a base/objective placement");
            }

            // Check path connectivity from each spawn to base
            foreach (var spawn in map.spawnPoints)
            {
                if (!IsPathConnected(map, spawn.position, map.basePosition))
                {
                    result.AddError($"Spawn at ({spawn.position.x}, {spawn.position.y}) has no valid path to base");
                }
            }

            // Check wave configuration
            if (map.waves.Count == 0)
            {
                result.AddWarning("Map has no wave configuration (default waves will be used)");
            }
            else
            {
                // Validate wave configuration details
                for (int i = 0; i < map.waves.Count; i++)
                {
                    var wave = map.waves[i];
                    
                    // Check wave number consistency
                    if (wave.waveNumber != i + 1)
                    {
                        result.AddWarning($"Wave {i + 1} has incorrect wave number ({wave.waveNumber})");
                    }

                    // Check enemy groups
                    if (wave.enemyGroups.Count == 0)
                    {
                        result.AddError($"Wave {wave.waveNumber} has no enemy groups");
                    }
                    else
                    {
                        int totalEnemies = 0;
                        foreach (var group in wave.enemyGroups)
                        {
                            totalEnemies += group.count;
                            
                            // Validate enemy count
                            if (group.count <= 0)
                            {
                                result.AddError($"Wave {wave.waveNumber} has enemy group with invalid count: {group.count}");
                            }
                            
                            // Validate spawn interval
                            if (group.spawnInterval < 0.1f)
                            {
                                result.AddWarning($"Wave {wave.waveNumber} has very fast spawn interval ({group.spawnInterval}s)");
                            }
                            
                            // Validate enemy type
                            if (string.IsNullOrEmpty(group.enemyType))
                            {
                                result.AddError($"Wave {wave.waveNumber} has enemy group with no type");
                            }
                        }

                        // Check total enemy count per wave
                        if (totalEnemies > 100)
                        {
                            result.AddWarning($"Wave {wave.waveNumber} has many enemies ({totalEnemies}). May cause performance issues.");
                        }
                        else if (totalEnemies < 5)
                        {
                            result.AddSuggestion($"Wave {wave.waveNumber} has few enemies ({totalEnemies}). Consider adding more for challenge.");
                        }
                    }

                    // Check credits reward
                    if (wave.creditsReward <= 0)
                    {
                        result.AddWarning($"Wave {wave.waveNumber} has no credits reward");
                    }
                }

                // Check wave count recommendations
                if (map.waves.Count < 5)
                {
                    result.AddSuggestion($"Map has only {map.waves.Count} waves. Consider adding more for longer gameplay.");
                }
                else if (map.waves.Count > 50)
                {
                    result.AddWarning($"Map has {map.waves.Count} waves. Very long gameplay may impact player retention.");
                }
            }

            // Check buildable space
            int buildableCount = 0;
            for (int x = 0; x < map.gridWidth; x++)
            {
                for (int y = 0; y < map.gridHeight; y++)
                {
                    if (map.GetTile(x, y) == TileType.Buildable)
                        buildableCount++;
                }
            }

            if (buildableCount < 10)
            {
                result.AddWarning("Map has very little buildable space (less than 10 tiles)");
            }

            // Balance suggestions
            if (map.startingCredits < 300)
            {
                result.AddSuggestion("Consider increasing starting credits for better player experience");
            }

            result.isValid = !result.HasErrors;
            return result;
        }

        private bool IsPathConnected(CustomMapData map, Vector2Int start, Vector2Int end)
        {
            // A* pathfinding to check connectivity
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                if (current == end)
                    return true;

                // Check neighbors (4-directional)
                Vector2Int[] neighbors = {
                    new Vector2Int(current.x + 1, current.y),
                    new Vector2Int(current.x - 1, current.y),
                    new Vector2Int(current.x, current.y + 1),
                    new Vector2Int(current.x, current.y - 1)
                };

                foreach (var neighbor in neighbors)
                {
                    if (!map.IsPositionValid(neighbor.x, neighbor.y))
                        continue;

                    if (visited.Contains(neighbor))
                        continue;

                    var tileType = map.GetTile(neighbor.x, neighbor.y);
                    
                    // Can traverse paths and base tiles
                    if (tileType == TileType.Path || tileType == TileType.Base)
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }

            return false; // No path found
        }

        // ══════════════════════════════════════════════════════════════════════
        // ──Visual Representation ──────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void GenerateGridVisuals()
        {
            ClearGridVisuals();

            if (currentMap == null) return;

            for (int x = 0; x < currentMap.gridWidth; x++)
            {
                for (int y = 0; y < currentMap.gridHeight; y++)
                {
                    CreateTileVisual(x, y);
                }
            }

            LogDebug("Generated grid visuals");
        }

        private void RegenerateGridVisuals()
        {
            GenerateGridVisuals();
        }

        private void CreateTileVisual(int x, int y)
        {
            if (tilePrefab == null) return;

            Vector3 worldPos = new Vector3(x * tileSize, 0, y * tileSize);
            GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, gridParent);
            tileObj.name = $"Tile_{x}_{y}";

            // TODO: Set tile visual based on type
            // Apply material/color based on currentMap.GetTile(x, y)

            tileObjects[new Vector2Int(x, y)] = tileObj;
        }

        private void UpdateTileVisual(int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);
            
            if (tileObjects.ContainsKey(pos))
            {
                // TODO: Update tile appearance based on type
                // currentMap.GetTile(x, y)
            }
        }

        private void ClearGridVisuals()
        {
            foreach (var tileObj in tileObjects.Values)
            {
                if (tileObj != null)
                    Destroy(tileObj);
            }
            
            tileObjects.Clear();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Keyboard Shortcuts ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void HandleKeyboardShortcuts()
        {
            // Ctrl+Z = Undo
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
            }

            // Ctrl+Y = Redo
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            {
                Redo();
            }

            // Ctrl+S = Save
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                SaveCurrentMap();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Utilities ─────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        // Static property to store map for test play session
        public static CustomMapData PendingTestPlayMap { get; set; }
        public static bool IsTestPlayMode { get; set; }

        /// <summary>
        /// Test play the current map. Validates first, then loads game scene.
        /// </summary>
        public void TestPlayCurrentMap()
        {
            if (currentMap == null)
            {
                Debug.LogError("[MapEditorManager] No map to test play!");
                return;
            }

            // Validate map first
            var validationResult = ValidateMap(currentMap);
            
            if (validationResult.Errors.Count > 0)
            {
                Debug.LogWarning($"[MapEditorManager] Cannot test play - map has {validationResult.Errors.Count} error(s)!");
                
                // Show error to user
                string errorMessage = "Cannot test play map with errors:\n\n";
                foreach (var error in validationResult.Errors)
                {
                    errorMessage += $"• {error}\n";
                }
                
                OnMapValidated?.Invoke(errorMessage);
                return;
            }

            // Save map before test play
            bool saved = SaveCurrentMap();
            if (!saved)
            {
                Debug.LogError("[MapEditorManager] Failed to save map before test play!");
                return;
            }

            // Store map for test play
            PendingTestPlayMap = currentMap.Clone();
            IsTestPlayMode = true;

            // Log wave configuration status
            if (currentMap.waves.Count > 0)
            {
                LogDebug($"Starting test play with {currentMap.waves.Count} custom waves");
            }
            else
            {
                LogDebug($"Starting test play with procedural waves (no custom wave configuration)");
            }

            LogDebug($"Test playing map: {currentMap.mapName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_editor_test_play", new Dictionary<string, object>
                {
                    { "map_id", currentMap.mapId },
                    { "map_name", currentMap.mapName },
                    { "grid_width", currentMap.gridWidth },
                    { "grid_height", currentMap.gridHeight },
                    { "is_valid", validationResult.IsValid }
                });
            }

            // Load game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Return to map editor from test play.
        /// </summary>
        public static void ReturnToEditor()
        {
            IsTestPlayMode = false;
            PendingTestPlayMap = null;
            
            // Load editor scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapEditor");
        }

        public CustomMapData GetCurrentMap() => currentMap;
        public bool IsEditing() => isEditing;
        public bool HasUnsavedChanges() => hasUnsavedChanges;
        public EditorMode GetCurrentMode() => currentMode;
        public TileType GetSelectedTileType() => selectedTileType;

        public void SetEditorMode(EditorMode mode)
        {
            currentMode = mode;
            OnModeChanged?.Invoke(mode);
            LogDebug($"Editor mode changed to: {mode}");
        }

        public void SetSelectedTileType(TileType type)
        {
            selectedTileType = type;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[MapEditorManager] {message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Context Menu Commands ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [ContextMenu("Create Test Map")]
        private void CreateTestMap()
        {
            CreateNewMap("Test Map", 15, 10);
            
            // Add test spawn
            PlaceSpawnPoint(0, 5);
            
           // Add test base
            PlaceBase(14, 5);
            
            // Create simple path
            for (int x = 0; x < 15; x++)
            {
                SetTile(x, 5, TileType.Path);
            }

            SaveCurrentMap();
            Debug.Log("Created and saved test map");
        }

        [ContextMenu("Validate Current Map")]
        private void ValidateCurrentMap()
        {
            if (currentMap == null)
            {
                Debug.Log("No map loaded");
                return;
            }

            var result = ValidateMap(currentMap);
            
            Debug.Log($"Validation Result: {(result.isValid ? "VALID" : "INVALID")}");
            
            if (result.HasErrors)
            {
                Debug.Log("Errors:");
                foreach (var error in result.errors)
                    Debug.Log($"  - {error}");
            }
            
            if (result.HasWarnings)
            {
                Debug.Log("Warnings:");
                foreach (var warning in result.warnings)
                    Debug.Log($"  - {warning}");
            }
            
            if (result.HasSuggestions)
            {
                Debug.Log("Suggestions:");
                foreach (var suggestion in result.suggestions)
                    Debug.Log($"  - {suggestion}");
            }
        }
    }

    /// <summary>
    /// Editor mode enumeration.
    /// </summary>
    public enum EditorMode
    {
        Tile,           // Place/edit tiles
        Path,           // Draw enemy paths
        Spawn,          // Place spawn points
        Base,           // Place base/objective
        Obstacle,       // Place obstacles
        Decoration,     // Place decorations
        Wave,           // Configure waves
        Test            // Test play mode
    }
}
