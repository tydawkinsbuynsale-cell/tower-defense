using UnityEngine;
using System;
using System.Collections.Generic;

namespace RobotTD.Progression
{
    // ── Tech Node definition ─────────────────────────────────────────────────

    public enum TechUpgrade
    {
        Firepower,      // +5% global damage per level
        Efficiency,     // -5% tower cost per level
        Resilience,     // +2 starting lives per level
        Tactics,        // +10% kill credit reward per level
        RapidDeploy,    // -10% tower placement cooldown per level
        Recycling,      // +10% sell-back value per level
    }

    [Serializable]
    public class TechNode
    {
        public TechUpgrade upgrade;
        public string displayName;
        [TextArea(1, 3)] public string description;
        public Sprite icon;
        public int maxLevel = 5;
        public int costPerLevel = 3;   // Tech points per level
        public TechUpgrade[] prerequisites; // Unlock requirements
    }

    // ── TechTree ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Manages the permanent upgrade tech tree.
    /// Stat bonuses are read from here by GameManager / Tower systems.
    /// </summary>
    public class TechTree : MonoBehaviour
    {
        public static TechTree Instance { get; private set; }

        [Header("Tech Nodes")]
        [SerializeField] private TechNode[] nodes;

        public event Action OnTreeChanged;

        private Core.TechTreeSaveData saveRef;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Core.SaveManager.Instance != null)
                saveRef = Core.SaveManager.Instance.Data.techTree;
        }

        // ── Accessors ────────────────────────────────────────────────────────

        public int GetLevel(TechUpgrade upgrade)
        {
            EnsureSaveRef();
            return upgrade switch
            {
                TechUpgrade.Firepower   => saveRef.firepower,
                TechUpgrade.Efficiency  => saveRef.efficiency,
                TechUpgrade.Resilience  => saveRef.resilience,
                TechUpgrade.Tactics     => saveRef.tactics,
                TechUpgrade.RapidDeploy => saveRef.rapidDeploy,
                TechUpgrade.Recycling   => saveRef.recycling,
                _ => 0
            };
        }

        private void SetLevel(TechUpgrade upgrade, int level)
        {
            EnsureSaveRef();
            switch (upgrade)
            {
                case TechUpgrade.Firepower:   saveRef.firepower   = level; break;
                case TechUpgrade.Efficiency:  saveRef.efficiency  = level; break;
                case TechUpgrade.Resilience:  saveRef.resilience  = level; break;
                case TechUpgrade.Tactics:     saveRef.tactics     = level; break;
                case TechUpgrade.RapidDeploy: saveRef.rapidDeploy = level; break;
                case TechUpgrade.Recycling:   saveRef.recycling   = level; break;
            }
        }

        // ── Stat Bonuses (used by game systems) ──────────────────────────────

        /// <summary>Global multiplier applied to all tower damage.</summary>
        public float DamageMultiplier    => 1f + GetLevel(TechUpgrade.Firepower) * 0.05f;

        /// <summary>Multiplier applied to tower purchase costs (< 1 = cheaper).</summary>
        public float CostMultiplier      => 1f - GetLevel(TechUpgrade.Efficiency) * 0.05f;

        /// <summary>Extra lives added to starting lives.</summary>
        public int BonusStartingLives    => GetLevel(TechUpgrade.Resilience) * 2;

        /// <summary>Multiplier applied to credit rewards from kills.</summary>
        public float KillRewardMultiplier => 1f + GetLevel(TechUpgrade.Tactics) * 0.10f;

        /// <summary>Multiplier applied to sell-back credit value.</summary>
        public float SellValueMultiplier => 1f + GetLevel(TechUpgrade.Recycling) * 0.10f;

        // ── Upgrade Logic ────────────────────────────────────────────────────

        public bool CanUpgrade(TechUpgrade upgrade)
        {
            TechNode node = GetNode(upgrade);
            if (node == null) return false;

            int currentLevel = GetLevel(upgrade);
            if (currentLevel >= node.maxLevel) return false;

            int techPointsAvailable = Core.SaveManager.Instance?.Data.techPoints ?? 0;
            if (techPointsAvailable < node.costPerLevel) return false;

            // Check prerequisites
            foreach (var prereq in node.prerequisites)
            {
                if (GetLevel(prereq) < 1) return false;
            }

            return true;
        }

        public bool TryUpgrade(TechUpgrade upgrade)
        {
            if (!CanUpgrade(upgrade)) return false;

            TechNode node = GetNode(upgrade);
            Core.SaveManager.Instance.Data.techPoints -= node.costPerLevel;
            SetLevel(upgrade, GetLevel(upgrade) + 1);

            Core.SaveManager.Instance.Save();
            OnTreeChanged?.Invoke();

            // Track achievement - count total upgrades across all branches
            int totalUpgrades = 0;
            foreach (TechUpgrade tech in Enum.GetValues(typeof(TechUpgrade)))
            {
                totalUpgrades += GetLevel(tech);
            }
            AchievementManager.Instance?.OnTechUpgrade(totalUpgrades);

            return true;
        }

        public TechNode GetNode(TechUpgrade upgrade)
        {
            foreach (var n in nodes)
                if (n.upgrade == upgrade) return n;
            return null;
        }

        /// <summary>
        /// Credit reward multiplier (alias for backward compatibility)
        /// </summary>
        public float CreditRewardMultiplier => KillRewardMultiplier;

        public int GetUpgradeCost(TechUpgrade upgrade)
        {
            return GetNode(upgrade)?.costPerLevel ?? 0;
        }

        private void EnsureSaveRef()
        {
            if (saveRef == null && Core.SaveManager.Instance != null)
                saveRef = Core.SaveManager.Instance.Data.techTree;

            saveRef ??= new Core.TechTreeSaveData();
        }
    }
}
