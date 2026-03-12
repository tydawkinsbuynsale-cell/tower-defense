using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.Progression
{
    public enum AchievementId
    {
        // Kill milestones
        KillsFirst10,
        KillsFirst100,
        Kills500,
        Kills2500,
        Kills10000,
        Kills50000,

        // Wave milestones
        CompleteWave5,
        CompleteWave10,
        CompleteWave20,
        CompleteWave30,
        SurviveAllWaves,

        // Level milestones
        ReachLevel3,
        ReachLevel5,
        ReachLevel10,
        ReachLevel25,
        ReachLevel50,

        // Build milestones
        PlaceFirstTower,
        Place10Towers,
        Place50Towers,
        Place100Towers,
        Use5DifferentTowers,
        UseAllTowers,
        UpgradeFirstTower,
        Upgrade10Towers,
        Upgrade50Towers,
        MaxUpgradeTower,

        // Challenge
        NoDamageTaken,          // Complete a wave without losing life
        PerfectDefense,         // Complete a map with 100% lives
        SpeedRunner,            // Complete a map within time limit
        NoTowerSell,            // Win without selling any tower
        MinimalistVictory,      // Win with 5 or fewer towers
        BossKiller,             // Kill a Boss enemy
        BossSlayer,             // Kill 10 Boss enemies

        // Progression
        TechTreeFirstUpgrade,
        TechTreeFiveUpgrades,
        CompleteTechBranch,
        CompleteAllTech,

        // Maps
        CompleteFirstMap,
        Complete5Maps,
        CompleteAllMaps,
        ThreeStarRating,
        ThreeStarAllMaps,

        // Economy
        SaverFirst1000,
        Saver10000,
        SpenderFirst5000,
        Spender50000,

        // Special
        FirstVictory,
        TenVictories,
        HundredVictories,
        PlayFor1Hour,
        PlayFor10Hours,
        Dedicated,              // Play for 50 hours
    }

    [Serializable]
    public class AchievementDef
    {
        public AchievementId id;
        public string title;
        [TextArea(1, 2)] public string description;
        public Sprite icon;
        public int xpReward = 50;
        public int techPointReward = 0;
        public AchievementCategory category = AchievementCategory.General;
        public bool isHidden = false; // Hidden until unlocked
    }

    public enum AchievementCategory
    {
        General,
        Combat,
        Building,
        Progression,
        Challenge,
        Special
    }

    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [SerializeField] private AchievementDef[] definitions;
        [SerializeField] private UI.AchievementPopup achievementPopup; // New popup system

        public event Action<AchievementDef> OnAchievementUnlocked;

        private HashSet<string> unlocked = new HashSet<string>();
        private Dictionary<AchievementId, AchievementDef> defMap
            = new Dictionary<AchievementId, AchievementDef>();

        // Session tracking (not persisted — recalc per session)
        private HashSet<Towers.TowerType> towersUsedThisSession = new HashSet<Towers.TowerType>();
        private bool soldTowerThisGame;
        private bool lostLifeThisWave;
        private int towerCountThisGame = 0;
        private int bossKillCount = 0;
        private int mapsCompleted = 0;
        private int threeStarMaps = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var def in definitions)
                defMap[def.id] = def;
        }

        private void Start()
        {
            // Load persisted achievements
            var saved = Core.SaveManager.Instance?.Data.completedAchievements;
            if (saved != null)
                foreach (var s in saved)
                    unlocked.Add(s);

            // Initialize session data
            ResetSessionTracking();

            // Use new popup system if available, fallback to legacy toast
            if (achievementPopup == null)
            {
                achievementPopup = UI.AchievementPopup.Instance;
            }
        }

        /// <summary>
        /// Reset session-specific tracking (call at start of new game)
        /// </summary>
        public void ResetSessionTracking()
        {
            towersUsedThisSession.Clear();
            soldTowerThisGame = false;
            lostLifeThisWave = false;
            towerCountThisGame = 0;
        }

        // ── Public check methods ─────────────────────────────────────────────

        public void CheckKillCount(int totalKills)
        {
            if (totalKills >= 10)    TryUnlock(AchievementId.KillsFirst10);
            if (totalKills >= 100)   TryUnlock(AchievementId.KillsFirst100);
            if (totalKills >= 500)   TryUnlock(AchievementId.Kills500);
            if (totalKills >= 2500)  TryUnlock(AchievementId.Kills2500);
            if (totalKills >= 10000) TryUnlock(AchievementId.Kills10000);
            if (totalKills >= 50000) TryUnlock(AchievementId.Kills50000);
        }

        public void CheckLevelUp(int level)
        {
            if (level >= 3)  TryUnlock(AchievementId.ReachLevel3);
            if (level >= 5)  TryUnlock(AchievementId.ReachLevel5);
            if (level >= 10) TryUnlock(AchievementId.ReachLevel10);
            if (level >= 25) TryUnlock(AchievementId.ReachLevel25);
            if (level >= 50) TryUnlock(AchievementId.ReachLevel50);
        }

        public void CheckWaveComplete(int wave, bool livesIntact)
        {
            if (wave >= 5)  TryUnlock(AchievementId.CompleteWave5);
            if (wave >= 10) TryUnlock(AchievementId.CompleteWave10);
            if (wave >= 20) TryUnlock(AchievementId.CompleteWave20);
            if (wave >= 30) TryUnlock(AchievementId.CompleteWave30);

            if (livesIntact && !lostLifeThisWave)
                TryUnlock(AchievementId.NoDamageTaken);

            lostLifeThisWave = false; // Reset for next wave
        }

        public void CheckVictory(float clearTimeSeconds, int stars, int livesRemaining, int startingLives)
        {
            TryUnlock(AchievementId.FirstVictory);
            TryUnlock(AchievementId.SurviveAllWaves);

            // Check victory count
            if (Core.SaveManager.Instance != null)
            {
                int victories = Core.SaveManager.Instance.Data.totalVictories;
                if (victories >= 10) TryUnlock(AchievementId.TenVictories);
                if (victories >= 100) TryUnlock(AchievementId.HundredVictories);
            }

            // Challenge achievements
            if (!soldTowerThisGame)
                TryUnlock(AchievementId.NoTowerSell);

            if (clearTimeSeconds < 600f)  // 10 min speed run threshold
                TryUnlock(AchievementId.SpeedRunner);

            if (towerCountThisGame <= 5)
                TryUnlock(AchievementId.MinimalistVictory);

            if (livesRemaining >= startingLives)
                TryUnlock(AchievementId.PerfectDefense);

            // Star achievements
            if (stars >= 3)
            {
                TryUnlock(AchievementId.ThreeStarRating);
                threeStarMaps++;
            }

            // Map completion
            mapsCompleted++;
            TryUnlock(AchievementId.CompleteFirstMap);
            if (mapsCompleted >= 5) TryUnlock(AchievementId.Complete5Maps);
        }

        public void OnTowerPlaced(Towers.TowerType type)
        {
            TryUnlock(AchievementId.PlaceFirstTower);
            towerCountThisGame++;
            towersUsedThisSession.Add(type);

            // Check total towers placed
            if (Core.SaveManager.Instance != null)
            {
                int totalPlaced = Core.SaveManager.Instance.Data.totalTowersPlaced;
                if (totalPlaced >= 10) TryUnlock(AchievementId.Place10Towers);
                if (totalPlaced >= 50) TryUnlock(AchievementId.Place50Towers);
                if (totalPlaced >= 100) TryUnlock(AchievementId.Place100Towers);
            }

            // Check tower variety
            if (towersUsedThisSession.Count >= 5)
                TryUnlock(AchievementId.Use5DifferentTowers);

            int totalTowerTypes = Enum.GetValues(typeof(Towers.TowerType)).Length;
            if (towersUsedThisSession.Count >= totalTowerTypes)
                TryUnlock(AchievementId.UseAllTowers);
        }

        public void OnTowerUpgraded(int upgradeLevel)
        {
            TryUnlock(AchievementId.UpgradeFirstTower);
            
            // Check total upgrades
            if (Core.SaveManager.Instance != null)
            {
                int totalUpgrades = Core.SaveManager.Instance.Data.totalTowersUpgraded;
                if (totalUpgrades >= 10) TryUnlock(AchievementId.Upgrade10Towers);
                if (totalUpgrades >= 50) TryUnlock(AchievementId.Upgrade50Towers);
            }

            if (upgradeLevel >= 3)
                TryUnlock(AchievementId.MaxUpgradeTower);
        }

        public void OnTowerSold()
        {
            soldTowerThisGame = true;
        }

        public void OnLifeLost()
        {
            lostLifeThisWave = true;
        }

        public void OnBossKilled()
        {
            bossKillCount++;
            TryUnlock(AchievementId.BossKiller);
            if (bossKillCount >= 10)
                TryUnlock(AchievementId.BossSlayer);
        }

        public void OnTechUpgrade(int totalUpgrades)
        {
            TryUnlock(AchievementId.TechTreeFirstUpgrade);
            if (totalUpgrades >= 5)
                TryUnlock(AchievementId.TechTreeFiveUpgrades);
        }

        public void CheckCreditsEarned(int totalCredits)
        {
            if (totalCredits >= 1000) TryUnlock(AchievementId.SaverFirst1000);
            if (totalCredits >= 10000) TryUnlock(AchievementId.Saver10000);
        }

        public void CheckCreditsSpent(int totalSpent)
        {
            if (totalSpent >= 5000) TryUnlock(AchievementId.SpenderFirst5000);
            if (totalSpent >= 50000) TryUnlock(AchievementId.Spender50000);
        }

        public void CheckPlayTime(float totalHours)
        {
            if (totalHours >= 1f) TryUnlock(AchievementId.PlayFor1Hour);
            if (totalHours >= 10f) TryUnlock(AchievementId.PlayFor10Hours);
            if (totalHours >= 50f) TryUnlock(AchievementId.Dedicated);
        }

        // ── Core unlock ──────────────────────────────────────────────────────

        private void TryUnlock(AchievementId id)
        {
            string key = id.ToString();
            if (unlocked.Contains(key)) return;

            unlocked.Add(key);

            if (Core.SaveManager.Instance != null &&
                !Core.SaveManager.Instance.Data.completedAchievements.Contains(key))
            {
                Core.SaveManager.Instance.Data.completedAchievements.Add(key);
                Core.SaveManager.Instance.Save();
            }

            if (defMap.TryGetValue(id, out AchievementDef def))
            {
                // Grant rewards
                Core.SaveManager.Instance?.AddXP(def.xpReward);
                if (def.techPointReward > 0 && Core.SaveManager.Instance != null)
                    Core.SaveManager.Instance.Data.techPoints += def.techPointReward;

                // Show popup notification
                if (achievementPopup != null)
                {
                    achievementPopup.ShowAchievement(def);
                }

                // Fire event
                OnAchievementUnlocked?.Invoke(def);

                Debug.Log($"[Achievement] Unlocked: {def.title} (+{def.xpReward} XP" + 
                         (def.techPointReward > 0 ? $", +{def.techPointReward} Tech Points)" : ")"));
            }
        }

        public bool IsUnlocked(AchievementId id) => unlocked.Contains(id.ToString());

        public List<AchievementDef> GetAll() => new List<AchievementDef>(definitions);

        public List<AchievementDef> GetUnlocked()
        {
            return definitions.Where(d => IsUnlocked(d.id)).ToList();
        }

        public List<AchievementDef> GetLocked()
        {
            return definitions.Where(d => !IsUnlocked(d.id)).ToList();
        }

        public int GetUnlockedCount() => unlocked.Count;

        public int GetTotalCount() => definitions.Length;

        public float GetCompletionPercent()
        {
            if (definitions.Length == 0) return 0f;
            return (float)unlocked.Count / definitions.Length * 100f;
        }

        public AchievementDef GetDefinition(AchievementId id)
        {
            return defMap.TryGetValue(id, out var def) ? def : null;
        }

        /// <summary>Called by EndlessMode at milestone wave intervals.</summary>
        public void CheckEndlessMilestone(int endlessWave)
        {
            if (endlessWave >= 5)  CheckWaveComplete(5, false);
            if (endlessWave >= 10) CheckWaveComplete(10, false);
            if (endlessWave >= 20) CheckWaveComplete(20, false);
            if (endlessWave >= 30) CheckWaveComplete(30, false);
            
            Debug.Log($"[Achievement] Endless milestone wave {endlessWave} checked.");
        }
    }
}
