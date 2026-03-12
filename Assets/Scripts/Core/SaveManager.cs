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
        public int playerLevel = 1;
        public int totalXP = 0;
        public int techPoints = 0;
        public int totalCreditsEarned = 0;
        public int totalEnemiesKilled = 0;
        public int totalWavesCompleted = 0;
        public float totalPlayTimeSeconds = 0f;

        // Settings
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 0.6f;
        public bool vibrationEnabled = true;
        public int graphicsQuality = 2; // 0=low 1=med 2=high

        // Progression
        public List<string> unlockedMaps = new List<string> { "TrainingGrounds" };
        public List<string> completedAchievements = new List<string>();
        public TechTreeSaveData techTree = new TechTreeSaveData();

        // Per-map best scores  (mapId -> MapRecord)
        public Dictionary<string, MapRecord> mapRecords = new Dictionary<string, MapRecord>();
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
        private const string SAVE_VERSION = "1.0";

        public PlayerSaveData Data { get; private set; }

        public event Action OnSaved;
        public event Action OnLoaded;

        private float autoSaveInterval = 60f;
        private float autoSaveTimer;
        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        private void Update()
        {
            // Auto-save every minute
            autoSaveTimer += Time.unscaledDeltaTime;
            if (autoSaveTimer >= autoSaveInterval)
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
                string json = JsonUtility.ToJson(Data, prettyPrint: true);
                File.WriteAllText(SavePath, json);
                OnSaved?.Invoke();
                Debug.Log($"[SaveManager] Saved to {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
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
                    Debug.Log("[SaveManager] Save loaded.");
                }
                else
                {
                    Data = new PlayerSaveData();
                    Debug.Log("[SaveManager] No save found — fresh data.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}. Starting fresh.");
                Data = new PlayerSaveData();
            }

            ApplySettings();
            OnLoaded?.Invoke();
        }

        public void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            Data = new PlayerSaveData();
            Save();
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

        public void RecordMapResult(string mapId, int score, int wave, int stars, float clearTime)
        {
            if (!Data.mapRecords.ContainsKey(mapId))
                Data.mapRecords[mapId] = new MapRecord();

            var record = Data.mapRecords[mapId];
            if (score > record.highScore) record.highScore = score;
            if (wave > record.bestWave) record.bestWave = wave;
            if (stars > record.starsEarned) record.starsEarned = stars;
            if (clearTime < record.fastestClear) record.fastestClear = clearTime;
            record.completed = true;

            Save();
        }

        public void AddXP(int amount)
        {
            Data.totalXP += amount;
            int newLevel = CalculateLevel(Data.totalXP);
            if (newLevel > Data.playerLevel)
            {
                int gained = newLevel - Data.playerLevel;
                Data.playerLevel = newLevel;
                Data.techPoints += gained * 3; // 3 tech points per level up
                Progression.AchievementManager.Instance?.CheckLevelUp(newLevel);
            }
        }

        public void AddKills(int count)
        {
            Data.totalEnemiesKilled += count;
            Progression.AchievementManager.Instance?.CheckKillCount(Data.totalEnemiesKilled);
        }

        public void UnlockMap(string mapId)
        {
            if (!Data.unlockedMaps.Contains(mapId))
            {
                Data.unlockedMaps.Add(mapId);
                Save();
            }
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
