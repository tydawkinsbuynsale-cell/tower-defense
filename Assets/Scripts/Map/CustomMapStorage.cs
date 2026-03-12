using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages local file system storage for custom maps.
/// Handles save/load operations, map library management, and thumbnail generation.
/// </summary>
public class CustomMapStorage : MonoBehaviour
{
    #region Singleton
    public static CustomMapStorage Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStorage();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Storage Configuration
    [Header("Storage Settings")]
    [SerializeField] private string customMapsFolder = "CustomMaps";
    [SerializeField] private string thumbnailsFolder = "Thumbnails";
    [SerializeField] private int maxLocalMaps = 100;
    [SerializeField] private bool enableAutoBackup = true;
    [SerializeField] private int maxBackups = 5;

    private string MapsDirectory => Path.Combine(Application.persistentDataPath, customMapsFolder);
    private string ThumbnailsDirectory => Path.Combine(Application.persistentDataPath, thumbnailsFolder);
    private string BackupsDirectory => Path.Combine(Application.persistentDataPath, customMapsFolder, "Backups");
    #endregion

    #region Events
    public static event Action<CustomMapData> OnMapSaved;
    public static event Action<CustomMapData> OnMapLoaded;
    public static event Action<string> OnMapDeleted;
    public static event Action OnLibraryRefreshed;
    public static event Action<string> OnStorageError;
    #endregion

    #region Private Fields
    private Dictionary<string, CustomMapMetadata> mapLibrary = new Dictionary<string, CustomMapMetadata>();
    private bool isInitialized = false;
    #endregion

    #region Initialization
    private void InitializeStorage()
    {
        try
        {
            // Create directories if they don't exist
            if (!Directory.Exists(MapsDirectory))
            {
                Directory.CreateDirectory(MapsDirectory);
                Debug.Log($"Created custom maps directory: {MapsDirectory}");
            }

            if (!Directory.Exists(ThumbnailsDirectory))
            {
                Directory.CreateDirectory(ThumbnailsDirectory);
                Debug.Log($"Created thumbnails directory: {ThumbnailsDirectory}");
            }

            if (enableAutoBackup && !Directory.Exists(BackupsDirectory))
            {
                Directory.CreateDirectory(BackupsDirectory);
                Debug.Log($"Created backups directory: {BackupsDirectory}");
            }

            // Load map library
            RefreshMapLibrary();

            isInitialized = true;
            Debug.Log($"CustomMapStorage initialized. Found {mapLibrary.Count} local maps.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize CustomMapStorage: {ex.Message}");
            OnStorageError?.Invoke($"Storage initialization failed: {ex.Message}");
        }
    }
    #endregion

    #region Save Operations
    /// <summary>
    /// Saves a custom map to the local file system.
    /// </summary>
    public bool SaveMap(CustomMapData mapData, bool createBackup = true)
    {
        if (!isInitialized)
        {
            Debug.LogError("CustomMapStorage not initialized!");
            return false;
        }

        if (mapData == null)
        {
            Debug.LogError("Cannot save null map data!");
            return false;
        }

        // Validate map has required data
        if (string.IsNullOrEmpty(mapData.mapId))
        {
            mapData.mapId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(mapData.mapName))
        {
            Debug.LogError("Cannot save map without a name!");
            OnStorageError?.Invoke("Map must have a name");
            return false;
        }

        try
        {
            // Check if we're at the limit for local maps
            if (!mapLibrary.ContainsKey(mapData.mapId) && mapLibrary.Count >= maxLocalMaps)
            {
                Debug.LogWarning($"Maximum local maps limit reached ({maxLocalMaps}). Cannot save new map.");
                OnStorageError?.Invoke($"Storage limit reached ({maxLocalMaps} maps). Delete some maps first.");
                return false;
            }

            // Create backup if requested and map already exists
            if (createBackup && enableAutoBackup && MapExists(mapData.mapId))
            {
                CreateBackup(mapData.mapId);
            }

            // Update metadata
            mapData.lastModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrEmpty(mapData.creationDate))
            {
                mapData.creationDate = mapData.lastModifiedDate;
            }

            // Serialize to JSON
            string json = JsonUtility.ToJson(mapData, true);
            string filePath = GetMapFilePath(mapData.mapId);

            // Write to file
            File.WriteAllText(filePath, json);

            // Update library
            UpdateMapMetadata(mapData);

            Debug.Log($"Map saved successfully: {mapData.mapName} ({mapData.mapId})");
            OnMapSaved?.Invoke(mapData);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save map: {ex.Message}");
            OnStorageError?.Invoke($"Save failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves a map with a new ID (Save As functionality).
    /// </summary>
    public CustomMapData SaveMapAs(CustomMapData originalMap, string newName)
    {
        if (originalMap == null) return null;

        // Clone the map data
        CustomMapData newMap = originalMap.Clone();
        newMap.mapId = Guid.NewGuid().ToString();
        newMap.mapName = newName;
        newMap.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newMap.lastModifiedDate = newMap.creationDate;
        newMap.playCount = 0;
        newMap.rating = 0f;
        newMap.likes = 0;

        if (SaveMap(newMap, false))
        {
            return newMap;
        }

        return null;
    }
    #endregion

    #region Load Operations
    /// <summary>
    /// Loads a custom map from the local file system.
    /// </summary>
    public CustomMapData LoadMap(string mapId)
    {
        if (!isInitialized)
        {
            Debug.LogError("CustomMapStorage not initialized!");
            return null;
        }

        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError("Cannot load map with empty ID!");
            return null;
        }

        try
        {
            string filePath = GetMapFilePath(mapId);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Map file not found: {mapId}");
                OnStorageError?.Invoke("Map file not found");
                return null;
            }

            // Read JSON
            string json = File.ReadAllText(filePath);

            // Deserialize
            CustomMapData mapData = JsonUtility.FromJson<CustomMapData>(json);

            if (mapData == null)
            {
                Debug.LogError($"Failed to deserialize map data: {mapId}");
                OnStorageError?.Invoke("Failed to load map data");
                return null;
            }

            Debug.Log($"Map loaded successfully: {mapData.mapName} ({mapId})");
            OnMapLoaded?.Invoke(mapData);

            return mapData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load map {mapId}: {ex.Message}");
            OnStorageError?.Invoke($"Load failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all maps in the local library.
    /// </summary>
    public List<CustomMapMetadata> GetAllMaps()
    {
        return mapLibrary.Values.OrderByDescending(m => m.lastModifiedDate).ToList();
    }

    /// <summary>
    /// Searches maps by name.
    /// </summary>
    public List<CustomMapMetadata> SearchMaps(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return GetAllMaps();
        }

        return mapLibrary.Values
            .Where(m => m.mapName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                       m.authorName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(m => m.lastModifiedDate)
            .ToList();
    }

    /// <summary>
    /// Gets maps filtered by difficulty.
    /// </summary>
    public List<CustomMapMetadata> GetMapsByDifficulty(int difficulty)
    {
        return mapLibrary.Values
            .Where(m => m.difficulty == difficulty)
            .OrderByDescending(m => m.lastModifiedDate)
            .ToList();
    }

    /// <summary>
    /// Gets maps sorted by play count.
    /// </summary>
    public List<CustomMapMetadata> GetMostPlayedMaps(int count = 10)
    {
        return mapLibrary.Values
            .OrderByDescending(m => m.playCount)
            .Take(count)
            .ToList();
    }
    #endregion

    #region Delete Operations
    /// <summary>
    /// Deletes a custom map from local storage.
    /// </summary>
    public bool DeleteMap(string mapId, bool createBackup = true)
    {
        if (!isInitialized)
        {
            Debug.LogError("CustomMapStorage not initialized!");
            return false;
        }

        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError("Cannot delete map with empty ID!");
            return false;
        }

        try
        {
            // Create backup before deletion if requested
            if (createBackup && enableAutoBackup && MapExists(mapId))
            {
                CreateBackup(mapId);
            }

            string filePath = GetMapFilePath(mapId);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Delete thumbnail if exists
            string thumbnailPath = GetThumbnailPath(mapId);
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }

            // Remove from library
            if (mapLibrary.ContainsKey(mapId))
            {
                mapLibrary.Remove(mapId);
            }

            Debug.Log($"Map deleted: {mapId}");
            OnMapDeleted?.Invoke(mapId);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete map {mapId}: {ex.Message}");
            OnStorageError?.Invoke($"Delete failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes all custom maps from local storage.
    /// </summary>
    public bool DeleteAllMaps(bool createBackup = true)
    {
        if (!isInitialized) return false;

        try
        {
            List<string> mapIds = new List<string>(mapLibrary.Keys);

            foreach (string mapId in mapIds)
            {
                DeleteMap(mapId, createBackup);
            }

            Debug.Log($"Deleted all custom maps ({mapIds.Count} maps)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete all maps: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Backup Operations
    /// <summary>
    /// Creates a backup of a map file.
    /// </summary>
    private void CreateBackup(string mapId)
    {
        try
        {
            string sourceFile = GetMapFilePath(mapId);
            if (!File.Exists(sourceFile)) return;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFile = Path.Combine(BackupsDirectory, $"{mapId}_{timestamp}.json");

            File.Copy(sourceFile, backupFile, true);

            // Clean up old backups
            CleanupOldBackups(mapId);

            Debug.Log($"Backup created: {backupFile}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create backup for {mapId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up old backup files, keeping only the most recent ones.
    /// </summary>
    private void CleanupOldBackups(string mapId)
    {
        try
        {
            if (!Directory.Exists(BackupsDirectory)) return;

            var backupFiles = Directory.GetFiles(BackupsDirectory, $"{mapId}_*.json")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            // Delete old backups beyond the limit
            for (int i = maxBackups; i < backupFiles.Count; i++)
            {
                backupFiles[i].Delete();
                Debug.Log($"Deleted old backup: {backupFiles[i].Name}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to cleanup old backups: {ex.Message}");
        }
    }

    /// <summary>
    /// Restores a map from backup.
    /// </summary>
    public bool RestoreFromBackup(string mapId, DateTime backupDate)
    {
        try
        {
            string timestamp = backupDate.ToString("yyyyMMdd_HHmmss");
            string backupFile = Path.Combine(BackupsDirectory, $"{mapId}_{timestamp}.json");

            if (!File.Exists(backupFile))
            {
                Debug.LogError($"Backup file not found: {backupFile}");
                return false;
            }

            string targetFile = GetMapFilePath(mapId);
            File.Copy(backupFile, targetFile, true);

            Debug.Log($"Restored map from backup: {mapId}");

            // Refresh library
            RefreshMapLibrary();

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to restore from backup: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets all available backups for a map.
    /// </summary>
    public List<DateTime> GetAvailableBackups(string mapId)
    {
        List<DateTime> backups = new List<DateTime>();

        try
        {
            if (!Directory.Exists(BackupsDirectory)) return backups;

            var backupFiles = Directory.GetFiles(BackupsDirectory, $"{mapId}_*.json");

            foreach (string file in backupFiles)
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                string[] parts = filename.Split('_');

                if (parts.Length >= 3)
                {
                    string dateStr = parts[parts.Length - 2];
                    string timeStr = parts[parts.Length - 1];

                    if (DateTime.TryParseExact(
                        $"{dateStr}_{timeStr}",
                        "yyyyMMdd_HHmmss",
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime backupDate))
                    {
                        backups.Add(backupDate);
                    }
                }
            }

            return backups.OrderByDescending(d => d).ToList();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get available backups: {ex.Message}");
            return backups;
        }
    }
    #endregion

    #region Library Management
    /// <summary>
    /// Refreshes the map library from the file system.
    /// </summary>
    public void RefreshMapLibrary()
    {
        if (!isInitialized && !Directory.Exists(MapsDirectory))
        {
            return;
        }

        try
        {
            mapLibrary.Clear();

            string[] mapFiles = Directory.GetFiles(MapsDirectory, "*.json");

            foreach (string file in mapFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    CustomMapData mapData = JsonUtility.FromJson<CustomMapData>(json);

                    if (mapData != null && !string.IsNullOrEmpty(mapData.mapId))
                    {
                        UpdateMapMetadata(mapData);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load map file {file}: {ex.Message}");
                }
            }

            Debug.Log($"Map library refreshed. Found {mapLibrary.Count} maps.");
            OnLibraryRefreshed?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to refresh map library: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates or adds map metadata to the library.
    /// </summary>
    private void UpdateMapMetadata(CustomMapData mapData)
    {
        CustomMapMetadata metadata = new CustomMapMetadata
        {
            mapId = mapData.mapId,
            mapName = mapData.mapName,
            authorName = mapData.authorName,
            authorId = mapData.authorId,
            difficulty = mapData.difficulty,
            gridWidth = mapData.gridWidth,
            gridHeight = mapData.gridHeight,
            creationDate = mapData.creationDate,
            lastModifiedDate = mapData.lastModifiedDate,
            playCount = mapData.playCount,
            rating = mapData.rating,
            likes = mapData.likes,
            fileSize = GetMapFileSize(mapData.mapId),
            hasThumbnail = ThumbnailExists(mapData.mapId)
        };

        if (mapLibrary.ContainsKey(mapData.mapId))
        {
            mapLibrary[mapData.mapId] = metadata;
        }
        else
        {
            mapLibrary.Add(mapData.mapId, metadata);
        }
    }

    /// <summary>
    /// Checks if a map exists in local storage.
    /// </summary>
    public bool MapExists(string mapId)
    {
        return !string.IsNullOrEmpty(mapId) && mapLibrary.ContainsKey(mapId);
    }

    /// <summary>
    /// Gets the total number of maps in local storage.
    /// </summary>
    public int GetMapCount()
    {
        return mapLibrary.Count;
    }

    /// <summary>
    /// Gets the total storage size used by custom maps.
    /// </summary>
    public long GetTotalStorageSize()
    {
        long totalSize = 0;

        try
        {
            if (Directory.Exists(MapsDirectory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(MapsDirectory);
                totalSize += dirInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly)
                    .Sum(f => f.Length);
            }

            if (Directory.Exists(ThumbnailsDirectory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(ThumbnailsDirectory);
                totalSize += dirInfo.GetFiles("*.png", SearchOption.AllDirectories)
                    .Sum(f => f.Length);
            }

            return totalSize;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to calculate storage size: {ex.Message}");
            return 0;
        }
    }
    #endregion

    #region Thumbnail Operations
    /// <summary>
    /// Saves a thumbnail for a custom map.
    /// </summary>
    public bool SaveThumbnail(string mapId, Texture2D thumbnail)
    {
        if (thumbnail == null || string.IsNullOrEmpty(mapId))
        {
            return false;
        }

        try
        {
            byte[] thumbnailData = thumbnail.EncodeToPNG();
            string thumbnailPath = GetThumbnailPath(mapId);

            File.WriteAllBytes(thumbnailPath, thumbnailData);

            Debug.Log($"Thumbnail saved for map: {mapId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save thumbnail: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads a thumbnail for a custom map.
    /// </summary>
    public Texture2D LoadThumbnail(string mapId)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            return null;
        }

        try
        {
            string thumbnailPath = GetThumbnailPath(mapId);

            if (!File.Exists(thumbnailPath))
            {
                return null;
            }

            byte[] thumbnailData = File.ReadAllBytes(thumbnailPath);
            Texture2D thumbnail = new Texture2D(2, 2);

            if (thumbnail.LoadImage(thumbnailData))
            {
                return thumbnail;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load thumbnail: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a thumbnail exists for a map.
    /// </summary>
    public bool ThumbnailExists(string mapId)
    {
        return !string.IsNullOrEmpty(mapId) && File.Exists(GetThumbnailPath(mapId));
    }

    /// <summary>
    /// Deletes a thumbnail for a map.
    /// </summary>
    public bool DeleteThumbnail(string mapId)
    {
        try
        {
            string thumbnailPath = GetThumbnailPath(mapId);

            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete thumbnail: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Helper Methods
    private string GetMapFilePath(string mapId)
    {
        return Path.Combine(MapsDirectory, $"{mapId}.json");
    }

    private string GetThumbnailPath(string mapId)
    {
        return Path.Combine(ThumbnailsDirectory, $"{mapId}.png");
    }

    private long GetMapFileSize(string mapId)
    {
        try
        {
            string filePath = GetMapFilePath(mapId);
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
        }
        catch { }

        return 0;
    }
    #endregion

    #region Import/Export
    /// <summary>
    /// Exports a map to a shareable file.
    /// </summary>
    public bool ExportMap(string mapId, string exportPath)
    {
        try
        {
            string sourceFile = GetMapFilePath(mapId);

            if (!File.Exists(sourceFile))
            {
                Debug.LogError($"Map file not found: {mapId}");
                return false;
            }

            File.Copy(sourceFile, exportPath, true);

            Debug.Log($"Map exported to: {exportPath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to export map: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Imports a map from an external file.
    /// </summary>
    public CustomMapData ImportMap(string importPath)
    {
        try
        {
            if (!File.Exists(importPath))
            {
                Debug.LogError($"Import file not found: {importPath}");
                return null;
            }

            string json = File.ReadAllText(importPath);
            CustomMapData mapData = JsonUtility.FromJson<CustomMapData>(json);

            if (mapData == null)
            {
                Debug.LogError("Failed to parse imported map data");
                return null;
            }

            // Generate new ID to avoid conflicts
            mapData.mapId = Guid.NewGuid().ToString();
            mapData.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            mapData.lastModifiedDate = mapData.creationDate;

            // Save the imported map
            if (SaveMap(mapData, false))
            {
                Debug.Log($"Map imported successfully: {mapData.mapName}");
                return mapData;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to import map: {ex.Message}");
            return null;
        }
    }
    #endregion
}

/// <summary>
/// Lightweight metadata for map library browsing.
/// </summary>
[Serializable]
public class CustomMapMetadata
{
    public string mapId;
    public string mapName;
    public string authorName;
    public string authorId;
    public int difficulty;
    public int gridWidth;
    public int gridHeight;
    public string creationDate;
    public string lastModifiedDate;
    public int playCount;
    public float rating;
    public int likes;
    public long fileSize;
    public bool hasThumbnail;
}
