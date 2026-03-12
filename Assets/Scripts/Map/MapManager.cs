using UnityEngine;
using System.Collections.Generic;

namespace RobotTD.Map
{
    /// <summary>
    /// Map data ScriptableObject for defining level layouts.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMap", menuName = "RobotTD/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Map Info")]
        public string mapName;
        [TextArea(2, 4)]
        public string description;
        public Sprite thumbnail;
        public int difficulty = 1; // 1-5
        public bool isUnlocked = true;
        public string nextMapId; // Name of the MapData asset to unlock after this map

        [Header("Waves")]
        public int totalWaves = 30;
        public float difficultyMultiplier = 1f; // Affects enemy stats

        [Header("Starting Resources")]
        public int startingCredits = 500;
        public int startingLives = 20;

        [Header("Path")]
        public Vector3[] pathPoints;

        [Header("Visuals")]
        public Material groundMaterial;
        public Material pathMaterial;
        public Color ambientColor = Color.white;
        public Color fogColor = Color.gray;
        public float fogDensity = 0.02f;

        [Header("Audio")]
        public AudioClip backgroundMusic;
        public AudioClip ambientSound;
    }

    /// <summary>
    /// Manages the current map, paths, and waypoints.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("Map Settings")]
        [SerializeField] private MapData currentMapData;
        [SerializeField] private Transform waypointParent;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform endPoint;

        [Header("Path Visuals")]
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private float pathWidth = 1.5f;
        [SerializeField] private Material pathMaterial;

        [Header("Grid")]
        [SerializeField] private float gridCellSize = 1f;
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 15;

        // Runtime
        private Transform[] waypoints;
        private bool[,] placementGrid; // true = can place tower

        public Transform[] Waypoints => waypoints;
        public Transform SpawnPoint => spawnPoint;
        public Transform EndPoint => endPoint;
        public float GridCellSize => gridCellSize;
        public string CurrentMapId => currentMapData != null ? currentMapData.name : string.Empty;
        public string NextMapId => currentMapData != null ? currentMapData.nextMapId : string.Empty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (currentMapData != null)
            {
                LoadMap(currentMapData);
            }
            else
            {
                SetupDefaultMap();
            }
        }

        /// <summary>
        /// Load a map from MapData
        /// </summary>
        public void LoadMap(MapData mapData)
        {
            currentMapData = mapData;

            // Apply visual settings
            RenderSettings.ambientLight = mapData.ambientColor;
            RenderSettings.fogColor = mapData.fogColor;
            RenderSettings.fogDensity = mapData.fogDensity;

            // Setup path from map data
            if (mapData.pathPoints != null && mapData.pathPoints.Length > 0)
            {
                CreateWaypointsFromData(mapData.pathPoints);
            }

            // Initialize placement grid
            InitializePlacementGrid();

            // Inform other systems
            Core.WaveManager.Instance?.SetPath(spawnPoint, waypoints);
            Core.GameManager.Instance?.InitializeGame();
        }

        private void SetupDefaultMap()
        {
            // Use existing waypoint children
            CollectWaypoints();
            InitializePlacementGrid();
            Core.WaveManager.Instance?.SetPath(spawnPoint, waypoints);
        }

        private void CollectWaypoints()
        {
            if (waypointParent != null)
            {
                List<Transform> points = new List<Transform>();
                foreach (Transform child in waypointParent)
                {
                    points.Add(child);
                }
                waypoints = points.ToArray();

                if (waypoints.Length > 0)
                {
                    spawnPoint = waypoints[0];
                    endPoint = waypoints[waypoints.Length - 1];
                }
            }
        }

        private void CreateWaypointsFromData(Vector3[] points)
        {
            // Clear existing waypoints
            if (waypointParent != null)
            {
                foreach (Transform child in waypointParent)
                {
                    Destroy(child.gameObject);
                }
            }

            // Create new waypoints
            List<Transform> newWaypoints = new List<Transform>();
            for (int i = 0; i < points.Length; i++)
            {
                GameObject wp = new GameObject($"Waypoint_{i}");
                wp.transform.SetParent(waypointParent);
                wp.transform.position = points[i];
                newWaypoints.Add(wp.transform);
            }

            waypoints = newWaypoints.ToArray();
            spawnPoint = waypoints[0];
            endPoint = waypoints[waypoints.Length - 1];

            // Draw path line
            DrawPathLine();
        }

        private void DrawPathLine()
        {
            if (pathLine == null || waypoints == null) return;

            pathLine.positionCount = waypoints.Length;
            pathLine.startWidth = pathWidth;
            pathLine.endWidth = pathWidth;

            if (pathMaterial != null || currentMapData?.pathMaterial != null)
            {
                pathLine.material = pathMaterial ?? currentMapData.pathMaterial;
            }

            for (int i = 0; i < waypoints.Length; i++)
            {
                pathLine.SetPosition(i, waypoints[i].position);
            }
        }

        #region Placement Grid

        private void InitializePlacementGrid()
        {
            placementGrid = new bool[gridWidth, gridHeight];

            // Initially all cells are placeable
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    placementGrid[x, y] = true;
                }
            }

            // Mark path cells as non-placeable
            MarkPathOnGrid();
        }

        private void MarkPathOnGrid()
        {
            if (waypoints == null) return;

            // Mark cells near path as non-placeable
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Vector3 start = waypoints[i].position;
                Vector3 end = waypoints[i + 1].position;

                // Sample points along the path segment
                float distance = Vector3.Distance(start, end);
                int samples = Mathf.CeilToInt(distance / (gridCellSize * 0.5f));

                for (int s = 0; s <= samples; s++)
                {
                    float t = s / (float)samples;
                    Vector3 point = Vector3.Lerp(start, end, t);
                    
                    // Mark this cell and adjacent cells
                    int gridX = Mathf.FloorToInt((point.x + (gridWidth * gridCellSize * 0.5f)) / gridCellSize);
                    int gridY = Mathf.FloorToInt((point.z + (gridHeight * gridCellSize * 0.5f)) / gridCellSize);

                    MarkCellUnplaceable(gridX, gridY);
                    MarkCellUnplaceable(gridX + 1, gridY);
                    MarkCellUnplaceable(gridX - 1, gridY);
                    MarkCellUnplaceable(gridX, gridY + 1);
                    MarkCellUnplaceable(gridX, gridY - 1);
                }
            }
        }

        private void MarkCellUnplaceable(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                placementGrid[x, y] = false;
            }
        }

        /// <summary>
        /// Check if a world position is valid for tower placement
        /// </summary>
        public bool CanPlaceAtPosition(Vector3 worldPos)
        {
            int gridX = Mathf.FloorToInt((worldPos.x + (gridWidth * gridCellSize * 0.5f)) / gridCellSize);
            int gridY = Mathf.FloorToInt((worldPos.z + (gridHeight * gridCellSize * 0.5f)) / gridCellSize);

            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                return placementGrid[gridX, gridY];
            }

            return false;
        }

        /// <summary>
        /// Mark a cell as occupied by a tower
        /// </summary>
        public void MarkCellOccupied(Vector3 worldPos)
        {
            int gridX = Mathf.FloorToInt((worldPos.x + (gridWidth * gridCellSize * 0.5f)) / gridCellSize);
            int gridY = Mathf.FloorToInt((worldPos.z + (gridHeight * gridCellSize * 0.5f)) / gridCellSize);

            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                placementGrid[gridX, gridY] = false;
            }
        }

        /// <summary>
        /// Free a cell when a tower is sold
        /// </summary>
        public void FreeCellAt(Vector3 worldPos)
        {
            int gridX = Mathf.FloorToInt((worldPos.x + (gridWidth * gridCellSize * 0.5f)) / gridCellSize);
            int gridY = Mathf.FloorToInt((worldPos.z + (gridHeight * gridCellSize * 0.5f)) / gridCellSize);

            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                // Check if it's not on the path before allowing placement again
                // TODO: Add path check
                placementGrid[gridX, gridY] = true;
            }
        }

        /// <summary>
        /// Get snapped grid position from world position
        /// </summary>
        public Vector3 GetGridPosition(Vector3 worldPos)
        {
            float x = Mathf.Round(worldPos.x / gridCellSize) * gridCellSize;
            float z = Mathf.Round(worldPos.z / gridCellSize) * gridCellSize;
            return new Vector3(x, 0, z);
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            // Draw waypoints
            if (waypoints != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < waypoints.Length - 1; i++)
                {
                    if (waypoints[i] != null && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }

                Gizmos.color = Color.green;
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                }

                Gizmos.color = Color.red;
                if (endPoint != null)
                {
                    Gizmos.DrawWireSphere(endPoint.position, 1f);
                }
            }

            // Draw placement grid
            if (placementGrid != null)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        Vector3 cellCenter = new Vector3(
                            (x - gridWidth * 0.5f) * gridCellSize + gridCellSize * 0.5f,
                            0.1f,
                            (y - gridHeight * 0.5f) * gridCellSize + gridCellSize * 0.5f
                        );

                        Gizmos.color = placementGrid[x, y] 
                            ? new Color(0, 1, 0, 0.2f) 
                            : new Color(1, 0, 0, 0.2f);
                        
                        Gizmos.DrawCube(cellCenter, new Vector3(gridCellSize * 0.9f, 0.1f, gridCellSize * 0.9f));
                    }
                }
            }
        }

        #endregion
    }
}
