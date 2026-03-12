using UnityEngine;
using System;

namespace RobotTD.Core
{
    /// <summary>
    /// Types of daily missions that can be assigned.
    /// </summary>
    public enum MissionType
    {
        // Combat Missions
        KillEnemies,                // Kill X enemies total
        KillEnemiesWithTower,       // Kill X enemies using specific tower type
        KillBosses,                 // Kill X boss enemies
        DealDamage,                 // Deal X total damage
        
        // Tower Missions
        PlaceTowers,                // Place X towers in a match
        UpgradeTowers,              // Upgrade towers X times
        UseTowerType,               // Place X of specific tower type
        MaxUpgradeTower,            // Fully upgrade X towers
        
        // Economy Missions
        EarnCredits,                // Earn X credits in single match
        SpendCredits,               // Spend X credits in single match
        EndWithCredits,             // End match with X+ credits remaining
        
        // Wave Missions
        CompleteWaves,              // Complete X waves (any game)
        CompleteWavesFlawless,      // Complete X waves without losing life
        SurviveWave,                // Survive to wave X
        
        // Map Missions
        CompleteMap,                // Complete specific map
        CompleteAnyMap,             // Complete any map
        WinWithoutLosingLife,       // Complete any map with all lives intact
        
        // Special Missions
        CompleteChallenge,          // Complete any challenge
        UseOnlyTowerTypes,          // Win using only X tower types
        WinWithTowerLimit,          // Win with X or fewer towers placed
        SpeedRun                    // Complete map in under X minutes
    }
    
    /// <summary>
    /// Difficulty and reward tier for missions.
    /// </summary>
    public enum MissionDifficulty
    {
        Easy = 1,       // Quick to complete, lower rewards
        Medium = 2,     // Moderate effort, decent rewards
        Hard = 3        // Challenging, high rewards
    }
    
    /// <summary>
    /// Rotation type for missions.
    /// </summary>
    public enum MissionRotationType
    {
        Daily,          // Rotates every 24 hours
        Weekly          // Rotates every 7 days
    }
    
    /// <summary>
    /// ScriptableObject defining a daily mission configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "New Mission", menuName = "Robot TD/Mission Data", order = 6)]
    public class MissionData : ScriptableObject
    {
        [Header("Mission Info")]
        [SerializeField] private string missionId = "mission_001";
        [SerializeField] private string missionName = "Robot Slayer";
        [SerializeField, TextArea(2, 3)] private string description = "Defeat 50 enemies";
        [SerializeField] private Sprite icon;
        
        [Header("Configuration")]
        [SerializeField] private MissionType missionType = MissionType.KillEnemies;
        [SerializeField] private MissionDifficulty difficulty = MissionDifficulty.Medium;
        [SerializeField] private int targetValue = 50; // Progress goal
        [SerializeField] private string targetParameter = ""; // e.g., "LaserTurret", "map_factory"
        
        [Header("Rewards")]
        [SerializeField] private int creditReward = 200;
        [SerializeField] private int techPointReward = 5;
        
        [Header("Rotation")]
        [SerializeField] private MissionRotationType rotationType = MissionRotationType.Daily;
        [SerializeField] private int rotationWeight = 10; // Higher = more likely to appear
        [SerializeField] private int minimumPlayerLevel = 1; // Unlock requirement
        
        // Properties
        public string MissionId => missionId;
        public string MissionName => missionName;
        public string Description => description;
        public Sprite Icon => icon;
        public MissionType Type => missionType;
        public MissionDifficulty Difficulty => difficulty;
        public int TargetValue => targetValue;
        public string TargetParameter => targetParameter;
        public int CreditReward => creditReward;
        public int TechPointReward => techPointReward;
        public MissionRotationType RotationType => rotationType;
        public int RotationWeight => rotationWeight;
        public int MinimumPlayerLevel => minimumPlayerLevel;
        
        /// <summary>
        /// Get user-facing progress text.
        /// </summary>
        public string GetProgressText(int currentProgress)
        {
            return $"{Mathf.Min(currentProgress, targetValue)}/{targetValue}";
        }
        
        /// <summary>
        /// Check if mission is completed.
        /// </summary>
        public bool IsComplete(int currentProgress)
        {
            return currentProgress >= targetValue;
        }
        
        /// <summary>
        /// Get completion percentage.
        /// </summary>
        public float GetCompletionPercentage(int currentProgress)
        {
            return Mathf.Clamp01((float)currentProgress / targetValue);
        }
        
        /// <summary>
        /// Get formatted description with current values.
        /// </summary>
        public string GetFormattedDescription()
        {
            string desc = description;
            
            // Replace common placeholders
            desc = desc.Replace("{target}", targetValue.ToString());
            desc = desc.Replace("{parameter}", GetReadableParameter());
            
            return desc;
        }
        
        private string GetReadableParameter()
        {
            if (string.IsNullOrEmpty(targetParameter))
                return "";
            
            // Convert technical names to readable format
            // e.g., "LaserTurret" -> "Laser Turret"
            return System.Text.RegularExpressions.Regex.Replace(
                targetParameter, 
                "(\\B[A-Z])", 
                " $1"
            );
        }
    }
    
    /// <summary>
    /// Runtime mission state and progress tracking.
    /// </summary>
    [Serializable]
    public class MissionProgress
    {
        public string missionId;
        public int currentProgress;
        public bool completed;
        public bool rewardClaimed;
        public DateTime assignedDate;
        
        public MissionProgress(string id)
        {
            missionId = id;
            currentProgress = 0;
            completed = false;
            rewardClaimed = false;
            assignedDate = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Container for daily mission set.
    /// </summary>
    [Serializable]
    public class DailyMissionSet
    {
        public string[] missionIds = new string[3];
        public DateTime assignedDate;
        public int dayIndex; // Days since epoch for rotation tracking
        
        public DailyMissionSet()
        {
            missionIds = new string[3];
            assignedDate = DateTime.Now;
            dayIndex = GetCurrentDayIndex();
        }
        
        public static int GetCurrentDayIndex()
        {
            return (int)(DateTime.Now - new DateTime(2020, 1, 1)).TotalDays;
        }
    }
    
    /// <summary>
    /// Container for weekly mission set.
    /// </summary>
    [Serializable]
    public class WeeklyMissionSet
    {
        public string[] missionIds = new string[3];
        public DateTime assignedDate;
        public int weekIndex; // Weeks since epoch for rotation tracking
        
        public WeeklyMissionSet()
        {
            missionIds = new string[3];
            assignedDate = DateTime.Now;
            weekIndex = GetCurrentWeekIndex();
        }
        
        public static int GetCurrentWeekIndex()
        {
            return (int)(DateTime.Now - new DateTime(2020, 1, 1)).TotalDays / 7;
        }
    }
}
