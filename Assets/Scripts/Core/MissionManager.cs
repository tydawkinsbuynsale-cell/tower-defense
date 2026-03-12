using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.Core
{
    /// <summary>
    /// Manages daily missions: rotation, progress tracking, rewards.
    /// Singleton that persists across scenes.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }
        
        [Header("Mission Library")]
        [SerializeField] private MissionData[] availableMissions;
        [SerializeField] private int missionsPerDay = 3;
        [SerializeField] private int missionsPerWeek = 3;
        
        [Header("Rotation Settings")]
        [SerializeField] private bool autoRotateDaily = true;
        [SerializeField] private bool autoRotateWeekly = true;
        [SerializeField] private int rotationHourUTC = 0; // Midnight UTC
        
        // Events
        public event Action<MissionData> OnMissionProgressUpdated;
        public event Action<MissionData> OnMissionCompleted;
        public event Action OnMissionsRotated;
        public event Action OnWeeklyMissionsRotated;
        
        // State
        private DailyMissionSet currentMissionSet;
        private WeeklyMissionSet currentWeeklyMissionSet;
        private Dictionary<string, MissionProgress> missionProgress = new Dictionary<string, MissionProgress>();
        private Dictionary<string, MissionData> missionDataCache = new Dictionary<string, MissionData>();
        
        // Properties
        public MissionData[] CurrentMissions => GetCurrentMissionData();
        public MissionData[] CurrentWeeklyMissions => GetCurrentWeeklyMissionData();
        public bool HasActiveMissions => currentMissionSet != null && currentMissionSet.missionIds.Length > 0;
        public bool HasActiveWeeklyMissions => currentWeeklyMissionSet != null && currentWeeklyMissionSet.missionIds.Length > 0;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeMissionCache();
            LoadProgress();
            
            if (autoRotateDaily)
            {
                CheckAndRotateMissions();
            }
            
            if (autoRotateWeekly)
            {
                CheckAndRotateWeeklyMissions();
            }
        }
        
        private void OnEnable()
        {
            SubscribeToGameEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromGameEvents();
        }
        
        // ── Initialization ────────────────────────────────────────────────────
        
        private void InitializeMissionCache()
        {
            missionDataCache.Clear();
            
            if (availableMissions == null || availableMissions.Length == 0)
            {
                Debug.LogWarning("[MissionManager] No missions configured in library!");
                return;
            }
            
            foreach (var mission in availableMissions)
            {
                if (mission != null && !string.IsNullOrEmpty(mission.MissionId))
                {
                    missionDataCache[mission.MissionId] = mission;
                }
            }
            
            Debug.Log($"[MissionManager] Initialized with {missionDataCache.Count} missions");
        }
        
        // ── Mission Rotation ──────────────────────────────────────────────────
        
        public void CheckAndRotateMissions()
        {
            int currentDayIndex = DailyMissionSet.GetCurrentDayIndex();
            
            // First time initialization or day has changed
            if (currentMissionSet == null || currentMissionSet.dayIndex != currentDayIndex)
            {
                RotateMissions();
            }
        }
        
        public void RotateMissions()
        {
            if (availableMissions == null || availableMissions.Length < missionsPerDay)
            {
                Debug.LogError("[MissionManager] Not enough missions in library for rotation!");
                return;
            }
            
            currentMissionSet = new DailyMissionSet();
            
            // Select missions using weighted random (daily missions only)
            List<MissionData> selectedMissions = SelectRandomMissions(missionsPerDay, MissionRotationType.Daily);
            
            for (int i = 0; i < selectedMissions.Count && i < missionsPerDay; i++)
            {
                currentMissionSet.missionIds[i] = selectedMissions[i].MissionId;
                
                // Create progress entry if doesn't exist
                if (!missionProgress.ContainsKey(selectedMissions[i].MissionId))
                {
                    missionProgress[selectedMissions[i].MissionId] = new MissionProgress(selectedMissions[i].MissionId);
                }
                else
                {
                    // Reset progress for new rotation
                    var progress = missionProgress[selectedMissions[i].MissionId];
                    progress.currentProgress = 0;
                    progress.completed = false;
                    progress.rewardClaimed = false;
                    progress.assignedDate = DateTime.Now;
                }
            }
            
            SaveProgress();
            OnMissionsRotated?.Invoke();
            
            Debug.Log($"[MissionManager] Rotated missions for day {currentMissionSet.dayIndex}");
            
            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(
                    Analytics.AnalyticsEvents.MISSIONS_ROTATED,
                    new Dictionary<string, object>
                    {
                        { "day_index", currentMissionSet.dayIndex },
                        { "mission_count", missionsPerDay }
                    }
                );
            }
        }
        
        public void CheckAndRotateWeeklyMissions()
        {
            int currentWeekIndex = WeeklyMissionSet.GetCurrentWeekIndex();
            
            // First time initialization or week has changed
            if (currentWeeklyMissionSet == null || currentWeeklyMissionSet.weekIndex != currentWeekIndex)
            {
                RotateWeeklyMissions();
            }
        }
        
        public void RotateWeeklyMissions()
        {
            if (availableMissions == null || availableMissions.Length < missionsPerWeek)
            {
                Debug.LogWarning("[MissionManager] Not enough weekly missions in library for rotation!");
                return;
            }
            
            currentWeeklyMissionSet = new WeeklyMissionSet();
            
            // Select missions using weighted random (weekly missions only)
            List<MissionData> selectedMissions = SelectRandomMissions(missionsPerWeek, MissionRotationType.Weekly);
            
            for (int i = 0; i < selectedMissions.Count && i < missionsPerWeek; i++)
            {
                currentWeeklyMissionSet.missionIds[i] = selectedMissions[i].MissionId;
                
                // Create progress entry if doesn't exist
                if (!missionProgress.ContainsKey(selectedMissions[i].MissionId))
                {
                    missionProgress[selectedMissions[i].MissionId] = new MissionProgress(selectedMissions[i].MissionId);
                }
                else
                {
                    // Reset progress for new rotation
                    var progress = missionProgress[selectedMissions[i].MissionId];
                    progress.currentProgress = 0;
                    progress.completed = false;
                    progress.rewardClaimed = false;
                    progress.assignedDate = DateTime.Now;
                }
            }
            
            SaveProgress();
            OnWeeklyMissionsRotated?.Invoke();
            
            Debug.Log($"[MissionManager] Rotated weekly missions for week {currentWeeklyMissionSet.weekIndex}");
            
            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(
                    "weekly_missions_rotated",
                    new Dictionary<string, object>
                    {
                        { "week_index", currentWeeklyMissionSet.weekIndex },
                        { "mission_count", missionsPerWeek }
                    }
                );
            }
        }
        
        private List<MissionData> SelectRandomMissions(int count, MissionRotationType rotationType)
        {
            // Get player level for filtering
            int playerLevel = SaveManager.Instance?.Data.playerLevel ?? 1;
            
            // Filter by level requirement and rotation type
            var eligibleMissions = availableMissions
                .Where(m => m != null && m.MinimumPlayerLevel <= playerLevel && m.RotationType == rotationType)
                .ToList();
            
            if (eligibleMissions.Count < count)
            {
                Debug.LogWarning($"[MissionManager] Not enough eligible {rotationType} missions ({eligibleMissions.Count} < {count})");
                return eligibleMissions;
            }
            
            // Weighted random selection
            List<MissionData> selected = new List<MissionData>();
            List<MissionData> pool = new List<MissionData>(eligibleMissions);
            
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int totalWeight = pool.Sum(m => m.RotationWeight);
                int randomWeight = UnityEngine.Random.Range(0, totalWeight);
                
                int cumulativeWeight = 0;
                MissionData chosenMission = pool[0];
                
                foreach (var mission in pool)
                {
                    cumulativeWeight += mission.RotationWeight;
                    if (randomWeight < cumulativeWeight)
                    {
                        chosenMission = mission;
                        break;
                    }
                }
                
                selected.Add(chosenMission);
                pool.Remove(chosenMission);
            }
            
            return selected;
        }
        
        // ── Progress Tracking ─────────────────────────────────────────────────
        
        public void UpdateMissionProgress(MissionType missionType, int amount = 1, string parameter = "")
        {
            bool progressUpdated = false;
            
            // Update daily missions
            if (currentMissionSet != null && currentMissionSet.missionIds != null)
            {
                foreach (string missionId in currentMissionSet.missionIds)
                {
                    if (UpdateSingleMissionProgress(missionId, missionType, amount, parameter))
                    {
                        progressUpdated = true;
                    }
                }
            }
            
            // Update weekly missions
            if (currentWeeklyMissionSet != null && currentWeeklyMissionSet.missionIds != null)
            {
                foreach (string missionId in currentWeeklyMissionSet.missionIds)
                {
                    if (UpdateSingleMissionProgress(missionId, missionType, amount, parameter))
                    {
                        progressUpdated = true;
                    }
                }
            }
            
            if (progressUpdated)
            {
                SaveProgress();
            }
        }
        
        private bool UpdateSingleMissionProgress(string missionId, MissionType missionType, int amount, string parameter)
        {
            if (string.IsNullOrEmpty(missionId)) return false;
            
            if (!missionDataCache.TryGetValue(missionId, out MissionData missionData))
                return false;
            
            if (!missionProgress.TryGetValue(missionId, out MissionProgress progress))
                return false;
            
            // Skip if already completed
            if (progress.completed) return false;
            
            // Check if this mission type matches
            if (missionData.Type != missionType) return false;
            
            // Check parameter match if specified
            if (!string.IsNullOrEmpty(missionData.TargetParameter) && 
                missionData.TargetParameter != parameter)
                return false;
            
            // Update progress
            progress.currentProgress += amount;
            
            OnMissionProgressUpdated?.Invoke(missionData);
            
            // Check completion
            if (missionData.IsComplete(progress.currentProgress) && !progress.completed)
            {
                CompleteMission(missionId);
            }
            
            return true;
        }
        
        private void CompleteMission(string missionId)
        {
            if (!missionProgress.TryGetValue(missionId, out MissionProgress progress))
                return;
            
            if (!missionDataCache.TryGetValue(missionId, out MissionData missionData))
                return;
            
            progress.completed = true;
            
            Debug.Log($"[MissionManager] Mission completed: {missionData.MissionName}");
            
            OnMissionCompleted?.Invoke(missionData);
            
            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(
                    Analytics.AnalyticsEvents.MISSION_COMPLETED,
                    new Dictionary<string, object>
                    {
                        { Analytics.AnalyticsParameters.MISSION_ID, missionId },
                        { Analytics.AnalyticsParameters.MISSION_TYPE, missionData.Type.ToString() },
                        { Analytics.AnalyticsParameters.MISSION_DIFFICULTY, missionData.Difficulty.ToString() },
                        { "credit_reward", missionData.CreditReward },
                        { "tech_point_reward", missionData.TechPointReward }
                    }
                );
            }
            
            SaveProgress();
        }
        
        public void ClaimReward(string missionId)
        {
            if (!missionProgress.TryGetValue(missionId, out MissionProgress progress))
            {
                Debug.LogWarning($"[MissionManager] No progress found for mission: {missionId}");
                return;
            }
            
            if (!progress.completed)
            {
                Debug.LogWarning($"[MissionManager] Mission not completed: {missionId}");
                return;
            }
            
            if (progress.rewardClaimed)
            {
                Debug.LogWarning($"[MissionManager] Reward already claimed: {missionId}");
                return;
            }
            
            if (!missionDataCache.TryGetValue(missionId, out MissionData missionData))
            {
                Debug.LogWarning($"[MissionManager] Mission data not found: {missionId}");
                return;
            }
            
            // Award rewards
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.AddCredits(missionData.CreditReward);
                SaveManager.Instance.AddTechPoints(missionData.TechPointReward);
            }
            
            progress.rewardClaimed = true;
            
            Debug.Log($"[MissionManager] Claimed rewards: {missionData.CreditReward} credits, {missionData.TechPointReward} tech points");
            
            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(
                    Analytics.AnalyticsEvents.MISSION_REWARD_CLAIMED,
                    new Dictionary<string, object>
                    {
                        { Analytics.AnalyticsParameters.MISSION_ID, missionId },
                        { "credits", missionData.CreditReward },
                        { "tech_points", missionData.TechPointReward }
                    }
                );
            }
            
            SaveProgress();
        }
        
        // ── Query Methods ─────────────────────────────────────────────────────
        
        public MissionProgress GetMissionProgress(string missionId)
        {
            missionProgress.TryGetValue(missionId, out MissionProgress progress);
            return progress;
        }
        
        public MissionData GetMissionData(string missionId)
        {
            missionDataCache.TryGetValue(missionId, out MissionData data);
            return data;
        }
        
        private MissionData[] GetCurrentMissionData()
        {
            if (currentMissionSet == null || currentMissionSet.missionIds == null)
                return new MissionData[0];
            
            List<MissionData> missions = new List<MissionData>();
            
            foreach (string id in currentMissionSet.missionIds)
            {
                if (!string.IsNullOrEmpty(id) && missionDataCache.TryGetValue(id, out MissionData data))
                {
                    missions.Add(data);
                }
            }
            
            return missions.ToArray();
        }
        
        private MissionData[] GetCurrentWeeklyMissionData()
        {
            if (currentWeeklyMissionSet == null || currentWeeklyMissionSet.missionIds == null)
                return new MissionData[0];
            
            List<MissionData> missions = new List<MissionData>();
            
            foreach (string id in currentWeeklyMissionSet.missionIds)
            {
                if (!string.IsNullOrEmpty(id) && missionDataCache.TryGetValue(id, out MissionData data))
                {
                    missions.Add(data);
                }
            }
            
            return missions.ToArray();
        }
        
        public int GetCompletedMissionsCount()
        {
            if (currentMissionSet == null) return 0;
            
            int count = 0;
            foreach (string id in currentMissionSet.missionIds)
            {
                if (missionProgress.TryGetValue(id, out MissionProgress progress) && progress.completed)
                {
                    count++;
                }
            }
            return count;
        }
        
        public TimeSpan GetTimeUntilRotation()
        {
            DateTime now = DateTime.Now;
            DateTime nextRotation = now.Date.AddDays(1).AddHours(rotationHourUTC);
            
            if (nextRotation < now)
            {
                nextRotation = nextRotation.AddDays(1);
            }
            
            return nextRotation - now;
        }
        
        public TimeSpan GetTimeUntilWeeklyRotation()
        {
            if (currentWeeklyMissionSet == null)
            {
                return TimeSpan.Zero;
            }
            
            DateTime assignedDate = currentWeeklyMissionSet.assignedDate;
            DateTime nextRotation = assignedDate.AddDays(7);
            DateTime now = DateTime.Now;
            
            if (nextRotation < now)
            {
                // Handle case where we've passed the rotation time
                return TimeSpan.Zero;
            }
            
            return nextRotation - now;
        }
        
        // ── Game Event Integration ────────────────────────────────────────────
        
        private void SubscribeToGameEvents()
        {
            // Subscribe to game events for automatic progress tracking
            if (GameManager.Instance != null)
            {
                GameManager.OnGameStarted += HandleGameStarted;
                GameManager.OnVictory += HandleVictory;
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnEnemyKilled += HandleEnemyKilled;
                WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
            }
        }
        
        private void UnsubscribeFromGameEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.OnGameStarted -= HandleGameStarted;
                GameManager.OnVictory -= HandleVictory;
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnEnemyKilled -= HandleEnemyKilled;
                WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
            }
        }
        
        private void HandleGameStarted()
        {
            // Track game start for missions that need it
        }
        
        private void HandleVictory()
        {
            UpdateMissionProgress(MissionType.CompleteAnyMap);
            
            // Check specific map completion
            if (Map.MapSelector.Instance != null)
            {
                var map = Map.MapSelector.Instance.GetSelectedMap();
                if (map != null)
                {
                    UpdateMissionProgress(MissionType.CompleteMap, 1, map.mapId);
                }
            }
            
            // Check flawless victory
            if (GameManager.Instance != null && GameManager.Instance.Lives == GameManager.Instance.MaxLives)
            {
                UpdateMissionProgress(MissionType.WinWithoutLosingLife);
            }
        }
        
        private void HandleEnemyKilled(Enemies.Enemy enemy)
        {
            UpdateMissionProgress(MissionType.KillEnemies, 1);
            
            // Check if boss
            if (enemy.IsBoss)
            {
                UpdateMissionProgress(MissionType.KillBosses, 1);
            }
        }
        
        private void HandleWaveCompleted(int waveNumber)
        {
            UpdateMissionProgress(MissionType.CompleteWaves, 1);
            
            // Check flawless wave completion
            if (GameManager.Instance != null && GameManager.Instance.Lives == GameManager.Instance.MaxLives)
            {
                UpdateMissionProgress(MissionType.CompleteWavesFlawless, 1);
            }
            
            // Check survive to wave X
            UpdateMissionProgress(MissionType.SurviveWave, waveNumber);
        }
        
        // ── Persistence ───────────────────────────────────────────────────────
        
        private void LoadProgress()
        {
            string json = PlayerPrefs.GetString("DailyMissions", "{}");
            
            try
            {
                var wrapper = JsonUtility.FromJson<MissionSaveData>(json);
                if (wrapper != null)
                {
                    // Load daily mission set
                    if (wrapper.currentMissionSet != null)
                    {
                        currentMissionSet = wrapper.currentMissionSet;
                    }
                    
                    // Load weekly mission set
                    if (wrapper.currentWeeklyMissionSet != null)
                    {
                        currentWeeklyMissionSet = wrapper.currentWeeklyMissionSet;
                    }
                    
                    // Load progress
                    if (wrapper.missionProgress != null)
                    {
                        missionProgress.Clear();
                        foreach (var progress in wrapper.missionProgress)
                        {
                            missionProgress[progress.missionId] = progress;
                        }
                    }
                    
                    Debug.Log($"[MissionManager] Loaded progress for {missionProgress.Count} missions");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MissionManager] Failed to load progress: {e.Message}");
            }
        }
        
        private void SaveProgress()
        {
            var wrapper = new MissionSaveData
            {
                currentMissionSet = currentMissionSet,
                currentWeeklyMissionSet = currentWeeklyMissionSet,
                missionProgress = missionProgress.Values.ToArray()
            };
            
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString("DailyMissions", json);
            PlayerPrefs.Save();
        }
        
        // ── Debug Helpers ─────────────────────────────────────────────────────
        
#if UNITY_EDITOR
        [ContextMenu("Force Rotate Missions")]
        private void DEBUG_ForceRotate()
        {
            RotateMissions();
            Debug.Log("[MissionManager] Forced mission rotation");
        }
        
        [ContextMenu("Force Rotate Weekly Missions")]
        private void DEBUG_ForceRotateWeekly()
        {
            RotateWeeklyMissions();
            Debug.Log("[MissionManager] Forced weekly mission rotation");
        }
        
        [ContextMenu("Complete All Current Missions")]
        private void DEBUG_CompleteAll()
        {
            if (currentMissionSet != null)
            {
                foreach (string id in currentMissionSet.missionIds)
                {
                    if (missionProgress.TryGetValue(id, out MissionProgress progress) &&
                        missionDataCache.TryGetValue(id, out MissionData data))
                    {
                        progress.currentProgress = data.TargetValue;
                        CompleteMission(id);
                    }
                }
            }
            
            if (currentWeeklyMissionSet != null)
            {
                foreach (string id in currentWeeklyMissionSet.missionIds)
                {
                    if (missionProgress.TryGetValue(id, out MissionProgress progress) &&
                        missionDataCache.TryGetValue(id, out MissionData data))
                    {
                        progress.currentProgress = data.TargetValue;
                        CompleteMission(id);
                    }
                }
            }
        }
        
        [ContextMenu("Reset All Progress")]
        private void DEBUG_ResetProgress()
        {
            missionProgress.Clear();
            currentMissionSet = null;
            currentWeeklyMissionSet = null;
            SaveProgress();
            Debug.Log("[MissionManager] Reset all mission progress");
        }
#endif
    }
    
    // ── Save Data Wrapper ─────────────────────────────────────────────────────
    
    [Serializable]
    public class MissionSaveData
    {
        public DailyMissionSet currentMissionSet;
        public WeeklyMissionSet currentWeeklyMissionSet;
        public MissionProgress[] missionProgress;
    }
}
