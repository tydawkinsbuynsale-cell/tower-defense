using UnityEngine;

namespace RobotTD.Towers
{
    /// <summary>
    /// ScriptableObject for tower configuration.
    /// Makes it easy to create and balance towers in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTower", menuName = "RobotTD/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("Info")]
        public string towerName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public TowerType towerType;

        [Header("Base Stats")]
        public int cost = 100;
        public float baseDamage = 10f;
        public float baseRange = 5f;
        public float baseFireRate = 1f; // Attacks per second
        public float rotationSpeed = 10f;

        [Header("Upgrades")]
        public int maxLevel = 3;
        public int[] upgradeCosts = { 75, 150, 300 };
        [Range(0f, 1f)] public float damageUpgradePercent = 0.25f;  // 25% more per level
        [Range(0f, 1f)] public float rangeUpgradePercent = 0.1f;    // 10% more per level
        [Range(0f, 1f)] public float fireRateUpgradePercent = 0.15f; // 15% more per level

        [Header("Targeting")]
        public TargetPriority targetPriority = TargetPriority.First;
        public bool canTargetFlying = true;
        public bool canTargetGround = true;

        [Header("Special Properties")]
        public float slowPercent = 0f;       // For freeze towers
        public float slowDuration = 0f;
        public float splashRadius = 0f;      // For splash damage
        public float splashDamagePercent = 0.5f;
        public int chainCount = 0;           // For chain lightning
        public float chainRange = 0f;
        public float dotDamage = 0f;         // Damage over time
        public float dotDuration = 0f;

        [Header("Visuals")]
        public GameObject projectilePrefab;
        public GameObject muzzleFlashPrefab;
        public Color towerColor = Color.white;
        public Color projectileColor = Color.white;

        [Header("Audio")]
        public AudioClip fireSound;
        public AudioClip upgradeSound;
        public AudioClip placeSound;

        /// <summary>
        /// Get upgrade cost for a specific level
        /// </summary>
        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= maxLevel) return 0;
            int index = currentLevel - 1;
            if (index >= 0 && index < upgradeCosts.Length)
            {
                return upgradeCosts[index];
            }
            return upgradeCosts[upgradeCosts.Length - 1];
        }

        /// <summary>
        /// Get total value of tower at current level (for selling)
        /// </summary>
        public int GetTotalValue(int level)
        {
            int total = cost;
            for (int i = 0; i < level - 1 && i < upgradeCosts.Length; i++)
            {
                total += upgradeCosts[i];
            }
            return total;
        }
    }

    /// <summary>
    /// Types of towers in the game
    /// </summary>
    public enum TowerType
    {
        // Standard Damage
        LaserTurret,     // Fast firing, low damage
        PlasmaCannon,    // Slow firing, high damage
        
        // Area Effect
        RocketLauncher,  // Splash damage
        ShockTower,      // Chain lightning
        
        // Utility
        FreezeTurret,    // Slows enemies
        EMPTower,        // Stuns/disables
        
        // Support
        BuffStation,     // Boosts nearby towers
        RepairDrone,     // Reduces enemy armor
        
        // Special
        SniperBot,       // Extreme range, high damage
        Flamethrower,    // Damage over time
        TeslaCoil,       // Auto-targets multiple enemies
        MissileBattery   // Multiple rockets
    }
}
