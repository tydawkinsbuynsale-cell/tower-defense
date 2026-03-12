using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace RobotTD.Core
{
    // ── Data containers ──────────────────────────────────────────────────────

    [Serializable]
    public class PlayerSaveData
    {
        public string saveVersion = "1.0";
        public long lastSaveTimestamp = 0;
        public string playerName = "Player";

        // Progression
        public int playerLevel = 1;
        public int totalXP = 0;
        public int techPoints = 0;
        public int totalCreditsEarned = 0;
        public int totalEnemiesKilled = 0;
        public int totalTowersPlaced = 0;
        public int totalTowersUpgraded = 0;
        public int totalWavesCompleted = 0;
        public float totalPlayTimeSeconds = 0f;
        public int totalGamesPlayed = 0;
        public int totalVictories = 0;

        // Session tracking (for current game session)
        public int currentSessionKills = 0;
        public int currentSessionCreditsEarned = 0;
        public float sessionStartTime = 0f;

        // Settings
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 0.6f;
        public bool vibrationEnabled = true;
        public int graphicsQuality = 2; // 0=low 1=med 2=high

        // Game state
        public List<string> unlockedMaps = new List<string> { "TrainingGrounds" };
        public List<string> completedAchievements = new List<string>();
        public TechTreeSaveData techTree = new TechTreeSaveData();

        // Per-map best scores  (mapId -> MapRecord)
        public Dictionary<string, MapRecord> mapRecords = new Dictionary<string, MapRecord>();

        // Daily/weekly challenges (future expansion)
        public int dailyStreak = 0;
        public long lastDailyLoginTimestamp = 0;
    }

    [Serializable]
    public class MapRecord
    {
        public int highScore = 0;
        public int bestWave = 0;
        public int starsEarned = 0;   // 0–3
        public float fastestClear = float.MaxValue;
        public bool completed = false;
    }

    [Serializable]
    public class TechTreeSaveData
    {
        // Each field is the upgrade level (0 = not purchased)
        public int firepower = 0;       // +5% global damage per level (max 5)
        public int efficiency = 0;      // -5% tower cost per level (max 5)
        public int resilience = 0;      // +2 starting lives per level (max 5)
        public int tactics = 0;         // +10% kill reward per level (max 5)
        public int rapidDeploy = 0;     // -10% placement time per level (max 3)
        public int recycling = 0;       // +10% sell value per level (max 3)
    }

    // ── SaveManager ──────────────────────────────────────────────────────────

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE = "player_save.json";
        private const string BACKUP_FILE = "player_save_backup.json";
        private const string SAVE_VERSION = "1.0";

        public PlayerSaveData Data { get; private set; }

        public event Action OnSaved;
        public event Action OnLoaded;
        public event Action<string> OnSaveError;

        [Header("Auto-Save Settings")]
        [SerializeField] private float autoSaveInterval = 60f;
        [SerializeField] private bool createBackups = true;
        [SerializeField] private bool verboseLogging = false;

        private float autoSaveTimer;
        private bool isDirty = false; // Track if data has changed since last save
        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);
        private string BackupPath => Path.Combine(Application.persistentDataPath, BACKUP_FILE);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        private void Update()
        {
            // Track play time
            if (Data != null)
            {
                Data.totalPlayTimeSeconds += Time.unscaledDeltaTime;
                
                // Check play time achievements periodically (every 60 seconds)
                if (Time.frameCount % 3600 == 0) // ~60 seconds at 60 FPS
                {
                    float totalHours = Data.totalPlayTimeSeconds / 3600f;
                    Progression.AchievementManager.Instance?.CheckPlayTime(totalHours);
                }
            }

            // Auto-save every minute (only if data changed)
            autoSaveTimer += Time.unscaledDeltaTime;
            if (autoSaveTimer >= autoSaveInterval && isDirty)
            {
                autoSaveTimer = 0f;
                Save();
            }
        }

        private void OnApplicationQuit() => Save();
        private void OnApplicationPause(bool paused) { if (paused) Save(); }

        // ── Save / Load ──────────────────────────────────────────────────────

        public void Save()
        {
            try
            {
                // Update timestamp
                Data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Data.saveVersion = SAVE_VERSION;

                string json = JsonUtility.ToJson(Data, prettyPrint: true);

                // Create backup of previous save
                if (createBackups && File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, overwrite: true);
                }

                // Write new save
                File.WriteAllText(SavePath, json);
                
                isDirty = false;
                OnSaved?.Invoke();
                
                if (verboseLogging)
                    Debug.Log($"[SaveManager] Saved to {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
                OnSaveError?.Invoke(e.Message);
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    Data = JsonUtility.FromJson<PlayerSaveData>(json);
                    
                    // Validate save version
                    if (string.IsNullOrEmpty(Data.saveVersion))
                    {
                        Debug.LogWarning("[SaveManager] Old save format detected. Migrating...");
                        Data.saveVersion = SAVE_VERSION;
                    }
                    
                    if (verboseLogging)
                        Debug.Log("[SaveManager] Save loaded successfully.");
                }
                else
                {
                    Data = new PlayerSaveData();
                    Debug.Log("[SaveManager] No save found — creating fresh data.");
                    Save(); // Create initial save file
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}. Attempting backup restore...");
                
                // Try loading backup
                if (TryLoadBackup())
                {
                    Debug.Log("[SaveManager] Backup loaded successfully!");
                }
                else
                {
                    Data = new PlayerSaveData();
                    Debug.Log("[SaveManager] Starting with fresh data.");
                }
            }

            ResetSessionData();
            ApplySettings();
            isDirty = false;
            OnLoaded?.Invoke();
        }

        private bool TryLoadBackup()
        {
            if (!File.Exists(BackupPath)) return false;

            try
            {
                string json = File.ReadAllText(BackupPath);
                Data = JsonUtility.FromJson<PlayerSaveData>(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
            
            if (File.Exists(BackupPath))
                File.Delete(BackupPath);

            Data = new PlayerSaveData();
            isDirty = true;
            Save();
            Debug.Log("[SaveManager] Save data deleted and reset.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void ApplySettings()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.MasterVolume = Data.masterVolume;
                Audio.AudioManager.Instance.SFXVolume = Data.sfxVolume;
                Audio.AudioManager.Instance.MusicVolume = Data.musicVolume;
            }

            QualitySettings.SetQualityLevel(Data.graphicsQuality, true);
        }

        public void RecordMapResult(string mapId, int score, int wave, int stars, float clearTime, bool victory)
        {
            if (!Data.mapRecords.ContainsKey(mapId))
                Data.mapRecords[mapId] = new MapRecord();

            var record = Data.mapRecords[mapId];
            bool isNewBest = false;

            if (score > record.highScore)
            {
                record.highScore = score;
                isNewBest = true;
            }
            if (wave > record.bestWave)
            {
                record.bestWave = wave;
            }
            if (stars > record.starsEarned)
            {
                record.starsEarned = stars;
                isNewBest = true;
            }
            if (victory && clearTime < record.fastestClear)
            {
                record.fastestClear = clearTime;
                isNewBest = true;
            }
            if (victory)
            {
                record.completed = true;
            }

            // Update global stats
            Data.totalGamesPlayed++;
            if (victory)
            {
                Data.totalVictories++;
            }
            Data.totalWavesCompleted += wave;

            // Grant XP for performance
            int xpGained = CalculateXPReward(score, wave, stars, victory);
            AddXP(xpGained);

            isDirty = true;
            Save();

            if (isNewBest)
            {
                Debug.Log($"[SaveManager] New record on {mapId}!");
            }
        }

        private int CalculateXPReward(int score, int wave, int stars, bool victory)
        {
            int xp = wave * 10; // 10 XP per wave completed
            xp += score / 100; // Bonus XP from score
            xp += stars * 50; // 50 XP per star
            if (victory) xp += 200; // Victory bonus
            return xp;
        }

        public void AddXP(int amount)
        {
            Data.totalXP += amount;
            int oldLevel = Data.playerLevel;
            int newLevel = CalculateLevel(Data.totalXP);
            
            if (newLevel > oldLevel)
            {
                int gained = newLevel - oldLevel;
                Data.playerLevel = newLevel;
                Data.techPoints += gained * 3; // 3 tech points per level up
                
                Debug.Log($"[SaveManager] Level Up! {oldLevel} → {newLevel} (+{gained * 3} Tech Points)");
                Progression.AchievementManager.Instance?.CheckLevelUp(newLevel);
                
                // Show level up notification
                UI.ToastNotification.Instance?.Show($"Level Up! Now Level {newLevel}", UI.ToastType.Success);
            }
            
            isDirty = true;
        }

        public void AddKills(int count)
        {
            Data.totalEnemiesKilled += count;
            Data.currentSessionKills += count;
            isDirty = true;
            
            Progression.AchievementManager.Instance?.CheckKillCount(Data.totalEnemiesKilled);
        }

        public void AddCreditsEarned(int amount)
        {
            Data.totalCreditsEarned += amount;
            Data.currentSessionCreditsEarned += amount;
            isDirty = true;

            // Check credit achievements
            Progression.AchievementManager.Instance?.CheckCreditsEarned(Data.totalCreditsEarned);
        }

        public void RecordTowerPlaced()
        {
            Data.totalTowersPlaced++;
            isDirty = true;
        }

        public void RecordTowerUpgraded()
        {
            Data.totalTowersUpgraded++;
            isDirty = true;
        }

        public void UnlockMap(string mapId)
        {
            if (!Data.unlockedMaps.Contains(mapId))
            {
                Data.unlockedMaps.Add(mapId);
                isDirty = true;
                Save();
                
                UI.ToastNotification.Instance?.Show($"New Map Unlocked: {mapId}", UI.ToastType.Success);
                Debug.Log($"[SaveManager] Map unlocked: {mapId}");
            }
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!Data.completedAchievements.Contains(achievementId))
            {
                Data.completedAchievements.Add(achievementId);
                isDirty = true;
                Save();
                
                Debug.Log($"[SaveManager] Achievement unlocked: {achievementId}");
            }
        }

        public bool IsMapUnlocked(string mapId)
        {
            return Data.unlockedMaps.Contains(mapId);
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
            return Data.completedAchievements.Contains(achievementId);
        }

        public MapRecord GetMapRecord(string mapId)
        {
            return Data.mapRecords.ContainsKey(mapId) ? Data.mapRecords[mapId] : new MapRecord();
        }

        private void ResetSessionData()
        {
            Data.currentSessionKills = 0;
            Data.currentSessionCreditsEarned = 0;
            Data.sessionStartTime = Time.time;
        }

        private int CalculateLevel(int xp)
        {
            // XP required per level: 100, 250, 500, 800, 1200, ...
            int level = 1;
            int required = 0;
            int increment = 100;
            while (xp >= required + increment)
            {
                required += increment;
                increment = Mathf.RoundToInt(increment * 1.5f);
                level++;
            }
            return level;
        }

        public float GetXPProgressToNextLevel()
        {
            int xpForCurrent = XPForLevel(Data.playerLevel);
            int xpForNext = XPForLevel(Data.playerLevel + 1);
            return Mathf.Clamp01((float)(Data.totalXP - xpForCurrent) / (xpForNext - xpForCurrent));
        }

        private int XPForLevel(int level)
        {
            int total = 0;
            int inc = 100;
            for (int i = 1; i < level; i++)
            {
                total += inc;
                inc = Mathf.RoundToInt(inc * 1.5f);
            }
            return total;
        }
    }
}
