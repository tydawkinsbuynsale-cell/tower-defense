using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Towers;
using RobotTD.Enemies;
using RobotTD.Map;
using RobotTD.Analytics;

namespace RobotTD.Content
{
    /// <summary>
    /// Content expansion pack manager for new towers, enemies, and maps.
    /// Handles registration, unlocking, and feature gating for expansion content.
    /// Includes 5 new tower types, 8 new enemy types, and 4 new campaign maps.
    /// </summary>
    public class ContentExpansionManager : MonoBehaviour
    {
        public static ContentExpansionManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableExpansionContent = true;
        [SerializeField] private bool verboseLogging = true;

        [Header("New Tower Types")]
        [SerializeField] private ExpandedTowerData[] newTowerTypes;

        [Header("New Enemy Types")]
        [SerializeField] private ExpandedEnemyData[] newEnemyTypes;

        [Header("New Campaign Maps")]
        [SerializeField] private ExpandedMapData[] newMapData;

        // State
        private bool isInitialized = false;
        private HashSet<string> unlockedTowers = new HashSet<string>();
        private HashSet<string> unlockedEnemies = new HashSet<string>();
        private HashSet<string> unlockedMaps = new HashSet<string>();

        // Tower type registrations
        private Dictionary<string, ExpandedTowerData> towerRegistry = new Dictionary<string, ExpandedTowerData>();
        private Dictionary<string, ExpandedEnemyData> enemyRegistry = new Dictionary<string, ExpandedEnemyData>();
        private Dictionary<string, ExpandedMapData> mapRegistry = new Dictionary<string, ExpandedMapData>();

        // Events
        public event Action<ExpandedTowerData> OnTowerTypeUnlocked;
        public event Action<ExpandedEnemyData> OnEnemyTypeUnlocked;
        public event Action<ExpandedMapData> OnMapUnlocked;
        public event Action OnExpansionContentInitialized;

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
            if (!enableExpansionContent)
            {
                LogDebug("Expansion content disabled");
                return;
            }

            StartCoroutine(InitializeExpansionContent());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeExpansionContent()
        {
            LogDebug("Initializing expansion content...");

            yield return new WaitForSeconds(0.3f);

            // Register all content
            RegisterTowerTypes();
            RegisterEnemyTypes();
            RegisterMaps();

            // Load unlocked content
            LoadUnlockedContent();

            // Auto-unlock content for testing (in production, use unlock requirements)
            #if UNITY_EDITOR
            AutoUnlockContentInEditor();
            #endif

            isInitialized = true;
            LogDebug($"Expansion content initialized: {unlockedTowers.Count} towers, {unlockedEnemies.Count} enemies, {unlockedMaps.Count} maps");

            OnExpansionContentInitialized?.Invoke();

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("expansion_content_initialized", new Dictionary<string, object>
                {
                    { "unlocked_towers", unlockedTowers.Count },
                    { "unlocked_enemies", unlockedEnemies.Count },
                    { "unlocked_maps", unlockedMaps.Count },
                    { "total_towers", newTowerTypes.Length },
                    { "total_enemies", newEnemyTypes.Length },
                    { "total_maps", newMapData.Length }
                });
            }
        }

        private void RegisterTowerTypes()
        {
            towerRegistry.Clear();
            foreach (var tower in newTowerTypes)
            {
                towerRegistry[tower.towerId] = tower;
            }
            LogDebug($"Registered {towerRegistry.Count} new tower types");
        }

        private void RegisterEnemyTypes()
        {
            enemyRegistry.Clear();
            foreach (var enemy in newEnemyTypes)
            {
                enemyRegistry[enemy.enemyId] = enemy;
            }
            LogDebug($"Registered {enemyRegistry.Count} new enemy types");
        }

        private void RegisterMaps()
        {
            mapRegistry.Clear();
            foreach (var map in newMapData)
            {
                mapRegistry[map.mapId] = map;
            }
            LogDebug($"Registered {mapRegistry.Count} new maps");
        }

        #if UNITY_EDITOR
        private void AutoUnlockContentInEditor()
        {
            // Auto-unlock all content in editor for testing
            foreach (var tower in newTowerTypes)
            {
                UnlockTower(tower.towerId, false);
            }
            foreach (var enemy in newEnemyTypes)
            {
                UnlockEnemy(enemy.enemyId, false);
            }
            foreach (var map in newMapData)
            {
                UnlockMap(map.mapId, false);
            }
            SaveUnlockedContent();
            LogDebug("Auto-unlocked all expansion content in editor");
        }
        #endif

        // ══════════════════════════════════════════════════════════════════════
        // ── New Tower Types ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a new tower type.
        /// </summary>
        public bool UnlockTower(string towerId, bool saveImmediately = true)
        {
            if (unlockedTowers.Contains(towerId))
            {
                LogDebug($"Tower already unlocked: {towerId}");
                return false;
            }

            if (!towerRegistry.ContainsKey(towerId))
            {
                LogDebug($"Tower not found in registry: {towerId}");
                return false;
            }

            unlockedTowers.Add(towerId);

            if (saveImmediately)
            {
                SaveUnlockedContent();
            }

            var towerData = towerRegistry[towerId];
            OnTowerTypeUnlocked?.Invoke(towerData);
            LogDebug($"Unlocked tower: {towerData.towerName} ({towerId})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("expansion_tower_unlocked", new Dictionary<string, object>
                {
                    { "tower_id", towerId },
                    { "tower_name", towerData.towerName },
                    { "tower_category", towerData.category.ToString() }
                });
            }

            return true;
        }

        /// <summary>
        /// Checks if a tower is unlocked.
        /// </summary>
        public bool IsTowerUnlocked(string towerId)
        {
            return unlockedTowers.Contains(towerId);
        }

        /// <summary>
        /// Gets all unlocked tower types.
        /// </summary>
        public List<ExpandedTowerData> GetUnlockedTowers()
        {
            return newTowerTypes.Where(tower => unlockedTowers.Contains(tower.towerId)).ToList();
        }

        /// <summary>
        /// Gets a specific tower data.
        /// </summary>
        public ExpandedTowerData GetTowerData(string towerId)
        {
            return towerRegistry.ContainsKey(towerId) ? towerRegistry[towerId] : null;
        }

        /// <summary>
        /// Gets all expansion tower types.
        /// </summary>
        public List<ExpandedTowerData> GetAllExpansionTowers()
        {
            return newTowerTypes.ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── New Enemy Types ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a new enemy type.
        /// </summary>
        public bool UnlockEnemy(string enemyId, bool saveImmediately = true)
        {
            if (unlockedEnemies.Contains(enemyId))
            {
                LogDebug($"Enemy already unlocked: {enemyId}");
                return false;
            }

            if (!enemyRegistry.ContainsKey(enemyId))
            {
                LogDebug($"Enemy not found in registry: {enemyId}");
                return false;
            }

            unlockedEnemies.Add(enemyId);

            if (saveImmediately)
            {
                SaveUnlockedContent();
            }

            var enemyData = enemyRegistry[enemyId];
            OnEnemyTypeUnlocked?.Invoke(enemyData);
            LogDebug($"Unlocked enemy: {enemyData.enemyName} ({enemyId})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("expansion_enemy_unlocked", new Dictionary<string, object>
                {
                    { "enemy_id", enemyId },
                    { "enemy_name", enemyData.enemyName },
                    { "enemy_class", enemyData.enemyClass.ToString() }
                });
            }

            return true;
        }

        /// <summary>
        /// Checks if an enemy is unlocked.
        /// </summary>
        public bool IsEnemyUnlocked(string enemyId)
        {
            return unlockedEnemies.Contains(enemyId);
        }

        /// <summary>
        /// Gets all unlocked enemy types.
        /// </summary>
        public List<ExpandedEnemyData> GetUnlockedEnemies()
        {
            return newEnemyTypes.Where(enemy => unlockedEnemies.Contains(enemy.enemyId)).ToList();
        }

        /// <summary>
        /// Gets a specific enemy data.
        /// </summary>
        public ExpandedEnemyData GetEnemyData(string enemyId)
        {
            return enemyRegistry.ContainsKey(enemyId) ? enemyRegistry[enemyId] : null;
        }

        /// <summary>
        /// Gets all expansion enemy types.
        /// </summary>
        public List<ExpandedEnemyData> GetAllExpansionEnemies()
        {
            return newEnemyTypes.ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── New Maps ──────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a new map.
        /// </summary>
        public bool UnlockMap(string mapId, bool saveImmediately = true)
        {
            if (unlockedMaps.Contains(mapId))
            {
                LogDebug($"Map already unlocked: {mapId}");
                return false;
            }

            if (!mapRegistry.ContainsKey(mapId))
            {
                LogDebug($"Map not found in registry: {mapId}");
                return false;
            }

            unlockedMaps.Add(mapId);

            if (saveImmediately)
            {
                SaveUnlockedContent();
            }

            var mapData = mapRegistry[mapId];
            OnMapUnlocked?.Invoke(mapData);
            LogDebug($"Unlocked map: {mapData.mapName} ({mapId})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("expansion_map_unlocked", new Dictionary<string, object>
                {
                    { "map_id", mapId },
                    { "map_name", mapData.mapName },
                    { "map_theme", mapData.theme.ToString() }
                });
            }

            return true;
        }

        /// <summary>
        /// Checks if a map is unlocked.
        /// </summary>
        public bool IsMapUnlocked(string mapId)
        {
            return unlockedMaps.Contains(mapId);
        }

        /// <summary>
        /// Gets all unlocked maps.
        /// </summary>
        public List<ExpandedMapData> GetUnlockedMaps()
        {
            return newMapData.Where(map => unlockedMaps.Contains(map.mapId)).ToList();
        }

        /// <summary>
        /// Gets a specific map data.
        /// </summary>
        public ExpandedMapData GetMapData(string mapId)
        {
            return mapRegistry.ContainsKey(mapId) ? mapRegistry[mapId] : null;
        }

        /// <summary>
        /// Gets all expansion maps.
        /// </summary>
        public List<ExpandedMapData> GetAllExpansionMaps()
        {
            return newMapData.ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Content Statistics ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets expansion pack completion percentage.
        /// </summary>
        public float GetExpansionCompletion()
        {
            int totalContent = newTowerTypes.Length + newEnemyTypes.Length + newMapData.Length;
            int unlockedContent = unlockedTowers.Count + unlockedEnemies.Count + unlockedMaps.Count;
            return totalContent > 0 ? (float)unlockedContent / totalContent : 0f;
        }

        /// <summary>
        /// Gets expansion pack statistics.
        /// </summary>
        public ExpansionStats GetExpansionStats()
        {
            return new ExpansionStats
            {
                towersUnlocked = unlockedTowers.Count,
                towersTotal = newTowerTypes.Length,
                enemiesUnlocked = unlockedEnemies.Count,
                enemiesTotal = newEnemyTypes.Length,
                mapsUnlocked = unlockedMaps.Count,
                mapsTotal = newMapData.Length,
                completionPercentage = GetExpansionCompletion()
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadUnlockedContent()
        {
            string json = PlayerPrefs.GetString("ExpansionContent", "{}");
            try
            {
                var data = JsonUtility.FromJson<ExpansionContentData>(json);
                if (data != null)
                {
                    unlockedTowers = new HashSet<string>(data.unlockedTowers ?? new string[0]);
                    unlockedEnemies = new HashSet<string>(data.unlockedEnemies ?? new string[0]);
                    unlockedMaps = new HashSet<string>(data.unlockedMaps ?? new string[0]);

                    LogDebug($"Loaded expansion content: {unlockedTowers.Count} towers, {unlockedEnemies.Count} enemies, {unlockedMaps.Count} maps");
                }
            }
            catch
            {
                LogDebug("No expansion content data found");
            }
        }

        private void SaveUnlockedContent()
        {
            var data = new ExpansionContentData
            {
                unlockedTowers = unlockedTowers.ToArray(),
                unlockedEnemies = unlockedEnemies.ToArray(),
                unlockedMaps = unlockedMaps.ToArray()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("ExpansionContent", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[ContentExpansionManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Expanded tower type definition.
    /// </summary>
    [Serializable]
    public class ExpandedTowerData
    {
        public string towerId;
        public string towerName;
        public string description;
        public TowerCategory category;
        
        [Header("Stats")]
        public int baseDamage;
        public float attackSpeed;
        public float range;
        public int cost;
        
        [Header("Special Abilities")]
        public string specialAbility;
        public float abilityValue;
        public float abilityCooldown;
        
        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefab;
        
        [Header("Unlock Requirements")]
        public string unlockRequirement;
        public int unlockLevel = 1;
    }

    /// <summary>
    /// Tower categories for expansion towers.
    /// </summary>
    public enum TowerCategory
    {
        AreaDenial,      // Laser Grid
        Disable,         // EMP Tower
        Trap,            // Mine Layer
        Support,         // Support Drone
        Environmental    // Weather Control
    }

    /// <summary>
    /// Expanded enemy type definition.
    /// </summary>
    [Serializable]
    public class ExpandedEnemyData
    {
        public string enemyId;
        public string enemyName;
        public string description;
        public EnemyClass enemyClass;
        
        [Header("Stats")]
        public int health;
        public float speed;
        public int damage;
        public int creditReward;
        
        [Header("Special Abilities")]
        public string specialAbility;
        public float abilityValue;
        public float abilityCooldown;
        
        [Header("Resistances")]
        public float physicalResistance;
        public float energyResistance;
        public float explosiveResistance;
        
        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefab;
        
        [Header("Unlock Requirements")]
        public string unlockRequirement;
        public int unlockWave = 1;
    }

    /// <summary>
    /// Enemy classes for expansion enemies.
    /// </summary>
    public enum EnemyClass
    {
        Infiltrator,     // Burrower
        Teleporter,      // Phaser
        Splitter,        // Replicator
        Drainer,         // Vampire
        Berserker,       // Berserker
        Adaptive,        // Mimic
        Support,         // Engineer
        Artillery        // Siege Engine
    }

    /// <summary>
    /// Expanded map definition.
    /// </summary>
    [Serializable]
    public class ExpandedMapData
    {
        public string mapId;
        public string mapName;
        public string description;
        public MapTheme theme;
        
        [Header("Map Settings")]
        public int totalWaves;
        public int pathCount;
        public int difficulty; // 1-5
        
        [Header("Unique Mechanics")]
        public string uniqueMechanic;
        public string mechanicDescription;
        public float mechanicValue;
        
        [Header("Environmental Hazards")]
        public string[] hazards;
        public float hazardDamage;
        public float hazardInterval;
        
        [Header("Visuals")]
        public Sprite previewImage;
        public string sceneName;
        
        [Header("Unlock Requirements")]
        public string unlockRequirement;
        public int unlockLevel = 1;
    }

    /// <summary>
    /// Map themes for expansion maps.
    /// </summary>
    public enum MapTheme
    {
        SpaceStation,    // Zero-gravity, multi-level
        Underwater,      // Pressure zones, bubble shields
        Arctic,          // Freezing, ice hazards
        Volcanic         // Lava flows, heat damage
    }

    /// <summary>
    /// Expansion pack statistics.
    /// </summary>
    [Serializable]
    public class ExpansionStats
    {
        public int towersUnlocked;
        public int towersTotal;
        public int enemiesUnlocked;
        public int enemiesTotal;
        public int mapsUnlocked;
        public int mapsTotal;
        public float completionPercentage;
    }

    /// <summary>
    /// Storage structure for expansion content.
    /// </summary>
    [Serializable]
    public class ExpansionContentData
    {
        public string[] unlockedTowers;
        public string[] unlockedEnemies;
        public string[] unlockedMaps;
    }
}
