using UnityEngine;
using System.Collections.Generic;

namespace RobotTD.Core
{
    /// <summary>
    /// Wave configuration data structure
    /// </summary>
    [System.Serializable]
    public class WaveData
    {
        [Header("Wave Info")]
        public int waveNumber = 1;
        public string waveName = "Wave 1";
        [TextArea(1, 2)]
        public string description = "First wave of enemies";

        [Header("Timing")]
        public float preparationTime = 5f;      // Time before wave starts
        public float timeBetweenSpawns = 1f;    // Time between enemy spawns
        public bool isBossWave;

        [Header("Enemy Composition")]
        public List<WaveEnemySpawn> enemies = new List<WaveEnemySpawn>();

        [Header("Scaling")]
        public float healthMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float rewardMultiplier = 1f;

        [Header("Rewards")]
        public int waveCompletionBonus = 100;
        public int techPointsReward = 0;

        /// <summary>
        /// Get total enemy count for this wave
        /// </summary>
        public int TotalEnemyCount
        {
            get
            {
                int total = 0;
                foreach (var spawn in enemies)
                    total += spawn.count;
                return total;
            }
        }

        /// <summary>
        /// Get estimated wave duration in seconds
        /// </summary>
        public float EstimatedDuration => preparationTime + (TotalEnemyCount * timeBetweenSpawns);
    }

    /// <summary>
    /// Individual enemy spawn entry in a wave
    /// </summary>
    [System.Serializable]
    public class WaveEnemySpawn
    {
        public string enemyPrefabId;        // ID to look up in ObjectPooler or Resources
        public int count = 1;               // Number of this enemy type
        public float spawnDelay = 0f;       // Additional delay before first spawn
        public SpawnPattern pattern = SpawnPattern.Sequential;

        // For grouped spawns
        public int groupSize = 1;           // Spawn N enemies at once
        public float groupDelay = 0.5f;     // Delay between groups
    }

    /// <summary>
    /// How enemies should spawn
    /// </summary>
    public enum SpawnPattern
    {
        Sequential,     // One after another
        Grouped,        // In groups (burst spawning)
        Random,         // Random intervals
        Swarm          // All at once
    }

    /// <summary>
    /// ScriptableObject container for wave configurations.
    /// Allows designers to create wave sets in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveSet", menuName = "RobotTD/Wave Set")]
    public class WaveSetData : ScriptableObject
    {
        [Header("Wave Set Info")]
        public string setName = "Training Grounds";
        [TextArea(2, 4)]
        public string description = "Easy waves for beginners";
        public int difficulty = 1;

        [Header("Waves")]
        public List<WaveData> waves = new List<WaveData>();

        [Header("Global Scaling")]
        [Tooltip("Applied on top of per-wave scaling")]
        public float globalHealthMultiplier = 1f;
        public float globalSpeedMultiplier = 1f;
        public float globalRewardMultiplier = 1f;

        [Header("Boss Waves")]
        [Tooltip("Wave numbers that should spawn bosses (e.g., 5, 10, 15)")]
        public List<int> bossWaveNumbers = new List<int>();

        /// <summary>
        /// Get wave data for a specific wave number
        /// </summary>
        public WaveData GetWave(int waveNumber)
        {
            if (waveNumber <= 0 || waveNumber > waves.Count)
                return null;
            return waves[waveNumber - 1];
        }

        /// <summary>
        /// Check if a wave is a boss wave
        /// </summary>
        public bool IsBossWave(int waveNumber)
        {
            return bossWaveNumbers.Contains(waveNumber);
        }

        /// <summary>
        /// Get total number of waves in this set
        /// </summary>
        public int TotalWaves => waves.Count;
    }

    /// <summary>
    /// Wave generation utilities for procedural wave creation
    /// </summary>
    public static class WaveGenerator
    {
        /// <summary>
        /// Generate a wave configuration procedurally based on wave number
        /// </summary>
        public static WaveData GenerateProceduralWave(int waveNumber)
        {
            WaveData wave = new WaveData
            {
                waveNumber = waveNumber,
                waveName = $"Wave {waveNumber}",
                preparationTime = 5f,
                timeBetweenSpawns = Mathf.Max(0.3f, 1f - (waveNumber * 0.02f)),
                healthMultiplier = 1f + (waveNumber - 1) * 0.15f,
                speedMultiplier = 1f + (waveNumber - 1) * 0.02f,
                rewardMultiplier = 1f,
                waveCompletionBonus = 100 + (waveNumber * 25)
            };

            // Determine enemy composition based on wave number
            if (waveNumber <= 3)
            {
                // Early waves: scouts only
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Scout",
                    count = 5 + waveNumber * 2,
                    pattern = SpawnPattern.Sequential
                });
            }
            else if (waveNumber <= 6)
            {
                // Mix scouts and soldiers
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Scout",
                    count = 5 + waveNumber,
                    pattern = SpawnPattern.Sequential
                });
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Soldier",
                    count = 3 + waveNumber,
                    spawnDelay = 2f,
                    pattern = SpawnPattern.Sequential
                });
            }
            else if (waveNumber <= 10)
            {
                // Add tanks
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Scout",
                    count = 8,
                    pattern = SpawnPattern.Grouped,
                    groupSize = 2,
                    groupDelay = 0.5f
                });
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Soldier",
                    count = 6,
                    spawnDelay = 3f,
                    pattern = SpawnPattern.Sequential
                });
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Tank",
                    count = 2,
                    spawnDelay = 5f,
                    pattern = SpawnPattern.Sequential
                });
            }
            else
            {
                // Late game: mixed composition with elites
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Scout",
                    count = 10,
                    pattern = SpawnPattern.Grouped,
                    groupSize = 3,
                    groupDelay = 0.3f
                });
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Soldier",
                    count = 8,
                    spawnDelay = 2f,
                    pattern = SpawnPattern.Sequential
                });
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Tank",
                    count = 3,
                    spawnDelay = 4f,
                    pattern = SpawnPattern.Sequential
                });
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Elite",
                    count = 2,
                    spawnDelay = 6f,
                    pattern = SpawnPattern.Sequential
                });
            }

            // Boss waves every 5 waves
            if (waveNumber % 5 == 0)
            {
                wave.isBossWave = true;
                wave.enemies.Add(new WaveEnemySpawn
                {
                    enemyPrefabId = "Boss",
                    count = 1,
                    spawnDelay = 8f,
                    pattern = SpawnPattern.Sequential
                });
                wave.waveCompletionBonus *= 2;
            }

            return wave;
        }

        /// <summary>
        /// Generate an endless mode wave (difficulty increases continuously)
        /// </summary>
        public static WaveData GenerateEndlessWave(int waveNumber)
        {
            WaveData wave = GenerateProceduralWave(waveNumber);
            
            // Endless mode has steeper scaling
            wave.healthMultiplier *= 1.2f;
            wave.speedMultiplier = Mathf.Min(wave.speedMultiplier * 1.1f, 2f);
            wave.rewardMultiplier = 1f + (waveNumber * 0.05f);
            
            return wave;
        }
    }
}
