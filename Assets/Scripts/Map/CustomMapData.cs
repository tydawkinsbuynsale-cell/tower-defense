using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure for player-created custom maps.
/// Stores all map configuration, tiles, spawn points, and gameplay settings.
/// </summary>
[Serializable]
public class CustomMapData
{
    #region Map Metadata
    public string mapId;
    public string mapName;
    public string description;
    public string authorName;
    public string authorId;
    public string creationDate;
    public string lastModifiedDate;
    public int version = 1; // For future compatibility
    #endregion

    #region Grid Configuration
    public int gridWidth = 20;
    public int gridHeight = 15;
    public TileType[,] grid;

    // Serialized version of grid for JSON
    public List<TileRow> gridData = new List<TileRow>();
    #endregion

    #region Map Elements
    public List<SpawnPointData> spawnPoints = new List<SpawnPointData>();
    public Vector2Int basePosition;
    public List<ObstacleData> obstacles = new List<ObstacleData>();
    public List<DecorationData> decorations = new List<DecorationData>();
    public List<Vector2Int> pathTiles = new List<Vector2Int>();
    #endregion

    #region Gameplay Settings
    public int startingCredits = 500;
    public int startingLives = 20;
    public int difficulty = 2; // 1=Tutorial, 2=Easy, 3=Normal, 4=Hard, 5=Expert
    public string estimatedPlayTime = "10-15 minutes";
    #endregion

    #region Wave Configuration
    public List<CustomWaveData> customWaves = new List<CustomWaveData>();
    #endregion

    #region Statistics
    public int playCount = 0;
    public float rating = 0f;
    public int likes = 0;
    public int downloads = 0;
    #endregion

    #region Constructors
    public CustomMapData()
    {
        mapId = Guid.NewGuid().ToString();
        creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        lastModifiedDate = creationDate;
    }

    public CustomMapData(int width, int height)
    {
        mapId = Guid.NewGuid().ToString();
        creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        lastModifiedDate = creationDate;
        gridWidth = width;
        gridHeight = height;
        InitializeGrid();
    }
    #endregion

    #region Grid Management
    /// <summary>
    /// Initializes the grid with buildable tiles.
    /// </summary>
    public void InitializeGrid()
    {
        grid = new TileType[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = TileType.Buildable;
            }
        }

        SerializeGrid();
    }

    /// <summary>
    /// Sets a tile type at the specified position.
    /// </summary>
    public void SetTile(int x, int y, TileType tileType)
    {
        if (IsValidGridPosition(x, y))
        {
            grid[x, y] = tileType;
        }
    }

    /// <summary>
    /// Gets the tile type at the specified position.
    /// </summary>
    public TileType GetTile(int x, int y)
    {
        if (IsValidGridPosition(x, y))
        {
            return grid[x, y];
        }
        return TileType.Buildable;
    }

    /// <summary>
    /// Checks if a grid position is valid.
    /// </summary>
    public bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    /// <summary>
    /// Serializes the 2D grid array for JSON storage.
    /// </summary>
    public void SerializeGrid()
    {
        gridData.Clear();

        for (int y = 0; y < gridHeight; y++)
        {
            TileRow row = new TileRow();
            row.tiles = new List<int>();

            for (int x = 0; x < gridWidth; x++)
            {
                row.tiles.Add((int)grid[x, y]);
            }

            gridData.Add(row);
        }
    }

    /// <summary>
    /// Deserializes the grid data from JSON storage.
    /// </summary>
    public void DeserializeGrid()
    {
        if (gridData == null || gridData.Count == 0)
        {
            InitializeGrid();
            return;
        }

        gridHeight = gridData.Count;
        gridWidth = gridData[0].tiles.Count;

        grid = new TileType[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth && x < gridData[y].tiles.Count; x++)
            {
                grid[x, y] = (TileType)gridData[y].tiles[x];
            }
        }
    }
    #endregion

    #region Spawn Points
    /// <summary>
    /// Adds a spawn point to the map.
    /// </summary>
    public void AddSpawnPoint(Vector2Int position)
    {
        if (!HasSpawnPointAt(position))
        {
            SpawnPointData spawnPoint = new SpawnPointData
            {
                position = position,
                spawnDelay = 1.0f
            };
            spawnPoints.Add(spawnPoint);
        }
    }

    /// <summary>
    /// Removes a spawn point from the map.
    /// </summary>
    public void RemoveSpawnPoint(Vector2Int position)
    {
        spawnPoints.RemoveAll(sp => sp.position == position);
    }

    /// <summary>
    /// Checks if there's a spawn point at the specified position.
    /// </summary>
    public bool HasSpawnPointAt(Vector2Int position)
    {
        return spawnPoints.Exists(sp => sp.position == position);
    }
    #endregion

    #region Validation
    /// <summary>
    /// Validates the map configuration.
    /// </summary>
    public MapValidationResult Validate()
    {
        MapValidationResult result = new MapValidationResult();

        // Check map name
        if (string.IsNullOrEmpty(mapName) || mapName.Length < 3)
        {
            result.errors.Add("Map must have a name (minimum 3 characters)");
        }

        // Check for spawn points
        if (spawnPoints.Count == 0)
        {
            result.errors.Add("Map must have at least one spawn point");
        }

        // Check for base position
        if (basePosition == Vector2Int.zero && !IsValidGridPosition(basePosition.x, basePosition.y))
        {
            result.errors.Add("Map must have a base position");
        }

        // Check path connectivity (basic check - at least some path tiles exist)
        int pathTileCount = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == TileType.Path)
                {
                    pathTileCount++;
                }
            }
        }

        if (pathTileCount == 0)
        {
            result.errors.Add("Map must have a path from spawn to base");
        }

        // Warnings
        if (customWaves.Count == 0)
        {
            result.warnings.Add("No wave configuration defined. Default waves will be used.");
        }

        // Count buildable tiles
        int buildableTiles = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == TileType.Buildable)
                {
                    buildableTiles++;
                }
            }
        }

        if (buildableTiles < 20)
        {
            result.warnings.Add($"Limited buildable space ({buildableTiles} tiles). Consider adding more.");
        }

        // Suggestions
        if (startingCredits < 300)
        {
            result.suggestions.Add("Low starting credits. Players may struggle early game.");
        }

        if (customWaves.Count > 0 && customWaves.Count < 10)
        {
            result.suggestions.Add("Short map. Consider adding more waves for better gameplay.");
        }

        result.isValid = result.errors.Count == 0;
        return result;
    }
    #endregion

    #region Cloning
    /// <summary>
    /// Creates a deep copy of this map data.
    /// </summary>
    public CustomMapData Clone()
    {
        CustomMapData clone = new CustomMapData(gridWidth, gridHeight);
        clone.mapId = this.mapId;
        clone.mapName = this.mapName;
        clone.description = this.description;
        clone.authorName = this.authorName;
        clone.authorId = this.authorId;
        clone.creationDate = this.creationDate;
        clone.lastModifiedDate = this.lastModifiedDate;
        clone.version = this.version;

        // Clone grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                clone.grid[x, y] = this.grid[x, y];
            }
        }
        clone.SerializeGrid();

        // Clone spawn points
        clone.spawnPoints = new List<SpawnPointData>();
        foreach (var sp in spawnPoints)
        {
            clone.spawnPoints.Add(new SpawnPointData
            {
                position = sp.position,
                spawnDelay = sp.spawnDelay
            });
        }

        clone.basePosition = this.basePosition;

        // Clone obstacles
        clone.obstacles = new List<ObstacleData>();
        foreach (var obs in obstacles)
        {
            clone.obstacles.Add(new ObstacleData
            {
                position = obs.position,
                obstacleType = obs.obstacleType
            });
        }

        // Clone decorations
        clone.decorations = new List<DecorationData>();
        foreach (var dec in decorations)
        {
            clone.decorations.Add(new DecorationData
            {
                position = dec.position,
                decorationType = dec.decorationType
            });
        }

        // Clone path tiles
        clone.pathTiles = new List<Vector2Int>(pathTiles);

        // Clone settings
        clone.startingCredits = this.startingCredits;
        clone.startingLives = this.startingLives;
        clone.difficulty = this.difficulty;
        clone.estimatedPlayTime = this.estimatedPlayTime;

        // Clone waves
        clone.customWaves = new List<CustomWaveData>();
        foreach (var wave in customWaves)
        {
            clone.customWaves.Add(wave.Clone());
        }

        // Clone statistics
        clone.playCount = this.playCount;
        clone.rating = this.rating;
        clone.likes = this.likes;
        clone.downloads = this.downloads;

        return clone;
    }
    #endregion
}

#region Supporting Data Structures
/// <summary>
/// Types of tiles in the custom map editor.
/// </summary>
public enum TileType
{
    Buildable = 0,    // Towers can be placed
    Path = 1,          // Enemy route
    Obstacle = 2,      // Blocked terrain
    Water = 3,         // Visual decoration
    SpawnPoint = 4,    // Enemy spawner
    Base = 5,          // Player objective
    Decoration = 6     // Visual only
}

/// <summary>
/// Represents a row of tiles for JSON serialization.
/// </summary>
[Serializable]
public class TileRow
{
    public List<int> tiles = new List<int>();
}

/// <summary>
/// Data for spawn points.
/// </summary>
[Serializable]
public class SpawnPointData
{
    public Vector2Int position;
    public float spawnDelay = 1.0f;
    public List<int> waveIndices = new List<int>(); // Which waves use this spawn
}

/// <summary>
/// Data for obstacles.
/// </summary>
[Serializable]
public class ObstacleData
{
    public Vector2Int position;
    public int obstacleType = 0; // Different visual variations
}

/// <summary>
/// Data for decorative elements.
/// </summary>
[Serializable]
public class DecorationData
{
    public Vector2Int position;
    public int decorationType = 0; // Different visual variations
}

/// <summary>
/// Custom wave configuration for map.
/// </summary>
[Serializable]
public class CustomWaveData
{
    public int waveNumber;
    public List<EnemySpawnGroup> enemyGroups = new List<EnemySpawnGroup>();
    public float timeBetweenGroups = 2f;
    public int creditsReward = 50;

    public CustomWaveData Clone()
    {
        CustomWaveData clone = new CustomWaveData
        {
            waveNumber = this.waveNumber,
            timeBetweenGroups = this.timeBetweenGroups,
            creditsReward = this.creditsReward,
            enemyGroups = new List<EnemySpawnGroup>()
        };

        foreach (var group in enemyGroups)
        {
            clone.enemyGroups.Add(new EnemySpawnGroup
            {
                enemyTypeId = group.enemyTypeId,
                count = group.count,
                spawnInterval = group.spawnInterval,
                spawnPointIndex = group.spawnPointIndex
            });
        }

        return clone;
    }
}

/// <summary>
/// Group of enemies to spawn in a wave.
/// </summary>
[Serializable]
public class EnemySpawnGroup
{
    public int enemyTypeId;
    public int count;
    public float spawnInterval = 0.5f;
    public int spawnPointIndex = 0;
}

/// <summary>
/// Result of map validation.
/// </summary>
[Serializable]
public class MapValidationResult
{
    public bool isValid = true;
    public List<string> errors = new List<string>();
    public List<string> warnings = new List<string>();
    public List<string> suggestions = new List<string>();
}
#endregion
