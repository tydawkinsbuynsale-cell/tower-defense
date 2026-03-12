using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Analytics;

namespace RobotTD.Online
{
    /// <summary>
    /// Community map sharing system for publishing and downloading custom maps.
    /// Handles map uploads, downloads, ratings, search, and play count tracking.
    /// Integrates with AuthenticationManager for user identification.
    /// </summary>
    public class MapSharingManager : MonoBehaviour
    {
        public static MapSharingManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableMapSharing = true;
        [SerializeField] private int maxMapsPerUser = 50;
        [SerializeField] private int communityBrowserPageSize = 20;
        [SerializeField] private bool verboseLogging = true;

        [Header("Cloud Backend")]
        [SerializeField] private string cloudEndpoint = "https://api.robottd.example.com/maps";
        [SerializeField] private float requestTimeout = 30f;

        // State
        private bool isInitialized = false;
        private List<CommunityMapMetadata> cachedCommunityMaps = new List<CommunityMapMetadata>();
        private Dictionary<string, int> userRatings = new Dictionary<string, int>(); // mapId -> rating (1-5)
        private HashSet<string> userLikes = new HashSet<string>(); // liked mapIds
        private DateTime lastBrowserRefresh = DateTime.MinValue;
        private float browserCacheTimeout = 300f; // 5 minutes

        // Events
        public event Action<CommunityMapMetadata> OnMapPublished;
        public event Action<CustomMapData> OnMapDownloaded;
        public event Action<List<CommunityMapMetadata>> OnCommunityMapsLoaded;
        public event Action<string> OnOperationError;
        public event Action<string, int> OnMapRated; // mapId, rating
        public event Action<string, bool> OnMapLiked; // mapId, isLiked

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

        private void Start()
        {
            if (!enableMapSharing)
            {
                LogDebug("Map sharing disabled");
                return;
            }

            StartCoroutine(InitializeMapSharing());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeMapSharing()
        {
            LogDebug("Initializing map sharing system...");

            // Wait for AuthenticationManager
            var authManager = AuthenticationManager.Instance;
            if (authManager != null)
            {
                float timeout = 10f;
                float elapsed = 0f;
                while (!authManager.IsInitialized && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }

                if (!authManager.IsAuthenticated)
                {
                    LogDebug("Not authenticated - map sharing requires login");
                    OnOperationError?.Invoke("Map sharing requires authentication. Please sign in.");
                }
                else
                {
                    LogDebug($"Map sharing ready for user: {authManager.PlayerName}");
                }
            }
            else
            {
                LogDebug("AuthenticationManager not found - map sharing unavailable");
            }

            // Load user's rating data from local storage
            LoadUserRatingsFromLocal();
            LoadUserLikesFromLocal();

            isInitialized = true;
            LogDebug("Map sharing initialized");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_sharing_initialized", new Dictionary<string, object>
                {
                    { "is_authenticated", authManager != null && authManager.IsAuthenticated }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Publishing Maps ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Publishes a custom map to the community.
        /// Uploads map data, thumbnail, and metadata to cloud.
        /// </summary>
        public void PublishMap(CustomMapData mapData, string[] tags = null, Action<bool> callback = null)
        {
            if (!CanPublishMaps())
            {
                OnOperationError?.Invoke("Cannot publish map. Check authentication and initialization.");
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(PublishMapCoroutine(mapData, tags, callback));
        }

        private IEnumerator PublishMapCoroutine(CustomMapData mapData, string[] tags, Action<bool> callback)
        {
            LogDebug($"Publishing map: {mapData.mapName}");

            var authManager = AuthenticationManager.Instance;
            if (authManager == null || !authManager.IsAuthenticated)
            {
                OnOperationError?.Invoke("Authentication required to publish maps");
                callback?.Invoke(false);
                yield break;
            }

            // Create metadata
            var metadata = new CommunityMapMetadata
            {
                mapId = mapData.mapId,
                mapName = mapData.mapName,
                authorId = authManager.PlayerId,
                authorName = authManager.PlayerName,
                description = mapData.description,
                difficulty = CalculateMapDifficulty(mapData),
                gridWidth = mapData.gridWidth,
                gridHeight = mapData.gridHeight,
                waveCount = mapData.customWaves != null ? mapData.customWaves.Count : 0,
                tags = tags ?? new string[0],
                publishedDate = DateTime.UtcNow,
                lastUpdatedDate = DateTime.UtcNow,
                playCount = 0,
                likeCount = 0,
                averageRating = 0f,
                ratingCount = 0
            };

            // Load thumbnail data
            byte[] thumbnailData = LoadThumbnailData(mapData.mapId);

            // Simulate cloud upload (replace with actual backend call)
            #if UNITY_EDITOR
            yield return SimulateCloudUpload(mapData, metadata, thumbnailData);
            #else
            yield return UploadToCloud(mapData, metadata, thumbnailData);
            #endif

            // Success
            OnMapPublished?.Invoke(metadata);
            callback?.Invoke(true);

            LogDebug($"Map published successfully: {mapData.mapName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_published", new Dictionary<string, object>
                {
                    { "map_id", mapData.mapId },
                    { "map_name", mapData.mapName },
                    { "author_id", metadata.authorId },
                    { "difficulty", metadata.difficulty },
                    { "wave_count", metadata.waveCount },
                    { "has_thumbnail", thumbnailData != null && thumbnailData.Length > 0 }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Downloading Maps ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Downloads a community map by ID.
        /// Saves to local storage after download.
        /// </summary>
        public void DownloadMap(string mapId, Action<CustomMapData> callback = null)
        {
            if (!isInitialized)
            {
                OnOperationError?.Invoke("Map sharing not initialized");
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(DownloadMapCoroutine(mapId, callback));
        }

        private IEnumerator DownloadMapCoroutine(string mapId, Action<CustomMapData> callback)
        {
            LogDebug($"Downloading map: {mapId}");

            // Simulate cloud download (replace with actual backend call)
            #if UNITY_EDITOR
            yield return SimulateCloudDownload(mapId, (mapData, thumbnailData) =>
            {
                if (mapData != null)
                {
                    // Save to local storage
                    SaveDownloadedMap(mapData, thumbnailData);

                    // Increment play count
                    IncrementPlayCount(mapId);

                    OnMapDownloaded?.Invoke(mapData);
                    callback?.Invoke(mapData);

                    LogDebug($"Map downloaded successfully: {mapData.mapName}");

                    // Track analytics
                    if (AnalyticsManager.Instance != null)
                    {
                        AnalyticsManager.Instance.TrackEvent("map_downloaded", new Dictionary<string, object>
                        {
                            { "map_id", mapId },
                            { "map_name", mapData.mapName }
                        });
                    }
                }
                else
                {
                    OnOperationError?.Invoke($"Failed to download map: {mapId}");
                    callback?.Invoke(null);
                }
            });
            #else
            yield return DownloadFromCloud(mapId, callback);
            #endif
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Community Browser ─────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Loads community maps with optional filtering.
        /// Results are cached for performance.
        /// </summary>
        public void LoadCommunityMaps(MapSearchFilter filter = null, Action<List<CommunityMapMetadata>> callback = null)
        {
            if (!isInitialized)
            {
                OnOperationError?.Invoke("Map sharing not initialized");
                callback?.Invoke(new List<CommunityMapMetadata>());
                return;
            }

            StartCoroutine(LoadCommunityMapsCoroutine(filter, callback));
        }

        private IEnumerator LoadCommunityMapsCoroutine(MapSearchFilter filter, Action<List<CommunityMapMetadata>> callback)
        {
            LogDebug("Loading community maps...");

            // Check cache
            bool useCache = (DateTime.UtcNow - lastBrowserRefresh).TotalSeconds < browserCacheTimeout;
            if (useCache && cachedCommunityMaps.Count > 0 && filter == null)
            {
                LogDebug($"Using cached community maps ({cachedCommunityMaps.Count} maps)");
                callback?.Invoke(cachedCommunityMaps);
                yield break;
            }

            // Simulate cloud fetch (replace with actual backend call)
            #if UNITY_EDITOR
            yield return SimulateLoadCommunityMaps((maps) =>
            {
                cachedCommunityMaps = maps;
                lastBrowserRefresh = DateTime.UtcNow;

                // Apply filter
                var filteredMaps = ApplySearchFilter(maps, filter);

                OnCommunityMapsLoaded?.Invoke(filteredMaps);
                callback?.Invoke(filteredMaps);

                LogDebug($"Loaded {filteredMaps.Count} community maps");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("community_maps_loaded", new Dictionary<string, object>
                    {
                        { "total_count", maps.Count },
                        { "filtered_count", filteredMaps.Count },
                        { "has_filter", filter != null }
                    });
                }
            });
            #else
            yield return LoadFromCloud(filter, callback);
            #endif
        }

        /// <summary>
        /// Refreshes community maps browser (bypasses cache).
        /// </summary>
        public void RefreshCommunityBrowser(Action<List<CommunityMapMetadata>> callback = null)
        {
            lastBrowserRefresh = DateTime.MinValue; // Invalidate cache
            LoadCommunityMaps(null, callback);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Rating & Likes ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Rates a map (1-5 stars).
        /// </summary>
        public void RateMap(string mapId, int rating, Action<bool> callback = null)
        {
            if (rating < 1 || rating > 5)
            {
                OnOperationError?.Invoke("Rating must be between 1 and 5");
                callback?.Invoke(false);
                return;
            }

            if (!CanInteractWithMaps())
            {
                OnOperationError?.Invoke("Authentication required to rate maps");
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(RateMapCoroutine(mapId, rating, callback));
        }

        private IEnumerator RateMapCoroutine(string mapId, int rating, Action<bool> callback)
        {
            LogDebug($"Rating map {mapId}: {rating} stars");

            // Save locally
            userRatings[mapId] = rating;
            SaveUserRatingsToLocal();

            // Simulate cloud update (replace with actual backend call)
            #if UNITY_EDITOR
            yield return SimulateCloudRating(mapId, rating);
            #else
            yield return SubmitRatingToCloud(mapId, rating);
            #endif

            OnMapRated?.Invoke(mapId, rating);
            callback?.Invoke(true);

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_rated", new Dictionary<string, object>
                {
                    { "map_id", mapId },
                    { "rating", rating }
                });
            }
        }

        /// <summary>
        /// Likes or unlikes a map.
        /// </summary>
        public void ToggleLikeMap(string mapId, Action<bool> callback = null)
        {
            if (!CanInteractWithMaps())
            {
                OnOperationError?.Invoke("Authentication required to like maps");
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(ToggleLikeMapCoroutine(mapId, callback));
        }

        private IEnumerator ToggleLikeMapCoroutine(string mapId, Action<bool> callback)
        {
            bool isLiked = userLikes.Contains(mapId);
            bool newState = !isLiked;

            LogDebug($"{(newState ? "Liking" : "Unliking")} map: {mapId}");

            // Update locally
            if (newState)
                userLikes.Add(mapId);
            else
                userLikes.Remove(mapId);

            SaveUserLikesToLocal();

            // Simulate cloud update (replace with actual backend call)
            #if UNITY_EDITOR
            yield return SimulateCloudLike(mapId, newState);
            #else
            yield return SubmitLikeToCloud(mapId, newState);
            #endif

            OnMapLiked?.Invoke(mapId, newState);
            callback?.Invoke(true);

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_liked", new Dictionary<string, object>
                {
                    { "map_id", mapId },
                    { "is_liked", newState }
                });
            }
        }

        /// <summary>
        /// Gets the user's rating for a map (0 if not rated).
        /// </summary>
        public int GetUserRating(string mapId)
        {
            return userRatings.ContainsKey(mapId) ? userRatings[mapId] : 0;
        }

        /// <summary>
        /// Checks if user has liked a map.
        /// </summary>
        public bool HasUserLiked(string mapId)
        {
            return userLikes.Contains(mapId);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Play Count Tracking ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Increments play count for a map.
        /// Called when user plays a community map.
        /// </summary>
        public void IncrementPlayCount(string mapId)
        {
            if (!isInitialized) return;

            StartCoroutine(IncrementPlayCountCoroutine(mapId));
        }

        private IEnumerator IncrementPlayCountCoroutine(string mapId)
        {
            LogDebug($"Incrementing play count for map: {mapId}");

            // Simulate cloud update (replace with actual backend call)
            #if UNITY_EDITOR
            yield return SimulateIncrementPlayCount(mapId);
            #else
            yield return IncrementPlayCountOnCloud(mapId);
            #endif

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("community_map_played", new Dictionary<string, object>
                {
                    { "map_id", mapId }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Search & Filter ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private List<CommunityMapMetadata> ApplySearchFilter(List<CommunityMapMetadata> maps, MapSearchFilter filter)
        {
            if (filter == null)
                return new List<CommunityMapMetadata>(maps);

            var filtered = maps.AsEnumerable();

            // Search by name
            if (!string.IsNullOrEmpty(filter.searchText))
            {
                filtered = filtered.Where(m =>
                    m.mapName.IndexOf(filter.searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (m.authorName != null && m.authorName.IndexOf(filter.searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (m.description != null && m.description.IndexOf(filter.searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // Filter by tags
            if (filter.tags != null && filter.tags.Length > 0)
            {
                filtered = filtered.Where(m => m.tags != null && m.tags.Intersect(filter.tags).Any());
            }

            // Filter by difficulty
            if (filter.minDifficulty.HasValue)
            {
                filtered = filtered.Where(m => m.difficulty >= filter.minDifficulty.Value);
            }
            if (filter.maxDifficulty.HasValue)
            {
                filtered = filtered.Where(m => m.difficulty <= filter.maxDifficulty.Value);
            }

            // Filter by rating
            if (filter.minRating.HasValue)
            {
                filtered = filtered.Where(m => m.averageRating >= filter.minRating.Value);
            }

            // Sort
            filtered = filter.sortBy switch
            {
                MapSortOption.MostRecent => filtered.OrderByDescending(m => m.publishedDate),
                MapSortOption.MostPlayed => filtered.OrderByDescending(m => m.playCount),
                MapSortOption.HighestRated => filtered.OrderByDescending(m => m.averageRating).ThenByDescending(m => m.ratingCount),
                MapSortOption.MostLiked => filtered.OrderByDescending(m => m.likeCount),
                MapSortOption.Alphabetical => filtered.OrderBy(m => m.mapName),
                _ => filtered.OrderByDescending(m => m.publishedDate)
            };

            // Pagination
            if (filter.pageIndex > 0)
            {
                filtered = filtered.Skip(filter.pageIndex * filter.pageSize);
            }
            if (filter.pageSize > 0)
            {
                filtered = filtered.Take(filter.pageSize);
            }

            return filtered.ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Helper Methods ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private bool CanPublishMaps()
        {
            if (!isInitialized) return false;
            var authManager = AuthenticationManager.Instance;
            return authManager != null && authManager.IsAuthenticated;
        }

        private bool CanInteractWithMaps()
        {
            if (!isInitialized) return false;
            var authManager = AuthenticationManager.Instance;
            return authManager != null && authManager.IsAuthenticated;
        }

        private int CalculateMapDifficulty(CustomMapData mapData)
        {
            // Simple difficulty calculation (1-5)
            // Based on wave count, enemy density, etc.
            int waveCount = mapData.customWaves != null ? mapData.customWaves.Count : 10;
            
            if (waveCount <= 10) return 1;
            if (waveCount <= 20) return 2;
            if (waveCount <= 30) return 3;
            if (waveCount <= 40) return 4;
            return 5;
        }

        private byte[] LoadThumbnailData(string mapId)
        {
            var thumbnailGenerator = RobotTD.Map.MapThumbnailGenerator.Instance;
            if (thumbnailGenerator == null)
                return null;

            string thumbnailPath = thumbnailGenerator.GetThumbnailPath(mapId);
            if (string.IsNullOrEmpty(thumbnailPath))
                return null;

            try
            {
                return System.IO.File.ReadAllBytes(thumbnailPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MapSharingManager] Failed to load thumbnail: {ex.Message}");
                return null;
            }
        }

        private void SaveDownloadedMap(CustomMapData mapData, byte[] thumbnailData)
        {
            // Save map data to PlayerPrefs
            string json = JsonUtility.ToJson(mapData);
            PlayerPrefs.SetString($"CustomMap_{mapData.mapId}", json);
            PlayerPrefs.Save();

            // Save thumbnail to local storage
            if (thumbnailData != null && thumbnailData.Length > 0)
            {
                try
                {
                    string thumbnailsPath = System.IO.Path.Combine(Application.persistentDataPath, "Thumbnails");
                    if (!System.IO.Directory.Exists(thumbnailsPath))
                    {
                        System.IO.Directory.CreateDirectory(thumbnailsPath);
                    }

                    string thumbnailPath = System.IO.Path.Combine(thumbnailsPath, $"{mapData.mapId}.png");
                    System.IO.File.WriteAllBytes(thumbnailPath, thumbnailData);
                    LogDebug($"Saved thumbnail: {thumbnailPath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MapSharingManager] Failed to save thumbnail: {ex.Message}");
                }
            }

            LogDebug($"Saved downloaded map: {mapData.mapName}");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage (Ratings & Likes) ───────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadUserRatingsFromLocal()
        {
            string ratingsJson = PlayerPrefs.GetString("MapRatings", "{}");
            try
            {
                var ratingsData = JsonUtility.FromJson<UserRatingsData>(ratingsJson);
                if (ratingsData != null && ratingsData.ratings != null)
                {
                    userRatings = new Dictionary<string, int>();
                    foreach (var rating in ratingsData.ratings)
                    {
                        userRatings[rating.mapId] = rating.rating;
                    }
                    LogDebug($"Loaded {userRatings.Count} user ratings");
                }
            }
            catch
            {
                LogDebug("No user ratings found");
            }
        }

        private void SaveUserRatingsToLocal()
        {
            var ratingsData = new UserRatingsData
            {
                ratings = userRatings.Select(kvp => new MapRatingEntry { mapId = kvp.Key, rating = kvp.Value }).ToArray()
            };
            string json = JsonUtility.ToJson(ratingsData);
            PlayerPrefs.SetString("MapRatings", json);
            PlayerPrefs.Save();
        }

        private void LoadUserLikesFromLocal()
        {
            string likesJson = PlayerPrefs.GetString("MapLikes", "{}");
            try
            {
                var likesData = JsonUtility.FromJson<UserLikesData>(likesJson);
                if (likesData != null && likesData.likedMaps != null)
                {
                    userLikes = new HashSet<string>(likesData.likedMaps);
                    LogDebug($"Loaded {userLikes.Count} user likes");
                }
            }
            catch
            {
                LogDebug("No user likes found");
            }
        }

        private void SaveUserLikesToLocal()
        {
            var likesData = new UserLikesData { likedMaps = userLikes.ToArray() };
            string json = JsonUtility.ToJson(likesData);
            PlayerPrefs.SetString("MapLikes", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Simulation Methods (Editor Testing) ──────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        #if UNITY_EDITOR
        private IEnumerator SimulateCloudUpload(CustomMapData mapData, CommunityMapMetadata metadata, byte[] thumbnailData)
        {
            yield return new WaitForSeconds(1f); // Simulate network delay
            LogDebug($"[SIMULATED] Uploaded map to cloud: {mapData.mapName}");
        }

        private IEnumerator SimulateCloudDownload(string mapId, Action<CustomMapData, byte[]> callback)
        {
            yield return new WaitForSeconds(1f);
            
            // Try to load from local storage for simulation
            string json = PlayerPrefs.GetString($"CustomMap_{mapId}", "");
            if (!string.IsNullOrEmpty(json))
            {
                CustomMapData mapData = JsonUtility.FromJson<CustomMapData>(json);
                byte[] thumbnailData = LoadThumbnailData(mapId);
                callback?.Invoke(mapData, thumbnailData);
                LogDebug($"[SIMULATED] Downloaded map from cloud: {mapData.mapName}");
            }
            else
            {
                callback?.Invoke(null, null);
                LogDebug($"[SIMULATED] Map not found: {mapId}");
            }
        }

        private IEnumerator SimulateLoadCommunityMaps(Action<List<CommunityMapMetadata>> callback)
        {
            yield return new WaitForSeconds(1f);
            
            // Generate some sample data for testing
            var sampleMaps = new List<CommunityMapMetadata>
            {
                new CommunityMapMetadata
                {
                    mapId = "sample_1",
                    mapName = "Desert Fortress",
                    authorId = "user_123",
                    authorName = "MapMaster",
                    description = "Challenging desert defense scenario",
                    difficulty = 3,
                    gridWidth = 20,
                    gridHeight = 15,
                    waveCount = 25,
                    tags = new[] { "desert", "hard", "fortress" },
                    publishedDate = DateTime.UtcNow.AddDays(-7),
                    playCount = 142,
                    likeCount = 28,
                    averageRating = 4.2f,
                    ratingCount = 15
                },
                new CommunityMapMetadata
                {
                    mapId = "sample_2",
                    mapName = "Beginner's Valley",
                    authorId = "user_456",
                    authorName = "EasyMapper",
                    description = "Perfect for learning tower defense",
                    difficulty = 1,
                    gridWidth = 15,
                    gridHeight = 10,
                    waveCount = 10,
                    tags = new[] { "beginner", "tutorial", "easy" },
                    publishedDate = DateTime.UtcNow.AddDays(-2),
                    playCount = 521,
                    likeCount = 89,
                    averageRating = 4.7f,
                    ratingCount = 43
                }
            };

            callback?.Invoke(sampleMaps);
            LogDebug($"[SIMULATED] Loaded {sampleMaps.Count} community maps");
        }

        private IEnumerator SimulateCloudRating(string mapId, int rating)
        {
            yield return new WaitForSeconds(0.5f);
            LogDebug($"[SIMULATED] Submitted rating to cloud: {mapId} = {rating}");
        }

        private IEnumerator SimulateCloudLike(string mapId, bool isLiked)
        {
            yield return new WaitForSeconds(0.5f);
            LogDebug($"[SIMULATED] Submitted like to cloud: {mapId} = {isLiked}");
        }

        private IEnumerator SimulateIncrementPlayCount(string mapId)
        {
            yield return new WaitForSeconds(0.5f);
            LogDebug($"[SIMULATED] Incremented play count for: {mapId}");
        }
        #endif

        // ══════════════════════════════════════════════════════════════════════
        // ── Cloud Backend Methods (Production) ────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        #if !UNITY_EDITOR
        private IEnumerator UploadToCloud(CustomMapData mapData, CommunityMapMetadata metadata, byte[] thumbnailData)
        {
            // TODO: Implement actual cloud upload using UnityWebRequest or REST API
            // POST to cloudEndpoint/publish with multipart form data
            yield return null;
        }

        private IEnumerator DownloadFromCloud(string mapId, Action<CustomMapData> callback)
        {
            // TODO: Implement actual cloud download using UnityWebRequest
            // GET from cloudEndpoint/download/{mapId}
            yield return null;
            callback?.Invoke(null);
        }

        private IEnumerator LoadFromCloud(MapSearchFilter filter, Action<List<CommunityMapMetadata>> callback)
        {
            // TODO: Implement actual cloud query using UnityWebRequest
            // GET from cloudEndpoint/browse with query parameters
            yield return null;
            callback?.Invoke(new List<CommunityMapMetadata>());
        }

        private IEnumerator SubmitRatingToCloud(string mapId, int rating)
        {
            // TODO: Implement actual API call
            // POST to cloudEndpoint/rate with mapId and rating
            yield return null;
        }

        private IEnumerator SubmitLikeToCloud(string mapId, bool isLiked)
        {
            // TODO: Implement actual API call
            // POST to cloudEndpoint/like with mapId and isLiked
            yield return null;
        }

        private IEnumerator IncrementPlayCountOnCloud(string mapId)
        {
            // TODO: Implement actual API call
            // POST to cloudEndpoint/play-count/{mapId}
            yield return null;
        }
        #endif

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[MapSharingManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Metadata for a community-shared map.
    /// </summary>
    [Serializable]
    public class CommunityMapMetadata
    {
        public string mapId;
        public string mapName;
        public string authorId;
        public string authorName;
        public string description;
        public int difficulty; // 1-5
        public int gridWidth;
        public int gridHeight;
        public int waveCount;
        public string[] tags;
        public DateTime publishedDate;
        public DateTime lastUpdatedDate;
        public int playCount;
        public int likeCount;
        public float averageRating; // 0-5
        public int ratingCount;
    }

    /// <summary>
    /// Search filter for community map browser.
    /// </summary>
    [Serializable]
    public class MapSearchFilter
    {
        public string searchText;
        public string[] tags;
        public int? minDifficulty;
        public int? maxDifficulty;
        public float? minRating;
        public MapSortOption sortBy = MapSortOption.MostRecent;
        public int pageIndex = 0;
        public int pageSize = 20;
    }

    /// <summary>
    /// Sort options for community map browser.
    /// </summary>
    public enum MapSortOption
    {
        MostRecent,
        MostPlayed,
        HighestRated,
        MostLiked,
        Alphabetical
    }

    /// <summary>
    /// User ratings data for serialization.
    /// </summary>
    [Serializable]
    public class UserRatingsData
    {
        public MapRatingEntry[] ratings;
    }

    [Serializable]
    public class MapRatingEntry
    {
        public string mapId;
        public int rating;
    }

    /// <summary>
    /// User likes data for serialization.
    /// </summary>
    [Serializable]
    public class UserLikesData
    {
        public string[] likedMaps;
    }
}
