using UnityEngine;
using System;
using System.Collections.Generic;

namespace RobotTD.Core
{
    /// <summary>
    /// Challenge modifier types that alter gameplay mechanics.
    /// </summary>
    public enum ChallengeModifier
    {
        None = 0,
        
        // Enemy Modifiers
        SpeedRush,          // Enemies move 50% faster
        ArmoredAssault,     // Enemies have 100% more HP
        SwarmMode,          // 50% more enemies per wave
        BossRush,           // Boss appears every 5 waves instead of 10
        RegeneratingFoes,   // Enemies slowly regenerate health
        
        // Tower Modifiers
        LimitedArsenal,     // Only 3 random tower types available
        BudgetCrisis,       // Towers cost 50% more
        TowerLimit,         // Maximum 10 towers on map
        NoUpgrades,         // Towers cannot be upgraded
        WeakenedTowers,     // Towers deal 30% less damage
        SlowBuild,          // Tower placement has 3-second delay
        
        // Economy Modifiers
        EconomicHardship,   // 50% less credits per kill
        StartingDebt,       // Start with 50% normal credits
        ExpensiveUpgrades,  // Upgrades cost 100% more
        
        // Wave Modifiers
        FastForward,        // Waves auto-start with 3s delay
        NoBreaks,           // Zero time between waves
        RandomWaves,        // Enemy types randomized each wave
        
        // Special Modifiers
        FogOfWar,           // Reduced vision range
        TimeAttack,         // Must complete in 15 minutes
        PerfectDefense      // One life only
    }

    /// <summary>
    /// ScriptableObject defining a challenge configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "New Challenge", menuName = "Robot TD/Challenge Data", order = 5)]
    public class ChallengeData : ScriptableObject
    {
        [Header("Challenge Info")]
        [SerializeField] private string challengeId = "challenge_001";
        [SerializeField] private string challengeName = "Speed Rush";
        [SerializeField, TextArea(2, 4)] private string description = "Enemies move 50% faster. Quick reflexes required!";
        [SerializeField] private Sprite icon;
        
        [Header("Configuration")]
        [SerializeField] private string mapId = "map_factory"; // Which map to use
        [SerializeField] private ChallengeModifier[] modifiers;
        [SerializeField] private DifficultyTier difficulty = DifficultyTier.Medium;
        
        [Header("Rewards")]
        [SerializeField] private int creditReward = 500;
        [SerializeField] private int techPointReward = 10;
        [SerializeField] private string achievementId = ""; // Optional linked achievement
        
        [Header("Rotation")]
        [SerializeField] private ChallengeRotationType rotationType = ChallengeRotationType.Weekly;
        [SerializeField] private int rotationIndex = 0; // For scheduled rotation
        
        // Properties
        public string ChallengeId => challengeId;
        public string ChallengeName => challengeName;
        public string Description => description;
        public Sprite Icon => icon;
        public string MapId => mapId;
        public ChallengeModifier[] Modifiers => modifiers;
        public DifficultyTier Difficulty => difficulty;
        public int CreditReward => creditReward;
        public int TechPointReward => techPointReward;
        public string AchievementId => achievementId;
        public ChallengeRotationType RotationType => rotationType;
        public int RotationIndex => rotationIndex;
        
        /// <summary>
        /// Get combined difficulty multiplier for score calculation.
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            float baseMultiplier = difficulty switch
            {
                DifficultyTier.Easy => 1.2f,
                DifficultyTier.Medium => 1.5f,
                DifficultyTier.Hard => 2.0f,
                DifficultyTier.Extreme => 3.0f,
                _ => 1.0f
            };
            
            // Each modifier adds 10% to score multiplier
            return baseMultiplier + (modifiers.Length * 0.1f);
        }
        
        /// <summary>
        /// Check if this challenge has a specific modifier.
        /// </summary>
        public bool HasModifier(ChallengeModifier modifier)
        {
            return System.Array.Exists(modifiers, m => m == modifier);
        }
    }
    
    public enum DifficultyTier
    {
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Extreme = 4
    }
    
    public enum ChallengeRotationType
    {
        Daily,      // Changes every 24 hours
        Weekly,     // Changes every 7 days
        Permanent   // Always available
    }
    
    /// <summary>
    /// Runtime challenge state and completion tracking.
    /// </summary>
    [Serializable]
    public class ChallengeProgress
    {
        public string challengeId;
        public bool completed;
        public long bestScore;
        public int bestWave;
        public DateTime completionDate;
        public int attempts;
        
        public ChallengeProgress(string id)
        {
            challengeId = id;
            completed = false;
            bestScore = 0;
            bestWave = 0;
            attempts = 0;
        }
    }
}
