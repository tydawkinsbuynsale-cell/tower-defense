using UnityEngine;

namespace RobotTD.Enemies
{
    /// <summary>
    /// ScriptableObject for enemy configuration.
    /// Easy to create and balance enemies in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "RobotTD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Info")]
        public string enemyName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public EnemyCategory category;

        [Header("Base Stats")]
        public float baseHealth = 100f;
        public float baseMoveSpeed = 2f;
        public int baseReward = 25;
        public int scoreValue = 10;
        public int liveDamage = 1; // Lives lost when reaching end

        [Header("Resistances (0-1)")]
        [Range(0f, 1f)] public float physicalResistance = 0f;
        [Range(0f, 1f)] public float energyResistance = 0f;
        [Range(0f, 1f)] public float fireResistance = 0f;
        [Range(0f, 1f)] public float electricResistance = 0f;
        [Range(0f, 1f)] public float plasmaResistance = 0f;

        [Header("Special Abilities")]
        public bool canFly = false;
        public bool canCloak = false;
        public bool hasShield = false;
        public float shieldHealth = 0f;
        public bool canHeal = false;
        public float healAmount = 0f;
        public float healCooldown = 5f;
        public bool canSplit = false; // Splits into smaller enemies on death
        public int splitCount = 2;
        public GameObject splitEnemyPrefab;

        [Header("Visuals")]
        public Color baseColor = Color.white;
        public Color damageFlashColor = Color.red;
        public float modelScale = 1f;
        public RuntimeAnimatorController animatorController;

        [Header("Audio")]
        public AudioClip spawnSound;
        public AudioClip hitSound;
        public AudioClip deathSound;
        public AudioClip abilitySound;
    }

    /// <summary>
    /// Enemy categories for filtering and wave composition
    /// </summary>
    public enum EnemyCategory
    {
        Scout,      // Fast, weak
        Soldier,    // Balanced
        Tank,       // Slow, high HP
        Elite,      // Fast, high HP
        Boss,       // Very powerful
        Flying,     // Can only be hit by certain towers
        Support     // Buffs other enemies
    }
}
