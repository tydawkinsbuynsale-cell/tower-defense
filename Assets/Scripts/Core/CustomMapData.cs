using UnityEngine;
using System;
using System.Collections.Generic;

namespace RobotTD.Core
{
    /// <summary>
    /// Data structure for custom player-created maps.
    /// Serializable for save/load and network transfer.
    /// </summary>
    [Serializable]
    public class CustomMapData
    {
        // ── Metadata ──────────────────────────────────────────────────────────
        public string mapId;
        public string mapName;
        public string authorId;
        public string authorName;
        public string description;
        public int gridWidth = 20;
        public int gridHeight = 15;
        public DateTime createdDate;
        public DateTime lastModifiedDate;
        public int version = 1;
        
        // ── Map Configuration ─────────────────────────────────────────────────
        public int startingCredits = 500;
        public int startingLives = 20;
        public int recommendedDifficulty = 1; // 1-5 scale
        public float estimatedPlayTime = 15f; // minutes
        
        // ── Tile Data ─────────────────────────────────────────────────────────
        public TileType[,] tiles; // 2D grid of tile types
        public List<Vector2Int> pathTiles = new List<Vector2Int>();
        public List<SpawnPointData> spawnPoints = new List<SpawnPointData>();
        public Vector2Int basePosition;
        public List<ObstacleData> obstacles = new List<ObstacleData>();
        public List<DecorationData> decorations = new List<DecorationData>();
        
        // ── Wave Configuration ────────────────────────────────────────────────
        public List<CustomWaveData> waves = new List<CustomWaveData>();
        public int totalWaves = 10;
        
        // ── Community Features ────────────────────────────────────────────────
        public bool isPublished = false;
        public int playCount = 0;
        public int likeCount = 0;
        public float averageRating = 0f;
        public int ratingCount = 0;
        public List<string> tags = new List<string>();
        
        // ── Validation ────────────────────────────────────────────────────────
        public bool isValid = false;
        public List<string> validationErrors = new List<string>();

        public CustomMapData()
        {
            mapId = Guid.NewGuid().ToString();
            createdDate = DateTime.UtcNow;
            lastModifiedDate = DateTime.UtcNow;
            InitializeTiles();
        }

        public CustomMapData(int width, int height)
        {
            mapId = Guid.NewGuid().ToString();
            createdDate = DateTime.UtcNow;
            lastModifiedDate = DateTime.UtcNow;
            gridWidth = width;
            gridHeight = height;
            InitializeTiles();
        }

        private void InitializeTiles()
        {
            tiles = new TileType[gridWidth, gridHeight];
            
            // Initialize all tiles as buildable (grass)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    tiles[x, y] = TileType.Buildable;
                }
            }
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                tiles[x, y] = type;
                lastModifiedDate = DateTime.UtcNow;
            }
        }

        public TileType GetTile(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                return tiles[x, y];
            }
            return TileType.Invalid;
        }

        public bool IsPositionValid(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        public void AddSpawnPoint(Vector2Int position, int waveStart = 1)
        {
            spawnPoints.Add(new SpawnPointData
            {
                position = position,
                waveStart = waveStart,
                spawnDelay = 0f
            });
            lastModifiedDate = DateTime.UtcNow;
        }

        public void RemoveSpawnPoint(Vector2Int position)
        {
            spawnPoints.RemoveAll(sp => sp.position == position);
            lastModifiedDate = DateTime.UtcNow;
        }

        public void SetBasePosition(Vector2Int position)
        {
            basePosition = position;
            lastModifiedDate = DateTime.UtcNow;
        }

        public CustomMapData Clone()
        {
            var clone = new CustomMapData(gridWidth, gridHeight);
            clone.mapName = mapName + " (Copy)";
            clone.authorId = authorId;
            clone.authorName = authorName;
            clone.description = description;
            clone.startingCredits = startingCredits;
            clone.startingLives = startingLives;
            clone.recommendedDifficulty = recommendedDifficulty;
            
            // Deep copy tiles
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    clone.tiles[x, y] = tiles[x, y];
                }
            }
            
            clone.pathTiles = new List<Vector2Int>(pathTiles);
            clone.spawnPoints = new List<SpawnPointData>(spawnPoints);
            clone.basePosition = basePosition;
            clone.obstacles = new List<ObstacleData>(obstacles);
            clone.decorations = new List<DecorationData>(decorations);
            clone.waves = new List<CustomWaveData>(waves);
            
            return clone;
        }
    }

    /// <summary>
    /// Tile type enumeration for map grid.
    /// </summary>
    [Serializable]
    public enum TileType
    {
        Invalid = -1,
        Buildable = 0,      // Can place towers
        Path = 1,           // Enemy path (cannot build)
        Obstacle = 2,       // Blocked terrain (cannot build)
        Water = 3,          // Visual only (cannot build)
        SpawnPoint = 4,     // Enemy spawn location
        Base = 5,           // Player base location
        Decoration = 6      // Visual only (can build over)
    }

    /// <summary>
    /// Spawn point configuration data.
    /// </summary>
    [Serializable]
    public class SpawnPointData
    {
        public Vector2Int position;
        public int waveStart = 1;           // First wave this spawn is active
        public int waveEnd = 999;           // Last wave this spawn is active
        public float spawnDelay = 0f;       // Delay before first enemy spawns
        public string spawnerType = "default"; // Type of spawner visual
    }

    /// <summary>
    /// Obstacle placement data.
    /// </summary>
    [Serializable]
    public class ObstacleData
    {
        public Vector2Int position;
        public string obstacleType;         // rock, tree, building, etc.
        public float rotation = 0f;
        public Vector2 scale = Vector2.one;
    }

    /// <summary>
    /// Decoration placement data.
    /// </summary>
    [Serializable]
    public class DecorationData
    {
        public Vector2Int position;
        public string decorationType;       // grass, flowers, signs, etc.
        public float rotation = 0f;
        public Vector2 scale = Vector2.one;
    }

    /// <summary>
    /// Custom wave configuration for player-created maps.
    /// </summary>
    [Serializable]
    public class CustomWaveData
    {
        public int waveNumber;
        public List<EnemySpawnGroup> enemyGroups = new List<EnemySpawnGroup>();
        public float timeBetweenGroups = 2f;
        public int creditsReward = 100;
        public string bossType = ""; // Empty if no boss
    }

    /// <summary>
    /// Enemy spawn group within a wave.
    /// </summary>
    [Serializable]
    public class EnemySpawnGroup
    {
        public string enemyType;
        public int count = 5;
        public float spawnInterval = 0.5f;
        public int spawnPointIndex = 0; // Which spawn point to use
    }

    /// <summary>
    /// Serialization wrapper for saving/loading custom maps.
    /// </summary>
    [Serializable]
    public class CustomMapDataWrapper
    {
        public string json;
        public int version = 1;
    }

    /// <summary>
    /// Map validation result.
    /// </summary>
    public class MapValidationResult
    {
        public bool isValid;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> suggestions = new List<string>();

        public void AddError(string error) => errors.Add(error);
        public void AddWarning(string warning) => warnings.Add(warning);
        public void AddSuggestion(string suggestion) => suggestions.Add(suggestion);

        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;
        public bool HasSuggestions => suggestions.Count > 0;
    }
}
